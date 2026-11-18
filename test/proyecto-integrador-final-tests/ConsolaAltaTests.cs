using System.Net;
using Proyecto_Integrador_Final.CargaPersonajes;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class ConsolaAltaTests
{
    [Fact]
    public async Task CargaDosPersonajesManualmenteYConfirmaSusIds()
    {
        await using var app = ServidorHttp.Crear([
            "--urls=http://127.0.0.1:0",
            "--ConnectionStrings:Juego=Data Source=:memory:",
            "--Catalogo:Credencial=prueba"
        ]);
        await app.StartAsync();
        using var cliente = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        using var entrada = new StringReader(
            "Áda\n\n2\n*\n**\n249\n250\n350\n450\n550\n650\n750\nsí\n" +
            "Lin\nDescripción\n1\n+\n750\n650\n550\n450\n350\n250\nno\n");
        using var salida = new StringWriter();

        Assert.Equal(0, await ConsolaAlta.Ejecutar(cliente, "prueba", entrada, salida));
        Assert.Contains("ID 1", salida.ToString());
        Assert.Contains("ID 2", salida.ToString());
    }

    [Fact]
    public async Task UnAltaRechazadaNoSeAnunciaComoConfirmada()
    {
        await using var app = ServidorHttp.Crear([
            "--urls=http://127.0.0.1:0",
            "--ConnectionStrings:Juego=Data Source=:memory:",
            "--Catalogo:Credencial=prueba"
        ]);
        await app.StartAsync();
        using var cliente = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        using var entrada = new StringReader("Ada\n\n1\n*\n250\n350\n450\n550\n650\n750\n");
        using var salida = new StringWriter();

        Assert.Equal(1, await ConsolaAlta.Ejecutar(cliente, "incorrecta", entrada, salida));
        Assert.Contains("credencial", salida.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("¿Deseás cargar otro personaje?", salida.ToString());
    }
}
