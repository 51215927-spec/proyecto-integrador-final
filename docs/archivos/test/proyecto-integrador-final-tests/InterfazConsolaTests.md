# InterfazConsolaTests.cs

Fuente: [test/proyecto-integrador-final-tests/InterfazConsolaTests.cs](../../../../test/proyecto-integrador-final-tests/InterfazConsolaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Ensaya selección, renderizado adaptable, mensajes y filtrado de controles con entrada/salida en memoria. No usa una terminal real.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartaVisible](../../src/cliente/VistasConsola.md), [InterfazConsola](../../src/cliente/InterfazConsola.md), [JugadorVisible](../../src/cliente/VistasConsola.md), [Ronda](../../src/servidor/Ronda.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class InterfazConsolaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos; auxiliar Carta produce modelos visibles. Cada writer/reader de los casos se descarta donde se declara using.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `InterfazConsolaTests.Carta`

private static: recibe id y nombre y devuelve CartaVisible con esos valores y diccionario fijo Fuerza750, Velocidad550, Resistencia500, Inteligencia250, Energía500, Habilidad450. No crea Personaje de dominio ni comprueba nombre.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static CartaVisible Carta(int id, string nombre) => new(id, nombre,
        new Dictionary<string, int>
        {
            ["Fuerza"] = 750, ["Velocidad"] = 550, ["Resistencia"] = 500,
            ["Inteligencia"] = 250, ["Energía"] = 500, ["Habilidad"] = 450
        });
```

### `InterfazConsolaTests.PedirCarta_ReintentaEntradasInvalidasYConservaLaMano`

Configura ancho30 y mano IDs15 Maga/30 Arquero. StringReader contiene 2, texto, l, Enter y sobra. PedirCarta debe retornar la segunda referencia, conservar longitud2 y dejar sobra sin leer. La salida debe mostrar foco > Arquero e Inteligencia. Así demuestra reintento y consumo exacto hasta confirmación, no selección por número.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PedirCarta_ReintentaEntradasInvalidasYConservaLaMano()
    {
        using var entrada = new StringReader("2\ntexto\nl\n\nsobra\n");
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(entrada, salida, ancho: 30);
        CartaVisible[] mano = [Carta(15, "Maga"), Carta(30, "Arquero")];

        Assert.Same(mano[1], interfaz.PedirCarta(mano));
        Assert.Equal(2, mano.Length);
        Assert.Equal("sobra", entrada.ReadLine());
        Assert.Contains("> Arquero", salida.ToString());
        Assert.Contains("Inteligencia", salida.ToString());
    }
```

### `InterfazConsolaTests.ManoVacia_SePuedeMostrarPeroNoSolicitar`

Con TextReader.Null, MostrarMano([]) debe imprimir No hay cartas disponibles. PedirCarta([]) debe lanzar InvalidOperationException antes de esperar input. No se prueba el arte ni ancho.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ManoVacia_SePuedeMostrarPeroNoSolicitar()
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida);

        interfaz.MostrarMano([]);

        Assert.Contains("No hay cartas disponibles", salida.ToString());
        Assert.Throws<InvalidOperationException>(() => interfaz.PedirCarta([]));
    }
```

### `InterfazConsolaTests.PedirAtributo_OfreceLosSeisAtributos`

Theory con pasos/esperado (0,Fuerza),(1,Velocidad),(2,Resistencia),(3,Inteligencia),(4,Energía),(5,Habilidad). Genera pasos líneas s seguidas de Enter; PedirAtributo sin lista explícita debe confirmar la posición correspondiente. No usa flechas reales ni valida otras teclas.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(0, "Fuerza")]
    [InlineData(1, "Velocidad")]
    [InlineData(2, "Resistencia")]
    [InlineData(3, "Inteligencia")]
    [InlineData(4, "Energía")]
    [InlineData(5, "Habilidad")]
    public void PedirAtributo_OfreceLosSeisAtributos(int pasos, string esperado)
    {
        var interfaz = new InterfazConsola(new StringReader(string.Concat(Enumerable.Repeat("s\n", pasos)) + "\n"), TextWriter.Null);

        Assert.Equal(esperado, interfaz.PedirAtributo());
    }
```

### `InterfazConsolaTests.PedirAtributo_RespetaLasOpcionesDeLaSolicitud`

Ofrece solo Habilidad e introduce 2 y Enter; la entrada no válida no agrega opciones y el resultado es Habilidad. No prueba una lista vacía ni todas las permutaciones.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PedirAtributo_RespetaLasOpcionesDeLaSolicitud()
    {
        var interfaz = new InterfazConsola(new StringReader("2\n\n"), TextWriter.Null);

        Assert.Equal("Habilidad", interfaz.PedirAtributo(["Habilidad"]));
    }
```

### `InterfazConsolaTests.FinDeEntrada_TerminaLaSolicitud`

Usa lector s seguido de EOF. PedirCarta con una carta debe lanzar por ausencia de confirmación; reutilizar el lector agotado para PedirAtributo también lanza EndOfStreamException. No se envía jugada de red.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void FinDeEntrada_TerminaLaSolicitud()
    {
        var interfaz = new InterfazConsola(new StringReader("s\n"), TextWriter.Null);

        Assert.Throws<EndOfStreamException>(() => interfaz.PedirCarta([Carta(1, "Maga")]));
        Assert.Throws<EndOfStreamException>(() => interfaz.PedirAtributo());
    }
```

### `InterfazConsolaTests.MostrarMano_AdaptaLasFichasAlAncho`

Theory (25,false),(100,true). Renderiza Maga con ESC y Arquero. Exige que ESC desaparezca y Habilidad siga presente. Comprueba si alguna línea contiene ambos nombres: solo debe ocurrir a ancho100. No asegura el ancho de cada carácter Unicode ni terminal específica.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(25, false)]
    [InlineData(100, true)]
    public void MostrarMano_AdaptaLasFichasAlAncho(int ancho, bool enParalelo)
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida, ancho);

        interfaz.MostrarMano([Carta(1, "Maga\u001b"), Carta(2, "Arquero")]);

        string resultado = salida.ToString();
        Assert.DoesNotContain('\u001b', resultado);
        Assert.Contains("Habilidad", resultado);
        Assert.Equal(enParalelo, resultado.Split('\n').Any(
            linea => linea.Contains("Maga") && linea.Contains("Arquero")));
    }
```

### `InterfazConsolaTests.MostrarCombate_PresentaElResultadoConfirmado`

Theory anchos30 y100. Envía duelo Fuerza de Maga/Arquero ambos750, ganador Mateo, marcador 0/1 y próximo elector Mateo. Exige atributo, ambas cartas, valor750, Ganador, Lucía0/Mateo1 y texto de elector. La vista no calcula el empate: recibe ganador ya confirmado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(30)]
    [InlineData(100)]
    public void MostrarCombate_PresentaElResultadoConfirmado(int ancho)
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida, ancho);

        interfaz.MostrarCombate(new("Fuerza", new("Lucía", "Maga", 750),
            new("Mateo", "Arquero", 750), "Mateo", 0, 1, "Mateo"));

        string resultado = salida.ToString();
        Assert.Contains("Fuerza", resultado);
        Assert.Contains("Maga", resultado);
        Assert.Contains("Arquero", resultado);
        Assert.Contains("750", resultado);
        Assert.Contains("Ganador", resultado);
        Assert.Contains("Lucía 0 / Mateo 1", resultado);
        Assert.Contains("Mateo elige el atributo", resultado);
    }
```

### `InterfazConsolaTests.PresentaMarcadorEstadosYCierresConLosNombresRecibidos`

Muestra Envío/Confirmación pendiente, dos esperas idénticas, ronda29 con Lucía14/Mateo15, ganadores de ronda/partida y error. Exige texto de estado, una sola línea de espera (deduplicación), marcador y nombres, y prefijo ERROR. No verifica colores ni un cierre de proceso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PresentaMarcadorEstadosYCierresConLosNombresRecibidos()
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida);
        var lucia = new JugadorVisible("Lucía", 14);
        var mateo = new JugadorVisible("Mateo", 15);

        interfaz.MostrarEstado("Envío", "Confirmación pendiente");
        interfaz.MostrarEsperando("Esperando a Mateo");
        interfaz.MostrarEsperando("Esperando a Mateo");
        interfaz.MostrarMarcador(lucia, mateo, 29);
        interfaz.MostrarGanadorRonda(mateo);
        interfaz.MostrarGanadorPartida(mateo);
        interfaz.MostrarError("Servidor inaccesible");

        string resultado = salida.ToString();
        Assert.Contains("Confirmación pendiente", resultado);
        Assert.Single(resultado.Split('\n').Where(linea => linea.Contains("Esperando a Mateo")));
        Assert.Contains("Ronda 29", resultado);
        Assert.Contains("Lucía 14 - 15 Mateo", resultado);
        Assert.Contains("Ganador de la ronda: Mateo", resultado);
        Assert.Contains("Ganador de la partida: Mateo", resultado);
        Assert.Contains("ERROR: Servidor inaccesible", resultado);
    }
```

## Recorridos y ejemplos

Normal: Con dos cartas y entrada 2/texto/l/Enter, las líneas numérica y de texto no confirman; l mueve foco a Arquero y Enter devuelve esa referencia.

Alternativo o de error: Mostrar mano vacía es válido pero pedir carta vacía lanza; EOF antes de confirmación lanza EndOfStreamException.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
