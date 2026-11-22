# ApiHttpTests.cs

Fuente: [test/proyecto-integrador-final-tests/ApiHttpTests.cs](../../../../test/proyecto-integrador-final-tests/ApiHttpTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Integración HTTP del host real en loopback: contrato de errores, catálogo, identidad, privacidad, idempotencia, transacciones, reinicio y finalización. No requiere DLL externas.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CoordinadorPartidas](../../src/servidor/Api/CoordinadorPartidas.md), [CatalogoPersonajes](../../src/servidor/Api/CatalogoPersonajes.md), [AdministradorSesiones](../../src/servidor/Api/AdministradorSesiones.md), [ServidorHttp](../../src/servidor/Api/ServidorHttp.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class ApiHttpTests`.

Declaración: `private sealed class RelojPruebas : TimeProvider`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

RelojPruebas privado deriva TimeProvider y guarda ahora=UnixEpoch; GetUtcNow lo retorna, Avanzar agrega segundos. No crea timers ni sincroniza. Cada test crea host/cliente/servicios aislados; las pruebas de persistencia usan subdirectorio temporal y Pooling=false.

Declaraciones reales de estado de `RelojPruebas` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private DateTimeOffset ahora = DateTimeOffset.UnixEpoch;
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ApiHttpTests.PresentacionDePartidaDesconocidaDevuelveErrorDelProtocolo`

Crea host memoria/loopback puerto0 y hace GET presentación de UUID nuevo sin credencial. Exige404 y código recursoNoEncontrado, más idSeguimiento no vacío. Demuestra prioridad del recurso desconocido; no exige401 para ese caso. using libera respuesta/JSON/cliente y await using host.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion`

Crea partida, usa Bearer J1 y PUT sesión con UUID/Ada; repite exactamente y después envía UUID nuevo. Exige200,200,409; primer body debe tener rolJ1 y mismo UUID. No examina el código de error del409 ni presencia posterior.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.AltaExigeCredencialAdministrativaYValidaAtributos`

Con credencial administrativa configurada, POST Ada válida anónima da401; con Bearer correcto pero fuerza751 da400; datos válidos750/550/500/250/500/450 dan201 y ID>0. Muestra autenticación y dominio, no reemplazo de lote.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez`

Carga seis cartas iguales, incorpora ambas plazas e inicia. J2 intentando solicitud J1 recibe409 solicitudNoVigente; atributo Suerte de J1 recibe422 eleccionInvalida. Lanza dos POST de J1 con mismo turno y distinto ID operación/carta en paralelo: uno201 y otro409 revisionDesactualizada. Si ganó alternativa, la usa como operación aceptada. Cambiar carta conservando ID aceptado da409 operacionIncompatible; repetir cuerpo idéntico da200 y mismo JSON. J2 no puede consultar recibo J1 (404), no ve la carta elegida ni resultados antes de responder, y su solicitud carta no ofrece atributos. Responder da201 y un único resultado Fuerza ganado por J2 por empate; mano J1 queda2. La concurrencia puede elegir cualquiera de los POST: la prueba no fija orden.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.ReinicioRecuperaSolicitudYRecibosYExigeSesionesNuevas`

Usa archivo en directorio temporal, Pooling=false. Primer host crea seis cartas/partida, incorpora plazas, acepta apertura J1 y guarda Location recibo y vista J2; desactiva una carta de mano de J2 para comprobar que snapshot no cambia. Reinicia mismo archivo: vieja sesión da409. Solo J2 reincorporado ve pausa y solicitud null; intentar elección previa da409 partidaPausada. Reincorporar J1 restaura vista J2 exactamente igual al JSON anterior y recibo GET200. Abandono J2 repetido da204 ambas veces, motivo abandono, sin propiedad ganador y revisión+1 una sola vez. finally elimina directorio.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales`

Host y archivo temporal; acepta apertura J1 y prepara respuesta J2. Crea trigger SQLite BEFORE INSERT ON Operacion que aborta. POST J2 debe responder503 servicioNoDisponible; presentación JSON queda idéntica y recibo de esa operación devuelve404 recursoNoEncontrado. Elimina trigger y reenvía mismos datos:201 y un único resultado. Confirma atomicidad de estado+recibo para este fallo inducido. Detiene host y borra subdirectorio.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.PresenciaRenuevaElPlazoSinCambiarLaRevision`

Usa RelojPruebas y memoria. Incorpora J1, guarda vista, avanza10s y PUT presencia exige204; avanza10s y vista debe ser idéntica (renovación prolongó plazo sin revisión). Avanza5s más: presencia llega exactamente al límite desde la renovación y debe devolver409 sesionVencida. No depende de esperar25 segundos reales.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza`

Incorpora J1, cambia credencial por AdministradorSesiones; consulta con vieja da401 credencialInvalida. Nueva credencial permite incorporar, pero CerrarSesion produce409 sesionVencida. Cancela partida y reincorpora con nueva credencial; vista conserva rolJ1, motivo cancelacion y revisión1. El test demuestra conservación del rol, motivo y revisión; no comprueba explícitamente la ausencia de un ganador.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra`

Host memoria con reloj detenido y logs Warning; seis cartas idénticas y ambas plazas. Dos bucles: rondas1..29 y combates1..3. Elector J1 si ronda+combate es par; cada vista debe indicar ronda/combate y mano de tamaño4−combate. Apertura con Energía y respuesta sin atributo dan201. Al final exige fase finalizada, motivo victoria, ganadorJ2, marcadorJ2=15, ronda29,87 resultados, mano vacía y solicitud null. Reloj fijo evita vencimiento; no valida tiempo de juego humano.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.AltaRechazaPuntajesRepresentadosComoTexto`

Con token administrativo válido envía POST cuyos datos son válidos salvo fuerza cadena750. Error exige400 entradaInvalida; el número como texto no se convierte silenciosamente. No es test de JSON malformado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.Error`

private static async Task: exige StatusCode esperado, deserializa JSON y compara codigo; mensaje e idSeguimiento deben tener texto no blanco. No libera respuesta porque el caller conserva using. JSON ausente o campos faltantes fallan mediante excepciones.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static async Task Error(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)
    {
        Assert.Equal(estado, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(codigo, error.GetProperty("codigo").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("mensaje").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("idSeguimiento").GetString()));
    }
```

### `RelojPruebas.GetUtcNow`

Override público retorna ahora sin avanzar ni consultar sistema; inicial UnixEpoch. Permite comparar expiraciones deterministas.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override DateTimeOffset GetUtcNow() => ahora;
```

### `RelojPruebas.Avanzar`

internal void recibe segundos (sin validar negativos) y asigna ahora.AddSeconds(segundos). No dispara callbacks ni usa demora real.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Avanzar(int segundos) => ahora = ahora.AddSeconds(segundos);
```

### `ApiHttpTests.DatosEleccion`

private static object: lee solicitud de vista y construye objeto anónimo con UUID operación nuevo, UUID solicitud, revisión actual, primer ID permitido y atributo nullable recibido. Asume solicitud no nula y array no vacío; excepciones de JSON se propagan. No busca carta óptima.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.Incorporar`

private static async Task<HttpClient>: crea cliente con base /api/v1/partidas/<id>/, Bearer credencial y UUID sesión nuevo. PUT participantes/me/sesion con nombre y UUID; EnsureSuccessStatusCode exige2xx, agrega X-Sesion y devuelve cliente al caller, que debe liberarlo. No valida campos del JSON de sesión aquí.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ApiHttpTests.Vista`

private static async Task<JsonElement>: GET presentacion, EnsureSuccessStatusCode, exige Cache-Control no-store y deserializa JSON. using libera respuesta, el JsonElement retornado proviene de la deserialización y no de documento descartado explícitamente. No decide el turno ni modifica revisión.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static async Task<JsonElement> Vista(HttpClient cliente)
    {
        using var respuesta = await cliente.GetAsync("presentacion");
        respuesta.EnsureSuccessStatusCode();
        Assert.True(respuesta.Headers.CacheControl?.NoStore);
        return await respuesta.Content.ReadFromJsonAsync<JsonElement>();
    }
```

## Recorridos y ejemplos

Normal: Cartas iguales fuerzan empates: 29 rondas de tres combates producen87 resultados, victoria J2 con15 y sin reparto siguiente.

Alternativo o de error: Un trigger que aborta insertar Operacion debe causar503 sin cambiar presentación ni crear recibo; quitarlo permite reintentar la operación original.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

## Actualización del spec 006

Preparación y fragmentos corresponden al código final: el catálogo, las sesiones y las acciones se resuelven por sus servicios separados con un AlmacenPartidas compartido. Las aserciones del contrato se conservan. Véase [auditoría006](../../verificacion.md#auditoría-del-spec-006--2026-11-22).
