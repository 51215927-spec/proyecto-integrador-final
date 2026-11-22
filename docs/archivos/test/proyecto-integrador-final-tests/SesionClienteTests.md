# SesionClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/SesionClienteTests.cs](../../../../test/proyecto-integrador-final-tests/SesionClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Ensaya el coordinador asíncrono del cliente con transporte controlado, entrada por canal y reloj sustituible. Permite observar concurrencia, recuperación y navegación sin servidor ni SQLite reales.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](../../src/cliente/ClientePartidaHttp.md), [ConfiguracionCliente](../../src/cliente/ConfiguracionCliente.md), [EleccionCliente](../../src/cliente/ContratosPartida.md), [EntradaCliente](../../src/cliente/EntradaCliente.md), [EstadoClienteTests](EstadoClienteTests.md), [InterfazConsola](../../src/cliente/InterfazConsola.md), [PresentacionCliente](../../src/cliente/ContratosPartida.md), [ReciboCliente](../../src/cliente/ContratosPartida.md), [SesionConfirmada](../../src/cliente/ContratosPartida.md), [SesionPartidaCliente](../../src/cliente/SesionPartidaCliente.md), [TransporteClienteTests](TransporteClienteTests.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class SesionClienteTests`.

Declaración: `internal sealed class Lineas : TextReader`.

Declaración: `internal sealed class Escenario : IAsyncDisposable`.

Declaración: `internal sealed class RelojCliente : TimeProvider`.

Declaración: `private sealed class Temporizador(RelojCliente reloj, TimerCallback callback, object? state) : ITimer`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Lineas internal sealed deriva TextReader y posee Channel<string?> ilimitado; null puede simular EOF. Escenario internal sealed implementa IAsyncDisposable: Reloj readonly, Vista mutable, Entrada readonly, contadores Consultas/Presencias/Elecciones/Recibos/Abandonos inicialmente0, Perdida nullable y función Interceptar nullable, lista Cuerpos, recibo nullable, CTS cierre, StringWriter salida, transporte http y `Task<bool> Tarea`. Texto proyecta writer sin bloqueo propio. RelojCliente internal sealed deriva TimeProvider: ahora=UtcNow inicial y lista de Temporizador; ambos se consultan bajo lock de lista. Temporizador privado con constructor primario captura reloj/callback/state e implementa ITimer, guarda Cerrado inicialmente false, Vence, Callback y State; no modela periodicidad automáticamente.

Declaraciones reales de estado de `Lineas` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Channel<string?> canal = Channel.CreateUnbounded<string?>();
```

Declaraciones reales de estado de `Escenario` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal readonly RelojCliente Reloj = new();
```

```csharp
internal PresentacionCliente Vista;
```

```csharp
internal readonly Lineas Entrada = new();
```

```csharp
internal int Consultas, Presencias, Elecciones, Recibos, Abandonos;
```

```csharp
internal string? Perdida;
```

```csharp
internal Func<HttpRequestMessage, Task<HttpResponseMessage?>>? Interceptar;
```

```csharp
internal readonly List<string> Cuerpos = [];
```

```csharp
private ReciboCliente? recibo;
```

```csharp
private readonly CancellationTokenSource cierre = new();
```

```csharp
private readonly StringWriter salida = new();
```

```csharp
private readonly ClientePartidaHttp http;
```

```csharp
internal Task<bool> Tarea;
```

```csharp
internal string Texto => salida.ToString();
```

Declaraciones reales de estado de `RelojCliente` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private DateTimeOffset ahora = DateTimeOffset.UtcNow;
```

```csharp
private readonly List<Temporizador> temporizadores = [];
```

Declaraciones reales de estado de `Temporizador` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal bool Cerrado;
```

```csharp
internal DateTimeOffset Vence;
```

```csharp
internal TimerCallback Callback = callback;
```

```csharp
internal object? State = state;
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `SesionClienteTests.ConsultaYPresenciaContinuanEnMenuYResultadoSinCambiarRevision`

Escenario inicial espera inicio; abre menú con esc y espera salida. Avanza reloj5s y exige consultas nuevas/presencia. Aplica pausa sin variar revisión y avanza1s para mostrarla. Cierra menú, agrega resultado y quita pausa, avanza1s y espera Enter para continuar. Otros5s deben aumentar consultas y presencias durante lectura; Elecciones queda0. Muestra tareas de fondo independientes de navegación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task ConsultaYPresenciaContinuanEnMenuYResultadoSinCambiarRevision()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Texto.Contains("Esperando inicio"));
        f.Tecla("esc");
        await f.Esperar(() => f.Texto.Contains("Salir sin abandonar"));
        int antes = f.Consultas;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => f.Consultas > antes && f.Presencias > 0);
        f.Vista = f.Vista with { Pausada = true };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Pausa confirmada"));
        f.Tecla("esc");
        await f.Esperar(() => f.Texto.Contains("Enter: volver"));
        f.Vista = f.Vista with { Pausada = false, Resultados = [EstadoClienteTests.Resultado(1, 1)] };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Enter para continuar"));
        antes = f.Consultas;
        int presencias = f.Presencias;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => f.Consultas > antes && f.Presencias > presencias);
        Assert.Equal(0, f.Elecciones);
    }
```

### `SesionClienteTests.RespuestaPerdidaConsultaReciboYReenviaSoloElMismoCuerpo`

Theory aplicado true/false. Prepara turno y Perdida despues o antes; confirma carta y atributo, espera primer envío. Avanza1s y espera consulta de recibo. Si no aplicado, avanza otro1s, exige segundo envío y cuerpos[0]==cuerpos[1]. Siempre espera recepción confirmada y total1/2 según parámetro. Es un doble de commit, no prueba transacción SQLite (integración separada).

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RespuestaPerdidaConsultaReciboYReenviaSoloElMismoCuerpo(bool aplicado)
    {
        await using var f = new Escenario();
        f.PrepararTurno();
        f.Perdida = aplicado ? "despues" : "antes";
        await f.Esperar(() => f.Texto.Contains("Tu turno"));
        f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("Atributo del combate"));
        f.Tecla("");
        await f.Esperar(() => f.Elecciones == 1);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Recibos > 0);
        if (!aplicado)
        {
            f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
            await f.Esperar(() => f.Elecciones == 2);
            Assert.Equal(f.Cuerpos[0], f.Cuerpos[1]);
        }
        await f.Esperar(() => f.Texto.Contains("Recepción confirmada"));
        Assert.Equal(aplicado ? 1 : 2, f.Elecciones);
    }
```

### `SesionClienteTests.EnterDeResultadoNoEligeTurnoSiguienteYFinDetienePresencia`

Tras primera consulta prepara turno y resultado nuevo; al avanzar1s muestra resultado. Enter debe llevar a Tu turno con Elecciones0, sin confirmar carta automáticamente. Cambia vista a finalizada/cancelacion, avanza1s y espera mensaje del operador. Guarda contadores; tras20s simulados y100ms reales deben mantenerse. Enter final termina Tarea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task EnterDeResultadoNoEligeTurnoSiguienteYFinDetienePresencia()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        f.PrepararTurno();
        f.Vista = f.Vista with { Resultados = [EstadoClienteTests.Resultado(1, 1)] };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Enter para continuar"));
        f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("Tu turno"));
        Assert.Equal(0, f.Elecciones);
        f.Vista = f.Vista with { Fase = "finalizada", Solicitud = null, MotivoFin = "cancelacion" };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("El operador canceló"));
        int presencias = f.Presencias, consultas = f.Consultas;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(20));
        await Task.Delay(100);
        Assert.Equal(presencias, f.Presencias);
        Assert.Equal(consultas, f.Consultas);
        f.Tecla("");
        await f.Esperar(() => f.Tarea.IsCompleted);
    }
```

### `SesionClienteTests.AbandonoExigeSiYSalidaLocalNoLoEnvia`

Espera consulta, abre confirmación de abandono y exige Abandonos0. Enter sobre No por defecto mantiene0 tras80ms. Reabre, mueve s a Sí y Enter debe llevar Abandonos1. No tiene rama que seleccione explícitamente Salir sin abandonar pese al nombre; su garantía directa es confirmación No/Sí.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task AbandonoExigeSiYSalidaLocalNoLoEnvia()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        await f.AbrirAbandono();
        await f.Esperar(() => f.Texto.Contains("¿Abandonar partida?"));
        Assert.Equal(0, f.Abandonos);
        f.Tecla(""); // No por defecto
        await Task.Delay(80);
        Assert.Equal(0, f.Abandonos);
        await f.AbrirAbandono();
        await f.TeclaConPantalla("s");
        f.Tecla("");
        await f.Esperar(() => f.Abandonos == 1);
    }
```

### `SesionClienteTests.UnaConsultaLentaNoSolapaConsultasNiRetrasaPresencia`

Intercepta presentación y devuelve TaskCompletionSource pendiente, contando iniciadas con Interlocked. Avanza1s hasta una consulta y10s hasta presencia; iniciadas debe seguir1, sin solapamiento de consultas. Quita interceptor, libera respuesta JSON actual y espera Consultas>1. No bloquea renovación mientras hay lectura HTTP pendiente.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task UnaConsultaLentaNoSolapaConsultasNiRetrasaPresencia()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        var retrasada = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        int iniciadas = 0;
        f.Interceptar = req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/presentacion"))
            { Interlocked.Increment(ref iniciadas); return retrasada.Task.ContinueWith<HttpResponseMessage?>(t => t.Result); }
            return Task.FromResult<HttpResponseMessage?>(null);
        };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => iniciadas == 1);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(10));
        await f.Esperar(() => f.Presencias > 0);
        Assert.Equal(1, iniciadas);
        f.Interceptar = null;
        retrasada.SetResult(Escenario.Json(f.Vista));
        await f.Esperar(() => f.Consultas > 1);
    }
```

### `SesionClienteTests.SesionVencidaReincorporaConMismoUuidYNoReproduceHistoria`

Interceptor hace fallar primera presentación con409 sesionVencida; en PUT sesión lee JSON, cuenta incorporaciones y exige idSesion igual a X-Sesion. Avanza reloj y espera reincorporación/consulta posterior; UUID capturado no null y no aparece Enter para continuar. Demuestra consistencia cuerpo/cabecera, no compara aquí una variable guardada de la incorporación inicial.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task SesionVencidaReincorporaConMismoUuidYNoReproduceHistoria()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        string? sesion = null;
        bool vencer = true;
        int incorporaciones = 0;
        f.Interceptar = async req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/presentacion") && vencer)
            { vencer = false; return Escenario.Json(new { codigo = "sesionVencida" }, HttpStatusCode.Conflict); }
            if (req.RequestUri.AbsolutePath.EndsWith("/sesion"))
            {
                incorporaciones++;
                var cuerpo = JsonDocument.Parse(await req.Content!.ReadAsStringAsync());
                sesion = cuerpo.RootElement.GetProperty("idSesion").GetString();
                Assert.Equal(req.Headers.GetValues("X-Sesion").Single(), sesion);
            }
            return null;
        };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => incorporaciones > 0 && f.Consultas > 1);
        Assert.NotNull(sesion);
        Assert.DoesNotContain("Enter para continuar", f.Texto);
    }
```

### `SesionClienteTests.ProtocoloIncompatibleExigeReintentoExplicitoYNoAplicaVistaParcial`

Interceptor responde presentación con objeto fase inventada. Al avanzar1s debe mostrar Respuesta incompatible. Tras20s y50ms exige solo un fallo y cero elecciones: no reintenta automáticamente. Quita interceptor, Enter habilita nueva consulta. No convierte la vista parcial en estado válido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task ProtocoloIncompatibleExigeReintentoExplicitoYNoAplicaVistaParcial()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        int fallos = 0;
        f.Interceptar = req => Task.FromResult<HttpResponseMessage?>(req.RequestUri!.AbsolutePath.EndsWith("/presentacion")
            ? (++fallos > 0 ? Escenario.Json(new { fase = "inventada" }) : null) : null);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Respuesta incompatible"));
        f.Reloj.Avanzar(TimeSpan.FromSeconds(20));
        await Task.Delay(50);
        Assert.Equal(1, fallos);
        Assert.Equal(0, f.Elecciones);
        f.Interceptar = null;
        f.Tecla("");
        await f.Esperar(() => f.Consultas > 1);
    }
```

### `SesionClienteTests.PausaDuranteSeleccionCancelaAtributoYRechazoPermiteOperacionNueva`

Prepara turno, confirma carta y espera atributo. Guarda solicitud; cambia vista Pausada true/Solicitud null, avanza1s y espera pausa. Enter y más tiempo no envían (Elecciones0). Restaura misma solicitud, quita pausa y avanza: Tu turno debe aparecer después del aviso de pausa. Dos Enter entonces producen una elección. No simula un rechazo HTTP concreto pese al nombre.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task PausaDuranteSeleccionCancelaAtributoYRechazoPermiteOperacionNueva()
    {
        await using var f = new Escenario();
        f.PrepararTurno();
        await f.Esperar(() => f.Texto.Contains("Tu turno"));
        f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("Atributo del combate"));
        var solicitud = f.Vista.Solicitud;
        f.Vista = f.Vista with { Pausada = true, Solicitud = null };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Pausa confirmada"));
        f.Tecla("");
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await Task.Delay(50);
        Assert.Equal(0, f.Elecciones);
        f.Vista = f.Vista with { Pausada = false, Solicitud = solicitud };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.LastIndexOf("Tu turno", StringComparison.Ordinal) > f.Texto.LastIndexOf("Pausa confirmada", StringComparison.Ordinal));
        f.Tecla(""); f.Tecla("");
        await f.Esperar(() => f.Elecciones == 1);
    }
```

### `SesionClienteTests.AbandonoInciertoConsultaVistaAntesDeRepetirYNoInventaCierre`

Interceptor lanza HttpRequestException solo para primer abandono. Abre confirmación, selecciona Sí y confirma; intentos1 pero Abandonos0 porque el doble no aplicó. Avanza1s hasta que se aplique abandono1 y muestre fin confirmado. Garantiza no mostrar cierre antes de recuperar; este caso no inspecciona explícitamente secuencia de rutas (la integración real sí).

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task AbandonoInciertoConsultaVistaAntesDeRepetirYNoInventaCierre()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        int intentos = 0;
        f.Interceptar = req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/abandono") && ++intentos == 1)
                throw new HttpRequestException();
            return Task.FromResult<HttpResponseMessage?>(null);
        };
        await f.AbrirAbandono();
        await f.TeclaConPantalla("s");
        f.Tecla("");
        await f.Esperar(() => intentos == 1);
        Assert.Equal(0, f.Abandonos);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Abandonos == 1);
        await f.Esperar(() => f.Texto.Contains("Partida finalizada por abandono"));
    }
```

### `SesionClienteTests.EleccionPendienteEnPausaConservaOperacionYNoEnviaAbandonoConcurrente`

Intercepta POST elección guardando JSON y bloqueando en TCS. Confirma jugada, luego solicita abandono Sí mientras sigue en vuelo. Tras5s presencia sigue y abandono0. Marca pausa sin solicitud, completa envío con excepción, quita interceptor y avanza1s: debe consultar recibo, mostrar pausa y mantener Elecciones/Abandonos0 (contador solo mide doble aplicado). PrepararTurno crea solicitud nueva: pendiente obsoleta no se reenvía, abandono ya solicitado puede ejecutarse y Elecciones sigue0.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task EleccionPendienteEnPausaConservaOperacionYNoEnviaAbandonoConcurrente()
    {
        await using var f = new Escenario();
        f.PrepararTurno();
        await f.Esperar(() => f.Texto.Contains("Tu turno"));
        var enVuelo = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        string? original = null;
        f.Interceptar = async req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/elecciones"))
            {
                original = await req.Content!.ReadAsStringAsync();
                return await enVuelo.Task;
            }
            return null;
        };
        f.Tecla(""); f.Tecla(""); f.Tecla(""); f.Tecla("");
        await f.Esperar(() => original is not null);
        await f.AbrirAbandono();
        await f.TeclaConPantalla("s");
        f.Tecla("");
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => f.Presencias > 0);
        Assert.Equal(0, f.Abandonos);
        f.Vista = f.Vista with { Pausada = true, Solicitud = null };
        enVuelo.SetException(new HttpRequestException());
        f.Interceptar = null;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Recibos > 0 && f.Texto.Contains("Pausa confirmada"));
        Assert.Equal(0, f.Elecciones);
        Assert.Equal(0, f.Abandonos);
        // Una solicitud nueva descarta el reenvío obsoleto después de consultar recibo y vista.
        f.PrepararTurno();
        await f.Esperar(() => f.Abandonos == 1);
        Assert.Equal(0, f.Elecciones);
    }
```

### `SesionClienteTests.ConflictoNoReenviaOperacionYVistaIdenticaNoReiniciaFoco`

Ofrece cartas A/B y opciones1/2; mueve s a B y espera foco. Guarda longitud y consultas, avanza1s y exige consulta nueva con salida sin cambios (no redibuja ni pierde foco). Intercepta elecciones devolviendo409 operacionIncompatible; confirmar carta/atributo muestra rechazo. Tras5s y nuevas consultas, intentos sigue1: no reenvío automático.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task ConflictoNoReenviaOperacionYVistaIdenticaNoReiniciaFoco()
    {
        await using var f = new Escenario();
        f.PrepararTurno();
        f.Vista = f.Vista with
        {
            Mano = [f.Vista.Mano[0], f.Vista.Mano[0] with { IdPersonaje = 2, Nombre = "Carta B" }],
            Solicitud = f.Vista.Solicitud! with { IdsPersonajePermitidos = [1, 2] }
        };
        await f.Esperar(() => f.Texto.Contains("Carta B"));
        f.Tecla("s");
        await f.Esperar(() => f.Texto.Contains("> Carta B"));
        int longitud = f.Texto.Length, consultas = f.Consultas;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Consultas > consultas);
        Assert.Equal(longitud, f.Texto.Length);
        int intentos = 0;
        f.Interceptar = req => Task.FromResult<HttpResponseMessage?>(req.RequestUri!.AbsolutePath.EndsWith("/elecciones") && ++intentos > 0
            ? Escenario.Json(new { codigo = "operacionIncompatible" }, HttpStatusCode.Conflict) : null);
        f.Tecla(""); f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("Elección rechazada: operacionIncompatible"));
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => f.Consultas > consultas + 1);
        Assert.Equal(1, intentos);
    }
```

### `SesionClienteTests.VictoriaEsperaLecturaDelUltimoCombateYNoReproduceResultadosIniciales`

Tras primera consulta agrega resultado ganador de ronda J2 y final victoria/J2. Avanza1s y espera Enter para continuar; antes no debe mostrar gana la partida. Enter entonces permite Ada/J2 gana la partida y Elecciones0. El método comprueba orden de lectura del último resultado; omisión de historia inicial se acredita principalmente en EstadoClienteTests.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task VictoriaEsperaLecturaDelUltimoCombateYNoReproduceResultadosIniciales()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        var resultado = EstadoClienteTests.Resultado(1, 1) with { GanadorRonda = "J2" };
        f.Vista = f.Vista with { Fase = "finalizada", MotivoFin = "victoria", Ganador = "J2", Resultados = [resultado] };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Enter para continuar"));
        Assert.DoesNotContain("gana la partida", f.Texto);
        f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("Ada gana la partida") || f.Texto.Contains("J2 gana la partida"));
        Assert.Equal(0, f.Elecciones);
    }
```

### `SesionClienteTests.RespuestaTardiaDePresenciaAnteriorNoInvalidaSesionRecuperada`

Interceptor deja presencia pendiente y hace vencer próxima presentación una sola vez; cuenta reincorporaciones. Avanza5s hasta presencia, luego1s para vencimiento y reincorporación; espera Estado actualizado posterior a Sesión vencida. Quita interceptor y resuelve presencia vieja con400 entradaInvalida; nueva consulta debe seguir y no aparecer Enter para corregir los datos. Prueba descarte de respuesta de generación anterior.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task RespuestaTardiaDePresenciaAnteriorNoInvalidaSesionRecuperada()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Texto.Contains("Esperando inicio"));
        var presencia = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        bool iniciada = false, vencer = false;
        int incorporaciones = 0;
        f.Interceptar = req =>
        {
            string ruta = req.RequestUri!.AbsolutePath;
            if (ruta.EndsWith("/presencia")) { iniciada = true; return presencia.Task.ContinueWith<HttpResponseMessage?>(t => t.Result); }
            if (ruta.EndsWith("/presentacion") && vencer)
            { vencer = false; return Task.FromResult<HttpResponseMessage?>(Escenario.Json(new { codigo = "sesionVencida" }, HttpStatusCode.Conflict)); }
            if (ruta.EndsWith("/sesion")) incorporaciones++;
            return Task.FromResult<HttpResponseMessage?>(null);
        };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => iniciada);
        vencer = true;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => incorporaciones > 0);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.LastIndexOf("Estado actualizado", StringComparison.Ordinal) > f.Texto.LastIndexOf("Sesión vencida", StringComparison.Ordinal));
        f.Interceptar = null;
        presencia.SetResult(Escenario.Json(new { codigo = "entradaInvalida" }, HttpStatusCode.BadRequest));
        int consultas = f.Consultas;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Consultas > consultas);
        Assert.DoesNotContain("Enter para corregir los datos", f.Texto);
    }
```

### `SesionClienteTests.PendienteSinReciboEsperaPausaYReenviaCuerpoOriginalAlReanudar`

En primer POST guarda JSON, marca vista pausada/sin solicitud y lanza excepción sin aplicar. Tras avance espera recibo consultado y pausa. Otros5s mantienen presencia pero Elecciones0. Restaura misma solicitud y quita pausa;1s después el doble aplica una elección y su único cuerpo coincide exactamente con original. No crea un nuevo UUID de operación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task PendienteSinReciboEsperaPausaYReenviaCuerpoOriginalAlReanudar()
    {
        await using var f = new Escenario();
        f.PrepararTurno();
        await f.Esperar(() => f.Texto.Contains("Tu turno"));
        var solicitud = f.Vista.Solicitud;
        string? original = null;
        f.Interceptar = async req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/elecciones") && original is null)
            {
                original = await req.Content!.ReadAsStringAsync();
                f.Vista = f.Vista with { Pausada = true, Solicitud = null };
                throw new HttpRequestException();
            }
            return null;
        };
        f.Tecla(""); f.Tecla("");
        await f.Esperar(() => original is not null);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Recibos > 0 && f.Texto.Contains("Pausa confirmada"));
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => f.Presencias > 0);
        Assert.Equal(0, f.Elecciones);
        f.Vista = f.Vista with { Pausada = false, Solicitud = solicitud };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Elecciones == 1);
        Assert.Equal(original, Assert.Single(f.Cuerpos));
    }
```

### `Lineas.Agregar`

internal void recibe string nullable y TryWrite al canal; null simula fin de entrada, cadena vacía Enter. No espera consumidor ni completa writer.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Agregar(string? linea) => canal.Writer.TryWrite(linea);
```

### `Lineas.ReadLineAsync`

Override público retorna ReadAsync con el token recibido; espera una línea disponible y propaga cancelación. Un null agregado retorna null sin cerrar el canal.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken) => canal.Reader.ReadAsync(cancellationToken);
```

### `Escenario.Escenario`

Constructor internal: crea configuración localhost ficticia, UUID partida/sesión y Vista de EstadoClienteTests. Instala Handler async: primero da oportunidad a Interceptar; respuesta no null sustituye, excepción se propaga, null continúa. Ruta /sesion retorna J1/mismo UUID/vencimientoUtc+15s; /presencia incrementa y204; /presentacion incrementa y JSON Vista. POST /elecciones captura cuerpo, incrementa contador y deserializa: Perdida antes se limpia y lanza sin recibo; si no crea recibo revisión+1 y cambia Vista revisión/solicitud null; Perdida despues se limpia y lanza tras aplicar; normal retorna recibo. GET /elecciones/<id> incrementa Recibos y da recibo existente o404. /abandono incrementa Abandonos, revisión+1, finalizada/motivoabandono/solicitud null y204. Cualquier otra ruta lanza InvalidOperationException. Finalmente inicia SesionPartidaCliente con Entrada, writer, reloj y token cierre. Es simulación controlada, no repositorio real.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal Escenario()
        {
            var config = new ConfiguracionCliente(new Uri("http://localhost/"), Guid.NewGuid(), "secreto", "Ana");
            Guid sesion = Guid.NewGuid();
            Vista = EstadoClienteTests.Vista(config.Partida);
            http = new(config, sesion, new TransporteClienteTests.Handler(async req =>
            {
                if (Interceptar is not null && await Interceptar(req) is { } respuesta) return respuesta;
                string ruta = req.RequestUri!.AbsolutePath;
                if (ruta.EndsWith("/sesion")) return Json(new SesionConfirmada("J1", sesion, DateTimeOffset.UtcNow.AddSeconds(15)));
                if (ruta.EndsWith("/presencia")) { Interlocked.Increment(ref Presencias); return new(HttpStatusCode.NoContent); }
                if (ruta.EndsWith("/presentacion")) { Interlocked.Increment(ref Consultas); return Json(Vista); }
                if (ruta.EndsWith("/elecciones"))
                {
                    string cuerpo = await req.Content!.ReadAsStringAsync();
                    Cuerpos.Add(cuerpo);
                    Interlocked.Increment(ref Elecciones);
                    var e = JsonSerializer.Deserialize<EleccionCliente>(cuerpo, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                    if (Perdida == "antes") { Perdida = null; throw new HttpRequestException(); }
                    recibo = new(e.IdOperacion, e.IdSolicitud, e.RevisionEsperada + 1);
                    Vista = Vista with { Revision = e.RevisionEsperada + 1, Solicitud = null };
                    if (Perdida == "despues") { Perdida = null; throw new HttpRequestException(); }
                    return Json(recibo);
                }
                if (ruta.Contains("/elecciones/"))
                {
                    Interlocked.Increment(ref Recibos);
                    return recibo is not null ? Json(recibo) : Json(new { codigo = "recursoNoEncontrado" }, HttpStatusCode.NotFound);
                }
                if (ruta.EndsWith("/abandono"))
                {
                    Interlocked.Increment(ref Abandonos);
                    Vista = Vista with { Revision = Vista.Revision + 1, Fase = "finalizada", Solicitud = null, MotivoFin = "abandono" };
                    return new(HttpStatusCode.NoContent);
                }
                throw new InvalidOperationException(ruta);
            }));
            Tarea = new SesionPartidaCliente(config, http, new InterfazConsola(Entrada, salida), Reloj)
                .Ejecutar(new EntradaCliente(Entrada), cierre.Token);
        }
```

### `Escenario.PrepararTurno`

internal void: copia Vista a esperandoElector, sin pausa, ronda1/combate1, mano de CartaA ID1 con seis puntajes100 y solicitud nueva UUID tipo cartaYAtributo permitiendo1/Fuerza. Avanza reloj1s para provocar consulta. Puntajes son fixture de presentación, no cartas de dominio válidas (no pasan reglas250..750).

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void PrepararTurno()
        {
            Vista = Vista with
            {
                Fase = "esperandoElector", Pausada = false, NumeroRonda = 1, NumeroCombate = 1,
                Mano = [new(1, "Carta A", new Dictionary<string, int> { ["Fuerza"] = 100, ["Velocidad"] = 100, ["Resistencia"] = 100, ["Inteligencia"] = 100, ["Energía"] = 100, ["Habilidad"] = 100 })],
                Solicitud = new(Guid.NewGuid(), "cartaYAtributo", [1], ["Fuerza"])
            };
            Reloj.Avanzar(TimeSpan.FromSeconds(1));
        }
```

### `Escenario.Tecla`

internal void delega Entrada.Agregar(linea), sin esperar a que dibuje ni validar tecla.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Tecla(string linea) => Entrada.Agregar(linea);
```

### `Escenario.TeclaConPantalla`

internal async Task: guarda longitud Texto, envía tecla y llama Esperar hasta longitud mayor. Serializa navegación respecto a un dibujo; no comprueba por sí sola texto de pantalla.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal async Task TeclaConPantalla(string linea)
        {
            int longitud = Texto.Length;
            Tecla(linea);
            await Esperar(() => Texto.Length > longitud);
        }
```

### `Escenario.AbrirAbandono`

internal async Task: cuatro pasos con TeclaConPantalla: esc, s, s, Enter. Abre menú, recorre dos opciones y entra en confirmación todavía sobre No. No manda Sí ni abandono.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal async Task AbrirAbandono()
        {
            await TeclaConPantalla("esc");
            await TeclaConPantalla("s");
            await TeclaConPantalla("s");
            await TeclaConPantalla("");
        }
```

### `Escenario.Esperar`

internal async Task con predicado: máximo150 vueltas. Si Tarea.IsFaulted espera Tarea para propagar excepción; si predicado true retorna. Si no, avanza reloj25ms y demora10ms reales. Al agotar exige predicado con Texto como diagnóstico. No trata toda Tarea completada como fallo: solo faulted.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal async Task Esperar(Func<bool> condicion)
        {
            for (int i = 0; i < 150; i++)
            {
                if (Tarea.IsFaulted) await Tarea;
                if (condicion()) return;
                Reloj.Avanzar(TimeSpan.FromMilliseconds(25));
                await Task.Delay(10);
            }
            Assert.True(condicion(), Texto);
        }
```

### `Escenario.DisposeAsync`

public async ValueTask: cancela CTS, avanza reloj1s para despertar esperas, espera Tarea máximo3s, libera http y CTS. No libera explícitamente StringWriter ni canal; ambos son recursos de memoria y no hay proceso externo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async ValueTask DisposeAsync()
        {
            cierre.Cancel();
            Reloj.Avanzar(TimeSpan.FromSeconds(1));
            await Tarea.WaitAsync(TimeSpan.FromSeconds(3));
            http.Dispose(); cierre.Dispose();
        }
```

### `Escenario.Json`

internal static: recibe objeto y status opcional200; serializa con JsonSerializerDefaults.Web y arma HttpResponseMessage con UTF8/application/json. No valida esquema ni hace I/O. Caller/transporte posee la respuesta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal static HttpResponseMessage Json(object cuerpo, HttpStatusCode codigo = HttpStatusCode.OK) => new(codigo)
        { Content = new StringContent(JsonSerializer.Serialize(cuerpo, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json") };
```

### `RelojCliente.GetUtcNow`

Override público: lock temporizadores y retorna ahora, sin avanzar. Sincroniza consulta con Avanzar y creación de timers.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override DateTimeOffset GetUtcNow() { lock (temporizadores) return ahora; }
```

### `RelojCliente.CreateTimer`

Override público recibe callback,state,dueTime,period; con lock crea Temporizador capturando reloj/callback/state, agrega a lista, llama Change y retorna ITimer. Period se recibe pero el timer no lo usa para reprogramar automáticamente.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            lock (temporizadores)
            {
                var timer = new Temporizador(this, callback, state);
                temporizadores.Add(timer); timer.Change(dueTime, period); return timer;
            }
        }
```

### `RelojCliente.Avanzar`

internal void: con lock suma intervalo a ahora; recorre una copia de temporizadores para tolerar cambios de lista por callbacks. Si no cerrado y Vence<=ahora, primero pone Vence en MaxValue y luego invoca callback(State). Cada vencimiento dispara una vez salvo Change desde callback. No valida intervalos negativos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Avanzar(TimeSpan intervalo)
        {
            lock (temporizadores)
            {
                ahora += intervalo;
                foreach (var t in temporizadores.ToArray())
                    if (!t.Cerrado && t.Vence <= ahora) { t.Vence = DateTimeOffset.MaxValue; t.Callback(t.State); }
            }
        }
```

### `Temporizador.Change`

public bool: asigna Vence=MaxValue si dueTime InfiniteTimeSpan, o reloj.ahora+dueTime en otro caso; retorna !Cerrado. Ignora period. Incluso cerrado asigna Vence pero Avanzar lo omite. No es una implementación completa de temporizador periódico de sistema.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                Vence = dueTime == Timeout.InfiniteTimeSpan ? DateTimeOffset.MaxValue : reloj.ahora + dueTime;
                return !Cerrado;
            }
```

### `Temporizador.Dispose`

public void: marca Cerrado=true. No quita de lista ni libera un recurso OS; Avanzar excluye los cerrados.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public void Dispose() => Cerrado = true;
```

### `Temporizador.DisposeAsync`

public ValueTask: llama Dispose y retorna default ValueTask completado; no trabajo asíncrono ni espera adicional.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public ValueTask DisposeAsync() { Dispose(); return default; }
```

## Recorridos y ejemplos

Normal: Respuesta perdida antes de aplicar produce consulta de recibo y segundo envío del cuerpo original; después de aplicar solo se consulta recibo.

Alternativo o de error: Pausa, protocolo incompatible o conflicto no deben enviar operación obsoleta. Un abandono espera la elección incierta en vuelo; respuesta tardía de presencia vieja no invalida sesión recuperada.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
