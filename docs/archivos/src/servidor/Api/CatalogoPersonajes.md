# CatalogoPersonajes.cs

Fuente: [src/servidor/Api/CatalogoPersonajes.cs](../../../../../src/servidor/Api/CatalogoPersonajes.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión: [catálogo común](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Administración y copia del catálogo.

## Estado, miembros y ejecución

DarDeAlta valida antes de escribir, asigna máximo ID positivo más uno y rechaza agotamiento. CantidadCatalogo cuenta todas las entradas. ReemplazarCatalogo rechaza lista vacía, valida todos los personajes y nombres duplicados antes de borrar; asigna IDs desde uno y reemplaza en una transacción. Desactivar excluye futuras copias sin borrar el personaje. CopiarActivos consulta activos dentro del contexto transaccional suministrado al inicio. CartaCatalogo crea CartaPartida con todos los datos y atributos.

## Errores, límites y efectos

El reemplazo no cambia las copias de partidas iniciadas. Comparación de nombres ignora mayúsculas y recorta extremos; no se introduce otra política de concurrencia. Validación y diagnóstico públicos conservan ErrorApi.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

internal sealed class CatalogoPersonajes(SqliteJuego infraestructura)
{
    internal int DarDeAlta(AltaPersonaje datos)
    {
        var personaje = datos.Validar();
        return infraestructura.Ejecutar(true, db =>
        {
            int mayor = db.Catalogo.Where(p => p.Id > 0).Select(p => (int?)p.Id).Max() ?? 0;
            if (mayor == int.MaxValue)
                throw new ErrorApi(409, "operacionIncompatible", "Se agotaron los IDs de alta automática.");
            personaje.Id = mayor + 1;
            db.Catalogo.Add(personaje);
            return personaje.Id;
        });
    }

    internal int CantidadCatalogo() => infraestructura.Ejecutar(false, db => db.Catalogo.Count());

    internal int ReemplazarCatalogo(AltaPersonaje[] datos)
    {
        if (datos.Length == 0) throw LotePersonajes.Invalido("La lista no puede estar vacía.");
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var personajes = datos.Select((dato, indice) =>
        {
            try
            {
                var personaje = dato.Validar();
                if (!nombres.Add(personaje.Nombre.Trim())) throw LotePersonajes.Invalido("nombre duplicado.");
                personaje.Id = indice + 1;
                return personaje;
            }
            catch (ErrorApi error)
            {
                throw LotePersonajes.Invalido($"Personaje {indice + 1} ({dato.Nombre}): {error.Message}");
            }
        }).ToArray();
        return infraestructura.Ejecutar(true, db =>
        {
            db.Catalogo.ExecuteDelete();
            db.Catalogo.AddRange(personajes);
            return personajes.Length;
        });
    }

    internal void Desactivar(int id) => infraestructura.Ejecutar(true, db =>
    {
        var personaje = db.Catalogo.Find(id) ?? throw ErrorApi.NoEncontrado();
        personaje.Activo = false;
        return true;
    });

    internal CartaPartida[] CopiarActivos(BaseJuego db) =>
        db.Catalogo.Where(p => p.Activo).ToArray().Select(CartaCatalogo).ToArray();

    private static CartaPartida CartaCatalogo(PersonajeCatalogo c) => new()
    {
        IdPersonaje = c.Id,
        Nombre = c.Nombre,
        Descripcion = c.Descripcion,
        Ascii = c.Ascii,
        Fuerza = c.Fuerza,
        Velocidad = c.Velocidad,
        Resistencia = c.Resistencia,
        Inteligencia = c.Inteligencia,
        Energia = c.Energia,
        Habilidad = c.Habilidad,
    };
}
```

## Relaciones y verificación

Véanse [arquitectura](../../../../arquitectura.md), [modelos](../../../../modelos.md) y [auditoría del spec 006](../../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
