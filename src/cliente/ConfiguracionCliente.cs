using System.Text;

namespace Proyecto_Integrador_Final.Cliente;

internal sealed record ConfiguracionCliente(Uri Url, Guid Partida, string Credencial, string Nombre)
{
    internal static ConfiguracionCliente Leer(TextReader entrada, TextWriter salida) =>
        LeerAsync(entrada, salida, default).GetAwaiter().GetResult();

    internal static async Task<ConfiguracionCliente> LeerAsync(TextReader entrada, TextWriter salida, CancellationToken ct)
    {
        async Task<string> Campo(string pregunta, Func<string, bool> valida, string error, bool secreto = false)
        {
            while (true)
            {
                salida.WriteLine(pregunta);
                string valor;
                if (ReferenceEquals(entrada, Console.In) && !Console.IsInputRedirected)
                {
                    var texto = new StringBuilder();
                    while (true)
                    {
                        while (!Console.KeyAvailable) await Task.Delay(25, ct);
                        var tecla = Console.ReadKey(true);
                        if (tecla.Key == ConsoleKey.Enter) { salida.WriteLine(); break; }
                        if (tecla.KeyChar is '\u0004' or '\u001a') throw new EndOfStreamException();
                        if (tecla.Key == ConsoleKey.Backspace)
                        {
                            if (texto.Length > 0) { texto.Length--; if (!secreto) salida.Write("\b \b"); }
                        }
                        else if (!char.IsControl(tecla.KeyChar)) { texto.Append(tecla.KeyChar); if (!secreto) salida.Write(tecla.KeyChar); }
                    }
                    valor = texto.ToString().Trim();
                }
                else valor = (await entrada.ReadLineAsync(ct) ?? throw new EndOfStreamException()).Trim();
                if (valida(valor)) return valor;
                salida.WriteLine(error);
            }
        }
        string url = await Campo("URL base HTTP/HTTPS del servidor", EsUrl,
            "Ingresá una URL absoluta HTTP/HTTPS sin credenciales, consulta ni fragmento.");
        string id = await Campo("UUID de partida", v => Guid.TryParse(v, out var g) && g != Guid.Empty,
            "Ingresá un UUID distinto de vacío.");
        string credencial = await Campo("Credencial de plaza", v => !string.IsNullOrWhiteSpace(v) && !v.Any(char.IsControl),
            "Ingresá una credencial no vacía y sin controles.", true);
        string nombre = await Campo("Nombre registrado (debe coincidir exactamente al retomar)",
            v => !string.IsNullOrWhiteSpace(v), "Ingresá un nombre no vacío.");
        return new(new Uri(url.TrimEnd('/') + "/"), Guid.Parse(id), credencial, nombre);
    }

    internal static bool EsUrl(string valor) => Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https" && uri.Host.Length > 0 && uri.UserInfo.Length == 0 &&
        uri.Query.Length == 0 && uri.Fragment.Length == 0;

    public override string ToString() => $"Partida {Partida}";
}
