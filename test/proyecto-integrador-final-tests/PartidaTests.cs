namespace Proyecto_Integrador_Final.Tests;

public class PartidaTests
{
    private static CartaPartida Carta(int id) => new() { IdPersonaje = id, Nombre = $"Carta {id}", Descripcion = "", Ascii = "", Fuerza = 700, Velocidad = 300, Resistencia = 500, Inteligencia = 500, Energia = 500, Habilidad = 500 };
    private static Partida Nueva() => new(new EstadoPartida());
    private static void Elegir(Partida partida)
    {
        var estado = partida.Estado;
        var solicitud = estado.Solicitudes.Single(s => s.Id == estado.IdSolicitud);
        var carta = estado.Cartas.First(c => c.Ubicacion == solicitud.Rol);
        partida.RegistrarEleccion(solicitud.Rol, solicitud.Id, estado.Revision, carta.IdPersonaje,
            solicitud.Tipo == "cartaYAtributo" ? "Fuerza" : null);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void RechazaUnMazoInsuficienteAntesDeRepartir(int cantidad)
    {
        var partida = Nueva();
        Assert.Throws<ExcepcionPartida>(() => partida.Iniciar(Enumerable.Range(1, cantidad).Select(Carta)));
        Assert.Empty(partida.Estado.Cartas);
        Assert.Equal(0, partida.Estado.Revision);
    }

    [Fact]
    public void AccionesSucesivasReanudanYCompletanQuinceRondasSinCambiarLaMeta()
    {
        var partida = Nueva();
        partida.Iniciar(Enumerable.Range(1, 8).Select(Carta));
        Assert.Equal(3, partida.Estado.Cartas.Count(c => c.Ubicacion == "J1"));
        Assert.Equal(3, partida.Estado.Cartas.Count(c => c.Ubicacion == "J2"));
        var primera = partida.Estado.IdSolicitud;
        Elegir(partida);
        partida = new Partida(partida.Estado); // reconstruir no repite efectos
        Assert.Equal(2, partida.Estado.Revision);
        Assert.NotEqual(primera, partida.Estado.IdSolicitud);
        Elegir(partida);
        Assert.Equal("J2", partida.Estado.Combates[0].Ganador); // empate al rival
        Assert.Equal("J2", partida.Estado.Combates[1].Elector);
        int acciones = 2;
        while (partida.Estado.Fase != "finalizada" && acciones++ < 400) Elegir(partida);
        var estado = partida.Estado;
        Assert.Equal("victoria", estado.MotivoFin);
        Assert.Equal(15, estado.Participantes.Max(p => p.RondasGanadas));
        Assert.Equal(29, estado.Rondas.Count); // empates alternan ganador de ronda
        Assert.Null(estado.IdSolicitud);
        Assert.All(estado.Rondas, r => Assert.Equal(2, Math.Max(r.VictoriasJ1, r.VictoriasJ2)));
        Assert.All(estado.Rondas, r => Assert.Equal(r.Numero % 2 == 1 ? "J1" : "J2", r.Mano));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CierreNoInventaVictoriaYConsumeSolicitud(bool abandono)
    {
        var partida = Nueva();
        partida.Iniciar(Enumerable.Range(1, 6).Select(Carta));
        if (abandono) { partida.Abandonar("J1"); partida.Abandonar("J1"); }
        else partida.Cancelar();
        Assert.Equal(2, partida.Estado.Revision);
        Assert.Null(partida.Estado.Ganador);
        Assert.Null(partida.Estado.IdSolicitud);
        Assert.All(partida.Estado.Solicitudes, s => Assert.True(s.Consumida));
    }

    [Fact]
    public void EleccionInvalidaNoConsumeSolicitudNiModificaRevision()
    {
        var partida = Nueva();
        partida.Iniciar(Enumerable.Range(1, 6).Select(Carta));
        var id = partida.Estado.IdSolicitud!.Value;
        Assert.Equal("eleccionInvalida", Assert.Throws<ExcepcionPartida>(() =>
            partida.RegistrarEleccion("J1", id, 1, -1, "Fuerza")).Codigo);
        Assert.Equal(1, partida.Estado.Revision);
        Assert.False(partida.Estado.Solicitudes[0].Consumida);
    }
}
