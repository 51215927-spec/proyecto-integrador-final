using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Proyecto_Integrador_Final.Api;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class IntegracionRespuestaPerdidaTests
{
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

    // La espera comprueba fallos del bucle y termina por plazo si no aparece el estado esperado.
    private static async Task Esperar(Func<bool> condicion, Task sesion)
    {
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        while (!condicion())
        {
            if (sesion.IsCompleted) { await sesion; Assert.Fail("La sesión terminó antes de confirmar la elección."); }
            await Task.Delay(20, limite.Token);
        }
    }

    // Reenvía HTTP a Kestrel real y corta una única respuesta. No simula recibos.
    private sealed class CorteHttp(bool despues, string rutaCorte = "elecciones") : DelegatingHandler(new HttpClientHandler())
    {
        // El corte ocurre una sola vez y las colas registran tráfico sin exponer credenciales.
        private bool cortada;
        internal bool ConfirmadaAntesDelCorte;
        internal ConcurrentQueue<string> Cuerpos = new();
        internal ConcurrentQueue<string> Rutas = new();
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
    }

    // El canal proporciona teclas de forma asíncrona mientras el cliente consulta el servidor.
    private sealed class Teclas : TextReader
    {
        private readonly Channel<string> canal = Channel.CreateUnbounded<string>();
        internal void Enviar(string tecla = "") => canal.Writer.TryWrite(tecla);
        public override ValueTask<string?> ReadLineAsync(CancellationToken ct) => Leer(ct);
        private async ValueTask<string?> Leer(CancellationToken ct) => await canal.Reader.ReadAsync(ct);
    }

    // La lectura de aserciones y la escritura del cliente comparten un bloqueo de salida.
    private sealed class Salida : StringWriter
    {
        private readonly object bloqueo = new();
        public override void WriteLine(string? value) { lock (bloqueo) base.WriteLine(value); }
        public override void Write(string? value) { lock (bloqueo) base.Write(value); }
        internal int Longitud { get { lock (bloqueo) return ToString().Length; } }
        internal bool Contiene(string texto) { lock (bloqueo) return ToString().Contains(texto); }
    }
}
