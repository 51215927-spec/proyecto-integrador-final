# Program.cs

Fuente: [src/cliente/Program.cs](../../../../src/cliente/Program.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `(global)`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Es el punto de entrada real del ejecutable cliente: pide configuración, crea la identidad del proceso y ejecuta la sesión REST. No ofrece selector de demostración ni acepta argumentos de aplicación.

## Dependencias

[ConfiguracionCliente](ConfiguracionCliente.md), [ClientePartidaHttp](ClientePartidaHttp.md), [EntradaCliente](EntradaCliente.md), [SesionPartidaCliente](SesionPartidaCliente.md), [InterfazConsola](InterfazConsola.md). `CancellationTokenSource` coordina salida; `SafeFileHandle`, FileStream, StreamWriter y UTF8Encoding permiten detectar tubería de stdout rota en Unix. Véase [instalación y uso](../../../instalacion-y-uso.md).

## Estructura y datos

`internal static class Program` en namespace global, sin interfaces, atributos, partial, propiedades ni campos. Dos métodos privados static. El estado vive en variables de Main: fuente de cancelación, callback, lector, IdSesion y flag corregir. Las variables using se liberan al salir de su ámbito; el descriptor stdout no es propiedad del cliente.

## Miembros y ejecución

`private static int InformarSalida(string mensaje)`: escribe mensaje a stdout y retorna 0. Si falla con IOException, intenta escribir a stderr `No se pudo usar la consola.`; si stderr también falla lo ignora y retorna 1. No imprime credenciales ni la excepción.

`private static async Task<int> Main()`: crea CancellationTokenSource y agrega ConsoleCancelEventHandler `cancelar = (_,e) => {e.Cancel=true; cierre.Cancel();}`. Impide la terminación inmediata de Ctrl+C para que se cancelen y esperen las tareas. En Unix con salida redirigida sustituye Console.Out por StreamWriter UTF-8 sin BOM, AutoFlush=true, sobre descriptor 1 y ownsHandle=false. Eso permite detectar EPIPE sin cerrar el descriptor estándar como propio.

Si stdin redirigido abre un StreamReader del estándar; si no usa Console.In. Genera un único Guid.NewGuid antes de cualquier corrección. En do/while lee configuración, crea y dispone transporte y entrada por cada intento, ejecuta una sesión nueva y repite solo si devuelve corregir=true y no se canceló. La sesión nueva evita arrastrar operaciones de otra plaza, conservando UUID del proceso.

```csharp
            Guid idSesion = Guid.NewGuid();
            bool corregir;
            do
            {
                var configuracion = await ConfiguracionCliente.LeerAsync(lector, Console.Out, cierre.Token);
                // Cada configuración tiene su transporte y coordinador para no mezclar operaciones de otras plazas.
                using var cliente = new ClientePartidaHttp(configuracion, idSesion);
                using var entrada = new EntradaCliente(lector);
                corregir = await new SesionPartidaCliente(configuracion, cliente, new InterfazConsola()).Ejecutar(entrada, cierre.Token);
            } while (corregir && !cierre.IsCancellationRequested);
            return 0;
```

EOF se traduce a salida local con mensaje de partida guardada mediante InformarSalida. OperationCanceledException se captura solo si cierre está cancelado y también informa salida local. IOException emite diagnóstico controlado a stderr y devuelve 1 (también ignora IOException secundaria en stderr). El finally siempre retira el manejador CancelKeyPress. Otras excepciones no tienen captura general aquí.

```csharp
        catch (EndOfStreamException)
        {
            return InformarSalida("Salida local · Fin de la entrada. La partida queda guardada.");
        }
        catch (OperationCanceledException) when (cierre.IsCancellationRequested)
        {
            return InformarSalida("Salida local · La partida queda guardada.");
        }
        catch (IOException)
        {
            try { Console.Error.WriteLine("No se pudo usar la consola."); } catch (IOException) { }
            return 1;
        }
        // Se retira el manejador al terminar para no dejar callbacks asociados al proceso.
        finally { Console.CancelKeyPress -= cancelar; }
```

## Recorridos y ejemplos

Normal: configuración válida, incorporación y cierre local desde menú: Ejecutar devuelve false, se disponen cliente/entrada y Main retorna 0. Corrección de credencial: la sesión devuelve true, se dispone el transporte y se piden los cuatro datos de nuevo con el mismo UUID de proceso.

Alternativo: EOF mientras se pide el nombre informa fin de entrada sin enviar abandono. Ctrl+C solicita cancelación y cierre ordenado. Una tubería stdout cerrada produce código 1 y diagnóstico por stderr; no se presenta como un defecto general del juego en el entorno del usuario.

## Errores, límites y efectos observables

No guarda configuración ni identidad en disco; reiniciar genera otra sesión. Salida local no confirma abandono ni libera inmediatamente plaza. La sustitución de Console.Out afecta el proceso entero. Los recursos HTTP se disponen tras terminar y esperar el coordinador, evitando tareas que usen un transporte ya cerrado.

## Relaciones y verificación

[ArranqueRestTests](../../test/proyecto-integrador-final-tests/ArranqueRestTests.md), [ClienteProcesoRestTests](../../test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md) y [EntradaProcesosContratoTests](../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) cubren procesos, EOF y recuperación. Requieren las DLL reales para ejecutar contratos condicionales; no acredita Windows por leer esta rama Unix. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.
