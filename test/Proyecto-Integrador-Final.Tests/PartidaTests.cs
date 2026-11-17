namespace Proyecto_Integrador_Final.Tests;

public class PartidaTests
{
    private readonly Jugador primero = new("Ana");
    private readonly Jugador segundo = new("Bruno");

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void RechazaUnMazoInsuficienteAntesDeRepartir(int cantidad)
    {
        var mazo = new Mazo(Enumerable.Range(0, cantidad).Select(_ => CartasDePrueba.Crear()).ToList());
        var partida = new Partida(primero, segundo, mazo);

        var error = Assert.Throws<InvalidOperationException>(() => partida.Jugar());

        Assert.Equal("No se puede jugar porque no hay suficientes cartas.", error.Message);

        Assert.Equal(cantidad, mazo.GetCantidad());
        Assert.Empty(primero.GetCartas());
        Assert.Empty(segundo.GetCartas());
    }
}
