using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ClienteProcesoRestTests
{
    [ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task ProcesoSeIncorporaReabreSinArchivosYEsperaPlazaHastaLiberacion()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
        var partida = juego.CrearPartida();
        await app.StartAsync();
        string directorio = Path.Combine(Path.GetTempPath(), $"guerra-proceso-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            await using (var primera = new Proceso(directorio))
            {
                await primera.Enviar($"recuperar acceso antiguo\n{app.Urls.Single()}\n{partida.Id}\n{partida.CredencialJ1}\nAna\n");
                await primera.Esperar("Esperando inicio");
                Assert.DoesNotContain(partida.CredencialJ1, primera.Texto);
                primera.CerrarEntrada();
                await primera.Terminar();
                Assert.Contains("Salida local", primera.Texto);
            }
            await using (var segunda = new Proceso(directorio))
            {
                await segunda.Enviar($"recuperar acceso antiguo\n{app.Urls.Single()}\n{partida.Id}\n{partida.CredencialJ1}\nAna\n");
                await segunda.Esperar("Sesión ocupada");
                app.Services.GetRequiredService<AdministradorSesiones>().CerrarSesion(partida.Id, "J1");
                await segunda.Esperar("Esperando inicio");
                await segunda.Enviar("esc\ns\n\n");
                await segunda.Terminar();
                Assert.DoesNotContain("Final confirmado", segunda.Texto);
            }
            Assert.Empty(Directory.GetFiles(directorio));
            Assert.Equal("esperandoInicio", juego.Presentacion(partida.Id, partida.CredencialJ2,
                app.Services.GetRequiredService<AdministradorSesiones>().Incorporar(partida.Id, partida.CredencialJ2, new(Guid.NewGuid(), "Ada")).IdSesion).Fase);
            await app.StopAsync();
        }
        finally { Directory.Delete(directorio, true); }
    }

    private sealed class Proceso : IAsyncDisposable
    {
        private readonly Process proceso;
        private readonly ConcurrentQueue<string> lineas = new();
        private readonly Task salida, errores;
        private readonly CancellationTokenSource plazo = new(TimeSpan.FromSeconds(15));
        internal string Texto => string.Join('\n', lineas);
        internal Proceso(string directorio)
        {
            var inicio = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directorio, RedirectStandardInput = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
            proceso = Process.Start(inicio)!;
            salida = Leer(proceso.StandardOutput);
            errores = Leer(proceso.StandardError);
        }
        private async Task Leer(StreamReader lector)
        {
            while (await lector.ReadLineAsync(plazo.Token) is { } linea) lineas.Enqueue(linea);
        }
        internal Task Enviar(string texto) => proceso.StandardInput.WriteAsync(texto);
        internal void CerrarEntrada() => proceso.StandardInput.Close();
        internal async Task Esperar(string texto)
        {
            while (!Texto.Contains(texto, StringComparison.Ordinal))
            {
                Assert.False(proceso.HasExited, Texto);
                await Task.Delay(20, plazo.Token);
            }
        }
        internal async Task Terminar()
        {
            await proceso.WaitForExitAsync(plazo.Token);
            await Task.WhenAll(salida, errores);
            Assert.True(proceso.ExitCode == 0, Texto);
        }
        public async ValueTask DisposeAsync()
        {
            if (!proceso.HasExited) { proceso.Kill(true); await proceso.WaitForExitAsync(); }
            await Task.WhenAll(salida, errores);
            proceso.Dispose(); plazo.Dispose();
        }
    }
}
