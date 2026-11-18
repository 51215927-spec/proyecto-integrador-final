using Proyecto_Integrador_Final.Api;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        await using var app = ServidorHttp.Crear(args);
        var juego = app.Services.GetRequiredService<RepositorioJuego>();
        await app.StartAsync();
        _ = Task.Run(() => ConsolaServidor.Ejecutar(juego, Console.In, Console.Out, app.Lifetime.ApplicationStopping));
        await app.WaitForShutdownAsync();
    }
}
