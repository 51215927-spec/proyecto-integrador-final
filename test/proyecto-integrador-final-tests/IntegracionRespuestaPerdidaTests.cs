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
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-red-{Guid.NewGuid():N}.db");
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
            using var corte = new CorteHttp(confirmarAntesDelCorte);
            using var cliente = new ClientePartidaHttp(config, Guid.NewGuid(), corte);
            await cliente.Incorporar(default);
            juego.Iniciar(partida.Id);
            var inicial = await cliente.Presentacion(default);
            using var cierre = new CancellationTokenSource(TimeSpan.FromSeconds(12));
            var entrada = new Teclas();
            var salida = new Salida();
            var sesion = new SesionPartidaCliente(config, cliente, new InterfazConsola(entrada, salida));
            var tarea = sesion.Ejecutar(new EntradaCliente(entrada), cierre.Token);
            try
            {
                await Esperar(() => salida.Contiene("Tu turno"), tarea);
                entrada.Enviar();
                await Esperar(() => salida.Contiene("Atributo del combate"), tarea);
                entrada.Enviar();
                await Esperar(() => salida.Contiene("Recepción confirmada"), tarea);
                var actual = await rival.Presentacion(default);
                Assert.Equal(inicial.Revision + 1, actual.Revision);
                Assert.Empty(actual.Resultados);
                Assert.Equal("carta", actual.Solicitud!.Tipo);
                string[] cuerpos = corte.Cuerpos.ToArray();
                Assert.Equal(confirmarAntesDelCorte ? 1 : 2, cuerpos.Length);
                Assert.All(cuerpos, cuerpo => Assert.Equal(cuerpos[0], cuerpo));
                string[] rutas = corte.Rutas.ToArray();
                int primerPost = Array.FindIndex(rutas, r => r == "POST elecciones");
                int recibo = Array.FindIndex(rutas, primerPost + 1, r => r.StartsWith("GET elecciones/"));
                Assert.True(recibo > primerPost);
                if (!confirmarAntesDelCorte)
                    Assert.True(Array.FindLastIndex(rutas, r => r == "POST elecciones") > recibo);
                Assert.Equal(confirmarAntesDelCorte, corte.ConfirmadaAntesDelCorte);
                await rival.Elegir(new(Guid.NewGuid(), actual.Solicitud.IdSolicitud, actual.Revision, actual.Solicitud.IdsPersonajePermitidos[0], null), default);
                var resultado = await rival.Presentacion(default);
                Assert.Single(resultado.Resultados);
                Assert.Equal(inicial.Revision + 2, resultado.Revision);
            }
            finally
            {
                cierre.Cancel();
                await tarea.WaitAsync(TimeSpan.FromSeconds(3));
            }
            await app.StopAsync();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }

    [Fact]
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
            using var j1 = new ClientePartidaHttp(config, Guid.NewGuid());
            using var j2 = new ClientePartidaHttp(config with { Credencial = partida.CredencialJ2, Nombre = "Luis" }, Guid.NewGuid());
            await j1.Incorporar(default);
            await j2.Incorporar(default);
            juego.Iniciar(partida.Id);
            int elecciones = 0;
            PresentacionCliente a, b;
            do
            {
                Assert.True(elecciones < 300, "La partida no llegó a quince rondas.");
                await j1.Renovar(default);
                await j2.Renovar(default);
                a = await j1.Presentacion(default);
                b = await j2.Presentacion(default);
                Assert.Equal(a.Revision, b.Revision);
                Assert.Equal(a.Marcador.OrderBy(x => x.Key), b.Marcador.OrderBy(x => x.Key));
                Assert.Equal(JsonSerializer.Serialize(a.Resultados), JsonSerializer.Serialize(b.Resultados));
                Assert.Empty(a.Mano.Select(x => x.IdPersonaje).Intersect(b.Mano.Select(x => x.IdPersonaje)));
                if (a.Fase == "finalizada") break;
                Assert.NotEqual(a.Solicitud is null, b.Solicitud is null);
                var vista = a.Solicitud is not null ? a : b;
                var transporte = a.Solicitud is not null ? j1 : j2;
                var solicitud = vista.Solicitud!;
                if (solicitud.Tipo == "cartaYAtributo")
                {
                    string elector = (vista.NumeroRonda + vista.NumeroCombate) % 2 == 0 ? "J1" : "J2";
                    Assert.Equal(elector, vista.RolPropio);
                    Assert.Equal(4 - vista.NumeroCombate, a.Mano.Length);
                    Assert.Equal(4 - vista.NumeroCombate, b.Mano.Length);
                    var usadas = vista.Resultados.Where(r => r.NumeroRonda == vista.NumeroRonda)
                        .SelectMany(r => r.Cartas.Values).Select(c => c.IdPersonaje);
                    Assert.Empty(a.Mano.Concat(b.Mano).Select(c => c.IdPersonaje).Intersect(usadas));
                }
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
            Assert.Equal(29, a.NumeroRonda);
            Assert.Equal(87, a.Resultados.Length);
            Assert.All(a.Resultados.GroupBy(r => r.NumeroRonda), ronda =>
            {
                Assert.Equal(3, ronda.Count());
                Assert.Equal(2, ronda.Count(r => r.GanadorCombate == ronda.Last().GanadorRonda));
            });
            Assert.Null(a.Solicitud);
            Assert.Null(b.Solicitud);
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
    public async Task AbandonoInciertoSoloMuestraFinConfirmadoSinSumarVictoria(bool confirmarAntesDelCorte)
    {
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
            using var corte = new CorteHttp(confirmarAntesDelCorte, "participantes/me/abandono");
            using var cliente = new ClientePartidaHttp(config, Guid.NewGuid(), corte);
            await cliente.Incorporar(default);
            juego.Iniciar(partida.Id);
            var inicial = await rival.Presentacion(default);
            using var cierre = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var entrada = new Teclas();
            var salida = new Salida();
            var tarea = new SesionPartidaCliente(config, cliente, new InterfazConsola(entrada, salida))
                .Ejecutar(new EntradaCliente(entrada), cierre.Token);
            try
            {
                await Esperar(() => salida.Contiene("Tu turno"), tarea);
                await Tecla("esc");
                await Tecla("s");
                await Tecla("s");
                await Tecla("");
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
                await Esperar(() => salida.Contiene("Partida finalizada por abandono"), tarea);
                var final = await rival.Presentacion(default);
                Assert.Equal("abandono", final.MotivoFin);
                Assert.Null(final.Ganador);
                Assert.Null(final.Solicitud);
                Assert.Equal(inicial.Revision + 1, final.Revision);
                Assert.Equal(inicial.Marcador.OrderBy(x => x.Key), final.Marcador.OrderBy(x => x.Key));
                Assert.Equal(confirmarAntesDelCorte, corte.ConfirmadaAntesDelCorte);
                Assert.Equal(confirmarAntesDelCorte ? 1 : 2,
                    corte.Rutas.Count(r => r == "PUT participantes/me/abandono"));
                Assert.DoesNotContain(corte.Rutas, r => r == "POST elecciones");
                await cliente.Abandonar(default);
                Assert.Equal(final.Revision, (await rival.Presentacion(default)).Revision);
            }
            finally { cierre.Cancel(); await tarea.WaitAsync(TimeSpan.FromSeconds(3)); }
            await app.StopAsync();

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
        private bool cortada;
        internal bool ConfirmadaAntesDelCorte;
        internal ConcurrentQueue<string> Cuerpos = new();
        internal ConcurrentQueue<string> Rutas = new();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            string ruta = request.RequestUri!.AbsolutePath.Split("/partidas/")[1];
            ruta = ruta[(ruta.IndexOf('/') + 1)..];
            Rutas.Enqueue($"{request.Method} {ruta}");
            bool eleccion = request.Method == HttpMethod.Post && ruta == "elecciones";
            if (eleccion) Cuerpos.Enqueue(await request.Content!.ReadAsStringAsync(ct));
            if (ruta == rutaCorte && request.Method != HttpMethod.Get && !cortada)
            {
                cortada = true;
                if (despues)
                {
                    using var respuesta = await base.SendAsync(request, ct);
                    respuesta.EnsureSuccessStatusCode();
                    await respuesta.Content.ReadAsByteArrayAsync(ct);
                    ConfirmadaAntesDelCorte = true;
                }
                throw new HttpRequestException("Corte controlado de ensayo");
            }
            return await base.SendAsync(request, ct);
        }
    }

    private sealed class Teclas : TextReader
    {
        private readonly Channel<string> canal = Channel.CreateUnbounded<string>();
        internal void Enviar(string tecla = "") => canal.Writer.TryWrite(tecla);
        public override ValueTask<string?> ReadLineAsync(CancellationToken ct) => Leer(ct);
        private async ValueTask<string?> Leer(CancellationToken ct) => await canal.Reader.ReadAsync(ct);
    }

    private sealed class Salida : StringWriter
    {
        private readonly object bloqueo = new();
        public override void WriteLine(string? value) { lock (bloqueo) base.WriteLine(value); }
        public override void Write(string? value) { lock (bloqueo) base.Write(value); }
        internal int Longitud { get { lock (bloqueo) return ToString().Length; } }
        internal bool Contiene(string texto) { lock (bloqueo) return ToString().Contains(texto); }
    }
}
