using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Proyecto_Integrador_Final.Cliente;

public sealed class ClienteConsolaContratoTests
{

    [Fact]
    public async Task Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince()
    {
        var salida = await EjecutarDemo(null, new string('\n', 16));
        EnOrden(salida, "Tu mano", "> Lysander", "Enter para confirmar", "Atributo del combate", "> Fuerza",
            "Recepción confirmada", "Ganador", "Te toca responder", "Ganador",
            "gana la ronda", "gana la partida");
        Assert.Contains("Velocidad", salida);
        Assert.Contains("Resistencia", salida);
        Assert.Contains("Inteligencia", salida);
        Assert.Contains("Energía", salida);
        Assert.Contains("Habilidad", salida);
        Assert.Contains("Mateo 15", salida);
        Assert.Equal(2, salida.Split("Atributo · ", StringSplitOptions.None).Length - 1);
        Assert.Equal(1, salida.Split("Atributo del combate", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta()
    {
        var salida = await EjecutarDemo(null, "w\ns\n" + new string('\n', 16));
        EnOrden(salida, "> Lysander", "> Reched", "Atributo del combate",
            "Recepción confirmada", "Atributo · Fuerza", "Lucía · Reched");
    }

    [Fact]
    public async Task Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir()
    {
        var salida = await EjecutarDemo("pausa", "\n\n\n\n\n");
        EnOrden(salida, "Sin comunicación", "Estado actual sin confirmar",
            "reintentar o salir", "Último marcador confirmado", "Lucía 14 / Mateo 14",
            "Salida local", "La partida queda guardada");
    }

    [Fact]
    public async Task Demo_DistingueAbandonoCancelacionYSalidaLocal()
    {
        var abandono = await EjecutarDemo("abandono", "\n\n\n\n");
        var cancelacion = await EjecutarDemo("cancelacion", "\n\n\n\n");
        var salida = await EjecutarDemo("salida", "\n\n\n\n");

        EnOrden(abandono, "Final confirmado", "abandonó", "sin sumar una victoria");
        EnOrden(cancelacion, "Final confirmado", "canceló", "sin sumar una victoria");
        EnOrden(salida, "Salida local", "La partida queda guardada");
        Assert.DoesNotContain("Final confirmado", salida);
    }

    [Fact]
    public async Task Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada()
    {
        var salida = await EjecutarDemo(null, string.Empty);
        EnOrden(salida, "Tu mano", "Enter para confirmar", "Fin de la entrada");
        Assert.DoesNotContain("Recepción confirmada", salida);
    }

    private static void EnOrden(string salida, params string[] fragmentos)
    {
        var inicio = 0;
        foreach (var fragmento in fragmentos)
        {
            var indice = salida.IndexOf(fragmento, inicio, StringComparison.Ordinal);
            Assert.True(indice >= 0, $"Falta «{fragmento}» después de la posición {inicio}.\n{salida}");
            inicio = indice + fragmento.Length;
        }
    }

    // La demostración se conserva como auxiliar; ya no forma parte del ejecutable.
    private static Task<string> EjecutarDemo(string? modo, string entrada)
    {
        using var lector = new StringReader(entrada);
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(lector, salida);
        try { new DemostracionConsola(interfaz, lector, salida, false).Recorrer(modo); }
        catch (EndOfStreamException) { salida.WriteLine("Fin de la entrada"); }
        return Task.FromResult(salida.ToString());
    }
}
