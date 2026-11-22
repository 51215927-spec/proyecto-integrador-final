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
        using var almacen = new AlmacenPartidas("Data Source=:memory:");
        var sesiones = new AdministradorSesiones(almacen, TimeProvider.System);
        var catalogo = new CatalogoPersonajes(almacen.Infraestructura);
        var juego = new CoordinadorPartidas(almacen, sesiones, catalogo);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cancelar\nincorrecto\n{partida.Id}\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, sesiones, catalogo, entrada, salida, CancellationToken.None);
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
        using var almacen = new AlmacenPartidas("Data Source=:memory:");
        var sesiones = new AdministradorSesiones(almacen, TimeProvider.System);
        var catalogo = new CatalogoPersonajes(almacen.Infraestructura);
        var juego = new CoordinadorPartidas(almacen, sesiones, catalogo);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cerrar-sesion\n{partida.Id}\nJ3\nj2\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, sesiones, catalogo, entrada, salida, CancellationToken.None);
        Assert.Contains("J1 o J2", salida.ToString());
        Assert.Contains("Sesión cerrada.", salida.ToString());
    }

    private static string Ejecutar(string comandos)
    {
        using var almacen = new AlmacenPartidas("Data Source=:memory:");
        var sesiones = new AdministradorSesiones(almacen, TimeProvider.System);
        var catalogo = new CatalogoPersonajes(almacen.Infraestructura);
        var juego = new CoordinadorPartidas(almacen, sesiones, catalogo);
        using var entrada = new StringReader(comandos);
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, sesiones, catalogo, entrada, salida, CancellationToken.None);
        return salida.ToString();
    }
}
