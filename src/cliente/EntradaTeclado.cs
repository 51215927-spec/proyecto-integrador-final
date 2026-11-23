using System.Globalization;
using System.Text;

namespace Proyecto_Integrador_Final.Cliente;

internal static class EntradaTeclado
{
    internal static int IndiceOpcion(string texto, IReadOnlyList<string> opciones)
    {
        string buscada = Normalizar(texto);
        if (buscada.Length == 0) return -1;
        for (int i = 0; i < opciones.Count; i++)
            if (EsIgual(texto, opciones[i])) return i;
        return int.TryParse(texto.Trim(), out int numero) && numero > 0 && numero <= opciones.Count
            ? numero - 1 : -1;
    }

    internal static bool EsIgual(string primero, string segundo) => Normalizar(primero) == Normalizar(segundo);

    internal static ConsoleKey Tecla(string? texto) => texto?.Trim().ToLowerInvariant() switch
    {
        "" or "enter" => ConsoleKey.Enter,
        "esc" or "escape" => ConsoleKey.Escape,
        "delete" or "del" => ConsoleKey.Delete,
        "backspace" or "bksp" => ConsoleKey.Backspace,
        "cancelar" or "c" => ConsoleKey.C,
        "salir" or "x" => ConsoleKey.Escape,
        "buscar oponente" or "jugar" => ConsoleKey.Enter,
        "w" => ConsoleKey.W, "a" => ConsoleKey.A, "s" => ConsoleKey.S, "d" => ConsoleKey.D,
        "h" => ConsoleKey.H, "j" => ConsoleKey.J, "k" => ConsoleKey.K, "l" => ConsoleKey.L,
        _ => ConsoleKey.NoName
    };

    private static string Normalizar(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(descompuesto.Length);
        foreach (char caracter in descompuesto)
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(caracter))
                resultado.Append(char.ToLowerInvariant(caracter));
        return resultado.ToString();
    }
}
