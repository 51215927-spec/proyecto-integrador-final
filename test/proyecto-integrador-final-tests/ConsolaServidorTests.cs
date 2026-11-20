using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ConsolaServidorTests
{
    [Fact]
    public void Ayuda_ExplicaLasAccionesYLosDatosQueSePiden()
    {
        string salida = Ejecutar("ayuda\n");
        Assert.Contains("Consola del servidor de Guerra", salida);
        Assert.Contains("los dos jugadores", salida);
        Assert.Contains("seis personajes activos", salida);
        Assert.DoesNotContain("<idPartida>", salida);
    }

    [Fact]
    public void Cancelar_PideUuidYReintentaSinPerderLaAccion()
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cancelar\nincorrecto\n{partida.Id}\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        Assert.Contains("UUID", salida.ToString());
        Assert.Contains("válido", salida.ToString());
        Assert.Contains("Partida cancelada.", salida.ToString());
    }

    [Theory]
    [InlineData("cancelar\n")]
    [InlineData("cerrar-sesion\n12345678-1234-4234-8234-123456789abc\n")]
    [InlineData("desactivar\nno\n")]
    public void FinDeEntrada_NoConfirmaAccionIncompleta(string comandos)
    {
        string salida = Ejecutar(comandos);
        Assert.DoesNotContain("Partida cancelada.", salida);
        Assert.DoesNotContain("Sesión cerrada.", salida);
        Assert.DoesNotContain("Personaje desactivado.", salida);
    }

    [Fact]
    public void CerrarSesion_PidePlazaYPermiteCorregirla()
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cerrar-sesion\n{partida.Id}\nJ3\nj2\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        Assert.Contains("J1 o J2", salida.ToString());
        Assert.Contains("Sesión cerrada.", salida.ToString());
    }

    private static string Ejecutar(string comandos)
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        using var entrada = new StringReader(comandos);
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        return salida.ToString();
    }
}
