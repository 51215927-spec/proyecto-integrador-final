using System.Net;
using System.Text;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class TransporteClienteTests
{
    [Fact]
    public async Task ConservaPrefijoSesionYCredencialSinPonerlaEnUrl()
    {
        // La inspección del transporte verifica identidad y rutas sin depender de un servidor.
        Guid partida = Guid.NewGuid(), sesion = Guid.NewGuid();
        int llamadas = 0;
        var handler = new Handler(async solicitud =>
        {
            llamadas++;
            // El prefijo de despliegue se conserva y la credencial se envía en la cabecera.
            Assert.Equal($"/juego/api/v1/partidas/{partida}/participantes/me/sesion", solicitud.RequestUri!.AbsolutePath);
            Assert.Equal("secreto", solicitud.Headers.Authorization!.Parameter);
            Assert.DoesNotContain("secreto", solicitud.RequestUri.ToString());
            string json = await solicitud.Content!.ReadAsStringAsync();
            Assert.Contains(sesion.ToString(), json);
            Assert.Contains("Ana  Pérez", System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("nombre").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"{{\"rol\":\"J2\",\"idSesion\":\"{sesion}\",\"venceUtc\":\"2026-11-21T12:00:00Z\"}}", Encoding.UTF8, "application/json") };
        });
        // Repetir la incorporación mantiene el UUID de sesión y permite confirmar el mismo rol.
        using var cliente = new ClientePartidaHttp(new(new Uri("http://localhost/juego/"), partida, "secreto", "Ana  Pérez"), sesion, handler);
        Assert.Equal("J2", (await cliente.Incorporar(default)).Rol);
        Assert.Equal(sesion, (await cliente.Incorporar(default)).IdSesion);
        Assert.Equal(2, llamadas);
    }

    // Los fallos de servicio, protocolo y sesión requieren decisiones de recuperación distintas.
    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, "no json", "servicioNoDisponible", true)]
    [InlineData(HttpStatusCode.Redirect, "", "protocoloIncompatible", false)]
    [InlineData(HttpStatusCode.OK, "{", "protocoloIncompatible", false)]
    [InlineData(HttpStatusCode.Conflict, "{\"codigo\":\"sesionOcupada\"}", "sesionOcupada", false)]
    public async Task ClasificaErroresSinExponerCuerpos(HttpStatusCode estado, string cuerpo, string codigo, bool transitorio)
    {
        using var cliente = new ClientePartidaHttp(new(new Uri("http://localhost/"), Guid.NewGuid(), "secreto", "Ana"), Guid.NewGuid(),
            new Handler(_ => Task.FromResult(new HttpResponseMessage(estado) { Content = new StringContent(cuerpo, Encoding.UTF8, "application/json") })));
        // La clasificación expone el código conocido, sin copiar secretos al mensaje de error.
        var error = await Assert.ThrowsAsync<FalloCliente>(() => cliente.Incorporar(default));
        Assert.Equal(codigo, error.Codigo);
        Assert.Equal(transitorio, error.Transitorio);
        Assert.DoesNotContain("secreto", error.Message);
    }

    // El doble permite inspeccionar o rechazar solicitudes antes de usar la red.
    internal sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => responder(request);
    }
}
