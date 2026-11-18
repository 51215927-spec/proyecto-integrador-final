using Proyecto_Integrador_Final.Cliente;

var interfaz = new InterfazConsola();
try
{
    interfaz.MostrarEstado("Juego Guerra", "El flujo de conexión a una partida todavía está pendiente.");
    return 0;
}
catch (IOException)
{
    try
    {
        Console.Error.WriteLine("No se pudo escribir en la consola.");
    }
    catch (IOException)
    {
        // La salida de errores también puede estar cerrada.
    }

    return 1;
}
