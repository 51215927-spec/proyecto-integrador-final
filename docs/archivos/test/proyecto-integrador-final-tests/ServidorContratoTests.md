# ServidorContratoTests.cs

Fuente: [test/proyecto-integrador-final-tests/ServidorContratoTests.cs](../../../../test/proyecto-integrador-final-tests/ServidorContratoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Contratos externos del ejecutable servidor: consola operativa, Kestrel y SQLite en directorio temporal. Requieren GUERRA_SERVIDOR_DLL; no se sustituyen por llamadas directas al repositorio.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [Jugador](../../src/servidor/Jugador.md), [Partida](../../src/servidor/Partida.md), [Personaje](../../src/servidor/Personaje.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ServidorContratoTests`.

Declaración: `private sealed record Eleccion(Guid idOperacion, Guid idSolicitud, int revisionEsperada, int idPersonaje, string? atributo);`.

Declaración: `private sealed record Jugador(string Rol, string Credencial, Guid Sesion);`.

Declaración: `private sealed class Servidor : IAsyncDisposable`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Namespace global. Dll privado const=GUERRA_SERVIDOR_DLL. Records privados Eleccion(Guid idOperacion,Guid idSolicitud,int revisionEsperada,int idPersonaje,string? atributo) y Jugador(string Rol,string Credencial,Guid Sesion) conservan datos por propiedades init y igualdad por valor, permitiendo with. Servidor privado implementa IAsyncDisposable: CredencialCatalogo const de ensayo; directorio GUID readonly; Process mutable; HttpClient con timeout5s reemplazado al reiniciar; Guid partida inicialmente vacío; J1/J2 públicas con private set inicialmente null! hasta Crear.

Declaraciones reales de estado de `ServidorContratoTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private const string Dll = "GUERRA_SERVIDOR_DLL";
```

Declaraciones reales de estado de `Servidor` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private const string CredencialCatalogo = "catalogo-de-pruebas";
```

```csharp
private readonly string directorio = Path.Combine(Path.GetTempPath(), $"guerra-{Guid.NewGuid():N}");
```

```csharp
private Process proceso;
```

```csharp
private HttpClient http = new() { Timeout = TimeSpan.FromSeconds(5) };
```

```csharp
private Guid partida;
```

```csharp
public Jugador J1 { get; private set; } = null!;
```

```csharp
public Jugador J2 { get; private set; } = null!;
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ServidorContratoTests.Incorporacion_ValidaCredencialYSesionYNoCambiaRevisionConPresencia`

Crea servidor y partida por consola. PUT sesión con token incorrecto exige401 credencialInvalida; incorpora J1 y exige esperandoInicio, mano vacía y números de ronda/combate null. UUID de sesión nuevo con plaza ocupada da409 sesionOcupada; usar esa sesión para GET da409 sesionVencida. Presencia de sesión correcta da204 y revisión se mantiene. await using detiene proceso y borra cwd.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ServidorContratoTests.Elecciones_ProtegenManosYDevuelvenElMismoReciboAlReintentar`

Inicia fixture completo; ambas manos tienen3 IDs distintos y ninguna intersección, J2 sin solicitud inicial. J2 no puede usar solicitud J1 (409); J1 no puede elegir carta de J2 (422), revisión intacta. POST válido201 tiene IDs originales, revisión+1 y Location; repetir da200 igual, GET propio200 igual, GET ajeno404. Mismo ID operación con otra carta da409 operacionIncompatible. Vista J2 conserva mano, resultados vacíos y solicitud carta sin atributos. Operación nueva sobre revisión previa da409 cuyo código puede ser revisionDesactualizada o solicitudNoVigente. Respuesta J2 revela un resultado idéntico para ambos y manos2.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ServidorContratoTests.EleccionesConcurrentes_ConsumenUnaSolaSolicitud`

Prepara dos operaciones distintas sobre misma solicitud y cartas diferentes. Task.WhenAll debe producir exactamente un201; identifica perdedora. Si perdió con503 (contención SQLite), la reintenta y exige409; si no, ya debe ser409. Revisión final es previa+1 y GET de recibo perdedor da404. Permite cualquiera como vencedora y no afirma que nunca exista503 transitorio.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ServidorContratoTests.SesionAusente_PausaYRecuperaLaMismaSolicitudSinCambiarRevision`

Guarda vista y elección de apertura, cierra J2 por consola y espera Pausada true; revisión igual y solicitud null. Intentar elección guardada da409 partidaPausada. Reincorpora J2 con UUID nuevo y mismo nombre, espera no pausada, misma revisión, mano e ID solicitud; elección original entonces da201. No espera caducidad real: usa acción administrativa.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ServidorContratoTests.Partida_AlternaElElectorEnEmpatesYFinalizaAlLlegarAQuinceRondas`

Fixture carga seis cartas iguales. Bucles rondas1..29 y combates1..3 renuevan presencias por ronda, mano inicial J1 en impares/J2 en pares y alternan tras cada combate. Comprueban ronda, combate y mano4−combate; cada apertura/respuesta da201. Final J2: fase finalizada, motivo victoria, solicitud null, ronda29 y87 resultados. No fija orden del barajado; atributos iguales hacen determinista al vencedor.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ServidorContratoTests.Reinicio_InvalidaSesionesYConservaEstadoYReciboConfirmados`

Acepta primera operación J1 y guarda recibo/vista J2. Reinicia mismo archivo; GET con sesión vieja da409 sesionVencida. Reincorpora ambos con UUID nuevos y mismos nombres. Vista J2 conserva revisión, fase esperandoRival, conjunto ordenado de mano e ID solicitud; consulta recibo anterior y compara revisionConfirmada. No acredita continuidad de una conexión TCP anterior.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ServidorContratoTests.Abandono_RepetirloConservaMarcadorYRevisionFinal`

Inicia y guarda vista; PUT abandono J2 da204, lee final, repite da204. Final debe ser finalizada/abandono/solicitud null; marcador textual coincide con previo, revisión+1 y segunda consulta mantiene revisión final. No declara ganador en esta prueba ni premia al rival.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `ServidorContratoTests.Revision`

private static int: extrae propiedad revision con GetInt32; JSON faltante o no entero falla, no usa valor por defecto.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static int Revision(JsonElement vista) => vista.GetProperty("revision").GetInt32();
```

### `ServidorContratoTests.Mano`

private static int[]: enumera mano, proyecta cada idPersonaje a Int32 y materializa nuevo array conservando orden de presentación. No filtra ni consulta catálogo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static int[] Mano(JsonElement vista) => vista.GetProperty("mano").EnumerateArray()
        .Select(carta => carta.GetProperty("idPersonaje").GetInt32()).ToArray();
```

### `ServidorContratoTests.Elegir`

private static Eleccion: toma solicitud de vista y crea UUID operación nuevo, UUID solicitud, revisión por auxiliar Revision y primer ID permitido. Si atributosPermitidos está vacío usa null; de lo contrario primer string. Asume solicitud habilitada y array de cartas no vacío; no optimiza estrategia.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static Eleccion Elegir(JsonElement vista)
    {
        var solicitud = vista.GetProperty("solicitud");
        var atributos = solicitud.GetProperty("atributosPermitidos");
        return new(Guid.NewGuid(), solicitud.GetProperty("idSolicitud").GetGuid(), Revision(vista),
            solicitud.GetProperty("idsPersonajePermitidos")[0].GetInt32(),
            atributos.GetArrayLength() == 0 ? null : atributos[0].GetString());
    }
```

### `ServidorContratoTests.Leer`

private static async Task<JsonElement>: lee body completo; Assert.True exige estado esperado incluyendo en fallo estados numéricos y cuerpo. Parse crea JsonDocument con using y retorna RootElement.Clone para sobrevivir al Dispose. No libera respuesta: caller es dueño.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static async Task<JsonElement> Leer(HttpResponseMessage respuesta, HttpStatusCode esperado)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.True(respuesta.StatusCode == esperado,
            $"Se esperaba HTTP {(int)esperado}, se recibió {(int)respuesta.StatusCode}. {cuerpo}");
        using var json = JsonDocument.Parse(cuerpo);
        return json.RootElement.Clone();
    }
```

### `ServidorContratoTests.Error`

private static async Task: posee respuesta mediante using, llama Leer con estado esperado, compara codigo y exige mensaje/idSeguimiento no blancos. Propaga errores de JSON y libera incluso al fallar aserción.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `Servidor.Servidor`

Constructor público del tipo privado: crea directorio de ensayo y asigna proceso=Lanzar(). No crea partida automáticamente; Crear o Iniciar debe hacerlo antes de usar J1/J2.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public Servidor()
        {
            Directory.CreateDirectory(directorio);
            proceso = Lanzar();
        }
```

### `Servidor.Lanzar`

private Process: toma ruta absoluta de GUERRA_SERVIDOR_DLL y exige File.Exists. Inicia dotnet en directorio temporal con stdin/stdout redirigidos y sin shell; silencia logs ASP.NET a Warning. Escribe por stdin escucha127.0.0.1:0, guerra.db y credencial catálogo de prueba, hace Flush y retorna proceso. No espera todavía Now listening; LeerDato lo hará. El archivo de base queda en cwd aislado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
            inicio.Environment["Logging__LogLevel__Microsoft.AspNetCore"] = "Warning";
            var lanzado = Process.Start(inicio)!;
            lanzado.StandardInput.WriteLine("http://127.0.0.1:0");
            lanzado.StandardInput.WriteLine("guerra.db");
            lanzado.StandardInput.WriteLine(CredencialCatalogo);
            lanzado.StandardInput.Flush();
            return lanzado;
        }
```

### `Servidor.Reiniciar`

public async Task: Detener proceso actual, Lanzar otro en mismo directorio/base, libera HttpClient anterior y crea nuevo timeout5s. Espera Now listening on y fija nueva BaseAddress. J1/J2 y partida se conservan en fixture, pero las sesiones del servidor reiniciado no.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async Task Reiniciar()
        {
            await Detener();
            proceso = Lanzar();
            http.Dispose();
            http = new() { Timeout = TimeSpan.FromSeconds(5) };
            http.BaseAddress = new Uri(await LeerDato("Now listening on: "));
        }
```

### `Servidor.Crear`

public async Task: primero lee URL de escucha para fijar BaseAddress; escribe crear en consola; lee Partida y parsea Guid, luego prefijos J1 y J2 para credenciales y asigna records con UUID de sesión nuevos. Exige credenciales diferentes. No incorpora jugadores ni llena catálogo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async Task Crear()
        {
            http.BaseAddress = new Uri(await LeerDato("Now listening on: "));
            await proceso.StandardInput.WriteLineAsync("crear");
            partida = Guid.Parse(await LeerDato("Partida "));
            J1 = new("J1", await LeerDato("J1 "), Guid.NewGuid());
            J2 = new("J2", await LeerDato("J2 "), Guid.NewGuid());
            Assert.NotEqual(J1.Credencial, J2.Credencial);
        }
```

### `Servidor.Iniciar`

public async Task: llama Crear, bucle i1..6 hace POST personajes con token administrativo y atributos500/650/350/500/500/500, exige201 e ID>0. Incorpora Lucía/J1 y Mateo/J2; escribe iniciar y UUID por consola; espera vista J1 esperandoElector antes de retornar. No reutiliza catálogo de otra prueba.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
            await proceso.StandardInput.WriteLineAsync($"iniciar\n{partida}");
            await EsperarVista(J1, vista => vista.GetProperty("fase").GetString() == "esperandoElector");
        }
```

### `Servidor.Renovar`

public async Task: PUT participantes/me/presencia mediante Enviar y exige204; using libera respuesta. No cambia fixture ni revisión.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async Task Renovar(Jugador jugador)
        {
            using var presencia = await Enviar(HttpMethod.Put, "participantes/me/presencia", jugador);
            Assert.Equal(HttpStatusCode.NoContent, presencia.StatusCode);
        }
```

### `Servidor.CerrarSesion`

public Task: escribe cerrar-sesion, UUID de partida y Rol del record, separados por saltos. No espera confirmación textual; el caller espera efecto con EsperarVista.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public Task CerrarSesion(Jugador jugador) =>
            proceso.StandardInput.WriteLineAsync($"cerrar-sesion\n{partida}\n{jugador.Rol}");
```

### `Servidor.EsperarVista`

public async Task<JsonElement>: CTS5s; bucle consulta Vista y aplica Func coincide, devuelve primera coincidencia; de lo contrario Delay50ms con token. Errores HTTP/aserciones de Vista fallan de inmediato; timeout interrumpe demora. No reintenta datos incorrectos ni reinicia proceso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `Servidor.Incorporar`

public async Task: PUT sesión con UUID y nombre del argumento; Leer exige200, comprueba rol y UUID iguales y venceUtc parseable como DateTimeOffset. No exige que vencimiento sea futuro ni renueva por sí sola después.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async Task Incorporar(Jugador jugador, string nombre)
        {
            using var respuesta = await Enviar(HttpMethod.Put, "participantes/me/sesion", jugador,
                new { idSesion = jugador.Sesion, nombre });
            var sesion = await Leer(respuesta, HttpStatusCode.OK);
            Assert.Equal(jugador.Rol, sesion.GetProperty("rol").GetString());
            Assert.Equal(jugador.Sesion, sesion.GetProperty("idSesion").GetGuid());
            Assert.True(sesion.GetProperty("venceUtc").TryGetDateTimeOffset(out _));
        }
```

### `Servidor.Vista`

public async Task<JsonElement>: GET presentación, Leer200, exige CacheControl.NoStore true, idPartida del fixture y rolPropio del jugador. Devuelve JSON clonado por Leer. No altera mano ni turno.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async Task<JsonElement> Vista(Jugador jugador)
        {
            using var respuesta = await Enviar(HttpMethod.Get, "presentacion", jugador);
            var vista = await Leer(respuesta, HttpStatusCode.OK);
            Assert.True(respuesta.Headers.CacheControl?.NoStore);
            Assert.Equal(partida, vista.GetProperty("idPartida").GetGuid());
            Assert.Equal(jugador.Rol, vista.GetProperty("rolPropio").GetString());
            return vista;
        }
```

### `Servidor.Post`

public Task<HttpResponseMessage>: delega Enviar POST elecciones con record de operación y devuelve respuesta sin consumir. Caller debe liberar.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public Task<HttpResponseMessage> Post(Jugador jugador, Eleccion eleccion) =>
            Enviar(HttpMethod.Post, "elecciones", jugador, eleccion);
```

### `Servidor.Enviar`

public async Task<HttpResponseMessage>: construye ruta absoluta /api/v1/partidas/<partida>/<ruta>, agrega Bearer y X-Sesion del record; si cuerpo no null lo serializa con JsonContent.Create. using libera request tras SendAsync y devuelve response para caller. Sin CancellationToken explícito: timeout del HttpClient5s gobierna la solicitud.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async Task<HttpResponseMessage> Enviar(HttpMethod metodo, string ruta, Jugador jugador, object? cuerpo = null)
        {
            using var solicitud = new HttpRequestMessage(metodo, $"/api/v1/partidas/{partida}/{ruta}");
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jugador.Credencial);
            solicitud.Headers.Add("X-Sesion", jugador.Sesion.ToString());
            if (cuerpo is not null)
                solicitud.Content = JsonContent.Create(cuerpo);
            return await http.SendAsync(solicitud);
        }
```

### `Servidor.LeerDato`

private async Task<string>: CTS15s, lee líneas de stdout, Trim y compara StartsWith ordinal con prefijo. Si coincide devuelve sufijo sin prefijo; ignora líneas ajenas. EOF sin coincidencia lanza InvalidOperationException; timeout propaga cancelación. No imprime valores de credenciales.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

### `Servidor.DisposeAsync`

public async ValueTask: libera http, espera Detener y borra recursivamente directorio temporal. No elimina worktree ni archivos de usuario; solo ruta GUID creada por fixture.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async ValueTask DisposeAsync()
        {
            http.Dispose();
            await Detener();
            Directory.Delete(directorio, recursive: true);
        }
```

### `Servidor.Detener`

private async Task: si proceso vivo mata árbol completo, espera salida siempre y libera Process. No usa comando de apagado ni acredita cierre amable; se busca liberar proceso antes de borrar base.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private async Task Detener()
        {
            if (!proceso.HasExited)
                proceso.Kill(entireProcessTree: true);
            await proceso.WaitForExitAsync();
            proceso.Dispose();
        }
```

## Recorridos y ejemplos

Normal: Dos plazas con tres cartas disjuntas alternan electores; empates dan29 rondas/87 combates y final J2.

Alternativo o de error: Credencial incorrecta401, sesión ajena409, elección inválida422, recibo ajeno404; reinicio conserva solicitud/recibo pero invalida sesiones.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
