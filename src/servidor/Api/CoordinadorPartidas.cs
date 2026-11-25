namespace Proyecto_Integrador_Final.Api;

internal sealed class CoordinadorPartidas(AlmacenPartidas almacen, AdministradorSesiones sesiones, CatalogoPersonajes catalogo, Avisos? avisos = null)
{
    private readonly VistaPartida vista = new();
    internal PartidaCreada CrearPartida() => almacen.CrearPartida();

    internal void Iniciar(Guid id) => Ejecutar(id, true, unidad =>
    {
        if (unidad.Partida.Estado.Fase != "esperandoInicio")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya se inició o finalizó.");
        ExigirPresencia(id);
        unidad.Partida.Iniciar(unidad.CopiarCatalogoActivo(catalogo));
        return true;
    });

    internal void Cancelar(Guid id) => Ejecutar(id, true, unidad =>
    {
        unidad.Partida.Cancelar();
        return true;
    });

    internal Confirmacion Elegir(Guid id, string? credencial, Guid idSesion, Eleccion datos)
    {
        if (datos.IdOperacion == Guid.Empty || datos.IdSolicitud == Guid.Empty ||
            datos.RevisionEsperada is null or < 0 || datos.IdPersonaje is null)
            throw ErrorApi.Entrada();
        datos = datos with { Atributo = Partida.NormalizarAtributo(datos.Atributo) };
        return Ejecutar(id, true, unidad =>
        {
            var rol = Acceso(unidad, id, credencial, idSesion);
            if (unidad.Recuperar(rol, datos) is { } anterior) return anterior;
            if (unidad.Partida.Estado.Fase is "esperandoElector" or "esperandoRival") ExigirPresencia(id);
            unidad.Partida.RegistrarEleccion(rol, datos.IdSolicitud, datos.RevisionEsperada.Value,
                datos.IdPersonaje.Value, datos.Atributo);
            return unidad.Confirmar(rol, datos);
        });
    }

    internal Recibo ConsultarRecibo(Guid id, string? credencial, Guid idSesion, Guid idOperacion) =>
        Ejecutar(id, false, unidad => unidad.ConsultarRecibo(Acceso(unidad, id, credencial, idSesion), idOperacion));

    internal void Abandonar(Guid id, string? credencial, Guid idSesion) => Ejecutar(id, true, unidad =>
    {
        unidad.Partida.Abandonar(Acceso(unidad, id, credencial, idSesion));
        return true;
    });

    internal PresentacionVisible Presentacion(Guid id, string? credencial, Guid idSesion) =>
        Ejecutar(id, false, unidad => vista.Construir(unidad.Partida.Estado,
            Acceso(unidad, id, credencial, idSesion), sesiones.AmbosPresentes(id)));

    private string Acceso(UnidadPartida unidad, Guid id, string? credencial, Guid idSesion)
    {
        var rol = unidad.Autenticar(credencial);
        sesiones.ExigirSesion(id, rol, idSesion);
        return rol;
    }

    private void ExigirPresencia(Guid id)
    {
        if (!sesiones.AmbosPresentes(id))
            throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");
    }

    private T Ejecutar<T>(Guid id, bool escritura, Func<UnidadPartida, T> accion)
    {
        lock (sesiones.Sincronizacion)
        {
            try
            {
                var resultado = almacen.Ejecutar(id, escritura, accion);
                if (escritura) avisos?.PublicarPartida(almacen, id);
                return resultado;
            }
            catch (ExcepcionPartida error)
            {
                throw new ErrorApi(error.Codigo == "eleccionInvalida" ? 422 : 409, error.Codigo, error.Message);
            }
        }
    }
}
