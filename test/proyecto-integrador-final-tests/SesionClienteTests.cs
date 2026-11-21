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

    [Fact]
    public async Task AbandonoExigeSiYSalidaLocalNoLoEnvia()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        f.Tecla("esc"); f.Tecla("s"); f.Tecla("s"); f.Tecla("");
        await f.Esperar(() => f.Texto.Contains("¿Abandonar partida?"));
        Assert.Equal(0, f.Abandonos);
        f.Tecla(""); // No por defecto
        await Task.Delay(80);
        Assert.Equal(0, f.Abandonos);
        f.Tecla("esc"); f.Tecla("s"); f.Tecla("s"); f.Tecla(""); f.Tecla("s"); f.Tecla("");
        await f.Esperar(() => f.Abandonos == 1);
    }

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
        f.Tecla("esc"); f.Tecla("s"); f.Tecla("s"); f.Tecla(""); f.Tecla("s"); f.Tecla("");
        await f.Esperar(() => intentos == 1);
        Assert.Equal(0, f.Abandonos);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Abandonos == 1);
        await f.Esperar(() => f.Texto.Contains("Partida finalizada por abandono"));
    }

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
        f.Tecla("esc"); f.Tecla("s"); f.Tecla("s"); f.Tecla(""); f.Tecla("s"); f.Tecla("");
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

    internal sealed class Lineas : TextReader
    {
        private readonly Channel<string?> canal = Channel.CreateUnbounded<string?>();
        internal void Agregar(string? linea) => canal.Writer.TryWrite(linea);
        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken) => canal.Reader.ReadAsync(cancellationToken);
    }

    internal sealed class Escenario : IAsyncDisposable
    {
        internal readonly RelojCliente Reloj = new();
        internal PresentacionCliente Vista;
        internal readonly Lineas Entrada = new();
        internal int Consultas, Presencias, Elecciones, Recibos, Abandonos;
        internal string? Perdida;
        internal Func<HttpRequestMessage, Task<HttpResponseMessage?>>? Interceptar;
        internal readonly List<string> Cuerpos = [];
        private ReciboCliente? recibo;
        private readonly CancellationTokenSource cierre = new();
        private readonly StringWriter salida = new();
        private readonly ClientePartidaHttp http;
        internal Task<bool> Tarea;
        internal string Texto => salida.ToString();
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
        internal void Tecla(string linea) => Entrada.Agregar(linea);
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
        public async ValueTask DisposeAsync()
        {
            cierre.Cancel();
            Reloj.Avanzar(TimeSpan.FromSeconds(1));
            await Tarea.WaitAsync(TimeSpan.FromSeconds(3));
            http.Dispose(); cierre.Dispose();
        }
        internal static HttpResponseMessage Json(object cuerpo, HttpStatusCode codigo = HttpStatusCode.OK) => new(codigo)
        { Content = new StringContent(JsonSerializer.Serialize(cuerpo, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json") };
    }

    internal sealed class RelojCliente : TimeProvider
    {
        private DateTimeOffset ahora = DateTimeOffset.UtcNow;
        private readonly List<Temporizador> temporizadores = [];
        public override DateTimeOffset GetUtcNow() { lock (temporizadores) return ahora; }
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            lock (temporizadores)
            {
                var timer = new Temporizador(this, callback, state);
                temporizadores.Add(timer); timer.Change(dueTime, period); return timer;
            }
        }
        internal void Avanzar(TimeSpan intervalo)
        {
            lock (temporizadores)
            {
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
