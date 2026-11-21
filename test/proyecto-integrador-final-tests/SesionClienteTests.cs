using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class SesionClienteTests
{
    [Fact]
    public async Task ConsultaYPresenciaContinuanEnMenuYResultadoSinCambiarRevision()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Texto.Contains("Esperando inicio"));
        f.Tecla("esc");
        await f.Esperar(() => f.Texto.Contains("Salir sin abandonar"));
        // El reloj avanza mientras el menú sigue abierto para comprobar consulta y presencia.
        // Leer un resultado tampoco debe detener las renovaciones ni enviar elecciones.
        int antes = f.Consultas;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => f.Consultas > antes && f.Presencias > 0);
        f.Vista = f.Vista with { Pausada = true };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Pausa confirmada"));
        f.Tecla("esc");
        await f.Esperar(() => f.Texto.Contains("Enter: volver"));
        // Reanudar recupera la solicitud, pero exige una nueva confirmación local.
        f.Vista = f.Vista with { Pausada = false, Resultados = [EstadoClienteTests.Resultado(1, 1)] };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Enter para continuar"));
        antes = f.Consultas;
        int presencias = f.Presencias;
        f.Reloj.Avanzar(TimeSpan.FromSeconds(5));
        await f.Esperar(() => f.Consultas > antes && f.Presencias > presencias);
        Assert.Equal(0, f.Elecciones);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RespuestaPerdidaConsultaReciboYReenviaSoloElMismoCuerpo(bool aplicado)
    {
        await using var f = new Escenario();
        f.PrepararTurno();
        // Las variantes distinguen una elección aceptada de otra que nunca llegó al servidor.
        f.Perdida = aplicado ? "despues" : "antes";
        await f.Esperar(() => f.Texto.Contains("Tu turno"));
        f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("Atributo del combate"));
        f.Tecla("");
        await f.Esperar(() => f.Elecciones == 1);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        // El recibo se consulta antes de decidir si corresponde reenviar el cuerpo original.
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
        // Al recibir el final confirmado deben detenerse consultas y presencia.
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

    [Fact]
    public async Task AbandonoExigeSiYSalidaLocalNoLoEnvia()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        // El menú comienza por No para exigir una confirmación explícita de abandono.
        await f.AbrirAbandono();
        await f.Esperar(() => f.Texto.Contains("¿Abandonar partida?"));
        Assert.Equal(0, f.Abandonos);
        f.Tecla(""); // No por defecto
        // Se deja procesar la negativa antes de comprobar que no hubo solicitud de abandono.
        await Task.Delay(80);
        Assert.Equal(0, f.Abandonos);
        await f.AbrirAbandono();
        await f.TeclaConPantalla("s");
        f.Tecla("");
        await f.Esperar(() => f.Abandonos == 1);
    }

    [Fact]
    public async Task UnaConsultaLentaNoSolapaConsultasNiRetrasaPresenciaYDescartaRespuestaAnterior()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        // La respuesta retenida permite comprobar que una consulta lenta no genera otra en paralelo.
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
        // El avance del reloj debe renovar presencia incluso mientras la vista está retenida.
        f.Reloj.Avanzar(TimeSpan.FromSeconds(10));
        await f.Esperar(() => f.Presencias > 0);
        Assert.Equal(1, iniciadas);
        f.Interceptar = null;
        retrasada.SetResult(Escenario.Json(f.Vista));
        await f.Esperar(() => f.Consultas > 1);
    }

    [Fact]
    public async Task SesionVencidaReincorporaConMismoUuidYNoReproduceHistoria()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        string? sesion = null;
        // El doble vence una única consulta para provocar la reincorporación automática.
        bool vencer = true;
        int incorporaciones = 0;
        f.Interceptar = async req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/presentacion") && vencer)
            { vencer = false; return Escenario.Json(new { codigo = "sesionVencida" }, HttpStatusCode.Conflict); }
            if (req.RequestUri.AbsolutePath.EndsWith("/sesion"))
            {
                incorporaciones++;
                // El cuerpo de incorporación debe usar la misma sesión que su cabecera.
                var cuerpo = JsonDocument.Parse(await req.Content!.ReadAsStringAsync());
                sesion = cuerpo.RootElement.GetProperty("idSesion").GetString();
                Assert.Equal(req.Headers.GetValues("X-Sesion").Single(), sesion);
            }
            return null;
        };
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => incorporaciones > 0 && f.Consultas > 1);
        Assert.NotNull(sesion);
        // Recuperar una sesión no obliga a reproducir combates históricos.
        Assert.DoesNotContain("Enter para continuar", f.Texto);
    }

    [Fact]
    public async Task ProtocoloIncompatibleExigeReintentoExplicitoYNoAplicaVistaParcial()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        // Una vista mal formada provoca un error de protocolo que requiere intervención.
        int fallos = 0;
        f.Interceptar = req => Task.FromResult<HttpResponseMessage?>(req.RequestUri!.AbsolutePath.EndsWith("/presentacion")
            ? (++fallos > 0 ? Escenario.Json(new { fase = "inventada" }) : null) : null);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Texto.Contains("Respuesta incompatible"));
        f.Reloj.Avanzar(TimeSpan.FromSeconds(20));
        await Task.Delay(50);
        // El cliente no repite automáticamente el contrato incompatible ni permite elegir.
        Assert.Equal(1, fallos);
        Assert.Equal(0, f.Elecciones);
        f.Interceptar = null;
        f.Tecla("");
        await f.Esperar(() => f.Consultas > 1);
    }

    [Fact]
    public async Task PausaDuranteSeleccionCancelaAtributoYRechazoPermiteOperacionNueva()
    {
        await using var f = new Escenario();
        f.PrepararTurno();
        await f.Esperar(() => f.Texto.Contains("Tu turno"));
        f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("Atributo del combate"));
        // La pausa retira las opciones visibles y cancela la selección local de atributo.
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

    [Fact]
    public async Task AbandonoInciertoConsultaVistaAntesDeRepetirYNoInventaCierre()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        // El primer abandono falla antes de aplicar cambios para comprobar la recuperación.
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
        // El cierre solo se muestra después de que el servidor confirme el abandono.
        await f.Esperar(() => intentos == 1);
        Assert.Equal(0, f.Abandonos);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Abandonos == 1);
        await f.Esperar(() => f.Texto.Contains("Partida finalizada por abandono"));
    }

    // El canal simula entrada de usuario sin bloquear el bucle de consultas.
    internal sealed class Lineas : TextReader
    {
        private readonly Channel<string?> canal = Channel.CreateUnbounded<string?>();
        internal void Agregar(string? linea) => canal.Writer.TryWrite(linea);
        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken) => canal.Reader.ReadAsync(cancellationToken);
    }

    internal sealed class Escenario : IAsyncDisposable
    {
        // El escenario controla tiempo, estado remoto y contadores de las solicitudes.
        internal readonly RelojCliente Reloj = new();
        internal PresentacionCliente Vista;
        internal readonly Lineas Entrada = new();
        internal int Consultas, Presencias, Elecciones, Recibos, Abandonos;
        internal string? Perdida;
        internal Func<HttpRequestMessage, Task<HttpResponseMessage?>>? Interceptar;
        internal readonly List<string> Cuerpos = [];
        // El doble conserva el recibo para responder consultas de una elección incierta.
        private ReciboCliente? recibo;
        private readonly CancellationTokenSource cierre = new();
        private readonly StringWriter salida = new();
        private readonly ClientePartidaHttp http;
        internal Task<bool> Tarea;
        internal string Texto => salida.ToString();
        internal Escenario()
        {
            // La sesión de prueba usa transporte interceptado y datos sin acceso a una red externa.
            var config = new ConfiguracionCliente(new Uri("http://localhost/"), Guid.NewGuid(), "secreto", "Ana");
            Guid sesion = Guid.NewGuid();
            Vista = EstadoClienteTests.Vista(config.Partida);
            http = new(config, sesion, new TransporteClienteTests.Handler(async req =>
            {
                // Cada caso puede interceptar una ruta y dejar pasar las demás al comportamiento base.
                if (Interceptar is not null && await Interceptar(req) is { } respuesta) return respuesta;
                string ruta = req.RequestUri!.AbsolutePath;
                if (ruta.EndsWith("/sesion")) return Json(new SesionConfirmada("J1", sesion, DateTimeOffset.UtcNow.AddSeconds(15)));
                if (ruta.EndsWith("/presencia")) { Interlocked.Increment(ref Presencias); return new(HttpStatusCode.NoContent); }
                if (ruta.EndsWith("/presentacion")) { Interlocked.Increment(ref Consultas); return Json(Vista); }
                if (ruta.EndsWith("/elecciones"))
                {
                    // Se guardan los cuerpos para verificar que un reintento no cambia la operación.
                    string cuerpo = await req.Content!.ReadAsStringAsync();
                    Cuerpos.Add(cuerpo);
                    Interlocked.Increment(ref Elecciones);
                    var e = JsonSerializer.Deserialize<EleccionCliente>(cuerpo, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                    if (Perdida == "antes") { Perdida = null; throw new HttpRequestException(); }
                    // La variante posterior al envío confirma estado y recibo antes de perder la respuesta.
                    recibo = new(e.IdOperacion, e.IdSolicitud, e.RevisionEsperada + 1);
                    Vista = Vista with { Revision = e.RevisionEsperada + 1, Solicitud = null };
                    if (Perdida == "despues") { Perdida = null; throw new HttpRequestException(); }
                    return Json(recibo);
                }
                // La ausencia de recibo se diferencia de una aceptación perdida.
                if (ruta.Contains("/elecciones/"))
                {
                    Interlocked.Increment(ref Recibos);
                    return recibo is not null ? Json(recibo) : Json(new { codigo = "recursoNoEncontrado" }, HttpStatusCode.NotFound);
                }
                // El abandono simulado modifica el motivo final sin sumar marcador.
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
        // Se ofrece una única carta válida para aislar la navegación y el envío.
        internal void PrepararTurno()
        {
            Vista = Vista with
            {
                Fase = "esperandoElector", NumeroRonda = 1, NumeroCombate = 1,
                Mano = [new(1, "Carta A", new Dictionary<string, int> { ["Fuerza"] = 100 })],
                Solicitud = new(Guid.NewGuid(), "cartaYAtributo", [1], ["Fuerza"])
            };
            Reloj.Avanzar(TimeSpan.FromSeconds(1));
        }
        internal void Tecla(string linea) => Entrada.Agregar(linea);
        // Se espera el dibujo antes de enviar otra tecla para no adelantarse a la interfaz.
        internal async Task TeclaConPantalla(string linea)
        {
            int longitud = Texto.Length;
            Tecla(linea);
            await Esperar(() => Texto.Length > longitud);
        }
        // La secuencia abre el menú y llega a la confirmación, todavía sobre la opción No.
        internal async Task AbrirAbandono()
        {
            await Esperar(() => Texto.Contains("Esperando inicio"));
            await TeclaConPantalla("esc");
            await TeclaConPantalla("s");
            await TeclaConPantalla("s");
            await TeclaConPantalla("");
        }

        internal async Task Esperar(Func<bool> condicion)
        {
            // La espera acotada adelanta el reloj del cliente y propaga fallos del bucle.
            for (int i = 0; i < 150; i++)
            {
                if (Tarea.IsFaulted) await Tarea;
                if (condicion()) return;
                Reloj.Avanzar(TimeSpan.FromMilliseconds(25));
                await Task.Delay(10);
            }
            Assert.True(condicion(), Texto);
        }
        // La cancelación y el avance final permiten liberar todas las tareas del escenario.
        public async ValueTask DisposeAsync()
        {
            cierre.Cancel();
            Reloj.Avanzar(TimeSpan.FromSeconds(1));
            await Tarea.WaitAsync(TimeSpan.FromSeconds(3));
            http.Dispose(); cierre.Dispose();
        }
        // Las respuestas del doble usan los mismos nombres JSON que el protocolo HTTP.
        internal static HttpResponseMessage Json(object cuerpo, HttpStatusCode codigo = HttpStatusCode.OK) => new(codigo)
        { Content = new StringContent(JsonSerializer.Serialize(cuerpo, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json") };
    }

    // El reloj manual permite probar intervalos de reintento sin esperar segundos reales.
    internal sealed class RelojCliente : TimeProvider
    {
        private DateTimeOffset ahora = DateTimeOffset.UtcNow;
        private readonly List<Temporizador> temporizadores = [];
        public override DateTimeOffset GetUtcNow() { lock (temporizadores) return ahora; }
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            lock (temporizadores)
            {
                // Cada espera registra un temporizador que el avance manual puede despertar.
                var timer = new Temporizador(this, callback, state);
                temporizadores.Add(timer); timer.Change(dueTime, period); return timer;
            }
        }
        internal void Avanzar(TimeSpan intervalo)
        {
            lock (temporizadores)
            {
                // Se despiertan únicamente los temporizadores vencidos que todavía no se cerraron.
                ahora += intervalo;
                foreach (var t in temporizadores.ToArray())
                    if (!t.Cerrado && t.Vence <= ahora) { t.Vence = DateTimeOffset.MaxValue; t.Callback(t.State); }
            }
        }
        private sealed class Temporizador(RelojCliente reloj, TimerCallback callback, object? state) : ITimer
        {
            internal bool Cerrado;
            internal DateTimeOffset Vence;
            internal TimerCallback Callback = callback;
            internal object? State = state;
            // El plazo infinito representa un temporizador suspendido hasta una nueva programación.
            public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                Vence = dueTime == Timeout.InfiniteTimeSpan ? DateTimeOffset.MaxValue : reloj.ahora + dueTime;
                return !Cerrado;
            }
            public void Dispose() => Cerrado = true;
            public ValueTask DisposeAsync() { Dispose(); return default; }
        }
    }
}
