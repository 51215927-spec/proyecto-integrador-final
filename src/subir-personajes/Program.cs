using Proyecto_Integrador_Final.Consola;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Proyecto_Integrador_Final.CargaPersonajes;

internal static class Program
{
    private static Task<int> Main(string[] args) => ConsolaAlta.Iniciar(Console.In, Console.Out, args);
}

internal static class ConsolaAlta
{
    internal static async Task<int> Iniciar(TextReader entrada, TextWriter salida, string[]? args = null)
    {
        try
        {
            var datos = await Leer(args?.FirstOrDefault(), entrada, salida);
            var lector = new EntradaConsola(entrada, salida);
            var servidor = new Uri(lector.Url("http://localhost:5000"));
            string credencial = lector.Secreto("Credencial de catálogo", permiteVacio: false);
            using var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = servidor };
            return await Enviar(cliente, credencial, entrada, salida, datos);
        }
        catch (Exception error) when (EsErrorEntrada(error))
        {
            salida.WriteLine($"No se pudo cargar el archivo: {error.Message}");
            return 1;
        }
    }

    internal static async Task<int> Ejecutar(HttpClient cliente, string credencial, TextReader entrada, TextWriter salida, string? ruta = null)
    {
        try { return await Enviar(cliente, credencial, entrada, salida, await Leer(ruta, entrada, salida)); }
        catch (Exception error) when (EsErrorEntrada(error))
        {
            salida.WriteLine($"No se pudo cargar el archivo: {error.Message}");
            return 1;
        }
    }

    private static bool EsErrorEntrada(Exception error) => error is IOException or UnauthorizedAccessException or ArgumentException or JsonException or FormatException;

    private static async Task<JsonElement> Leer(string? ruta, TextReader entrada, TextWriter salida)
    {
        if (ruta is null)
        {
            salida.WriteLine("Ruta del archivo JSON:");
            ruta = entrada.ReadLine();
        }
        if (string.IsNullOrWhiteSpace(ruta)) throw new FormatException("Falta la ruta del archivo JSON.");
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(ruta));
        ValidarFormato(json.RootElement);
        return json.RootElement.Clone();
    }

    private static void ValidarFormato(JsonElement raiz)
    {
        if (raiz.ValueKind != JsonValueKind.Array || raiz.GetArrayLength() == 0)
            throw new FormatException("La raíz debe ser una lista con al menos un personaje.");
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int indice = 0;
        foreach (var objeto in raiz.EnumerateArray())
        {
            string ubicacion = $"Personaje {++indice}";
            if (objeto.ValueKind != JsonValueKind.Object) throw new FormatException($"{ubicacion}: se esperaba un objeto.");
            if (objeto.TryGetProperty("nombre", out var nombre) && nombre.ValueKind == JsonValueKind.String)
                ubicacion += $" ({nombre.GetString()})";
            foreach (string campo in new[] { "nombre", "descripcion", "ascii", "fuerza", "velocidad", "resistencia", "inteligencia", "energia", "habilidad" })
            {
                bool texto = campo is "nombre" or "descripcion" or "ascii";
                if (!objeto.TryGetProperty(campo, out var valor) || (texto ? valor.ValueKind != JsonValueKind.String : valor.ValueKind != JsonValueKind.Number || !valor.TryGetInt32(out _)))
                    throw new FormatException($"{ubicacion}: {campo} es obligatorio y debe ser {(texto ? "texto" : "entero")}.");
            }
            if (objeto.EnumerateObject().Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("idPersonaje", StringComparison.OrdinalIgnoreCase)))
                throw new FormatException($"{ubicacion}: el ID lo asigna el servidor.");
            if (!nombres.Add(nombre.GetString()!.Trim())) throw new FormatException($"{ubicacion}: nombre duplicado.");
        }
    }

    private static async Task<int> Enviar(HttpClient cliente, string credencial, TextReader entrada, TextWriter salida, JsonElement datos)
    {
        bool enviado = false;
        try
        {
            using var resumen = new HttpRequestMessage(HttpMethod.Get, "/api/v1/personajes/resumen");
            resumen.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credencial);
            using var consulta = await cliente.SendAsync(resumen);
            if (!consulta.IsSuccessStatusCode) return await Rechazo(consulta, salida);
            var actual = await consulta.Content.ReadFromJsonAsync<Resumen>();
            if (actual?.Cantidad is null or < 0) throw new JsonException("Resumen inválido.");
            salida.WriteLine($"Catálogo actual: {actual.Cantidad}. Personajes del JSON: {datos.GetArrayLength()}.");
            while (true)
            {
                salida.WriteLine("¿Reemplazar el catálogo? (sí/no)");
                string? respuesta = entrada.ReadLine()?.Trim();
                if (respuesta is null || respuesta.Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    salida.WriteLine("Carga cancelada. Catálogo sin cambios.");
                    return 0;
                }
                if (respuesta.Equals("sí", StringComparison.OrdinalIgnoreCase) || respuesta.Equals("si", StringComparison.OrdinalIgnoreCase)) break;
                salida.WriteLine("Respondé sí o no.");
            }
            using var solicitud = new HttpRequestMessage(HttpMethod.Put, "/api/v1/personajes");
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credencial);
            solicitud.Content = JsonContent.Create(datos);
            enviado = true;
            using var respuestaCarga = await cliente.SendAsync(solicitud);
            if (!respuestaCarga.IsSuccessStatusCode) return await Rechazo(respuestaCarga, salida);
            var resultado = await respuestaCarga.Content.ReadFromJsonAsync<Resumen>();
            if (resultado?.Cantidad != datos.GetArrayLength()) throw new JsonException("Confirmación inválida.");
            salida.WriteLine($"Catálogo reemplazado. Personajes cargados: {resultado.Cantidad}.");
            return 0;
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or JsonException or IOException)
        {
            salida.WriteLine(enviado ? "No se pudo confirmar el resultado de la carga. El catálogo puede haber sido reemplazado. No se reintentará automáticamente." : "No se pudo consultar el catálogo del servidor. No se envió el reemplazo.");
            return 1;
        }
    }

    private static async Task<int> Rechazo(HttpResponseMessage respuesta, TextWriter salida)
    {
        string causa = respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized ? "La credencial de catálogo no es válida." : "El servidor rechazó la operación.";
        try
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            if (json.RootElement.TryGetProperty("mensaje", out var mensaje)) causa = mensaje.GetString() ?? causa;
        }
        catch (JsonException) { }
        salida.WriteLine($"HTTP {(int)respuesta.StatusCode}: {causa}");
        return 1;
    }

    private sealed record Resumen(int? Cantidad);
}
