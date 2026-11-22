# IntegracionRespuestaPerdidaTests.cs

Fuente: [test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs](../../../../test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Ensaya recuperación del bucle cliente real sobre HTTP loopback y SQLite temporal mediante intermediario que corta una única respuesta antes o después del commit. También recorre una partida completa con dos transportes.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](../../src/cliente/ClientePartidaHttp.md), [ConfiguracionCliente](../../src/cliente/ConfiguracionCliente.md), [EleccionCliente](../../src/cliente/ContratosPartida.md), [EntradaCliente](../../src/cliente/EntradaCliente.md), [InterfazConsola](../../src/cliente/InterfazConsola.md), [PresentacionCliente](../../src/cliente/ContratosPartida.md), [RepositorioJuego](../../src/servidor/Api/RepositorioJuego.md), [ServidorHttp](../../src/servidor/Api/ServidorHttp.md), [SesionPartidaCliente](../../src/cliente/SesionPartidaCliente.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class IntegracionRespuestaPerdidaTests`.

Declaración: `private sealed class CorteHttp(bool despues, string rutaCorte = "elecciones") : DelegatingHandler(new HttpClientHandler())`.

Declaración: `private sealed class Teclas : TextReader`.

Declaración: `private sealed class Salida : StringWriter`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

CorteHttp(bool despues,string rutaCorte="elecciones") privado hereda DelegatingHandler(new HttpClientHandler()); cortada inicialmente false limita el corte a uno, ConfirmadaAntesDelCorte false solo cambia después de respuesta real exitosa y Cuerpos/Rutas son colas concurrentes. Teclas privado deriva TextReader con Channel<string> ilimitado. Salida privado deriva StringWriter y serializa accesos con bloqueo; Longitud es accesor con lock que devuelve ToString().Length.

Declaraciones reales de estado de `CorteHttp` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private bool cortada;
```

```csharp
internal bool ConfirmadaAntesDelCorte;
```

```csharp
internal ConcurrentQueue<string> Cuerpos = new();
```

```csharp
internal ConcurrentQueue<string> Rutas = new();
```

Declaraciones reales de estado de `Teclas` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Channel<string> canal = Channel.CreateUnbounded<string>();
```

Declaraciones reales de estado de `Salida` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly object bloqueo = new();
```

```csharp
internal int Longitud { get { lock (bloqueo) return ToString().Length; } }
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `IntegracionRespuestaPerdidaTests.ClienteResuelveEleccionInciertaContraSQLiteReal`

Theory confirmarAntesDelCorte true/false. Base temporal con seis cartas, host loopback y transportes separados; J2 se incorpora, J1 usa CorteHttp, se inicia partida. Ejecuta SesionPartidaCliente con canal de teclas y CTS12s; espera turno, confirma carta, espera atributo y confirma. Tras Recepción confirmada, vista rival debe tener revisión inicial+1, sin resultados y solicitud carta. Exige un cuerpo si corte posterior o dos si previo, todos idénticos. Rutas deben mostrar GET elecciones/<id> después del primer POST y antes del reenvío cuando lo hay. Responder J2 produce un solo resultado y revisión inicial+2. finally interno cancela bucle y espera3s antes de detener host; externo limpia conexiones y archivos SQLite/-wal/-shm.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ClienteResuelveEleccionInciertaContraSQLiteReal(bool confirmarAntesDelCorte)
    {
        // La base temporal aísla el ensayo y permite comprobar commits reales de SQLite.
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-red-{Guid.NewGuid():N}.db");
        try
        {
            await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            var juego = app.Services.GetRequiredService<RepositorioJuego>();
            for (int i = 0; i < 6; i++) juego.DarDeAlta(new($"Carta {i}", "Descripción", "Arte", 750, 550, 500, 250, 500, 450));
            var partida = juego.CrearPartida();
            await app.StartAsync();
            // Los transportes conectan a Kestrel por loopback con credenciales separadas.
            var config = new ConfiguracionCliente(new Uri(app.Urls.Single() + "/"), partida.Id, partida.CredencialJ1, "Ana");
            using var rival = new ClientePartidaHttp(config with { Credencial = partida.CredencialJ2, Nombre = "Luis" }, Guid.NewGuid());
            await rival.Incorporar(default);
            // El intermediario decide si el primer envío llega a confirmarse antes del corte.
            using var corte = new CorteHttp(confirmarAntesDelCorte);
            using var cliente = new ClientePartidaHttp(config, Guid.NewGuid(), corte);
            await cliente.Incorporar(default);
            juego.Iniciar(partida.Id);
            var inicial = await cliente.Presentacion(default);
            using var cierre = new CancellationTokenSource(TimeSpan.FromSeconds(12));
            var entrada = new Teclas();
            var salida = new Salida();
            var sesion = new SesionPartidaCliente(config, cliente, new InterfazConsola(entrada, salida));
            // Se usa el bucle real del cliente para observar su recuperación, además del transporte.
            var tarea = sesion.Ejecutar(new EntradaCliente(entrada), cierre.Token);
            try
            {
                await Esperar(() => salida.Contiene("Tu turno"), tarea);
                entrada.Enviar();
                await Esperar(() => salida.Contiene("Atributo del combate"), tarea);
                entrada.Enviar();
                // La prueba espera confirmación del cliente antes de inspeccionar el estado del rival.
                await Esperar(() => salida.Contiene("Recepción confirmada"), tarea);
                var actual = await rival.Presentacion(default);
                Assert.Equal(inicial.Revision + 1, actual.Revision);
                Assert.Empty(actual.Resultados);
                Assert.Equal("carta", actual.Solicitud!.Tipo);
                // Si hubo reenvío, el JSON debe conservar exactamente la misma operación.
                string[] cuerpos = corte.Cuerpos.ToArray();
                Assert.Equal(confirmarAntesDelCorte ? 1 : 2, cuerpos.Length);
                Assert.All(cuerpos, cuerpo => Assert.Equal(cuerpos[0], cuerpo));
                string[] rutas = corte.Rutas.ToArray();
                // El orden de rutas demuestra que se consultó el recibo antes de volver a elegir.
                int primerPost = Array.FindIndex(rutas, r => r == "POST elecciones");
                int recibo = Array.FindIndex(rutas, primerPost + 1, r => r.StartsWith("GET elecciones/"));
                Assert.True(recibo > primerPost);
                if (!confirmarAntesDelCorte)
                    Assert.True(Array.FindLastIndex(rutas, r => r == "POST elecciones") > recibo);
                Assert.Equal(confirmarAntesDelCorte, corte.ConfirmadaAntesDelCorte);
                await rival.Elegir(new(Guid.NewGuid(), actual.Solicitud.IdSolicitud, actual.Revision, actual.Solicitud.IdsPersonajePermitidos[0], null), default);
                // Responder debe resolver un único combate, con una revisión por cada elección.
                var resultado = await rival.Presentacion(default);
                Assert.Single(resultado.Resultados);
                Assert.Equal(inicial.Revision + 2, resultado.Revision);
            }
            finally
            {
                // El bucle se cancela y se espera antes de detener el servidor del ensayo.
                cierre.Cancel();
                await tarea.WaitAsync(TimeSpan.FromSeconds(3));
            }
            await app.StopAsync();
        }
        finally
        {
            // La limpieza libera conexiones y elimina únicamente la base temporal de esta prueba.
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }
```

### `IntegracionRespuestaPerdidaTests.DosTransportesCompletanPartidaNormalHastaQuinceSinRepartoPosterior`

Base temporal y seis cartas de mismos atributos. Incorpora transportes J1/J2 e inicia. Bucle limitado a menos300 elecciones renueva ambas presencias, consulta ambas vistas y exige misma revisión, marcador ordenado, JSON de resultados y manos disjuntas. Si finalizada corta; si activa exactamente una solicitud debe existir. En apertura verifica elector por paridad, manos4−combate y exclusión de cartas ya reveladas en la ronda. Elige primera opción y atributo solo en cartaYAtributo, exige recibo revisión+1 y consulta igual. Final: victoria, ganador compartido con15, ronda29,87 resultados, tres por ronda y dos para ganador de ronda; solicitudes null y nueva consulta estable sin reparto. finally limpia base y pools.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    // Este recorrido comienza desde cero y conserva la meta normal de quince rondas.
    public async Task DosTransportesCompletanPartidaNormalHastaQuinceSinRepartoPosterior()
    {
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-completa-{Guid.NewGuid():N}.db");
        try
        {
            await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            var juego = app.Services.GetRequiredService<RepositorioJuego>();
            for (int i = 0; i < 6; i++) juego.DarDeAlta(new($"Carta {i}", "Descripción", "Arte", 750, 550, 500, 250, 500, 450));
            var partida = juego.CrearPartida();
            await app.StartAsync();
            var config = new ConfiguracionCliente(new Uri(app.Urls.Single() + "/"), partida.Id, partida.CredencialJ1, "Ana");
            // Cada plaza usa su propio transporte y UUID de sesión contra el servidor real.
            using var j1 = new ClientePartidaHttp(config, Guid.NewGuid());
            using var j2 = new ClientePartidaHttp(config with { Credencial = partida.CredencialJ2, Nombre = "Luis" }, Guid.NewGuid());
            await j1.Incorporar(default);
            await j2.Incorporar(default);
            juego.Iniciar(partida.Id);
            // El límite de elecciones evita un bucle infinito si la partida deja de progresar.
            int elecciones = 0;
            PresentacionCliente a, b;
            do
            {
                Assert.True(elecciones < 300, "La partida no llegó a quince rondas.");
                // Se renuevan ambas presencias durante el recorrido para mantener la partida activa.
                await j1.Renovar(default);
                await j2.Renovar(default);
                a = await j1.Presentacion(default);
                b = await j2.Presentacion(default);
                // Las dos vistas deben concordar en revisión, marcador y resultados ya revelados.
                Assert.Equal(a.Revision, b.Revision);
                Assert.Equal(a.Marcador.OrderBy(x => x.Key), b.Marcador.OrderBy(x => x.Key));
                Assert.Equal(JsonSerializer.Serialize(a.Resultados), JsonSerializer.Serialize(b.Resultados));
                Assert.Empty(a.Mano.Select(x => x.IdPersonaje).Intersect(b.Mano.Select(x => x.IdPersonaje)));
                // Mientras sigue activa, exactamente una plaza debe tener una solicitud habilitada.
                if (a.Fase == "finalizada") break;
                Assert.NotEqual(a.Solicitud is null, b.Solicitud is null);
                var vista = a.Solicitud is not null ? a : b;
                var transporte = a.Solicitud is not null ? j1 : j2;
                var solicitud = vista.Solicitud!;
                if (solicitud.Tipo == "cartaYAtributo")
                {
                    // La paridad de ronda y combate expresa la alternancia de mano y elector.
                    string elector = (vista.NumeroRonda + vista.NumeroCombate) % 2 == 0 ? "J1" : "J2";
                    Assert.Equal(elector, vista.RolPropio);
                    Assert.Equal(4 - vista.NumeroCombate, a.Mano.Length);
                    Assert.Equal(4 - vista.NumeroCombate, b.Mano.Length);
                    // Las cartas reveladas de la ronda no pueden volver a aparecer en ninguna mano.
                    var usadas = vista.Resultados.Where(r => r.NumeroRonda == vista.NumeroRonda)
                        .SelectMany(r => r.Cartas.Values).Select(c => c.IdPersonaje);
                    Assert.Empty(a.Mano.Concat(b.Mano).Select(c => c.IdPersonaje).Intersect(usadas));
                }
                // Cada turno utiliza solo opciones permitidas y luego comprueba su recibo persistido.
                var eleccion = new EleccionCliente(Guid.NewGuid(), solicitud.IdSolicitud, vista.Revision,
                    solicitud.IdsPersonajePermitidos[0], solicitud.Tipo == "cartaYAtributo" ? solicitud.AtributosPermitidos[0] : null);
                var recibo = await transporte.Elegir(eleccion, default);
                Assert.Equal(vista.Revision + 1, recibo.RevisionConfirmada);
                Assert.Equal(recibo, await transporte.Recibo(eleccion.IdOperacion, default));
                elecciones++;
            } while (true);
            Assert.Equal("victoria", a.MotivoFin);
            Assert.Equal(a.Ganador, b.Ganador);
            Assert.Equal(15, a.Marcador[a.Ganador!]);
            // Con cartas de iguales atributos, los empates producen 29 rondas de tres combates.
            Assert.Equal(29, a.NumeroRonda);
            Assert.Equal(87, a.Resultados.Length);
            Assert.All(a.Resultados.GroupBy(r => r.NumeroRonda), ronda =>
            {
                Assert.Equal(3, ronda.Count());
                Assert.Equal(2, ronda.Count(r => r.GanadorCombate == ronda.Last().GanadorRonda));
            });
            Assert.Null(a.Solicitud);
            Assert.Null(b.Solicitud);
            // Consultar de nuevo el final no cambia revisión ni genera un reparto posterior.
            var estable = await j1.Presentacion(default);
            Assert.Equal(a.Revision, estable.Revision);
            Assert.Equal(a.Mano.Select(x => x.IdPersonaje), estable.Mano.Select(x => x.IdPersonaje));
            await app.StopAsync();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }
```

### `IntegracionRespuestaPerdidaTests.AbandonoInciertoSoloMuestraFinConfirmadoSinSumarVictoria`

Theory corte antes/después. Partida distinta, SQLite y host real, intermediario corta solo ruta abandono. Bucle de consola con CTS15s: abre menú esc/s/s/Enter, exige cero PUT abandono; Enter en No vuelve sin enviar. Reabre, baja a Sí y confirma; espera fin por abandono. Vista rival debe tener motivo abandono, ganador/solicitud null, revisión+1, marcador igual al inicial y flag de corte acorde. Cuenta PUT1 si aplicado o2 si previo; nunca POST elecciones. Repetir abandono conserva revisión. Cancelación del bucle precede al cierre del host y a limpiar archivos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(true)]
    [InlineData(false)]
    // Ambas variantes recuperan el motivo de abandono sin presentar una victoria de juego.
    public async Task AbandonoInciertoSoloMuestraFinConfirmadoSinSumarVictoria(bool confirmarAntesDelCorte)
    {
        // El abandono usa otra partida porque una partida finalizada no permite continuar jugando.
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-abandono-{Guid.NewGuid():N}.db");
        try
        {
            await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            var juego = app.Services.GetRequiredService<RepositorioJuego>();
            for (int i = 0; i < 6; i++) juego.DarDeAlta(new($"Carta {i}", "Descripción", "Arte", 750, 550, 500, 250, 500, 450));
            var partida = juego.CrearPartida();
            await app.StartAsync();
            var config = new ConfiguracionCliente(new Uri(app.Urls.Single() + "/"), partida.Id, partida.CredencialJ1, "Ana");
            using var rival = new ClientePartidaHttp(config with { Credencial = partida.CredencialJ2, Nombre = "Luis" }, Guid.NewGuid());
            await rival.Incorporar(default);
            // Solo se interrumpe el abandono; consultas y presencia siguen usando HTTP real.
            using var corte = new CorteHttp(confirmarAntesDelCorte, "participantes/me/abandono");
            using var cliente = new ClientePartidaHttp(config, Guid.NewGuid(), corte);
            await cliente.Incorporar(default);
            juego.Iniciar(partida.Id);
            var inicial = await rival.Presentacion(default);
            // La entrada y salida controladas permiten recorrer el menú real con un tiempo máximo.
            using var cierre = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var entrada = new Teclas();
            var salida = new Salida();
            var tarea = new SesionPartidaCliente(config, cliente, new InterfazConsola(entrada, salida))
                .Ejecutar(new EntradaCliente(entrada), cierre.Token);
            try
            {
                await Esperar(() => salida.Contiene("Tu turno"), tarea);
                await Tecla("esc");
                // La navegación espera una nueva pantalla antes de enviar la siguiente tecla.
                await Tecla("s");
                await Tecla("s");
                await Tecla("");
                // Llegar a la confirmación no debe enviar todavía la solicitud de abandono.
                Assert.DoesNotContain(corte.Rutas, r => r == "PUT participantes/me/abandono");
                // La opción No vuelve al juego sin enviar abandono.
                await Tecla("");
                Assert.DoesNotContain(corte.Rutas, r => r == "PUT participantes/me/abandono");
                await Tecla("esc");
                await Tecla("s");
                await Tecla("s");
                await Tecla("");
                await Tecla("s");
                entrada.Enviar();
                // El fin visible debe coincidir con una presentación final confirmada por el servidor.
                await Esperar(() => salida.Contiene("Partida finalizada por abandono"), tarea);
                var final = await rival.Presentacion(default);
                Assert.Equal("abandono", final.MotivoFin);
                Assert.Null(final.Ganador);
                Assert.Null(final.Solicitud);
                Assert.Equal(inicial.Revision + 1, final.Revision);
                // Ni la pérdida de respuesta ni su recuperación deben modificar el marcador.
                Assert.Equal(inicial.Marcador.OrderBy(x => x.Key), final.Marcador.OrderBy(x => x.Key));
                Assert.Equal(confirmarAntesDelCorte, corte.ConfirmadaAntesDelCorte);
                Assert.Equal(confirmarAntesDelCorte ? 1 : 2,
                    corte.Rutas.Count(r => r == "PUT participantes/me/abandono"));
                Assert.DoesNotContain(corte.Rutas, r => r == "POST elecciones");
                // Repetir el abandono ya confirmado no genera una nueva revisión.
                await cliente.Abandonar(default);
                Assert.Equal(final.Revision, (await rival.Presentacion(default)).Revision);
            }
            finally { cierre.Cancel(); await tarea.WaitAsync(TimeSpan.FromSeconds(3)); }
            await app.StopAsync();

            // El tamaño de la salida indica que el bucle ya procesó el paso anterior del menú.
            async Task Tecla(string tecla)
            {
                int longitud = salida.Longitud;
                entrada.Enviar(tecla);
                await Esperar(() => salida.Longitud > longitud, tarea);
            }
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }
```

### `IntegracionRespuestaPerdidaTests.Tecla`

Función local async del caso abandono: captura salida, entrada y tarea. Guarda Longitud, envía tecla y espera que longitud crezca para no adelantar la navegación a un dibujo anterior. No comprueba por sí misma cuál pantalla se dibujó; las aserciones del caso verifican el resultado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
async Task Tecla(string tecla)
            {
                int longitud = salida.Longitud;
                entrada.Enviar(tecla);
                await Esperar(() => salida.Longitud > longitud, tarea);
            }
```

### `IntegracionRespuestaPerdidaTests.Esperar`

private static async Task: recibe predicado y tarea sesión; CTS8s. Mientras predicado false, si sesión ya completó espera su resultado (propaga fallos) y falla explícitamente porque terminó antes de confirmación. Si sigue viva retrasa20ms con token. True retorna; timeout cancela y falla. No reinicia tarea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static async Task Esperar(Func<bool> condicion, Task sesion)
    {
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        while (!condicion())
        {
            if (sesion.IsCompleted) { await sesion; Assert.Fail("La sesión terminó antes de confirmar la elección."); }
            await Task.Delay(20, limite.Token);
        }
    }
```

### `CorteHttp.SendAsync`

Override protected async: extrae de AbsolutePath la ruta después de /partidas/<UUID>/ y registra método+ruta sin cabeceras. Si es POST elecciones registra cuerpo. Si ruta coincide con rutaCorte, método no es GET y aún no cortó, marca cortada; con despues true transmite al handler real, exige éxito y consume bytes antes de marcar ConfirmadaAntesDelCorte; después lanza HttpRequestException. Con despues false lanza sin transmitir. GET y todo tráfico posterior pasan al handler real con ct. No inventa recibos ni respuestas de éxito.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            // Se registra únicamente la ruta relativa de la operación, sin sus cabeceras de identidad.
            string ruta = request.RequestUri!.AbsolutePath.Split("/partidas/")[1];
            ruta = ruta[(ruta.IndexOf('/') + 1)..];
            Rutas.Enqueue($"{request.Method} {ruta}");
            bool eleccion = request.Method == HttpMethod.Post && ruta == "elecciones";
            if (eleccion) Cuerpos.Enqueue(await request.Content!.ReadAsStringAsync(ct));
            // Los GET atraviesan el intermediario para permitir consultas y recuperación del recibo.
            if (ruta == rutaCorte && request.Method != HttpMethod.Get && !cortada)
            {
                cortada = true;
                if (despues)
                {
                    // En la variante posterior se exige éxito real y se consume la respuesta antes de ocultarla.
                    using var respuesta = await base.SendAsync(request, ct);
                    respuesta.EnsureSuccessStatusCode();
                    await respuesta.Content.ReadAsByteArrayAsync(ct);
                    ConfirmadaAntesDelCorte = true;
                }
                throw new HttpRequestException("Corte controlado de ensayo");
            }
            // Después del primer corte, todas las solicitudes vuelven a pasar normalmente.
            return await base.SendAsync(request, ct);
        }
```

### `Teclas.Enviar`

internal void con tecla predeterminada vacía: TryWrite al Channel; simula Enter enviando una línea vacía. No cierra canal ni agrega un salto de línea físico.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Enviar(string tecla = "") => canal.Writer.TryWrite(tecla);
```

### `Teclas.ReadLineAsync`

Override público devuelve Leer(ct); permite que EntradaCliente lea de modo asíncrono mientras sigue el tráfico.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override ValueTask<string?> ReadLineAsync(CancellationToken ct) => Leer(ct);
```

### `Teclas.Leer`

private async ValueTask<string?>: espera canal.Reader.ReadAsync(ct) y devuelve línea; cancelación se propaga. El canal nunca se completa aquí, de modo que EOF no es el escenario de este auxiliar.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private async ValueTask<string?> Leer(CancellationToken ct) => await canal.Reader.ReadAsync(ct);
```

### `Salida.WriteLine`

Override público: lock sobre bloqueo, delega a base.WriteLine(value). Evita que la aserción de Longitud/Contiene lea mientras se añade una línea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override void WriteLine(string? value) { lock (bloqueo) base.WriteLine(value); }
```

### `Salida.Write`

Override público con el mismo bloqueo y base.Write(value); serializa también escrituras sin salto. No filtra contenido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override void Write(string? value) { lock (bloqueo) base.Write(value); }
```

### `Salida.Contiene`

internal bool: con lock devuelve ToString().Contains(texto), sensible a mayúsculas por defecto. No espera ni consume contenido; Esperar controla repetición y plazo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal bool Contiene(string texto) { lock (bloqueo) return ToString().Contains(texto); }
```

## Recorridos y ejemplos

Normal: Corte posterior a confirmar elección: solo un POST, consulta de recibo y revisión+1. Corte previo: GET recibo antes del segundo POST, cuerpo idéntico y misma revisión+1.

Alternativo o de error: Abandono incierto requiere Sí: No no envía; perder respuesta no suma victoria ni cambia marcador y repetir abandono ya confirmado no aumenta revisión.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
