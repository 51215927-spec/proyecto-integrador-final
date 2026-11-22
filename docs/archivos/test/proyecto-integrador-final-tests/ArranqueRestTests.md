# ArranqueRestTests.cs

Fuente: [test/proyecto-integrador-final-tests/ArranqueRestTests.cs](../../../../test/proyecto-integrador-final-tests/ArranqueRestTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Contratos del ejecutable real cliente sobre stdin/stdout/stderr redirigidos. Requieren GUERRA_CLIENTE_DLL no vacía, dotnet disponible y DLL compilada de esta versión.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ArranqueRestTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos. ProcessStartInfo usa dotnet con DLL de entorno; procesos y plazos se liberan con using. El primer caso mata el árbol en finally si sigue vivo; el segundo depende de timeout y salida normal.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ArranqueRestTests.TuberiaCerradaTerminaSinEsperarEntradaNiExponerCredencial`

ContratoFact GUERRA_CLIENTE_DLL. Lanza proceso, cierra su StandardOutput y escribe URL loopback puerto1, UUID fijo, secreto-tuberia y Ana. Espera máximo5s; stderr debe indicar No se pudo usar la consola, no incluir secreto y ExitCode1. finally mata árbol y espera si no salió. Acredita tubería cerrada, no servidor disponible.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task TuberiaCerradaTerminaSinEsperarEntradaNiExponerCredencial()
    {
        var inicio = new ProcessStartInfo("dotnet")
        { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
        using var proceso = Process.Start(inicio)!;
        try
        {
            proceso.StandardOutput.Close();
            await proceso.StandardInput.WriteAsync("http://127.0.0.1:1\n11223344-1122-3344-5566-778899aabbcc\nsecreto-tuberia\nAna\n");
            await proceso.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            string errores = await proceso.StandardError.ReadToEndAsync();
            Assert.Equal(1, proceso.ExitCode);
            Assert.Contains("No se pudo usar la consola", errores);
            Assert.DoesNotContain("secreto-tuberia", errores);
        }
        finally
        {
            if (!proceso.HasExited) { proceso.Kill(true); await proceso.WaitForExitAsync(); }
        }
    }
```

### `ArranqueRestTests.SolicitaConexionCorrigeCamposYNoOfreceDemo`

ContratoFact GUERRA_CLIENTE_DLL. Lanza dotnet con tres flujos redirigidos y plazo10s; lee output concurrentemente, escribe URL inválida/válida con prefijo y UUID inválido/cero, cierra stdin y espera. Exige código0, prompts URL base y UUID, ausencia de Demostración y Credencial porque Guid.Empty no se acepta. No completa una incorporación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task SolicitaConexionCorrigeCamposYNoOfreceDemo()
    {
        var inicio = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
        using var proceso = Process.Start(inicio)!;
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var salida = proceso.StandardOutput.ReadToEndAsync(plazo.Token);
        await proceso.StandardInput.WriteAsync("invalida\nhttp://localhost:5000/prefijo/\ninvalido\n00000000-0000-0000-0000-000000000000\n");
        proceso.StandardInput.Close();
        await proceso.WaitForExitAsync(plazo.Token);
        var texto = await salida;
        Assert.Equal(0, proceso.ExitCode);
        Assert.Contains("URL base", texto);
        Assert.Contains("UUID", texto);
        Assert.DoesNotContain("Demostración", texto);
        Assert.DoesNotContain("Credencial", texto); // No avanza con un UUID vacío.
    }
```

## Recorridos y ejemplos

Normal: Input con URL y UUID inválidos vuelve a preguntar y, tras UUID vacío y EOF, termina0 sin pedir credencial.

Alternativo o de error: Cerrar la salida del proceso provoca diagnóstico de consola y código1 sin revelar el secreto.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
