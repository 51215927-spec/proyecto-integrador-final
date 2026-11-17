namespace Proyecto_Integrador_Final.Tests;

public class CombateTests
{
    private readonly Jugador elector = new("Ana");
    private readonly Jugador rival = new("Bruno");

    [Fact]
    public void ValidaParticipantesYDevuelveUnaCopiaEnOrden()
    {
        Assert.NotNull(Record.Exception(() => new Combate(null!, rival)));
        Assert.NotNull(Record.Exception(() => new Combate(elector, null!)));
        Assert.NotNull(Record.Exception(() => new Combate(elector, elector)));
        var combate = new Combate(elector, rival);

        combate.GetParticipantes()[0] = rival;

        Assert.Equal(new[] { elector, rival }, combate.GetParticipantes());
    }

    [Fact]
    public void RechazaConsultasPrematurasYAtributosInvalidosORepetidos()
    {
        var combate = new Combate(elector, rival);

        Assert.False(combate.EstaCompleto());
        Assert.NotNull(Record.Exception(() => combate.GetGanador()));
        Assert.NotNull(Record.Exception(() => combate.GetAtributo()));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(elector)));
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
        Assert.NotNull(Record.Exception(() => combate.SetAtributo((TipoAtributo)99)));

        combate.SetAtributo(TipoAtributo.Velocidad);

        Assert.Equal(TipoAtributo.Velocidad, combate.GetAtributo());
        Assert.NotNull(Record.Exception(() => combate.SetAtributo(TipoAtributo.Fuerza)));
        Assert.Equal(TipoAtributo.Velocidad, combate.GetAtributo());
        Assert.False(combate.EstaCompleto());
    }

    [Fact]
    public void RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador()
    {
        var combate = new Combate(elector, rival);
        var cartaElector = CartasDePrueba.Crear();
        var otraCartaElector = CartasDePrueba.Crear();
        var cartaRival = CartasDePrueba.Crear();
        var ajeno = new Jugador("Ana");
        ajeno.RecibirCartas([cartaElector]);
        elector.RecibirCartas([cartaElector, otraCartaElector]);
        rival.RecibirCartas([cartaRival]);

        Assert.NotNull(Record.Exception(() => combate.SetCarta(null!, cartaElector)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, null!)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(ajeno, cartaElector)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, cartaRival)));

        Assert.Equal(new[] { cartaElector, otraCartaElector }, elector.GetCartas());
        Assert.Same(cartaRival, Assert.Single(rival.GetCartas()));

        combate.SetCarta(elector, cartaElector);
        Assert.Same(otraCartaElector, Assert.Single(elector.GetCartas()));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, otraCartaElector)));
        Assert.Same(otraCartaElector, Assert.Single(elector.GetCartas()));

        combate.SetCarta(rival, cartaRival);
        Assert.Empty(rival.GetCartas());
        Assert.False(combate.EstaCompleto());

        combate.SetAtributo(TipoAtributo.Fuerza);
        Assert.True(combate.EstaCompleto());
        Assert.Same(cartaElector, combate.GetCarta(elector));
        Assert.Same(cartaRival, combate.GetCarta(rival));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(null!)));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(ajeno)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo(bool juegaElector)
    {
        var combate = new Combate(elector, rival);
        var jugador = juegaElector ? elector : rival;
        var carta = CartasDePrueba.Crear();
        jugador.RecibirCartas([carta]);
        combate.SetAtributo(TipoAtributo.Fuerza);
        combate.SetCarta(jugador, carta);

        Assert.False(combate.EstaCompleto());
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
    }

    [Theory]
    [InlineData(650, 750, true)]
    [InlineData(750, 650, false)]
    [InlineData(650, 650, false)]
    public void ResuelveSegunElAtributoElegidoYDaElEmpateAlRival(int fuerzaElector, int fuerzaRival, bool ganaElector)
    {
        var combate = new Combate(elector, rival);
        var cartaElector = CartasDePrueba.Crear(fuerzaElector);
        var cartaRival = CartasDePrueba.Crear(fuerzaRival);
        elector.RecibirCartas([cartaElector]);
        rival.RecibirCartas([cartaRival]);

        combate.SetAtributo(TipoAtributo.Velocidad);
        combate.SetCarta(elector, cartaElector);
        Assert.False(combate.EstaCompleto());
        combate.SetCarta(rival, cartaRival);

        Jugador ganadorEsperado = ganaElector ? elector : rival;
        Assert.Same(ganadorEsperado, combate.Resolver());
        Assert.Same(ganadorEsperado, combate.GetGanador());
        Assert.Equal(0, elector.GetRondasGanadas());
        Assert.Equal(0, rival.GetRondasGanadas());
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
    }
}
