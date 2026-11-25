namespace Proyecto_Integrador_Final.Cliente;

internal sealed class EntradaCliente(TextReader? entrada = null) : IDisposable
{
    // La terminal usa teclas; las pruebas y tuberías conservan el orden de las líneas.
    private readonly bool interactiva = !Console.IsInputRedirected;
    private readonly TextReader lector = entrada ?? Console.In;
    internal string? Texto { get; private set; }
    internal char Caracter { get; private set; }

    internal async Task<ConsoleKey?> Leer(CancellationToken ct)
    {
        Texto = null;
        Caracter = '\0';
        if (interactiva)
        {
            // Esperar de forma cancelable evita que una tecla pendiente impida cerrar el proceso.
            while (!Console.KeyAvailable) await Task.Delay(25, ct);
            var tecla = Console.ReadKey(true);
            Caracter = tecla.KeyChar;
            if (tecla.KeyChar is '\u0004' or '\u001a') return null;
            return tecla.Key;
        }
        // Una línea vacía confirma; null significa EOF y nunca debe convertirse en Enter.
        string? linea = await lector.ReadLineAsync(ct);
        if (linea is null) return null;
        Texto = linea.Trim();
        return EntradaTeclado.Tecla(Texto);
    }

    // Solo se vacía el teclado físico: las líneas futuras pueden ser acciones deliberadas.
    internal void DescartarSobrante()
    {
        if (interactiva) while (Console.KeyAvailable) Console.ReadKey(true);
    }

    // El lector pertenece al arranque y se reutiliza si el usuario corrige la configuración.
    public void Dispose() { }
}
