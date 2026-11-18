using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ConsolaServidorTests
{
    [Fact]
    public void Ayuda_ExplicaArgumentosYComoEmpezar()
    {
        string inicio = Ejecutar("");
        string salida = Ejecutar("ayuda");

        Assert.Equal(inicio + inicio, salida);
        Assert.Contains("Consola del servidor de Guerra", salida);
        Assert.Contains("Partidas", salida);
        Assert.Contains("Jugadores", salida);
        Assert.Contains("Catálogo", salida);
        Assert.Contains("iniciar <idPartida>", salida);
        Assert.Contains("los dos jugadores", salida);
        Assert.Contains("seis personajes activos", salida);
        Assert.Contains("J1 o J2", salida);
        Assert.Contains("ID numérico", salida);
        Assert.Contains("desactivar 12", salida);
    }

    [Theory]
    [InlineData("iniciar abc", "Uso: iniciar <idPartida>")]
    [InlineData("cerrar-sesion 12345678-1234-4234-8234-123456789abc J3", "Uso: cerrar-sesion <idPartida> <J1|J2>")]
    [InlineData("crear extra", "Uso: crear")]
    [InlineData("desactivar abc", "Uso: desactivar <idPersonaje>")]
    [InlineData("listar", "Comando desconocido: listar. Escribí ayuda")]
    public void ComandoInvalido_MuestraComoCorregirlo(string comando, string mensaje)
    {
        string salida = Ejecutar(comando);
        Assert.Contains(mensaje, salida[Ejecutar("").Length..]);
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
