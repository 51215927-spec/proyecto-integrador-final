using Proyecto_Integrador_Final.Cliente;

internal static class Program
{
    private static int Main(string[] args)
    {
        var interfaz = new InterfazConsola();
        try
        {
            switch (args)
            {
                case []:
                    interfaz.MostrarPreparacion(null, "Conexión pendiente. Usá --demo para probar las pantallas localmente.");
                    return 0;
                case ["--demo"]:
                case ["--demo", "pausa" or "abandono" or "cancelacion" or "salida"]:
                    var demo = new DemostracionConsola(interfaz, Console.In, Console.Out,
                        !Console.IsInputRedirected && !Console.IsOutputRedirected);
                    demo.Recorrer(args.Length == 2 ? args[1] : null);
                    return 0;
                case ["--help"] or ["-h"]:
                    Console.WriteLine("dotnet run --project src/cliente -- --demo [pausa|abandono|cancelacion|salida]");
                    return 0;
                default:
                    interfaz.MostrarError("Opción desconocida. Usá --help para ver los argumentos.");
                    return 2;
            }
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
