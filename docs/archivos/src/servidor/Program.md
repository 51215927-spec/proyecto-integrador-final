# Program.cs

Fuente: [src/servidor/Program.cs](../../../../src/servidor/Program.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Es el punto de entrada del ejecutable servidor. Lee configuración, crea el host, arranca HTTP y la consola administrativa, y coordina su cierre. Reintenta el inicio ante errores de dirección/archivo sin cambiar código o persistencia para ocultarlos.

## Dependencias

[ConfiguracionServidor](Api/ConfiguracionServidor.md) obtiene datos; [ServidorHttp](Api/ServidorHttp.md) construye WebApplication; [RepositorioJuego](Api/RepositorioJuego.md) se resuelve por DI; [ConsolaServidor](Api/ConsolaServidor.md) atiende comandos. Microsoft.Data.Sqlite.SqliteException clasifica fallos de base. Task.Run, WhenAny y WaitForShutdownAsync coordinan trabajos. [Instalación y uso](../../../instalacion-y-uso.md) describe ejecución.

## Estructura y estado

`internal static class Program` en namespace global (no declara namespace). Contiene solo `private static async Task<int> Main()`, sin argumentos de aplicación ni campos, propiedades, eventos, partial o constructores de instancia. Los datos de inicio, app, repositorio y tareas son variables locales de cada intento. `await using` libera host al salir del bloque incluso ante excepción.

## Miembros y ejecución

Main retorna código de proceso. Dentro de `while(true)`:

1. Lee configuración desde Console.In/Out. EndOfStreamException imprime «Fin de la entrada. Inicio cancelado.» y retorna 0; IOException retorna 1 sin diagnóstico adicional. Otros fallos de esa primera lectura no se capturan aquí.
2. Crea app mediante ServidorHttp.Crear con array de args vacío e inicio interactivo. Resuelve RepositorioJuego (que abre/crea SQLite) antes de StartAsync.
3. Espera StartAsync; después lanza ConsolaServidor.Ejecutar en Task.Run con token ApplicationStopping. Espera la primera tarea terminada entre consola y espera de cierre del host; luego hace await de esa tarea para propagar su error, llama StopAsync y retorna 0.
4. En el bloque de host captura IOException, UnauthorizedAccessException, SqliteException o InvalidOperationException. Imprime mensaje genérico de revisar dirección/SQLite y repite toda la configuración; no muestra secreto ni detalles internos. No limita intentos: termina por inicio/parada correctos o EOF/error de lectura.

Fragmento real de coordinación de vida del host:

```csharp
                await using var app = ServidorHttp.Crear([], inicio: inicio);
                var juego = app.Services.GetRequiredService<RepositorioJuego>();
                await app.StartAsync();
                var consola = Task.Run(() => ConsolaServidor.Ejecutar(juego, Console.In, Console.Out,
                    app.Lifetime.ApplicationStopping));
                var terminada = await Task.WhenAny(consola, app.WaitForShutdownAsync());
                await terminada;
                await app.StopAsync();
                return 0;
```

La tarea que no ganó WhenAny no se espera explícitamente aquí; StopAsync notifica parada y la disposición asíncrona libera host. ApplicationStopping no fuerza cancelación de una ReadLine ya bloqueada dentro de consola. La lambda de Task.Run solo llama al ejecutor con los flujos y token capturados.

### Fragmentos de errores y reintento de inicio

EOF durante configuración termina con éxito local sin crear host; un fallo de lectura de consola retorna 1. Estos catch pertenecen al bloque de lectura, no al host:

```csharp
            catch (EndOfStreamException)
            {
                Console.WriteLine("Fin de la entrada. Inicio cancelado.");
                return 0;
            }
            catch (IOException)
            {
                return 1;
            }
```

La captura del host escribe diagnóstico genérico y cae al siguiente ciclo de configuración, sin credenciales ni excepción completa:

```csharp
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or SqliteException or InvalidOperationException)
            {
                Console.WriteLine("No se pudo iniciar el servidor. Revisá la dirección y el archivo SQLite. Volvé a ingresar la configuración.");
            }
```

## Recorridos y ejemplos

Normal: ingresar URL `http://127.0.0.1:5000`, archivo temporal válido y secreto ficticio; tras iniciar, la consola recibe EOF, su tarea termina, Main para el host y devuelve 0. Alternativo: directorio existente pero sin permiso efectivo de crear SQLite pasa la validación de ruta, falla al abrir base, imprime reintento y vuelve a preguntar. EOF en ese segundo intento devuelve 0 sin arrancar otra aplicación. Son recorridos de lectura, no apertura de una base real desde esta página.

## Errores, límites y efectos observables

El arranque puede crear SQLite y escuchar en red. El código no elimina la base al cerrar ni migra esquemas. La captura de InvalidOperationException cubre también errores de operación durante el bloque de host y los presenta con el mismo diagnóstico genérico. No implementa un parser CLI de opciones: Main no recibe args y llama Crear con `[]`.

## Relaciones y verificación

[EntradaProcesosContratoTests](../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) define inicio sin argumentos, EOF, parada y corrección tras base inválida; [ServidorContratoTests](../../test/proyecto-integrador-final-tests/ServidorContratoTests.md) usa proceso real y SQLite aislada. Véase [verificación de referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) para comandos/resultados reales y límites del entorno.
