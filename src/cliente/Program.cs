using Proyecto_Integrador_Final.Consola;
using Proyecto_Integrador_Final.Cliente;

internal static class Program
{
    private static int Main()
    {
        var interfaz = new InterfazConsola();
        try
        {
            var lector = new EntradaConsola(Console.In, Console.Out);
            string modo = lector.Validar(
                "Demostración local: victoria, pausa, abandono, cancelacion o salida",
                valor => valor.ToLowerInvariant() is "victoria" or "pausa" or "abandono" or "cancelacion" or "salida",
                "Elegí victoria, pausa, abandono, cancelacion o salida.").ToLowerInvariant();
            var demo = new DemostracionConsola(interfaz, Console.In, Console.Out,
                !Console.IsInputRedirected && !Console.IsOutputRedirected);
            demo.Recorrer(modo == "victoria" ? null : modo);
            return 0;
        }
        catch (EndOfStreamException)
        {
            interfaz.MostrarInterrupcion("Lucía", "Fin de la entrada.", "Se cerró la demostración local.", confirmada: false);
            return 0;
        }
        catch (IOException)
        {
            try
            {
                Console.Error.WriteLine("No se pudo usar la consola.");
            }
            catch (IOException)
            {
                // La salida de errores también puede estar cerrada.
            }

            return 1;
        }
    }
}
