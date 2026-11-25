using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ClienteProcesoRestTests
{
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
