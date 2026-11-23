using System.Net.Http.Json;
using System.Text.Json;

namespace Proyecto_Integrador_Final.Cliente;

internal sealed record DisponibilidadCliente(bool Disponible);
internal sealed record EstadoAccesoCliente(string Estado, string Nombre, bool Listo, string? Rival,
    bool RivalListo, string? Aviso, Guid? Partida);
internal sealed record AltaAccesoCliente(Guid Id, string Secreto)
{
    public override string ToString() => "Alta de invitado";
}

internal sealed class ClienteAccesoHttp : IDisposable
{
    private readonly HttpClient http;
    internal Guid Sesion { get; } = Guid.NewGuid();
    internal Uri Servidor { get; }
    internal InvitadoLocal Invitado { get; }
    private long version;
    private volatile bool conectado;
    internal long Version => Interlocked.Read(ref version);
    internal bool Conectado => conectado;

    internal ClienteAccesoHttp(Uri servidor, InvitadoLocal invitado)
    {
        Servidor = servidor; Invitado = invitado;
        http = new(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = servidor, Timeout = Timeout.InfiniteTimeSpan };
        http.DefaultRequestHeaders.Authorization = new("Bearer", invitado.Secreto);
        http.DefaultRequestHeaders.Add("X-Invitado", invitado.Id.ToString());
        http.DefaultRequestHeaders.Add("X-Sesion", Sesion.ToString());
    }
    internal static async Task<InvitadoLocal> Crear(Uri servidor, string nombre, CancellationToken ct)
    {
        using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = servidor, Timeout = TimeSpan.FromSeconds(3) };
        try
        {
            using var respuesta = await http.PostAsJsonAsync("api/v2/invitados", new { nombre }, ct);
            await Comprobar(respuesta, ct);
            var acceso = await respuesta.Content.ReadFromJsonAsync<AltaAccesoCliente>(ct) ?? throw new FalloCliente("protocoloIncompatible");
            if (acceso.Id == Guid.Empty || acceso.Secreto is not { Length: 64 }) throw new FalloCliente("protocoloIncompatible");
            return new(acceso.Id, acceso.Secreto, nombre);
        }
        catch (HttpRequestException) { throw new FalloCliente("sinComunicacion", true); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new FalloCliente("sinComunicacion", true); }
        catch (JsonException) { throw new FalloCliente("protocoloIncompatible"); }
    }
    internal async Task<bool> Disponible(CancellationToken ct) =>
        (await Enviar<DisponibilidadCliente>("disponibilidad", ct)).Disponible;
    internal Task<EstadoAccesoCliente> Accion(string? accion, CancellationToken ct) => Enviar<EstadoAccesoCliente>(accion, ct);
    internal Task Renovar(CancellationToken ct) => Enviar<object>("presencia", ct);
    private async Task<T> Enviar<T>(string? accion, CancellationToken ct)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(TimeSpan.FromSeconds(3));
        try
        {
            using var respuesta = accion is null ? await http.GetAsync("api/v2/acceso", limite.Token)
                : await http.PutAsJsonAsync($"api/v2/acceso/{accion}", new { }, limite.Token);
            await Comprobar(respuesta, limite.Token);
            if (typeof(T) == typeof(object))
            {
                if (respuesta.StatusCode != System.Net.HttpStatusCode.NoContent) throw new FalloCliente("protocoloIncompatible");
                return default!;
            }
            return await respuesta.Content.ReadFromJsonAsync<T>(limite.Token) ?? throw new FalloCliente("protocoloIncompatible");
        }
        catch (HttpRequestException) { throw new FalloCliente("sinComunicacion", true); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new FalloCliente("sinComunicacion", true); }
        catch (JsonException) { throw new FalloCliente("protocoloIncompatible"); }
    }
    private static async Task Comprobar(HttpResponseMessage respuesta, CancellationToken ct)
    {
        if (respuesta.IsSuccessStatusCode) return;
        if ((int)respuesta.StatusCode >= 500) throw new FalloCliente("servicioNoDisponible", true);
        string[] conocidos = ["invitadoInvalido", "sesionOcupada", "sesionVencida", "operacionIncompatible", "entradaInvalida"];
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorCliente>(ct);
        throw new FalloCliente(error is not null && conocidos.Contains(error.Codigo) ? error.Codigo : "protocoloIncompatible");
    }
    internal async Task Escuchar(CancellationToken ct)
    {
        int fallos = 0;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Accion("sesion", ct);
                using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
                limite.CancelAfter(TimeSpan.FromSeconds(5));
                using var respuesta = await http.GetAsync("api/v2/avisos", HttpCompletionOption.ResponseHeadersRead, limite.Token);
                await Comprobar(respuesta, limite.Token);
                limite.CancelAfter(Timeout.InfiniteTimeSpan);
                using var lector = new StreamReader(await respuesta.Content.ReadAsStreamAsync(ct));
                conectado = true; Interlocked.Increment(ref version); fallos = 0;
                while (true)
                {
                    using var silencio = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    silencio.CancelAfter(TimeSpan.FromSeconds(12));
                    var linea = await lector.ReadLineAsync(silencio.Token);
                    if (linea is null) break;
                    if (linea == "data: cambio") Interlocked.Increment(ref version);
                }
            }
            catch (Exception error) when (error is HttpRequestException or IOException or FalloCliente or OperationCanceledException)
            {
                if (ct.IsCancellationRequested) break;
                // El menú informa rechazos definitivos mediante sus acciones, sin imprimir cuerpos del servidor.
            }
            conectado = false; Interlocked.Increment(ref version);
            await Task.Delay(TimeSpan.FromSeconds(++fallos switch { 1 => 1, 2 => 2, _ => 5 }), ct);
        }
        conectado = false;
    }
    public void Dispose() => http.Dispose();
}
