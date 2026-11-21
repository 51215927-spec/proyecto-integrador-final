using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ConfiguracionClienteTests
{
    // La tabla cubre URLs admitidas y componentes que no deben aceptar datos de conexión.
    [Theory]
    [InlineData("http://localhost:5000", true)]
    [InlineData("https://localhost/prefijo", true)]
    [InlineData("/relativa", false)]
    [InlineData("ftp://localhost", false)]
    [InlineData("http://usuario:clave@localhost", false)]
    [InlineData("http://localhost/?consulta=1", false)]
    [InlineData("http://localhost/#fragmento", false)]
    public void ValidaUrlSegunContrato(string url, bool valida) => Assert.Equal(valida, ConfiguracionCliente.EsUrl(url));

    [Fact]
    public void RecortaExterioresPreservaNombreYRepiteSoloCampoInvalido()
    {
        // La entrada combina errores recuperables, espacios exteriores y un nombre con espacios internos.
        Guid id = Guid.NewGuid();
        using var entrada = new StringReader($"invalida\n https://localhost/prefijo \ninvalido\n{id}\n  secreto  \n  Ana  Pérez  \n");
        using var salida = new StringWriter();
        var config = ConfiguracionCliente.Leer(entrada, salida);
        // La normalización no altera el nombre registrado ni expone la credencial en diagnósticos.
        Assert.Equal("Ana  Pérez", config.Nombre);
        Assert.Equal("secreto", config.Credencial);
        Assert.Equal(new Uri("https://localhost/prefijo/"), config.Url);
        Assert.Equal(id, config.Partida);
        Assert.DoesNotContain("secreto", salida.ToString() + config.ToString());
    }
}
