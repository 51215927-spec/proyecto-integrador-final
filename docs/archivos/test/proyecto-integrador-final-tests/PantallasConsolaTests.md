# PantallasConsolaTests.cs

Fuente: [test/proyecto-integrador-final-tests/PantallasConsolaTests.cs](../../../../test/proyecto-integrador-final-tests/PantallasConsolaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba composición de paneles de preparación, turno, envío, pausa y cierres usando una mesa y tres cartas visibles fijas.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartaVisible](../../src/cliente/VistasConsola.md), [Combate](../../src/servidor/Combate.md), [EstadoEnvio](../../src/cliente/VistasConsola.md), [InterfazConsola](../../src/cliente/InterfazConsola.md), [Jugador](../../src/servidor/Jugador.md), [MesaVisible](../../src/cliente/VistasConsola.md), [Ronda](../../src/servidor/Ronda.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class PantallasConsolaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Mesa static readonly: local Lucía, marcador4/3, ronda8, combate1 y combates0/0. Cartas static readonly: Lysander fuerza700/velocidad550, Reched350/700 y Vigía500/350. No se mutan en los casos; with crea copias de Mesa.

Declaraciones reales de estado de `PantallasConsolaTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private static readonly MesaVisible Mesa = new("Lucía", new("Lucía", 4), new("Mateo", 3), 8, 1, 0, 0);
```

```csharp
private static readonly CartaVisible[] Cartas = [
        new(1, "Lysander", new Dictionary<string, int> { ["Fuerza"] = 700, ["Velocidad"] = 550 }),
        new(2, "Reched", new Dictionary<string, int> { ["Fuerza"] = 350, ["Velocidad"] = 700 }),
        new(3, "Vigía", new Dictionary<string, int> { ["Fuerza"] = 500, ["Velocidad"] = 350 })
    ];
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `PantallasConsolaTests.PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles`

Dibuja preparación de Lucía, espera inicial con Mateo ausente e inicio de ronda. Exige etiquetas Jugador local, Estado, Participantes, A cargo del operador, apertura de Lucía, mano y rondas ganadas. Solo inspecciona texto, no red ni cambios de fase.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarPreparacion("Lucía", "Preparando la partida…");
        interfaz.MostrarEsperaInicial("Lucía", "Lucía presente · Mateo por incorporarse", "Falta Mateo");
        interfaz.MostrarInicioRonda(Mesa, "Lucía", Cartas);

        string salida = texto.ToString();
        Assert.Contains("Jugador local", salida);
        Assert.Contains("Estado", salida);
        Assert.Contains("Participantes", salida);
        Assert.Contains("A cargo del operador", salida);
        Assert.Contains("Lucía abre la ronda", salida);
        Assert.Contains("Tu mano", salida);
        Assert.Contains("Rondas ganadas", salida);
    }
```

### `PantallasConsolaTests.Apertura_UsaMesaConFichasEncabezadoYPie`

Theory (100,true),(35,false). Lee d y Enter en PedirCarta con abre true; retorna Cartas[1]. Exige encabezado GUERRA/Lucía, ronda8/combate1, mano, Tu turno, aviso de atributo posterior y borde. Any sobre líneas identifica si Lysander/Reched están juntos según ancho. La comparación de aviso normaliza saltos de línea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(100, true)]
    [InlineData(35, false)]
    public void Apertura_UsaMesaConFichasEncabezadoYPie(int ancho, bool paralelas)
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader("d\n\n"), texto, ancho);

        Assert.Same(Cartas[1], interfaz.PedirCarta(Mesa, Cartas, abre: true));

        string salida = texto.ToString();
        Assert.Contains("GUERRA / Lucía", salida);
        Assert.Contains("Ronda 8 · Combate 1", salida);
        Assert.Contains("Tu mano", salida);
        Assert.Contains("Tu turno", salida);
        Assert.Contains("Después elegís el atributo", salida.ReplaceLineEndings(" "));
        Assert.Contains("┌", salida);
        Assert.Equal(paralelas, salida.Split('\n').Any(l => l.Contains("Lysander") && l.Contains("Reched")));
    }
```

### `PantallasConsolaTests.EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo`

Dibuja espera por Mateo y pide respuesta con abre false, confirmando primera carta por Enter. Exige mensajes de espera, disponibles, Fuerza/Velocidad, foco y marcador; excluye Atributo del combate y aviso de elegir atributo después. Demuestra distinción visual entre apertura y respuesta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader("\n"), texto, 100);

        interfaz.MostrarEsperaRival(Mesa, "Mateo", Cartas);
        Assert.Same(Cartas[0], interfaz.PedirCarta(Mesa, Cartas, abre: false));

        string salida = texto.ToString();
        Assert.Contains("Mateo está eligiendo", salida);
        Assert.Contains("Esperá la solicitud para responder", salida);
        Assert.Contains("Cartas disponibles", salida);
        Assert.Contains("Fuerza", salida);
        Assert.Contains("Velocidad", salida);
        Assert.Contains("> Lysander", salida);
        Assert.Contains("Te toca responder", salida);
        Assert.Contains("Marcador", salida);
        Assert.DoesNotContain("Atributo del combate", salida);
        Assert.DoesNotContain("Después elegís el atributo", salida);
    }
```

### `PantallasConsolaTests.EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados`

Renderiza los tres EstadoEnvio, dos suspensiones (confirmada true y false) y dos interrupciones (remota confirmada y salida local). Recorre ocho textos esperados: Envío en curso, Confirmación pendiente, Recepción confirmada, Pausa confirmada, Estado actual sin confirmar, Último marcador confirmado, Final confirmado y Salida local. No comprueba que la comunicación haya ocurrido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Enviando);
        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Pendiente);
        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Recibido);
        interfaz.MostrarSuspension(Mesa, "Mateo perdió la sesión", true);
        interfaz.MostrarSuspension(Mesa, "No se pudo contactar al servidor", false);
        interfaz.MostrarInterrupcion("Lucía", "Mateo abandonó la partida", "La partida terminó", true);
        interfaz.MostrarInterrupcion("Lucía", "Cierre solicitado", "La partida queda guardada", false);

        string salida = texto.ToString();
        foreach (string esperado in new[] { "Envío en curso", "Confirmación pendiente", "Recepción confirmada",
            "Pausa confirmada", "Estado actual sin confirmar", "Último marcador confirmado", "Final confirmado", "Salida local" })
            Assert.Contains(esperado, salida);
    }
```

### `PantallasConsolaTests.ResultadoYRondas_UsanDueloVictoriaYPlanilla`

Muestra combate Fuerza Lysander700 frente Reched350 con Lucía vencedora; cierra ronda con combates2/1 y próxima mano Mateo; cierra partida Lucía15/Mateo11. Exige cartas, atributo, ganador y próximo elector, ronda9, victoria de Lucía, ambos totales y Enter para salir. No acredita la lógica de puntuación: son datos de presentación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ResultadoYRondas_UsanDueloVictoriaYPlanilla()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarCombate(Mesa, new("Fuerza", new("Lucía", "Lysander", 700),
            new("Mateo", "Reched", 350), "Lucía", 1, 0, "Mateo"));
        interfaz.MostrarCierreRonda(Mesa with { CombatesJ1 = 2, CombatesJ2 = 1 }, "Lucía", "Mateo");
        interfaz.MostrarCierrePartida("Lucía", "Lucía", new("Lucía", 15), new("Mateo", 11));

        string salida = texto.ToString();
        Assert.Contains("Atributo · Fuerza", salida);
        Assert.Contains("Lucía · Lysander", salida);
        Assert.Contains("Mateo · Reched", salida);
        Assert.Contains("Ganador", salida);
        Assert.Contains("Mateo elige el atributo", salida);
        Assert.Contains("Lucía gana la ronda", salida);
        Assert.Contains("Mateo abre la ronda 9", salida);
        Assert.Contains("Lucía gana la partida", salida);
        Assert.Contains("15 rondas ganadas", salida);
        Assert.Contains("11 rondas ganadas", salida);
        Assert.Contains("Pulsá Enter para salir", salida);
    }
```

## Recorridos y ejemplos

Normal: En ronda8/combate1, d mueve el foco a Reched y Enter la confirma; ancho100 coloca fichas en paralelo.

Alternativo o de error: A ancho35 las fichas no comparten línea; al responder no se solicita atributo. La suspensión no confirmada usa un mensaje diferente de la pausa confirmada.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
