using Proyecto_Integrador_Final.Cliente;

internal static class Program
{
    // Una salida local también tolera que la tubería de consola se haya cerrado.
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
        // Ctrl+C cancela el cliente sin enviar una solicitud de abandono.
        using var cierre = new CancellationTokenSource();
        ConsoleCancelEventHandler cancelar = (_, e) => { e.Cancel = true; cierre.Cancel(); };
        Console.CancelKeyPress += cancelar;
        try
        {
            // La entrada redirigida se comparte entre configuración y navegación de la sesión.
            using var flujo = Console.IsInputRedirected ? new StreamReader(Console.OpenStandardInput()) : null;
            TextReader lector = flujo ?? Console.In;
            // Corregir los datos en este proceso conserva el UUID de sesión para los reintentos.
            Guid idSesion = Guid.NewGuid();
            bool corregir;
            do
            {
                // Cada intento libera su transporte antes de volver a solicitar los datos.
                var configuracion = await ConfiguracionCliente.LeerAsync(lector, Console.Out, cierre.Token);
                using var cliente = new ClientePartidaHttp(configuracion, idSesion);
                using var entrada = new EntradaCliente(lector);
                corregir = await new SesionPartidaCliente(configuracion, cliente, new InterfazConsola()).Ejecutar(entrada, cierre.Token);
            } while (corregir && !cierre.IsCancellationRequested);
            return 0;
        }
        // EOF y cancelación son cierres locales; no confirman el final de la partida.
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
        // Se retira el manejador para no conservar referencias después del cierre.
        finally { Console.CancelKeyPress -= cancelar; }
    }
}
