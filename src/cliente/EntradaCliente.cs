namespace Proyecto_Integrador_Final.Cliente;

internal sealed class EntradaCliente(TextReader? entrada = null) : IDisposable
{
    // Las terminales usan teclas; las pruebas y tuberías pueden proporcionar líneas.
    private readonly bool interactiva = !Console.IsInputRedirected;
    private readonly TextReader lector = entrada ?? Console.In;

    internal async Task<ConsoleKey?> Leer(CancellationToken ct)
    {
        if (interactiva)
        {
            // La espera breve deja que Ctrl+C cancele sin bloquear una lectura de tecla.
            while (!Console.KeyAvailable) await Task.Delay(25, ct);
            var tecla = Console.ReadKey(true);
            if (tecla.KeyChar is '\u0004' or '\u001a') return null;
            return tecla.Key;
        }
        // Las líneas vacías equivalen a Enter; las desconocidas no ejecutan una acción.
        string? linea = await lector.ReadLineAsync(ct);
        return linea?.Trim().ToLowerInvariant() switch
        {
            null => null, "" => ConsoleKey.Enter, "esc" => ConsoleKey.Escape,
            "w" => ConsoleKey.W, "a" => ConsoleKey.A, "s" => ConsoleKey.S, "d" => ConsoleKey.D,
            "h" => ConsoleKey.H, "j" => ConsoleKey.J, "k" => ConsoleKey.K, "l" => ConsoleKey.L,
            _ => ConsoleKey.NoName
        };
    }

    // Se descartan teclas acumuladas al cambiar la selección o la pantalla.
    internal void DescartarSobrante()
    {
        if (interactiva) while (Console.KeyAvailable) Console.ReadKey(true);
    }

    public void Dispose() { }
}
