using Proyecto_Integrador_Final.Consola;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Proyecto_Integrador_Final.CargaPersonajes;

internal static class Program
{
    private static Task<int> Main() => ConsolaAlta.Iniciar(Console.In, Console.Out);
}

internal static class ConsolaAlta
{
    internal static async Task<int> Iniciar(TextReader entrada, TextWriter salida)
    {
        try
        {
            var lector = new EntradaConsola(entrada, salida);
            var servidor = new Uri(lector.Url("http://localhost:5000"));
            string credencial = lector.Secreto("Credencial de catálogo", permiteVacio: false);
            using var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = servidor
            };
            return await Ejecutar(cliente, credencial, entrada, salida);
        }
        catch (EndOfStreamException)
        {
            salida.WriteLine("La entrada terminó antes de completar la conexión.");
            return 1;
        }
        catch (IOException)
        {
            return 1;
        }
    }

    internal static async Task<int> Ejecutar(HttpClient cliente, string credencial, TextReader entrada, TextWriter salida)
    {
        var lector = new EntradaConsola(entrada, salida);
        string PedirTexto(string etiqueta) => lector.Texto(etiqueta);

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
                string nombre = lector.Validar("Nombre", v => !string.IsNullOrWhiteSpace(v), "El nombre no puede estar vacío.");
                string descripcion = PedirTexto("Descripción (opcional)");
                var lineas = new string[PedirEntero("Cantidad de líneas de arte ASCII", 0, 25)];
                for (int i = 0; i < lineas.Length; i++)
                {
                    while (true)
                    {
                        string linea = PedirTexto($"Línea {i + 1} de arte ASCII");
                        if (linea.Length <= 120 && !linea.Contains('\t'))
                        {
                            lineas[i] = linea;
                            break;
                        }
                        salida.WriteLine("Usá hasta 120 caracteres sin tabulaciones.");
                    }
                }

                string[] etiquetas = ["Fuerza", "Velocidad", "Resistencia", "Inteligencia", "Energía", "Habilidad"];
                int[] atributos;
                while (true)
                {
                    atributos = etiquetas.Select(e => PedirEntero(e, 250, 750)).ToArray();
                    if (atributos.Sum() == 3000 && atributos.Max() >= 650 && atributos.Min() <= 350)
                        break;
                    salida.WriteLine("Los seis atributos deben sumar 3000, incluir uno de al menos 650 y uno de hasta 350. Volvé a ingresar los seis valores.");
                }

                var datos = new
                {
                    nombre,
                    descripcion,
                    ascii = string.Join('\n', lineas),
                    fuerza = atributos[0],
                    velocidad = atributos[1],
                    resistencia = atributos[2],
                    inteligencia = atributos[3],
                    energia = atributos[4],
                    habilidad = atributos[5]
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

                // El alta se confirma cuando la respuesta incluye el identificador asignado por el servidor.
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
