namespace Proyecto_Integrador_Final.Tests;

public class PersonajeTests
{
    [Fact]
    public void ConservaNombreAsciiYLosSeisAtributos()
    {
        var carta = new Personaje("Lysander", " /\\", 250, 750, 350, 650, 450, 550);

        Assert.Equal("Lysander", carta.GetNombre());
        Assert.Equal(" /\\", carta.GetAscii());
        Assert.Equal(250, carta.GetAtributo(TipoAtributo.Fuerza));
        Assert.Equal(750, carta.GetAtributo(TipoAtributo.Velocidad));
        Assert.Equal(350, carta.GetAtributo(TipoAtributo.Resistencia));
        Assert.Equal(650, carta.GetAtributo(TipoAtributo.Inteligencia));
        Assert.Equal(450, carta.GetAtributo(TipoAtributo.Energia));
        Assert.Equal(550, carta.GetAtributo(TipoAtributo.Habilidad));
    }

    [Fact]
    public void AceptaUmbralesInclusivosYAsciiVacio()
    {
        var carta = new Personaje("Carta", "", 650, 350, 500, 500, 500, 500);

        Assert.Empty(carta.GetAscii());
        Assert.Equal(650, carta.GetAtributo(TipoAtributo.Fuerza));
        Assert.Equal(350, carta.GetAtributo(TipoAtributo.Velocidad));
    }

    [Theory]
    [InlineData(649, 350, 501, 500)]
    [InlineData(650, 351, 499, 500)]
    [InlineData(650, 350, 499, 500)]
    [InlineData(650, 350, 501, 500)]
    [InlineData(750, 249, 501, 500)]
    [InlineData(751, 250, 499, 500)]
    [InlineData(500, 500, 500, 500)]
    public void RechazaAtributosQueIncumplenUnaRegla(int fuerza, int velocidad, int resistencia, int inteligencia)
    {
        Assert.Throws<ArgumentException>(() =>
            new Personaje("Carta", "", fuerza, velocidad, resistencia, inteligencia, 500, 500));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RechazaNombreSinTexto(string? nombre)
    {
        Assert.Throws<ArgumentException>(() => new Personaje(nombre!, "", 650, 350, 500, 500, 500, 500));
    }

    [Fact]
    public void RechazaAsciiNulo()
    {
        Assert.Throws<ArgumentException>(() => new Personaje("Carta", null!, 650, 350, 500, 500, 500, 500));
    }
}
