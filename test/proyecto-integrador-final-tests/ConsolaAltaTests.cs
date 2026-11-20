using System.Net;
using Proyecto_Integrador_Final.CargaPersonajes;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class ConsolaAltaTests
{
    [Fact]
    public async Task Inicio_PideUrlYCredencialAntesDelPersonajeYPermiteCorregirlas()
    {
        await using var app = ServidorHttp.Crear([
            "--urls=http://127.0.0.1:0",
            "--ConnectionStrings:Juego=Data Source=:memory:",
            "--Catalogo:Credencial=secreto-003"
        ]);
        await app.StartAsync();
        using var salida = new StringWriter();
        using var entrada = new StringReader($"ftp://incorrecta\n{app.Urls.Single()}\n\nsecreto-003\nAda\n\n0\n250\n350\n450\n550\n650\n750\nno\n");
        Assert.Equal(0, await ConsolaAlta.Iniciar(entrada, salida));
        string texto = salida.ToString();
        Assert.Contains("ID 1", texto);
        Assert.Contains("http://localhost:5000", texto);
        Assert.True(texto.IndexOf("Credencial", StringComparison.Ordinal) < texto.IndexOf("Nombre", StringComparison.Ordinal));
        Assert.DoesNotContain("secreto-003", texto);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\nclave\n")]
    [InlineData("incorrecta\n")]
    public async Task Inicio_FinDeEntradaSaleSinEnviarPersonaje(string texto)
    {
        using var salida = new StringWriter();
        Assert.Equal(1, await ConsolaAlta.Iniciar(new StringReader(texto), salida));
        Assert.DoesNotContain("Personaje cargado", salida.ToString());
    }

    [Fact]
    public async Task DatosInvalidos_SeCorrigenAntesDeEnviarElAlta()
    {
        using var handler = new AltaCapturada();
        using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        using var entrada = new StringReader("\nAda\n\n1\n\t\n*\n500\n500\n500\n500\n500\n500\n250\n350\n450\n550\n650\n750\nno\n");
        using var salida = new StringWriter();
        Assert.Equal(0, await ConsolaAlta.Ejecutar(cliente, "secreto-003", entrada, salida));
        Assert.Equal(1, handler.Cantidad);
        Assert.Contains("3000", salida.ToString());
        Assert.DoesNotContain("secreto-003", salida.ToString());
    }

    private sealed class AltaCapturada : HttpMessageHandler
    {
        internal int Cantidad { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Cantidad++;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/personajes", request.RequestUri!.AbsolutePath);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("secreto-003", request.Headers.Authorization.Parameter);
            using var json = System.Text.Json.JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal("Ada", json.RootElement.GetProperty("nombre").GetString());
            Assert.Equal("*", json.RootElement.GetProperty("ascii").GetString());
            Assert.Equal(250, json.RootElement.GetProperty("fuerza").GetInt32());
            return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("{\"idPersonaje\":1}") };
        }
    }

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
