# ConsolaServidorTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConsolaServidorTests.cs](../../../../test/proyecto-integrador-final-tests/ConsolaServidorTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba ayuda y diálogos administrativos con repositorio SQLite en memoria y texto redirigido. No arranca Kestrel.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ConsolaServidor](../../src/servidor/Api/ConsolaServidor.md), [Partida](../../src/servidor/Partida.md), [Personaje](../../src/servidor/Personaje.md), [RepositorioJuego](../../src/servidor/Api/RepositorioJuego.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ConsolaServidorTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos. Cada caso/auxiliar posee su RepositorioJuego y flujos con using, sin base persistida.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ConsolaServidorTests.Ayuda_ExplicaLasAccionesYLosDatosQueSePiden`

Ejecuta ayuda y EOF; exige título, mención a los dos jugadores y seis activos, y excluye sintaxis antigua <idPartida>. No ejecuta inicio de partida.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void Ayuda_ExplicaLasAccionesYLosDatosQueSePiden()
    {
        string salida = Ejecutar("ayuda\n");
        Assert.Contains("Consola del servidor de Guerra", salida);
        Assert.Contains("los dos jugadores", salida);
        Assert.Contains("seis personajes activos", salida);
        Assert.DoesNotContain("<idPartida>", salida);
    }
```

### `ConsolaServidorTests.Cancelar_PideUuidYReintentaSinPerderLaAccion`

Crea partida en repositorio memoria, alimenta cancelar/incorrecto/UUID real y ejecuta con CancellationToken.None. Exige prompt UUID, aviso válido y confirmación cancelada. No consulta fase final: el alcance de la aserción es salida del operador.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void Cancelar_PideUuidYReintentaSinPerderLaAccion()
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cancelar\nincorrecto\n{partida.Id}\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        Assert.Contains("UUID", salida.ToString());
        Assert.Contains("válido", salida.ToString());
        Assert.Contains("Partida cancelada.", salida.ToString());
    }
```

### `ConsolaServidorTests.FinDeEntrada_NoConfirmaAccionIncompleta`

Theory: cancelar+EOF; cerrar-sesion+UUID fijo+EOF antes de plaza; desactivar+no+EOF tras inválido. Para cada salida excluye las tres confirmaciones. No demuestra que todo estado interno permanezca igual, solo que no se anuncia una acción incompleta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("cancelar\n")]
    [InlineData("cerrar-sesion\n12345678-1234-4234-8234-123456789abc\n")]
    [InlineData("desactivar\nno\n")]
    public void FinDeEntrada_NoConfirmaAccionIncompleta(string comandos)
    {
        string salida = Ejecutar(comandos);
        Assert.DoesNotContain("Partida cancelada.", salida);
        Assert.DoesNotContain("Sesión cerrada.", salida);
        Assert.DoesNotContain("Personaje desactivado.", salida);
    }
```

### `ConsolaServidorTests.CerrarSesion_PidePlazaYPermiteCorregirla`

Crea partida y envía cerrar-sesion, UUID, J3 y j2. Exige instrucción J1 o J2 y Sesión cerrada, mostrando corrección y aceptación sin mayúsculas. No incorpora una sesión activa en esta prueba.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void CerrarSesion_PidePlazaYPermiteCorregirla()
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cerrar-sesion\n{partida.Id}\nJ3\nj2\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        Assert.Contains("J1 o J2", salida.ToString());
        Assert.Contains("Sesión cerrada.", salida.ToString());
    }
```

### `ConsolaServidorTests.Ejecutar`

private static string: recibe secuencia de comandos, crea repositorio :memory:, StringReader y StringWriter, llama ConsolaServidor.Ejecutar sin cancelación externa y devuelve texto al alcanzar EOF. using libera recursos aunque la ejecución lance.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static string Ejecutar(string comandos)
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        using var entrada = new StringReader(comandos);
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        return salida.ToString();
    }
```

## Recorridos y ejemplos

Normal: cancelar, UUID inválido y UUID real conserva la acción hasta imprimir Partida cancelada.

Alternativo o de error: EOF durante cancelar/cerrar-sesion/desactivar no imprime confirmación de acción; J3 se corrige a j2.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
