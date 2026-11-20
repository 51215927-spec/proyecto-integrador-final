using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ConfiguracionServidorTests
{
    [Fact]
    public void Enter_ConservaValoresInicialesYDeshabilitaAltaSinCredencial()
    {
        using var salida = new StringWriter();
        var config = ConfiguracionServidor.Leer(new StringReader("\n\n\n"), salida);
        Assert.Equal("http://0.0.0.0:5000", config.Direccion);
        Assert.Equal("guerra.db", config.Archivo);
        Assert.Equal("", config.Credencial);
        Assert.Contains("alta de personajes deshabilitada", salida.ToString());
    }

    [Fact]
    public void ConfiguracionInvalida_PermiteCorregirCadaCampoSinMostrarLaCredencial()
    {
        using var salida = new StringWriter();
        var archivo = Path.Combine(Path.GetTempPath(), $"guerra-{Guid.NewGuid()}.db");
        var config = ConfiguracionServidor.Leer(new StringReader(
            $"ftp://localhost\nhttp://127.0.0.1:0\n\0\n{archivo}\nclave secreta\nsecreto-003\n"), salida);
        Assert.Equal("http://127.0.0.1:0", config.Direccion);
        Assert.Equal(archivo, config.Archivo);
        Assert.Equal("secreto-003", config.Credencial);
        Assert.DoesNotContain("secreto-003", salida.ToString());
        Assert.DoesNotContain("clave secreta", salida.ToString());
        Assert.False(File.Exists(archivo));
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\n\n")]
    [InlineData("incorrecta\n")]
    public void FinDeEntrada_InterrumpeLaConfiguracion(string texto)
    {
        Assert.Throws<EndOfStreamException>(() =>
            ConfiguracionServidor.Leer(new StringReader(texto), new StringWriter()));
    }
}
