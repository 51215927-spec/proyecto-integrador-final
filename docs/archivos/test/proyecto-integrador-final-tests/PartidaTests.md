# PartidaTests.cs

Fuente: [test/proyecto-integrador-final-tests/PartidaTests.cs](../../../../test/proyecto-integrador-final-tests/PartidaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Protege la precondición de seis cartas antes del reparto en el coordinador local. No prueba una partida completa ni la API.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](CartasDePrueba.md), [Jugador](../../src/servidor/Jugador.md), [Mazo](../../src/servidor/Mazo.md), [Partida](../../src/servidor/Partida.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class PartidaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Dos campos privados readonly inicializan jugadores Ana y Bruno para cada instancia de prueba. Los casos de Theory reciben fixtures nuevos.

Declaraciones reales de estado de `PartidaTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Jugador primero = new("Ana");
```

```csharp
private readonly Jugador segundo = new("Bruno");
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `PartidaTests.RechazaUnMazoInsuficienteAntesDeRepartir`

Theory con 0 y 5. Crea exactamente cantidad personajes mediante Enumerable.Range y CartasDePrueba, construye Partida con ambos jugadores y captura InvalidOperationException de Jugar. Compara mensaje exacto y luego Count y manos para demostrar que la guarda antecede al reparto. No comprueba el comportamiento con 6 ni ejecuta una ronda.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void RechazaUnMazoInsuficienteAntesDeRepartir(int cantidad)
    {
        var mazo = new Mazo(Enumerable.Range(0, cantidad).Select(_ => CartasDePrueba.Crear()).ToList());
        var partida = new Partida(primero, segundo, mazo);

        var error = Assert.Throws<InvalidOperationException>(() => partida.Jugar());

        Assert.Equal("No se puede jugar porque no hay suficientes cartas.", error.Message);

        Assert.Equal(cantidad, mazo.GetCantidad());
        Assert.Empty(primero.GetCartas());
        Assert.Empty(segundo.GetCartas());
    }
```

## Recorridos y ejemplos

Normal: Con cantidad 5, Jugar lanza el mensaje de mazo insuficiente, conserva cinco cartas y ambas manos vacías.

Alternativo o de error: Cantidad 0 recorre la misma guarda. No hay recorrido exitoso de Jugar en este archivo; el coordinador local no incorpora elecciones de clientes.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
