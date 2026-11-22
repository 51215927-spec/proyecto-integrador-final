# CombateTests.cs

Fuente: [test/proyecto-integrador-final-tests/CombateTests.cs](../../../../test/proyecto-integrador-final-tests/CombateTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba guardas, consumo de cartas, estado incompleto, consulta de datos y resolución con desempate del modelo local. No ejecuta HTTP ni incrementa rondas de jugadores.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](CartasDePrueba.md), [Combate](../../src/servidor/Combate.md), [Jugador](../../src/servidor/Jugador.md), [TipoAtributo](../../src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class CombateTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Dos campos privados readonly elector=Ana y rival=Bruno, nuevos por instancia/caso xUnit. Las cartas se agregan durante cada test; no hay recursos externos ni limpieza de archivos.

Declaraciones reales de estado de `CombateTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Jugador elector = new("Ana");
```

```csharp
private readonly Jugador rival = new("Bruno");
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `CombateTests.ValidaParticipantesYDevuelveUnaCopiaEnOrden`

Exige alguna excepción al construir con elector null, rival null o la misma referencia en ambos roles mediante Record.Exception (no fija tipo exacto). Construye combate válido, sustituye posición0 del array GetParticipantes y exige que nueva consulta conserve [elector,rival]. Prueba copia del contenedor y orden, no clonación de objetos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ValidaParticipantesYDevuelveUnaCopiaEnOrden()
    {
        Assert.NotNull(Record.Exception(() => new Combate(null!, rival)));
        Assert.NotNull(Record.Exception(() => new Combate(elector, null!)));
        Assert.NotNull(Record.Exception(() => new Combate(elector, elector)));
        var combate = new Combate(elector, rival);

        combate.GetParticipantes()[0] = rival;

        Assert.Equal(new[] { elector, rival }, combate.GetParticipantes());
    }
```

### `CombateTests.RechazaConsultasPrematurasYAtributosInvalidosORepetidos`

Combate nuevo no completo: consultar ganador, atributo o carta y Resolver deben arrojar alguna excepción. SetAtributo((TipoAtributo)99) también falla. Asigna Velocidad y comprueba getter; intentar Fuerza después falla sin reemplazar Velocidad. Sigue incompleto porque faltan cartas. Las aserciones no exigen mensajes ni tipos de excepción concretos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RechazaConsultasPrematurasYAtributosInvalidosORepetidos()
    {
        var combate = new Combate(elector, rival);

        Assert.False(combate.EstaCompleto());
        Assert.NotNull(Record.Exception(() => combate.GetGanador()));
        Assert.NotNull(Record.Exception(() => combate.GetAtributo()));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(elector)));
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
        Assert.NotNull(Record.Exception(() => combate.SetAtributo((TipoAtributo)99)));

        combate.SetAtributo(TipoAtributo.Velocidad);

        Assert.Equal(TipoAtributo.Velocidad, combate.GetAtributo());
        Assert.NotNull(Record.Exception(() => combate.SetAtributo(TipoAtributo.Fuerza)));
        Assert.Equal(TipoAtributo.Velocidad, combate.GetAtributo());
        Assert.False(combate.EstaCompleto());
    }
```

### `CombateTests.RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador`

Crea dos cartas para elector y una para rival y un jugador ajeno llamado también Ana (identidad distinta). SetCarta rechaza jugador null, carta null, ajeno y carta del rival; manos quedan intactas. Registrar primera carta elector la retira, segunda elección del mismo participante falla sin quitar la otra. Rival registra su carta y mano queda vacía, pero falta atributo: aún incompleto. Tras Fuerza está completo, GetCarta retorna mismas referencias y rechaza null/ajeno. Demuestra identidad por referencia y consumo después de validar.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador()
    {
        var combate = new Combate(elector, rival);
        var cartaElector = CartasDePrueba.Crear();
        var otraCartaElector = CartasDePrueba.Crear();
        var cartaRival = CartasDePrueba.Crear();
        var ajeno = new Jugador("Ana");
        ajeno.RecibirCartas([cartaElector]);
        elector.RecibirCartas([cartaElector, otraCartaElector]);
        rival.RecibirCartas([cartaRival]);

        Assert.NotNull(Record.Exception(() => combate.SetCarta(null!, cartaElector)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, null!)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(ajeno, cartaElector)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, cartaRival)));

        Assert.Equal(new[] { cartaElector, otraCartaElector }, elector.GetCartas());
        Assert.Same(cartaRival, Assert.Single(rival.GetCartas()));

        combate.SetCarta(elector, cartaElector);
        Assert.Same(otraCartaElector, Assert.Single(elector.GetCartas()));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, otraCartaElector)));
        Assert.Same(otraCartaElector, Assert.Single(elector.GetCartas()));

        combate.SetCarta(rival, cartaRival);
        Assert.Empty(rival.GetCartas());
        Assert.False(combate.EstaCompleto());

        combate.SetAtributo(TipoAtributo.Fuerza);
        Assert.True(combate.EstaCompleto());
        Assert.Same(cartaElector, combate.GetCarta(elector));
        Assert.Same(cartaRival, combate.GetCarta(rival));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(null!)));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(ajeno)));
    }
```

### `CombateTests.FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo`

Theory juegaElector false/true: elige participante, le da una carta, asigna Fuerza y registra solo esa. EstaCompleto debe false y Resolver debe lanzar alguna excepción. Verifica simétricamente que atributo+una carta no alcanza; no impone orden entre atributo y carta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo(bool juegaElector)
    {
        var combate = new Combate(elector, rival);
        var jugador = juegaElector ? elector : rival;
        var carta = CartasDePrueba.Crear();
        jugador.RecibirCartas([carta]);
        combate.SetAtributo(TipoAtributo.Fuerza);
        combate.SetCarta(jugador, carta);

        Assert.False(combate.EstaCompleto());
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
    }
```

### `CombateTests.ResuelveSegunElAtributoElegidoYDaElEmpateAlRival`

Theory (fuerzaElector650,fuerzaRival750,ganaElector true),(750,650,false),(650,650,false). CartasDePrueba fija Velocidad=1000−Fuerza, y el combate compara Velocidad, no Fuerza. Registra atributo/carta elector y exige incompleto; registra rival, resuelve y compara misma referencia al ganador esperado tanto en Resolver como GetGanador. Marcadores de rondas de ambos siguen0; una segunda resolución debe lanzar alguna excepción. Acredita mayor atributo y empate al rival sin sumar ronda automáticamente.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(650, 750, true)]
    [InlineData(750, 650, false)]
    [InlineData(650, 650, false)]
    public void ResuelveSegunElAtributoElegidoYDaElEmpateAlRival(int fuerzaElector, int fuerzaRival, bool ganaElector)
    {
        var combate = new Combate(elector, rival);
        var cartaElector = CartasDePrueba.Crear(fuerzaElector);
        var cartaRival = CartasDePrueba.Crear(fuerzaRival);
        elector.RecibirCartas([cartaElector]);
        rival.RecibirCartas([cartaRival]);

        combate.SetAtributo(TipoAtributo.Velocidad);
        combate.SetCarta(elector, cartaElector);
        Assert.False(combate.EstaCompleto());
        combate.SetCarta(rival, cartaRival);

        Jugador ganadorEsperado = ganaElector ? elector : rival;
        Assert.Same(ganadorEsperado, combate.Resolver());
        Assert.Same(ganadorEsperado, combate.GetGanador());
        Assert.Equal(0, elector.GetRondasGanadas());
        Assert.Equal(0, rival.GetRondasGanadas());
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
    }
```

## Recorridos y ejemplos

Normal: Con fuerza650 frente750 pero atributo Velocidad, gana elector: el auxiliar convierte a velocidades350/250.

Alternativo o de error: Con velocidades iguales gana rival; cartas ajenas o repetidas no deben consumirse y Resolver incompleto o repetido falla.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
