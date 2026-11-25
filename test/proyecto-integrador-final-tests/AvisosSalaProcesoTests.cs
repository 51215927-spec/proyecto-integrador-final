using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class AvisosSalaProcesoTests
{
    [ProcesoAutomaticoFact]
    public async Task DosClientesEnSalaNoConsultanEstadoPeriodicamenteDuranteTreintaSegundos()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        var peticiones = new ConcurrentQueue<string>();
        app.Use(async (c, siguiente) => { peticiones.Enqueue(c.Request.Method + " " + c.Request.Path); await siguiente(c); });
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "sala-proceso-" + Guid.NewGuid());
        Directory.CreateDirectory(carpeta);
        try
        {
            await using var a = new InicioAutomaticoProcesoTests.Proceso(Path.Combine(carpeta, "a"));
            await using var b = new InicioAutomaticoProcesoTests.Proceso(Path.Combine(carpeta, "b"));
            await a.Enviar($"conexion\n{app.Urls.Single()}\njugar\n\n");
            await a.Esperar("Buscando oponente");
            await b.Enviar($"conexion\n{app.Urls.Single()}\njugar\n\n");
            await a.Esperar("Sala ·"); await b.Esperar("Sala ·");
            await Task.Delay(500);
            int Consultas() => peticiones.Count(p => p == "GET /api/v2/acceso" || p.EndsWith("/presentacion"));
            int inicial = Consultas();
            await Task.Delay(TimeSpan.FromSeconds(30));
            Assert.Equal(inicial, Consultas());
            a.CerrarEntrada(); b.CerrarEntrada(); await a.Terminar(); await b.Terminar();
        }
        finally { Directory.Delete(carpeta, true); }
    }
}
