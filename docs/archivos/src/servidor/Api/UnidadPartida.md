# UnidadPartida.cs

Fuente: [src/servidor/Api/UnidadPartida.cs](../../../../../src/servidor/Api/UnidadPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión: [catálogo común](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Estado y operaciones de una transacción cargada.

## Estado, miembros y ejecución

El constructor recibe Partida, participantes persistidos, operaciones previas y acceso transaccional al catálogo. Partida expone el dominio; NuevasOperaciones acumula exclusivamente los recibos que guardará AlmacenPartidas. Autenticar delega comparación de hash. CopiarCatalogoActivo invoca CatalogoPersonajes dentro de la misma transacción. Recuperar busca por rol/ID, compara Eleccion por valor y devuelve Confirmacion repetida o conflicto. Confirmar registra operación y revisión confirmada; ConsultarRecibo devuelve el recibo del rol o 404.

## Errores, límites y efectos

No expone DbContext ni IQueryable al coordinador. Confirmar no hace commit: solo AlmacenPartidas y SqliteJuego confirman estado y operaciones juntos. No conserva una segunda copia global del estado.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
namespace Proyecto_Integrador_Final.Api;

// Unidad cargada bajo una única transacción. No expone DbContext ni consultas diferidas.
internal sealed class UnidadPartida(Partida partida, ParticipanteGuardado[] participantes,
    List<OperacionGuardada> operaciones, Func<CatalogoPersonajes, CartaPartida[]> catalogo)
{
    internal Partida Partida { get; } = partida;
    internal List<OperacionGuardada> NuevasOperaciones { get; } = [];
    internal string Autenticar(string? credencial) => AlmacenPartidas.Autenticar(participantes, credencial).Rol;
    internal CartaPartida[] CopiarCatalogoActivo(CatalogoPersonajes origen) => catalogo(origen);

    internal Confirmacion? Recuperar(string rol, Eleccion datos)
    {
        var anterior = operaciones.Find(o => o.Rol == rol && o.IdOperacion == datos.IdOperacion);
        if (anterior is null) return null;
        if (anterior.Datos() != datos)
            throw new ErrorApi(409, "operacionIncompatible", "La operación ya tiene otros datos.");
        return new Confirmacion(anterior.Recibo(), true);
    }

    internal Confirmacion Confirmar(string rol, Eleccion datos)
    {
        var operacion = new OperacionGuardada
        {
            IdPartida = Partida.Estado.Id, Rol = rol, IdOperacion = datos.IdOperacion,
            IdSolicitud = datos.IdSolicitud, RevisionEsperada = datos.RevisionEsperada!.Value,
            IdPersonaje = datos.IdPersonaje!.Value, Atributo = datos.Atributo,
            RevisionConfirmada = Partida.Estado.Revision
        };
        NuevasOperaciones.Add(operacion);
        return new Confirmacion(operacion.Recibo(), false);
    }

    internal Recibo ConsultarRecibo(string rol, Guid idOperacion) =>
        operaciones.Find(o => o.Rol == rol && o.IdOperacion == idOperacion)?.Recibo()
        ?? throw ErrorApi.NoEncontrado();
}
```

## Relaciones y verificación

Véanse [arquitectura](../../../../arquitectura.md), [modelos](../../../../modelos.md) y [auditoría del spec 006](../../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
