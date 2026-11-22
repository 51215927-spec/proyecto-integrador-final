# AdministradorSesiones.cs

Fuente: [src/servidor/Api/AdministradorSesiones.cs](../../../../../src/servidor/Api/AdministradorSesiones.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión: [catálogo común](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Presencia e identidad temporal por plaza.

## Estado, miembros y ejecución

Incorporar valida ID/nombre, autentica, comprueba exclusión y nombre registrado y confirma la incorporación antes de publicar presencia. Renovar exige la misma sesión vigente y extiende quince segundos. CerrarSesion verifica la plaza y retira presencia. ReemplazarCredencial confirma el hash nuevo antes de retirar presencia; un fallo de escritura conserva la sesión. AmbosPresentes consulta las dos plazas; Vigente compara el vencimiento con TimeProvider; ExigirSesion exige identidad y vigencia. Sincronizacion es el bloqueo compartido de SQLite, también tomado por el coordinador. El diccionario sesiones no se persiste.

## Errores, límites y efectos

Una sesión distinta recibe sesionOcupada; al vencer recibe sesionVencida. La renovación y las consultas de presencia no incrementan revisión. El reloj es sustituible; no se añaden garantías entre servidores independientes.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
namespace Proyecto_Integrador_Final.Api;

internal sealed record Incorporacion(Guid IdSesion, string? Nombre);
internal sealed record Sesion(string Rol, Guid IdSesion, DateTimeOffset VenceUtc);

internal sealed class AdministradorSesiones(AlmacenPartidas almacen, TimeProvider reloj)
{
    private readonly Dictionary<(Guid, string), Sesion> sesiones = [];
    internal object Sincronizacion => almacen.Infraestructura.Sincronizacion;

    internal Sesion Incorporar(Guid id, string? credencial, Incorporacion datos)
    {
        if (datos.IdSesion == Guid.Empty || string.IsNullOrWhiteSpace(datos.Nombre))
            throw ErrorApi.Entrada();
        lock (Sincronizacion)
        {
            var rol = almacen.RegistrarNombre(id, credencial, datos.Nombre, rol =>
            {
                if (Vigente(id, rol) is { } actual && actual.IdSesion != datos.IdSesion)
                    throw new ErrorApi(409, "sesionOcupada", "La plaza tiene una sesión vigente.");
            });
            var sesion = new Sesion(rol, datos.IdSesion, reloj.GetUtcNow().AddSeconds(15));
            sesiones[(id, rol)] = sesion;
            return sesion;
        }
    }

    internal void Renovar(Guid id, string? credencial, Guid idSesion)
    {
        lock (Sincronizacion)
        {
            var rol = almacen.Autenticar(id, credencial);
            var sesion = ExigirSesion(id, rol, idSesion);
            sesiones[(id, rol)] = sesion with { VenceUtc = reloj.GetUtcNow().AddSeconds(15) };
        }
    }

    internal void CerrarSesion(Guid id, string rol)
    {
        lock (Sincronizacion)
        {
            almacen.ExigirParticipante(id, rol);
            sesiones.Remove((id, rol));
        }
    }

    internal string ReemplazarCredencial(Guid id, string rol)
    {
        lock (Sincronizacion)
        {
            var nueva = almacen.ReemplazarCredencial(id, rol);
            sesiones.Remove((id, rol));
            return nueva;
        }
    }

    internal bool AmbosPresentes(Guid id)
    {
        lock (Sincronizacion) return Vigente(id, "J1") is not null && Vigente(id, "J2") is not null;
    }

    private Sesion? Vigente(Guid id, string rol) =>
        sesiones.TryGetValue((id, rol), out var sesion) && sesion.VenceUtc > reloj.GetUtcNow() ? sesion : null;

    internal Sesion ExigirSesion(Guid id, string rol, Guid idSesion)
    {
        lock (Sincronizacion)
            return Vigente(id, rol) is { } sesion && sesion.IdSesion == idSesion
                ? sesion : throw new ErrorApi(409, "sesionVencida", "Se requiere una sesión vigente.");
    }
}
```

## Relaciones y verificación

Véanse [arquitectura](../../../../arquitectura.md), [modelos](../../../../modelos.md) y [auditoría del spec 006](../../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
