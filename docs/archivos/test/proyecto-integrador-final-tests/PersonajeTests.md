# PersonajeTests.cs

Fuente: [test/proyecto-integrador-final-tests/PersonajeTests.cs](../../../../test/proyecto-integrador-final-tests/PersonajeTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba conservación de datos y guardas de construcción del personaje local, incluidos los límites de atributos.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [Personaje](../../src/servidor/Personaje.md), [TipoAtributo](../../src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class PersonajeTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos ni recursos externos. Cada lambda de Assert.Throws construye una instancia descartada.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `PersonajeTests.ConservaNombreAsciiYLosSeisAtributos`

Construye Lysander con arte de barra y diagonal y valores 250,750,350,650,450,550; compara nombre, ASCII y el getter de cada constante de TipoAtributo. Acredita mapeo de los seis campos, no una comparación de combate.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ConservaNombreAsciiYLosSeisAtributos()
    {
        var carta = new Personaje("Lysander", " /\\", 250, 750, 350, 650, 450, 550);

        Assert.Equal("Lysander", carta.GetNombre());
        Assert.Equal(" /\\", carta.GetAscii());
        Assert.Equal(250, carta.GetAtributo(TipoAtributo.Fuerza));
        Assert.Equal(750, carta.GetAtributo(TipoAtributo.Velocidad));
        Assert.Equal(350, carta.GetAtributo(TipoAtributo.Resistencia));
        Assert.Equal(650, carta.GetAtributo(TipoAtributo.Inteligencia));
        Assert.Equal(450, carta.GetAtributo(TipoAtributo.Energia));
        Assert.Equal(550, carta.GetAtributo(TipoAtributo.Habilidad));
    }
```

### `PersonajeTests.AceptaUmbralesInclusivosYAsciiVacio`

Construye Carta con (650,350,500,500,500,500), exige arte vacío y getters fuerza 650 y velocidad 350. Demuestra inclusión de los dos umbrales, no todos los límites exteriores.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void AceptaUmbralesInclusivosYAsciiVacio()
    {
        var carta = new Personaje("Carta", "", 650, 350, 500, 500, 500, 500);

        Assert.Empty(carta.GetAscii());
        Assert.Equal(650, carta.GetAtributo(TipoAtributo.Fuerza));
        Assert.Equal(350, carta.GetAtributo(TipoAtributo.Velocidad));
    }
```

### `PersonajeTests.RechazaAtributosQueIncumplenUnaRegla`

Theory fija energía y habilidad en 500 y varía (fuerza,velocidad,resistencia,inteligencia): (649,350,501,500) carece de ≥650; (650,351,499,500) carece de ≤350; (650,350,499,500) suma 2999; (650,350,501,500) suma 3001; (750,249,501,500) y (751,250,499,500) salen de [250,750]; (500,500,500,500) no alcanza umbrales. Cada construcción debe lanzar ArgumentException; no se verifican mensaje ni nombre de parámetro.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(649, 350, 501, 500)]
    [InlineData(650, 351, 499, 500)]
    [InlineData(650, 350, 499, 500)]
    [InlineData(650, 350, 501, 500)]
    [InlineData(750, 249, 501, 500)]
    [InlineData(751, 250, 499, 500)]
    [InlineData(500, 500, 500, 500)]
    public void RechazaAtributosQueIncumplenUnaRegla(int fuerza, int velocidad, int resistencia, int inteligencia)
    {
        Assert.Throws<ArgumentException>(() =>
            new Personaje("Carta", "", fuerza, velocidad, resistencia, inteligencia, 500, 500));
    }
```

### `PersonajeTests.RechazaNombreSinTexto`

Theory con null, cadena vacía y tres espacios. La supresión ! solo evita advertencia estática: el null llega al constructor. Con atributos válidos y arte vacío espera ArgumentException; no comprueba trim ni longitud máxima.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RechazaNombreSinTexto(string? nombre)
    {
        Assert.Throws<ArgumentException>(() => new Personaje(nombre!, "", 650, 350, 500, 500, 500, 500));
    }
```

### `PersonajeTests.RechazaAsciiNulo`

Con nombre y atributos válidos pasa null! como arte y exige ArgumentException. No comprueba otros caracteres ni límites de tamaño del arte.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RechazaAsciiNulo()
    {
        Assert.Throws<ArgumentException>(() => new Personaje("Carta", null!, 650, 350, 500, 500, 500, 500));
    }
```

## Recorridos y ejemplos

Normal: (650,350,500,500,500,500) se acepta y ASCII vacío se conserva.

Alternativo o de error: Los seis valores 500 suman 3000 pero son rechazados por los umbrales; nombre null, vacío o espacios y ASCII null también se rechazan.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
