namespace Proyecto_Integrador_Final.Tests;

public class RondaTests
{
    private readonly Jugador mano = new("Ana");
    private readonly Jugador rival = new("Bruno");

    [Fact]
    public void EmpiezaSinCombatesYEligeElJugadorMano()
    {
        var ronda = new Ronda(mano, rival);

        Assert.Empty(ronda.GetCombates());
        Assert.False(ronda.EstaDefinida());
        Assert.Same(mano, ronda.GetQuienEligeProximo());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DosVictoriasSeguidasCierranLaRonda(bool ganaMano)
    {
        var ronda = new Ronda(mano, rival);
        var ganador = ganaMano ? mano : rival;
        var primero = ResolverCombate(mano, rival, ganador);
        ronda.AgregarCombate(primero);

        Assert.False(ronda.EstaDefinida());
        Assert.Same(rival, ronda.GetQuienEligeProximo());

        var segundo = ResolverCombate(rival, mano, ganador);
        ronda.AgregarCombate(segundo);

        Assert.True(ronda.EstaDefinida());
        Assert.Same(ganador, ronda.GetGanador());
        ronda.GetCombates()[0] = segundo;
        Assert.Equal(new[] { primero, segundo }, ronda.GetCombates());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ElTercerCombateDefineLaRondaCuandoVanUnoAUno(bool ganaMano)
    {
        var ronda = new Ronda(mano, rival);
        ronda.AgregarCombate(ResolverCombate(mano, rival, mano));
        ronda.AgregarCombate(ResolverCombate(rival, mano, rival));

        Assert.False(ronda.EstaDefinida());
        Assert.Same(mano, ronda.GetQuienEligeProximo());

        var ganador = ganaMano ? mano : rival;
        ronda.AgregarCombate(ResolverCombate(mano, rival, ganador));

        Assert.True(ronda.EstaDefinida());
        Assert.Same(ganador, ronda.GetGanador());
        Assert.Equal(3, ronda.GetCombates().Length);
    }

    private static Combate ResolverCombate(Jugador elector, Jugador oponente, Jugador ganador)
    {
        var cartaElector = CartasDePrueba.Crear(ReferenceEquals(elector, ganador) ? 750 : 650);
        var cartaOponente = CartasDePrueba.Crear(ReferenceEquals(oponente, ganador) ? 750 : 650);
        elector.RecibirCartas([cartaElector]);
        oponente.RecibirCartas([cartaOponente]);

        var combate = new Combate(elector, oponente);
        combate.SetAtributo(TipoAtributo.Fuerza);
        combate.SetCarta(elector, cartaElector);
        combate.SetCarta(oponente, cartaOponente);
        combate.Resolver();
        return combate;
    }
}
