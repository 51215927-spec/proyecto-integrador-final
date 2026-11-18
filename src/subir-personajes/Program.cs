using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Proyecto_Integrador_Final.CargaPersonajes;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 1 ||
            !Uri.TryCreate(args.FirstOrDefault() ?? "http://localhost:5000", UriKind.Absolute, out var servidor) ||
            servidor.Scheme is not ("http" or "https"))
        {
            Console.Error.WriteLine("Uso: dotnet run --project src/subir-personajes -- [URL del servidor]");
            return 1;
        }

        string? credencial = Environment.GetEnvironmentVariable("Catalogo__Credencial");
        if (string.IsNullOrWhiteSpace(credencial))
        {
            Console.Error.WriteLine("Configurá Catalogo__Credencial con la credencial de alta del servidor.");
            return 1;
        }

        using var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = servidor
        };
        return await ConsolaAlta.Ejecutar(cliente, credencial, Console.In, Console.Out);
    }
}

internal static class ConsolaAlta
{
    internal static async Task<int> Ejecutar(HttpClient cliente, string credencial, TextReader entrada, TextWriter salida)
    {
        string PedirTexto(string etiqueta)
        {
            salida.WriteLine(etiqueta);
            return entrada.ReadLine() ?? throw new EndOfStreamException();
        }

        int PedirEntero(string etiqueta, int minimo, int maximo)
        {
            while (true)
            {
                if (int.TryParse(PedirTexto($"{etiqueta} ({minimo} a {maximo})"), out int valor) &&
                    valor >= minimo && valor <= maximo)
                    return valor;
                salida.WriteLine("Ingresá un entero dentro del intervalo.");
            }
        }

        try
        {
            while (true)
            {
                string nombre = PedirTexto("Nombre");
                string descripcion = PedirTexto("Descripción (opcional)");
                var lineas = new string[PedirEntero("Cantidad de líneas de arte ASCII", 0, 25)];
                for (int i = 0; i < lineas.Length; i++)
                    lineas[i] = PedirTexto($"Línea {i + 1} de arte ASCII");

                var datos = new
                {
                    nombre,
                    descripcion,
                    ascii = string.Join('\n', lineas),
                    fuerza = PedirEntero("Fuerza", 250, 750),
                    velocidad = PedirEntero("Velocidad", 250, 750),
                    resistencia = PedirEntero("Resistencia", 250, 750),
                    inteligencia = PedirEntero("Inteligencia", 250, 750),
                    energia = PedirEntero("Energía", 250, 750),
                    habilidad = PedirEntero("Habilidad", 250, 750)
                };

                using var solicitud = new HttpRequestMessage(HttpMethod.Post, "/api/v1/personajes");
                solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credencial);
                solicitud.Content = JsonContent.Create(datos);
                using var respuesta = await cliente.SendAsync(solicitud);
                if (respuesta.StatusCode != HttpStatusCode.Created)
                {
                    salida.WriteLine(respuesta.StatusCode switch
                    {
                        HttpStatusCode.BadRequest => "El servidor rechazó los datos del personaje.",
                        HttpStatusCode.Unauthorized => "La credencial de catálogo no es válida.",
                        _ => $"El servidor rechazó el alta (HTTP {(int)respuesta.StatusCode})."
                    });
                    return 1;
                }

                var alta = await respuesta.Content.ReadFromJsonAsync<AltaConfirmada>();
                if (alta is null || alta.IdPersonaje <= 0)
                {
                    salida.WriteLine("El servidor no devolvió un ID de personaje válido.");
                    return 1;
                }

                salida.WriteLine($"Personaje cargado. ID {alta.IdPersonaje}");
                salida.WriteLine("¿Deseás cargar otro personaje? (sí/no)");
                string? continuar = entrada.ReadLine()?.Trim();
                while (continuar is not null && !continuar.Equals("no", StringComparison.OrdinalIgnoreCase) &&
                    !EsSi(continuar))
                {
                    salida.WriteLine("Respondé sí o no.");
                    continuar = entrada.ReadLine()?.Trim();
                }
                if (continuar is null || !EsSi(continuar))
                    return 0;
            }
        }
        catch (EndOfStreamException)
        {
            salida.WriteLine("La entrada terminó antes de completar el personaje.");
            return 1;
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or JsonException)
        {
            salida.WriteLine("No se pudo confirmar el alta con el servidor.");
            return 1;
        }
    }

    private static bool EsSi(string respuesta) =>
        respuesta.Equals("sí", StringComparison.OrdinalIgnoreCase) ||
        respuesta.Equals("si", StringComparison.OrdinalIgnoreCase);

    private sealed record AltaConfirmada(int IdPersonaje);
}
