# PresentacionRedTests.cs

Fuente: [test/proyecto-integrador-final-tests/PresentacionRedTests.cs](../../../../test/proyecto-integrador-final-tests/PresentacionRedTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Verifica que la vista de red reproduce el marcador histórico del resultado leído y neutraliza controles del nombre remoto.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [EstadoClienteTests](EstadoClienteTests.md), [InterfazConsola](../../src/cliente/InterfazConsola.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class PresentacionRedTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos; fixtures de EstadoClienteTests y records with producen datos locales sin HTTP.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `PresentacionRedTests.MarcadorDelResultadoCorrespondeASuRondaYNeutralizaTextoRemoto`

Theory anchos38/100. Construye resultado (1,1), cierre (1,2) con ganador ronda J2 y posterior (2,1) J1. Presentación actual ronda2/combate2 y marcador7/9, nombre remoto con ESC. MostrarRed recibe explícitamente cierre, sin selección (-1) ni menús. Exige gana la ronda y Ada1; excluye Ada9, ESC y Tu turno. Acredita reconstrucción histórica y sanitización, no resultado calculado por servidor.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(38)]
    [InlineData(100)]
    public void MarcadorDelResultadoCorrespondeASuRondaYNeutralizaTextoRemoto(int ancho)
    {
        Guid id = Guid.NewGuid();
        var primero = EstadoClienteTests.Resultado(1, 1);
        var cierre = EstadoClienteTests.Resultado(1, 2) with { GanadorRonda = "J2" };
        var posterior = EstadoClienteTests.Resultado(2, 1) with { GanadorCombate = "J1" };
        var vista = EstadoClienteTests.Vista(id) with
        {
            NumeroRonda = 2, NumeroCombate = 2, Marcador = new() { ["J1"] = 7, ["J2"] = 9 },
            Nombres = new() { ["J1"] = "Ana\u001b[31m", ["J2"] = "Ada" }, Resultados = [primero, cierre, posterior]
        };
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader(""), salida, ancho);
        interfaz.MostrarRed("Ana", vista, true, "En línea", cierre, -1, false, 0, false, false, false);
        string texto = salida.ToString();
        Assert.Contains("gana la ronda", texto);
        Assert.Contains("Ada 1", texto);
        Assert.DoesNotContain("Ada 9", texto);
        Assert.DoesNotContain('\u001b', texto);
        Assert.DoesNotContain("Tu turno", texto);
    }
```

## Recorridos y ejemplos

Normal: Resultados de ronda1 con cierre J2 y posterior de ronda2: mostrar el cierre de ronda1 debe decir Ada1, aunque la presentación actual lleva Ada9.

Alternativo o de error: Con nombre Ana seguido de ESC[31m no debe salir ESC; mostrar resultado no habilita Tu turno. Se ensayan anchos38 y100.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
