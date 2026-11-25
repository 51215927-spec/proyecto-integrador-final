using System.Threading.Channels;

namespace Proyecto_Integrador_Final.Cliente;

// Una sola lectura física de stdin durante todo el proceso. Cancelar una pantalla
// cancela su espera, sin bloquear por una tubería ni consumir la próxima línea.
internal sealed class EntradaLineas : TextReader
{
    private readonly Channel<string> lineas = Channel.CreateUnbounded<string>(new() { SingleReader = true, SingleWriter = true });
    internal EntradaLineas(TextReader lector)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                while (await lector.ReadLineAsync() is { } linea) await lineas.Writer.WriteAsync(linea);
                lineas.Writer.TryComplete();
            }
            catch (IOException) { lineas.Writer.TryComplete(new IOException("No se pudo leer la entrada.")); }
            catch (ObjectDisposedException) { lineas.Writer.TryComplete(); }
        });
    }
    public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        try { return await lineas.Reader.ReadAsync(cancellationToken); }
        catch (ChannelClosedException error) when (error.InnerException is null) { return null; }
        catch (ChannelClosedException) { throw new IOException("No se pudo leer la entrada."); }
    }
    public override Task<string?> ReadLineAsync() => ReadLineAsync(default).AsTask();
}
