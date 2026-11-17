namespace Proyecto_Integrador_Final.Tests;

public class JugadorTests
{
    [Fact]
    public void IniciaConNombreManoVaciaYSinVictorias()
    {
        var jugador = new Jugador("Ana");

        Assert.Equal("Ana", jugador.GetNombre());
        Assert.Empty(jugador.GetCartas());
        Assert.Equal(0, jugador.GetRondasGanadas());

        jugador.SumarRondaGanada();
        jugador.SumarRondaGanada();

        Assert.Equal(2, jugador.GetRondasGanadas());
    }

    [Fact]
    public void AcumulaCartasQuitaUnaYEntregaElResto()
    {
        var jugador = new Jugador("Ana");
        var primera = CartasDePrueba.Crear();
        var segunda = CartasDePrueba.Crear();
        var tercera = CartasDePrueba.Crear();

        Personaje[] recibidas = [primera, segunda];
        jugador.RecibirCartas(recibidas);
        recibidas[0] = tercera;
        jugador.RecibirCartas([tercera]);
        Assert.Equal(new[] { primera, segunda, tercera }, jugador.GetCartas());

        jugador.GetCartas()[0] = tercera;
        Assert.Same(primera, jugador.GetCartas()[0]);

        jugador.QuitarCarta(segunda);
        Assert.Equal(new[] { primera, tercera }, jugador.GetCartas());
        Assert.Equal(new[] { primera, tercera }, jugador.EntregarCartas());
        Assert.Empty(jugador.GetCartas());
        Assert.Empty(jugador.EntregarCartas());
    }
}
