using Microsoft.Extensions.DependencyInjection;
using Proyecto_Integrador_Final.Api;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class ReconexionAvisosTests
{
    [Fact]
    public async Task ReiniciarHostResincronizaAvisosYNoReviveBusquedaNiSala()
    {
        var archivo = Path.Combine(Path.GetTempPath(), "reinicio-avisos-" + Guid.NewGuid() + ".db");
        var args = new[] { $"--ConnectionStrings:Juego=Data Source={archivo};Pooling=False", "--urls=http://127.0.0.1:0" };
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            await using var primero = ServidorHttp.Crear(args);
            await primero.StartAsync(limite.Token);
            var url = new Uri(primero.Urls.Single() + "/");
            for (int i = 0; i < 6; i++) primero.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
            async Task<ClienteAccesoHttp> Nuevo()
            {
                var http = new ClienteAccesoHttp(url, await ClienteAccesoHttp.Crear(url, "Invitado", limite.Token));
                await http.Accion("sesion", limite.Token); return http;
            }
            using var a = await Nuevo(); using var b = await Nuevo();
            await a.Accion("jugar", limite.Token); await b.Accion("jugar", limite.Token);
            var partida = (await b.Accion("jugar", limite.Token)).Partida;
            var catalogo = primero.Services.GetRequiredService<CatalogoPersonajes>();
            for (int i = 1; i <= 6; i++) catalogo.Desactivar(i);
            using var c = await Nuevo(); using var d = await Nuevo(); using var e = await Nuevo();
            await c.Accion("jugar", limite.Token); await d.Accion("jugar", limite.Token);
            await e.Accion("jugar", limite.Token);
            var escuchar = a.Escuchar(limite.Token);
            try
            {
                while (!a.Conectado) await Task.Delay(20, limite.Token);
                await primero.StopAsync(limite.Token);
                while (a.Conectado) await Task.Delay(20, limite.Token);
                await using var segundo = ServidorHttp.Crear([args[0], $"--urls={url.ToString().TrimEnd('/')}"]);
                await segundo.StartAsync(limite.Token);
                while (!a.Conectado) await Task.Delay(20, limite.Token);
                Assert.Equal(partida, (await a.Accion(null, limite.Token)).Partida);
                Assert.Equal(partida, (await b.Accion("sesion", limite.Token)).Partida);
                Assert.True(segundo.Services.GetRequiredService<AdministradorSesiones>().AmbosPresentes(partida!.Value));
                foreach (var anterior in new[] { c, d, e })
                {
                    var estado = await anterior.Accion("sesion", limite.Token);
                    Assert.Equal("libre", estado.Estado); Assert.Null(estado.Rival);
                }
                await segundo.StopAsync(limite.Token);
            }
            finally
            {
                limite.Cancel();
                try { await escuchar; } catch (OperationCanceledException) { }
            }
        }
        finally { File.Delete(archivo); }
    }
}
