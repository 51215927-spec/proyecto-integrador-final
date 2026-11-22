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
