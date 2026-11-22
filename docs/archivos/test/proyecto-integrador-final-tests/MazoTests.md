# MazoTests.cs

Fuente: [test/proyecto-integrador-final-tests/MazoTests.cs](../../../../test/proyecto-integrador-final-tests/MazoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Verifica copia, prefijos repartidos, devolución y conservación de cartas durante mezcla. Las aserciones usan identidad de objeto para no depender del orden aleatorio.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](CartasDePrueba.md), [Mazo](../../src/servidor/Mazo.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class MazoTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin estado de instancia; cada caso crea su lista y mazo.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `MazoTests.CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal`

Crea siete cartas únicas, copia a una lista, construye Mazo y limpia la lista de origen. Count debe seguir en 7. Dos Repartir(3) deben retornar exactamente los slices [0..3] y [3..6] y dejar 4 y 1. Devolver concatena los seis al final; Repartir(7) debe producir carta[6] seguida de cartas[0..6] y vaciar. No invoca Mezclar: el orden esperado es determinista.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal()
    {
        var cartas = Enumerable.Range(0, 7).Select(_ => CartasDePrueba.Crear()).ToArray();
        var origen = cartas.ToList();
        var mazo = new Mazo(origen);
        origen.Clear();

        Assert.Equal(7, mazo.GetCantidad());
        var paraElPrimero = mazo.Repartir(3);
        Assert.Equal(cartas[..3], paraElPrimero);
        Assert.Equal(4, mazo.GetCantidad());

        var paraElSegundo = mazo.Repartir(3);
        Assert.Equal(cartas[3..6], paraElSegundo);
        Assert.Equal(1, mazo.GetCantidad());

        mazo.Devolver([.. paraElPrimero, .. paraElSegundo]);
        Assert.Equal(7, mazo.GetCantidad());
        Assert.Equal(new[] { cartas[6] }.Concat(cartas[..6]), mazo.Repartir(7));
        Assert.Equal(0, mazo.GetCantidad());
    }
```

### `MazoTests.MezclarConservaTodasLasCartas`

Theory con cantidades 0,1,6. Crea cantidad referencias, mezcla y exige mismo Count. Reparte todas y, para cada carta original, Assert.Single busca ReferenceEquals: no debe perder ni duplicar ninguna. No exige que el resultado cambie de orden, ni mide uniformidad estadística.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    public void MezclarConservaTodasLasCartas(int cantidad)
    {
        var cartas = Enumerable.Range(0, cantidad).Select(_ => CartasDePrueba.Crear()).ToList();
        var mazo = new Mazo(cartas);

        mazo.Mezclar();

        Assert.Equal(cantidad, mazo.GetCantidad());
        var repartidas = mazo.Repartir(cantidad);
        Assert.All(cartas, carta => Assert.Single(repartidas, repartida => ReferenceEquals(carta, repartida)));
    }
```

## Recorridos y ejemplos

Normal: Siete cartas: repartir 3 deja 4; repartir 3 deja 1; devolver seis deja 7 y el siguiente reparto empieza por la carta que nunca salió.

Alternativo o de error: Con 0 o 1 carta Mezclar conserva la misma colección sin requerir un intercambio. Las cantidades inválidas de Repartir no están ensayadas.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
