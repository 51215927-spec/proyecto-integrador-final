using System.Diagnostics;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ArranqueRestTests
{
    [ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task TuberiaCerradaTerminaSinEsperarEntradaNiExponerCredencial()
    {
        var inicio = new ProcessStartInfo("dotnet")
        { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
        using var proceso = Process.Start(inicio)!;
        try
        {
            proceso.StandardOutput.Close();
            await proceso.StandardInput.WriteAsync("http://127.0.0.1:1\n11223344-1122-3344-5566-778899aabbcc\nsecreto-tuberia\nAna\n");
            await proceso.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            string errores = await proceso.StandardError.ReadToEndAsync();
            Assert.Equal(1, proceso.ExitCode);
            Assert.Contains("No se pudo usar la consola", errores);
            Assert.DoesNotContain("secreto-tuberia", errores);
        }
        finally
        {
            if (!proceso.HasExited) { proceso.Kill(true); await proceso.WaitForExitAsync(); }
        }
    }

    [ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task RecuperacionExcepcionalCorrigeCamposYNoOfreceDemo()
    {
        var inicio = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
        using var proceso = Process.Start(inicio)!;
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var salida = proceso.StandardOutput.ReadToEndAsync(plazo.Token);
        await proceso.StandardInput.WriteAsync("recuperar acceso antiguo\ninvalida\nhttp://localhost:5000/prefijo/\ninvalido\n00000000-0000-0000-0000-000000000000\n");
        proceso.StandardInput.Close();
        await proceso.WaitForExitAsync(plazo.Token);
        var texto = await salida;
        Assert.Equal(0, proceso.ExitCode);
        Assert.Contains("URL base", texto);
        Assert.Contains("UUID", texto);
        Assert.DoesNotContain("Demostración", texto);
        Assert.DoesNotContain("Credencial", texto); // No avanza con un UUID vacío.
    }
}
