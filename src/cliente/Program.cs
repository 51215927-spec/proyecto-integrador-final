using Proyecto_Integrador_Final.Cliente;

internal static class Program
{
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
        using var cierre = new CancellationTokenSource();
        ConsoleCancelEventHandler cancelar = (_, e) => { e.Cancel = true; cierre.Cancel(); };
        Console.CancelKeyPress += cancelar;
        try
        {
            using var flujo = Console.IsInputRedirected ? new StreamReader(Console.OpenStandardInput()) : null;
            TextReader lector = flujo ?? Console.In;
            Guid idSesion = Guid.NewGuid();
            bool corregir;
            do
            {
                var configuracion = await ConfiguracionCliente.LeerAsync(lector, Console.Out, cierre.Token);
                using var cliente = new ClientePartidaHttp(configuracion, idSesion);
                using var entrada = new EntradaCliente(lector);
                corregir = await new SesionPartidaCliente(configuracion, cliente, new InterfazConsola()).Ejecutar(entrada, cierre.Token);
            } while (corregir && !cierre.IsCancellationRequested);
            return 0;
        }
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
        finally { Console.CancelKeyPress -= cancelar; }
    }
}
