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
