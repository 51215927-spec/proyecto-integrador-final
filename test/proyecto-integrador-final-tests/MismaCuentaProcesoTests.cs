using Microsoft.Extensions.DependencyInjection;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class MismaCuentaProcesoTests
{
    [ProcesoAutomaticoFact]
    public async Task DosTerminalesCompartenCuentaCreanPerfilesJueganYReutilizanElUnicoLibre()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        for (int i = 0; i < 6; i++) app.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
        await app.StartAsync();
        var datos = Path.Combine(Path.GetTempPath(), "misma-cuenta-" + Guid.NewGuid());
        Directory.CreateDirectory(datos);
        try
        {
            await using var a = new InicioAutomaticoProcesoTests.Proceso(datos);
            await a.Enviar($"conexion\n{app.Urls.Single()}\njugar\nAna\n");
            await a.Esperar("Buscando oponente");
            await using var b = new InicioAutomaticoProcesoTests.Proceso(datos);
            await b.Enviar("jugar\n"); await b.Esperar("Todos los perfiles están en uso");
            await b.Enviar("crear perfil\nBruno\n");
            await a.Esperar("Sala ·"); await b.Esperar("Sala ·");
            await a.Enviar("listo\n"); await b.Enviar("listo\n");
            await a.Esperar("Tu plaza:"); await b.Esperar("Tu plaza:");
            a.CerrarEntrada(); await a.Terminar();
            await using var regreso = new InicioAutomaticoProcesoTests.Proceso(datos);
            await regreso.Enviar("jugar\n"); await regreso.Esperar("Tenés una partida activa");
            Assert.Contains("Perfil: Ana", regreso.Texto);
            Assert.DoesNotContain("Elegí un perfil", regreso.Texto);
            Assert.DoesNotContain("Nombre opcional", regreso.Texto);
            await regreso.Enviar("retomar\n"); await regreso.Esperar("Tu plaza:");
            regreso.CerrarEntrada(); b.CerrarEntrada(); await regreso.Terminar(); await b.Terminar();
        }
        finally { Directory.Delete(datos, true); }
    }
}
