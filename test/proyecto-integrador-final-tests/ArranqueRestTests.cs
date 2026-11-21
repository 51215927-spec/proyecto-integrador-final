using System.Diagnostics;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ArranqueRestTests
{
    [ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task SolicitaConexionCorrigeCamposYNoOfreceDemo()
    {
        var inicio = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
        using var proceso = Process.Start(inicio)!;
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var salida = proceso.StandardOutput.ReadToEndAsync(plazo.Token);
        await proceso.StandardInput.WriteAsync("invalida\nhttp://localhost:5000/prefijo/\ninvalido\n00000000-0000-0000-0000-000000000000\n");
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
