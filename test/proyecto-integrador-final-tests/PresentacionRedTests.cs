using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class PresentacionRedTests
{
    [Theory]
    [InlineData(38)]
    [InlineData(100)]
    public void MarcadorDelResultadoCorrespondeASuRondaYNeutralizaTextoRemoto(int ancho)
    {
        // Se mezclan resultados de dos rondas para detectar marcadores históricos incorrectos.
        Guid id = Guid.NewGuid();
        var primero = EstadoClienteTests.Resultado(1, 1);
        var cierre = EstadoClienteTests.Resultado(1, 2) with { GanadorRonda = "J2" };
        var posterior = EstadoClienteTests.Resultado(2, 1) with { GanadorCombate = "J1" };
        var vista = EstadoClienteTests.Vista(id) with
        {
            NumeroRonda = 2, NumeroCombate = 2, Marcador = new() { ["J1"] = 7, ["J2"] = 9 },
            Nombres = new() { ["J1"] = "Ana\u001b[31m", ["J2"] = "Ada" }, Resultados = [primero, cierre, posterior]
        };
        // La misma presentación debe poder dibujarse con los dos anchos de consola.
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader(""), salida, ancho);
        interfaz.MostrarRed("Ana", vista, true, "En línea", cierre, -1, false, 0, false, false, false);
        string texto = salida.ToString();
        // El resultado usa su marcador histórico y neutraliza controles incluidos en nombres remotos.
        Assert.Contains("gana la ronda", texto);
        Assert.Contains("Ada 1", texto);
        Assert.DoesNotContain("Ada 9", texto);
        Assert.DoesNotContain('\u001b', texto);
        Assert.DoesNotContain("Tu turno", texto);
    }
}
