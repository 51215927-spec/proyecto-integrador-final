using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Proyecto_Integrador_Final.Cliente;

// Los errores del transporte exponen códigos controlados, nunca cuerpos ni credenciales.
internal sealed class FalloCliente(string codigo, bool transitorio = false) : Exception(codigo)
{
    internal string Codigo { get; } = codigo;
    internal bool Transitorio { get; } = transitorio;
}

internal sealed class ClientePartidaHttp : IDisposable
{
    private readonly HttpClient http;
    private readonly ConfiguracionCliente configuracion;
    internal Guid IdSesion { get; }
    // Los DTO siguen camelCase y rechazan números JSON codificados como texto.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict,
    };

    internal ClientePartidaHttp(ConfiguracionCliente configuracion, Guid idSesion, HttpMessageHandler? handler = null)
    {
        this.configuracion = configuracion;
        IdSesion = idSesion;
        // No seguir redirecciones evita enviar las cabeceras de identidad a otro destino.
        http = new(handler ?? new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = configuracion.Url, Timeout = TimeSpan.FromSeconds(3)
        };
    }

    // Incorporación y reintentos conservan el UUID generado para este proceso.
    internal Task<SesionConfirmada> Incorporar(CancellationToken ct) =>
        Enviar<SesionConfirmada>(HttpMethod.Put, "participantes/me/sesion", new IncorporacionCliente(IdSesion, configuracion.Nombre), ct);
    internal Task Renovar(CancellationToken ct) => Enviar<object>(HttpMethod.Put, "participantes/me/presencia", null, ct);
    internal Task<PresentacionCliente> Presentacion(CancellationToken ct) => Enviar<PresentacionCliente>(HttpMethod.Get, "presentacion", null, ct);
    internal Task<ReciboCliente> Elegir(EleccionCliente eleccion, CancellationToken ct) => Enviar<ReciboCliente>(HttpMethod.Post, "elecciones", eleccion, ct);
    // Consultar el recibo permite resolver una respuesta perdida antes de reenviar la elección.
    internal Task<ReciboCliente> Recibo(Guid id, CancellationToken ct) => Enviar<ReciboCliente>(HttpMethod.Get, $"elecciones/{id}", null, ct);
    internal Task Abandonar(CancellationToken ct) => Enviar<object>(HttpMethod.Put, "participantes/me/abandono", null, ct);

    private async Task<T> Enviar<T>(HttpMethod metodo, string ruta, object? cuerpo, CancellationToken ct)
    {
        // La ruta es relativa para conservar el prefijo configurado y la identidad viaja en cabeceras.
        using var peticion = new HttpRequestMessage(metodo, $"api/v1/partidas/{configuracion.Partida}/{ruta}");
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuracion.Credencial);
        peticion.Headers.Add("X-Sesion", IdSesion.ToString());
        peticion.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };
        if (cuerpo is not null) peticion.Content = JsonContent.Create(cuerpo, options: Json);
        try
        {
            using var respuesta = await http.SendAsync(peticion, ct);
            if ((int)respuesta.StatusCode >= 500) throw new FalloCliente("servicioNoDisponible", true);
            // Los fallos de disponibilidad se recuperan automáticamente; los demás conservan su código.
            if (!respuesta.IsSuccessStatusCode)
            {
                if ((int)respuesta.StatusCode is >= 300 and < 400) throw new FalloCliente("protocoloIncompatible");
                var error = await respuesta.Content.ReadFromJsonAsync<ErrorCliente>(Json, ct);
                string[] conocidos = ["entradaInvalida", "credencialInvalida", "sinPermiso", "recursoNoEncontrado", "sesionOcupada", "sesionVencida", "partidaPausada", "revisionDesactualizada", "solicitudNoVigente", "operacionIncompatible", "eleccionInvalida"];
                // Un código desconocido no se interpreta como un rechazo conocido ni como aceptación.
                if (error is null || !conocidos.Contains(error.Codigo)) throw new FalloCliente("protocoloIncompatible");
                throw new FalloCliente(error.Codigo);
            }
            // El contrato exige 204 sin cuerpo o 200/201 con el DTO esperado para esa operación.
            bool exitoEsperado = typeof(T) == typeof(object) ? respuesta.StatusCode == HttpStatusCode.NoContent
                : respuesta.StatusCode == HttpStatusCode.OK || metodo == HttpMethod.Post && respuesta.StatusCode == HttpStatusCode.Created;
            if (!exitoEsperado) throw new FalloCliente("protocoloIncompatible");
            if (typeof(T) == typeof(object)) return default!;
            return await respuesta.Content.ReadFromJsonAsync<T>(Json, ct) ?? throw new FalloCliente("protocoloIncompatible");
        }
        // JSON incompatible requiere intervención; transporte y timeout permiten reintentos automáticos.
        catch (JsonException) { throw new FalloCliente("protocoloIncompatible"); }
        catch (HttpRequestException) { throw new FalloCliente("sinComunicacion", true); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new FalloCliente("tiempoAgotado", true); }
    }

    public void Dispose() => http.Dispose();
}
