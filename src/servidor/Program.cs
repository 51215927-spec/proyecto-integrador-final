using Proyecto_Integrador_Final.Api;

await using var app = ServidorHttp.Crear(args);
var juego = app.Services.GetRequiredService<RepositorioJuego>();
await app.StartAsync();
_ = Task.Run(() => ConsolaServidor.Ejecutar(juego, Console.In, Console.Out, app.Lifetime.ApplicationStopping));
await app.WaitForShutdownAsync();
