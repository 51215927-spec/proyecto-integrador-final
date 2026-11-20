using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public sealed class ClienteConsolaContratoTests
{
    private const string Dll = "GUERRA_CLIENTE_DLL";

    [ContratoFact(Dll)]
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

    [ContratoFact(Dll)]
    public async Task Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta()
    {
        var salida = await EjecutarDemo(null, "w\ns\n" + new string('\n', 16));
        EnOrden(salida, "> Lysander", "> Reched", "Atributo del combate",
            "Recepción confirmada", "Atributo · Fuerza", "Lucía · Reched");
    }

    [ContratoFact(Dll)]
    public async Task Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir()
    {
        var salida = await EjecutarDemo("pausa", "\n\n\n\n\n");
        EnOrden(salida, "Sin comunicación", "Estado actual sin confirmar",
            "reintentar o salir", "Último marcador confirmado", "Lucía 14 / Mateo 14",
            "Salida local", "La partida queda guardada");
    }

    [ContratoFact(Dll)]
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

    [ContratoFact(Dll)]
    public async Task Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada()
    {
        var salida = await EjecutarDemo(null, string.Empty);
        EnOrden(salida, "Tu mano", "Enter para confirmar", "Fin de la entrada");
        Assert.DoesNotContain("Recepción confirmada", salida);
    }

    [ContratoFact(Dll)]
    public async Task Menu_ReintentaUnaOpcionInvalidaYPermiteElegirSalida()
    {
        var salida = await EjecutarDemo("incorrecta\nsalida", "");
        EnOrden(salida, "Demostración", "Elegí", "Salida local");
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

    private static async Task<string> EjecutarDemo(string? modo, string entrada)
    {
        var ruta = Path.GetFullPath(Environment.GetEnvironmentVariable(Dll)!);
        Assert.True(File.Exists(ruta), $"No existe la DLL en {ruta}");
        var inicio = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.ArgumentList.Add(ruta);
        entrada = (modo ?? "victoria") + "\n" + entrada;
        using var proceso = Process.Start(inicio)!;
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            var salida = proceso.StandardOutput.ReadToEndAsync(plazo.Token);
            var errores = proceso.StandardError.ReadToEndAsync(plazo.Token);
            await proceso.StandardInput.WriteAsync(entrada.AsMemory(), plazo.Token);
            proceso.StandardInput.Close();
            await proceso.WaitForExitAsync(plazo.Token);
            Assert.True(proceso.ExitCode == 0, await errores);
            return await salida;
        }
        finally
        {
            if (!proceso.HasExited)
            {
                proceso.Kill(entireProcessTree: true);
                await proceso.WaitForExitAsync();
            }
        }
    }
}
