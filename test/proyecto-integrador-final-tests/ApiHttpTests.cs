using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class ApiHttpTests
{
    [Fact]
    public async Task PresentacionDePartidaDesconocidaDevuelveErrorDelProtocolo()
    {
        await using WebApplication app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        await app.StartAsync();
        using var cliente = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

        using var respuesta = await cliente.GetAsync($"/api/v1/partidas/{Guid.NewGuid()}/presentacion");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        using var error = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal("recursoNoEncontrado", error.RootElement.GetProperty("codigo").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.RootElement.GetProperty("idSeguimiento").GetString()));
    }
    [Fact]
    public async Task IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        var creada = app.Services.GetRequiredService<CoordinadorPartidas>().CrearPartida();
        await app.StartAsync();
        using var cliente = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", creada.CredencialJ1);
        var ruta = $"/api/v1/partidas/{creada.Id}/participantes/me/sesion";
        var sesion = new { idSesion = Guid.NewGuid(), nombre = "Ada" };

        using var primera = await cliente.PutAsJsonAsync(ruta, sesion);
        using var repetida = await cliente.PutAsJsonAsync(ruta, sesion);
        using var ocupada = await cliente.PutAsJsonAsync(ruta, new { idSesion = Guid.NewGuid(), nombre = "Ada" });

        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repetida.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, ocupada.StatusCode);
        using var cuerpo = JsonDocument.Parse(await primera.Content.ReadAsStringAsync());
        Assert.Equal("J1", cuerpo.RootElement.GetProperty("rol").GetString());
        Assert.Equal(sesion.idSesion, cuerpo.RootElement.GetProperty("idSesion").GetGuid());
    }

    [Fact]
    public async Task AltaExigeCredencialAdministrativaYValidaAtributos()
    {
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0",
            "--Catalogo:Credencial=credencial-de-prueba"]);
        await app.StartAsync();
        using var cliente = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        var ruta = "/api/v1/personajes";
        var datos = new
        {
            nombre = "Ada", descripcion = "", ascii = "*",
            fuerza = 750, velocidad = 550, resistencia = 500,
            inteligencia = 250, energia = 500, habilidad = 450
        };

        using var anonima = await cliente.PostAsJsonAsync(ruta, datos);
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", "credencial-de-prueba");
        using var invalida = await cliente.PostAsJsonAsync(ruta, datos with { fuerza = 751 });
        using var valida = await cliente.PostAsJsonAsync(ruta, datos);

        Assert.Equal(HttpStatusCode.Unauthorized, anonima.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalida.StatusCode);
        Assert.Equal(HttpStatusCode.Created, valida.StatusCode);
        using var cuerpo = JsonDocument.Parse(await valida.Content.ReadAsStringAsync());
        Assert.True(cuerpo.RootElement.GetProperty("idPersonaje").GetInt32() > 0);
    }

    [Fact]
    public async Task CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:",
            "--urls=http://127.0.0.1:0"]);
        var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
        var partida = juego.CrearPartida();
        for (int i = 0; i < 6; i++)
            app.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
        await app.StartAsync();
        using var j1 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ1, "Ada");
        using var j2 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ2, "Ana");
        juego.Iniciar(partida.Id);

        var inicial = await Vista(j1);
        var turno = inicial.GetProperty("solicitud");
        var eleccion = new
        {
            idOperacion = Guid.NewGuid(),
            idSolicitud = turno.GetProperty("idSolicitud").GetGuid(),
            revisionEsperada = inicial.GetProperty("revision").GetInt32(),
            idPersonaje = turno.GetProperty("idsPersonajePermitidos")[0].GetInt32(),
            atributo = "Fuerza"
        };
        using var ajena = await j2.PostAsJsonAsync("elecciones", eleccion);
        await Error(ajena, HttpStatusCode.Conflict, "solicitudNoVigente");
        using var invalida = await j1.PostAsJsonAsync("elecciones", eleccion with { atributo = "Suerte" });
        await Error(invalida, HttpStatusCode.UnprocessableEntity, "eleccionInvalida");

        var alternativa = eleccion with
        {
            idOperacion = Guid.NewGuid(),
            idPersonaje = turno.GetProperty("idsPersonajePermitidos")[1].GetInt32()
        };
        var respuestas = await Task.WhenAll(
            j1.PostAsJsonAsync("elecciones", eleccion), j1.PostAsJsonAsync("elecciones", alternativa));
        using var aceptada = Assert.Single(respuestas, r => r.StatusCode == HttpStatusCode.Created);
        using var descartada = Assert.Single(respuestas, r => r.StatusCode == HttpStatusCode.Conflict);
        await Error(descartada, HttpStatusCode.Conflict, "revisionDesactualizada");
        if (ReferenceEquals(aceptada, respuestas[1]))
            eleccion = alternativa;
        using var incompatible = await j1.PostAsJsonAsync("elecciones",
            eleccion with { idPersonaje = -1 });
        await Error(incompatible, HttpStatusCode.Conflict, "operacionIncompatible");
        using var repetida = await j1.PostAsJsonAsync("elecciones", eleccion);
        Assert.Equal(HttpStatusCode.OK, repetida.StatusCode);
        Assert.Equal(await aceptada.Content.ReadAsStringAsync(), await repetida.Content.ReadAsStringAsync());
        using var reciboAjeno = await j2.GetAsync(aceptada.Headers.Location!);
        Assert.Equal(HttpStatusCode.NotFound, reciboAjeno.StatusCode);

        var vistaRival = await Vista(j2);
        Assert.Equal(0, vistaRival.GetProperty("resultados").GetArrayLength());
        Assert.All(vistaRival.GetProperty("mano").EnumerateArray(),
            carta => Assert.NotEqual(eleccion.idPersonaje, carta.GetProperty("idPersonaje").GetInt32()));
        var turnoRival = vistaRival.GetProperty("solicitud");
        Assert.Equal("carta", turnoRival.GetProperty("tipo").GetString());
        Assert.Equal(0, turnoRival.GetProperty("atributosPermitidos").GetArrayLength());

        using var respuesta = await j2.PostAsJsonAsync("elecciones", new
        {
            idOperacion = Guid.NewGuid(),
            idSolicitud = turnoRival.GetProperty("idSolicitud").GetGuid(),
            revisionEsperada = vistaRival.GetProperty("revision").GetInt32(),
            idPersonaje = turnoRival.GetProperty("idsPersonajePermitidos")[0].GetInt32()
        });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        var final = await Vista(j1);
        var resultado = Assert.Single(final.GetProperty("resultados").EnumerateArray());
        Assert.Equal("Fuerza", resultado.GetProperty("atributo").GetString());
        Assert.Equal("J2", resultado.GetProperty("ganadorCombate").GetString());
        Assert.Equal(2, final.GetProperty("mano").GetArrayLength());
    }

    [Fact]
    public async Task ReinicioRecuperaSolicitudYRecibosYExigeSesionesNuevas()
    {
        var directorio = Directory.CreateTempSubdirectory();
        string[] args = [$"--ConnectionStrings:Juego=Data Source={Path.Combine(directorio.FullName, "juego.db")};Pooling=false",
            "--urls=http://127.0.0.1:0"];
        try
        {
            PartidaCreada partida;
            Guid sesionAntigua;
            JsonElement antes;
            Uri recibo;
            await using (var app = ServidorHttp.Crear(args))
            {
                var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
                partida = juego.CrearPartida();
                for (int i = 0; i < 6; i++)
                    app.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
                await app.StartAsync();
                using var j1 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ1, "Ada");
                using var j2 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ2, "Ana");
                sesionAntigua = Guid.Parse(j1.DefaultRequestHeaders.GetValues("X-Sesion").Single());
                juego.Iniciar(partida.Id);
                var vista = await Vista(j1);
                using var elegida = await j1.PostAsJsonAsync("elecciones", DatosEleccion(vista, "Fuerza"));
                Assert.Equal(HttpStatusCode.Created, elegida.StatusCode);
                recibo = elegida.Headers.Location!;
                antes = await Vista(j2);
                app.Services.GetRequiredService<CatalogoPersonajes>().Desactivar(antes.GetProperty("mano")[0].GetProperty("idPersonaje").GetInt32());
                await app.StopAsync();
            }

            await using (var app = ServidorHttp.Crear(args))
            {
                await app.StartAsync();
                string url = app.Urls.Single();
                using var vieja = new HttpClient { BaseAddress = new Uri(url) };
                vieja.DefaultRequestHeaders.Authorization = new("Bearer", partida.CredencialJ1);
                vieja.DefaultRequestHeaders.Add("X-Sesion", sesionAntigua.ToString());
                using var vencida = await vieja.GetAsync($"/api/v1/partidas/{partida.Id}/presentacion");
                Assert.Equal(HttpStatusCode.Conflict, vencida.StatusCode);

                using var j2 = await Incorporar(url, partida.Id, partida.CredencialJ2, "Ana");
                var pausada = await Vista(j2);
                Assert.True(pausada.GetProperty("pausada").GetBoolean());
                Assert.Equal(JsonValueKind.Null, pausada.GetProperty("solicitud").ValueKind);
                using var bloqueada = await j2.PostAsJsonAsync("elecciones", DatosEleccion(antes, null));
                Assert.Equal(HttpStatusCode.Conflict, bloqueada.StatusCode);
                var error = await bloqueada.Content.ReadFromJsonAsync<JsonElement>();
                Assert.Equal("partidaPausada", error.GetProperty("codigo").GetString());

                using var j1 = await Incorporar(url, partida.Id, partida.CredencialJ1, "Ada");
                var retomada = await Vista(j2);
                Assert.Equal(antes.GetRawText(), retomada.GetRawText());
                using var confirmada = await j1.GetAsync(recibo);
                Assert.Equal(HttpStatusCode.OK, confirmada.StatusCode);
                using var abandono = await j2.PutAsync("participantes/me/abandono", null);
                using var repetido = await j2.PutAsync("participantes/me/abandono", null);
                Assert.Equal(HttpStatusCode.NoContent, abandono.StatusCode);
                Assert.Equal(HttpStatusCode.NoContent, repetido.StatusCode);
                var final = await Vista(j1);
                Assert.Equal("abandono", final.GetProperty("motivoFin").GetString());
                Assert.False(final.TryGetProperty("ganador", out _));
                Assert.Equal(retomada.GetProperty("revision").GetInt32() + 1,
                    final.GetProperty("revision").GetInt32());
                await app.StopAsync();
            }
        }
        finally
        {
            directorio.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales()
    {
        var directorio = Directory.CreateTempSubdirectory();
        string conexion = $"Data Source={Path.Combine(directorio.FullName, "juego.db")};Pooling=false";
        try
        {
            await using var app = ServidorHttp.Crear([
                $"--ConnectionStrings:Juego={conexion}", "--urls=http://127.0.0.1:0"]);
            var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
            var partida = juego.CrearPartida();
            for (int i = 0; i < 6; i++)
                app.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
            await app.StartAsync();
            using var j1 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ1, "Ada");
            using var j2 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ2, "Ana");
            juego.Iniciar(partida.Id);
            using var delElector = await j1.PostAsJsonAsync("elecciones", DatosEleccion(await Vista(j1), "Habilidad"));
            Assert.Equal(HttpStatusCode.Created, delElector.StatusCode);
            var antes = await Vista(j2);
            var solicitud = antes.GetProperty("solicitud");
            var pendiente = new
            {
                idOperacion = Guid.NewGuid(),
                idSolicitud = solicitud.GetProperty("idSolicitud").GetGuid(),
                revisionEsperada = antes.GetProperty("revision").GetInt32(),
                idPersonaje = solicitud.GetProperty("idsPersonajePermitidos")[0].GetInt32()
            };

            using var sqlite = new Microsoft.Data.Sqlite.SqliteConnection(conexion);
            sqlite.Open();
            using var comando = sqlite.CreateCommand();
            comando.CommandText = """
                CREATE TRIGGER fallo_recibo BEFORE INSERT ON Operacion
                BEGIN SELECT RAISE(ABORT, 'fallo inducido'); END;
                """;
            comando.ExecuteNonQuery();

            using var fallida = await j2.PostAsJsonAsync("elecciones", pendiente);
            await Error(fallida, HttpStatusCode.ServiceUnavailable, "servicioNoDisponible");
            Assert.Equal(antes.GetRawText(), (await Vista(j2)).GetRawText());
            using var ausente = await j2.GetAsync($"elecciones/{pendiente.idOperacion}");
            await Error(ausente, HttpStatusCode.NotFound, "recursoNoEncontrado");

            comando.CommandText = "DROP TRIGGER fallo_recibo";
            comando.ExecuteNonQuery();
            using var reintento = await j2.PostAsJsonAsync("elecciones", pendiente);
            Assert.Equal(HttpStatusCode.Created, reintento.StatusCode);
            Assert.Single((await Vista(j2)).GetProperty("resultados").EnumerateArray());
            await app.StopAsync();
        }
        finally
        {
            directorio.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task PresenciaRenuevaElPlazoSinCambiarLaRevision()
    {
        var reloj = new RelojPruebas();
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"], reloj);
        var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
        var partida = juego.CrearPartida();
        await app.StartAsync();
        using var j1 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ1, "Ada");
        var antes = await Vista(j1);
        reloj.Avanzar(10);
        using var presencia = await j1.PutAsync("participantes/me/presencia", null);
        Assert.Equal(HttpStatusCode.NoContent, presencia.StatusCode);
        reloj.Avanzar(10);
        Assert.Equal(antes.GetRawText(), (await Vista(j1)).GetRawText());
        reloj.Avanzar(5);
        using var vencida = await j1.PutAsync("participantes/me/presencia", null);
        await Error(vencida, HttpStatusCode.Conflict, "sesionVencida");
    }

    [Fact]
    public async Task ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza()
    {
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
        var partida = juego.CrearPartida();
        await app.StartAsync();
        using var original = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ1, "Ada");
        var nueva = app.Services.GetRequiredService<AdministradorSesiones>().ReemplazarCredencial(partida.Id, "J1");

        using var revocada = await original.GetAsync("presentacion");
        await Error(revocada, HttpStatusCode.Unauthorized, "credencialInvalida");
        using var j1 = await Incorporar(app.Urls.Single(), partida.Id, nueva, "Ada");
        app.Services.GetRequiredService<AdministradorSesiones>().CerrarSesion(partida.Id, "J1");
        using var cerrada = await j1.GetAsync("presentacion");
        await Error(cerrada, HttpStatusCode.Conflict, "sesionVencida");

        juego.Cancelar(partida.Id);
        using var reincorporada = await Incorporar(app.Urls.Single(), partida.Id, nueva, "Ada");
        var final = await Vista(reincorporada);
        Assert.Equal("J1", final.GetProperty("rolPropio").GetString());
        Assert.Equal("cancelacion", final.GetProperty("motivoFin").GetString());
        Assert.Equal(1, final.GetProperty("revision").GetInt32());
    }

    [Fact]
    public async Task PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra()
    {
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0",
            "--Logging:LogLevel:Default=Warning"], new RelojPruebas());
        var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
        var partida = juego.CrearPartida();
        for (int i = 0; i < 6; i++)
            app.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
        await app.StartAsync();
        using var j1 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ1, "Ada");
        using var j2 = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ2, "Ana");
        juego.Iniciar(partida.Id);

        // Con atributos iguales gana el rival del elector. La mano alterna en cada ronda.
        for (int ronda = 1; ronda <= 29; ronda++)
        {
            for (int combate = 1; combate <= 3; combate++)
            {
                bool eligeJ1 = (ronda + combate) % 2 == 0;
                var elector = eligeJ1 ? j1 : j2;
                var rival = eligeJ1 ? j2 : j1;
                var vista = await Vista(elector);
                Assert.Equal(ronda, vista.GetProperty("numeroRonda").GetInt32());
                Assert.Equal(combate, vista.GetProperty("numeroCombate").GetInt32());
                Assert.Equal(4 - combate, vista.GetProperty("mano").GetArrayLength());
                using var elegida = await elector.PostAsJsonAsync("elecciones", DatosEleccion(vista, "Energía"));
                Assert.Equal(HttpStatusCode.Created, elegida.StatusCode);
                using var resuelta = await rival.PostAsJsonAsync("elecciones", DatosEleccion(await Vista(rival), null));
                Assert.Equal(HttpStatusCode.Created, resuelta.StatusCode);
            }
        }

        var final = await Vista(j1);
        Assert.Equal("finalizada", final.GetProperty("fase").GetString());
        Assert.Equal("victoria", final.GetProperty("motivoFin").GetString());
        Assert.Equal("J2", final.GetProperty("ganador").GetString());
        Assert.Equal(15, final.GetProperty("marcador").GetProperty("J2").GetInt32());
        Assert.Equal(29, final.GetProperty("numeroRonda").GetInt32());
        Assert.Equal(87, final.GetProperty("resultados").GetArrayLength());
        Assert.Equal(0, final.GetProperty("mano").GetArrayLength());
        Assert.Equal(JsonValueKind.Null, final.GetProperty("solicitud").ValueKind);
    }

    [Fact]
    public async Task AltaRechazaPuntajesRepresentadosComoTexto()
    {
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0",
            "--Catalogo:Credencial=credencial-de-prueba"]);
        await app.StartAsync();
        using var cliente = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", "credencial-de-prueba");
        using var respuesta = await cliente.PostAsJsonAsync("/api/v1/personajes", new
        {
            nombre = "Ada", descripcion = "", ascii = "*", fuerza = "750",
            velocidad = 550, resistencia = 500, inteligencia = 250, energia = 500, habilidad = 450
        });
        await Error(respuesta, HttpStatusCode.BadRequest, "entradaInvalida");
    }

    private static async Task Error(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)
    {
        Assert.Equal(estado, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(codigo, error.GetProperty("codigo").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("mensaje").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("idSeguimiento").GetString()));
    }

    private sealed class RelojPruebas : TimeProvider
    {
        private DateTimeOffset ahora = DateTimeOffset.UnixEpoch;

        public override DateTimeOffset GetUtcNow() => ahora;
        internal void Avanzar(int segundos) => ahora = ahora.AddSeconds(segundos);
    }

    private static object DatosEleccion(JsonElement vista, string? atributo)
    {
        var solicitud = vista.GetProperty("solicitud");
        return new
        {
            idOperacion = Guid.NewGuid(),
            idSolicitud = solicitud.GetProperty("idSolicitud").GetGuid(),
            revisionEsperada = vista.GetProperty("revision").GetInt32(),
            idPersonaje = solicitud.GetProperty("idsPersonajePermitidos")[0].GetInt32(),
            atributo
        };
    }

    private static async Task<HttpClient> Incorporar(string url, Guid partida, string credencial, string nombre)
    {
        var cliente = new HttpClient { BaseAddress = new Uri($"{url}/api/v1/partidas/{partida}/") };
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", credencial);
        var idSesion = Guid.NewGuid();
        using var respuesta = await cliente.PutAsJsonAsync("participantes/me/sesion", new { idSesion, nombre });
        respuesta.EnsureSuccessStatusCode();
        cliente.DefaultRequestHeaders.Add("X-Sesion", idSesion.ToString());
        return cliente;
    }

    private static async Task<JsonElement> Vista(HttpClient cliente)
    {
        using var respuesta = await cliente.GetAsync("presentacion");
        respuesta.EnsureSuccessStatusCode();
        Assert.True(respuesta.Headers.CacheControl?.NoStore);
        return await respuesta.Content.ReadFromJsonAsync<JsonElement>();
    }
}
