using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class InicioAutomaticoHttpTests
{
    [Fact]
    public async Task EmiteIdentidadSinNombreYLaAutenticaSinExponerElSecreto()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        await app.StartAsync();
        using var http = new HttpClient { BaseAddress = new(app.Urls.Single()) };
        using var respuesta = await http.PostAsJsonAsync("/api/v2/invitados", new { nombre = (string?)null });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var acceso = (await respuesta.Content.ReadFromJsonAsync<JsonElement>());
        var secreto = acceso.GetProperty("secreto").GetString()!;
        Assert.True(secreto.Length >= 64);
        http.DefaultRequestHeaders.Authorization = new("Bearer", secreto);
        http.DefaultRequestHeaders.Add("X-Invitado", acceso.GetProperty("id").GetString());
        http.DefaultRequestHeaders.Add("X-Sesion", Guid.NewGuid().ToString());
        using var sesion = await http.PutAsJsonAsync("/api/v2/acceso/sesion", new { });
        Assert.Equal(HttpStatusCode.OK, sesion.StatusCode);
        var texto = await sesion.Content.ReadAsStringAsync();
        Assert.DoesNotContain(secreto, texto);
        Assert.Contains("Invitado", texto);
        using var presencia = await http.PutAsJsonAsync("api/v2/acceso/presencia", new { });
        Assert.Equal(HttpStatusCode.NoContent, presencia.StatusCode);
        Assert.Empty(await presencia.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ColaFifoIniciaAlEmparejarYReintentaCuandoSeCargaElCatalogo()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        await app.StartAsync();
        using var a = await Nuevo(app.Urls.Single());
        using var b = await Nuevo(app.Urls.Single());
        using var c = await Nuevo(app.Urls.Single());
        Assert.Equal("buscando", (await Accion(a, "jugar")).GetProperty("estado").GetString());
        await Accion(b, "jugar");
        Assert.Equal("buscando", (await Accion(c, "jugar")).GetProperty("estado").GetString());
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Accion(a, "jugar")));
        var bloqueada = await Estado(b);
        Assert.Equal("sala", bloqueada.GetProperty("estado").GetString());
        Assert.Equal("catalogoInsuficiente", bloqueada.GetProperty("aviso").GetString());
        Assert.Equal("catalogoInsuficiente", (await Estado(a)).GetProperty("aviso").GetString());
        var catalogo = app.Services.GetRequiredService<CatalogoPersonajes>();
        for (int i = 0; i < 6; i++) catalogo.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
        var iniciada = await Estado(a);
        Assert.Equal("partida", iniciada.GetProperty("estado").GetString());
        var id = iniciada.GetProperty("partida").GetGuid();
        Assert.Equal(id, (await Estado(b)).GetProperty("partida").GetGuid());
        Assert.Equal(id, (await Accion(a, "cancelar")).GetProperty("partida").GetGuid());
        Assert.Equal(id, (await Accion(a, "jugar")).GetProperty("partida").GetGuid());
        Assert.Equal("buscando", (await Estado(c)).GetProperty("estado").GetString());
        using var vista = await a.GetAsync($"/api/v1/partidas/{id}/presentacion");
        Assert.Equal(HttpStatusCode.OK, vista.StatusCode);
        Assert.Contains("esperandoElector", await vista.Content.ReadAsStringAsync());
        using var duplicado = new HttpClient { BaseAddress = a.BaseAddress };
        duplicado.DefaultRequestHeaders.Authorization = a.DefaultRequestHeaders.Authorization;
        duplicado.DefaultRequestHeaders.Add("X-Invitado", a.DefaultRequestHeaders.GetValues("X-Invitado"));
        duplicado.DefaultRequestHeaders.Add("X-Sesion", Guid.NewGuid().ToString());
        using var ocupado = await duplicado.PutAsJsonAsync("api/v2/acceso/sesion", new { });
        Assert.Equal(HttpStatusCode.Conflict, ocupado.StatusCode);
    }

    [Fact]
    public async Task CancelarSalaDevuelveRivalABusquedaSinConfirmacion()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        await app.StartAsync();
        using var a = await Nuevo(app.Urls.Single());
        using var b = await Nuevo(app.Urls.Single());
        await Accion(a, "jugar"); await Accion(b, "jugar");
        Assert.Equal("libre", (await Accion(a, "cancelar")).GetProperty("estado").GetString());
        var rival = await Estado(b);
        Assert.Equal("buscando", rival.GetProperty("estado").GetString());
        Assert.Equal(JsonValueKind.Null, rival.GetProperty("partida").ValueKind);
        using var listo = await b.PutAsJsonAsync("api/v2/acceso/listo", new { });
        Assert.Equal(HttpStatusCode.NotFound, listo.StatusCode);
        Assert.Equal("rivalSalio", rival.GetProperty("aviso").GetString());
    }

    [Fact]
    public async Task AvisosAutenticadosDespiertanAlRivalSinDatosPrivados()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        await app.StartAsync();
        using var a = await Nuevo(app.Urls.Single());
        using var b = await Nuevo(app.Urls.Single());
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var respuesta = await a.GetAsync("api/v2/avisos", HttpCompletionOption.ResponseHeadersRead, limite.Token);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var lector = new StreamReader(await respuesta.Content.ReadAsStreamAsync(limite.Token));
        Assert.Equal("data: cambio", await lector.ReadLineAsync(limite.Token));
        await lector.ReadLineAsync(limite.Token);
        await Accion(a, "jugar");
        await Accion(b, "jugar");
        Assert.Equal("data: cambio", await lector.ReadLineAsync(limite.Token));
        using var anonimo = new HttpClient { BaseAddress = a.BaseAddress };
        using var rechazo = await anonimo.GetAsync("api/v2/avisos", limite.Token);
        Assert.Equal(HttpStatusCode.Unauthorized, rechazo.StatusCode);
    }

    private static async Task<HttpClient> Nuevo(string url)
    {
        var http = new HttpClient { BaseAddress = new(url) };
        using var alta = await http.PostAsJsonAsync("api/v2/invitados", new { });
        alta.EnsureSuccessStatusCode();
        var acceso = await alta.Content.ReadFromJsonAsync<JsonElement>();
        http.DefaultRequestHeaders.Authorization = new("Bearer", acceso.GetProperty("secreto").GetString());
        http.DefaultRequestHeaders.Add("X-Invitado", acceso.GetProperty("id").GetString());
        http.DefaultRequestHeaders.Add("X-Sesion", Guid.NewGuid().ToString());
        await Accion(http, "sesion");
        return http;
    }
    private static async Task<JsonElement> Accion(HttpClient http, string accion)
    {
        using var respuesta = await http.PutAsJsonAsync($"api/v2/acceso/{accion}", new { });
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<JsonElement>();
    }
    private static async Task<JsonElement> Estado(HttpClient http) =>
        await http.GetFromJsonAsync<JsonElement>("api/v2/acceso");
}
