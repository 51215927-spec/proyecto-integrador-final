# ClienteConsolaContratoTests.cs

Fuente: [test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs](../../../../test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Contratos de salida del auxiliar DemostracionConsola conservado para enseñanza. Aunque el nombre dice contrato de consola, no arranca el ejecutable REST ni usa HTTP.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [DemostracionConsola](../../src/cliente/DemostracionConsola.md), [InterfazConsola](../../src/cliente/InterfazConsola.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ClienteConsolaContratoTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos; EjecutarDemo crea lector y escritor por llamada. Namespace global: el archivo no declara namespace.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ClienteConsolaContratoTests.Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince`

Ejecuta modo null con 16 saltos de línea. EnOrden exige mano, Lysander enfocado, confirmación, Fuerza enfocada, recepción, ganador, respuesta y cierres. Comprueba los seis nombres de atributos, Mateo15, dos apariciones de Atributo· y una solicitud de atributo. El marcador inicial de demo es histórico: no recorre quince rondas desde cero.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince()
    {
        var salida = await EjecutarDemo(null, new string('\n', 16));
        EnOrden(salida, "Tu mano", "> Lysander", "Enter para confirmar", "Atributo del combate", "> Fuerza",
            "Recepción confirmada", "Ganador", "Te toca responder", "Ganador",
            "gana la ronda", "gana la partida");
        Assert.Contains("Velocidad", salida);
        Assert.Contains("Resistencia", salida);
        Assert.Contains("Inteligencia", salida);
        Assert.Contains("Energía", salida);
        Assert.Contains("Habilidad", salida);
        Assert.Contains("Mateo 15", salida);
        Assert.Equal(2, salida.Split("Atributo · ", StringSplitOptions.None).Length - 1);
        Assert.Equal(1, salida.Split("Atributo del combate", StringSplitOptions.None).Length - 1);
    }
```

### `ClienteConsolaContratoTests.Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta`

Entrada w/s seguida de 16 Enter. Exige secuencia de foco Lysander→Reched, solicitud de atributo, recepción, Fuerza y Lucía·Reched. Es input redirigido, no flechas de pseudoterminal.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta()
    {
        var salida = await EjecutarDemo(null, "w\ns\n" + new string('\n', 16));
        EnOrden(salida, "> Lysander", "> Reched", "Atributo del combate",
            "Recepción confirmada", "Atributo · Fuerza", "Lucía · Reched");
    }
```

### `ClienteConsolaContratoTests.Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir`

Modo pausa y cinco Enter. Exige Sin comunicación, estado sin confirmar, reintentar o salir, último marcador Lucía14/Mateo14 y salida local con partida guardada. No simula transporte real ni verifica persistencia.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir()
    {
        var salida = await EjecutarDemo("pausa", "\n\n\n\n\n");
        EnOrden(salida, "Sin comunicación", "Estado actual sin confirmar",
            "reintentar o salir", "Último marcador confirmado", "Lucía 14 / Mateo 14",
            "Salida local", "La partida queda guardada");
    }
```

### `ClienteConsolaContratoTests.Demo_DistingueAbandonoCancelacionYSalidaLocal`

Llama tres veces con modos abandono/cancelacion/salida y cuatro Enter. Los dos primeros deben tener Final confirmado y aclaración sin sumar victoria; la salida conserva partida y no debe contener Final confirmado. Cada llamada tiene flujos nuevos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_DistingueAbandonoCancelacionYSalidaLocal()
    {
        var abandono = await EjecutarDemo("abandono", "\n\n\n\n");
        var cancelacion = await EjecutarDemo("cancelacion", "\n\n\n\n");
        var salida = await EjecutarDemo("salida", "\n\n\n\n");

        EnOrden(abandono, "Final confirmado", "abandonó", "sin sumar una victoria");
        EnOrden(cancelacion, "Final confirmado", "canceló", "sin sumar una victoria");
        EnOrden(salida, "Salida local", "La partida queda guardada");
        Assert.DoesNotContain("Final confirmado", salida);
    }
```

### `ClienteConsolaContratoTests.Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada`

Input vacío con modo normal. Exige mano, Enter para confirmar y Fin de la entrada en ese orden, sin Recepción confirmada. El texto EOF lo agrega el auxiliar al capturar EndOfStreamException.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada()
    {
        var salida = await EjecutarDemo(null, string.Empty);
        EnOrden(salida, "Tu mano", "Enter para confirmar", "Fin de la entrada");
        Assert.DoesNotContain("Recepción confirmada", salida);
    }
```

### `ClienteConsolaContratoTests.EnOrden`

private static void: salida y params fragmentos. Comienza índice0, busca cada fragmento con IndexOf ordinal a partir del fin del anterior. Assert.True exige índice≥0 e incluye salida en diagnóstico. Avanza por longitud del fragmento, evitando que una aparición anterior satisfaga un paso posterior; termina tras todos. No exige igualdad completa de pantalla.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static void EnOrden(string salida, params string[] fragmentos)
    {
        var inicio = 0;
        foreach (var fragmento in fragmentos)
        {
            var indice = salida.IndexOf(fragmento, inicio, StringComparison.Ordinal);
            Assert.True(indice >= 0, $"Falta «{fragmento}» después de la posición {inicio}.\n{salida}");
            inicio = indice + fragmento.Length;
        }
    }
```

### `ClienteConsolaContratoTests.EjecutarDemo`

private static Task<string>: con modo nullable e input crea StringReader/StringWriter e InterfazConsola. Ejecuta Recorrer con interactiva false; captura solo EndOfStreamException y agrega Fin de la entrada. Devuelve Task.FromResult del texto (no trabajo paralelo). using libera ambos flujos; otras excepciones fallan la prueba.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static Task<string> EjecutarDemo(string? modo, string entrada)
    {
        using var lector = new StringReader(entrada);
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(lector, salida);
        try { new DemostracionConsola(interfaz, lector, salida, false).Recorrer(modo); }
        catch (EndOfStreamException) { salida.WriteLine("Fin de la entrada"); }
        return Task.FromResult(salida.ToString());
    }
```

## Recorridos y ejemplos

Normal: Con 16 Enter la demostración muestra selección de carta/atributo, recepción, dos combates y victoria de Mateo15.

Alternativo o de error: EOF inicial termina sin recepción; modos pausa, abandono, cancelacion y salida muestran mensajes distintos sin atribuir victoria de juego.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
