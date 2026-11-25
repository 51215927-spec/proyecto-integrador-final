using Proyecto_Integrador_Final.Api;
using Microsoft.Data.Sqlite;

internal static class Program
{
    private static async Task<int> Main()
    {
        while (true)
        {
            ConfiguracionServidor inicio;
            try
            {
                inicio = ConfiguracionServidor.Leer(Console.In, Console.Out);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Fin de la entrada. Inicio cancelado.");
                return 0;
            }
            catch (IOException)
            {
                return 1;
            }

            try
            {
                await using var app = ServidorHttp.Crear([], inicio: inicio);
                CatalogoPredeterminado.CargarSiVacio(app.Services.GetRequiredService<CatalogoPersonajes>());
                await app.StartAsync();
                await app.WaitForShutdownAsync();
                await app.StopAsync();
                return 0;
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or SqliteException or InvalidOperationException)
            {
                Console.WriteLine("No se pudo iniciar el servidor. Revisá la dirección y el archivo SQLite. Volvé a ingresar la configuración.");
            }
        }
    }
}
