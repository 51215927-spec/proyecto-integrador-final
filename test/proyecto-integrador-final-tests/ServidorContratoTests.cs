using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public sealed class ServidorContratoTests
{
    private const string Dll = "GUERRA_SERVIDOR_DLL";

    [ContratoFact(Dll)]
    public async Task Incorporacion_ValidaCredencialYSesionYNoCambiaRevisionConPresencia()
    {
        await using var juego = new Servidor();
        await juego.Crear();
        var j1 = juego.J1;

        await Error(await juego.Enviar(HttpMethod.Put, "participantes/me/sesion",
            j1 with { Credencial = "incorrecta" }, new { idSesion = j1.Sesion, nombre = "Lucía" }),
            HttpStatusCode.Unauthorized, "credencialInvalida");
        await juego.Incorporar(j1, "Lucía");
        var inicial = await juego.Vista(j1);
        Assert.Equal("esperandoInicio", inicial.GetProperty("fase").GetString());
        Assert.Empty(inicial.GetProperty("mano").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, inicial.GetProperty("numeroRonda").ValueKind);
        Assert.Equal(JsonValueKind.Null, inicial.GetProperty("numeroCombate").ValueKind);

        var otra = j1 with { Sesion = Guid.NewGuid() };
        await Error(await juego.Enviar(HttpMethod.Put, "participantes/me/sesion", otra,
            new { idSesion = otra.Sesion, nombre = "Lucía" }), HttpStatusCode.Conflict, "sesionOcupada");
        await Error(await juego.Enviar(HttpMethod.Get, "presentacion", otra),
            HttpStatusCode.Conflict, "sesionVencida");
        using var presencia = await juego.Enviar(HttpMethod.Put, "participantes/me/presencia", j1);
        Assert.Equal(HttpStatusCode.NoContent, presencia.StatusCode);
        Assert.Equal(Revision(inicial), Revision(await juego.Vista(j1)));
    }

    [ContratoFact(Dll)]
    public async Task Elecciones_ProtegenManosYDevuelvenElMismoReciboAlReintentar()
    {
        await using var juego = new Servidor();
        await juego.Iniciar();
        var vista1 = await juego.Vista(juego.J1);
        var vista2 = await juego.Vista(juego.J2);
        var mano1 = Mano(vista1);
        var mano2 = Mano(vista2);
        Assert.Equal(3, mano1.Distinct().Count());
        Assert.Equal(3, mano2.Distinct().Count());
        Assert.Empty(mano1.Intersect(mano2));
        Assert.Equal(JsonValueKind.Null, vista2.GetProperty("solicitud").ValueKind);

        var eleccion = Elegir(vista1);
        await Error(await juego.Post(juego.J2, eleccion),
            HttpStatusCode.Conflict, "solicitudNoVigente");
        await Error(await juego.Post(juego.J1, eleccion with { idPersonaje = mano2[0] }),
            HttpStatusCode.UnprocessableEntity, "eleccionInvalida");
        Assert.Equal(Revision(vista1), Revision(await juego.Vista(juego.J1)));

        using var aceptada = await juego.Post(juego.J1, eleccion);
        var recibo = await Leer(aceptada, HttpStatusCode.Created);
        Assert.Equal(eleccion.idOperacion, recibo.GetProperty("idOperacion").GetGuid());
        Assert.Equal(eleccion.idSolicitud, recibo.GetProperty("idSolicitud").GetGuid());
        Assert.Equal(Revision(vista1) + 1, recibo.GetProperty("revisionConfirmada").GetInt32());
        Assert.EndsWith($"/elecciones/{eleccion.idOperacion}", aceptada.Headers.Location?.ToString());
        using var repetida = await juego.Post(juego.J1, eleccion);
        Assert.Equal(recibo.ToString(), (await Leer(repetida, HttpStatusCode.OK)).ToString());
        using var consulta = await juego.Enviar(HttpMethod.Get, $"elecciones/{eleccion.idOperacion}", juego.J1);
        Assert.Equal(recibo.ToString(), (await Leer(consulta, HttpStatusCode.OK)).ToString());
        await Error(await juego.Enviar(HttpMethod.Get, $"elecciones/{eleccion.idOperacion}", juego.J2),
            HttpStatusCode.NotFound, "recursoNoEncontrado");
        await Error(await juego.Post(juego.J1, eleccion with { idPersonaje = mano1.First(id => id != eleccion.idPersonaje) }),
            HttpStatusCode.Conflict, "operacionIncompatible");

        var esperando = await juego.Vista(juego.J2);
        Assert.Equal(Revision(vista1) + 1, Revision(esperando));
        Assert.Equal("esperandoRival", esperando.GetProperty("fase").GetString());
        Assert.Equal(mano2, Mano(esperando));
        Assert.Empty(esperando.GetProperty("resultados").EnumerateArray());
        var solicitud = esperando.GetProperty("solicitud");
        Assert.Equal("carta", solicitud.GetProperty("tipo").GetString());
        Assert.Empty(solicitud.GetProperty("atributosPermitidos").EnumerateArray());

        using var desactualizada = await juego.Post(juego.J1, eleccion with { idOperacion = Guid.NewGuid() });
        var errorDesactualizada = await Leer(desactualizada, HttpStatusCode.Conflict);
        Assert.Contains(errorDesactualizada.GetProperty("codigo").GetString(),
            new[] { "revisionDesactualizada", "solicitudNoVigente" });

        using var respuesta = await juego.Post(juego.J2, Elegir(esperando));
        await Leer(respuesta, HttpStatusCode.Created);
        var resultado1 = Assert.Single((await juego.Vista(juego.J1)).GetProperty("resultados").EnumerateArray());
        var resultado2 = Assert.Single((await juego.Vista(juego.J2)).GetProperty("resultados").EnumerateArray());
        Assert.Equal(resultado1.ToString(), resultado2.ToString());
        Assert.Equal(2, Mano(await juego.Vista(juego.J1)).Length);
        Assert.Equal(2, Mano(await juego.Vista(juego.J2)).Length);
    }

    [ContratoFact(Dll)]
    public async Task EleccionesConcurrentes_ConsumenUnaSolaSolicitud()
    {
        await using var juego = new Servidor();
        await juego.Iniciar();
        var vista = await juego.Vista(juego.J1);
        var primera = Elegir(vista);
        var segunda = primera with
        {
            idOperacion = Guid.NewGuid(),
            idPersonaje = Mano(vista).First(id => id != primera.idPersonaje)
        };
        var respuestas = await Task.WhenAll(juego.Post(juego.J1, primera), juego.Post(juego.J1, segunda));
        using var respuesta1 = respuestas[0];
        using var respuesta2 = respuestas[1];
        Assert.Single(respuestas, respuesta => respuesta.StatusCode == HttpStatusCode.Created);
        var indice = Array.FindIndex(respuestas, respuesta => respuesta.StatusCode != HttpStatusCode.Created);
        var perdedora = indice == 0 ? primera : segunda;
        if (respuestas[indice].StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            using var reintento = await juego.Post(juego.J1, perdedora);
            Assert.Equal(HttpStatusCode.Conflict, reintento.StatusCode);
        }
        else
            Assert.Equal(HttpStatusCode.Conflict, respuestas[indice].StatusCode);
        Assert.Equal(Revision(vista) + 1, Revision(await juego.Vista(juego.J1)));
        await Error(await juego.Enviar(HttpMethod.Get, $"elecciones/{perdedora.idOperacion}", juego.J1),
            HttpStatusCode.NotFound, "recursoNoEncontrado");
    }

    [ContratoFact(Dll)]
    public async Task SesionAusente_PausaYRecuperaLaMismaSolicitudSinCambiarRevision()
    {
        await using var juego = new Servidor();
        await juego.Iniciar();
        var previa = await juego.Vista(juego.J1);
        var pendiente = Elegir(previa);

        await juego.CerrarSesion(juego.J2);
        var pausada = await juego.EsperarVista(juego.J1, vista => vista.GetProperty("pausada").GetBoolean());
        Assert.Equal(Revision(previa), Revision(pausada));
        Assert.Equal(JsonValueKind.Null, pausada.GetProperty("solicitud").ValueKind);
        await Error(await juego.Post(juego.J1, pendiente), HttpStatusCode.Conflict, "partidaPausada");

        await juego.Incorporar(juego.J2 with { Sesion = Guid.NewGuid() }, "Mateo");
        var reanudada = await juego.EsperarVista(juego.J1, vista => !vista.GetProperty("pausada").GetBoolean());
        Assert.Equal(Revision(previa), Revision(reanudada));
        Assert.Equal(Mano(previa), Mano(reanudada));
        Assert.Equal(pendiente.idSolicitud, reanudada.GetProperty("solicitud").GetProperty("idSolicitud").GetGuid());
        using var respuesta = await juego.Post(juego.J1, pendiente);
        await Leer(respuesta, HttpStatusCode.Created);
    }

    [ContratoFact(Dll)]
    public async Task Partida_AlternaElElectorEnEmpatesYFinalizaAlLlegarAQuinceRondas()
    {
        await using var juego = new Servidor();
        await juego.Iniciar();

        // Las seis cartas tienen los mismos puntajes. El que responde gana cada empate.
        // En cada ronda termina 2–1 y gana quien respondió en el primer combate.
        for (var ronda = 1; ronda <= 29; ronda++)
        {
            await juego.Renovar(juego.J1);
            await juego.Renovar(juego.J2);
            var elector = ronda % 2 == 1 ? juego.J1 : juego.J2;
            var rival = elector == juego.J1 ? juego.J2 : juego.J1;
            for (var combate = 1; combate <= 3; combate++)
            {
                var apertura = await juego.Vista(elector);
                Assert.Equal(ronda, apertura.GetProperty("numeroRonda").GetInt32());
                Assert.Equal(combate, apertura.GetProperty("numeroCombate").GetInt32());
                Assert.Equal(4 - combate, Mano(apertura).Length);
                using var primera = await juego.Post(elector, Elegir(apertura));
                await Leer(primera, HttpStatusCode.Created);

                var respuesta = await juego.Vista(rival);
                using var segunda = await juego.Post(rival, Elegir(respuesta));
                await Leer(segunda, HttpStatusCode.Created);
                (elector, rival) = (rival, elector);
            }
        }

        var final = await juego.Vista(juego.J2);
        Assert.Equal("finalizada", final.GetProperty("fase").GetString());
        Assert.Equal("victoria", final.GetProperty("motivoFin").GetString());
        Assert.Equal("J2", final.GetProperty("ganador").GetString());
        Assert.Equal(JsonValueKind.Null, final.GetProperty("solicitud").ValueKind);
        Assert.Equal(29, final.GetProperty("numeroRonda").GetInt32());
        Assert.Equal(87, final.GetProperty("resultados").GetArrayLength());
    }

    [ContratoFact(Dll)]
    public async Task Reinicio_InvalidaSesionesYConservaEstadoYReciboConfirmados()
    {
        await using var juego = new Servidor();
        await juego.Iniciar();
        var operacion = Elegir(await juego.Vista(juego.J1));
        using var aceptada = await juego.Post(juego.J1, operacion);
        var recibo = await Leer(aceptada, HttpStatusCode.Created);
        var previa = await juego.Vista(juego.J2);

        await juego.Reiniciar();
        await Error(await juego.Enviar(HttpMethod.Get, "presentacion", juego.J1),
            HttpStatusCode.Conflict, "sesionVencida");
        var j1 = juego.J1 with { Sesion = Guid.NewGuid() };
        var j2 = juego.J2 with { Sesion = Guid.NewGuid() };
        await juego.Incorporar(j1, "Lucía");
        await juego.Incorporar(j2, "Mateo");

        var recuperada = await juego.Vista(j2);
        Assert.Equal(Revision(previa), Revision(recuperada));
        Assert.Equal("esperandoRival", recuperada.GetProperty("fase").GetString());
        Assert.Equal(Mano(previa).Order(), Mano(recuperada).Order());
        Assert.Equal(previa.GetProperty("solicitud").GetProperty("idSolicitud").GetGuid(),
            recuperada.GetProperty("solicitud").GetProperty("idSolicitud").GetGuid());
        using var consulta = await juego.Enviar(HttpMethod.Get, $"elecciones/{operacion.idOperacion}", j1);
        var recuperado = await Leer(consulta, HttpStatusCode.OK);
        Assert.Equal(recibo.GetProperty("revisionConfirmada").GetInt32(),
            recuperado.GetProperty("revisionConfirmada").GetInt32());
    }

    [ContratoFact(Dll)]
    public async Task Abandono_RepetirloConservaMarcadorYRevisionFinal()
    {
        await using var juego = new Servidor();
        await juego.Iniciar();
        var previa = await juego.Vista(juego.J1);
        using var primero = await juego.Enviar(HttpMethod.Put, "participantes/me/abandono", juego.J2);
        Assert.Equal(HttpStatusCode.NoContent, primero.StatusCode);
        var final = await juego.Vista(juego.J1);
        using var repetido = await juego.Enviar(HttpMethod.Put, "participantes/me/abandono", juego.J2);
        Assert.Equal(HttpStatusCode.NoContent, repetido.StatusCode);
        Assert.Equal("finalizada", final.GetProperty("fase").GetString());
        Assert.Equal("abandono", final.GetProperty("motivoFin").GetString());
        Assert.Equal(JsonValueKind.Null, final.GetProperty("solicitud").ValueKind);
        Assert.Equal(previa.GetProperty("marcador").ToString(), final.GetProperty("marcador").ToString());
        Assert.Equal(Revision(previa) + 1, Revision(final));
        Assert.Equal(Revision(final), Revision(await juego.Vista(juego.J1)));
    }

    private static int Revision(JsonElement vista) => vista.GetProperty("revision").GetInt32();

    private static int[] Mano(JsonElement vista) => vista.GetProperty("mano").EnumerateArray()
        .Select(carta => carta.GetProperty("idPersonaje").GetInt32()).ToArray();

    private static Eleccion Elegir(JsonElement vista)
    {
        var solicitud = vista.GetProperty("solicitud");
        var atributos = solicitud.GetProperty("atributosPermitidos");
        return new(Guid.NewGuid(), solicitud.GetProperty("idSolicitud").GetGuid(), Revision(vista),
            solicitud.GetProperty("idsPersonajePermitidos")[0].GetInt32(),
            atributos.GetArrayLength() == 0 ? null : atributos[0].GetString());
    }

    private static async Task<JsonElement> Leer(HttpResponseMessage respuesta, HttpStatusCode esperado)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.True(respuesta.StatusCode == esperado,
            $"Se esperaba HTTP {(int)esperado}, se recibió {(int)respuesta.StatusCode}. {cuerpo}");
        using var json = JsonDocument.Parse(cuerpo);
        return json.RootElement.Clone();
    }

    private static async Task Error(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)
    {
        using (respuesta)
        {
            var error = await Leer(respuesta, estado);
            Assert.Equal(codigo, error.GetProperty("codigo").GetString());
            Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("mensaje").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("idSeguimiento").GetString()));
        }
    }

    private sealed record Eleccion(Guid idOperacion, Guid idSolicitud, int revisionEsperada,
        int idPersonaje, string? atributo);
    private sealed record Jugador(string Rol, string Credencial, Guid Sesion);

    private sealed class Servidor : IAsyncDisposable
    {
        private const string CredencialCatalogo = "catalogo-de-pruebas";
        private readonly string directorio = Path.Combine(Path.GetTempPath(), $"guerra-{Guid.NewGuid():N}");
        private Process proceso;
        private HttpClient http = new() { Timeout = TimeSpan.FromSeconds(5) };
        private Guid partida;
        public Jugador J1 { get; private set; } = null!;
        public Jugador J2 { get; private set; } = null!;

        public Servidor()
        {
            Directory.CreateDirectory(directorio);
            proceso = Lanzar();
        }

        private Process Lanzar()
        {
            var dll = Path.GetFullPath(Environment.GetEnvironmentVariable(Dll)!);
            Assert.True(File.Exists(dll), $"No existe la DLL en {dll}");
            var inicio = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directorio, UseShellExecute = false,
                RedirectStandardInput = true, RedirectStandardOutput = true
            };
            inicio.ArgumentList.Add(dll);
            inicio.ArgumentList.Add("--urls");
            inicio.ArgumentList.Add("http://127.0.0.1:0");
            inicio.Environment["ConnectionStrings__Juego"] = "Data Source=guerra.db;Default Timeout=1";
            inicio.Environment["Catalogo__Credencial"] = CredencialCatalogo;
            inicio.Environment["Logging__LogLevel__Microsoft.AspNetCore"] = "Warning";
            return Process.Start(inicio)!;
        }

        public async Task Reiniciar()
        {
            await Detener();
            proceso = Lanzar();
            http.Dispose();
            http = new() { Timeout = TimeSpan.FromSeconds(5) };
            http.BaseAddress = new Uri(await LeerDato("Now listening on: "));
        }

        public async Task Crear()
        {
            http.BaseAddress = new Uri(await LeerDato("Now listening on: "));
            await proceso.StandardInput.WriteLineAsync("crear");
            partida = Guid.Parse(await LeerDato("Partida "));
            J1 = new("J1", await LeerDato("J1 "), Guid.NewGuid());
            J2 = new("J2", await LeerDato("J2 "), Guid.NewGuid());
            Assert.NotEqual(J1.Credencial, J2.Credencial);
        }

        public async Task Iniciar()
        {
            await Crear();
            for (var i = 1; i <= 6; i++)
            {
                using var alta = new HttpRequestMessage(HttpMethod.Post, "/api/v1/personajes");
                alta.Headers.Authorization = new("Bearer", CredencialCatalogo);
                alta.Content = JsonContent.Create(new
                {
                    nombre = $"Personaje {i}", descripcion = "", ascii = "O",
                    fuerza = 500, velocidad = 650, resistencia = 350,
                    inteligencia = 500, energia = 500, habilidad = 500
                });
                using var respuesta = await http.SendAsync(alta);
                Assert.True((await Leer(respuesta, HttpStatusCode.Created)).GetProperty("idPersonaje").GetInt32() > 0);
            }
            await Incorporar(J1, "Lucía");
            await Incorporar(J2, "Mateo");
            await proceso.StandardInput.WriteLineAsync($"iniciar {partida}");
            await EsperarVista(J1, vista => vista.GetProperty("fase").GetString() == "esperandoElector");
        }

        public async Task Renovar(Jugador jugador)
        {
            using var presencia = await Enviar(HttpMethod.Put, "participantes/me/presencia", jugador);
            Assert.Equal(HttpStatusCode.NoContent, presencia.StatusCode);
        }

        public Task CerrarSesion(Jugador jugador) =>
            proceso.StandardInput.WriteLineAsync($"cerrar-sesion {partida} {jugador.Rol}");

        public async Task<JsonElement> EsperarVista(Jugador jugador, Func<JsonElement, bool> coincide)
        {
            using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (true)
            {
                var vista = await Vista(jugador);
                if (coincide(vista))
                    return vista;
                await Task.Delay(50, plazo.Token);
            }
        }

        public async Task Incorporar(Jugador jugador, string nombre)
        {
            using var respuesta = await Enviar(HttpMethod.Put, "participantes/me/sesion", jugador,
                new { idSesion = jugador.Sesion, nombre });
            var sesion = await Leer(respuesta, HttpStatusCode.OK);
            Assert.Equal(jugador.Rol, sesion.GetProperty("rol").GetString());
            Assert.Equal(jugador.Sesion, sesion.GetProperty("idSesion").GetGuid());
            Assert.True(sesion.GetProperty("venceUtc").TryGetDateTimeOffset(out _));
        }

        public async Task<JsonElement> Vista(Jugador jugador)
        {
            using var respuesta = await Enviar(HttpMethod.Get, "presentacion", jugador);
            var vista = await Leer(respuesta, HttpStatusCode.OK);
            Assert.True(respuesta.Headers.CacheControl?.NoStore);
            Assert.Equal(partida, vista.GetProperty("idPartida").GetGuid());
            Assert.Equal(jugador.Rol, vista.GetProperty("rolPropio").GetString());
            return vista;
        }

        public Task<HttpResponseMessage> Post(Jugador jugador, Eleccion eleccion) =>
            Enviar(HttpMethod.Post, "elecciones", jugador, eleccion);

        public async Task<HttpResponseMessage> Enviar(HttpMethod metodo, string ruta, Jugador jugador, object? cuerpo = null)
        {
            using var solicitud = new HttpRequestMessage(metodo, $"/api/v1/partidas/{partida}/{ruta}");
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jugador.Credencial);
            solicitud.Headers.Add("X-Sesion", jugador.Sesion.ToString());
            if (cuerpo is not null)
                solicitud.Content = JsonContent.Create(cuerpo);
            return await http.SendAsync(solicitud);
        }

        private async Task<string> LeerDato(string prefijo)
        {
            using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            while (await proceso.StandardOutput.ReadLineAsync(plazo.Token) is { } linea)
            {
                linea = linea.Trim();
                if (linea.StartsWith(prefijo, StringComparison.Ordinal))
                    return linea[prefijo.Length..];
            }
            throw new InvalidOperationException($"El servidor cerró la consola antes de emitir {prefijo}.");
        }

        public async ValueTask DisposeAsync()
        {
            http.Dispose();
            await Detener();
            Directory.Delete(directorio, recursive: true);
        }

        private async Task Detener()
        {
            if (!proceso.HasExited)
                proceso.Kill(entireProcessTree: true);
            await proceso.WaitForExitAsync();
            proceso.Dispose();
        }
    }
}
