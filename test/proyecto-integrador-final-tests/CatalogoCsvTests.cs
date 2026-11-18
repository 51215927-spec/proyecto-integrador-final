using Proyecto_Integrador_Final.Api;
using Xunit;

namespace Proyecto_Integrador_Final.Tests;

public class CatalogoCsvTests
{
    private const string Encabezado = "habilidad,id,nombre,descripcion,ascii,fuerza,velocidad,resistencia,inteligencia,energia";

    [Fact]
    public void LeeColumnasReordenadasYConservaLosEscapesDelCatalogo()
    {
        const string fila = """750,17,"Áda, la primera","Comillas: \" y barra: \\","*\n**",250,350,450,550,650""";
        var personaje = Assert.Single(Leer(Encabezado, "", fila));

        Assert.Equal(17, personaje.Id);
        Assert.Equal("Áda, la primera", personaje.Nombre);
        Assert.Equal("Comillas: \" y barra: \\", personaje.Descripcion);
        Assert.Equal("*\n**", personaje.Ascii);
        Assert.Equal(250, personaje.Fuerza);
        Assert.Equal(750, personaje.Habilidad);
    }

    [Theory]
    [InlineData("""750,17,"Ada",null,"*",250,350,450,550,650""")]
    [InlineData("""750,17,"Ada","","*\t",250,350,450,550,650""")]
    [InlineData("""750,17,"Ada","","*",250,350,450,550""")]
    [InlineData("""750,17,"Ada","","*",250,350,450,550,abc""")]
    [InlineData("""750,17,"Ada","","*,250,350,450,550,650""")]
    public void IdentificaLaLineaDeUnRegistroInvalido(string fila)
    {
        var error = Assert.Throws<FormatException>(() => Leer(Encabezado, "", fila));
        Assert.Contains("Línea 3", error.Message);
    }

    [Fact]
    public void DistingueTextoLiteralNullDeUnValorAusente()
    {
        var personaje = Assert.Single(Leer(Encabezado,
            """750,17,"Ada","null","",250,350,450,550,650"""));
        Assert.Equal("null", personaje.Descripcion);
        Assert.Empty(personaje.Ascii);
    }

    private static PersonajeCatalogo[] Leer(params string[] lineas)
    {
        string ruta = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(ruta, lineas);
            return CatalogoCsv.Leer(ruta);
        }
        finally
        {
            File.Delete(ruta);
        }
    }
}
