# ConfiguracionServidorTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs](../../../../test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba lectura administrativa sin iniciar host, valores por Enter, corrección por campo y fin de entrada.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ConfiguracionServidor](../../src/servidor/Api/ConfiguracionServidor.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ConfiguracionServidorTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos. La ruta temporal usada para validar configuración no se crea; Assert.False lo comprueba.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ConfiguracionServidorTests.Enter_ConservaValoresInicialesYDeshabilitaAltaSinCredencial`

Lee tres Enter con StringReader; compara dirección y archivo predeterminados, credencial vacía y mensaje de altas deshabilitadas. No demuestra apertura del puerto ni creación de SQLite.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void Enter_ConservaValoresInicialesYDeshabilitaAltaSinCredencial()
    {
        using var salida = new StringWriter();
        var config = ConfiguracionServidor.Leer(new StringReader("\n\n\n"), salida);
        Assert.Equal("http://0.0.0.0:5000", config.Direccion);
        Assert.Equal("guerra.db", config.Archivo);
        Assert.Equal("", config.Credencial);
        Assert.Contains("alta de personajes deshabilitada", salida.ToString());
    }
```

### `ConfiguracionServidorTests.ConfiguracionInvalida_PermiteCorregirCadaCampoSinMostrarLaCredencial`

Alimenta ftp://localhost luego http://127.0.0.1:0; NUL luego ruta temporal; clave con espacio luego secreto-003. Exige los valores corregidos, ausencia de ambas credenciales en salida y que la ruta no exista. Permite puerto 0 para pruebas; no acredita que el directorio sea escribible al iniciar host.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ConfiguracionInvalida_PermiteCorregirCadaCampoSinMostrarLaCredencial()
    {
        using var salida = new StringWriter();
        var archivo = Path.Combine(Path.GetTempPath(), $"guerra-{Guid.NewGuid()}.db");
        var config = ConfiguracionServidor.Leer(new StringReader(
            $"ftp://localhost\nhttp://127.0.0.1:0\n\0\n{archivo}\nclave secreta\nsecreto-003\n"), salida);
        Assert.Equal("http://127.0.0.1:0", config.Direccion);
        Assert.Equal(archivo, config.Archivo);
        Assert.Equal("secreto-003", config.Credencial);
        Assert.DoesNotContain("secreto-003", salida.ToString());
        Assert.DoesNotContain("clave secreta", salida.ToString());
        Assert.False(File.Exists(archivo));
    }
```

### `ConfiguracionServidorTests.FinDeEntrada_InterrumpeLaConfiguracion`

Theory con entrada vacía, un Enter, dos Enter e incorrecta seguido de EOF. Cada Leer debe lanzar EndOfStreamException en el campo incompleto o al repetir el inválido. No construye host ni aplica configuración parcial.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\n\n")]
    [InlineData("incorrecta\n")]
    public void FinDeEntrada_InterrumpeLaConfiguracion(string texto)
    {
        Assert.Throws<EndOfStreamException>(() =>
            ConfiguracionServidor.Leer(new StringReader(texto), new StringWriter()));
    }
```

## Recorridos y ejemplos

Normal: Tres líneas vacías devuelven escucha http://0.0.0.0:5000, archivo guerra.db y credencial vacía con aviso de altas deshabilitadas.

Alternativo o de error: FTP, ruta con NUL y credencial con espacio se corrigen con nuevas líneas sin exponer secretos; EOF aborta.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
