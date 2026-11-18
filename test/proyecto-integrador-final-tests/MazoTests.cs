namespace Proyecto_Integrador_Final.Tests;

public class MazoTests
{
    [Fact]
    public void CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal()
    {
        var cartas = Enumerable.Range(0, 7).Select(_ => CartasDePrueba.Crear()).ToArray();
        var origen = cartas.ToList();
        var mazo = new Mazo(origen);
        origen.Clear();

        Assert.Equal(7, mazo.GetCantidad());
        var paraElPrimero = mazo.Repartir(3);
        Assert.Equal(cartas[..3], paraElPrimero);
        Assert.Equal(4, mazo.GetCantidad());

        var paraElSegundo = mazo.Repartir(3);
        Assert.Equal(cartas[3..6], paraElSegundo);
        Assert.Equal(1, mazo.GetCantidad());

        mazo.Devolver([.. paraElPrimero, .. paraElSegundo]);
        Assert.Equal(7, mazo.GetCantidad());
        Assert.Equal(new[] { cartas[6] }.Concat(cartas[..6]), mazo.Repartir(7));
        Assert.Equal(0, mazo.GetCantidad());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    public void MezclarConservaTodasLasCartas(int cantidad)
    {
        var cartas = Enumerable.Range(0, cantidad).Select(_ => CartasDePrueba.Crear()).ToList();
        var mazo = new Mazo(cartas);

        mazo.Mezclar();

        Assert.Equal(cantidad, mazo.GetCantidad());
        var repartidas = mazo.Repartir(cantidad);
        Assert.All(cartas, carta => Assert.Single(repartidas, repartida => ReferenceEquals(carta, repartida)));
    }
}
