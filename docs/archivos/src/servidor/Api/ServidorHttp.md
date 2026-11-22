# ServidorHttp.cs

Fuente: [src/servidor/Api/ServidorHttp.cs](../../../../../src/servidor/Api/ServidorHttp.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Construye el host ASP.NET Core, configura JSON y dependencias, traduce errores a respuestas y registra rutas del protocolo. No arranca la escucha: [Program](../Program.md) llama StartAsync. [RepositorioJuego](RepositorioJuego.md) es el dueño de las transacciones y autenticación de plazas.

## Dependencias

ASP.NET Core aporta WebApplication, middleware, grupos de rutas, DI y Results. EF Core/SQLite clasifican errores de almacenamiento. SHA256/UTF8 y FixedTimeEquals comprueban secreto administrativo; AuthenticationHeaderValue parsea Bearer. [ConfiguracionServidor](ConfiguracionServidor.md) suministra inicio; [LotePersonajes](LotePersonajes.md), [EstadoPartida](EstadoPartida.md) y [PresentacionPartida](PresentacionPartida.md) definen entradas/salidas. [Protocolo](../../../../protocolo-de-partida.md) es la guía operativa compartida.

## Estructura y estado

`internal sealed class ErrorApi(int estado,string codigo,string mensaje) : Exception(mensaje)` conserva propiedades internas get-only `int Estado` y `string Codigo`, e hereda Message. Declara fábricas y Responder. `internal static class ServidorHttp` declara Crear, Credencial e IdSesion. Crear contiene función local ExigirCatalogo y lambdas de configuración, middleware y handlers. No hay partial, eventos ni campos estáticos propios.

## Miembros y ejecución

### ErrorApi

El constructor primario guarda estado/código y pasa mensaje a Exception. `internal static ErrorApi Entrada()` crea 400/entradaInvalida; `NoEncontrado()` 404/recursoNoEncontrado; `Indisponible()` 503/servicioNoDisponible. No lanzan por sí mismas. `internal Task Responder(HttpContext contexto)` fija StatusCode y retorna WriteAsJsonAsync con Codigo, mensaje=Message e idSeguimiento=TraceIdentifier; no incluye stack ni credenciales. La configuración JSON web serializa Codigo como `codigo`.

### `internal static WebApplication Crear(string[] args,TimeProvider? reloj = null,ConfiguracionServidor? inicio = null)`

Crea builder con args. Si inicio no null agrega configuración en memoria: urls, cadena SQLite con DataSource=inicio.Archivo y DefaultTimeout=1 segundo, Catalogo:Credencial; además UseUrls. Si configuración urls sigue null usa http://0.0.0.0:5000. Registra repositorio singleton con cadena configurada o `Data Source=guerra.db;Default Timeout=1`, y TimeProvider suministrado o System. La lambda de DI obtiene reloj al materializar repositorio; no crea una instancia por petición. JSON NumberHandling.Strict impide convertir strings a números. ThrowOnBadRequest permite traducir fallos de binding a error común.

Tras Build agrega middleware asíncrono: await siguiente; captura ErrorApi y responde con sus datos; BadHttpRequestException → 400; SqliteException → 503; DbUpdateException solo si InnerException es SqliteException → 503. Otras excepciones no se convierten aquí. Configura rutas y devuelve app sin StartAsync.

### Autenticación y funciones auxiliares

`private static string? Credencial(HttpContext contexto)` usa TryParse Authorization; acepta esquema Bearer ignorando caso y devuelve Parameter, o null para cabecera ausente/inválida/esquema diferente. No recorta manualmente secreto. `private static Guid IdSesion(HttpContext contexto)` parsea X-Sesion o devuelve Guid.Empty, que será rechazado por el acceso del repositorio.

La función local `void ExigirCatalogo(HttpContext contexto)` lee Catalogo:Credencial y Bearer; secreto configurado null/blanco, recibido null o hashes SHA256 diferentes fallan 401 credencialInvalida. Compara hashes con FixedTimeEquals. Se aplica tanto al resumen como al reemplazo y alta; secreto vacío deshabilita las tres rutas administrativas, sin acceso anónimo.

```csharp
        void ExigirCatalogo(HttpContext contexto)
        {
            var esperada = app.Configuration["Catalogo:Credencial"];
            var recibida = Credencial(contexto);
            if (string.IsNullOrWhiteSpace(esperada) || recibida is null ||
                !CryptographicOperations.FixedTimeEquals(
                    SHA256.HashData(Encoding.UTF8.GetBytes(esperada)),
                    SHA256.HashData(Encoding.UTF8.GetBytes(recibida))))
                throw new ErrorApi(401, "credencialInvalida", "La credencial no es válida.");
        }
```

### Rutas y lambdas de handlers

Las rutas de partidas tienen prefijo `/api/v1/partidas/{id}`, con id Guid; Bearer identifica plaza y X-Sesion su sesión vigente salvo incorporación, que lleva UUID en cuerpo. No hay endpoint público para crear/iniciar partidas: eso se hace en consola.

| Método y ruta | Entrada/autenticación → llamada | Salida y efecto |
|---|---|---|
| POST `/api/v1/personajes` | AltaPersonaje + secreto catálogo → DarDeAlta | 201 `{idPersonaje}`; inserta una carta activa con ID automático. |
| GET `/api/v1/personajes/resumen` | Secreto catálogo → CantidadCatalogo | 200 `{cantidad}`; cuenta activos e inactivos, sin mutar. |
| PUT `/api/v1/personajes` | JsonElement array + secreto catálogo → Leer y ReemplazarCatalogo | 200 `{cantidad}`; reemplazo atómico completo, no acumulación. |
| PUT `.../participantes/me/sesion` | Incorporacion {idSesion,nombre}, Bearer → Incorporar | 200 Sesion {rol,idSesion,venceUtc}; fija nombre inicial y presencia. No requiere X-Sesion. |
| PUT `.../participantes/me/presencia` | Bearer/X-Sesion, sin cuerpo → Renovar | 204; prolonga presencia, no revisión. |
| GET `.../presentacion` | Bearer/X-Sesion → Presentacion | 200 PresentacionVisible; Cache-Control no-store; no muta ni renueva. |
| POST `.../elecciones` | Eleccion + Bearer/X-Sesion → Elegir | Primera 201 + Location `.../elecciones/{idOperacion}`; repetida idéntica 200. Ambas retornan Recibo. |
| GET `.../elecciones/{idOperacion}` | Guid operación + Bearer/X-Sesion → ConsultarRecibo | 200 Recibo propio o 404; no muta. |
| PUT `.../participantes/me/abandono` | Bearer/X-Sesion, sin cuerpo → Abandonar | 204; finaliza por abandono o conserva repetición del mismo rol. |

Fragmento real que distingue confirmación nueva de reintento:

```csharp
                var resultado = juego.Elegir(id, Credencial(contexto), IdSesion(contexto), datos);
                return resultado.Repetida ? Results.Ok(resultado.Recibo)
                    : Results.Created($"/api/v1/partidas/{id}/elecciones/{datos.IdOperacion}", resultado.Recibo);
```

Los contratos de carta/elección/sesión se documentan en sus páginas enlazadas. Binding inválido produce 400; dominio del catálogo también 400; credencial inválida 401; nombre registrado distinto 403; recurso/recibo inexistente 404; sesión ocupada/vencida, pausa, revisión desactualizada, solicitud no vigente u operación incompatible 409; carta/atributo no permitido 422; almacenamiento no disponible 503. El orden exacto de validaciones por operación está en repositorio/transiciones.

### Fragmentos de configuración, traducción y efectos HTTP

Crear incorpora configuración interactiva si se suministra; en ausencia de URL usa la dirección de escucha por defecto. El repositorio singleton recibe el reloj registrado y SQLite con timeout de un segundo:

```csharp
        if (inicio is not null)
        {
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["urls"] = inicio.Direccion,
                ["ConnectionStrings:Juego"] = new SqliteConnectionStringBuilder
                {
                    DataSource = inicio.Archivo, DefaultTimeout = 1
                }.ToString(),
                ["Catalogo:Credencial"] = inicio.Credencial
            });
            builder.WebHost.UseUrls(inicio.Direccion);
        }
        if (builder.Configuration["urls"] is null)
            builder.WebHost.UseUrls("http://0.0.0.0:5000");
        builder.Services.AddSingleton(sp => new RepositorioJuego(
            builder.Configuration.GetConnectionString("Juego") ?? "Data Source=guerra.db;Default Timeout=1",
            sp.GetRequiredService<TimeProvider>()));
        builder.Services.AddSingleton(reloj ?? TimeProvider.System);
        builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
        builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
```

El middleware convierte únicamente las excepciones enumeradas; `DbUpdateException` requiere SQLite interno. No promete convertir todos los fallos en un error del protocolo:

```csharp
            catch (ErrorApi error)
            {
                await error.Responder(contexto);
            }
            catch (BadHttpRequestException)
            {
                await ErrorApi.Entrada().Responder(contexto);
            }
            catch (SqliteException)
            {
                await ErrorApi.Indisponible().Responder(contexto);
            }
            catch (DbUpdateException error) when (error.InnerException is SqliteException)
            {
                await ErrorApi.Indisponible().Responder(contexto);
            }
        });
```

Los tres handlers del catálogo autentican antes de invocar operaciones. Resumen no escribe; PUT primero interpreta el lote y luego llama al reemplazo transaccional:

```csharp
        app.MapGet("/api/v1/personajes/resumen", (HttpContext contexto, RepositorioJuego juego) =>
        {
            ExigirCatalogo(contexto);
            return Results.Ok(new { cantidad = juego.CantidadCatalogo() });
        });
        app.MapPut("/api/v1/personajes", (System.Text.Json.JsonElement datos, HttpContext contexto, RepositorioJuego juego) =>
        {
            ExigirCatalogo(contexto);
            return Results.Ok(new { cantidad = juego.ReemplazarCatalogo(LotePersonajes.Leer(datos)) });
        });
```

La extracción de cabeceras no lanza por formato inválido: devuelve null o Guid.Empty, para que las reglas de acceso respondan con el error correspondiente:

```csharp
    private static string? Credencial(HttpContext contexto) =>
        AuthenticationHeaderValue.TryParse(contexto.Request.Headers.Authorization, out var cabecera)
            && cabecera.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)
            ? cabecera.Parameter : null;

    private static Guid IdSesion(HttpContext contexto) =>
        Guid.TryParse(contexto.Request.Headers["X-Sesion"], out var id) ? id : Guid.Empty;
```

## Recorridos y ejemplos

Normal: GET resumen con secreto ficticio correcto devuelve cantidad 6; PUT con seis objetos válidos devuelve cantidad 6 y reemplaza en una transacción. Una elección nueva devuelve 201, reenviar mismo cuerpo y UUID devuelve 200 con el mismo recibo.

Alternativo: secreto configurado vacío causa 401 incluso al consultar resumen. Una incorporación con X-Sesion inválido todavía puede proceder porque usa idSesion del cuerpo; una presentación con ese X-Sesion falla sesionVencida. Un puntaje JSON `"650"` es string y falla binding estricto antes del handler de alta. Ejemplos contrastados por lectura y pruebas definidas, sin petición real aquí.

## Errores, límites y efectos observables

No redirige ni configura HTTPS/autorización de operador remotamente. Credencial viaja en cabecera Bearer, no en URL ni cuerpo de error. No hay rate limiting, paginación ni captura universal de excepciones. Configuración de host puede recibir otras fuentes cuando se llama Crear sin inicio; el ejecutable de servidor usa arreglo args vacío e inicio interactivo. La identidad de sesión no es una credencial independiente y siempre se valida junto a Bearer.

## Relaciones y verificación

[ApiHttpTests](../../../test/proyecto-integrador-final-tests/ApiHttpTests.md) define rutas, códigos, privacidad y binding estricto. [CargaJsonTests](../../../test/proyecto-integrador-final-tests/CargaJsonTests.md) define autenticación y atomicidad HTTP del catálogo. [ServidorContratoTests](../../../test/proyecto-integrador-final-tests/ServidorContratoTests.md) define protocolos por proceso. Consultar [plan de pruebas](../../../../plan-de-pruebas.md) para resultados reales; estas definiciones no acreditan ejecución de esta página.
