# ClientePartidaHttp.cs

Fuente: [src/cliente/ClientePartidaHttp.cs](../../../../src/cliente/ClientePartidaHttp.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Es el transporte HTTP de la plaza. Construye rutas, autentica cada solicitud y convierte fallos en códigos controlados sin imprimir cuerpos ni credenciales. No decide turnos ni reintenta por sí mismo; esa política pertenece a [SesionPartidaCliente](SesionPartidaCliente.md).

## Dependencias

[ConfiguracionCliente](ConfiguracionCliente.md) fija destino/partida/credencial/nombre; [ContratosPartida](ContratosPartida.md) declara mensajes JSON. `HttpClient`, cabeceras HTTP, `System.Net.Http.Json` y `System.Text.Json` serializan y envían. [Protocolo](../../../protocolo-de-partida.md) describe la API del servidor.

## Estructura y datos

`internal sealed class FalloCliente(string codigo, bool transitorio = false) : Exception(codigo)` contiene propiedades internal de solo lectura `Codigo` y `Transitorio`, inicializadas por constructor primario. Message de Exception recibe el código, no el cuerpo remoto.

`internal sealed class ClientePartidaHttp : IDisposable`: campos privados readonly `HttpClient http` y ConfiguracionCliente configuracion; propiedad internal Guid IdSesion de solo lectura; campo private static readonly JsonSerializerOptions Json con defaults Web (camelCase) y NumberHandling.Strict, que rechaza números como cadenas. No hay eventos ni partial.

## Miembros y ejecución

`internal ClientePartidaHttp(ConfiguracionCliente configuracion, Guid idSesion, HttpMessageHandler? handler = null)` guarda configuración y UUID, construye HttpClient con handler inyectado o HttpClientHandler que desactiva redirecciones. BaseAddress=configuracion.Url y Timeout=3 segundos. Dispose del cliente también libera su handler; la regla de no redirección se establece solo en el handler creado por defecto.

Todos los siguientes métodos son internal y retornan Task (o Task del DTO indicado), propagan CancellationToken ct y usan el prefijo relativo `api/v1/partidas/{Partida}/`:

| Firma | Método y sufijo | Cuerpo y respuesta |
|---|---|---|
| `Task<SesionConfirmada> Incorporar(CancellationToken ct)` | PUT participantes/me/sesion | IncorporacionCliente con IdSesion estable y Nombre; DTO de sesión |
| `Task Renovar(CancellationToken ct)` | PUT participantes/me/presencia | Sin cuerpo; exige 204 |
| `Task<PresentacionCliente> Presentacion(CancellationToken ct)` | GET presentacion | Sin cuerpo; DTO completo |
| `Task<ReciboCliente> Elegir(EleccionCliente eleccion, CancellationToken ct)` | POST elecciones | Cuerpo recibido sin cambiar UUID; recibo |
| `Task<ReciboCliente> Recibo(Guid id, CancellationToken ct)` | GET elecciones/{id} | Sin cuerpo; recibo de operación |
| `Task Abandonar(CancellationToken ct)` | PUT participantes/me/abandono | Sin cuerpo; exige 204 |

`private async Task<T> Enviar<T>(HttpMethod metodo, string ruta, object? cuerpo, CancellationToken ct)`: crea y luego libera petición y respuesta con using. Authorization Bearer lleva la credencial; X-Sesion lleva UUID; Cache-Control no-store. Solo crea JsonContent si cuerpo no null. La ruta no empieza con `/`, por lo que preserva prefijo configurado.

```csharp
        using var peticion = new HttpRequestMessage(metodo, $"api/v1/partidas/{configuracion.Partida}/{ruta}");
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuracion.Credencial);
        peticion.Headers.Add("X-Sesion", IdSesion.ToString());
        peticion.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };
        if (cuerpo is not null) peticion.Content = JsonContent.Create(cuerpo, options: Json);
```

Tras SendAsync: código ≥500 lanza servicioNoDisponible transitorio. Si no es éxito, 3xx es protocoloIncompatible; otros errores se deserializan como ErrorCliente y solo aceptan los códigos entradaInvalida, credencialInvalida, sinPermiso, recursoNoEncontrado, sesionOcupada, sesionVencida, partidaPausada, revisionDesactualizada, solicitudNoVigente, operacionIncompatible, eleccionInvalida. Un error null/desconocido es incompatibilidad. Los códigos conocidos se lanzan como no transitorios: el coordinador decide su tratamiento concreto.

En éxito: T=object exige 204 y retorna default (no lee cuerpo). Los DTO exigen 200, o 201 si el método es POST; 202 u otros éxitos no se aceptan. DTO null también es incompatible.

```csharp
            bool exitoEsperado = typeof(T) == typeof(object) ? respuesta.StatusCode == HttpStatusCode.NoContent
                : respuesta.StatusCode == HttpStatusCode.OK || metodo == HttpMethod.Post && respuesta.StatusCode == HttpStatusCode.Created;
            if (!exitoEsperado) throw new FalloCliente("protocoloIncompatible");
            if (typeof(T) == typeof(object)) return default!;
            return await respuesta.Content.ReadFromJsonAsync<T>(Json, ct) ?? throw new FalloCliente("protocoloIncompatible");
```

JSON inválido se traduce a protocoloIncompatible; HttpRequestException a sinComunicacion transitorio; timeout/cancelación interna a tiempoAgotado transitorio únicamente si ct no fue cancelado. La cancelación pedida por el llamador se propaga como OperationCanceledException. La creación de petición/cabeceras precede al try; una configuración directa inválida puede fallar fuera de esta traducción.

`public void Dispose()` libera HttpClient y sus recursos, sin enviar salida/abandono.

## Recorridos y ejemplos

Normal: URL ficticia `https://ejemplo.test/juego/`, partida P y sesión S; Presentacion envía GET `https://ejemplo.test/juego/api/v1/partidas/P/presentacion` con Bearer y X-Sesion, y deserializa un 200 compatible. Reintentar Incorporar conserva S.

Alternativo: POST aplicado cuya respuesta se pierde produce sinComunicacion; el transporte no decide repetirlo. El coordinador consulta Recibo con el mismo UUID. Un 302 no reenvía identidad con el handler normal y se marca incompatible; 503 permite recuperación automática; JSON con revisión `"2"` falla por formato estricto.

## Errores, límites y efectos observables

Cada llamada realiza una solicitud real; PUT/POST pueden cambiar sesión o estado persistido en el servidor, aun si se pierde la respuesta. No registra cuerpos de error ni usa la URL para credenciales. No valida aquí semántica de todos los DTO (roles, revisiones, opciones): EstadoCliente y el coordinador lo hacen. No implementa política general de reintentos ni caché.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### Enviar: clasificación de errores

El servidor no disponible se trata antes del cuerpo JSON. Redirecciones y códigos desconocidos no se asumen como aceptación ni rechazo de negocio conocido.

```csharp
            using var respuesta = await http.SendAsync(peticion, ct);
            if ((int)respuesta.StatusCode >= 500) throw new FalloCliente("servicioNoDisponible", true);
            // Los fallos de disponibilidad se recuperan automáticamente; los demás conservan su código.
            if (!respuesta.IsSuccessStatusCode)
            {
                if ((int)respuesta.StatusCode is >= 300 and < 400) throw new FalloCliente("protocoloIncompatible");
                var error = await respuesta.Content.ReadFromJsonAsync<ErrorCliente>(Json, ct);
                string[] conocidos = ["entradaInvalida", "credencialInvalida", "sinPermiso", "recursoNoEncontrado", "sesionOcupada", "sesionVencida", "partidaPausada", "revisionDesactualizada", "solicitudNoVigente", "operacionIncompatible", "eleccionInvalida"];
                // Un código desconocido no se interpreta como un rechazo conocido ni como aceptación.
                if (error is null || !conocidos.Contains(error.Codigo)) throw new FalloCliente("protocoloIncompatible");
                throw new FalloCliente(error.Codigo);
            }
```

### Enviar: errores de formato y transporte

El filtro distingue timeout interno de cancelación solicitada por el usuario. Los mensajes contienen códigos controlados, no cuerpos ni secretos.

```csharp
        // JSON incompatible requiere intervención; transporte y timeout permiten reintentos automáticos.
        catch (JsonException) { throw new FalloCliente("protocoloIncompatible"); }
        catch (HttpRequestException) { throw new FalloCliente("sinComunicacion", true); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new FalloCliente("tiempoAgotado", true); }
```

### Constructor: timeout y redirecciones

Los campos guardan identidad/configuración y opciones de números estrictos. El constructor configura tres segundos de timeout y deshabilita redirecciones solo para el handler que crea por defecto.

```csharp
    private readonly HttpClient http;
    private readonly ConfiguracionCliente configuracion;
    internal Guid IdSesion { get; }
    // Los DTO siguen camelCase y rechazan números JSON codificados como texto.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict,
    };

    internal ClientePartidaHttp(ConfiguracionCliente configuracion, Guid idSesion, HttpMessageHandler? handler = null)
    {
        this.configuracion = configuracion;
        IdSesion = idSesion;
        // No seguir redirecciones evita enviar las cabeceras de identidad a otro destino.
        http = new(handler ?? new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = configuracion.Url, Timeout = TimeSpan.FromSeconds(3)
```

## Relaciones y verificación

[TransporteClienteTests](../../test/proyecto-integrador-final-tests/TransporteClienteTests.md) verifica prefijo, UUID y autenticación en cabeceras, y clasificación de respuestas sin exponer cuerpos. [ClienteRestIntegracionTests](../../test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.md) usa servidor local. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
