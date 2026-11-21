using System.Net;
using System.Text;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class TransporteClienteTests
{
    [Fact]
    public async Task ConservaPrefijoSesionYCredencialSinPonerlaEnUrl()
    {
        Guid partida = Guid.NewGuid(), sesion = Guid.NewGuid();
        int llamadas = 0;
        var handler = new Handler(async solicitud =>
        {
            llamadas++;
            Assert.Equal($"/juego/api/v1/partidas/{partida}/participantes/me/sesion", solicitud.RequestUri!.AbsolutePath);
            Assert.Equal("secreto", solicitud.Headers.Authorization!.Parameter);
            Assert.DoesNotContain("secreto", solicitud.RequestUri.ToString());
            string json = await solicitud.Content!.ReadAsStringAsync();
            Assert.Contains(sesion.ToString(), json);
            Assert.Contains("Ana  Pérez", System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("nombre").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"{{\"rol\":\"J2\",\"idSesion\":\"{sesion}\",\"venceUtc\":\"2026-11-21T12:00:00Z\"}}", Encoding.UTF8, "application/json") };
        });
        using var cliente = new ClientePartidaHttp(new(new Uri("http://localhost/juego/"), partida, "secreto", "Ana  Pérez"), sesion, handler);
        Assert.Equal("J2", (await cliente.Incorporar(default)).Rol);
        Assert.Equal(sesion, (await cliente.Incorporar(default)).IdSesion);
        Assert.Equal(2, llamadas);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, "no json", "servicioNoDisponible", true)]
    [InlineData(HttpStatusCode.Redirect, "", "protocoloIncompatible", false)]
    [InlineData(HttpStatusCode.OK, "{", "protocoloIncompatible", false)]
    [InlineData(HttpStatusCode.Conflict, "{\"codigo\":\"sesionOcupada\"}", "sesionOcupada", false)]
    public async Task ClasificaErroresSinExponerCuerpos(HttpStatusCode estado, string cuerpo, string codigo, bool transitorio)
    {
        using var cliente = new ClientePartidaHttp(new(new Uri("http://localhost/"), Guid.NewGuid(), "secreto", "Ana"), Guid.NewGuid(),
            new Handler(_ => Task.FromResult(new HttpResponseMessage(estado) { Content = new StringContent(cuerpo, Encoding.UTF8, "application/json") })));
        var error = await Assert.ThrowsAsync<FalloCliente>(() => cliente.Incorporar(default));
        Assert.Equal(codigo, error.Codigo);
        Assert.Equal(transitorio, error.Transitorio);
        Assert.DoesNotContain("secreto", error.Message);
    }

    internal sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => responder(request);
    }
}
