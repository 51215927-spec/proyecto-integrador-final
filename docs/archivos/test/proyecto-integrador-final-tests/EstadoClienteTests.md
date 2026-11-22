# EstadoClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/EstadoClienteTests.cs](../../../../test/proyecto-integrador-final-tests/EstadoClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Verifica aplicación de presentaciones y cola local de resultados sin transporte. Los auxiliares Vista y Resultado son fixtures de contrato, no datos validados por el servidor.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [EstadoCliente](../../src/cliente/EstadoCliente.md), [FalloCliente](../../src/cliente/ClientePartidaHttp.md), [PresentacionCliente](../../src/cliente/ContratosPartida.md), [ResultadoCliente](../../src/cliente/ContratosPartida.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class EstadoClienteTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos de fixture. Cada EstadoCliente conserva su vista y cola solo dentro del caso.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `EstadoClienteTests.PrimeraVistaOmiteHistoriaYReconectarConservaResultadosNuevosEnOrden`

Crea partida, estado, resultado anterior (ronda1/combate1) y nuevo (1/2). Aplica anterior y exige cola vacía. Marca pérdida y aplica nuevo/anterior, luego anterior/nuevo; Assert.Single debe ser la misma referencia nuevo y Actual debe ser true. Prueba deduplicación e historia inicial, no un reinicio de proceso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PrimeraVistaOmiteHistoriaYReconectarConservaResultadosNuevosEnOrden()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        var anterior = Resultado(1, 1);
        var nuevo = Resultado(1, 2);
        estado.Aplicar(Vista(partida) with { Resultados = [anterior] });
        Assert.Empty(estado.Resultados);
        estado.PerderComunicacion();
        estado.Aplicar(Vista(partida) with { Resultados = [nuevo, anterior] });
        estado.Aplicar(Vista(partida) with { Resultados = [anterior, nuevo] });
        Assert.Same(nuevo, Assert.Single(estado.Resultados));
        Assert.True(estado.Actual);
    }
```

### `EstadoClienteTests.PausaYNombresSeAplicanSinCambiarRevisionYErroresNoDestruyenUltimoEstado`

Aplica vista inicial y luego record con pausa y nombres J1=Ana/J2=Ada sin aumentar revisión; exige que Vista sea esa misma instancia. PerderComunicacion baja Actual pero conserva vista. Aplicar copia con fase desconocida lanza FalloCliente y no sustituye la vista pausada.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PausaYNombresSeAplicanSinCambiarRevisionYErroresNoDestruyenUltimoEstado()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        var inicial = Vista(partida);
        estado.Aplicar(inicial);
        var pausa = inicial with { Pausada = true, Nombres = new() { ["J1"] = "Ana", ["J2"] = "Ada" } };
        estado.Aplicar(pausa);
        Assert.Same(pausa, estado.Vista);
        estado.PerderComunicacion();
        Assert.False(estado.Actual);
        Assert.Same(pausa, estado.Vista);
        Assert.Throws<FalloCliente>(() => estado.Aplicar(pausa with { Fase = "desconocida" }));
        Assert.Same(pausa, estado.Vista);
    }
```

### `EstadoClienteTests.ManoVaciaEsValidaEnEsperaPeroNoConOpcionesImposibles`

Aplica Vista de espera con mano vacía sin excepción. Después agrega una solicitud tipo carta con ID permitido 999, conservando Fase=esperandoInicio y mano vacía; Aplicar debe lanzar FalloCliente. La validación rechaza primero que exista solicitud en esa fase no electora, antes de inspeccionar los IDs permitidos. Este caso no aísla la guarda de pertenencia de 999 a la mano y no observa un rechazo HTTP.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ManoVaciaEsValidaEnEsperaPeroNoConOpcionesImposibles()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        estado.Aplicar(Vista(partida));
        Assert.Throws<FalloCliente>(() => estado.Aplicar(Vista(partida) with
        {
            Solicitud = new(Guid.NewGuid(), "carta", [999], [])
        }));
    }
```

### `EstadoClienteTests.Vista`

Auxiliar internal static retorna PresentacionCliente con id recibido, revisión1, fase esperandoInicio, sin pausa, rol J1, nombres Ana/null, ronda/combate null, marcador 0/0, mano/resultados vacíos, solicitud y cierre null. Cada llamada crea un record nuevo; no hace I/O.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal static PresentacionCliente Vista(Guid id) => new(id, 1, "esperandoInicio", false, "J1",
        new() { ["J1"] = "Ana", ["J2"] = null }, null, null, new() { ["J1"] = 0, ["J2"] = 0 }, [], null, [], null, null);
```

### `EstadoClienteTests.Resultado`

Auxiliar internal static retorna resultado para ronda y combate recibidos: cartas J1=(1,A) y J2=(2,B) con mapas vacíos, Fuerza, valores 10/20, ganador J2 y sin ganador de ronda. Es un fixture deliberadamente mínimo para cola/presentación, no una carta de dominio válida.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal static ResultadoCliente Resultado(int ronda, int combate) => new(ronda, combate,
        new() { ["J1"] = new(1, "A", new Dictionary<string, int>()), ["J2"] = new(2, "B", new Dictionary<string, int>()) },
        "Fuerza", new() { ["J1"] = 10, ["J2"] = 20 }, "J2", null);
```

## Recorridos y ejemplos

Normal: Primera vista con resultado (1,1) omite historia; después de corte llega (1,2) y la cola lo contiene una sola vez pese al orden recibido.

Alternativo o de error: Una fase desconocida debe lanzar FalloCliente y conservar la última vista; una solicitud de carta 999 sin esa carta es inválida.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
