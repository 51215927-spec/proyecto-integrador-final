using Proyecto_Integrador_Final.Consola;

namespace Proyecto_Integrador_Final.Api;

internal sealed record ConfiguracionServidor(string Direccion, string Archivo, string Credencial)
{
    internal static ConfiguracionServidor Leer(TextReader entrada, TextWriter salida)
    {
        var lector = new EntradaConsola(entrada, salida);
        string direccion = lector.Url("http://0.0.0.0:5000");
        string archivo = lector.Validar("Archivo SQLite (Enter: guerra.db)", ArchivoValido,
            "Ingresá una ruta de archivo válida en un directorio existente.", "guerra.db");
        string credencial = lector.Secreto("Credencial de catálogo (Enter: deshabilitar altas)", permiteVacio: true);
        if (credencial.Length == 0)
            salida.WriteLine("Credencial vacía: alta de personajes deshabilitada.");
        return new(direccion, archivo, credencial);
    }

    private static bool ArchivoValido(string valor)
    {
        try
        {
            string ruta = Path.GetFullPath(valor);
            return !valor.Any(char.IsControl) && !Directory.Exists(ruta) &&
                Directory.Exists(Path.GetDirectoryName(ruta));
        }
        catch (Exception error) when (error is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
