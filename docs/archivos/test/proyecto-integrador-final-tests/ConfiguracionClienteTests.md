# ConfiguracionClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs](../../../../test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba reglas de URL, repetición de campos inválidos, conservación de espacios internos y ocultación de credencial en configuración.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ConfiguracionCliente](../../src/cliente/ConfiguracionCliente.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ConfiguracionClienteTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin estado de fixture. StringReader y StringWriter son descartados mediante using.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ConfiguracionClienteTests.ValidaUrlSegunContrato`

Theory: http://localhost:5000 y https://localhost/prefijo deben retornar true; /relativa, ftp://localhost, http://usuario:clave@localhost, http://localhost/?consulta=1 y http://localhost/#fragmento retornan false. Solo llama EsUrl y compara bool; no prueba resolución DNS ni TLS.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("http://localhost:5000", true)]
    [InlineData("https://localhost/prefijo", true)]
    [InlineData("/relativa", false)]
    [InlineData("ftp://localhost", false)]
    [InlineData("http://usuario:clave@localhost", false)]
    [InlineData("http://localhost/?consulta=1", false)]
    [InlineData("http://localhost/#fragmento", false)]
    public void ValidaUrlSegunContrato(string url, bool valida) => Assert.Equal(valida, ConfiguracionCliente.EsUrl(url));
```

### `ConfiguracionClienteTests.RecortaExterioresPreservaNombreYRepiteSoloCampoInvalido`

Genera UUID, alimenta URL inválida seguida de HTTPS válido, UUID inválido seguido del válido, credencial con espacios y nombre con doble espacio interior. Leer debe devolver Uri con / final, mismo UUID, secreto sin exteriores y Ana··Pérez con interiores intactos. Exige que secreto no aparezca ni en salida ni en ToString del record. No cuenta exactamente prompts ni mide ocultación por teclado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RecortaExterioresPreservaNombreYRepiteSoloCampoInvalido()
    {
        Guid id = Guid.NewGuid();
        using var entrada = new StringReader($"invalida\n https://localhost/prefijo \ninvalido\n{id}\n  secreto  \n  Ana  Pérez  \n");
        using var salida = new StringWriter();
        var config = ConfiguracionCliente.Leer(entrada, salida);
        Assert.Equal("Ana  Pérez", config.Nombre);
        Assert.Equal("secreto", config.Credencial);
        Assert.Equal(new Uri("https://localhost/prefijo/"), config.Url);
        Assert.Equal(id, config.Partida);
        Assert.DoesNotContain("secreto", salida.ToString() + config.ToString());
    }
```

## Recorridos y ejemplos

Normal: URL con prefijo HTTPS se normaliza con barra final, nombre exterior recortado conserva Ana··Pérez y credencial queda secreto.

Alternativo o de error: URL relativa, FTP, userinfo, query y fragment son rechazados. No ejecuta conexión ni verifica una terminal interactiva.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
