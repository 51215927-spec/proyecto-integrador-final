using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class InicioAutomaticoProcesoTests
{
    [ProcesoAutomaticoFact]
    public async Task DosClientesInicianSinDatosInternosNoHacenPollingYRetomanAlReabrir()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        var peticiones = new ConcurrentQueue<string>();
        app.Use(async (c, siguiente) => { peticiones.Enqueue(c.Request.Method + " " + c.Request.Path); await siguiente(c); });
        for (int i = 0; i < 6; i++) app.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "automatico-" + Guid.NewGuid());
        Directory.CreateDirectory(carpeta);
        try
        {
            await using var a = new Proceso(Path.Combine(carpeta, "a"));
            await a.Enviar($"conexion\n{app.Urls.Single()}\njugar\n\n");
            await a.Esperar("Buscando oponente");
            await using var b = new Proceso(Path.Combine(carpeta, "b"));
            await b.Enviar($"conexion\n{app.Urls.Single()}\njugar\n\n");
            await a.Esperar("Tu plaza:"); await b.Esperar("Tu plaza:");
            Assert.DoesNotContain("UUID", a.Texto + b.Texto);
            Assert.DoesNotContain("Credencial", a.Texto + b.Texto);
            await Task.Delay(500);
            int Consultas() => peticiones.Count(p => p == "GET /api/v2/acceso" || p.EndsWith("/presentacion"));
            int antes = Consultas();
            await Task.Delay(TimeSpan.FromSeconds(30));
            Assert.Equal(antes, Consultas());
            a.CerrarEntrada(); await a.Terminar();
            await b.Esperar("Pausa confirmada");
            await using var reabierto = new Proceso(Path.Combine(carpeta, "a"));
            await reabierto.Enviar("jugar\n");
            await reabierto.Esperar("Tenés una partida activa");
            await reabierto.Enviar("retomar\n");
            await reabierto.Esperar("Tu plaza:");
            Assert.DoesNotContain("Nombre opcional", reabierto.Texto);
            reabierto.CerrarEntrada(); b.CerrarEntrada();
            await reabierto.Terminar(); await b.Terminar();
        }
        finally { Directory.Delete(carpeta, true); }
    }

    internal sealed class Proceso : IAsyncDisposable
    {
        private readonly Process proceso;
        private readonly ConcurrentQueue<string> lineas = new();
        private readonly Task salida, errores;
        private readonly CancellationTokenSource plazo = new(TimeSpan.FromSeconds(65));
        internal string Texto => string.Join('\n', lineas);
        internal Proceso(string datos)
        {
            var inicio = new ProcessStartInfo("dotnet") { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
            inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
            inicio.Environment["XDG_DATA_HOME"] = datos;
            proceso = Process.Start(inicio)!;
            salida = Leer(proceso.StandardOutput); errores = Leer(proceso.StandardError);
        }
        private async Task Leer(StreamReader lector)
        { while (await lector.ReadLineAsync(plazo.Token) is { } linea) lineas.Enqueue(linea); }
        internal Task Enviar(string texto) => proceso.StandardInput.WriteAsync(texto);
        internal void CerrarEntrada() => proceso.StandardInput.Close();
        internal async Task Esperar(string texto)
        {
            using var espera = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                while (!Texto.Contains(texto, StringComparison.Ordinal))
                { Assert.False(proceso.HasExited, Texto); await Task.Delay(20, espera.Token); }
            }
            catch (OperationCanceledException) { Assert.Fail($"No apareció {texto}: {Texto}"); }
        }
        internal async Task Terminar()
        {
            await proceso.WaitForExitAsync(plazo.Token); await Task.WhenAll(salida, errores);
            Assert.True(proceso.ExitCode == 0, Texto);
        }
        public async ValueTask DisposeAsync()
        {
            if (!proceso.HasExited) { proceso.Kill(true); await proceso.WaitForExitAsync(); }
            await Task.WhenAll(salida, errores); proceso.Dispose(); plazo.Dispose();
        }
    }
}

internal sealed class ProcesoAutomaticoFactAttribute : FactAttribute
{
    public ProcesoAutomaticoFactAttribute()
    {
        if (OperatingSystem.IsWindows()) Skip = "El aislamiento XDG requiere Unix; comprobar dos usuarios físicos en Windows.";
        else if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")))
            Skip = "Definí GUERRA_CLIENTE_DLL para ejecutar el contrato de procesos.";
    }
}
