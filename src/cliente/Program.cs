using Proyecto_Integrador_Final.Cliente;
using Microsoft.Win32.SafeHandles;
using System.Text;

internal static class Program
{
    // Incluso un cierre normal puede encontrar una tubería rota: el diagnóstico usa stderr.
    private static int InformarSalida(string mensaje)
    {
        try { Console.WriteLine(mensaje); return 0; }
        catch (IOException)
        {
            try { Console.Error.WriteLine("No se pudo usar la consola."); } catch (IOException) { }
            return 1;
        }
    }

    private static async Task<int> Main()
    {
        // Ctrl+C solicita una salida local y cancela las tareas; no equivale a abandonar la partida.
        using var cierre = new CancellationTokenSource();
        ConsoleCancelEventHandler cancelar = (_, e) => { e.Cancel = true; cierre.Cancel(); };
        Console.CancelKeyPress += cancelar;
        try
        {
            // Console.Out omite EPIPE en Unix. El descriptor estándar no es propiedad
            // del cliente; este escritor permite detectar una tubería cerrada.
            if (!OperatingSystem.IsWindows() && Console.IsOutputRedirected)
                Console.SetOut(new StreamWriter(new FileStream(new SafeFileHandle((IntPtr)1, ownsHandle: false),
                    FileAccess.Write), new UTF8Encoding(false)) { AutoFlush = true });
            using var flujo = Console.IsInputRedirected ? new StreamReader(Console.OpenStandardInput()) : null;
            TextReader lector = flujo is null ? Console.In : new EntradaLineas(flujo);
            await new InicioCliente(lector, Console.Out, AccesoLocal.DelUsuario()).Ejecutar(cierre.Token);
            return 0;
        }
        // EOF y cancelación cierran localmente; solo el menú confirmado puede enviar abandono.
        catch (EndOfStreamException)
        {
            return InformarSalida("Salida local · Fin de la entrada. La partida queda guardada.");
        }
        catch (OperationCanceledException) when (cierre.IsCancellationRequested)
        {
            return InformarSalida("Salida local · La partida queda guardada.");
        }
        catch (IOException)
        {
            try { Console.Error.WriteLine("No se pudo usar la consola."); } catch (IOException) { }
            return 1;
        }
        // Se retira el manejador al terminar para no dejar callbacks asociados al proceso.
        finally { Console.CancelKeyPress -= cancelar; }
    }
}
