using System.Diagnostics;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public sealed class EntradaProcesosContratoTests
{
    [ContratoFact("GUERRA_SERVIDOR_DLL")]
    public async Task Servidor_FinDeEntradaDuranteConfiguracionNoIniciaHostNiCreaBase()
    {
        foreach (string entrada in new[] { "", "\n", "\n\n", "invalida\n" })
        {
            var resultado = await Ejecutar("GUERRA_SERVIDOR_DLL", entrada);
            Assert.Equal(0, resultado.Codigo);
            Assert.DoesNotContain("Now listening", resultado.Salida);
            Assert.False(resultado.CreoBase);
        }
    }

    [ContratoFact("GUERRA_SERVIDOR_DLL")]
    public async Task Servidor_SinCredencialRechazaAltasYFinDeEntradaDetieneElHost()
    {
        var resultado = await Ejecutar("GUERRA_SERVIDOR_DLL", "http://127.0.0.1:0\n\n\n");
        Assert.Equal(0, resultado.Codigo);
        Assert.Contains("alta de personajes deshabilitada", resultado.Salida);
        Assert.Contains("Now listening on: http://127.0.0.1:", resultado.Salida);
        Assert.True(resultado.CreoBase);
        Assert.Contains("Application is shutting down", resultado.Salida);
    }

    [ContratoFact("GUERRA_SERVIDOR_DLL")]
    public async Task Servidor_BaseInvalidaPermiteCorregirLaConfiguracionSinExponerCredencial()
    {
        var resultado = await Ejecutar("GUERRA_SERVIDOR_DLL",
            "http://127.0.0.1:0\n\nsecreto-003\nhttp://127.0.0.1:0\ncorregida.db\n\n", baseInvalida: true);
        Assert.Equal(0, resultado.Codigo);
        Assert.Contains("Volvé a ingresar", resultado.Salida);
        Assert.Contains("Now listening", resultado.Salida);
        Assert.DoesNotContain("secreto-003", resultado.Salida + resultado.Errores);
    }

    [ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task Cliente_FinDeEntradaEnMenuSaleSinSimularUnaVictoria()
    {
        var resultado = await Ejecutar("GUERRA_CLIENTE_DLL", "");
        Assert.Equal(0, resultado.Codigo);
        Assert.Contains("Fin de la entrada", resultado.Salida);
        Assert.DoesNotContain("gana la partida", resultado.Salida);
    }

    [ContratoFact("GUERRA_CARGA_DLL")]
    public async Task Carga_SinArgumentosNiCredencialDeEntornoEnviaElContratoExistente()
    {
        await using var app = ServidorHttp.Crear([
            "--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:", "--Catalogo:Credencial=secreto-003"
        ]);
        await app.StartAsync();
        var resultado = await Ejecutar("GUERRA_CARGA_DLL",
            $"{app.Urls.Single()}\nsecreto-003\nAda\n\n0\n250\n350\n450\n550\n650\n750\nno\n");
        Assert.Equal(0, resultado.Codigo);
        Assert.Contains("ID 1", resultado.Salida);
        Assert.DoesNotContain("secreto-003", resultado.Salida + resultado.Errores);
    }

    [ContratoFact("GUERRA_CARGA_DLL")]
    public async Task Carga_FinDeEntradaDuranteConexionOPersonajeSaleSinAlta()
    {
        foreach (string entrada in new[] { "", "\n", "\nsecreto-003\n", "\nsecreto-003\nAda\n\n0\n250\n" })
        {
            var resultado = await Ejecutar("GUERRA_CARGA_DLL", entrada);
            Assert.Equal(1, resultado.Codigo);
            Assert.DoesNotContain("Personaje cargado", resultado.Salida);
            Assert.DoesNotContain("secreto-003", resultado.Salida + resultado.Errores);
        }
    }

    private static async Task<(int Codigo, string Salida, string Errores, bool CreoBase)> Ejecutar(string variable, string entrada, bool baseInvalida = false)
    {
        string directorio = Path.Combine(Path.GetTempPath(), $"guerra-entrada-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        if (baseInvalida) File.WriteAllText(Path.Combine(directorio, "guerra.db"), "esto no es SQLite");
        var inicio = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directorio, UseShellExecute = false,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.ArgumentList.Add(Path.GetFullPath(Environment.GetEnvironmentVariable(variable)!));
        inicio.Environment.Remove("Catalogo__Credencial");
        inicio.Environment.Remove("ConnectionStrings__Juego");
        inicio.Environment.Remove("ASPNETCORE_URLS");
        using var proceso = Process.Start(inicio)!;
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            var salida = proceso.StandardOutput.ReadToEndAsync(plazo.Token);
            var errores = proceso.StandardError.ReadToEndAsync(plazo.Token);
            await proceso.StandardInput.WriteAsync(entrada.AsMemory(), plazo.Token);
            proceso.StandardInput.Close();
            await proceso.WaitForExitAsync(plazo.Token);
            return (proceso.ExitCode, await salida, await errores, File.Exists(Path.Combine(directorio, "guerra.db")));
        }
        finally
        {
            if (!proceso.HasExited)
            {
                proceso.Kill(entireProcessTree: true);
                await proceso.WaitForExitAsync();
            }
            Directory.Delete(directorio, recursive: true);
        }
    }
}
