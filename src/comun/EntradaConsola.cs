using System.Text;

namespace Proyecto_Integrador_Final.Consola;

internal sealed class EntradaConsola(TextReader entrada, TextWriter salida)
{
    internal string Texto(string pregunta)
    {
        salida.WriteLine(pregunta);
        return entrada.ReadLine() ?? throw new EndOfStreamException();
    }

    internal string Validar(string pregunta, Func<string, bool> valida, string error, string? defecto = null)
    {
        while (true)
        {
            string valor = Texto(pregunta).Trim();
            if (valor.Length == 0 && defecto is not null)
                valor = defecto;
            if (valida(valor))
                return valor;
            salida.WriteLine(error);
        }
    }

    internal string Url(string defecto) => Validar($"Dirección HTTP (Enter: {defecto})",
        EsUrl, "Ingresá una dirección HTTP o HTTPS válida, sin credenciales, consulta ni fragmento.", defecto);

    private static bool EsUrl(string valor) =>
        Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https" && !string.IsNullOrEmpty(uri.Host) &&
        uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0 &&
        uri.AbsolutePath == "/";

    internal string Secreto(string pregunta, bool permiteVacio)
    {
        while (true)
        {
            salida.WriteLine(pregunta);
            string valor = ReferenceEquals(entrada, Console.In) && !Console.IsInputRedirected
                ? LeerSecreto() : entrada.ReadLine() ?? throw new EndOfStreamException();
            if ((permiteVacio && string.IsNullOrWhiteSpace(valor)) ||
                (!string.IsNullOrWhiteSpace(valor) && !valor.Any(char.IsControl) && !valor.Any(char.IsWhiteSpace)))
                return string.IsNullOrWhiteSpace(valor) ? "" : valor;
            salida.WriteLine("Ingresá una credencial no vacía, sin espacios ni caracteres de control.");
        }
    }

    private static string LeerSecreto()
    {
        var texto = new StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return texto.ToString();
            }
            if (tecla.KeyChar is '\u0004' or '\u001a')
                throw new EndOfStreamException();
            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (texto.Length > 0) texto.Length--;
            }
            else if (!char.IsControl(tecla.KeyChar))
                texto.Append(tecla.KeyChar);
        }
    }
}
