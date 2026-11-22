# RondaTests.cs

Fuente: [test/proyecto-integrador-final-tests/RondaTests.cs](../../../../test/proyecto-integrador-final-tests/RondaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba elección inicial, alternancia por combate y cierre al alcanzar dos victorias, con combates reales del modelo ya resueltos.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](CartasDePrueba.md), [Combate](../../src/servidor/Combate.md), [Jugador](../../src/servidor/Jugador.md), [Ronda](../../src/servidor/Ronda.md), [TipoAtributo](../../src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class RondaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Campos privados readonly mano=Ana y rival=Bruno, nuevos por caso; el auxiliar agrega cartas y los combates las consumen.

Declaraciones reales de estado de `RondaTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Jugador mano = new("Ana");
```

```csharp
private readonly Jugador rival = new("Bruno");
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `RondaTests.EmpiezaSinCombatesYEligeElJugadorMano`

Construye Ronda(mano,rival); exige GetCombates vacío, EstaDefinida false y que el próximo elector sea la misma referencia mano. No consulta GetGanador antes de tiempo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void EmpiezaSinCombatesYEligeElJugadorMano()
    {
        var ronda = new Ronda(mano, rival);

        Assert.Empty(ronda.GetCombates());
        Assert.False(ronda.EstaDefinida());
        Assert.Same(mano, ronda.GetQuienEligeProximo());
    }
```

### `RondaTests.DosVictoriasSeguidasCierranLaRonda`

Theory true/false elige como ganador mano o rival. Agrega primer combate resuelto y exige ronda abierta y próximo elector rival. El segundo invierte elector pero conserva vencedor; exige cierre y ganador por referencia. Reemplaza una posición del array devuelto por GetCombates y confirma que la lista interna conserva primero/segundo: copia defensiva.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DosVictoriasSeguidasCierranLaRonda(bool ganaMano)
    {
        var ronda = new Ronda(mano, rival);
        var ganador = ganaMano ? mano : rival;
        var primero = ResolverCombate(mano, rival, ganador);
        ronda.AgregarCombate(primero);

        Assert.False(ronda.EstaDefinida());
        Assert.Same(rival, ronda.GetQuienEligeProximo());

        var segundo = ResolverCombate(rival, mano, ganador);
        ronda.AgregarCombate(segundo);

        Assert.True(ronda.EstaDefinida());
        Assert.Same(ganador, ronda.GetGanador());
        ronda.GetCombates()[0] = segundo;
        Assert.Equal(new[] { primero, segundo }, ronda.GetCombates());
    }
```

### `RondaTests.ElTercerCombateDefineLaRondaCuandoVanUnoAUno`

Theory true/false. Fuerza primer triunfo de mano y segundo de rival; exige ronda abierta y elector mano. Agrega tercer combate con vencedor elegido por parámetro y exige cierre, ganador idéntico y longitud tres. No acredita marcador de Partida.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ElTercerCombateDefineLaRondaCuandoVanUnoAUno(bool ganaMano)
    {
        var ronda = new Ronda(mano, rival);
        ronda.AgregarCombate(ResolverCombate(mano, rival, mano));
        ronda.AgregarCombate(ResolverCombate(rival, mano, rival));

        Assert.False(ronda.EstaDefinida());
        Assert.Same(mano, ronda.GetQuienEligeProximo());

        var ganador = ganaMano ? mano : rival;
        ronda.AgregarCombate(ResolverCombate(mano, rival, ganador));

        Assert.True(ronda.EstaDefinida());
        Assert.Same(ganador, ronda.GetGanador());
        Assert.Equal(3, ronda.GetCombates().Length);
    }
```

### `RondaTests.ResolverCombate`

Auxiliar private static: recibe elector, oponente y ganador esperado, crea carta de fuerza 750 para el participante idéntico al ganador y 650 para el otro. Agrega cada carta a su jugador, selecciona Fuerza, registra ambas, resuelve y devuelve Combate. Asume que ganador es uno de los dos; no lo valida. La comparación por referencia determina las dos ramas de asignación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static Combate ResolverCombate(Jugador elector, Jugador oponente, Jugador ganador)
    {
        var cartaElector = CartasDePrueba.Crear(ReferenceEquals(elector, ganador) ? 750 : 650);
        var cartaOponente = CartasDePrueba.Crear(ReferenceEquals(oponente, ganador) ? 750 : 650);
        elector.RecibirCartas([cartaElector]);
        oponente.RecibirCartas([cartaOponente]);

        var combate = new Combate(elector, oponente);
        combate.SetAtributo(TipoAtributo.Fuerza);
        combate.SetCarta(elector, cartaElector);
        combate.SetCarta(oponente, cartaOponente);
        combate.Resolver();
        return combate;
    }
```

## Recorridos y ejemplos

Normal: Dos victorias de Ana cierran la ronda tras el segundo combate; si quedan uno a uno, el tercero decide.

Alternativo o de error: Cada cierre se parametriza para que también gane Bruno. No hay pruebas de agregar combate no resuelto ni participantes extraños.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
