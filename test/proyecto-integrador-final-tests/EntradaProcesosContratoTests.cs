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
        string ruta = Path.Combine(Path.GetTempPath(), $"personajes {Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            var resultado = await Ejecutar("GUERRA_CARGA_DLL",
                $"{ruta}\n{app.Urls.Single()}\nsecreto-003\nsi\n");
            Assert.Equal(0, resultado.Codigo);
            Assert.Contains("Catálogo reemplazado", resultado.Salida);
            Assert.DoesNotContain("secreto-003", resultado.Salida + resultado.Errores);
        }
        finally { File.Delete(ruta); }
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

    [ContratoFact("GUERRA_CARGA_DLL")]
    public async Task Carga_RecorridoCompletoDesdeProcesoConBaseTemporal()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"recorrido-{Guid.NewGuid()}.db");
        string archivo = Path.Combine(Path.GetTempPath(), $"un personaje {Guid.NewGuid()}.json");
        string ejemplos = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Environment.GetEnvironmentVariable("GUERRA_CARGA_DLL"))!, "../../../ejemplos/personajes.json"));
        await File.WriteAllTextAsync(archivo, CargaJsonTests.Uno);
        try
        {
            await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={ruta};Pooling=False", "--Catalogo:Credencial=clave"]);
            await app.StartAsync();
            using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            http.DefaultRequestHeaders.Authorization = new("Bearer", "clave");
            async Task Cantidad(int esperada)
            {
                using var respuesta = await http.GetAsync("/api/v1/personajes/resumen");
                Assert.Contains($"\"cantidad\":{esperada}", await respuesta.Content.ReadAsStringAsync());
            }
            await Cantidad(0);
            foreach (var paso in new[] { (ejemplos,"si",6), (archivo,"no",6), (archivo,"si",1), (archivo,"si",1), (ejemplos,"si",6) })
            {
                var resultado = await Ejecutar("GUERRA_CARGA_DLL", $"{app.Urls.Single()}\nclave\n{paso.Item2}\n", argumentos: [paso.Item1]);
                Assert.Equal(0, resultado.Codigo);
                Assert.DoesNotContain("Ruta del archivo JSON:", resultado.Salida);
                await Cantidad(paso.Item3);
            }
            await File.WriteAllTextAsync(archivo, CargaJsonTests.Uno.Replace("250", "249"));
            var rechazo = await Ejecutar("GUERRA_CARGA_DLL", $"{app.Urls.Single()}\nclave\nsi\n", argumentos: [archivo]);
            Assert.Equal(1, rechazo.Codigo);
            Assert.Contains("Personaje 1 (Ada)", rechazo.Salida);
            await Cantidad(6);
        }
        finally { File.Delete(archivo); File.Delete(ruta); }
    }

    private static async Task<(int Codigo, string Salida, string Errores, bool CreoBase)> Ejecutar(string variable, string entrada, bool baseInvalida = false, string[]? argumentos = null)
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
        foreach (var argumento in argumentos ?? []) inicio.ArgumentList.Add(argumento);
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
