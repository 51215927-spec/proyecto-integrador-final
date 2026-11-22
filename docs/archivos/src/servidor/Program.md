# Program.cs

Fuente: [src/servidor/Program.cs](../../../../src/servidor/Program.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `(global)`. Versión: [catálogo común](../../README.md#versión-documentada).

## Propósito y responsabilidad

Arranque y ciclo de vida del servidor.

## Estado, miembros y ejecución

Main obtiene ConfiguracionServidor, ofrece reconfigurar si falla el arranque y crea ServidorHttp. La consola recibe por DI los tres componentes administrativos: CoordinadorPartidas, AdministradorSesiones y CatalogoPersonajes. Conserva el control de cancelación, parada del host y liberación de recursos.

## Errores, límites y efectos

Es el Program de src/servidor, independiente de cliente y cargador. No ejecuta reglas de partida ni administra conexiones directamente; los errores y opciones de reconfiguración conservan el flujo previo.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
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
                var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
                await app.StartAsync();
                var consola = Task.Run(() => ConsolaServidor.Ejecutar(juego, app.Services.GetRequiredService<AdministradorSesiones>(),
                    app.Services.GetRequiredService<CatalogoPersonajes>(), Console.In, Console.Out,
                    app.Lifetime.ApplicationStopping));
                var terminada = await Task.WhenAny(consola, app.WaitForShutdownAsync());
                await terminada;
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
```

## Relaciones y verificación

Véanse [arquitectura](../../../arquitectura.md), [modelos](../../../modelos.md) y [auditoría del spec 006](../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
