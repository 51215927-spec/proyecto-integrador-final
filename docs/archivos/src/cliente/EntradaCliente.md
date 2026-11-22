# EntradaCliente.cs

Fuente: [src/cliente/EntradaCliente.cs](../../../../src/cliente/EntradaCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Convierte teclado físico o líneas redirigidas en eventos de navegación para [SesionPartidaCliente](SesionPartidaCliente.md). Distingue Enter de EOF para que el cierre de entrada nunca confirme una elección.

## Dependencias

`Console`, `TextReader`, `Task.Delay` y CancellationToken. No conoce HTTP ni estado de partida. [Program](Program.md) proporciona el lector; [InterfazConsola](InterfazConsola.md) solo presenta resultados.

## Estructura y datos

`internal sealed class EntradaCliente(TextReader? entrada = null) : IDisposable`. Constructor primario sin guardas: `private readonly bool interactiva = !Console.IsInputRedirected` y `private readonly TextReader lector = entrada ?? Console.In`. La selección interactiva depende de stdin global, no de que se haya inyectado otro lector. No hay propiedades, eventos, partial ni estado de selección.

## Miembros y ejecución

`internal async Task<ConsoleKey?> Leer(CancellationToken ct)`: en modo interactivo espera cada 25 ms mientras no haya tecla; cancelar interrumpe la espera. `ReadKey(true)` no hace eco; Ctrl+D/Ctrl+Z retornan null, cualquier otra tecla retorna su ConsoleKey. En modo redirigido ReadLineAsync(ct) consume una sola línea y aplica Trim/ToLowerInvariant: null→null; vacío→Enter; `esc`→Escape; w/a/s/d/h/j/k/l→sus teclas; cualquier otra cadena→NoName. No interpreta nombres de flechas escritos como texto.

Fragmento real de `Leer`, tras la rama interactiva:

```csharp
        string? linea = await lector.ReadLineAsync(ct);
        return linea?.Trim().ToLowerInvariant() switch
        {
            null => null, "" => ConsoleKey.Enter, "esc" => ConsoleKey.Escape,
            "w" => ConsoleKey.W, "a" => ConsoleKey.A, "s" => ConsoleKey.S, "d" => ConsoleKey.D,
            "h" => ConsoleKey.H, "j" => ConsoleKey.J, "k" => ConsoleKey.K, "l" => ConsoleKey.L,
            _ => ConsoleKey.NoName
        };
```

`internal void DescartarSobrante()`: solo en modo interactivo consume teclas mientras KeyAvailable. En redirección conserva líneas futuras, que pueden ser acciones intencionales; no modifica el lector.

```csharp
    internal void DescartarSobrante()
    {
        if (interactiva) while (Console.KeyAvailable) Console.ReadKey(true);
    }

    // El lector pertenece al arranque y se reutiliza si el usuario corrige la configuración.
    public void Dispose() { }
```

`public void Dispose()` no hace nada: la propiedad del lector corresponde a Program y puede reutilizarse al corregir configuración. No cierra Console.In ni un lector inyectado.

## Recorridos y ejemplos

Normal en tubería: líneas `s`, vacía y `esc` devuelven S, Enter y Escape en llamadas consecutivas. En terminal una flecha retorna directamente el código de flecha.

Alternativo: una línea `otra` devuelve NoName y no confirma; EOF devuelve null. Cancelación durante espera lanza OperationCanceledException hacia el coordinador. DescartarSobrante en tubería no elimina el siguiente Enter.

## Errores, límites y efectos observables

Lee entrada; no imprime ni envía HTTP. ReadLineAsync puede lanzar IOException; el arranque la maneja. La espera de teclado es cancelable; no hay cancelación específica entre detectar tecla y leerla. No posee recursos que deba liberar.

### Leer: espera física cancelable

La rama física espera con token y retorna null para los controles EOF, mientras la rama de líneas ya mostrada mantiene la distinción entre null y vacío.

```csharp
    internal async Task<ConsoleKey?> Leer(CancellationToken ct)
    {
        if (interactiva)
        {
            // Esperar de forma cancelable evita que una tecla pendiente impida cerrar el proceso.
            while (!Console.KeyAvailable) await Task.Delay(25, ct);
            var tecla = Console.ReadKey(true);
            if (tecla.KeyChar is '\u0004' or '\u001a') return null;
            return tecla.Key;
        }
        // Una línea vacía confirma; null significa EOF y nunca debe convertirse en Enter.
```

## Relaciones y verificación

[SesionClienteTests](../../test/proyecto-integrador-final-tests/SesionClienteTests.md) y [ClienteProcesoRestTests](../../test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md) ejercitan navegación y salida; no equivalen a prueba de todos los teclados o sistemas operativos. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.
