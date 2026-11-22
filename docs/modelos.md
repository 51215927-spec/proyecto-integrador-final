# Modelos y ejecución por archivo

Esta guía desarrolla los tipos, datos y flujos de la [versión común del catálogo](archivos/README.md#versión-documentada): `main` en `135daa7eb196b9bca4e40a1aef76e1131a9b24e8`, 2026-11-22. Incluye las mismas explicaciones contrastadas por archivo que las páginas individuales: miembros públicos y privados, condiciones, fragmentos, recorridos, errores y evidencia. Las secciones se despliegan por archivo para permitir una lectura completa sin abrir el fuente. El [catálogo](archivos/README.md) ofrece navegación por proyecto; la [verificación](archivos/verificacion.md) registra cobertura y límites.

## Conceptos compartidos

Una `class` encapsula estado y comportamiento; `internal` limita acceso al ensamblado, salvo amistad explícita de pruebas con `InternalsVisibleTo`. La visibilidad pública de un miembro no supera la de su tipo. `sealed` impide heredar; `static` sitúa el miembro en el tipo. `partial` reparte un mismo tipo entre fuentes, no crea instancias independientes. Las porciones de RepositorioJuego y de InterfazConsola se enlazan entre sí en sus páginas.

Los `record` posicionales generan constructor, propiedades `init`, deconstrucción e igualdad por valor; no clonan automáticamente arrays, diccionarios ni objetos referidos. `with` copia el record superficialmente y sustituye propiedades indicadas. `readonly` evita reasignar un campo después de construirlo, pero no vuelve inmutable la lista que contiene. `required` exige inicializar una propiedad desde C#, sin reemplazar validaciones de dominio o de JSON.

`?` en una referencia expresa nulabilidad; en un tipo valor como `Guid?` o `int?` permite representar ausencia sin confundirla con cero. `!` suprime la advertencia del compilador; no comprueba valores durante ejecución. `?? throw` convierte ausencia en error de forma explícita. Las guardas se ejecutan en el orden documentado: una excepción interrumpe las instrucciones posteriores, no revierte por sí sola los efectos previos.

`async Task` permite esperar operaciones sin bloquear el hilo; no significa que todas se ejecuten en paralelo. El cliente mantiene una tarea por canal HTTP y serializa escrituras. `CancellationToken` comunica cancelación cooperativa. `using`/`await using` liberan recursos al salir del ámbito, incluso por excepción; no equivalen a abandonar una partida. `lock` serializa acceso en una instancia de repositorio; las transacciones SQLite cubren el estado persistido y su commit.

Los DTO son mensajes HTTP; las entidades EF Core son filas persistidas; los modelos visibles son datos del renderizador. Nombres repetidos como `CartaVisible` en cliente y API pertenecen a namespaces/ensamblados diferentes. El cliente no referencia el proyecto servidor. El coordinador local `Partida.Jugar()` sigue incompleto; el juego REST usa las porciones de RepositorioJuego. No se atribuye al primero el éxito de las pruebas de integración del segundo.

Los fragmentos siguientes son reales y están explicados junto al comportamiento correspondiente. Los recorridos escritos por lectura no se presentan como ejecuciones. La [suite de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) aprobó 179 casos con contratos de procesos habilitados; no acredita red física entre dos Windows ni repite todos los ejemplos manualmente.

## Desarrollo completo

Cada sección despliega un archivo y conserva su enlace a la página individual. Al modificarlo, mantener ambos desarrollos coherentes; esta edición usa las mismas explicaciones y fragmentos para evitar divergencias entre dos inventarios.

<details>
<summary>src/cliente/ClientePartidaHttp.cs</summary>

[Página individual](archivos/src/cliente/ClientePartidaHttp.md)

## src/cliente/ClientePartidaHttp.cs

Fuente: [src/cliente/ClientePartidaHttp.cs](../src/cliente/ClientePartidaHttp.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Es el transporte HTTP de la plaza. Construye rutas, autentica cada solicitud y convierte fallos en códigos controlados sin imprimir cuerpos ni credenciales. No decide turnos ni reintenta por sí mismo; esa política pertenece a [SesionPartidaCliente](archivos/src/cliente/SesionPartidaCliente.md).

### Dependencias

[ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md) fija destino/partida/credencial/nombre; [ContratosPartida](archivos/src/cliente/ContratosPartida.md) declara mensajes JSON. `HttpClient`, cabeceras HTTP, `System.Net.Http.Json` y `System.Text.Json` serializan y envían. [Protocolo](protocolo-de-partida.md) describe la API del servidor.

### Estructura y datos

`internal sealed class FalloCliente(string codigo, bool transitorio = false) : Exception(codigo)` contiene propiedades internal de solo lectura `Codigo` y `Transitorio`, inicializadas por constructor primario. Message de Exception recibe el código, no el cuerpo remoto.

`internal sealed class ClientePartidaHttp : IDisposable`: campos privados readonly `HttpClient http` y ConfiguracionCliente configuracion; propiedad internal Guid IdSesion de solo lectura; campo private static readonly JsonSerializerOptions Json con defaults Web (camelCase) y NumberHandling.Strict, que rechaza números como cadenas. No hay eventos ni partial.

### Miembros y ejecución

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

### Recorridos y ejemplos

Normal: URL ficticia `https://ejemplo.test/juego/`, partida P y sesión S; Presentacion envía GET `https://ejemplo.test/juego/api/v1/partidas/P/presentacion` con Bearer y X-Sesion, y deserializa un 200 compatible. Reintentar Incorporar conserva S.

Alternativo: POST aplicado cuya respuesta se pierde produce sinComunicacion; el transporte no decide repetirlo. El coordinador consulta Recibo con el mismo UUID. Un 302 no reenvía identidad con el handler normal y se marca incompatible; 503 permite recuperación automática; JSON con revisión `"2"` falla por formato estricto.

### Errores, límites y efectos observables

Cada llamada realiza una solicitud real; PUT/POST pueden cambiar sesión o estado persistido en el servidor, aun si se pierde la respuesta. No registra cuerpos de error ni usa la URL para credenciales. No valida aquí semántica de todos los DTO (roles, revisiones, opciones): EstadoCliente y el coordinador lo hacen. No implementa política general de reintentos ni caché.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### Enviar: clasificación de errores

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

#### Enviar: errores de formato y transporte

El filtro distingue timeout interno de cancelación solicitada por el usuario. Los mensajes contienen códigos controlados, no cuerpos ni secretos.

```csharp
        // JSON incompatible requiere intervención; transporte y timeout permiten reintentos automáticos.
        catch (JsonException) { throw new FalloCliente("protocoloIncompatible"); }
        catch (HttpRequestException) { throw new FalloCliente("sinComunicacion", true); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new FalloCliente("tiempoAgotado", true); }
```

#### Constructor: timeout y redirecciones

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

### Relaciones y verificación

[TransporteClienteTests](archivos/test/proyecto-integrador-final-tests/TransporteClienteTests.md) verifica prefijo, UUID y autenticación en cabeceras, y clasificación de respuestas sin exponer cuerpos. [ClienteRestIntegracionTests](archivos/test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.md) usa servidor local. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/ConfiguracionCliente.cs</summary>

[Página individual](archivos/src/cliente/ConfiguracionCliente.md)

## src/cliente/ConfiguracionCliente.cs

Fuente: [src/cliente/ConfiguracionCliente.cs](../src/cliente/ConfiguracionCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Recoge y valida los cuatro datos de conexión antes de crear el transporte REST. La corrección vuelve a pedir solo el campo inválido; no envía solicitudes HTTP ni crea partidas.

### Dependencias

`TextReader`/`TextWriter` permiten usar consola o lectores en memoria. `Console` permite captura sin eco para el secreto; `StringBuilder` acumula caracteres; `Uri` y `Guid` verifican formatos. [Program](archivos/src/cliente/Program.md) llama a la lectura y [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md) consume el resultado. Véase [instalación y uso](instalacion-y-uso.md).

### Estructura y datos

`internal sealed record ConfiguracionCliente(Uri Url, Guid Partida, string Credencial, string Nombre)`. El constructor posicional y las propiedades init generadas almacenan exactamente esos valores; construir directamente el record no ejecuta las validaciones de lectura. Record aporta igualdad por valor y copia `with`; no tiene interfaces explícitas ni partial. Sus métodos de lectura son internal static; `ToString` es public override. No hay eventos ni persistencia local.

### Miembros y ejecución

- `Leer(TextReader entrada, TextWriter salida)` retorna ConfiguracionCliente bloqueando en `LeerAsync(..., default).GetAwaiter().GetResult()`. Reutiliza las mismas validaciones; propaga EOF y fallos de E/S.
- `LeerAsync(TextReader entrada, TextWriter salida, CancellationToken ct)` retorna Task del record. Pide URL, UUID no vacío, credencial y nombre, en ese orden. Cada valor se recorta por fuera. El UUID usa `Guid.TryParse` y no puede ser Guid.Empty. Credencial no vacía ni con controles; permite espacios internos. Nombre no blanco, conserva mayúsculas y espacios internos. La URL final elimina barras finales y añade exactamente una para concatenar rutas relativas.
- Función local `async Task<string> Campo(string pregunta, Func<string,bool> valida, string error, bool secreto = false)`: repite pregunta hasta validar. Si el lector es exactamente Console.In y stdin no está redirigido, espera teclas cada 25 ms con cancelación. Enter termina, Ctrl+D/Ctrl+Z lanzan EndOfStreamException, Backspace elimina si hay caracteres y solo borra visualmente en campos visibles; caracteres no de control se agregan y solo se imprimen si no es secreto. Otros controles se ignoran. Si hay tubería u otro lector, usa ReadLineAsync(ct); null lanza EOF. Tras Trim, devuelve si el predicado acepta o imprime el error y repite. El callback del UUID convierte y compara; los otros callbacks aplican las restricciones anteriores.
- `EsUrl(string valor)` devuelve bool: absoluta, HTTP o HTTPS, host presente, sin usuario/contraseña, query ni fragmento. Permite prefijo de ruta (a diferencia del lector común del servidor). No prueba conectividad.
- `public override string ToString()` retorna únicamente `Partida {Partida}`; no incluye credencial, URL ni nombre.

Fragmento de la secuencia final de `LeerAsync`; los bucles de Campo ya se explicaron:

```csharp
        string url = await Campo("URL base HTTP/HTTPS del servidor", EsUrl,
            "Ingresá una URL absoluta HTTP/HTTPS sin credenciales, consulta ni fragmento.");
        string id = await Campo("UUID de partida", v => Guid.TryParse(v, out var g) && g != Guid.Empty,
            "Ingresá un UUID distinto de vacío.");
        // El nombre conserva mayúsculas y espacios internos: el servidor exige coincidencia al retomar.
        string credencial = await Campo("Credencial de plaza", v => !string.IsNullOrWhiteSpace(v) && !v.Any(char.IsControl),
            "Ingresá una credencial no vacía y sin controles.", true);
        string nombre = await Campo("Nombre registrado (debe coincidir exactamente al retomar)",
            v => !string.IsNullOrWhiteSpace(v), "Ingresá un nombre no vacío.");
        return new(new Uri(url.TrimEnd('/') + "/"), Guid.Parse(id), credencial, nombre);
```

El sufijo `/` conserva un prefijo como `/juego/` al combinarlo con rutas del cliente. La validación de URL es:

```csharp
    internal static bool EsUrl(string valor) => Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https" && uri.Host.Length > 0 && uri.UserInfo.Length == 0 &&
        uri.Query.Length == 0 && uri.Fragment.Length == 0;
```

### Recorridos y ejemplos

Normal: `https://ejemplo.test/juego`, UUID `11111111-1111-1111-1111-111111111111`, credencial ficticia `plaza-demo`, nombre ` Ana María ` producen URL `https://ejemplo.test/juego/`, UUID parseado, secreto intacto y nombre `Ana María`. No hay HTTP todavía.

Alternativo: `ftp://ejemplo.test` repite solo URL. Después de URL válida, UUID vacío repite solo UUID. EOF mientras se pide el nombre lanza sin devolver configuración parcial; Ctrl+C cancela mediante el token del arranque. Una credencial escrita en terminal no aparece en eco; un lector redirigido es consumido línea por línea.

### Errores, límites y efectos observables

Imprime preguntas y errores controlados; no diagnostica contenidos secretos. El constructor directo y las copias `with` pueden contener valores que la lectura rechazaría. No comprueba longitud de nombre ni privilegios en el servidor. No tiene transacciones ni sincronización; su asincronía sirve a lectura y cancelación.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### Campo: teclado oculto y cancelable

Este fragmento de la función local muestra los dos bucles, los 25 ms cancelables, EOF físico, Backspace y eco condicionado por secreto. Tras Trim el predicado acepta o repite el campo.

```csharp
        // Cada campo se corrige por separado para conservar los valores ya aceptados.
        async Task<string> Campo(string pregunta, Func<string, bool> valida, string error, bool secreto = false)
        {
            while (true)
            {
                salida.WriteLine(pregunta);
                string valor;
                if (ReferenceEquals(entrada, Console.In) && !Console.IsInputRedirected)
                {
                    // La lectura por teclas permite ocultar el secreto y cancelar sin bloquear ReadLine.
                    var texto = new StringBuilder();
                    while (true)
                    {
                        while (!Console.KeyAvailable) await Task.Delay(25, ct);
                        var tecla = Console.ReadKey(true);
                        if (tecla.Key == ConsoleKey.Enter) { salida.WriteLine(); break; }
                        if (tecla.KeyChar is '\u0004' or '\u001a') throw new EndOfStreamException();
                        // El borrado modifica el valor; solo se refleja en pantalla para campos visibles.
                        if (tecla.Key == ConsoleKey.Backspace)
                        {
                            if (texto.Length > 0) { texto.Length--; if (!secreto) salida.Write("\b \b"); }
                        }
                        else if (!char.IsControl(tecla.KeyChar)) { texto.Append(tecla.KeyChar); if (!secreto) salida.Write(tecla.KeyChar); }
                    }
                    valor = texto.ToString().Trim();
                }
                // En una tubería se consume una línea por campo y EOF cancela la configuración.
                else valor = (await entrada.ReadLineAsync(ct) ?? throw new EndOfStreamException()).Trim();
                if (valida(valor)) return valor;
                salida.WriteLine(error);
            }
```

### Relaciones y verificación

[ConfiguracionClienteTests](archivos/test/proyecto-integrador-final-tests/ConfiguracionClienteTests.md) parametriza formatos URL y verifica recorte, conservación de nombre y repetición por campo. [ArranqueRestTests](archivos/test/proyecto-integrador-final-tests/ArranqueRestTests.md) cubre el ejecutable. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/ContratosPartida.cs</summary>

[Página individual](archivos/src/cliente/ContratosPartida.md)

## src/cliente/ContratosPartida.cs

Fuente: [src/cliente/ContratosPartida.cs](../src/cliente/ContratosPartida.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Declara los mensajes del cliente REST sin depender del ensamblado del servidor. El transporte los serializa/deserializa; EstadoCliente y SesionPartidaCliente verifican coherencia antes de habilitar interacción. No contienen reglas de combate ni operaciones de E/S.

### Dependencias

`System.Text.Json.Serialization.JsonRequiredAttribute` marca propiedades obligatorias al deserializar. Las cartas usan [CartaVisible](archivos/src/cliente/VistasConsola.md). [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md) usa defaults Web, y [EstadoCliente](archivos/src/cliente/EstadoCliente.md) verifica los valores. Véase [protocolo](protocolo-de-partida.md).

### Estructura, estado y miembros declarativos

Todos son `internal sealed record`, sin base explícita, interfaces ni partial. El constructor posicional crea propiedades init; el compilador genera igualdad, deconstrucción, copia `with` y representación textual. No se declaran métodos con lógica, campos mutables ni eventos. Los records no validan por sí mismos un argumento de constructor; arrays y diccionarios referenciados siguen siendo mutables.

| Tipo y firma posicional | Significado de cada dato y restricciones aplicadas fuera del record |
|---|---|
| `IncorporacionCliente(Guid IdSesion, string Nombre)` | UUID estable del proceso y nombre registrado exacto; cuerpo de PUT sesión. Sin JsonRequired porque se usa para envío. |
| `SesionConfirmada(string Rol, Guid IdSesion, DateTimeOffset VenceUtc)` | Rol J1/J2, identidad confirmada, vencimiento UTC. Las tres propiedades son JsonRequired. El coordinador contrasta UUID y rol; no usa VenceUtc para programar renovación. |
| `SolicitudCliente(Guid IdSolicitud, string Tipo, int[] IdsPersonajePermitidos, string[] AtributosPermitidos)` | UUID de solicitud, carta/cartaYAtributo, IDs ofrecidos, atributos ofrecidos; todos Required. EstadoCliente exige no vacío y coherencia con mano. |
| `ResultadoCliente(int NumeroRonda, int NumeroCombate, Dictionary<string,CartaVisible> Cartas, string Atributo, Dictionary<string,int> Puntajes, string GanadorCombate, string? GanadorRonda)` | Coordenadas del historial, ambas cartas reveladas por J1/J2, atributo comparado, puntajes por rol, ganador de combate y posible ganador de ronda. Todas Required: GanadorRonda debe existir en JSON aunque su valor pueda ser null. |
| `PresentacionCliente(Guid IdPartida, int Revision, string Fase, bool Pausada, string RolPropio, Dictionary<string,string?> Nombres, int? NumeroRonda, int? NumeroCombate, Dictionary<string,int> Marcador, CartaVisible[] Mano, SolicitudCliente? Solicitud, ResultadoCliente[] Resultados, string? MotivoFin, string? Ganador)` | Estado completo privado de plaza. Las primeras doce propiedades son Required, incluidas las que aceptan null; MotivoFin y Ganador pueden omitirse antes de cierre. Nombres usa roles y admite nombres null de plazas aún no incorporadas. Mano no contiene cartas rivales ocultas. |
| `EleccionCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionEsperada, int IdPersonaje, string? Atributo)` | UUID idempotente del envío, solicitud y revisión de origen, carta elegida, atributo null al responder. El coordinador conserva el mismo cuerpo al recuperar. |
| `ReciboCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionConfirmada)` | Aceptación persistida de elección, no resultado del combate. Todo Required; el coordinador exige coincidencias e incremento de revisión. |
| `ErrorCliente(string Codigo, string Mensaje, string IdSeguimiento)` | Código remoto, texto e identificador diagnóstico. Transporte solo publica un código conocido; no expone Mensaje ni cuerpo en su excepción. |

### Ejecución y fragmentos

No hay ejecución propia salvo constructores/accesores generados: asignan los argumentos sin normalización. La deserialización sí aplica atributos Required; son atributos de la propiedad (`property:`), no del parámetro primario.

```csharp
internal sealed record SolicitudCliente(
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] string Tipo,
    [property: JsonRequired] int[] IdsPersonajePermitidos,
    [property: JsonRequired] string[] AtributosPermitidos);
```

Los arrays obligatorios evitan aceptar omisiones como null de forma silenciosa; Required exige presencia, no rango semántico ni valor no null. EstadoCliente añade esas comprobaciones. La elección y el recibo real son:

```csharp
internal sealed record EleccionCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionEsperada, int IdPersonaje, string? Atributo);
// El recibo acredita aceptación de la elección, sin anticipar el resultado del combate.
internal sealed record ReciboCliente(
    [property: JsonRequired] Guid IdOperacion,
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] int RevisionConfirmada);
```

Conservar IdOperacion y cuerpo permite consultar un recibo después de perder una respuesta sin consumir otra solicitud.

### Recorridos y ejemplos

Normal: una solicitud tipo carta ofrece ID 7, atributos `[]`; el cliente construye elección con nueva operación O, UUID S, revisión 4, carta 7, atributo null. Un recibo O/S/revisión 5 confirma recepción; no afirma que ese jugador ganó.

Alternativo: JSON de SesionConfirmada sin rol falla por propiedad obligatoria. GanadorRonda null explícito es válido, pero omitido incumple Required. Tipo de solicitud desconocido puede construirse como record, aunque EstadoCliente lo rechaza antes de aplicarlo.

### Errores, límites y efectos observables

Nulabilidad C# es ayuda estática; no sustituye validación de JSON. Igualdad record de arrays/diccionarios no compara profundamente su contenido; el coordinador usa JSON para huellas de pantalla/selección. No hay liberación de recursos, cancelación ni persistencia en estos tipos.

### Relaciones y verificación

[TransporteClienteTests](archivos/test/proyecto-integrador-final-tests/TransporteClienteTests.md), [EstadoClienteTests](archivos/test/proyecto-integrador-final-tests/EstadoClienteTests.md) y [SesionClienteTests](archivos/test/proyecto-integrador-final-tests/SesionClienteTests.md) verifican partes diferentes del contrato. No confundir un DTO declarable con una respuesta semánticamente aceptable. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/DemostracionConsola.cs</summary>

[Página individual](archivos/src/cliente/DemostracionConsola.md)

## src/cliente/DemostracionConsola.cs

Fuente: [src/cliente/DemostracionConsola.cs](../src/cliente/DemostracionConsola.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Recorre pantallas de una partida simulada local con Lucía y Mateo, o un cierre simulado. Es un auxiliar interno utilizado por pruebas; el Program actual inicia REST y no ofrece este recorrido como modo del ejecutable. No prueba conectividad ni el estado persistido del servidor.

### Dependencias

[InterfazConsola](archivos/src/cliente/InterfazConsola.md) dibuja y recoge elecciones; [VistasConsola](archivos/src/cliente/VistasConsola.md) lleva cartas, mesa y resultados. TextReader/TextWriter coordinan las pausas en modo interactivo. Véase [diseño de interfaz](diseno-de-interfaz.md).

### Estructura y datos

`internal sealed class DemostracionConsola(InterfazConsola interfaz, TextReader entrada, TextWriter salida, bool interactiva)`. El constructor primario captura esos colaboradores; no valida null ni dispone sus flujos. No hay interfaces, eventos, partial ni campos explícitos. Recorrer crea estado local por llamada: Lucía y Mateo con 14 rondas cada uno, ronda 29, combate 1, victorias de combate cero; tres cartas propias y tres rivales. Los puntajes son valores de presentación, sin pasar por Personaje.Validar.

### Miembros y ejecución

`public void Recorrer(string? cierre)`: crea mano Lucía (Lysander ID1, Reched ID2, Vigía ID3), mano rival (Centinela ID4, Oráculo ID5, Lancero ID6); llama a preparación, espera inicial e inicio de ronda, pausando tras cada pantalla. Si cierre no null, delega MostrarCierreSimulado y retorna sin jugar.

Con cierre null, repite combates: abre Lucía en combate impar. Si responde, muestra espera rival y pausa. Pide carta propia; al abrir pide atributo, al responder usa Inteligencia fija. La carta rival se toma por índice combate−1. Muestra envío en curso; en el primer combate además confirmación pendiente; después recepción confirmada, cada una seguida de Continuar. Compara los dos valores del atributo: gana Lucía si es mayor o, empatando, si ella no abrió (pierde el elector). Retira su carta y crea copia de mesa incrementando contador del ganador. Continúa mientras ninguno llegue a dos victorias; muestra duelo con próximo elector alternado cuando continúa. Pausa tras resultado y, si sigue, incrementa combate. Con dos victorias el ganador recibe 15 rondas en la mesa; muestra cierre de ronda sin próximo mano y cierre de partida. Al final interactivo hace un ReadLine adicional cuyo null no lanza explícitamente.

Fragmento de comparación y transición de mesa de Recorrer:

```csharp
            int valorPropio = propia.Atributos[atributo];
            int valorRival = contraria.Atributos[atributo];
            ganaLucia = valorPropio > valorRival || (valorPropio == valorRival && !abre);
            mano.Remove(propia);
            mesa = ganaLucia
                ? mesa with { CombatesJ1 = mesa.CombatesJ1 + 1 }
                : mesa with { CombatesJ2 = mesa.CombatesJ2 + 1 };

            bool rondaContinua = mesa.CombatesJ1 < 2 && mesa.CombatesJ2 < 2;
```

`private void MostrarCierreSimulado(MesaVisible mesa, string cierre)`: si cierre="pausa", primero muestra pausa confirmada por pérdida de sesión, pausa, luego pérdida no confirmada y pausa. Después también ejecuta el switch de cierre: abandono/cancelacion generan final confirmado sin victoria; cualquier otro valor (incluida pausa) muestra salida local guardada, no confirmada. No cambia la mesa ni llama al servidor.

```csharp
        (string causa, string efecto, bool confirmada) = cierre switch
        {
            "abandono" => ("Mateo abandonó la partida.", "La partida terminó sin sumar una victoria.", true),
            "cancelacion" => ("El operador canceló la partida.", "La partida terminó sin sumar una victoria.", true),
            _ => ("Se cerró el cliente.", "La partida queda guardada.", false)
        };
        interfaz.MostrarInterrupcion(mesa.Local, causa, efecto, confirmada);
```

`private void Continuar()`: no interactiva retorna de inmediato. Interactiva imprime `Pulsá Enter para avanzar.`, lee una línea y lanza EndOfStreamException si null. Cualquier línea no null avanza, sin comprobar que sea vacía.

`private static CartaVisible Carta(int id, string nombre, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)`: retorna CartaVisible con diccionario de las seis claves españolas; no normaliza, valida ni hace copias posteriores. Energía lleva tilde en la clave aunque parámetro se llame energia.

### Recorridos y ejemplos

Normal: elegir Lysander y Fuerza en combate 1 compara 750 con Centinela 750; gana Mateo porque Lucía eligió atributo. En combate 2 Lucía puede responder con Vigía: Inteligencia 500 frente a Oráculo 750 vuelve a dar victoria a Mateo; llega a dos combates y 15 rondas, cierra sin combate 3. Este ejemplo es deducción de datos y decisiones del auxiliar.

Alternativo: cierre="pausa" muestra primero ambas suspensiones y luego salida local; cierre="abandono" no juega ni suma victoria. EOF en una pausa interactiva lanza y no continúa el recorrido. En modo no interactivo se omiten pausas pero PedirCarta/PedirAtributo aún consumen las líneas del lector de la interfaz.

### Errores, límites y efectos observables

La simulación termina en dos o tres combates porque cada combate otorga una victoria. No permite partida completa desde ronda 1, reconexión real, persistencia ni cambios de nombre. Depende de opciones válidas de la interfaz; mano vacía o atributos inexistentes no tienen recuperación propia. No dispone recursos ajenos ni captura excepciones de E/S.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### Recorrer: cierre temprano y elecciones

Antes del bucle se ofrecen pantallas iniciales; cierre no null interrumpe el juego. La carta rival se indexa por combate y al responder el atributo está fijado a Inteligencia.

```csharp
        if (cierre is not null)
        {
            MostrarCierreSimulado(mesa, cierre);
            return;
        }

        bool ganaLucia;
        do
        {
            bool abre = mesa.Combate % 2 == 1;
            if (!abre)
            {
                interfaz.MostrarEsperaRival(mesa, rival, mano);
                Continuar();
            }

            CartaVisible propia = interfaz.PedirCarta(mesa, mano, abre);
            string atributo = abre ? interfaz.PedirAtributo(mesa: mesa) : "Inteligencia";
            CartaVisible contraria = manoRival[mesa.Combate - 1];

            interfaz.MostrarEnvio(mesa, EstadoEnvio.Enviando);
            Continuar();
            if (mesa.Combate == 1)
            {
                interfaz.MostrarEnvio(mesa, EstadoEnvio.Pendiente);
                Continuar();
            }
            interfaz.MostrarEnvio(mesa, EstadoEnvio.Recibido);
            Continuar();
```

#### Recorrer: fin de ronda y marcador

La salida del bucle ocurre con dos victorias; solo entonces cambia el marcador de rondas a 15. El ReadLine final interactivo no lanza explícitamente por null.

```csharp
            bool rondaContinua = mesa.CombatesJ1 < 2 && mesa.CombatesJ2 < 2;
            interfaz.MostrarCombate(mesa, new(atributo,
                new(local, propia.Nombre, valorPropio), new(rival, contraria.Nombre, valorRival),
                ganaLucia ? local : rival, mesa.CombatesJ1, mesa.CombatesJ2,
                rondaContinua ? (abre ? rival : local) : null, ganaLucia));
            Continuar();
            if (!rondaContinua)
                break;

            mesa = mesa with { Combate = mesa.Combate + 1 };
        } while (true);

        mesa = ganaLucia ? mesa with { J1 = mesa.J1 with { RondasGanadas = 15 } }
            : mesa with { J2 = mesa.J2 with { RondasGanadas = 15 } };
        string ganador = ganaLucia ? local : rival;
        interfaz.MostrarCierreRonda(mesa, ganador, proximoMano: null);
        Continuar();
        interfaz.MostrarCierrePartida(local, ganador, mesa.J1, mesa.J2);
        if (interactiva)
            entrada.ReadLine();
    }
```

#### MostrarCierreSimulado: pausa y Continuar

La variante pausa muestra dos estados de suspensión antes del switch de interrupción ya documentado.

```csharp
    private void MostrarCierreSimulado(MesaVisible mesa, string cierre)
    {
        if (cierre == "pausa")
        {
            interfaz.MostrarSuspension(mesa, "Mateo perdió la sesión. Esperando que se reincorpore.", confirmada: true);
            Continuar();
            interfaz.MostrarSuspension(mesa, "Podés reintentar o salir.", confirmada: false);
            Continuar();
        }
```

#### Continuar y Carta

La pausa no interactiva retorna sin leer. La fábrica crea mapa con las seis claves y no aplica validación de dominio.

```csharp
    private void Continuar()
    {
        if (!interactiva)
            return;

        salida.WriteLine("Pulsá Enter para avanzar.");
        if (entrada.ReadLine() is null)
            throw new EndOfStreamException();
    }

    private static CartaVisible Carta(int id, string nombre, int fuerza, int velocidad,
        int resistencia, int inteligencia, int energia, int habilidad) =>
        new(id, nombre, new Dictionary<string, int>
        {
            ["Fuerza"] = fuerza, ["Velocidad"] = velocidad, ["Resistencia"] = resistencia,
            ["Inteligencia"] = inteligencia, ["Energía"] = energia, ["Habilidad"] = habilidad
        });
```

### Relaciones y verificación

[ClienteConsolaContratoTests](archivos/test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.md) conserva pruebas directas de victoria y cierres simulados. No son pruebas del arranque REST ni de transporte real. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/EntradaCliente.cs</summary>

[Página individual](archivos/src/cliente/EntradaCliente.md)

## src/cliente/EntradaCliente.cs

Fuente: [src/cliente/EntradaCliente.cs](../src/cliente/EntradaCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Convierte teclado físico o líneas redirigidas en eventos de navegación para [SesionPartidaCliente](archivos/src/cliente/SesionPartidaCliente.md). Distingue Enter de EOF para que el cierre de entrada nunca confirme una elección.

### Dependencias

`Console`, `TextReader`, `Task.Delay` y CancellationToken. No conoce HTTP ni estado de partida. [Program](archivos/src/cliente/Program.md) proporciona el lector; [InterfazConsola](archivos/src/cliente/InterfazConsola.md) solo presenta resultados.

### Estructura y datos

`internal sealed class EntradaCliente(TextReader? entrada = null) : IDisposable`. Constructor primario sin guardas: `private readonly bool interactiva = !Console.IsInputRedirected` y `private readonly TextReader lector = entrada ?? Console.In`. La selección interactiva depende de stdin global, no de que se haya inyectado otro lector. No hay propiedades, eventos, partial ni estado de selección.

### Miembros y ejecución

`internal async Task<ConsoleKey?> Leer(CancellationToken ct)`: en modo interactivo espera cada 25 ms mientras no haya tecla; cancelar interrumpe la espera. `ReadKey(true)` no hace eco; Ctrl+D/Ctrl+Z retornan null, cualquier otra tecla retorna su ConsoleKey. En modo redirigido ReadLineAsync(ct) consume una sola línea y aplica Trim/ToLowerInvariant: null→null; vacío→Enter; `esc`→Escape; w/a/s/d/h/j/k/l→sus teclas; cualquier otra cadena→NoName. No interpreta nombres de flechas escritos como texto.

Fragmento real de `Leer`, tras la rama interactiva:

```csharp
        string? linea = await lector.ReadLineAsync(ct);
        return linea?.Trim().ToLowerInvariant() switch
        {
            null => null, "" => ConsoleKey.Enter, "esc" => ConsoleKey.Escape,
            "w" => ConsoleKey.W, "a" => ConsoleKey.A, "s" => ConsoleKey.S, "d" => ConsoleKey.D,
            "h" => ConsoleKey.H, "j" => ConsoleKey.J, "k" => ConsoleKey.K, "l" => ConsoleKey.L,
            _ => ConsoleKey.NoName
        };
```

`internal void DescartarSobrante()`: solo en modo interactivo consume teclas mientras KeyAvailable. En redirección conserva líneas futuras, que pueden ser acciones intencionales; no modifica el lector.

```csharp
    internal void DescartarSobrante()
    {
        if (interactiva) while (Console.KeyAvailable) Console.ReadKey(true);
    }

    // El lector pertenece al arranque y se reutiliza si el usuario corrige la configuración.
    public void Dispose() { }
```

`public void Dispose()` no hace nada: la propiedad del lector corresponde a Program y puede reutilizarse al corregir configuración. No cierra Console.In ni un lector inyectado.

### Recorridos y ejemplos

Normal en tubería: líneas `s`, vacía y `esc` devuelven S, Enter y Escape en llamadas consecutivas. En terminal una flecha retorna directamente el código de flecha.

Alternativo: una línea `otra` devuelve NoName y no confirma; EOF devuelve null. Cancelación durante espera lanza OperationCanceledException hacia el coordinador. DescartarSobrante en tubería no elimina el siguiente Enter.

### Errores, límites y efectos observables

Lee entrada; no imprime ni envía HTTP. ReadLineAsync puede lanzar IOException; el arranque la maneja. La espera de teclado es cancelable; no hay cancelación específica entre detectar tecla y leerla. No posee recursos que deba liberar.

#### Leer: espera física cancelable

La rama física espera con token y retorna null para los controles EOF, mientras la rama de líneas ya mostrada mantiene la distinción entre null y vacío.

```csharp
    internal async Task<ConsoleKey?> Leer(CancellationToken ct)
    {
        if (interactiva)
        {
            // Esperar de forma cancelable evita que una tecla pendiente impida cerrar el proceso.
            while (!Console.KeyAvailable) await Task.Delay(25, ct);
            var tecla = Console.ReadKey(true);
            if (tecla.KeyChar is '\u0004' or '\u001a') return null;
            return tecla.Key;
        }
        // Una línea vacía confirma; null significa EOF y nunca debe convertirse en Enter.
```

### Relaciones y verificación

[SesionClienteTests](archivos/test/proyecto-integrador-final-tests/SesionClienteTests.md) y [ClienteProcesoRestTests](archivos/test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md) ejercitan navegación y salida; no equivalen a prueba de todos los teclados o sistemas operativos. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/EstadoCliente.cs</summary>

[Página individual](archivos/src/cliente/EstadoCliente.md)

## src/cliente/EstadoCliente.cs

Fuente: [src/cliente/EstadoCliente.cs](../src/cliente/EstadoCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Guarda la última presentación íntegra validada y una cola de resultados nuevos para leer. Marca el estado como no actual ante pérdida de red, sin inventar una pausa ni borrar el último marcador.

### Dependencias

Los DTO vienen de [ContratosPartida](archivos/src/cliente/ContratosPartida.md) y las cartas de [VistasConsola](archivos/src/cliente/VistasConsola.md). LINQ ordena y contrasta opciones; [FalloCliente](archivos/src/cliente/ClientePartidaHttp.md) comunica incompatibilidad al [coordinador](archivos/src/cliente/SesionPartidaCliente.md). Véase [protocolo](protocolo-de-partida.md).

### Estructura y datos

`internal sealed class EstadoCliente(Guid partida)` captura la identidad fija del constructor primario. `private readonly HashSet<(int Ronda,int Combate)> conocidos` inicia vacío, conserva claves incluso tras reconectar. `internal PresentacionCliente? Vista {get; private set;}` inicia null; `internal bool Actual {get; private set;}` inicia false; `internal Queue<ResultadoCliente> Resultados {get;}` crea cola vacía mutable, consumida por el coordinador. No hay herencia explícita, eventos, interfaces ni partial.

### Miembros y ejecución

`internal void PerderComunicacion()` solo asigna Actual=false; no toca Vista, cola ni conocidos.

`internal void Aplicar(PresentacionCliente vista)` valida primero. NullReferenceException, KeyNotFoundException y ArgumentException durante Validar se convierten en FalloCliente("protocoloIncompatible"); ese mismo FalloCliente ya emitido por Exigir se propaga. Solo tras validar recorre resultados por ronda y combate ascendentes. Añade cada clave al conjunto; en la primera vista no encola historia previa. En vistas posteriores encola solo claves inéditas. Finalmente reemplaza Vista y establece Actual=true, incluso si la revisión no cambió. La monotonía de revisiones se controla en el coordinador, no aquí.

```csharp
        bool inicial = Vista is null;
        foreach (var resultado in vista.Resultados.OrderBy(r => r.NumeroRonda).ThenBy(r => r.NumeroCombate))
            if (conocidos.Add((resultado.NumeroRonda, resultado.NumeroCombate)) && !inicial)
                Resultados.Enqueue(resultado);
        Vista = vista;
        Actual = true;
```

`private void Validar(PresentacionCliente vista)` usa funciones locales estáticas `bool Rol(string? v)` (solo J1 o J2) y `void Exigir(bool condicion)` (lanza incompatibilidad si false). Verifica en orden:

1. UUID coincide con partida, revisión ≥0, rol propio conocido; fase en esperandoInicio/esperandoElector/esperandoRival/finalizada.
2. Nombres y Marcador contienen J1 y J2; Mano y Resultados no son null.
3. Toda carta propia tiene ID positivo, nombre y atributos no null, y las seis claves Fuerza, Velocidad, Resistencia, Inteligencia, Energía, Habilidad. IDs de mano sin duplicados.
4. Si finalizada: solicitud null y motivo victoria/abandono/cancelacion; victoria exige ganador J1/J2. No exige ganador nulo para los otros motivos.
5. Si existe solicitud: no pausada, fase de elección; UUID no vacío, tipo carta o cartaYAtributo; al menos un ID permitido y todos presentes en mano. Tipo carta requiere cero atributos; cartaYAtributo requiere al menos uno y cada atributo existe en todas las cartas permitidas. No exige explícitamente unicidad de opciones ni que los atributos correspondan solo a las seis claves conocidas.
6. Por cada resultado: ronda y combate positivos, ganador de combate válido, ganador de ronda null o válido, mapas Cartas y Puntajes con J1/J2, atributo no null. No recalcula ganador ni valida aquí cada detalle de las cartas históricas.

Fragmento de la guarda de solicitudes, que bloquea opciones imposibles:

```csharp
        if (vista.Solicitud is { } s)
        {
            Exigir(!vista.Pausada && vista.Fase is "esperandoElector" or "esperandoRival");
            Exigir(s.IdSolicitud != Guid.Empty && s.Tipo is "carta" or "cartaYAtributo");
            // Una mano vacía es válida en espera, pero ninguna solicitud puede ofrecer cartas inexistentes.
            Exigir(s.IdsPersonajePermitidos.Length > 0 && s.IdsPersonajePermitidos.All(id => vista.Mano.Any(c => c.IdPersonaje == id)));
            Exigir(s.Tipo == "carta" ? s.AtributosPermitidos.Length == 0 : s.AtributosPermitidos.Length > 0 &&
                s.AtributosPermitidos.All(a => vista.Mano.Where(c => s.IdsPersonajePermitidos.Contains(c.IdPersonaje)).All(c => c.Atributos.ContainsKey(a))));
        }

```

### Recorridos y ejemplos

Normal: primera vista con resultado (1,1) lo marca conocido sin mostrarlo otra vez. Segunda vista con (1,1),(1,3),(1,2) encola solo (1,2),(1,3), en ese orden. PerderComunicacion mantiene la cola; nueva vista de igual revisión puede actualizar la pausa.

Error: solicitud ofrece ID 99 ausente de mano; Validar lanza y conserva Vista, Actual y cola anteriores. Mano vacía es válida si no hay solicitud; con opciones obligatorias es incompatible. No crea estados parciales.

### Errores, límites y efectos observables

No hace HTTP ni persiste. No limita tamaño de historia o cola, ni impide revisiones menores por sí mismo. No hay bloqueos: el bucle del coordinador es su dueño único. Una clave repetida con contenido distinto se considera ya conocida y no vuelve a encolarse.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### Validar: identidad, mano y cierre

Las funciones locales Rol y Exigir sustentan comprobaciones previas a mutar estado. Required de JSON no reemplaza estas condiciones de coherencia.

```csharp
    {
        // Las identidades son J1/J2: dos jugadores pueden tener el mismo nombre visible.
        static bool Rol(string? v) => v is "J1" or "J2";
        static void Exigir(bool condicion) { if (!condicion) throw new FalloCliente("protocoloIncompatible"); }
        Exigir(vista.IdPartida == partida && vista.Revision >= 0 && Rol(vista.RolPropio));
        Exigir(vista.Fase is "esperandoInicio" or "esperandoElector" or "esperandoRival" or "finalizada");
        // Los datos usados por el renderizado deben estar completos antes de habilitar la interacción.
        Exigir(vista.Nombres.ContainsKey("J1") && vista.Nombres.ContainsKey("J2") &&
            vista.Marcador.ContainsKey("J1") && vista.Marcador.ContainsKey("J2"));
        Exigir(vista.Mano is not null && vista.Resultados is not null);
        string[] atributos = ["Fuerza", "Velocidad", "Resistencia", "Inteligencia", "Energía", "Habilidad"];
        Exigir(vista.Mano!.All(c => c.IdPersonaje > 0 && c.Nombre is not null && c.Atributos is not null &&
            atributos.All(c.Atributos.ContainsKey)));
        Exigir(vista.Mano!.Select(c => c.IdPersonaje).Distinct().Count() == vista.Mano!.Length);
        // Un cierre válido ya no publica solicitudes y solo atribuye ganador a una victoria.
        if (vista.Fase == "finalizada")
            Exigir(vista.Solicitud is null && vista.MotivoFin is "victoria" or "abandono" or "cancelacion" &&
                (vista.MotivoFin != "victoria" || Rol(vista.Ganador)));
```

#### Validar: resultados revelados

Solo se verifica la forma y roles de resultados; no se vuelve a resolver el duelo ni se recalculan puntajes.

```csharp
        foreach (var r in vista.Resultados!)
            Exigir(r.NumeroRonda > 0 && r.NumeroCombate > 0 && Rol(r.GanadorCombate) &&
                (r.GanadorRonda is null || Rol(r.GanadorRonda)) && r.Cartas.ContainsKey("J1") && r.Cartas.ContainsKey("J2") &&
                r.Puntajes.ContainsKey("J1") && r.Puntajes.ContainsKey("J2") && r.Atributo is not null);
```

#### Aplicar: guarda antes de mutación

La validación antecede a cualquier inserción en conocidos o cola. Los errores de forma se traducen en incompatibilidad; primera vista establece historia sin reproducirla.

```csharp

    internal void Aplicar(PresentacionCliente vista)
    {
        // Se valida antes de modificar el estado para no aplicar respuestas parcialmente incompatibles.
        try { Validar(vista); }
        catch (Exception ex) when (ex is NullReferenceException or KeyNotFoundException or ArgumentException)
        { throw new FalloCliente("protocoloIncompatible"); }
        // La primera vista establece la línea de base; una reconexión conserva las claves conocidas.
        bool inicial = Vista is null;
        foreach (var resultado in vista.Resultados.OrderBy(r => r.NumeroRonda).ThenBy(r => r.NumeroCombate))
            if (conocidos.Add((resultado.NumeroRonda, resultado.NumeroCombate)) && !inicial)
                Resultados.Enqueue(resultado);
        Vista = vista;
        Actual = true;
    }
```

### Relaciones y verificación

[EstadoClienteTests](archivos/test/proyecto-integrador-final-tests/EstadoClienteTests.md): primera vista/historia, orden tras reconectar, pausa y nombres con igual revisión, rechazo sin destruir estado y mano vacía frente a opciones imposibles. No prueban por sí solos el transporte. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/InterfazConsola.Red.cs</summary>

[Página individual](archivos/src/cliente/InterfazConsola.Red.md)

## src/cliente/InterfazConsola.Red.cs

Fuente: [src/cliente/InterfazConsola.Red.cs](../src/cliente/InterfazConsola.Red.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Adapta una presentación REST y el estado local de interacción a paneles de consola. Prioriza menú, resultado en lectura, final y selección actual, sin pedir teclas ni realizar red. Los ganadores vienen del servidor, no de una comparación local de puntajes.

### Dependencias

Es la segunda porción de [InterfazConsola](archivos/src/cliente/InterfazConsola.md): reutiliza Encabezado, Paneles, PanelInfo, ListaOpciones, MostrarCombate, MostrarMano, Escribir y Pie. Consume [ContratosPartida](archivos/src/cliente/ContratosPartida.md); lo invoca [SesionPartidaCliente](archivos/src/cliente/SesionPartidaCliente.md). Véase [diseño de interfaz](diseno-de-interfaz.md).

### Estructura y datos

`internal sealed partial class InterfazConsola`, sin campos, propiedades ni eventos nuevos. La herencia y atributos son los de la porción principal (ninguno explícito). Este archivo aporta un solo método internal con funciones locales. No conserva estado entre llamadas.

### Miembros y ejecución

`internal void MostrarRed(string local, PresentacionCliente? vista, bool actual, string aviso, ResultadoCliente? resultado, int menu, bool confirma, int foco, bool atributo, bool pendiente, bool abandono)` presenta y retorna void. local y aviso son textos, vista puede faltar; actual indica si se ha confirmado vigencia, no si hay una vista guardada. menu<0 significa cerrado; confirma selecciona No/Sí; foco selecciona carta o atributo; pendiente/abandono bloquean elección.

Función local `string Nombre(string rol)` usa Nombres de vista o el propio rol si no existe/está null. Resuelve roles, por lo que nombres iguales no identifican al ganador. El estado de encabezado se elige en orden: vista finalizada→Final confirmado; no actual→sin comunicación; vista pausada→pausa confirmada; resto→En línea. Añade aviso.

Orden de ramas y efectos:

1. menu≥0: dibuja menú o confirmación No/Sí con foco menu. Si abandono incierto imprime advertencia; pie de controles y return. Esta rama tiene precedencia incluso sobre un resultado o final.
2. vista null: pie Esc/Ctrl+C y return, sin acceder a datos de partida.
3. resultado no null: toma todos los resultados anteriores o iguales por coordenadas. Funciones locales `int Combates(string rol)` cuentan victorias de esa ronda hasta ese combate; `int Rondas(string rol)` cuenta ganadores de ronda hasta ese punto. Imprime coordenadas; MostrarCombate recibe cartas y puntajes revelados, ganador por rol, contador histórico y GanaJ1 explícito. Si GanadorRonda no null lo anuncia. Imprime marcador histórico y pie Enter/Esc, return. La revisión presente puede estar mucho más adelantada: no se mezcla su marcador.
4. Estado vigente: imprime nombres con J1/J2 y rol propio, y rondas del mapa Marcador. Si finalizada, victoria muestra Ganador; abandono muestra que no suma victoria; cualquier otro motivo se presenta como cancelación del operador (EstadoCliente ya restringe valores). Pie Enter para salir y return.
5. Si esperandoInicio, indica que depende del operador; si no, imprime coordenadas y cuenta combates ganados de la ronda actual.
6. Solo habilita selección si actual, no pausada, no pendiente, no abandono y solicitud no null. En atributo muestra opciones remotas de atributos con foco; en carta muestra texto de respuesta/apertura y cartas ordenadas según IDs permitidos, cada una resuelta con Single sobre la mano. Pie de controles/Esc.
7. Si no habilita selección, muestra mano completa propia. Si pendiente avisa recuperación; si no, actual y no pausada y no esperandoInicio, avisa que espera solicitud. Pie permite repintar estado o salir.

Fragmento de precedencia de menú y ausencia de vista; el encabezado lo precede:

```csharp
        if (menu >= 0)
        {
            Paneles(PanelInfo(confirma ? "¿Abandonar partida?" : "Menú",
                ListaOpciones(confirma ? ["No", "Sí"] : ["Continuar", "Salir sin abandonar", "Abandonar partida"], menu)));
            if (abandono) Escribir("Abandono sin confirmar. Salir localmente no confirma el abandono.");
            Pie(Controles); return;
        }
        if (vista is null) { Pie("Esc: menú · Ctrl+C: salir sin abandonar"); return; }
```

Fragmento de selección: el estado anterior ya fue dibujado; el `else` de espera lo sigue:

```csharp
        bool habilitada = actual && !vista.Pausada && !pendiente && !abandono && vista.Solicitud is not null;
        if (habilitada && vista.Solicitud is { } s)
        {
            if (atributo) Paneles(PanelInfo("Atributo del combate", ListaOpciones(s.AtributosPermitidos, foco)));
            else
            {
                Escribir(s.Tipo == "carta" ? "Te toca responder" : "Tu turno: carta y atributo");
                // El foco recorre exclusivamente las cartas incluidas en las opciones remotas.
                MostrarMano(s.IdsPersonajePermitidos.Select(id => vista.Mano.Single(c => c.IdPersonaje == id)).ToArray(), foco);
            }
            Pie(Controles + "\nEsc: menú");
```

Las opciones no se reconstruyen del catálogo local: la presentación remota manda el subconjunto y su orden.

### Recorridos y ejemplos

Normal: vista actual esperandoElector, solicitud cartaYAtributo IDs [7,9], foco 1, atributo=false: resuelve ambas cartas propias en ese orden y destaca ID9. Tras elegir carta, atributo=true presenta atributos permitidos en vez de la mano.

Alternativo: vista guardada pero actual=false nunca ofrece elección; presenta sin comunicación y última mano. Resultado (2,1) mientras vista ya está en ronda 3 muestra el marcador acumulado solo hasta (2,1). Ambos nombres Ana se distinguen para ganador usando rol J1/J2. Menú de abandono inicia en No por estado del coordinador.

### Errores, límites y efectos observables

Solo imprime. Confía en que EstadoCliente verificó claves, IDs y datos; Single lanza si faltan o se duplican cartas en mano. No vuelve a validar índice foco ni menu; el coordinador los limita. Los ayudantes principales neutralizan controles remotos y usan Text en vez de interpretar markup. No consulta HTTP ni cambia DTO, estado o marcador.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### MostrarRed: nombres y encabezado

Nombre resuelve por rol con fallback; la etiqueta prioriza cierre final, vigencia y pausa antes de dibujar.

```csharp
    internal void MostrarRed(string local, PresentacionCliente? vista, bool actual, string aviso,
        ResultadoCliente? resultado, int menu, bool confirma, int foco, bool atributo, bool pendiente, bool abandono)
    {
        // Se resuelven nombres por rol y se distingue pausa confirmada de pérdida de comunicación.
        string Nombre(string rol) => vista?.Nombres.GetValueOrDefault(rol) ?? rol;
        string estado = vista?.Fase == "finalizada" ? "Final confirmado" : !actual ? "Sin comunicación · Estado actual sin confirmar"
            : vista?.Pausada == true ? "Pausa confirmada" : "En línea";
        Encabezado(local, estado, aviso);
        // El menú conserva el estado en el encabezado y presenta No como primera opción de abandono.
```

#### MostrarRed: historial en lectura

Estos contadores recortan historia por ronda/combate del resultado que se lee. GanaJ1 deriva del rol, por lo que nombres iguales no confunden al ganador.

```csharp
            var anteriores = vista.Resultados.Where(x => x.NumeroRonda < r.NumeroRonda ||
                x.NumeroRonda == r.NumeroRonda && x.NumeroCombate <= r.NumeroCombate).ToArray();
            int Combates(string rol) => anteriores.Count(x => x.NumeroRonda == r.NumeroRonda && x.GanadorCombate == rol);
            int Rondas(string rol) => anteriores.Count(x => x.GanadorRonda == rol);
            Escribir($"Ronda {r.NumeroRonda} · Combate {r.NumeroCombate}");
            // Se muestran los ganadores publicados por el servidor, sin volver a comparar atributos.
            MostrarCombate(new(r.Atributo,
                new(Nombre("J1"), r.Cartas["J1"].Nombre, r.Puntajes["J1"]),
                new(Nombre("J2"), r.Cartas["J2"].Nombre, r.Puntajes["J2"]),
                Nombre(r.GanadorCombate), Combates("J1"), Combates("J2"), null, r.GanadorCombate == "J1"));
            if (r.GanadorRonda is { } ganador) Escribir($"{Nombre(ganador)} gana la ronda");
            Escribir($"Rondas · {Nombre("J1")} {Rondas("J1")} / {Nombre("J2")} {Rondas("J2")}");
            Pie("Enter para continuar · Esc: menú"); return;
        }
        // Al agotar la lectura se muestra el estado vigente, que puede estar en una ronda posterior.
        Escribir($"{Nombre("J1")} (J1) / {Nombre("J2")} (J2) · Tu plaza: {vista.RolPropio}");
        Escribir($"Rondas · {Nombre("J1")} {vista.Marcador["J1"]} / {Nombre("J2")} {vista.Marcador["J2"]}");
        // Abandono y cancelación tienen mensajes propios y no se presentan como victorias de juego.
        if (vista.Fase == "finalizada")
        {
            Escribir(vista.MotivoFin switch
```

#### MostrarRed: cierre y espera

La rama de cierre retorna antes de selección; abandono/cancelación no se muestran como victoria. En espera inicial se atribuye inicio al operador.

```csharp
            {
                "victoria" => $"{Nombre(vista.Ganador!)} gana la partida",
                "abandono" => "Partida finalizada por abandono, sin sumar una victoria de juego.",
                _ => "El operador canceló la partida, sin sumar una victoria de juego."
            });
            Pie("Enter para salir"); return;
        }
        // La espera inicial solo indica lo conocido: el inicio depende del operador.
        if (vista.Fase == "esperandoInicio") Escribir("Esperando inicio a cargo del operador.");
        else
        {
            Escribir($"Ronda {vista.NumeroRonda} · Combate {vista.NumeroCombate}");
            var combates = vista.Resultados.Where(r => r.NumeroRonda == vista.NumeroRonda).ToArray();
            Escribir($"Combates · {Nombre("J1")} {combates.Count(r => r.GanadorCombate == "J1")} / {Nombre("J2")} {combates.Count(r => r.GanadorCombate == "J2")}");
        }
        // Una vista anterior o una operación sin resolver nunca habilitan otra elección.
```

#### MostrarRed: selección bloqueada

Cuando no hay elección habilitada conserva mano propia y explica operación pendiente o espera de solicitud, sin inventar opciones.

```csharp
        else
        {
            // Durante la espera se conserva la mano propia y se explica si falta confirmar una elección.
            MostrarMano(vista.Mano);
            if (pendiente) Escribir("Confirmación pendiente · Comprobando la elección antes de permitir otra.");
            else if (actual && !vista.Pausada && vista.Fase != "esperandoInicio") Escribir("Esperando solicitud del servidor.");
            Pie("Enter: volver a mostrar el estado · Esc: menú · Ctrl+C: salir sin abandonar");
        }
```

### Relaciones y verificación

[PresentacionRedTests](archivos/test/proyecto-integrador-final-tests/PresentacionRedTests.md) parametriza anchos y contrasta marcador histórico y neutralización de texto remoto; [SesionClienteTests](archivos/test/proyecto-integrador-final-tests/SesionClienteTests.md) verifica navegación, pendientes y resultados. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/InterfazConsola.cs</summary>

[Página individual](archivos/src/cliente/InterfazConsola.md)

## src/cliente/InterfazConsola.cs

Fuente: [src/cliente/InterfazConsola.cs](../src/cliente/InterfazConsola.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Dibuja paneles del juego y ofrece selección síncrona para el auxiliar de demostración. El cliente REST usa principalmente la otra porción partial, [InterfazConsola.Red](archivos/src/cliente/InterfazConsola.Red.md), con su coordinador asíncrono. Este archivo no envía HTTP ni calcula victorias de juego.

### Dependencias

Spectre.Console 0.57.2 aporta IAnsiConsole, Text, Panel, Rows, Columns, Grid, Table y Rule. `CultureInfo.InvariantCulture` fija números; UnicodeCategory permite neutralizar texto remoto. [VistasConsola](archivos/src/cliente/VistasConsola.md) lleva datos; [DemostracionConsola](archivos/src/cliente/DemostracionConsola.md) pide selecciones; [SesionPartidaCliente](archivos/src/cliente/SesionPartidaCliente.md) usa presentación de red. Véase [diseño de interfaz](diseno-de-interfaz.md).

### Estructura

`internal sealed partial class InterfazConsola`; la porción de red declara MostrarRed y no duplica estos campos. No tiene interfaces, atributos, eventos ni operadores propios. Métodos públicos tienen visibilidad efectiva limitada por el tipo internal; los ayudantes son privados y algunos static.

### Estado y datos

| Declaración | Valor y uso |
|---|---|
| private static readonly `string[] Atributos` | Fuerza, Velocidad, Resistencia, Inteligencia, Energía, Habilidad; opciones por defecto. readonly no congela elementos, pero el código no los muta. |
| private static readonly `Color Verde` | RGB 189,232,140. |
| private static readonly `Style Titulo` | Verde y negrita. |
| private static readonly `Style Barra` | Verde sobre RGB 32,44,36 y negrita. |
| private static readonly `Style Borde` | RGB 95,120,105. |
| private const `string Controles` | Flechas/WASD/HJKL, salto de línea y Enter para confirmar. |
| private readonly `TextReader entrada` | Entrada inyectada o Console.In; no se dispone aquí. |
| private readonly bool `leerTeclas`, `limpiarPantalla`, `salidaLineal` | Fijados en constructor según redirección, lector y ancho. |
| private readonly `IAnsiConsole terminal` | Consola de Spectre con salida/configuración propia. |
| private `string? ultimaEspera` | null inicial; evita repetir un mismo mensaje de espera, se invalida con cualquier Renderizar. |

### Miembros y ejecución

#### Construcción y pantallas de preparación/mano

`public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)` considera interactiva solo si salida no inyectada y stdout no redirigido. En ese caso limpia pantalla; lee teclas solo además si no hay lector inyectado y stdin no redirigido. Salida lineal se activa si no interactiva y ancho no proporcionado. Crea terminal con ANSI Detect en interactiva o No en el resto. Si ancho presente fija Profile.Width y fuerza capacidad Unicode. No valida ancho manualmente ni posee/disciplina los flujos del llamador.

- `public void MostrarPreparacion(string? local, string estado)`: encabezado Preparación, panel identidad (pendiente si null) y estado sanitizado.
- `public void MostrarEsperaInicial(string local, string participantes, string motivo)`: encabezado con motivo, participantes e inicio a cargo del operador; pie esperar rival/inicio.
- `public void MostrarInicioRonda(MesaVisible mesa, string mano, IReadOnlyList<CartaVisible> cartas)`: anuncia mano, resume cartas y rondas. Si mesa.Local==mano pie pide carta/atributo; si no, espera al nombre mano. Comparación de strings, no roles, en este auxiliar local.
- `public void MostrarMano(IReadOnlyList<CartaVisible> mano)` delega a privada con foco null.
- `private void MostrarMano(IReadOnlyList<CartaVisible> mano, int? foco, string titulo = "Tu mano")`: título con cantidad y singular/plural; mano vacía imprime aviso y retorna. Si hay cartas crea Ficha por elemento, enfocada cuando índice==foco, y las pasa a Paneles. No cambia la mano.
- `public void MostrarEsperaRival(MesaVisible mesa, string rival, IReadOnlyList<CartaVisible> cartas)`: estado rival eligiendo, resumen de mano y marcador, pie espera de solicitud.

#### Selección síncrona

`public CartaVisible PedirCarta(IReadOnlyList<CartaVisible> mano)` copia opciones con CopiarOpciones y llama Elegir. Su lambda Action<int> redibuja encabezado, mano con foco y controles; retorna la carta del índice confirmado.

`public CartaVisible PedirCarta(MesaVisible mesa, IReadOnlyList<CartaVisible> cartas, bool abre)` hace la misma copia. Lambda distingue apertura (rondas y pie indicando atributo siguiente) de respuesta (combates/rondas y Cartas disponibles); retorna referencia confirmada. No retira cartas.

`public string PedirAtributo(IReadOnlyList<string>? permitidos = null, MesaVisible? mesa = null)` copia permitidos o usa seis Atributos. Cero opciones lanza InvalidOperationException antes de dibujar. Lambda usa encabezado simple o de mesa según null, panel de lista y pie de controles; retorna el string del índice confirmado. No valida sus nombres contra la lista canónica si el llamador pasa otros.

`private int Elegir(int cantidad, Action<int> dibujar)` inicializa foco 0 y dibuja. Repite LeerTecla; Enter retorna foco. Up/Left/W/A/K/H restan uno, Down/Right/S/D/J/L suman uno, otras teclas paso 0. Clamp evita salir de [0,cantidad−1], sin circular; solo redibuja si cambió foco. Las precondiciones de cantidad positiva las garantizan CopiarOpciones/PedirAtributo. No hay cancelación por token ni Esc especial en esta selección síncrona.

```csharp
            int paso = tecla switch
            {
                ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.K or ConsoleKey.H => -1,
                ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L => 1,
                _ => 0
            };

            int nuevo = Math.Clamp(foco + paso, 0, cantidad - 1);
            if (nuevo != foco)
            {
                foco = nuevo;
                dibujar(foco);
            }
```

`private ConsoleKey LeerTecla()` usa ReadKey(intercept:true).Key si leerTeclas; de otro modo ReadLine, EOF lanza EndOfStreamException con mensaje. Trim y minúsculas: vacío Enter; w/a/s/d/h/j/k/l sus teclas; cualquier otro texto NoName. A diferencia de EntradaCliente, aquí `esc` escrito no se convierte en Escape, ni Ctrl+D/Ctrl+Z físicos tienen un tratamiento propio sobre KeyChar.

#### Envíos, resultados, cierres y avisos públicos

| Firma | Decisiones, salida y efectos |
|---|---|
| `void MostrarEnvio(MesaVisible mesa, EstadoEnvio estado)` | Encabezado; switch Enviando→en curso, Recibido→confirmada, Pendiente→comprobando recibo. Estado enum desconocido lanza ArgumentOutOfRangeException. Siempre título elección en curso; Pendiente usa Pie, otros Escribir. |
| `void MostrarCombate(MesaVisible mesa, CombateVisible combate)` | Encabezado con rondas y delega a la sobrecarga sin mesa. |
| `void MostrarCombate(CombateVisible combate)` | Escribe atributo, determina resaltado usando GanaJ1 explícito o comparación de nombre J1 con Ganador. Crea fichas jugadas. Si ancho<54 usa salto de línea entre elementos; si no separador punto medio. Si ProximoEnElegir no null añade anuncio; pie con victorias de combate. No compara puntajes para elegir ganador. |
| `void MostrarCierreRonda(MesaVisible mesa, string ganador, string? proximoMano)` | Encabezado ronda finalizada; ganador, combates, rondas/meta15. Si próximo no null anuncia ronda+1; si null omite ese pie. No incrementa marcador. |
| `void MostrarCierrePartida(string local, string ganador, JugadorVisible j1, JugadorVisible j2)` | Encabezado y ganador; tabla dos filas con nombres/rondas, borde cuadrado; pie Enter salir. No lee Enter. |
| `void MostrarSuspension(MesaVisible mesa, string motivo, bool confirmada)` | Distingue pausa confirmada de sin comunicación; muestra último marcador confirmado. |
| `void MostrarInterrupcion(string local, string causa, string efecto, bool confirmada)` | Encabezado de sesión; etiqueta Final confirmado o Salida local, causa y efecto en pie. |
| `void MostrarMarcador(JugadorVisible j1, JugadorVisible j2, int ronda)` | Encabezado sin identidad local con número de ronda y rondas de ambos; no cambia estado. |
| `void MostrarEsperando(string mensaje)` | Si ultimaEspera==mensaje retorna sin imprimir. En otro caso llama MostrarEstado y después registra el mensaje (Renderizar lo había invalidado). |
| `void MostrarGanadorRonda(JugadorVisible ganador)` | MostrarEstado con nombre y rondas. |
| `void MostrarGanadorPartida(JugadorVisible ganador)` | Igual, título de partida. |
| `void MostrarError(string mensaje)` | Texto prefijado ERROR, rojo y negrita; contenido sanitizado. |
| `void MostrarEstado(string titulo, string mensaje)` | Encabezado sin local con título y después mensaje sanitizado. |

Todas las firmas de esta tabla son public y retornan void. Los objetos de entrada son de solo lectura desde estas operaciones.

#### Composición y formato privados

- `void Encabezado(MesaVisible mesa, string? detalle = null)`: delega con Local, Ronda y Combate al otro encabezado.
- `void Encabezado(string? local, string situacion, string? detalle = null)`: si limpiarPantalla, terminal.Clear(home:true). Identidad GUERRA o GUERRA/local. Sanitiza situación/detalle, compone con separador; si longitud >= ancho, coloca cada parte en línea separada. Escribe línea vacía, barra y Rule. No trunca datos.
- `void Pie(string mensaje)`: Rule y Escribir con estilo Barra.
- `void Paneles(params Panel[] paneles)`: si no salidaLineal, hay más de uno y ancho≥cantidad×27, reparte `espacio=ancho−cantidad+1`, anchura entera y resto a los primeros paneles; Renderizar Columns con padding derecho 1 y return. En otros casos recorre y renderiza uno por uno. No cambia orden, sí Width de cada panel recibido.

```csharp
        if (!salidaLineal && paneles.Length > 1 && ancho >= paneles.Length * 27)
        {
            int espacio = ancho - paneles.Length + 1;
            for (int indice = 0; indice < paneles.Length; indice++)
                paneles[indice].Width = espacio / paneles.Length + (indice < espacio % paneles.Length ? 1 : 0);

            Renderizar(new Columns(paneles) { Padding = new Padding(0, 0, 1, 0) });
            return;
        }

        foreach (Panel panel in paneles)
            Renderizar(panel);
```

- `static Panel PanelInfo(string titulo, string detalle)`: separa detalle en saltos y crea Rows de Text sanitizados; delega a sobrecarga IRenderable.
- `static Panel PanelInfo(string titulo, IRenderable detalle)`: Panel con título sanitizado y Markup.Escape dentro del markup fijo negrita verde; borde cuadrado, estilo Borde, Expand=true. Solo el título usa markup y está escapado.
- `static Panel Ficha(CartaVisible carta, bool enfocada)`: Grid de dos columnas (valor alineado derecha), recorre mapa en su orden de enumeración y agrega atributos y enteros invariantes; Rows con Opcion(nombre,enfocada) y grid. Borde Titulo si enfocada, si no Borde.
- `static Panel FichaJugada(CartaJugada jugada, bool ganadora)`: Rows de jugador/carta, valor y Ganador o vacío; borde ganador resaltado. No infiere ganador por número.
- `static string ResumenMano(IReadOnlyList<CartaVisible> cartas)`: aviso si vacía, si no concatena nombres sanitizados con punto medio.
- `static string Rondas(MesaVisible mesa)`: nombre J1/rondas J1, barra, nombre J2/rondas J2.
- `static string Combates(MesaVisible mesa)`: mismo formato con contadores parciales.
- `static string Marcador(MesaVisible mesa)`: Combates y Rondas en dos líneas.
- `static CartaVisible[] CopiarOpciones(IReadOnlyList<CartaVisible> mano)`: Count>0 produce ToArray; si cero lanza InvalidOperationException. Es copia de colección, no clon de cartas.
- `static Rows ListaOpciones(IEnumerable<string> opciones, int foco)`: enumera opciones y genera Opcion destacando índice igual a foco.
- `static Text Opcion(string opcion, bool enfocada)`: enfocada usa prefijo `> ` y estilo Barra; resto dos espacios sin estilo. Sanitiza texto, no interpreta markup.
- `static string TextoVisible(string texto)`: por cada carácter reemplaza por espacio controles, categorías Format, LineSeparator y ParagraphSeparator; resto conserva. No borra markup textual, que se dibuja como Text.

```csharp
    private static string TextoVisible(string texto) =>
        string.Concat(texto.Select(caracter =>
            char.IsControl(caracter) || char.GetUnicodeCategory(caracter) is UnicodeCategory.Format
                or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator
                ? ' ' : caracter));
```

- `void Escribir(string texto, Style? estilo = null)`: divide por `\n`, crea Text sanitizado para cada línea y Renderizar Rows. La división previa conserva saltos internos de textos compuestos; controles incrustados en nombres ya se neutralizan antes de concatenarlos.
- `void Renderizar(IRenderable contenido)`: terminal.Write y ultimaEspera=null. Todas las salidas por aquí invalidan deduplicación salvo que MostrarEsperando la establezca después.

### Recorridos y ejemplos

Normal: mano [A,B,C], foco inicial A. Teclas S,S,S,Enter seleccionan C: el tercer S queda limitado en índice2 y no redibuja; Enter retorna la misma instancia C sin quitarla. Un ancho 90 y tres paneles permite columnas (90≥81); ancho 60 obliga apilado para tres.

Alternativo: mano vacía en MostrarMano imprime aviso; la misma mano en PedirCarta lanza antes de leer. Un nombre remoto con escape/control se muestra con espacios en esas posiciones, sin ejecutar controles de terminal. EOF de lector síncrono lanza; no se transforma en confirmación. MostrarEnvio((EstadoEnvio)99) lanza. No hay un flujo alternativo de red aquí: eso se presenta en la porción Red.

### Errores, límites y efectos observables

Solo modifica pantalla, terminal y ultimaEspera, además del Width de paneles internos. E/S, datos null o claves ausentes pueden lanzar: no hay captura general ni validación semántica de las vistas. La selección síncrona bloquea esperando entrada y no coordina presencia; el ejecutable REST usa EntradaCliente y SesionPartidaCliente para evitar ese bloqueo. No conserva ni envía elecciones en este archivo.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### Constructor: terminal y redirección

El modo interactivo determina limpieza, ANSI y teclas; ancho explícito configura perfil y permite probar composición sin terminal física.

```csharp
    public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)
    {
        this.entrada = entrada ?? Console.In;
        bool interactiva = salida is null && !Console.IsOutputRedirected;
        limpiarPantalla = interactiva;
        leerTeclas = interactiva && entrada is null && !Console.IsInputRedirected;
        salidaLineal = !interactiva && ancho is null;
        terminal = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(salida ?? Console.Out),
            Ansi = interactiva ? AnsiSupport.Detect : AnsiSupport.No
        });

        if (ancho is { } fijo)
        {
            terminal.Profile.Width = fijo;
            terminal.Profile.Capabilities.Unicode = true;
        }
    }
```

#### MostrarInicioRonda y MostrarMano: estado y vacío

La igualdad de nombre decide el pie local del auxiliar. El aviso de mano vacía no intenta crear columnas de cartas.

```csharp
    public void MostrarInicioRonda(MesaVisible mesa, string mano, IReadOnlyList<CartaVisible> cartas)
    {
        Encabezado(mesa, $"{TextoVisible(mano)} abre la ronda");
        Paneles(PanelInfo("Tu mano", ResumenMano(cartas)), PanelInfo("Rondas ganadas", Rondas(mesa)));
        Pie(mesa.Local == mano ? "Elegí una carta y después un atributo." : $"Esperá a {TextoVisible(mano)}.");
    }

    public void MostrarMano(IReadOnlyList<CartaVisible> mano) => MostrarMano(mano, foco: null);

    private void MostrarMano(IReadOnlyList<CartaVisible> mano, int? foco, string titulo = "Tu mano")
    {
        Escribir($"{titulo} · {mano.Count} {(mano.Count == 1 ? "carta" : "cartas")}", Titulo);
        if (mano.Count == 0)
        {
            Escribir("No hay cartas disponibles.");
            return;
        }

        Paneles(mano.Select((carta, i) => Ficha(carta, i == foco)).ToArray());
    }
```

#### PedirCarta: lambda de selección contextual

La copia fija las opciones durante la selección síncrona. El callback redibuja apertura o respuesta, y el índice retornado selecciona la referencia original.

```csharp
    public CartaVisible PedirCarta(MesaVisible mesa, IReadOnlyList<CartaVisible> cartas, bool abre)
    {
        CartaVisible[] opciones = CopiarOpciones(cartas);
        int elegida = Elegir(opciones.Length, foco =>
        {
            string estado = abre
                ? $"Rondas ganadas · {Rondas(mesa)}"
                : $"Te toca responder · Marcador · Combates {Combates(mesa)} · Rondas {Rondas(mesa)}";
            Encabezado(mesa, estado);
            MostrarMano(opciones, foco, abre ? "Tu mano" : "Cartas disponibles");
            Pie(abre ? $"Tu turno\nDespués elegís el atributo.\n{Controles}" : Controles);
        });
        return opciones[elegida];
    }
```

#### PedirAtributo: guarda y callback

La lista vacía falla antes de leer; mesa opcional elige encabezado, sin cambiar reglas de foco.

```csharp
    public string PedirAtributo(IReadOnlyList<string>? permitidos = null, MesaVisible? mesa = null)
    {
        string[] opciones = permitidos?.ToArray() ?? Atributos;
        if (opciones.Length == 0)
            throw new InvalidOperationException("Se solicitó un atributo sin opciones disponibles.");

        int elegida = Elegir(opciones.Length, foco =>
        {
            if (mesa is null)
                Encabezado((string?)null, "Elegí un atributo");
            else
                Encabezado(mesa, "Elegí un atributo");

            Paneles(PanelInfo("Atributo del combate", ListaOpciones(opciones, foco)));
            Pie(Controles);
        });
        return opciones[elegida];
    }
```

#### MostrarEnvio: estados válidos y desconocidos

Cada enum tiene mensaje propio; desconocido lanza. Pendiente se enfatiza en el pie en vez de describirse como recepción.

```csharp
    public void MostrarEnvio(MesaVisible mesa, EstadoEnvio estado)
    {
        Encabezado(mesa);
        (string etiqueta, string mensaje) = estado switch
        {
            EstadoEnvio.Enviando => ("Envío en curso", "Enviando tu elección…"),
            EstadoEnvio.Recibido => ("Recepción confirmada", "Elección recibida. Esperando el resultado."),
            EstadoEnvio.Pendiente => ("Confirmación pendiente", "Comprobando el recibo. La elección sigue en trámite."),
            _ => throw new ArgumentOutOfRangeException(nameof(estado))
        };
        Escribir("Elección en curso", Titulo);
        if (estado == EstadoEnvio.Pendiente)
            Pie($"{etiqueta} · {TextoVisible(mensaje)}");
        else
            Escribir($"{etiqueta} · {TextoVisible(mensaje)}");
    }
```

#### MostrarCombate: ganador y ancho

GanaJ1 explícito prevalece sobre nombres; el ancho cambia separadores y ProximoEnElegir null evita anunciar otro combate.

```csharp
    public void MostrarCombate(CombateVisible combate)
    {
        Escribir($"Atributo · {TextoVisible(combate.Atributo)}", Titulo);
        bool ganaJ1 = combate.GanaJ1 ?? combate.J1.Jugador == combate.Ganador;
        Paneles(FichaJugada(combate.J1, ganaJ1), FichaJugada(combate.J2, !ganaJ1));

        string separador = terminal.Profile.Width < 54 ? "\n" : " · ";
        string siguiente = combate.ProximoEnElegir is { } nombre
            ? $"\nPróximo combate{separador}{TextoVisible(nombre)} elige el atributo" : "";
        Pie($"Combates de la ronda{separador}{TextoVisible(combate.J1.Jugador)} {combate.CombatesGanadosJ1} / {TextoVisible(combate.J2.Jugador)} {combate.CombatesGanadosJ2}{siguiente}");
    }
```

#### MostrarCierreRonda y MostrarCierrePartida

El cierre de ronda omite próximo mano si no existe. El cierre de partida crea tabla con valores recibidos y solo imprime indicación, sin leer Enter.

```csharp
    public void MostrarCierreRonda(MesaVisible mesa, string ganador, string? proximoMano)
    {
        Encabezado(mesa.Local, $"Ronda {mesa.Ronda} finalizada");
        Escribir($"{TextoVisible(ganador)} gana la ronda", Titulo);
        Escribir($"Combates · {Combates(mesa)}");
        Escribir($"Rondas ganadas · {Rondas(mesa)} · Meta 15");
        if (proximoMano is not null)
            Pie($"{TextoVisible(proximoMano)} abre la ronda {mesa.Ronda + 1}");
    }

    public void MostrarCierrePartida(string local, string ganador, JugadorVisible j1, JugadorVisible j2)
    {
        Encabezado(local, "Partida finalizada");
        Escribir($"{TextoVisible(ganador)} gana la partida", Titulo);
        var planilla = new Table { Border = TableBorder.Square, BorderStyle = Borde, Expand = true };
        planilla.AddColumn("Jugador").AddColumn("Resultado");
        planilla.AddRow(new Text(TextoVisible(j1.Nombre)), new Text($"{j1.RondasGanadas} rondas ganadas"));
        planilla.AddRow(new Text(TextoVisible(j2.Nombre)), new Text($"{j2.RondasGanadas} rondas ganadas"));
        Renderizar(planilla);
        Pie("Pulsá Enter para salir.");
    }
```

#### MostrarSuspension y MostrarInterrupcion

Los booleanos confirmada cambian etiquetas, evitando presentar pérdida de red o cierre local como final remoto.

```csharp
    public void MostrarSuspension(MesaVisible mesa, string motivo, bool confirmada)
    {
        Encabezado(mesa);
        Escribir("Jugada temporalmente indisponible", Titulo);
        string estado = confirmada ? "Pausa confirmada" : "Sin comunicación · Estado actual sin confirmar";
        Escribir($"{TextoVisible(estado)} · {TextoVisible(motivo)}");
        Pie($"Último marcador confirmado · {Marcador(mesa)}");
    }

    public void MostrarInterrupcion(string local, string causa, string efecto, bool confirmada)
    {
        Encabezado(local, $"Sesión de {TextoVisible(local)}");
        Escribir("Cierre de sesión o partida", Titulo);
        Escribir($"{(confirmada ? "Final confirmado" : "Salida local")} · {TextoVisible(causa)}");
        Pie(TextoVisible(efecto));
    }
```

#### MostrarEsperando: deduplicación

Solo un mensaje igual al último de espera se omite; MostrarEstado renderiza y después se vuelve a fijar ultimaEspera.

```csharp
    public void MostrarEsperando(string mensaje)
    {
        if (ultimaEspera == mensaje)
            return;

        MostrarEstado("En espera", mensaje);
        ultimaEspera = mensaje;
    }
```

#### Encabezado: limpieza y disposición

El encabezado elige identidad, sanea texto y apila las partes si su longitud alcanza el ancho. No interpreta texto remoto como markup.

```csharp
    private void Encabezado(string? local, string situacion, string? detalle = null)
    {
        if (limpiarPantalla)
            terminal.Clear(home: true);

        string identidad = local is null ? "GUERRA" : $"GUERRA / {TextoVisible(local)}";
        string[] partes = detalle is null ? [identidad, TextoVisible(situacion)] : [identidad, TextoVisible(situacion), TextoVisible(detalle)];
        string linea = string.Join("  │  ", partes);
        terminal.WriteLine();
        Escribir(linea.Length >= terminal.Profile.Width ? string.Join('\n', partes) : linea, Barra);
        Renderizar(new Rule { Style = Borde });
    }
```

#### PanelInfo y Ficha: texto y atributos

El título de panel escapa markup; los valores de cartas se convierten con cultura invariante y el foco solo cambia el estilo de borde.

```csharp
    private static Panel PanelInfo(string titulo, string detalle) =>
        PanelInfo(titulo, new Rows(detalle.Split('\n').Select(linea => new Text(TextoVisible(linea)))));

    private static Panel PanelInfo(string titulo, IRenderable detalle) => new(detalle)
    {
        Header = new PanelHeader($"[bold #bde88c]{Markup.Escape(TextoVisible(titulo))}[/]"),
        Border = BoxBorder.Square,
        BorderStyle = Borde,
        Expand = true
    };

    private static Panel Ficha(CartaVisible carta, bool enfocada)
    {
        var valores = new Grid { Expand = true };
        valores.AddColumn(new GridColumn());
        valores.AddColumn(new GridColumn { Alignment = Justify.Right });
        foreach ((string atributo, int valor) in carta.Atributos)
            valores.AddRow(new Text(TextoVisible(atributo)), new Text(valor.ToString(CultureInfo.InvariantCulture), Titulo));

        var ficha = new Rows(Opcion(carta.Nombre, enfocada), valores);
        return new Panel(ficha) { Border = BoxBorder.Square, BorderStyle = enfocada ? Titulo : Borde, Expand = true };
    }
```

#### FichaJugada: ganador anunciado

Recibe el booleano ganador, dibuja el valor sin compararlo y añade etiqueta/borde ganador.

```csharp
    private static Panel FichaJugada(CartaJugada jugada, bool ganadora)
    {
        var resultado = new Rows(
            new Text(TextoVisible($"{jugada.Jugador} · {jugada.Carta}")),
            new Text(jugada.Valor.ToString(CultureInfo.InvariantCulture), Titulo),
            new Text(ganadora ? "Ganador" : "", Titulo));
        return new Panel(resultado)
        {
            Border = BoxBorder.Square,
            BorderStyle = ganadora ? Titulo : Borde,
            Expand = true
        };
    }
```

#### ResumenMano y formatos de marcador

Estas expresiones no cambian marcador: resumen vacío, contadores, guarda de opciones, fila enfocada y lista se construyen a partir de datos recibidos.

```csharp
    private static string ResumenMano(IReadOnlyList<CartaVisible> cartas) => cartas.Count == 0
        ? "No hay cartas disponibles."
        : string.Join(" · ", cartas.Select(c => TextoVisible(c.Nombre)));

    private static string Rondas(MesaVisible mesa) =>
        $"{TextoVisible(mesa.J1.Nombre)} {mesa.J1.RondasGanadas} / {TextoVisible(mesa.J2.Nombre)} {mesa.J2.RondasGanadas}";

    private static string Combates(MesaVisible mesa) =>
        $"{TextoVisible(mesa.J1.Nombre)} {mesa.CombatesJ1} / {TextoVisible(mesa.J2.Nombre)} {mesa.CombatesJ2}";

    private static string Marcador(MesaVisible mesa) =>
        $"Combates · {Combates(mesa)}\nRondas · {Rondas(mesa)}";

    private static CartaVisible[] CopiarOpciones(IReadOnlyList<CartaVisible> mano) =>
        mano.Count > 0 ? mano.ToArray() : throw new InvalidOperationException("Se solicitó una carta con la mano vacía.");

    private static Rows ListaOpciones(IEnumerable<string> opciones, int foco) =>
        new(opciones.Select((opcion, i) => Opcion(opcion, i == foco)));

    private static Text Opcion(string opcion, bool enfocada) =>
        enfocada ? new Text($"> {TextoVisible(opcion)}", Barra) : new Text($"  {TextoVisible(opcion)}");
```

#### LeerTecla: modo físico y línea

En modo de líneas solo las teclas listadas se traducen; EOF lanza y nunca se transforma en Enter.

```csharp
    private ConsoleKey LeerTecla()
    {
        if (leerTeclas)
            return Console.ReadKey(intercept: true).Key;

        // Con entrada por líneas, una línea vacía equivale a presionar Enter.
        string linea = entrada.ReadLine()
            ?? throw new EndOfStreamException("Se cerró la entrada de consola.");

        return linea.Trim().ToLowerInvariant() switch
        {
            "" => ConsoleKey.Enter,
            "w" => ConsoleKey.W,
            "a" => ConsoleKey.A,
            "s" => ConsoleKey.S,
            "d" => ConsoleKey.D,
            "h" => ConsoleKey.H,
            "j" => ConsoleKey.J,
            "k" => ConsoleKey.K,
            "l" => ConsoleKey.L,
            _ => ConsoleKey.NoName
        };
    }
```

#### Escribir y Renderizar: salida y caché de espera

Separar líneas precede al saneado y toda renderización invalida ultimaEspera. La caché solo se vuelve a establecer desde MostrarEsperando.

```csharp
    private void Escribir(string texto, Style? estilo = null) =>
        Renderizar(new Rows(texto.Split('\n').Select(linea => new Text(TextoVisible(linea), estilo))));

    private void Renderizar(IRenderable contenido)
    {
        terminal.Write(contenido);
        ultimaEspera = null;
    }
```

### Relaciones y verificación

[InterfazConsolaTests](archivos/test/proyecto-integrador-final-tests/InterfazConsolaTests.md) cubre selección, foco, fin de entrada y formato; [PantallasConsolaTests](archivos/test/proyecto-integrador-final-tests/PantallasConsolaTests.md) cubre preparación, espera, anchos, envío y cierres; [PresentacionRedTests](archivos/test/proyecto-integrador-final-tests/PresentacionRedTests.md) agrega pruebas del adaptador. Esos escenarios no cubren todos los drivers/terminales ni garantizan renderizado idéntico entre plataformas. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/Program.cs</summary>

[Página individual](archivos/src/cliente/Program.md)

## src/cliente/Program.cs

Fuente: [src/cliente/Program.cs](../src/cliente/Program.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `(global)`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Es el punto de entrada real del ejecutable cliente: pide configuración, crea la identidad del proceso y ejecuta la sesión REST. No ofrece selector de demostración ni acepta argumentos de aplicación.

### Dependencias

[ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md), [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md), [EntradaCliente](archivos/src/cliente/EntradaCliente.md), [SesionPartidaCliente](archivos/src/cliente/SesionPartidaCliente.md), [InterfazConsola](archivos/src/cliente/InterfazConsola.md). `CancellationTokenSource` coordina salida; `SafeFileHandle`, FileStream, StreamWriter y UTF8Encoding permiten detectar tubería de stdout rota en Unix. Véase [instalación y uso](instalacion-y-uso.md).

### Estructura y datos

`internal static class Program` en namespace global, sin interfaces, atributos, partial, propiedades ni campos. Dos métodos privados static. El estado vive en variables de Main: fuente de cancelación, callback, lector, IdSesion y flag corregir. Las variables using se liberan al salir de su ámbito; el descriptor stdout no es propiedad del cliente.

### Miembros y ejecución

`private static int InformarSalida(string mensaje)`: escribe mensaje a stdout y retorna 0. Si falla con IOException, intenta escribir a stderr `No se pudo usar la consola.`; si stderr también falla lo ignora y retorna 1. No imprime credenciales ni la excepción.

`private static async Task<int> Main()`: crea CancellationTokenSource y agrega ConsoleCancelEventHandler `cancelar = (_,e) => {e.Cancel=true; cierre.Cancel();}`. Impide la terminación inmediata de Ctrl+C para que se cancelen y esperen las tareas. En Unix con salida redirigida sustituye Console.Out por StreamWriter UTF-8 sin BOM, AutoFlush=true, sobre descriptor 1 y ownsHandle=false. Eso permite detectar EPIPE sin cerrar el descriptor estándar como propio.

Si stdin redirigido abre un StreamReader del estándar; si no usa Console.In. Genera un único Guid.NewGuid antes de cualquier corrección. En do/while lee configuración, crea y dispone transporte y entrada por cada intento, ejecuta una sesión nueva y repite solo si devuelve corregir=true y no se canceló. La sesión nueva evita arrastrar operaciones de otra plaza, conservando UUID del proceso.

```csharp
            Guid idSesion = Guid.NewGuid();
            bool corregir;
            do
            {
                var configuracion = await ConfiguracionCliente.LeerAsync(lector, Console.Out, cierre.Token);
                // Cada configuración tiene su transporte y coordinador para no mezclar operaciones de otras plazas.
                using var cliente = new ClientePartidaHttp(configuracion, idSesion);
                using var entrada = new EntradaCliente(lector);
                corregir = await new SesionPartidaCliente(configuracion, cliente, new InterfazConsola()).Ejecutar(entrada, cierre.Token);
            } while (corregir && !cierre.IsCancellationRequested);
            return 0;
```

EOF se traduce a salida local con mensaje de partida guardada mediante InformarSalida. OperationCanceledException se captura solo si cierre está cancelado y también informa salida local. IOException emite diagnóstico controlado a stderr y devuelve 1 (también ignora IOException secundaria en stderr). El finally siempre retira el manejador CancelKeyPress. Otras excepciones no tienen captura general aquí.

```csharp
        catch (EndOfStreamException)
        {
            return InformarSalida("Salida local · Fin de la entrada. La partida queda guardada.");
        }
        catch (OperationCanceledException) when (cierre.IsCancellationRequested)
        {
            return InformarSalida("Salida local · La partida queda guardada.");
        }
        catch (IOException)
        {
            try { Console.Error.WriteLine("No se pudo usar la consola."); } catch (IOException) { }
            return 1;
        }
        // Se retira el manejador al terminar para no dejar callbacks asociados al proceso.
        finally { Console.CancelKeyPress -= cancelar; }
```

### Recorridos y ejemplos

Normal: configuración válida, incorporación y cierre local desde menú: Ejecutar devuelve false, se disponen cliente/entrada y Main retorna 0. Corrección de credencial: la sesión devuelve true, se dispone el transporte y se piden los cuatro datos de nuevo con el mismo UUID de proceso.

Alternativo: EOF mientras se pide el nombre informa fin de entrada sin enviar abandono. Ctrl+C solicita cancelación y cierre ordenado. Una tubería stdout cerrada produce código 1 y diagnóstico por stderr; no se presenta como un defecto general del juego en el entorno del usuario.

### Errores, límites y efectos observables

No guarda configuración ni identidad en disco; reiniciar genera otra sesión. Salida local no confirma abandono ni libera inmediatamente plaza. La sustitución de Console.Out afecta el proceso entero. Los recursos HTTP se disponen tras terminar y esperar el coordinador, evitando tareas que usen un transporte ya cerrado.

### Relaciones y verificación

[ArranqueRestTests](archivos/test/proyecto-integrador-final-tests/ArranqueRestTests.md), [ClienteProcesoRestTests](archivos/test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md) y [EntradaProcesosContratoTests](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) cubren procesos, EOF y recuperación. Requieren las DLL reales para ejecutar contratos condicionales; no acredita Windows por leer esta rama Unix. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/SesionPartidaCliente.cs</summary>

[Página individual](archivos/src/cliente/SesionPartidaCliente.md)

## src/cliente/SesionPartidaCliente.cs

Fuente: [src/cliente/SesionPartidaCliente.cs](../src/cliente/SesionPartidaCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Es el coordinador del cliente REST. Un único bucle aplica respuestas, mantiene la sesión, recupera elecciones inciertas y procesa teclado. Las tareas HTTP producen respuestas pero nunca dibujan ni cambian el modelo por su cuenta. Separa el resultado que se está leyendo del estado remoto más reciente.

### Dependencias

[ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md) fija partida/nombre; [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md) envía; [EstadoCliente](archivos/src/cliente/EstadoCliente.md) valida y encola historia nueva; [EntradaCliente](archivos/src/cliente/EntradaCliente.md) lee; [InterfazConsola.Red](archivos/src/cliente/InterfazConsola.Red.md) dibuja. [ContratosPartida](archivos/src/cliente/ContratosPartida.md) contiene DTO. TimeProvider sustituible permite pruebas de intervalos; JsonSerializer obtiene huellas; Volatile lee versión de pantalla. Véanse [protocolo](protocolo-de-partida.md) y [diseño de interfaz](diseno-de-interfaz.md).

### Estructura

`internal sealed class SesionPartidaCliente(ConfiguracionCliente configuracion, ClientePartidaHttp http, InterfazConsola interfaz, TimeProvider? reloj = null)` captura colaboradores, sin guardas explícitas. No hereda explícitamente ni declara interfaces, atributos, partial, eventos u operadores. La entrada pública efectiva del tipo es `internal async Task<bool> Ejecutar`; los demás ayudantes son privados.

### Estado y datos

| Campos/propiedad privados | Inicialización, significado y cambios |
|---|---|
| readonly `TimeProvider tiempo` | reloj o TimeProvider.System; todas las fechas/reintentos y delay del bucle usan este reloj. |
| readonly `EstadoCliente estado` | nueva instancia fijada a configuracion.Partida; único dueño de su aplicación y lectura de cola. |
| bool `incorporado` | false; true solo tras confirmar sesión; vuelve false al vencer o reintentar explícitamente. |
| bool `terminal` | false; true tras vista finalizada válida, bloquea nuevos envíos, permite leer últimos resultados. |
| bool `protocolo`, `corregir` | false; protocolo detiene consultas/escrituras hasta decisión del usuario; corregir hace salir hacia la configuración de Program. |
| bool `abandono`, `salidaFinal` | false; abandono solicita PUT cuando no haya elección incierta; salidaFinal evita mostrar salida local tras Enter final. |
| string `aviso` | `Conectando…`; mensajes de red, recepción, error, cierre. |
| string? `huella`, `seleccion` | null; huella JSON evita repintado idéntico; seleccion JSON identifica revisión/solicitud/pausa que autorizan el foco actual. |
| int `foco`, `carta`, `menu=-1` | foco/carta cero, menú cerrado; carta guarda ID tras elegir, foco índice de opciones y menú índice de acción. |
| bool `atributo`, `confirmarAbandono` | false; paso de selección atributo y confirmación No/Sí. |
| `EleccionCliente? pendiente` | null; cuerpo inmutable con UUID único guardado antes de primer POST y conservado en pérdida incierta. |
| bool `reciboAusente` | false; true permite enviar cuerpo si vista vigente; false exige consultar recibo antes de reenviar. |
| `ResultadoCliente? leyendo` | null; resultado en pantalla hasta Enter, independiente de revisiones nuevas. |
| DateTimeOffset `proximaVista`, `proximaPresencia`, `proximaEscritura` | default; instantes a partir de los que cada canal puede comenzar otra solicitud. |
| int `fallosVista`, `fallosPresencia`, `fallosEscritura` | cero; escalonan reintentos independientes, se reinician tras éxito de su canal. |
| `Task<PresentacionCliente>? consulta`, `Task? presencia`, `Task<object?>? escritura` | null; como máximo una operación en vuelo por canal. |
| string `claseEscritura` | vacío; sesion/eleccion/recibo/abandono identifica la respuesta serializada. |
| int `revisionMinima` | cero; suelo confirmado por recibo/abandono para descartar vistas anteriores. |
| int `generacion`, `generacionVista`, `generacionPresencia` | cero; cada recuperación/reintento invalida respuestas antiguas de consultas/presencia. |
| bool `descartarEntrada` | false; solicita vaciar sobrante físico tras cambiar contexto; no consume líneas redirigidas. |
| bool `corregible`, `presenciaBloqueada` | false; permite volver a datos o detiene renovación ante error definitivo. |
| int `versionPantalla` | cero; cambia al cambiar solicitud/lectura/elección y etiqueta eventos contra confirmaciones viejas. |
| `DateTimeOffset Ahora` | getter lógico `tiempo.GetUtcNow()`; sin setter. |

No hay persistencia local. La identidad HTTP la posee el transporte; estado de esta instancia se descarta cuando Program permite corregir configuración.

### Miembros y ejecución

#### Reloj y bucle principal

`private TimeSpan Espera(ref int fallos)` incrementa el contador por referencia y retorna 1 segundo para el primero, 2 para segundo y 5 para tercero y siguientes. No cambia otros canales.

`internal async Task<bool> Ejecutar(EntradaCliente entrada, CancellationToken ct)` crea una fuente enlazada a ct y una tarea opcional de evento `(ConsoleKey? Tecla,int Version)`. Dibuja una vez. Mientras no cancelación externa ni corregir:

1. Completa tareas terminadas en orden escritura, consulta, presencia; solo esos ayudantes aplican respuestas.
2. Si no terminal y ya incorporado, inicia consulta si no protocolo, no hay consulta en vuelo y llegó proximaVista; toma generación actual y deja próximo tick a +1 segundo. Independientemente, inicia presencia si no corregible ni bloqueada, no hay presencia y llegó su instante; próxima a +5 segundos. Una consulta lenta no bloquea presencia ni crea cola de ticks atrasados.
3. Si no protocolo, no escritura y llegó proximaEscritura: primero incorporación si falta sesión. Si hay elección pendiente y no reciboAusente, GET recibo. Si reciboAusente, requiere estado actual: pausa conserva operación sin enviar, solicitud no Vigente descarta pendiente y avisa, vigente envía mismo cuerpo. Solo si no hay elección pendiente considera abandono, y requiere estado actual.
4. Mantiene una sola LeerEvento en vuelo. EOF deja aviso y rompe. Procesa tecla si menú abierto, resultado en lectura, Escape, o versión del evento igual a pantalla. Si Procesar retorna true sale. Evento de otra pantalla se descarta para no confirmar opciones obsoletas.
5. Si descartarEntrada llama al vaciado físico y baja flag. Espera 25 ms mediante TimeProvider/token y repite.

Fragmento de serialización de escrituras: la incorporación precede al pendiente y el abandono lo sigue:

```csharp
                        if (!incorporado) IniciarEscritura("sesion", async () => await http.Incorporar(token));
                        else if (pendiente is not null)
                        {
                            // Tras una respuesta perdida se busca el recibo antes de considerar cualquier reenvío.
                            if (!reciboAusente) IniciarEscritura("recibo", async () => await http.Recibo(pendiente.IdOperacion, token));
                            else if (estado.Actual && estado.Vista is { } v)
                            {
                                // La pausa conserva la operación; una solicitud distinta impide reutilizarla.
                                if (v.Pausada) { /* La operación se conserva hasta reanudación. */ }
                                else if (!Vigente(pendiente, v)) { pendiente = null; reciboAusente = false; aviso = "La solicitud cambió; no se reenvió la elección."; Dibujar(); }
                                else IniciarEscritura("eleccion", async () => await http.Elegir(pendiente, token));
                            }
                        }
                        else if (abandono && estado.Actual)
                            IniciarEscritura("abandono", async () => { await http.Abandonar(token); return null; });
```

Las lambdas de IniciarEscritura adaptan Task del DTO a Task<object?>; abandono espera PUT y retorna null. No mutan interfaz. Si cancelación del token enlazado ocurre se captura. Finally cancela y espera consulta/presencia/escritura/tecla pendientes una a una, ignorando únicamente FalloCliente, OperationCanceledException e IOException de esa limpieza. Tras finalizar, salvo corrección o salidaFinal, muestra salida local: si abandono aún true advierte que no está confirmado; si no, partida guardada y plaza posiblemente ocupada quince segundos o hasta cierre del operador. Retorna corregir para el do/while de Program.

#### Escrituras y recuperación idempotente

`private void IniciarEscritura(string clase, Func<Task<object?>> iniciar)` fija claseEscritura, pone aviso de envío si clase eleccion y guarda tarea devuelta por iniciar. Lo llama solo el bucle cuando escritura es null; no hay escritura paralela.

`private async Task CompletarEscritura()` toma tarea y pone escritura=null antes de await. Éxito pone contador fallosEscritura=0 y proximaEscritura=Ahora. Switch:

- sesion: convierte a SesionConfirmada y exige IdSesion==http.IdSesion y rol J1/J2. Si válido incorporado=true, aviso con plaza, consulta inmediata y presencia +5s. No valida VenceUtc aquí.
- eleccion o recibo: convierte ReciboCliente; exige pendiente no null, ambos UUID iguales y RevisionConfirmada>RevisionEsperada. Sube revisionMinima al máximo, borra pendiente y reciboAusente, marca estado no actual, consulta inmediata y aviso de recepción. Un recibo no revela el próximo turno ni habilita otra selección.
- abandono: revisionMinima sube a vista.Revision+1 si no terminal (o misma si ya terminal), abandono=false, estado no actual, aviso confirmado y consulta inmediata. Solo una vista posterior define motivo final.

Fragmento real de aceptación del recibo:

```csharp
                    var r = (ReciboCliente)respuesta!;
                    if (pendiente is null || r.IdOperacion != pendiente.IdOperacion || r.IdSolicitud != pendiente.IdSolicitud || r.RevisionConfirmada <= pendiente.RevisionEsperada)
                        throw new FalloCliente("protocoloIncompatible");
                    // Se rechazan vistas anteriores al recibo para no reactivar una solicitud ya consumida.
                    revisionMinima = Math.Max(revisionMinima, r.RevisionConfirmada);
                    pendiente = null;
                    reciboAusente = false;
                    estado.PerderComunicacion(); // Un recibo no es una presentación del turno siguiente.
                    proximaVista = Ahora;
                    aviso = "Recepción confirmada. Esperando presentación del servidor.";
```

Catch FalloCliente se divide: recibo con recursoNoEncontrado→reciboAusente=true, estado no actual y nueva vista inmediata; eleccion rechazada con entradaInvalida/eleccionInvalida/revisionDesactualizada/solicitudNoVigente/operacionIncompatible→borra operación, no reutiliza UUID, informa rechazo, invalida estado y refresca; resto→si falló eleccion marca reciboAusente=false (podría haberse aplicado), llama Tratar y programa proximaEscritura con backoff. Siempre Dibujar al terminar; no inventa aceptación.

#### Consultas, presencia y clasificación de fallos

`private async Task CompletarConsulta()` toma/borra tarea, espera vista; ignora respuesta si generación ya cambió o terminal. Revisión inferior a revisionMinima o a última Vista se descarta y programa otra consulta inmediata. Aplica EstadoCliente (puede rechazar semántica). Reinicia fallosVista; si ClaveSeleccion cambió, actualiza clave, foco=0, atributo=false, descartarEntrada=true y versionPantalla++. Si no había leyendo y hay resultados nuevos, extrae uno y cambia versión/descarta sobrante. Vista finalizada pone terminal=true, abandono=false y aviso final; si no final ni pendiente ni abandono, aviso actualizado. Dibuja. FalloCliente de consulta vieja/final se ignora; resto Tratar, proximaVista con backoff y dibuja. El resultado leído no se reemplaza por una vista más nueva.

`private async Task CompletarPresencia()` toma/borra tarea; éxito reinicia contador. Ante FalloCliente ignora respuesta vieja o terminal; resto Tratar. Si no transitorio y código no sesionVencida, bloquea futuras presencias; programa reintento de presencia y dibuja. El éxito de presencia no aplica una presentación ni actualiza Actual.

`private void Tratar(FalloCliente error)` marca estado no actual, seleccion=null y atributo=false; transitorio muestra reconexión con último estado. Luego distingue códigos:

- sesionVencida: incorporado=false, generacion++, reciboAusente=false; conserva pendiente e identidad, avisa reincorporación.
- entradaInvalida/credencialInvalida/sinPermiso/recursoNoEncontrado: protocolo=true, corregible=true; Enter vuelve a configuración, Escape permite menú; advierte que una elección previa puede haberse aplicado y exige datos originales para recuperarla.
- protocoloIncompatible: protocolo=true, Enter reintenta explícitamente; no aplica vista parcial.
- sesionOcupada: aviso de esperar liberación o quince segundos, sin marcar corregible.
- otros códigos conservan su aviso; el planificador define el siguiente intento. No convierte pérdida de comunicación en pausa confirmada.

#### Contexto de entrada y vigencia

`private async Task<(ConsoleKey? Tecla,int Version)> LeerEvento(EntradaCliente entrada, CancellationToken ct)` espera entrada y lee versionPantalla con Volatile.Read al llegar la tecla, no al comenzar la espera. Esto permite usar el contexto que ya estaba visible cuando se tecleó.

`private static bool Vigente(EleccionCliente e, PresentacionCliente v)` exige revisión idéntica, solicitud presente con mismo UUID, carta en IDs y atributo compatible: tipo carta exige e.Atributo null; otro tipo exige pertenencia al array permitido. No prueba pausa aquí: el llamador la comprueba antes de reenviar.

```csharp
    private static bool Vigente(EleccionCliente e, PresentacionCliente v) =>
        v.Revision == e.RevisionEsperada && v.Solicitud is { } s && s.IdSolicitud == e.IdSolicitud &&
        s.IdsPersonajePermitidos.Contains(e.IdPersonaje) && (s.Tipo == "carta" ? e.Atributo is null : s.AtributosPermitidos.Contains(e.Atributo));
```

`private static string ClaveSeleccion(PresentacionCliente v)` serializa objeto anónimo con Revision, Solicitud y Pausada. Cambios de nombres o presencia que no cambian esos datos no reinician foco. Es una huella de contenido, no igualdad de referencia de records.

#### Procesar teclado

`private bool Procesar(ConsoleKey tecla)` retorna true solo para salir localmente o terminar lectura del final; false sigue bucle. Orden de ramas:

1. Escape alterna menu entre 0 y −1, borra confirmación, dibuja forzado y retorna. Puede abrir menú aun en red perdida o envío pendiente.
2. Calcula paso −1 con Up/Left/W/A/H/K, +1 con Down/Right/S/D/J/L; resto0.
3. Menú abierto: paso clamp a [0,1] en confirmación o [0,2] normal. Enter confirmando: Sí y no terminal marca abandono=true y aviso; No no escribe; ambos cierran confirmación/menú. Menú normal: índice1 retorna true (salir sin abandonar); índice2 y no terminal abre confirmación con menu0 (No); resto cierra menú. Dibuja y retorna false. El flag abandono no ejecuta PUT en esta función.
4. Enter con protocolo: si corregible pone corregir=true y retorna false, saldrá bucle. Si no, quita protocolo/bloqueo de presencia, incorporado=false, generacion++, reciboAusente=false, escritura inmediata, dibuja y sigue; obliga comprobar sesión/operación.
5. Si leyendo: Enter pasa al siguiente resultado o null, descarta sobrante y cambia versión, dibuja; cualquier tecla restante no elige nada y retorna false.
6. Si terminal: salidaFinal=true solo para Enter y retorna ese bool. No crea más tráfico.
7. Si estado no actual, pendiente, abandono o escritura en vuelo: Enter solo repinta, resto ignora. Igual si vista pausada o no tiene solicitud.
8. Si clave de selección desactualizada: restablece clave/foco/paso atributo, dibuja y retorna sin confirmar con esa tecla.
9. Paso cambia foco con Clamp según longitud de atributos o IDs. Enter en carta guarda ID; si cartaYAtributo pasa a atributos y foco0 sin crear operación todavía. Si ya atributo o solo carta, crea EleccionCliente con nuevo UUID, UUID solicitud, revisión, carta y atributo o null; si no Vigente no hace nada. Si válido guarda pendiente ANTES del envío, reciboAusente=true, resetea selección, cambia versión/descarta sobrante y dibuja.

Fragmento de creación única de operación, después de completar la selección:

```csharp
            var e = new EleccionCliente(Guid.NewGuid(), s.IdSolicitud, v.Revision, carta,
                atributo ? s.AtributosPermitidos[foco] : null);
            if (!Vigente(e, v)) return false;
            pendiente = e; reciboAusente = true; atributo = false; foco = 0;
            aviso = "Envío en curso"; descartarEntrada = true; versionPantalla++;
            Dibujar(true);
        }
        return false;
```

#### Dibujo

`private void Dibujar(bool forzar = false)` serializa vista, Actual, aviso, menu, confirmarAbandono, leyendo, foco, atributo, pendiente y terminal. Si no forzar y huella igual retorna; si cambia o se fuerza guarda huella y llama MostrarRed con los flags correspondientes. Evita repintados periódicos idénticos y no reinicia foco. El flag abandono se pasa al renderizador; no figura directamente en la huella, aunque sus cambios normales también alteran aviso/menu/pendiente.

### Recorridos y ejemplos

Normal: incorporación confirma sesión S/J1. Consulta revisión4 con solicitud Q permite carta7 y Fuerza; Enter carta, Enter atributo crean operación O con revisión4. POST devuelve recibo O/Q/revisión5: borra pendiente, bloquea elección y consulta. Vista5 con resultado nuevo se valida y se encola. Enter leyendo solo cierra ese resultado; otro Enter posterior puede empezar una elección nueva si el servidor la ofrece.

Alternativo: POST se aplicó pero respuesta se pierde. pendiente conserva O y cuerpo; tras backoff consulta recibo. Si existe, acepta revisión confirmada sin reenviar; si 404, refresca vista y solo reenvía exactamente el cuerpo original cuando revisión/Q/carta/atributo coinciden. Si está pausada conserva pendiente; si solicitud cambió lo descarta y no envía sobre otra. Un abandono solicitado durante esta recuperación espera detrás de ella. Respuesta tardía de presencia de una generación anterior no invalida la sesión reincorporada.

### Errores, límites y efectos observables

Puede cambiar sesión y partida remotamente por HTTP; perder respuesta no prueba ausencia de efectos persistidos. La serialización evita dos escrituras simultáneas, pero consulta y presencia son independientes. No hay escritura automática de abandono por EOF/Ctrl+C; puede quedar plaza ocupada. No reconcilia operaciones de otro proceso después de cerrar porque no guarda UUID/cuerpo en disco. Una vista final detiene tráfico nuevo, no borra tareas ya enviadas; se esperan al salir. Los fallos distintos a los previstos no se capturan como si fueran errores de red.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### Espera: incremento y escalones

El incremento por referencia ocurre antes del switch; el tercer fallo y todos los posteriores usan cinco segundos.

```csharp
    private DateTimeOffset Ahora => tiempo.GetUtcNow();
    private TimeSpan Espera(ref int fallos) => TimeSpan.FromSeconds(++fallos switch { 1 => 1, 2 => 2, _ => 5 });
```

#### Ejecutar: planificación independiente

Este inicio del método procesa respuestas antes de programar consultas/presencia, fija sus generaciones y limita cada canal a una tarea. El resto de serialización de escrituras ya se mostró arriba.

```csharp
    internal async Task<bool> Ejecutar(EntradaCliente entrada, CancellationToken ct)
    {
        using var cierre = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = cierre.Token;
        // La versión de pantalla permite descartar confirmaciones capturadas para opciones anteriores.
        Task<(ConsoleKey? Tecla, int Version)>? tecla = null;
        try
        {
            Dibujar();
            while (!ct.IsCancellationRequested && !corregir)
            {
                // Se incorporan las respuestas antes de procesar entrada o programar nuevos envíos.
                if (escritura?.IsCompleted == true) await CompletarEscritura();
                if (consulta?.IsCompleted == true) await CompletarConsulta();
                if (presencia?.IsCompleted == true) await CompletarPresencia();
                if (!terminal)
                {
                    if (incorporado)
                    {
                        // Consulta y presencia avanzan por separado, incluso mientras se lee o se navega.
                        if (!protocolo && consulta is null && Ahora >= proximaVista)
                        {
                            generacionVista = generacion;
                            consulta = http.Presentacion(token);
                            proximaVista = Ahora.AddSeconds(1);
                        }
                        // Una consulta lenta no retiene la renovación de presencia ni acumula ticks atrasados.
                        if (!corregible && !presenciaBloqueada && presencia is null && Ahora >= proximaPresencia)
                        {
                            generacionPresencia = generacion;
                            presencia = http.Renovar(token);
                            proximaPresencia = Ahora.AddSeconds(5);
                        }
                    }
```

#### Ejecutar: eventos y limpieza

Tras programar la red, solo un evento queda en espera. La versión limita confirmaciones antiguas, mientras menú/resultados/Escape siguen accesibles. El finally cancela y espera todas las tareas antes del retorno y del mensaje de salida.

```csharp
                // Menú y resultados admiten interacción mientras la red actualiza el modelo de fondo.
                tecla ??= LeerEvento(entrada, token);
                if (tecla.IsCompleted)
                {
                    var evento = await tecla;
                    tecla = null;
                    if (evento.Tecla is null) { aviso = "Fin de la entrada."; break; }
                    if ((menu >= 0 || leyendo is not null || evento.Tecla == ConsoleKey.Escape || evento.Version == versionPantalla) && Procesar(evento.Tecla.Value)) break;
                }
                // Al cambiar de pantalla solo se descarta el sobrante interactivo, nunca las líneas futuras.
                if (descartarEntrada) { entrada.DescartarSobrante(); descartarEntrada = false; }
                await Task.Delay(TimeSpan.FromMilliseconds(25), tiempo, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally
        {
            // La salida cancela y espera las tareas antes de desechar sus recursos.
            cierre.Cancel();
            var tareas = new Task?[] { consulta, presencia, escritura, tecla }.Where(t => t is not null).Cast<Task>();
            foreach (var tarea in tareas)
                try { await tarea; } catch (Exception ex) when (ex is FalloCliente or OperationCanceledException or IOException) { }
        }
        // Salir localmente no confirma un abandono pendiente ni libera inmediatamente la plaza.
        if (!corregir && !salidaFinal)
            interfaz.MostrarInterrupcion(configuracion.Nombre, "Salida local", abandono
                ? "Abandono sin confirmar. La plaza puede seguir ocupada hasta quince segundos."
                : "La partida queda guardada. La plaza puede seguir ocupada hasta quince segundos o hasta que el operador cierre la sesión.", false);
        return corregir;
    }
```

#### IniciarEscritura: dueño único del canal

Registra clase y tarea; el planificador garantiza que no exista otra escritura en vuelo.

```csharp
    private void IniciarEscritura(string clase, Func<Task<object?>> iniciar)
    {
        claseEscritura = clase;
        if (clase == "eleccion") aviso = "Envío en curso";
        escritura = iniciar();
    }
```

#### CompletarEscritura: errores y reintentos

Este catch sigue al switch de éxito descrito arriba. El 404 de recibo exige refrescar vista, el rechazo definitivo descarta operación y los fallos inciertos fuerzan recuperar recibo antes de repetir.

```csharp
        catch (FalloCliente error)
        {
            // Un 404 de recibo significa ausencia de confirmación; exige refrescar la vista antes de reenviar.
            if (claseEscritura == "recibo" && error.Codigo == "recursoNoEncontrado")
            {
                reciboAusente = true;
                estado.PerderComunicacion();
                proximaVista = Ahora;
            }
            // Los rechazos definitivos permiten corregir con otra operación, sin reutilizar el UUID rechazado.
            else if (claseEscritura == "eleccion" && error.Codigo is "entradaInvalida" or "eleccionInvalida" or "revisionDesactualizada" or "solicitudNoVigente" or "operacionIncompatible")
            {
                pendiente = null;
                reciboAusente = false;
                aviso = $"Elección rechazada: {error.Codigo}. Actualizando opciones.";
                estado.PerderComunicacion();
                proximaVista = Ahora;
            }
            else
            {
                // Un fallo incierto obliga a volver al recibo; no se asume que el POST quedó sin aplicar.
                if (claseEscritura == "eleccion") reciboAusente = false;
                Tratar(error);
                proximaEscritura = Ahora + Espera(ref fallosEscritura);
            }
        }
        Dibujar();
    }
```

#### CompletarConsulta: generaciones y cola

El método entero muestra la precedencia de descartar respuestas viejas, validar, cambiar contexto de selección y conservar la lectura actual. Los errores programan backoff sin aplicar vistas parciales.

```csharp
    private async Task CompletarConsulta()
    {
        var tarea = consulta!;
        consulta = null;
        try
        {
            var vista = await tarea;
            // Una recuperación invalida respuestas antiguas; una revisión menor tampoco puede retroceder el estado.
            if (generacionVista != generacion || terminal) return;
            if (vista.Revision < revisionMinima || vista.Revision < (estado.Vista?.Revision ?? 0))
            { proximaVista = Ahora; return; }
            estado.Aplicar(vista);
            fallosVista = 0;
            if (seleccion != ClaveSeleccion(vista))
            {
                seleccion = ClaveSeleccion(vista); foco = 0; atributo = false; descartarEntrada = true; versionPantalla++;
            }
            // Un resultado permanece visible hasta Enter, aunque sigan llegando vistas más nuevas.
            if (leyendo is null && estado.Resultados.Count > 0)
            { leyendo = estado.Resultados.Dequeue(); descartarEntrada = true; versionPantalla++; }
            // El cierre detiene nuevas solicitudes, pero respeta la lectura de los últimos resultados.
            if (vista.Fase == "finalizada")
            {
                terminal = true;
                abandono = false;
                aviso = "Final confirmado por el servidor";
            }
            else if (pendiente is null && !abandono) aviso = "Estado actualizado";
            Dibujar();
        }
        catch (FalloCliente error)
        {
            if (generacionVista != generacion || terminal) return;
            Tratar(error);
            proximaVista = Ahora + Espera(ref fallosVista);
            Dibujar();
        }
    }
```

#### CompletarPresencia: aislamiento de respuestas antiguas

Un fallo antiguo no toca la sesión nueva; el éxito reinicia solo contador de presencia. El bloqueo definitivo evita renovación inútil hasta decisión explícita.

```csharp
    private async Task CompletarPresencia()
    {
        var tarea = presencia!;
        presencia = null;
        try { await tarea; fallosPresencia = 0; }
        catch (FalloCliente error)
        {
            if (generacionPresencia != generacion || terminal) return;
            Tratar(error);
            if (!error.Transitorio && error.Codigo != "sesionVencida") presenciaBloqueada = true;
            proximaPresencia = Ahora + Espera(ref fallosPresencia);
            Dibujar();
        }
    }
```

#### Tratar: clasificación y reincorporación

Cada rama invalida vigencia de pantalla antes de decidir recuperación, corrección de identidad o reintento explícito. El cambio de generación conserva la elección pendiente.

```csharp
    private void Tratar(FalloCliente error)
    {
        estado.PerderComunicacion();
        seleccion = null;
        atributo = false;
        aviso = error.Transitorio ? "Sin comunicación · Último estado confirmado. Reconectando…" : error.Codigo;
        // La reincorporación conserva identidad y operación, pero invalida las respuestas que quedaron en vuelo.
        if (error.Codigo == "sesionVencida")
        {
            incorporado = false; generacion++; reciboAusente = false;
            aviso = "Sesión vencida. Reincorporando con la misma identidad…";
        }
        // Identidad o partida incorrectas requieren corregir datos: no se reintenta indefinidamente lo mismo.
        else if (error.Codigo is "entradaInvalida" or "credencialInvalida" or "sinPermiso" or "recursoNoEncontrado")
        {
            protocolo = true;
            aviso += " · Enter para corregir los datos o Esc para salir. Una elección anterior puede haberse aplicado; recuperar requiere sus datos originales.";
            corregible = true;
        }
        // Ante un protocolo desconocido el usuario decide reintentar o salir antes de continuar.
        else if (error.Codigo == "protocoloIncompatible")
        {
            protocolo = true;
            aviso = "Respuesta incompatible o inconsistente · Enter para reintentar o Esc para salir.";
        }
        else if (error.Codigo == "sesionOcupada")
            aviso = "Sesión ocupada. Esperando liberación o vencimiento (hasta quince segundos). Esc para salir.";
    }
```

#### LeerEvento y ClaveSeleccion: contexto de confirmación

LeerEvento etiqueta al llegar; Vigente comprueba contexto completo; la clave JSON incluye pausa además de revisión y solicitud.

```csharp
    private async Task<(ConsoleKey? Tecla, int Version)> LeerEvento(EntradaCliente entrada, CancellationToken ct)
    {
        var tecla = await entrada.Leer(ct);
        return (tecla, Volatile.Read(ref versionPantalla));
    }
    // Antes del POST deben coincidir revisión, solicitud, carta y atributo de la operación original.
    private static bool Vigente(EleccionCliente e, PresentacionCliente v) =>
        v.Revision == e.RevisionEsperada && v.Solicitud is { } s && s.IdSolicitud == e.IdSolicitud &&
        s.IdsPersonajePermitidos.Contains(e.IdPersonaje) && (s.Tipo == "carta" ? e.Atributo is null : s.AtributosPermitidos.Contains(e.Atributo));
    private static string ClaveSeleccion(PresentacionCliente v) => JsonSerializer.Serialize(new { v.Revision, v.Solicitud, v.Pausada });
```

#### Procesar: Escape, menú y reintento

Este prefijo del método permite salida y confirmación No/Sí aun durante errores. Abandono solo pone un flag; el bucle lo envía después del pendiente. El reintento de protocolo crea otra generación.

```csharp
    private bool Procesar(ConsoleKey tecla)
    {
        // Esc siempre puede abrir o cerrar el menú, aun durante reconexión o envío pendiente.
        if (tecla == ConsoleKey.Escape)
        {
            menu = menu < 0 ? 0 : -1;
            confirmarAbandono = false;
            Dibujar(true);
            return false;
        }
        // Flechas, WASD y HJKL comparten navegación; el foco se limita a los extremos de la lista.
        int paso = tecla switch
        {
            ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.H or ConsoleKey.K => -1,
            ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L => 1,
            _ => 0
        };
        if (menu >= 0)
        {
            if (paso != 0) menu = Math.Clamp(menu + paso, 0, confirmarAbandono ? 1 : 2);
            if (tecla == ConsoleKey.Enter)
            {
                // Elegir No no escribe en el servidor; elegir Sí solo deja el abandono listo para el coordinador.
                if (confirmarAbandono)
                {
                    if (menu == 1 && !terminal) { abandono = true; aviso = "Abandono pendiente de confirmación del servidor."; }
                    confirmarAbandono = false; menu = -1;
                }
                else if (menu == 1) return true;
                else if (menu == 2 && !terminal) { confirmarAbandono = true; menu = 0; }
                else menu = -1;
            }
            Dibujar(true);
            return false;
        }
        // Un reintento explícito crea otra generación y vuelve a comprobar la sesión y la operación incierta.
        if (tecla == ConsoleKey.Enter && protocolo)
        {
            if (corregible) { corregir = true; return false; }
            protocolo = false; presenciaBloqueada = false; incorporado = false; generacion++; reciboAusente = false; proximaEscritura = Ahora;
            Dibujar(true); return false;
        }
        // Enter retira únicamente el resultado leído y nunca confirma una jugada del turno siguiente.
```

#### Procesar: lectura, terminal y selección

La rama de lectura retorna antes de las elecciones; un Enter no puede a la vez cerrar resultado y jugar. Las guardas bloquean vista vieja/pausada y operaciones pendientes antes de acceder a opciones. La creación final de operación ya se mostró arriba.

```csharp
        if (leyendo is not null)
        {
            if (tecla == ConsoleKey.Enter)
            {
                leyendo = estado.Resultados.Count > 0 ? estado.Resultados.Dequeue() : null;
                descartarEntrada = true; versionPantalla++;
                Dibujar(true);
            }
            return false;
        }
        // La pantalla final solo admite salir; los estados no actuales o pendientes bloquean cualquier jugada.
        if (terminal)
        {
            salidaFinal = tecla == ConsoleKey.Enter;
            return salidaFinal;
        }
        if (!estado.Actual || pendiente is not null || abandono || escritura is not null)
        { if (tecla == ConsoleKey.Enter) Dibujar(true); return false; }
        var v = estado.Vista!;
        if (v.Pausada || v.Solicitud is not { } s)
        { if (tecla == ConsoleKey.Enter) Dibujar(true); return false; }
        // Si cambió el contexto de selección, se restablece el foco y se exige una confirmación nueva.
        if (seleccion != ClaveSeleccion(v)) { seleccion = ClaveSeleccion(v); foco = 0; atributo = false; Dibujar(true); return false; }
        if (paso != 0)
        {
            foco = Math.Clamp(foco + paso, 0, (atributo ? s.AtributosPermitidos.Length : s.IdsPersonajePermitidos.Length) - 1);
            Dibujar(true);
        }
        if (tecla == ConsoleKey.Enter)
        {
            // Abrir requiere carta y atributo; responder usa solo la carta publicada en la solicitud.
            if (!atributo)
            {
                carta = s.IdsPersonajePermitidos[foco];
                if (s.Tipo == "cartaYAtributo") { atributo = true; foco = 0; Dibujar(true); return false; }
            }
```

#### Dibujar: huella del estado visible

Una huella igual evita repintado; la orden forzada de navegación permite dibujar incluso sin cambios de red. No modifica el estado remoto.

```csharp
    private void Dibujar(bool forzar = false)
    {
        var v = estado.Vista;
        string nueva = JsonSerializer.Serialize(new { v, estado.Actual, aviso, menu, confirmarAbandono, leyendo, foco, atributo, pendiente, terminal });
        if (!forzar && huella == nueva) return;
        huella = nueva;
        interfaz.MostrarRed(configuracion.Nombre, v, estado.Actual, aviso, leyendo, menu, confirmarAbandono,
            foco, atributo, pendiente is not null, abandono);
    }
```

#### CompletarEscritura: incorporación y abandono

El éxito de incorporación confirma UUID/rol y fija próximos ticks. La rama abandono eleva el suelo de revisión y exige otra presentación antes de mostrar el motivo final; la rama de recibo, incluida aquí como contexto, ya se explicó arriba.

```csharp
    private async Task CompletarEscritura()
    {
        var tarea = escritura!;
        escritura = null;
        try
        {
            // La respuesta se aplica dentro del coordinador, que es el único dueño del estado.
            object? respuesta = await tarea;
            fallosEscritura = 0;
            proximaEscritura = Ahora;
            switch (claseEscritura)
            {
                case "sesion":
                    var s = (SesionConfirmada)respuesta!;
                    // Una incorporación solo es válida si confirma la sesión solicitada y un rol conocido.
                    if (s.IdSesion != http.IdSesion || s.Rol is not ("J1" or "J2")) throw new FalloCliente("protocoloIncompatible");
                    incorporado = true;
                    aviso = $"{configuracion.Nombre} · Plaza confirmada: {s.Rol}";
                    proximaVista = Ahora;
                    proximaPresencia = Ahora.AddSeconds(5);
                    break;
                case "eleccion":
                case "recibo":
                    // Un recibo ajeno o incoherente no permite retirar la operación pendiente.
                    var r = (ReciboCliente)respuesta!;
                    if (pendiente is null || r.IdOperacion != pendiente.IdOperacion || r.IdSolicitud != pendiente.IdSolicitud || r.RevisionConfirmada <= pendiente.RevisionEsperada)
                        throw new FalloCliente("protocoloIncompatible");
                    // Se rechazan vistas anteriores al recibo para no reactivar una solicitud ya consumida.
                    revisionMinima = Math.Max(revisionMinima, r.RevisionConfirmada);
                    pendiente = null;
                    reciboAusente = false;
                    estado.PerderComunicacion(); // Un recibo no es una presentación del turno siguiente.
                    proximaVista = Ahora;
                    aviso = "Recepción confirmada. Esperando presentación del servidor.";
                    break;
                // El éxito confirma el abandono; todavía se consulta el motivo final y se excluyen vistas viejas.
                case "abandono":
                    revisionMinima = Math.Max(revisionMinima, (estado.Vista?.Revision ?? 0) + (terminal ? 0 : 1));
                    abandono = false;
                    estado.PerderComunicacion();
                    aviso = "Abandono confirmado. Consultando motivo final.";
                    proximaVista = Ahora;
                    break;
            }
```

### Relaciones y verificación

[SesionClienteTests](archivos/test/proyecto-integrador-final-tests/SesionClienteTests.md) define consulta/presencia mientras menú/resultado, consulta lenta sin solapamiento, pérdida antes/después de aplicar POST, recibo/cuerpo original, Enter separado, confirmación No/Sí, EOF local, reincorporación estable, incompatibilidad explícita, pausa durante selección, rechazo/operación nueva, abandono incierto y serializado, foco estable, lectura final y respuesta tardía descartada. Usa transporte controlado/reloj sustituible; [IntegracionRespuestaPerdidaTests](archivos/test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.md) añade servidor/SQLite temporal. No acreditan por sí solos una partida entre dos equipos o todos los teclados. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/cliente/VistasConsola.cs</summary>

[Página individual](archivos/src/cliente/VistasConsola.md)

## src/cliente/VistasConsola.cs

Fuente: [src/cliente/VistasConsola.cs](../src/cliente/VistasConsola.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Son valores de presentación para la consola local y REST. Separan la información visible de las entidades del servidor. No calculan victoria ni aplican reglas; las pantallas consumen sus datos.

### Dependencias

`IReadOnlyDictionary<string,int>` ofrece lectura de atributos sin exponer una API de escritura por esa referencia, aunque el diccionario subyacente puede ser mutable. [InterfazConsola](archivos/src/cliente/InterfazConsola.md), [InterfazConsola.Red](archivos/src/cliente/InterfazConsola.Red.md), [DemostracionConsola](archivos/src/cliente/DemostracionConsola.md) y [ContratosPartida](archivos/src/cliente/ContratosPartida.md) usan estos tipos. Véase [diseño de interfaz](diseno-de-interfaz.md).

### Estructura, estado y miembros

Cinco `internal sealed record` sin herencia explícita, interfaces declaradas, atributos ni partial; un enum internal. No hay métodos con cuerpo ni eventos. Los constructores posicionales almacenan datos en propiedades init, con igualdad/copia/deconstrucción generadas por C#. No validan null, rangos, identidad ni consistencia.

| Tipo y constructor | Datos y ciclo de vida |
|---|---|
| `CartaVisible(int IdPersonaje, string Nombre, IReadOnlyDictionary<string,int> Atributos)` | ID de catálogo, nombre visible, mapa atributo→puntaje. Se comparte entre mano, opciones y resultados. El ID distingue cartas con mismo nombre. |
| `JugadorVisible(string Nombre, int RondasGanadas)` | Nombre mostrado y marcador de rondas; `with` crea un valor actualizado en demostración. |
| `CartaJugada(string Jugador, string Carta, int Valor)` | Nombre de participante, nombre de carta revelada y valor del atributo ya elegido. |
| `CombateVisible(string Atributo, CartaJugada J1, CartaJugada J2, string Ganador, int CombatesGanadosJ1, int CombatesGanadosJ2, string? ProximoEnElegir, bool? GanaJ1 = null)` | Duelo, ganador y marcador. ProximoEnElegir null omite el anuncio siguiente. GanaJ1 explícito evita ambigüedad con nombres iguales; si null, la interfaz compara nombre J1 con Ganador. |
| `MesaVisible(string Local, JugadorVisible J1, JugadorVisible J2, int Ronda, int Combate, int CombatesJ1, int CombatesJ2)` | Contexto local, marcador, coordenadas de ronda/combate y victorias parciales. |
| `EstadoEnvio` | `Enviando=0`, `Recibido=1`, `Pendiente=2` por orden implícito; distingue envío, recibo confirmado y recuperación incierta. No es flags. |

### Ejecución y fragmentos

El constructor de CombateVisible incluye una opción explícita de ganador:

```csharp
internal sealed record CombateVisible(
    string Atributo,
    CartaJugada J1,
    CartaJugada J2,
    string Ganador,
    int CombatesGanadosJ1,
    int CombatesGanadosJ2,
    string? ProximoEnElegir,
    bool? GanaJ1 = null);
```

No hay comparación de puntajes en este record. El campo nullable permite al adaptador REST pasar identidad por rol en vez de inferir por nombre. La distinción de envío se declara así:

```csharp
internal enum EstadoEnvio
{
    Enviando,
    Recibido,
    Pendiente
}
```

### Recorridos y ejemplos

Normal: una CartaVisible ID 7 llamada Vigía con Fuerza=650 se muestra en la mano. Al revelar, CartaJugada(Ana,Vigía,650) y CartaJugada(Beto,Oráculo,350) forman un CombateVisible de Fuerza con GanaJ1=true y marcador parcial 1/0. La pantalla resalta J1; no recalcula 650>350.

Alternativo: ambos participantes se llaman Ana. El adaptador pasa GanaJ1=false para resaltar J2 aunque Ganador también sea Ana. ProximoEnElegir=null no muestra turno siguiente. Construir un record con valores inconsistentes no lanza por sí mismo: su llamador debe validarlos.

### Errores, límites y efectos observables

No hay E/S, mutación posterior por setters ordinarios, transacciones ni cancelación. Propiedades init no vuelven inmutables los objetos referenciados. EstadoEnvio convertido desde un entero desconocido existe como valor enum, pero MostrarEnvio lo rechaza.

### Relaciones y verificación

[InterfazConsolaTests](archivos/test/proyecto-integrador-final-tests/InterfazConsolaTests.md), [PantallasConsolaTests](archivos/test/proyecto-integrador-final-tests/PantallasConsolaTests.md) y [PresentacionRedTests](archivos/test/proyecto-integrador-final-tests/PresentacionRedTests.md) usan estas vistas para verificar representación, marcadores y nombres. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/comun/EntradaConsola.cs</summary>

[Página individual](archivos/src/comun/EntradaConsola.md)

## src/comun/EntradaConsola.cs

Fuente: [src/comun/EntradaConsola.cs](../src/comun/EntradaConsola.cs). Proyecto: fuente enlazado por cliente, servidor y subir-personajes. Namespace: `Proyecto_Integrador_Final.Consola`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Lector compartido por las consolas administrativas del servidor y la carga. Centraliza preguntas, reintento por campo, URL sin prefijo y secreto oculto. Está enlazado en los tres csproj; el cliente REST usa su propio lector de configuración con políticas distintas.

### Dependencias

`TextReader`, `TextWriter`, `Console`, `StringBuilder`, `Uri`, LINQ `Any` y excepciones de System. Lo usan configuración del servidor y herramienta subir-personajes; [ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md) admite prefijos y no llama a este lector para su configuración. Véase [instalación y uso](instalacion-y-uso.md).

### Estructura y datos

`internal sealed class EntradaConsola(TextReader entrada, TextWriter salida)`, constructor primario captura ambos flujos sin validarlos ni asumir propiedad de ellos. Sin herencia explícita, interfaces, atributos, partial, campos declarados adicionales, propiedades, eventos ni liberación de recursos. Todo es síncrono; no acepta token.

### Miembros y ejecución

- `internal string Texto(string pregunta)`: imprime pregunta por WriteLine y lee una línea; conserva espacios, retorna string, null produce EndOfStreamException.
- `internal string Validar(string pregunta, Func<string,bool> valida, string error, string? defecto = null)`: bucle sin límite de intentos. Texto seguido de Trim; si queda vacío y defecto no null, sustituye por defecto (sin recortarlo de nuevo). Retorna al aceptar predicado; si rechaza imprime error y repite el campo. Excepciones del predicado no se capturan.
- `internal string Url(string defecto)`: usa Validar y pregunta con valor por defecto; Enter lo aplica. `private static bool EsUrl(string valor)` exige Uri absoluta HTTP/HTTPS, host no vacío, sin UserInfo/query/fragmento y AbsolutePath exactamente `/`. No acepta prefijos como `/juego/` y no intenta conexión.
- `internal string Secreto(string pregunta, bool permiteVacio)`: repite pregunta; si entrada es Console.In y stdin no redirigido usa LeerSecreto, si no ReadLine o EOF. Acepta blanco solo si permiteVacio; lo normaliza a `""`. Un secreto no blanco exige cero caracteres de control y cero whitespace, sin Trim. Rechazo imprime un mensaje fijo y repite.
- `private static string LeerSecreto()`: StringBuilder, ReadKey(intercept:true) sin eco. Enter imprime salto y retorna; Ctrl+D/Ctrl+Z lanzan EOF; Backspace borra el último carácter si existe; otros no controles se agregan. No muestra asteriscos. El ciclo termina solo por Enter o EOF; no es cancelable por token.

Fragmento de `Validar`, con sustitución antes del predicado:

```csharp
            string valor = Texto(pregunta).Trim();
            if (valor.Length == 0 && defecto is not null)
                valor = defecto;
            if (valida(valor))
                return valor;
            salida.WriteLine(error);
```

Fragmento de la aceptación de secretos, sin exponer su contenido:

```csharp
            if ((permiteVacio && string.IsNullOrWhiteSpace(valor)) ||
                (!string.IsNullOrWhiteSpace(valor) && !valor.Any(char.IsControl) && !valor.Any(char.IsWhiteSpace)))
                return string.IsNullOrWhiteSpace(valor) ? "" : valor;
            salida.WriteLine("Ingresá una credencial no vacía, sin espacios ni caracteres de control.");
```

### Recorridos y ejemplos

Normal: Url con defecto `http://localhost:5000`, línea vacía, retorna ese destino. Secreto con `plaza-demo` lo retorna sin mostrarlo en terminal. Texto devuelve literalmente una línea como `  crear  `; quien llame decide si recorta.

Alternativo: URL `http://localhost:5000/api` repite por ruta distinta de `/`. Secreto `dos palabras` se rechaza; blanco con permiteVacio=true retorna vacío y puede deshabilitar altas en el servidor. EOF interrumpe sin devolver un dato incompleto.

### Errores, límites y efectos observables

Imprime preguntas y errores, no realiza HTTP ni persiste. Los flujos pueden lanzar IOException. No limita longitud de secretos ni reintentos. El modo oculto solo se activa con el objeto Console.In exacto; con un lector envuelto no usa captura de teclas. No dispone los flujos ajenos.

### Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

#### Texto, Url y EsUrl

Texto conserva espacios; Validar los recorta y aplica defecto; EsUrl restringe camino raíz. Estos contratos no son idénticos a los del cliente REST.

```csharp
    internal string Texto(string pregunta)
    {
        salida.WriteLine(pregunta);
        return entrada.ReadLine() ?? throw new EndOfStreamException();
    }

    internal string Validar(string pregunta, Func<string, bool> valida, string error, string? defecto = null)
    {
        while (true)
        {
            string valor = Texto(pregunta).Trim();
            if (valor.Length == 0 && defecto is not null)
                valor = defecto;
            if (valida(valor))
                return valor;
            salida.WriteLine(error);
        }
    }

    internal string Url(string defecto) => Validar($"Dirección HTTP (Enter: {defecto})",
        EsUrl, "Ingresá una dirección HTTP o HTTPS válida, sin credenciales, consulta ni fragmento.", defecto);

    private static bool EsUrl(string valor) =>
        Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https" && !string.IsNullOrEmpty(uri.Host) &&
        uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0 &&
        uri.AbsolutePath == "/";

    internal string Secreto(string pregunta, bool permiteVacio)
```

#### LeerSecreto: terminal sin eco

ReadKey intercepta, Enter devuelve, controles EOF lanzan y Backspace modifica solo memoria. No imprime el secreto ni asteriscos.

```csharp
    private static string LeerSecreto()
    {
        var texto = new StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return texto.ToString();
            }
            if (tecla.KeyChar is '\u0004' or '\u001a')
                throw new EndOfStreamException();
            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (texto.Length > 0) texto.Length--;
            }
            else if (!char.IsControl(tecla.KeyChar))
                texto.Append(tecla.KeyChar);
        }
    }
```

### Relaciones y verificación

[ConfiguracionServidorTests](archivos/test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md), [ConsolaAltaTests](archivos/test/proyecto-integrador-final-tests/ConsolaAltaTests.md) y [EntradaProcesosContratoTests](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) ejercitan corrección, EOF y configuración. Los lectores en memoria no prueban por sí solos ocultación física de terminal. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.

</details>

<details>
<summary>src/servidor/Api/BaseJuego.cs</summary>

[Página individual](archivos/src/servidor/Api/BaseJuego.md)

## src/servidor/Api/BaseJuego.cs

Fuente: [src/servidor/Api/BaseJuego.cs](../src/servidor/Api/BaseJuego.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Define el contexto de Entity Framework Core y las entidades principales de partida/plaza. Centraliza nombres de tablas, claves y relaciones para que [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md) abra unidades de trabajo sobre SQLite. No autentica ni ejecuta turnos por sí mismo.

### Dependencias

`Microsoft.EntityFrameworkCore` aporta DbContext, DbSet, ModelBuilder y mapeo fluido. [PersonajeCatalogo](archivos/src/servidor/Api/PersonajeCatalogo.md) es la entidad de catálogo; [EstadoPartida](archivos/src/servidor/Api/EstadoPartida.md) declara cartas, rondas, combates, solicitudes y operaciones. [Persistencia](bases-de-datos.md) explica la separación entre catálogo y copias por partida.

### Estructura

`internal sealed class BaseJuego(DbContextOptions<BaseJuego> opciones) : DbContext(opciones)` usa constructor primario: pasa las opciones directamente a la base. Sobrescribe `protected override void OnModelCreating(ModelBuilder modelo)`; no declara partial. También declara `internal sealed class PartidaGuardada` y `internal sealed class ParticipanteGuardado`, sin bases ni interfaces explícitas. No hay eventos, operadores o funciones locales.

### Estado y datos

| Miembro de BaseJuego | Tipo, acceso y función |
|---|---|
| Catalogo | internal DbSet<PersonajeCatalogo>, getter `Set<T>()` |
| Partidas | internal DbSet<PartidaGuardada>, getter `Set<T>()` |
| Participantes | internal DbSet<ParticipanteGuardado>, getter `Set<T>()` |
| Cartas | internal DbSet<PersonajePartida>, getter `Set<T>()` |
| Rondas | internal DbSet<RondaGuardada>, getter `Set<T>()` |
| Combates | internal DbSet<CombateGuardado>, getter `Set<T>()` |
| Solicitudes | internal DbSet<SolicitudGuardada>, getter `Set<T>()` |
| Operaciones | internal DbSet<OperacionGuardada>, getter `Set<T>()` |

Estos accesores no ejecutan consultas ni guardan cambios; exponen conjuntos del contexto. El contexto se desecha por operación en el repositorio, que gobierna conexión y transacción.

`PartidaGuardada` tiene propiedades públicas get/set: `Guid Id` (clave por convención), `int Revision` (cero inicial); `int? NumeroRonda`, `NumeroCombate`; `Guid? IdSolicitud`; `string? MotivoFin`, `Ganador`, `AbandonoDe`, todos inicialmente null; `string Fase = "esperandoInicio"`. Los anulables distinguen preparación o ausencia de final de una partida activa; no hay guardas en sus setters.

`ParticipanteGuardado` tiene `Guid IdPartida`, `required string Rol`, `required byte[] Hash`, `string? Nombre` inicialmente null y `int RondasGanadas` inicialmente cero, todos públicos get/set. Hash almacena SHA-256, no la credencial. El array es mutable; la propiedad no lo copia. Rol junto a IdPartida forma la clave. `required` obliga inicialización de código, no garantiza valores J1/J2 en una asignación arbitraria.

### Miembros y ejecución

El constructor de BaseJuego no abre ni crea SQLite: lo hace el repositorio después. `OnModelCreating` se invoca por EF al construir el modelo. No retorna datos ni recorre partidas; declara este mapa:

| Entidad → tabla | Clave y restricciones declaradas aquí |
|---|---|
| PersonajeCatalogo → PersonajeCatalogo | Id no generado (`ValueGeneratedNever`) |
| PartidaGuardada → Partida | Id por convención |
| ParticipanteGuardado → Participante | (IdPartida,Rol); FK a Partida por IdPartida |
| PersonajePartida → PersonajePartida | (IdPartida,IdPersonaje); FK a Partida |
| RondaGuardada → Ronda | (IdPartida,Numero); FK a Partida |
| CombateGuardado → Combate | (IdPartida,NumeroRonda,Numero); FK a Ronda (IdPartida,Numero) |
| SolicitudGuardada → Solicitud | Id por convención; índice no único en IdPartida; FK a Combate (IdPartida,NumeroRonda,NumeroCombate) |
| OperacionGuardada → Operacion | (IdPartida,Rol,IdOperacion); FK a Participante (IdPartida,Rol) y a Solicitud por IdSolicitud |

Fragmento real del mapeo compuesto de combates y operaciones:

```csharp
        modelo.Entity<CombateGuardado>().ToTable("Combate")
            .HasKey(c => new { c.IdPartida, c.NumeroRonda, c.Numero });
        modelo.Entity<CombateGuardado>().HasOne<RondaGuardada>().WithMany()
            .HasForeignKey(c => new { c.IdPartida, Numero = c.NumeroRonda });
        modelo.Entity<SolicitudGuardada>().ToTable("Solicitud").HasIndex(s => s.IdPartida);
        modelo.Entity<SolicitudGuardada>().HasOne<CombateGuardado>().WithMany()
            .HasForeignKey(s => new { s.IdPartida, s.NumeroRonda, Numero = s.NumeroCombate });
        modelo.Entity<OperacionGuardada>().ToTable("Operacion")
            .HasKey(o => new { o.IdPartida, o.Rol, o.IdOperacion });
        modelo.Entity<OperacionGuardada>().HasOne<ParticipanteGuardado>().WithMany()
            .HasForeignKey(o => new { o.IdPartida, o.Rol });
        modelo.Entity<OperacionGuardada>().HasOne<SolicitudGuardada>().WithMany()
            .HasForeignKey(o => o.IdSolicitud);
```

La propiedad anónima `Numero = c.NumeroRonda` adapta el nombre del campo de combate al de la clave de Ronda; no cambia el número guardado. `HasOne<...>().WithMany()` declara relaciones sin propiedades de navegación. No declara FK de carta al catálogo: la copia debe sobrevivir al reemplazo del catálogo. Tampoco declara una FK para Partida.IdSolicitud; las transiciones mantienen ese puntero.

### Recorridos y ejemplos

Normal: crear partida P y participantes `(P,J1)` y `(P,J2)` permite dos plazas con mismo IdPartida porque la clave incluye Rol. Guardar un combate `(P,1,1)` requiere la ronda `(P,1)`. Alternativo: insertar otra plaza `(P,J1)` entra en conflicto de clave; una operación que referencia una solicitud inexistente viola la FK. Son consecuencias del modelo, no inserciones ejecutadas aquí.

### Errores, límites y efectos observables

Este archivo no convierte excepciones de base ni verifica umbrales académicos, roles o fases. Los setters admiten asignaciones que luego pueden fallar al guardar. EnsureCreated del repositorio crea esquema si falta; este archivo no contiene migraciones ni actualiza por sí solo un esquema antiguo. La configuración exacta de proveedor se recibe en opciones.

### Relaciones y verificación

[ApiHttpTests](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md) define recuperación tras reinicio y rollback de elección; [CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md) define persistencia exacta y reversión de reemplazo fallido. Cubren usos del esquema, no toda asignación arbitraria a las entidades. Véase [plan de pruebas](plan-de-pruebas.md) para ejecución y limitaciones.

</details>

<details>
<summary>src/servidor/Api/ConfiguracionServidor.cs</summary>

[Página individual](archivos/src/servidor/Api/ConfiguracionServidor.md)

## src/servidor/Api/ConfiguracionServidor.cs

Fuente: [src/servidor/Api/ConfiguracionServidor.cs](../src/servidor/Api/ConfiguracionServidor.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Obtiene la dirección de escucha, ruta SQLite y secreto administrativo antes de crear el host. Separa la lectura interactiva del arranque del [servidor](archivos/src/servidor/Program.md).

### Dependencias

[EntradaConsola](archivos/src/comun/EntradaConsola.md) reintenta campos, aplica valores por defecto y oculta secretos en terminal interactiva. `Path` y `Directory` inspeccionan rutas, sin crear el archivo. [Instalación](instalacion-y-uso.md) explica las tres preguntas.

### Estructura

`internal sealed record ConfiguracionServidor(string Direccion, string Archivo, string Credencial)` genera constructor posicional, propiedades públicas init, igualdad por valor y deconstrucción. No tiene partial, interfaces explícitas ni eventos. Sus dos métodos son estáticos; no guarda lector entre llamadas.

### Estado y datos

`Direccion`, `Archivo` y `Credencial` son strings no anulables. Se conservan en la instancia devuelta. Una credencial vacía es válida para deshabilitar la administración HTTP del catálogo; no significa acceso anónimo. La ruta puede ser relativa y se resolverá desde el directorio de ejecución.

### Miembros y ejecución

`internal static ConfiguracionServidor Leer(TextReader entrada, TextWriter salida)` crea el lector, pide URL con defecto `http://0.0.0.0:5000`, archivo con defecto `guerra.db`, y secreto con `permiteVacio: true`, en ese orden. Archivo inválido repite ese campo, no toda la configuración. Si el secreto resulta de longitud cero, informa que las altas quedan deshabilitadas. Devuelve un record con los tres resultados. EOF y errores de lectura se propagan al Program; no abre SQLite ni comprueba permiso efectivo de escritura.

`private static bool ArchivoValido(string valor)` obtiene `Path.GetFullPath`, luego exige que no haya caracteres de control, que la ruta no sea un directorio y que su directorio padre exista. Acepta archivo existente o aún no creado. Captura `ArgumentException`, `IOException`, `NotSupportedException` y `UnauthorizedAccessException`, devolviendo false; otros errores no se convierten en false.

Fragmento de `ArchivoValido` que sustenta aceptación y rechazo:

```csharp
            string ruta = Path.GetFullPath(valor);
            return !valor.Any(char.IsControl) && !Directory.Exists(ruta) &&
                Directory.Exists(Path.GetDirectoryName(ruta));
```

El `&&` corta al primer requisito falso. No hay prueba de escritura ni creación de directorios.

#### Fragmentos de lectura y recuperación

Leer aplica valores predeterminados y permite secreto vacío únicamente para deshabilitar administración. No abre base durante estas preguntas:

```csharp
        var lector = new EntradaConsola(entrada, salida);
        string direccion = lector.Url("http://0.0.0.0:5000");
        string archivo = lector.Validar("Archivo SQLite (Enter: guerra.db)", ArchivoValido,
            "Ingresá una ruta de archivo válida en un directorio existente.", "guerra.db");
        string credencial = lector.Secreto("Credencial de catálogo (Enter: deshabilitar altas)", permiteVacio: true);
        if (credencial.Length == 0)
            salida.WriteLine("Credencial vacía: alta de personajes deshabilitada.");
        return new(direccion, archivo, credencial);
```

ArchivoValido convierte únicamente estas excepciones de ruta en false; el lector volverá a preguntar el campo:

```csharp
        catch (Exception error) when (error is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
        {
            return false;
        }
```

### Recorridos y ejemplos

Normal: dos Enter y secreto ficticio `catalogo-demo` producen URL por defecto y `guerra.db` si el directorio actual existe. Alternativo: indicar un directorio como archivo devuelve false y repite la pregunta; un secreto vacío termina correctamente e informa administración deshabilitada. EOF durante las preguntas impide devolver una configuración completa. Recorridos contrastados por lectura, sin abrir servicios reales.

### Errores, límites y efectos observables

Solo escribe preguntas/diagnósticos y consume entrada. Un directorio existente puede no ser escribible: el fallo de apertura se gestiona luego en Program. No registra el secreto ni lo incorpora al diagnóstico de ruta.

### Relaciones y verificación

[ConfiguracionServidorTests](archivos/test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md) define valores predeterminados, configuración personalizada, corrección y EOF. [EntradaProcesosContratoTests](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) comprueba reintento ante base no utilizable. Una prueba definida no acredita su ejecución; consultar [evidencia](plan-de-pruebas.md).

</details>

<details>
<summary>src/servidor/Api/ConsolaServidor.cs</summary>

[Página individual](archivos/src/servidor/Api/ConsolaServidor.md)

## src/servidor/Api/ConsolaServidor.cs

Fuente: [src/servidor/Api/ConsolaServidor.cs](../src/servidor/Api/ConsolaServidor.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Expone operaciones administrativas por entrada/salida del operador local, con preguntas separadas para UUID, plaza o personaje. [Program](archivos/src/servidor/Program.md) ejecuta su bucle junto al host. No ofrece un protocolo de consola para los jugadores: ellos usan HTTP.

### Dependencias

[RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md) crea/cancela partidas, revoca sesiones y desactiva cartas; [transiciones](archivos/src/servidor/Api/TransicionesPartida.md) inicia. [EntradaConsola](archivos/src/comun/EntradaConsola.md) valida y repite preguntas. SqliteException y DbUpdateException producen diagnóstico de almacenamiento. [Protocolo](protocolo-de-partida.md) describe operación del servidor.

### Estructura y estado

`internal static class ConsolaServidor`, sin instancia, partial, eventos o interfaces. Campo privado estático readonly `Dictionary<string,(string Grupo,string Descripcion)> comandos`: crear/iniciar/cancelar en Partidas; cerrar-sesion/reemplazar-credencial en Jugadores; desactivar en Catálogo; ayuda en Ayuda. El diccionario conserva textos para mostrar ayuda, no delegados que ejecuten comandos; el switch de Comando hace el despacho. No se modifica después de inicializar.

### Miembros y ejecución

`internal static void Ejecutar(RepositorioJuego juego,TextReader entrada,TextWriter salida,CancellationToken detener)` imprime Ayuda y repite mientras no haya cancelación y ReadLine devuelva línea. Vuelve a comprobar cancelación después de leer para no aplicar una línea recibida al detener. Pasa `linea.Trim()` a Comando. EOF durante preguntas produce EndOfStreamException capturada y retorno sin ejecutar la acción incompleta; ErrorApi imprime Message y continúa con próximo comando; SqliteException o DbUpdateException imprimen «El almacenamiento no está disponible.» y continúan. Otras excepciones se propagan. Una cancelación no interrumpe una ReadLine ya bloqueada: se comprueba al recuperar control.

`private static void Comando(RepositorioJuego juego,string linea,TextReader entrada,TextWriter salida)` pasa comando a minúsculas invariantes, crea lector, inicia id=Guid.Empty y rol=null. Para iniciar/cancelar/cerrar-sesion/reemplazar-credencial pregunta UUID, exige TryParse y no Empty y repite si falla; después Parse es seguro para la cadena aceptada. Para cerrar/reemplazar pregunta rol con aceptación J1/J2 ignorando caso y almacena mayúsculas. Para desactivar pide entero positivo. No mezcla los argumentos con la línea del comando: escribir `iniciar UUID` se trata como comando desconocido.

Fragmento real de las precondiciones de preguntas:

```csharp
        if (comando is "iniciar" or "cancelar" or "cerrar-sesion" or "reemplazar-credencial")
            id = Guid.Parse(lector.Validar("UUID de la partida", v => Guid.TryParse(v, out var g) && g != Guid.Empty,
                "Ingresá un UUID válido y distinto de cero."));
        if (comando is "cerrar-sesion" or "reemplazar-credencial")
            rol = lector.Validar("Plaza: J1 o J2", v => v.ToUpperInvariant() is "J1" or "J2",
                "Elegí J1 o J2.").ToUpperInvariant();
        string argumento = comando == "desactivar"
            ? lector.Validar("ID numérico del personaje", v => int.TryParse(v, out int n) && n > 0,
                "Ingresá un ID numérico positivo.") : "";
```

Despacho y efectos del switch:

| Comando | Acción y salida |
|---|---|
| cadena vacía | No hace nada; vuelve al bucle. |
| ayuda | Reimprime Ayuda. |
| crear | CrearPartida; imprime `Partida {UUID}`, `J1 {credencial}` y `J2 {credencial}`. Las credenciales se revelan aquí intencionalmente al operador. |
| iniciar | Iniciar(id); si acepta imprime «Partida iniciada.». |
| cancelar | Cancelar(id); imprime «Partida cancelada.». |
| cerrar-sesion | CerrarSesion(id,rol); imprime «Sesión cerrada.». |
| reemplazar-credencial | ReemplazarCredencial(id,rol); imprime rol y nuevo secreto. |
| desactivar | Si argumento parsea int, Desactivar; imprime «Personaje desactivado.». La pregunta ya validó positivo. |
| cualquier otro | Imprime comando desconocido y sugiere ayuda; no llama al repositorio. |

`private static void Ayuda(TextWriter salida)` imprime título e instrucciones crear/iniciar, agrupa comandos por Grupo con GroupBy, recorre cada grupo y luego cada entrada mostrando nombre y descripción. Termina con instrucciones de responder preguntas y EOF. Las iteraciones son finitas sobre siete entradas; no interactúan con la base ni consumen entrada.

Fragmento real del bucle de ayuda:

```csharp
        foreach (var grupo in comandos.GroupBy(comando => comando.Value.Grupo))
        {
            salida.WriteLine();
            salida.WriteLine(grupo.Key);
            foreach (var comando in grupo)
            {
                salida.WriteLine($"  {comando.Key}");
                salida.WriteLine($"    {comando.Value.Descripcion}");
            }
        }
```

#### Fragmentos de despacho y control del bucle

Ejecutar vuelve a comprobar cancelación después de leer; EOF en una pregunta retorna, mientras ErrorApi permite continuar. El diagnóstico de base no imprime detalles internos:

```csharp
        while (!detener.IsCancellationRequested && entrada.ReadLine() is string linea)
        {
            if (detener.IsCancellationRequested)
                break;

            try
            {
                Comando(juego, linea.Trim(), entrada, salida);
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (ErrorApi error)
            {
                salida.WriteLine(error.Message);
            }
            catch (Exception error) when (error is SqliteException or DbUpdateException)
            {
                salida.WriteLine("El almacenamiento no está disponible.");
            }
        }
```

Estos casos del switch vinculan comandos aceptados con efectos reales: crear revela credenciales al operador; iniciar/cancelar solo imprimen éxito después del retorno del repositorio; cerrar/reemplazar afectan sesión/credencial; desactivar actúa sobre catálogo:

```csharp
            case "crear":
                var partida = juego.CrearPartida();
                salida.WriteLine($"Partida {partida.Id}");
                salida.WriteLine($"J1 {partida.CredencialJ1}");
                salida.WriteLine($"J2 {partida.CredencialJ2}");
                break;
            case "iniciar":
                juego.Iniciar(id);
                salida.WriteLine("Partida iniciada.");
                break;
            case "cancelar":
                juego.Cancelar(id);
                salida.WriteLine("Partida cancelada.");
                break;
            case "cerrar-sesion":
                juego.CerrarSesion(id, rol!);
                salida.WriteLine("Sesión cerrada.");
                break;
            case "reemplazar-credencial":
                salida.WriteLine($"{rol} {juego.ReemplazarCredencial(id, rol!)}");
                break;
            case "desactivar" when int.TryParse(argumento, out int personaje):
                juego.Desactivar(personaje);
                salida.WriteLine("Personaje desactivado.");
                break;
            default:
                salida.WriteLine("Comando desconocido. Escribí ayuda para ver los comandos.");
                break;
```

### Recorridos y ejemplos

Normal: escribir `crear` produce UUID P y dos credenciales; tras incorporar ambas plazas y cargar seis cartas, escribir `iniciar`, luego P, inicia y confirma. Entrada ` CERRAR-SESION ` se recorta y baja a minúsculas; pregunta UUID y plaza, acepta `j1` y la convierte a J1 antes de cerrar.

Alternativo: `cancelar`, UUID inválido, UUID válido reintenta solo el dato y finalmente cancela. EOF mientras espera UUID cierra sin Cancelar. `iniciar P` en una sola línea imprime desconocido. ErrorApi de partida inexistente imprime su mensaje y permite otro comando. Recorridos de lectura, no administración de servicios reales.

### Errores, límites y efectos observables

Hay escritura persistida cuando el repositorio la ejecuta y cambios de sesión en memoria para cerrar/reemplazar. No imprime stack ni errores SQLite internos. Solo captura los fallos enumerados. Una cancelación durante lectura no puede cerrar el flujo por sí misma; Program coordina parada del host.

### Relaciones y verificación

[ConsolaServidorTests](archivos/test/proyecto-integrador-final-tests/ConsolaServidorTests.md) define ayuda, corrección de UUID/plaza y EOF antes de confirmar. [EntradaProcesosContratoTests](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) ejercita entrada del proceso real. La suite de referencia se registra en [verificación spec 004](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004); no equivale a ensayo de administración de un servidor real de producción.

</details>

<details>
<summary>src/servidor/Api/EstadoPartida.cs</summary>

[Página individual](archivos/src/servidor/Api/EstadoPartida.md)

## src/servidor/Api/EstadoPartida.cs

Fuente: [src/servidor/Api/EstadoPartida.cs](../src/servidor/Api/EstadoPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Agrupa los datos persistidos de cartas de una partida, rondas, combates, solicitudes y recibos idempotentes. Proporciona conversiones entre copia persistida, dominio local y JSON visible. [TransicionesPartida](archivos/src/servidor/Api/TransicionesPartida.md) modifica estas entidades; [PresentacionPartida](archivos/src/servidor/Api/PresentacionPartida.md) compone la vista sin filtrar manos ajenas por accidente.

### Dependencias

[PersonajeCatalogo](archivos/src/servidor/Api/PersonajeCatalogo.md) es origen de copias; [Personaje](archivos/src/servidor/Personaje.md) valida/reconstruye reglas; [BaseJuego](archivos/src/servidor/Api/BaseJuego.md) declara claves compuestas y FKs. `Dictionary<string,int>` transporta puntajes. [Protocolo](protocolo-de-partida.md) explica solicitudes y recibos.

### Estructura

Clases `internal sealed`: PersonajePartida, RondaGuardada, CombateGuardado, SolicitudGuardada y OperacionGuardada. Records posicionales `internal sealed`: CartaVisible, Eleccion, Recibo y Confirmacion. No tienen partial, interfaces explícitas, atributos, eventos, operadores ni funciones locales. Los records generan constructor, propiedades públicas init e igualdad por valor; arrays/diccionarios no se clonan mediante esa generación.

### Estado y datos

Todas las propiedades de las clases son públicas get/set y por tanto mutables. Sin inicializador, enteros son cero, bool es false, Guid es Empty y anulables null; `required` exige inicializar al crear desde C#.

| Tipo | Propiedades completas y significado |
|---|---|
| PersonajePartida | Guid IdPartida, int IdPersonaje (clave compuesta); required string Nombre/Descripcion/Ascii; int Fuerza/Velocidad/Resistencia/Inteligencia/Energia/Habilidad; string Ubicacion = "mazo"; int Orden. Ubicación cambia a J1/J2/jugada en el ciclo de ronda. Orden fija presentación y reparto. |
| RondaGuardada | Guid IdPartida; int Numero; required string Mano; int VictoriasJ1/VictoriasJ2; string? Ganador. Mano inicia elecciones y alterna entre rondas; contadores son combates ganados, no rondas de partida. |
| CombateGuardado | Guid IdPartida; int NumeroRonda/Numero; required string Elector; int? IdCartaElector/IdCartaRival; string? Atributo; int? PuntajeElector/PuntajeRival; string? Ganador. Los null distinguen elecciones/resultados aún incompletos. |
| SolicitudGuardada | Guid Id/IdPartida; int NumeroRonda/NumeroCombate; required string Rol/Tipo; bool Consumida. Una solicitud identifica la oportunidad concreta para una plaza. |
| OperacionGuardada | Guid IdPartida/IdOperacion/IdSolicitud; required string Rol; int RevisionEsperada/IdPersonaje/RevisionConfirmada; string? Atributo. Conserva entrada y revisión posterior para comparar reintentos. |
| CartaVisible | int IdPersonaje; string Nombre/Descripcion/Ascii; Dictionary<string,int> Atributos. Payload de carta, sin ubicación del rival ni credenciales. |
| Eleccion | Guid IdOperacion/IdSolicitud; int? RevisionEsperada/IdPersonaje; string? Atributo. Anulables permiten distinguir ausencia de campos numéricos. |
| Recibo | Guid IdOperacion/IdSolicitud; int RevisionConfirmada. Confirma la transacción, no el turno actual. |
| Confirmacion | Recibo Recibo; bool Repetida. El bool selecciona HTTP 200 para repetición y 201 para primera confirmación. |

### Miembros y ejecución

`internal static PersonajePartida Copiar(Guid partida, PersonajeCatalogo origen)` crea entidad nueva con IdPartida recibido y copia Id, nombre, descripción, arte y los seis puntajes del catálogo, sin normalización ni vínculo vivo con ese objeto. No copia Activo: el llamador ya filtró activos. Orden queda cero y Ubicacion mazo hasta repartir; null en origen produciría NullReferenceException porque no hay guarda.

`internal Personaje Dominio()` construye un nuevo Personaje con nombre, arte y seis puntajes; puede lanzar ArgumentException si la fila es inválida. No copia descripción o ID al dominio porque ese tipo no los tiene. No persiste cambios.

`internal CartaVisible Vista()` crea record y diccionario nuevos con los seis valores. Las claves visibles llevan inicial mayúscula y `Energía` con tilde, mientras el campo persistido se llama Energia. No oculta atributos ni modifica la entidad.

Fragmentos reales de las conversiones, sin omitir ninguna asignación:

```csharp
    internal static PersonajePartida Copiar(Guid partida, PersonajeCatalogo origen) => new()
    {
        IdPartida = partida, IdPersonaje = origen.Id, Nombre = origen.Nombre,
        Descripcion = origen.Descripcion, Ascii = origen.Ascii, Fuerza = origen.Fuerza,
        Velocidad = origen.Velocidad, Resistencia = origen.Resistencia,
        Inteligencia = origen.Inteligencia, Energia = origen.Energia, Habilidad = origen.Habilidad
    };

    internal Personaje Dominio() =>
        new(Nombre, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);

    internal CartaVisible Vista() => new(IdPersonaje, Nombre, Descripcion, Ascii, new()
    {
        ["Fuerza"] = Fuerza, ["Velocidad"] = Velocidad, ["Resistencia"] = Resistencia,
        ["Inteligencia"] = Inteligencia, ["Energía"] = Energia, ["Habilidad"] = Habilidad
    });
```

`internal Eleccion Datos()` de OperacionGuardada devuelve exactamente los cinco campos de entrada (no RevisionConfirmada); habilita comparación por igualdad del record. `internal Recibo Recibo()` devuelve IdOperacion, IdSolicitud y RevisionConfirmada. Son asignaciones sin ramas, consultas o efectos externos:

```csharp
    internal Eleccion Datos() => new(IdOperacion, IdSolicitud, RevisionEsperada, IdPersonaje, Atributo);
    internal Recibo Recibo() => new(IdOperacion, IdSolicitud, RevisionConfirmada);
```

Las demás clases solo contienen datos; sus constructores implícitos y accesores no añaden validación ni recorridos propios.

### Recorridos y ejemplos

Normal: copiar catálogo ID 8 con fuerza 650 a partida P produce clave `(P,8)`, ubicación mazo y fuerza 650. Vista devuelve clave `Fuerza:650`; cambiar después el catálogo no modifica la copia. Una operación con revisión esperada 4 y confirmada 5 devuelve Datos con 4 y Recibo con 5.

Alternativo: Dominio de una fila con seis 500 falla por falta de fortaleza aunque la suma sea correcta; los setters de la entidad no lo habían impedido. No existe rama de error en Vista ni Datos/Recibo cuando sus datos están inicializados: solo proyectan valores. Recorridos deducidos por lectura.

### Errores, límites y efectos observables

No guarda ni hace transacciones. El repositorio asigna IDs, cambia ubicaciones y marca Consumida. Required no equivale a validación de JSON ni SQL de reglas académicas. Las propiedades permiten estados intermedios deliberados (combate sin ambas cartas); las transiciones fijan cuándo pueden publicarse.

### Relaciones y verificación

[ApiHttpTests](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md) y [ServidorContratoTests](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md) definen privacidad, recuperación y recibo estable; [CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md) define datos exactos y reversión del catálogo; no ensaya explícitamente una partida activa durante su reemplazo. La independencia de copias se comprueba aquí por las asignaciones de Copiar y la ausencia de FK al catálogo. La lectura de estas pruebas no acredita ejecución; consultar [plan](plan-de-pruebas.md).

</details>

<details>
<summary>src/servidor/Api/LotePersonajes.cs</summary>

[Página individual](archivos/src/servidor/Api/LotePersonajes.md)

## src/servidor/Api/LotePersonajes.cs

Fuente: [src/servidor/Api/LotePersonajes.cs](../src/servidor/Api/LotePersonajes.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Convierte el JSON de reemplazo de catálogo en un array tipado, comprobando forma, tipos, IDs prohibidos y nombres duplicados antes de persistir. [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md) lo llama para PUT del catálogo; [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md) vuelve a validar reglas de dominio y realiza el reemplazo atómico.

### Dependencias

`System.Text.Json.JsonElement` inspecciona el árbol sin deserialización permisiva. [AltaPersonaje](archivos/src/servidor/Api/PersonajeCatalogo.md) recibe los nueve campos. [ErrorApi](archivos/src/servidor/Api/ServidorHttp.md) transporta el rechazo HTTP 400. No requiere base ni configuración.

### Estructura

`internal static class LotePersonajes`, sin estado estático mutable. Declara `Leer`, `Invalido` y las funciones locales `Texto` y `Entero` dentro del recorrido. Las lambdas de `Any` detectan nombres de propiedad prohibidos. No tiene constructor de instancia, propiedades, eventos ni partial.

### Estado y datos

Cada llamada crea una lista de altas y un `HashSet<string>(StringComparer.OrdinalIgnoreCase)`. El hash usa nombre recortado con `Trim`, pero el nombre que se conserva en el alta no se recorta. El índice de error empieza en 1 y avanza solo tras agregar un personaje válido. Campos extra no prohibidos se ignoran; los nueve obligatorios se buscan con nombre exacto en minúsculas.

### Miembros y ejecución

`internal static AltaPersonaje[] Leer(JsonElement raiz)` rechaza raíz no-array o array vacío con 400. Recorre todos los elementos en orden; cada uno debe ser objeto. Si `nombre` ya es string, lo incluye en la ubicación del error. Antes de crear datos, examina todas las propiedades y rechaza `id` o `idPersonaje` sin distinguir mayúsculas. Llama a Texto para nombre, descripcion y ascii; a Entero para fuerza, velocidad, resistencia, inteligencia, energia y habilidad. Luego agrega el nombre normalizado al conjunto; si ya existía rechaza el lote completo. Si todo termina devuelve un array en orden de entrada, sin asignar IDs ni guardar nada.

`string Texto(string campo)` captura el objeto y la ubicación actuales: propiedad ausente o no-string produce error; string vacío sí pasa esta capa. Retorna el string no nulo. `int Entero(string campo)` exige propiedad numérica y `TryGetInt32` exitoso; no convierte texto ni fracciones fuera de la representación int32 aceptada. No aplica rangos académicos. Ambas funciones terminan en retorno o excepción, sin reintentos.

Fragmentos de `Leer`, incluyendo auxiliares y control de IDs:

```csharp
            string Texto(string campo)
            {
                if (!objeto.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.String)
                    throw Invalido($"{ubicacion}: {campo} debe ser texto y es obligatorio.");
                return valor.GetString()!;
            }
            int Entero(string campo)
            {
                if (!objeto.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.Number || !valor.TryGetInt32(out int numero))
                    throw Invalido($"{ubicacion}: {campo} debe ser un entero y es obligatorio.");
                return numero;
            }
            if (objeto.EnumerateObject().Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("idPersonaje", StringComparison.OrdinalIgnoreCase)))
                throw Invalido($"{ubicacion}: el ID lo asigna el servidor.");
```

`internal static ErrorApi Invalido(string mensaje)` devuelve una excepción nueva con estado 400 y código `entradaInvalida`; no la lanza por sí mismo. Los llamadores usan `throw`. La validación de atributos y ASCII está en AltaPersonaje, no en estas funciones locales.

#### Fragmentos de raíz, ubicación y duplicados

El recorrido existe solo para un array no vacío; el diagnóstico de un elemento no-objeto antecede al acceso a propiedades:

```csharp
        if (raiz.ValueKind != JsonValueKind.Array || raiz.GetArrayLength() == 0)
            throw Invalido("La raíz debe ser una lista con al menos un personaje.");
        var resultado = new List<AltaPersonaje>();
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var objeto in raiz.EnumerateArray())
        {
            string ubicacion = $"Personaje {resultado.Count + 1}";
            if (objeto.ValueKind != JsonValueKind.Object) throw Invalido($"{ubicacion}: se esperaba un objeto.");
            if (objeto.TryGetProperty("nombre", out var nombre) && nombre.ValueKind == JsonValueKind.String)
                ubicacion += $" ({nombre.GetString()})";
```

Al construir AltaPersonaje, cada llamada local exige tipo exacto. El conjunto comprueba nombre recortado sin modificar el valor guardado:

```csharp
            var datos = new AltaPersonaje(Texto("nombre"), Texto("descripcion"), Texto("ascii"),
                Entero("fuerza"), Entero("velocidad"), Entero("resistencia"), Entero("inteligencia"), Entero("energia"), Entero("habilidad"));
            if (!nombres.Add(datos.Nombre!.Trim())) throw Invalido($"{ubicacion}: nombre duplicado.");
            resultado.Add(datos);
        }
        return resultado.ToArray();
```

### Recorridos y ejemplos

Normal: un objeto con nombre Ada, descripcion vacía, ascii `*` y seis enteros `(250,350,450,550,650,750)` produce un alta; RepositorioJuego decidirá el ID. Alternativo: nombres `Ada` y ` ada ` provocan `Personaje 2 ( ada ): nombre duplicado.`; una propiedad `ID` se rechaza aunque los demás datos sean correctos. Un array `[7]` falla como objeto antes de leer campos. Recorridos de lectura, no peticiones ejecutadas aquí.

### Errores, límites y efectos observables

No impone tamaño máximo de lote propio, ni verifica nombre en blanco, límites ASCII o suma de puntajes: la capa de dominio posterior lo hace. No contiene persistencia, concurrencia ni efectos externos; al fallar no devuelve un lote parcial.

### Relaciones y verificación

[CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md) define rechazo de formato incompleto, duplicados normalizados, obligatoriedad y tipos estrictos. La atomicidad pertenece al repositorio y se prueba allí mediante reemplazo y fallo de inserción. Véase [plan de pruebas](plan-de-pruebas.md) para ejecución real y [carga](archivos/src/subir-personajes/Program.md) para la comprobación previa del cliente.

</details>

<details>
<summary>src/servidor/Api/PersonajeCatalogo.cs</summary>

[Página individual](archivos/src/servidor/Api/PersonajeCatalogo.md)

## src/servidor/Api/PersonajeCatalogo.cs

Fuente: [src/servidor/Api/PersonajeCatalogo.cs](../src/servidor/Api/PersonajeCatalogo.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Declara el contrato de alta y la entidad persistida del catálogo. `AltaPersonaje.Validar` transforma un pedido en una entidad válida, combinando reglas del arte y del modelo [Personaje](archivos/src/servidor/Personaje.md). El ID lo asigna [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md).

### Dependencias

[Personaje](archivos/src/servidor/Personaje.md) comprueba nombre y puntajes; [LotePersonajes.Invalido](archivos/src/servidor/Api/LotePersonajes.md) crea errores 400; [BaseJuego](archivos/src/servidor/Api/BaseJuego.md) mapea PersonajeCatalogo a SQLite. `string.Replace`, Split y LINQ Any calculan límites del arte. [Bases de datos](bases-de-datos.md) explica el catálogo y sus copias de partida.

### Estructura

`internal sealed record AltaPersonaje(string? Nombre, string? Descripcion, string? Ascii, int Fuerza, int Velocidad, int Resistencia, int Inteligencia, int Energia, int Habilidad)` genera propiedades públicas init, constructor e igualdad por valor. Declara `internal PersonajeCatalogo Validar()`.

`internal sealed class PersonajeCatalogo` contiene solo propiedades públicas get/set, sin constructor explícito, herencia, interfaces, atributos, partial ni lógica de acceso. `required` exige inicialización en C#, pero no es una validación HTTP de ejecución.

### Estado y datos

| Tipo | Propiedades y significado |
|---|---|
| AltaPersonaje | Nombre/Descripcion/Ascii anulables de entrada; seis puntajes int sin defecto explícito (cero si la deserialización no los aporta). |
| PersonajeCatalogo | `Id` int, clave manual; `required string Nombre`, `Descripcion`, `Ascii`; seis puntajes int con los mismos nombres; `bool Activo = true`. Todos mutables, persistidos por EF. |

No hay eventos, campos adicionales ni credenciales. La descripción vacía y el arte vacío se aceptan; nombre vacío no.

### Miembros y ejecución

`Validar()` rechaza primero descripción null, después arte null, después tabulaciones. Para contar líneas normaliza CRLF y CR a LF en una variable local y divide por LF. Si hay más de 25 líneas o cualquiera supera 120 caracteres UTF-16, falla. No almacena la versión normalizada: conserva el arte exacto recibido, incluidos saltos originales. Después construye temporalmente Personaje con los seis puntajes: rechaza nombre sin texto, atributo fuera de [250,750], suma distinta de 3000, falta de valor ≥650 o falta de valor ≤350, en ese orden. Captura `ArgumentException` de ese constructor y lo envuelve en error 400 con nombre de parámetro y mensaje. Si acepta, devuelve entidad nueva con todos los valores copiados, Id 0 aún sin asignar y Activo true; no persiste.

Fragmento real de la validación de arte y delegación numérica:

```csharp
        if (Descripcion is null) throw LotePersonajes.Invalido("descripcion: debe ser texto no nulo.");
        if (Ascii is null) throw LotePersonajes.Invalido("ascii: debe ser texto no nulo.");
        if (Ascii.Contains('\t')) throw LotePersonajes.Invalido("ascii: no admite tabulaciones.");
        var lineas = Ascii.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (lineas.Length > 25 || lineas.Any(linea => linea.Length > 120))
            throw LotePersonajes.Invalido("ascii: admite hasta 25 líneas de 120 caracteres.");

        try
        {
            _ = new Personaje(Nombre!, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);
        }
        catch (ArgumentException error)
        {
            throw LotePersonajes.Invalido($"{error.ParamName}: {error.Message}");
        }
```

No hay setters con lógica: el bloque final de inicialización simplemente copia los nueve campos. Las seis propiedades numéricas son Fuerza, Velocidad, Resistencia, Inteligencia, Energia y Habilidad; no convierten unidades ni redondean.

#### Fragmento de entidad aceptada

Solo después de las validaciones se devuelve esta entidad. La inicialización no asigna ID ni Activo: quedan en cero y true por sus declaraciones hasta que el repositorio les asigne identidad:

```csharp
        return new()
        {
            Nombre = Nombre!,
            Descripcion = Descripcion,
            Ascii = Ascii,
            Fuerza = Fuerza,
            Velocidad = Velocidad,
            Resistencia = Resistencia,
            Inteligencia = Inteligencia,
            Energia = Energia,
            Habilidad = Habilidad
        };
```

### Recorridos y ejemplos

Normal: Ada, descripción vacía, arte `*` y `(250,350,450,550,650,750)` retorna entidad activa con Id pendiente. Alternativo: arte con tabulación falla antes de medir líneas; 26 líneas falla antes de construir Personaje; seis 500 suman 3000 pero fallan por falta de fortaleza. Si el nombre es null y el arte contiene tabulación, el error de arte aparece primero. Ejemplos revisados por lectura.

### Errores, límites y efectos observables

Todos los rechazos explícitos son `ErrorApi(400, entradaInvalida, ...)`. No comprueba duplicados de nombre, caracteres de control distintos de tab, unicidad o longitud del nombre. La entidad puede recibir valores inválidos si se crea directamente sin pasar por Validar; las propiedades no contienen las reglas. No altera ni normaliza los valores aceptados.

### Relaciones y verificación

[CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md) define límites de texto, reglas de puntajes y diagnóstico de atributo; [ApiHttpTests](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md) define alta autenticada y rechazo de puntajes como texto. [PersonajeTests](archivos/test/proyecto-integrador-final-tests/PersonajeTests.md) cubre el dominio subyacente. Consultar [evidencia de ejecución](plan-de-pruebas.md); esta página no presenta tests definidos como ejecutados.

</details>

<details>
<summary>src/servidor/Api/PresentacionPartida.cs</summary>

[Página individual](archivos/src/servidor/Api/PresentacionPartida.md)

## src/servidor/Api/PresentacionPartida.cs

Fuente: [src/servidor/Api/PresentacionPartida.cs](../src/servidor/Api/PresentacionPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Construye el estado visible para una plaza autenticada, sin revelar la mano rival ni cartas de combates incompletos. Es la porción de lectura de `internal sealed partial RepositorioJuego`, junto a [repositorio](archivos/src/servidor/Api/RepositorioJuego.md) y [transiciones](archivos/src/servidor/Api/TransicionesPartida.md). La presencia puede cambiar Pausada sin modificar revisión persistida.

### Dependencias

[EstadoPartida](archivos/src/servidor/Api/EstadoPartida.md) proporciona CartaVisible y entidades; [BaseJuego](archivos/src/servidor/Api/BaseJuego.md) permite consultas EF. `System.Text.Json.Serialization.JsonIgnore` omite dos campos de cierre cuando son null. LINQ ordena y agrupa historial. [Protocolo](protocolo-de-partida.md) describe privacidad y orden de resultados.

### Estructura

Además del método de la clase parcial declara tres `internal sealed record` posicionales. Constructor, propiedades públicas init e igualdad se generan; no hay setters con lógica, eventos ni funciones locales. El record no hace copia profunda de arrays o diccionarios.

### Estado y datos

| Record | Propiedades completas |
|---|---|
| SolicitudVisible | Guid IdSolicitud; string Tipo; int[] IdsPersonajePermitidos; string[] AtributosPermitidos. Solo se entrega a quien debe elegir. |
| ResultadoVisible | int NumeroRonda/NumeroCombate; Dictionary<string,CartaVisible> Cartas; string Atributo; Dictionary<string,int> Puntajes; string GanadorCombate; string? GanadorRonda. Mapas indexados por J1/J2. |
| PresentacionVisible | Guid IdPartida; int Revision; string Fase; bool Pausada; string RolPropio; Dictionary<string,string?> Nombres; int? NumeroRonda/NumeroCombate; Dictionary<string,int> Marcador; CartaVisible[] Mano; SolicitudVisible? Solicitud; ResultadoVisible[] Resultados; string? MotivoFin/Ganador. |

MotivoFin y Ganador llevan `[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`: se omiten solo al ser null. Otros anulables no tienen ese atributo. No hay campos de estado nuevos en la porción parcial; las colecciones temporales duran una lectura.

### Miembros y ejecución

`internal PresentacionVisible Presentacion(Guid id,string? credencial,Guid idSesion)` llama Ejecutar(false,...), por tanto abre una transacción de lectura y lock sin SaveChanges. Dentro:

1. Acceso exige existencia, credencial y sesión propia vigente. Calcula Pausada según ambos presentes y fase.
2. Consulta participantes de la partida y cartas ordenadas por Orden. Filtra mano por Ubicacion == rol propio y proyecta Vista. No incluye la reserva ni cartas en jugada dentro de Mano.
3. Inicia visible=null. Solo si no pausada y hay IdSolicitud busca la fila; solo si Rol coincide crea SolicitudVisible con IDs de la mano propia. Para cartaYAtributo incluye las seis claves canónicas del mapa; para respuesta carta devuelve atributos vacíos. No entrega solicitud al observador de turno ajeno.
4. Consulta combates de la partida con Ganador no null, ordenados primero por ronda y luego número. Obtiene ganadores de rondas ya definidas y el último combate resuelto por ronda. Crea diccionario de cartas por ID para reconstruir resultados con ambas cartas y puntajes.
5. Por cada combate resuelto arma ResultadoVisible. GanadorRonda solo se incluye en el último combate resuelto de su ronda y si la ronda está definida; los anteriores llevan null. Devuelve todos los resultados históricos resueltos, no solo los nuevos para un cliente.
6. Retorna presentación con revisión/fase persistidas, pausa calculada, rol, nombres, números actuales, marcador de rondas, mano propia, solicitud visible, resultados y cierre. No renueva presencia ni incrementa revisión.

Fragmento real de la condición de solicitud:

```csharp
            SolicitudVisible? visible = null;
            if (!pausada && partida.IdSolicitud is Guid idSolicitud)
            {
                var solicitud = db.Solicitudes.Find(idSolicitud)!;
                if (solicitud.Rol == jugador.Rol)
                    visible = new(solicitud.Id, solicitud.Tipo, mano.Select(c => c.IdPersonaje).ToArray(),
                        solicitud.Tipo == "cartaYAtributo" ? atributos.Keys.ToArray() : []);
            }
```

Fragmento de selección y orden del historial; excluye elecciones aún pendientes:

```csharp
            var historial = db.Combates.Where(c => c.IdPartida == id && c.Ganador != null)
                .OrderBy(c => c.NumeroRonda).ThenBy(c => c.Numero).ToArray();
            var ganadores = db.Rondas.Where(r => r.IdPartida == id && r.Ganador != null)
                .ToDictionary(r => r.Numero, r => r.Ganador);
            var ultimos = historial.GroupBy(c => c.NumeroRonda).ToDictionary(g => g.Key, g => g.Last().Numero);
            var cartasPorId = cartas.ToDictionary(c => c.IdPersonaje);
```

Las lambdas de proyección usan `Otro(c.Elector)` para asociar segunda carta y puntaje al rival, no a un orden fijo de J1/J2. Los `!.Value` presuponen que un combate con ganador tiene cartas y puntajes completos. Las claves duplicadas o referencias faltantes revelarían una base inconsistente mediante excepción, no se silencian.

#### Fragmento de proyección de resultados y vista final

Cada resultado asigna carta/puntaje por rol del elector y su opuesto. La condición del último combate liga GanadorRonda solo al cierre de esa ronda; luego se combinan nombres/marcador y mano propia sin secreto:

```csharp
            var resultados = historial.Select(c => new ResultadoVisible(
                c.NumeroRonda, c.Numero,
                new()
                {
                    [c.Elector] = cartasPorId[c.IdCartaElector!.Value].Vista(),
                    [Otro(c.Elector)] = cartasPorId[c.IdCartaRival!.Value].Vista()
                },
                c.Atributo!,
                new()
                {
                    [c.Elector] = c.PuntajeElector!.Value,
                    [Otro(c.Elector)] = c.PuntajeRival!.Value
                },
                c.Ganador!,
                ultimos[c.NumeroRonda] == c.Numero ? ganadores.GetValueOrDefault(c.NumeroRonda) : null))
                .ToArray();

            return new PresentacionVisible(id, partida.Revision, partida.Fase, pausada, jugador.Rol,
                participantes.ToDictionary(p => p.Rol, p => p.Nombre), partida.NumeroRonda,
                partida.NumeroCombate, participantes.ToDictionary(p => p.Rol, p => p.RondasGanadas),
                mano, visible, resultados, partida.MotivoFin, partida.Ganador);
```

### Recorridos y ejemplos

Normal: J1 espera elegir y ambos presentes; su Mano tiene tres cartas, Solicitud incluye sus tres IDs y seis atributos. J2 recibe su propia mano y Solicitud null. Tras resolver combate 1 ambos ven las dos cartas del combate en Resultados ordenados; las cartas restantes del rival siguen fuera de Mano.

Alternativo: vence presencia de J2 durante esperandoElector; J1 aún autenticado recibe Pausada true y Solicitud null con la misma Revision. Si consultara J2 con su sesión vencida, falla Acceso antes de proyectar. En esperandoInicio, números pueden ser null e historial vacío; no hay carta que revelar. Recorridos de lectura, no requests ejecutados aquí.

### Errores, límites y efectos observables

Errores de acceso son 404/401/409 desde helpers. No hay escritura ni secreto dentro del payload. El historial crece con toda la partida; este método no pagina ni acepta revisión desde la cual leer. `no-store` lo agrega el handler HTTP, no este método. Los diccionarios y arrays son nuevos para la respuesta, pero el código no aplica saneamiento terminal: lo hace el cliente al presentar.

### Relaciones y verificación

[ApiHttpTests](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md) define elecciones privadas y presencia sin cambio de revisión; [ServidorContratoTests](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md) define visibilidad por plaza, pausa y misma solicitud tras recuperar. [EstadoCliente](archivos/src/cliente/EstadoCliente.md) decide qué resultados nuevos mostrar. Véase [plan de pruebas](plan-de-pruebas.md) para evidencia de ejecución.

</details>

<details>
<summary>src/servidor/Api/RepositorioJuego.cs</summary>

[Página individual](archivos/src/servidor/Api/RepositorioJuego.md)

## src/servidor/Api/RepositorioJuego.cs

Fuente: [src/servidor/Api/RepositorioJuego.cs](../src/servidor/Api/RepositorioJuego.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Proporciona acceso serializado al catálogo y partidas, transacciones SQLite y autenticación por plaza. Conserva presencia en memoria durante 15 segundos. Es la parte central de un tipo parcial: [TransicionesPartida](archivos/src/servidor/Api/TransicionesPartida.md) ejecuta jugadas y [PresentacionPartida](archivos/src/servidor/Api/PresentacionPartida.md) proyecta el estado.

### Dependencias

[BaseJuego](archivos/src/servidor/Api/BaseJuego.md) y EF Core consultan/persisten entidades; Microsoft.Data.Sqlite abre transacciones. SHA256, UTF8 y `CryptographicOperations.FixedTimeEquals` comparan hashes; RandomNumberGenerator genera credenciales. TimeProvider permite tiempo sustituible. [AltaPersonaje](archivos/src/servidor/Api/PersonajeCatalogo.md) valida altas. [Protocolo](protocolo-de-partida.md) explica presencia, errores y operación administrativa; [base de datos](bases-de-datos.md) explica durabilidad.

### Estructura

`internal sealed partial class RepositorioJuego : IDisposable`, con constructor interno y Dispose público. Esta porción declara catálogo, credenciales, sesiones, apertura y ejecución; los métodos privados son compartidos por las otras porciones. Los records `internal sealed record PartidaCreada(Guid Id,string CredencialJ1,string CredencialJ2)`, `internal sealed record Incorporacion(Guid IdSesion,string? Nombre)` y `internal sealed record Sesion(string Rol,Guid IdSesion,DateTimeOffset VenceUtc)` generan constructor, propiedades públicas init e igualdad por valor. No tienen lógica adicional ni guardas propias.

### Estado y datos

Campos privados readonly: `string cadena` (conexión); `SqliteConnection? memoria` (solo se abre para DataSource exactamente `:memory:`); `TimeProvider reloj`; `object sincronizacion = new()`; `Dictionary<(Guid,string),Sesion> sesiones = []`. Readonly impide reasignar el contenedor, no mutar el diccionario. Las claves son partida/rol y no se persisten; reiniciar pierde sesiones, mientras nombres, hashes, turnos y recibos quedan en SQLite. Las credenciales en claro se devuelven al crear/reemplazar, pero solo su hash se guarda.

### Miembros y ejecución

#### Apertura, transacción y recursos

`internal RepositorioJuego(string cadena, TimeProvider reloj)` conserva argumentos, interpreta la cadena y, si DataSource es `:memory:`, abre una conexión ancla para que la base sobreviva a los contextos. Luego crea un contexto temporal y llama EnsureCreated; no migra esquema existente. Errores de cadena, permisos o SQLite se propagan.

`private BaseJuego Abrir()` construye opciones EF: UseSqlite(cadena) para archivo o UseSqlite(memoria) para conexión ancla; devuelve contexto nuevo, aún sin ejecutar consultas. `public void Dispose()` desecha la conexión ancla si existe; para archivos no tiene conexión permanente que cerrar.

`private T Ejecutar<T>(bool escritura, Func<BaseJuego,T> operacion)` adquiere lock del repositorio, abre contexto y conexión, inicia transacción con `deferred: !escritura`, asocia EF, ejecuta el delegado, hace SaveChanges solo en escritura, confirma y devuelve resultado. Escritura reserva bloqueo SQLite antes de leer lo que modificará. Excepción antes de Commit sale mediante using y revierte la transacción; no retorna un resultado parcial. Lock de C# es reentrante, por eso puede llamarse desde otros bloques sobre el mismo objeto. Fragmento completo:

```csharp
    private T Ejecutar<T>(bool escritura, Func<BaseJuego, T> operacion)
    {
        lock (sincronizacion)
        {
            using var db = Abrir();
            db.Database.OpenConnection();
            // La escritura adquiere el bloqueo de SQLite antes de leer el estado que va a modificar.
            using var transaccion = ((SqliteConnection)db.Database.GetDbConnection())
                .BeginTransaction(deferred: !escritura);
            db.Database.UseTransaction(transaccion);
            var resultado = operacion(db);
            if (escritura)
                db.SaveChanges();
            transaccion.Commit();
            return resultado;
        }
    }

```

No hay await dentro del lock. El delegado no debe escapar con un contexto vivo; el contexto queda desechado al retornar.

#### Catálogo

`internal int DarDeAlta(AltaPersonaje datos)` valida antes de la transacción. Dentro calcula el máximo ID positivo (cero si no existe); si es int.MaxValue falla 409 operacionIncompatible. Asigna max+1, agrega entidad y retorna ID después del commit. No comprueba duplicado de nombre en alta individual.

`internal int CantidadCatalogo()` ejecuta lectura Count de todos, incluidos inactivos.

`internal int ReemplazarCatalogo(AltaPersonaje[] datos)` rechaza array vacío; valida cada alta en orden con Select, asigna IDs índice+1, rechaza nombres repetidos tras Trim con comparación OrdinalIgnoreCase. ErrorApi durante esa preparación se envuelve con índice desde 1 y nombre. ToArray fuerza validación completa antes de borrar. Luego, en una única transacción de escritura, ExecuteDelete borra catálogo, AddRange inserta todos y retorna cantidad después de SaveChanges/Commit. Si falla inserción, rollback recupera catálogo anterior; las copias PersonajePartida no se borran. Fragmento decisivo:

```csharp
        return Ejecutar(true, db =>
        {
            db.Catalogo.ExecuteDelete();
            db.Catalogo.AddRange(personajes);
            return personajes.Length;
        });
```

`internal void Desactivar(int id)` busca entidad por ID o falla 404; pone Activo false y confirma. Repetirlo conserva false; no altera copias ya repartidas.

#### Administración de partidas y credenciales

`internal PartidaCreada CrearPartida()` genera Guid y dos secretos antes de escribir. Agrega PartidaGuardada (fase esperandoInicio, revisión cero) y dos participantes J1/J2 con hashes; retorna ID y ambos secretos tras commit. No inicia ni copia cartas.

`internal void CerrarSesion(Guid id,string rol)` dentro de lock exige participante existente mediante lectura, luego elimina presencia en memoria. No cambia revisión ni hash; rol ausente produce 404 antes de borrar.

`internal string ReemplazarCredencial(Guid id,string rol)` genera secreto nuevo, guarda su hash sobre participante existente en transacción y luego retira la sesión; retorna nuevo secreto. Si falla guardado no elimina sesión ni devuelve secreto válido.

`internal void Cancelar(Guid id)` busca partida o 404; si ya finalizada falla 409 operacionIncompatible. Incrementa revisión y llama Finalizar con motivo cancelacion sin ganador. No borra plazas ni historial.

#### Incorporación y presencia

`internal Sesion Incorporar(Guid id,string? credencial,Incorporacion datos)` rechaza IdSesion Empty o nombre null/blanco con 400 antes del lock. Dentro exige partida, autentica plaza, consulta sesión vigente; una vigente con UUID distinto falla 409 sesionOcupada. Si ya hay nombre registrado y no coincide exactamente (incluidos espacios/caso), falla 403 sinPermiso. Si no existe, asigna nombre. Crea sesión que vence en reloj UTC+15s, confirma transacción y recién entonces actualiza diccionario. Misma sesión vigente puede renovarse mediante incorporación; una vencida permite nuevo UUID con mismo nombre. No incrementa revisión.

`internal void Renovar(Guid id,string? credencial,Guid idSesion)` dentro de lock exige partida, credencial y sesión vigente exacta. Después sustituye record por `with { VenceUtc = ahora+15s }`; no persiste sesión ni cambia revisión. Datos inválidos no prolongan presencia.

#### Ayudantes privados

- `static ParticipanteGuardado BuscarParticipante(BaseJuego db,Guid id,string rol)`: Find compuesto o ErrorApi.NoEncontrado (404).
- `static PartidaGuardada Buscar(BaseJuego db,Guid id)`: Find de partida o 404.
- `static ParticipanteGuardado Autenticar(BaseJuego db,Guid id,string? credencial)`: si no es null/blanco, calcula hash y recorre participantes de esa partida, devolviendo primero con FixedTimeEquals. Si ninguna coincidencia, 401 credencialInvalida. No recorta secreto ni revela qué plaza falló.
- `Sesion? Vigente(Guid id,string rol)`: devuelve sesión del diccionario solo si VenceUtc > reloj UTC actual; igualdad exacta ya está vencida. No elimina entradas vencidas.
- `Sesion ExigirSesion(Guid id,string rol,Guid idSesion)`: exige Vigente y UUID igual o 409 sesionVencida.
- `static byte[] Hash(string credencial)`: SHA256 de bytes UTF8 exactos.
- `static string NuevaCredencial()`: 32 bytes criptográficos a hexadecimal (64 caracteres); no usa Random.Shared.

Fragmento real de sesión vigente y errores:

```csharp
    private Sesion? Vigente(Guid id, string rol) =>
        sesiones.TryGetValue((id, rol), out var sesion) && sesion.VenceUtc > reloj.GetUtcNow()
            ? sesion : null;

    private Sesion ExigirSesion(Guid id, string rol, Guid idSesion) =>
        Vigente(id, rol) is { } sesion && sesion.IdSesion == idSesion
            ? sesion : throw new ErrorApi(409, "sesionVencida", "Se requiere una sesión vigente.");
```

#### Fragmentos de autenticación y mutaciones

El constructor conserva una conexión solo para `:memory:`; mantenerla abierta evita que cada contexto vea una base nueva. EnsureCreated usa ese mismo origen:

```csharp
        if (new SqliteConnectionStringBuilder(cadena).DataSource == ":memory:")
        {
            memoria = new SqliteConnection(cadena);
            memoria.Open();
        }

        using var db = Abrir();
        db.Database.EnsureCreated();
```

DarDeAlta calcula un ID positivo nuevo bajo transacción; agotar int32 produce conflicto en vez de desbordar:

```csharp
            int mayor = db.Catalogo.Where(p => p.Id > 0).Select(p => (int?)p.Id).Max() ?? 0;
            if (mayor == int.MaxValue)
                throw new ErrorApi(409, "operacionIncompatible", "Se agotaron los IDs de alta automática.");
            personaje.Id = mayor + 1;
            db.Catalogo.Add(personaje);
            return personaje.Id;
```

Antes de ReemplazarCatalogo, el Select se materializa con ToArray: valida y asigna IDs antes del bloque que borra. Cada error señala personaje y nombre:

```csharp
        if (datos.Length == 0) throw LotePersonajes.Invalido("La lista no puede estar vacía.");
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var personajes = datos.Select((dato, indice) =>
        {
            try
            {
                var personaje = dato.Validar();
                if (!nombres.Add(personaje.Nombre.Trim())) throw LotePersonajes.Invalido("nombre duplicado.");
                personaje.Id = indice + 1;
                return personaje;
            }
            catch (ErrorApi error)
            {
                throw LotePersonajes.Invalido($"Personaje {indice + 1} ({dato.Nombre}): {error.Message}");
            }
        }).ToArray();
```

CrearPartida agrega partida y ambas plazas en la misma operación; las credenciales en claro `j1`/`j2` solo forman el valor de retorno y sus hashes se persisten:

```csharp
        var id = Guid.NewGuid();
        var j1 = NuevaCredencial();
        var j2 = NuevaCredencial();
        return Ejecutar(true, db =>
        {
            db.Partidas.Add(new PartidaGuardada { Id = id });
            db.Participantes.AddRange(
                new ParticipanteGuardado { IdPartida = id, Rol = "J1", Hash = Hash(j1) },
                new ParticipanteGuardado { IdPartida = id, Rol = "J2", Hash = Hash(j2) });
            return new PartidaCreada(id, j1, j2);
```

ReemplazarCredencial solo borra sesión después de confirmar el hash nuevo. Si falla el guardado, no ejecuta Remove:

```csharp
            var nueva = NuevaCredencial();
            Ejecutar(true, db => BuscarParticipante(db, id, rol).Hash = Hash(nueva));
            sesiones.Remove((id, rol));
            return nueva;
```

Cancelar rechaza una partida ya cerrada antes del incremento y usa el helper compartido de finalización:

```csharp
        if (partida.Fase == "finalizada")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.");
        partida.Revision++;
        Finalizar(db, partida, "cancelacion");
```

Incorporar distingue plaza ocupada de nombre distinto y guarda el nombre una sola vez. El diccionario de sesiones se actualiza tras la transacción:

```csharp
                var actual = Vigente(id, participante.Rol);
                if (actual is not null && actual.IdSesion != datos.IdSesion)
                    throw new ErrorApi(409, "sesionOcupada", "La plaza tiene una sesión vigente.");
                if (participante.Nombre is not null && participante.Nombre != datos.Nombre)
                    throw new ErrorApi(403, "sinPermiso", "El nombre registrado debe coincidir.");

                participante.Nombre ??= datos.Nombre;
                return new Sesion(participante.Rol, datos.IdSesion, reloj.GetUtcNow().AddSeconds(15));
            });
            sesiones[(id, sesion.Rol)] = sesion;
            return sesion;
```

La renovación exige acceso vigente y sustituye únicamente vencimiento de presencia; no cambia estado de partida:

```csharp
                Buscar(db, id);
                var jugador = Autenticar(db, id, credencial);
                return ExigirSesion(id, jugador.Rol, idSesion);
            });
            sesiones[(id, sesion.Rol)] = sesion with { VenceUtc = reloj.GetUtcNow().AddSeconds(15) };
```

Autenticar recorre solo plazas de la partida pedida y compara hashes con tiempo fijo; un secreto null o blanco cae directamente en 401:

```csharp
        if (!string.IsNullOrWhiteSpace(credencial))
        {
            var hash = Hash(credencial);
            foreach (var jugador in db.Participantes.Where(p => p.IdPartida == id))
            {
                if (CryptographicOperations.FixedTimeEquals(jugador.Hash, hash))
                    return jugador;
            }
        }

        throw new ErrorApi(401, "credencialInvalida", "La credencial no es válida.");
```

### Recorridos y ejemplos

Normal: CrearPartida produce P y secretos ficticios representados aquí como C1/C2; Incorporar(P,C1,{S1,"Ana"}) guarda Ana y presencia hasta t+15; Renovar a t+10 mueve vencimiento a t+25 sin cambiar revisión. Reiniciar conserva Ana/hash pero exige nueva incorporación porque sesiones está vacío.

Alternativo: a t+14, UUID S2 sobre la misma plaza falla sesionOcupada; a t+15 exacto la sesión anterior ya venció y S2 puede incorporarse si nombre coincide. ReemplazarCatalogo con dos nombres `Ada`/` ada ` falla antes del borrado. Un fallo de inserción tras ExecuteDelete revierte todo. Recorridos por lectura, no operaciones contra servicios reales.

### Errores, límites y efectos observables

Catalogo, nombres y hashes son persistentes; presencia solo en memoria. No hay cancelación asíncrona ni reintento automático de SQLite. Null en datos o array no está defendido por estos contratos internos. El lock serializa esta instancia, no otros procesos; transacciones SQLite protegen almacenamiento. EnsureCreated no sustituye migraciones. Dispose no limpia sesiones individualmente ni elimina la base.

### Relaciones y verificación

[ApiHttpTests](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md) define incorporación, reloj, credencial reemplazada, rollback y reinicio. [CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md) define reemplazo repetido sin acumulación, validación previa, datos exactos y rollback por fallo de inserción. [ServidorContratoTests](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md) ejercita procesos reales. Consultar [plan de pruebas](plan-de-pruebas.md) para ejecución real; la descripción de los casos no prueba que se hayan ejecutado aquí.

</details>

<details>
<summary>src/servidor/Api/ServidorHttp.cs</summary>

[Página individual](archivos/src/servidor/Api/ServidorHttp.md)

## src/servidor/Api/ServidorHttp.cs

Fuente: [src/servidor/Api/ServidorHttp.cs](../src/servidor/Api/ServidorHttp.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Construye el host ASP.NET Core, configura JSON y dependencias, traduce errores a respuestas y registra rutas del protocolo. No arranca la escucha: [Program](archivos/src/servidor/Program.md) llama StartAsync. [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md) es el dueño de las transacciones y autenticación de plazas.

### Dependencias

ASP.NET Core aporta WebApplication, middleware, grupos de rutas, DI y Results. EF Core/SQLite clasifican errores de almacenamiento. SHA256/UTF8 y FixedTimeEquals comprueban secreto administrativo; AuthenticationHeaderValue parsea Bearer. [ConfiguracionServidor](archivos/src/servidor/Api/ConfiguracionServidor.md) suministra inicio; [LotePersonajes](archivos/src/servidor/Api/LotePersonajes.md), [EstadoPartida](archivos/src/servidor/Api/EstadoPartida.md) y [PresentacionPartida](archivos/src/servidor/Api/PresentacionPartida.md) definen entradas/salidas. [Protocolo](protocolo-de-partida.md) es la guía operativa compartida.

### Estructura y estado

`internal sealed class ErrorApi(int estado,string codigo,string mensaje) : Exception(mensaje)` conserva propiedades internas get-only `int Estado` y `string Codigo`, e hereda Message. Declara fábricas y Responder. `internal static class ServidorHttp` declara Crear, Credencial e IdSesion. Crear contiene función local ExigirCatalogo y lambdas de configuración, middleware y handlers. No hay partial, eventos ni campos estáticos propios.

### Miembros y ejecución

#### ErrorApi

El constructor primario guarda estado/código y pasa mensaje a Exception. `internal static ErrorApi Entrada()` crea 400/entradaInvalida; `NoEncontrado()` 404/recursoNoEncontrado; `Indisponible()` 503/servicioNoDisponible. No lanzan por sí mismas. `internal Task Responder(HttpContext contexto)` fija StatusCode y retorna WriteAsJsonAsync con Codigo, mensaje=Message e idSeguimiento=TraceIdentifier; no incluye stack ni credenciales. La configuración JSON web serializa Codigo como `codigo`.

#### `internal static WebApplication Crear(string[] args,TimeProvider? reloj = null,ConfiguracionServidor? inicio = null)`

Crea builder con args. Si inicio no null agrega configuración en memoria: urls, cadena SQLite con DataSource=inicio.Archivo y DefaultTimeout=1 segundo, Catalogo:Credencial; además UseUrls. Si configuración urls sigue null usa http://0.0.0.0:5000. Registra repositorio singleton con cadena configurada o `Data Source=guerra.db;Default Timeout=1`, y TimeProvider suministrado o System. La lambda de DI obtiene reloj al materializar repositorio; no crea una instancia por petición. JSON NumberHandling.Strict impide convertir strings a números. ThrowOnBadRequest permite traducir fallos de binding a error común.

Tras Build agrega middleware asíncrono: await siguiente; captura ErrorApi y responde con sus datos; BadHttpRequestException → 400; SqliteException → 503; DbUpdateException solo si InnerException es SqliteException → 503. Otras excepciones no se convierten aquí. Configura rutas y devuelve app sin StartAsync.

#### Autenticación y funciones auxiliares

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

#### Rutas y lambdas de handlers

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

#### Fragmentos de configuración, traducción y efectos HTTP

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

### Recorridos y ejemplos

Normal: GET resumen con secreto ficticio correcto devuelve cantidad 6; PUT con seis objetos válidos devuelve cantidad 6 y reemplaza en una transacción. Una elección nueva devuelve 201, reenviar mismo cuerpo y UUID devuelve 200 con el mismo recibo.

Alternativo: secreto configurado vacío causa 401 incluso al consultar resumen. Una incorporación con X-Sesion inválido todavía puede proceder porque usa idSesion del cuerpo; una presentación con ese X-Sesion falla sesionVencida. Un puntaje JSON `"650"` es string y falla binding estricto antes del handler de alta. Ejemplos contrastados por lectura y pruebas definidas, sin petición real aquí.

### Errores, límites y efectos observables

No redirige ni configura HTTPS/autorización de operador remotamente. Credencial viaja en cabecera Bearer, no en URL ni cuerpo de error. No hay rate limiting, paginación ni captura universal de excepciones. Configuración de host puede recibir otras fuentes cuando se llama Crear sin inicio; el ejecutable de servidor usa arreglo args vacío e inicio interactivo. La identidad de sesión no es una credencial independiente y siempre se valida junto a Bearer.

### Relaciones y verificación

[ApiHttpTests](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md) define rutas, códigos, privacidad y binding estricto. [CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md) define autenticación y atomicidad HTTP del catálogo. [ServidorContratoTests](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md) define protocolos por proceso. Consultar [plan de pruebas](plan-de-pruebas.md) para resultados reales; estas definiciones no acreditan ejecución de esta página.

</details>

<details>
<summary>src/servidor/Api/TransicionesPartida.cs</summary>

[Página individual](archivos/src/servidor/Api/TransicionesPartida.md)

## src/servidor/Api/TransicionesPartida.cs

Fuente: [src/servidor/Api/TransicionesPartida.cs](../src/servidor/Api/TransicionesPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Implementa la máquina de estados persistida de partidas en red: inicio, elecciones, combate, rondas, victoria y abandono. Es una porción de `internal sealed partial RepositorioJuego`; [RepositorioJuego.cs](archivos/src/servidor/Api/RepositorioJuego.md) aporta conexión, transacciones, autenticación y sesiones; [PresentacionPartida](archivos/src/servidor/Api/PresentacionPartida.md) publica vistas. No usa el bucle incompleto de Partida.Jugar: reconstruye cada Combate para aplicar sus reglas.

### Dependencias

[BaseJuego](archivos/src/servidor/Api/BaseJuego.md) persiste claves/relaciones; [EstadoPartida](archivos/src/servidor/Api/EstadoPartida.md) declara las entidades y contratos. [Combate](archivos/src/servidor/Combate.md), [Jugador](archivos/src/servidor/Jugador.md), [Personaje](archivos/src/servidor/Personaje.md), [Ronda](archivos/src/servidor/Ronda.md) y [Partida](archivos/src/servidor/Partida.md) aportan comparación y constantes de 2 combates/15 rondas. Random.Shared.Shuffle mezcla copias. [Protocolo](protocolo-de-partida.md) define solicitudes, revisión e idempotencia.

### Estructura y estado

No declara otro tipo ni constructores, eventos o interfaces. Campo privado estático readonly `Dictionary<string,TipoAtributo> atributos`, con OrdinalIgnoreCase, contiene Fuerza, Velocidad, Resistencia, Inteligencia, Energía y Habilidad mapeados al enum. Readonly no vuelve inmutable el diccionario, aunque esta implementación no lo modifica tras inicializar. Los demás estados viven en entidades y en las sesiones de la otra porción; no hay campos de ronda en memoria en este archivo.

### Miembros y ejecución

#### `internal void Iniciar(Guid id)`

Ejecuta transacción de escritura. Busca partida o 404; exige fase esperandoInicio (si no, 409 operacionIncompatible), luego ambos presentes (409 partidaPausada). Copia solo catálogo activo con PersonajePartida.Copiar; menos de seis falla 409 operacionIncompatible antes de agregar copias. Agrega las cartas, reparte ronda 1 mano J1, incrementa revisión una vez y confirma. Las copias aíslan partidas iniciadas de cambios futuros del catálogo.

#### `internal Confirmacion Elegir(Guid id,string? credencial,Guid idSesion,Eleccion datos)`

Antes de escribir rechaza IdOperacion/IdSolicitud Empty, RevisionEsperada null o negativa e IdPersonaje null con 400 entradaInvalida. Normaliza atributo. Dentro de transacción llama Acceso; consulta operación previa por `(id,rol,idOperacion)` **antes** de evaluar pausa/revisión/turno. Si existe y Datos difiere de la elección normalizada, falla 409 operacionIncompatible; si coincide retorna recibo previo con Repetida true, sin consumir nada ni incrementar revisión.

Para primera operación verifica en este orden: no Pausada (409 partidaPausada); revisión igual (409 revisionDesactualizada); IdSolicitud igual al puntero actual, fila existente, no consumida y Rol propio (409 solicitudNoVigente). Busca carta por partida e ID; exige ubicación propia. Si tipo cartaYAtributo exige atributo no nulo y existente en el mapa; al responder carta exige atributo null. Cualquiera de estos errores produce 422 eleccionInvalida, sin cambiar estado.

Luego marca solicitud Consumida e incrementa revisión. Elector: guarda IdCartaElector y Atributo, abre solicitud carta al otro. Respondedor: llama Resolver. En ambos crea OperacionGuardada con entrada normalizada y revisión posterior, añade al contexto y retorna Confirmacion con Repetida false. SaveChanges y commit se realizan junto al avance por Ejecutar; un fallo revierte recibo y jugada. Fragmento real de la prioridad de idempotencia:

```csharp
            var anterior = db.Operaciones.Find(id, jugador.Rol, datos.IdOperacion);
            if (anterior is not null)
            {
                if (anterior.Datos() != datos)
                    throw new ErrorApi(409, "operacionIncompatible", "La operación ya tiene otros datos.");
                return new Confirmacion(anterior.Recibo(), true);
            }
```

La comparación de records incluye operación, solicitud, revisión esperada, ID de carta y atributo. Un reintento idéntico sigue siendo recuperable después de avanzar turno o finalizar, siempre que credencial/sesión sean válidas.

#### Consulta y abandono

`internal Recibo ConsultarRecibo(Guid id,string? credencial,Guid idSesion,Guid idOperacion)` ejecuta lectura, exige Acceso y busca solo operación de la plaza propia. Devuelve Recibo o 404; no renueva presencia ni cambia revisión.

`internal void Abandonar(Guid id,string? credencial,Guid idSesion)` ejecuta escritura y exige Acceso. Si AbandonoDe ya es ese rol retorna sin cambios, incluso si la partida finalizó por ese abandono. Si finalizó por otra causa/rol, 409 operacionIncompatible. En otro caso guarda AbandonoDe, incrementa revisión y Finalizar con motivo abandono, sin ganador. No transforma una salida local en victoria del rival.

#### Acceso, presencia y normalización

- `private (PartidaGuardada Partida,ParticipanteGuardado Jugador) Acceso(BaseJuego db,Guid id,string? credencial,Guid idSesion)`: Busca, Autenticar, ExigirSesion, en ese orden; retorna la tupla. No renueva sesión.
- `private bool AmbosPresentes(Guid id)`: exige Vigente de J1 y J2; el && evita consultar J2 si falta J1.
- `private bool Pausada(PartidaGuardada partida)`: true solo en esperandoElector/esperandoRival cuando no están ambos; preparación/finalizada nunca aparecen pausadas por esta regla. No guarda flag ni incrementa revisión.
- `private static string Otro(string rol)`: J1 → J2; cualquier otro string → J1. Solo se llama con roles internos válidos.
- `private static string? NormalizarAtributo(string? atributo)`: null sigue null; si no, Trim y Normalize (forma Unicode predeterminada). Energia sin tilde, en cualquier caso, pasa a Energía. Busca nombre canónico del diccionario ignorando caso; desconocido conserva texto normalizado para posterior rechazo. No convierte cadena vacía a null.

#### Reparto y apertura

`private static void Repartir(BaseJuego db,PartidaGuardada partida,PersonajePartida[] cartas,int numero,string mano)` mezcla todo el array. For desde 0 hasta Length−1 asigna Orden=i y Ubicacion J1 para i<3, J2 para 3≤i<6 y mazo para el resto. Agrega ronda con Mano, asigna NumeroRonda y abre combate 1 con ese elector. No consulta catálogo ni añade rondas ganadas. Precondición interna: al menos seis cartas; Iniciar la garantiza y las rondas siguientes reutilizan el conjunto completo.

`private static void AbrirCombate(BaseJuego db,PartidaGuardada partida,int numero,string elector)` crea fila usando NumeroRonda no nulo, agrega, asigna NumeroCombate y abre solicitud cartaYAtributo del elector. `private static void AbrirSolicitud(BaseJuego db,PartidaGuardada partida,CombateGuardado combate,string rol,string tipo)` crea Guid nuevo, copia coordenadas, rol y tipo, deja Consumida false; agrega solicitud, actualiza IdSolicitud y pone esperandoRival si tipo carta, esperandoElector en cualquier otro caso.

#### Resolución y finalización

`private static void Resolver(BaseJuego db,PartidaGuardada partida,CombateGuardado guardado,PersonajePartida cartaRival)` busca carta del elector, crea dos Jugador con nombres J1/J2, convierte cartas al dominio y las entrega a cada jugador temporal. Mapea atributo, crea Combate, fija atributo y ambas cartas y resuelve. Usa nombre del ganador como rol; empate gana rival del elector según Combate. Guarda carta rival, ambos puntajes y ganador, y ubica ambas cartas en jugada.

Incrementa victorias de combate de J1 o J2 en RondaGuardada. Si quedan por debajo de 2, abre combate siguiente con elector alternado y retorna. Si llega a 2, guarda ganador de ronda y suma una ronda al participante; si alcanza 15, Finalizar(victoria,ganador) y retorna **sin repartir otra ronda**. En otro caso carga todas las cartas de la partida y Repartir con número siguiente y mano opuesta. No incrementa revisión adicional: Elegir ya lo hizo una vez por elección.

Fragmento real que sustenta cierre de combate/ronda/partida:

```csharp
        var ronda = db.Rondas.Find(partida.Id, guardado.NumeroRonda)!;
        var victorias = ganador == "J1" ? ++ronda.VictoriasJ1 : ++ronda.VictoriasJ2;
        if (victorias < Ronda.CombatesParaGanar)
        {
            AbrirCombate(db, partida, guardado.Numero + 1, Otro(guardado.Elector));
            return;
        }

        ronda.Ganador = ganador;
        var jugador = db.Participantes.Find(partida.Id, ganador)!;
        if (++jugador.RondasGanadas >= Partida.RondasParaGanar)
        {
            Finalizar(db, partida, "victoria", ganador);
            return;
        }

        var cartas = db.Cartas.Where(p => p.IdPartida == partida.Id).ToArray();
        Repartir(db, partida, cartas, ronda.Numero + 1, Otro(ronda.Mano));
```

`private static void Finalizar(BaseJuego db,PartidaGuardada partida,string motivo,string? ganador = null)` consume solicitud actual si existe, deja IdSolicitud null, fase finalizada y motivo/ganador. No borra historial, no incrementa revisión ni entrega cartas; cada llamador gestiona su incremento. Usa null-forgiving en búsquedas internas porque presupone integridad de transiciones.

#### Fragmentos de las decisiones y efectos restantes

En `Iniciar`, estos guardas preceden al reparto y evitan crear copias si falta presencia o catálogo. Tras agregar/repartir, la revisión sube una vez:

```csharp
        var partida = Buscar(db, id);
        if (partida.Fase != "esperandoInicio")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya se inició o finalizó.");
        if (!AmbosPresentes(id))
            throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");

        // Cada partida conserva su propia copia de los personajes activos al iniciar.
        var cartas = db.Catalogo.Where(p => p.Activo).ToArray()
            .Select(p => PersonajePartida.Copiar(id, p)).ToArray();
        if (cartas.Length < 6)
            throw new ErrorApi(409, "operacionIncompatible", "Se necesitan seis personajes activos.");
        db.Cartas.AddRange(cartas);
        Repartir(db, partida, cartas, 1, "J1");
        partida.Revision++;
```

En `Elegir`, las validaciones de primera operación preceden a toda mutación. `esElector` selecciona la regla de atributo: obligatorio al abrir y prohibido al responder:

```csharp
            if (Pausada(partida))
                throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");
            if (partida.Revision != datos.RevisionEsperada)
                throw new ErrorApi(409, "revisionDesactualizada", "La revisión ya cambió.");
            var solicitud = partida.IdSolicitud == datos.IdSolicitud
                ? db.Solicitudes.Find(datos.IdSolicitud) : null;
            if (solicitud is null || solicitud.Consumida || solicitud.Rol != jugador.Rol)
                throw new ErrorApi(409, "solicitudNoVigente", "La solicitud no está vigente para esta plaza.");

            var carta = db.Cartas.Find(id, datos.IdPersonaje.Value);
            bool esElector = solicitud.Tipo == "cartaYAtributo";
            if (carta is null || carta.Ubicacion != jugador.Rol ||
                (esElector ? datos.Atributo is null || !atributos.ContainsKey(datos.Atributo) : datos.Atributo is not null))
                throw new ErrorApi(422, "eleccionInvalida", "La carta o el atributo no está entre las opciones.");
```

La solicitud se consume y la revisión sube antes de bifurcar apertura/respuesta. La fila de operación se añade en la misma transacción; por eso respuesta perdida no exige volver a consumir la solicitud:

```csharp
            var combate = db.Combates.Find(id, solicitud.NumeroRonda, solicitud.NumeroCombate)!;
            solicitud.Consumida = true;
            partida.Revision++;
            if (esElector)
            {
                combate.IdCartaElector = carta.IdPersonaje;
                combate.Atributo = datos.Atributo;
                AbrirSolicitud(db, partida, combate, Otro(jugador.Rol), "carta");
            }
            else
            {
                Resolver(db, partida, combate, carta);
            }

            // El recibo se guarda en la misma transacción que la jugada que confirma.
            var operacion = new OperacionGuardada
            {
                IdPartida = id, Rol = jugador.Rol, IdOperacion = datos.IdOperacion,
                IdSolicitud = datos.IdSolicitud, RevisionEsperada = datos.RevisionEsperada.Value,
                IdPersonaje = carta.IdPersonaje, Atributo = datos.Atributo,
                RevisionConfirmada = partida.Revision
            };
            db.Operaciones.Add(operacion);
            return new Confirmacion(operacion.Recibo(), false);
```

En `Abandonar`, la repetición del mismo rol se acepta antes del control de fase finalizada; el incremento corresponde solo a la primera aceptación:

```csharp
        if (partida.AbandonoDe == jugador.Rol)
            return true;
        if (partida.Fase == "finalizada")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.");
        partida.AbandonoDe = jugador.Rol;
        partida.Revision++;
        Finalizar(db, partida, "abandono");
```

La normalización del atributo no transforma desconocidos en una opción válida. Devuelve el nombre canónico o el texto para que la validación posterior lo rechace:

```csharp
        if (atributo is null)
            return null;
        var texto = atributo.Trim().Normalize();
        if (texto.Equals("Energia", StringComparison.OrdinalIgnoreCase))
            return "Energía";
        return atributos.Keys.FirstOrDefault(a => a.Equals(texto, StringComparison.OrdinalIgnoreCase)) ?? texto;
```

El reparto reasigna **todas** las ubicaciones y el orden. Esto reincorpora cartas jugadas y no jugadas al empezar otra ronda:

```csharp
        Random.Shared.Shuffle(cartas);
        for (int i = 0; i < cartas.Length; i++)
        {
            cartas[i].Orden = i;
            cartas[i].Ubicacion = i < 3 ? "J1" : i < 6 ? "J2" : "mazo";
        }
        db.Rondas.Add(new RondaGuardada { IdPartida = partida.Id, Numero = numero, Mano = mano });
        partida.NumeroRonda = numero;
        AbrirCombate(db, partida, 1, mano);
```

`AbrirSolicitud` crea una oportunidad nueva (UUID distinto) y cambia fase según tipo; no reutiliza una solicitud consumida:

```csharp
        var solicitud = new SolicitudGuardada
        {
            Id = Guid.NewGuid(), IdPartida = partida.Id, NumeroRonda = combate.NumeroRonda,
            NumeroCombate = combate.Numero, Rol = rol, Tipo = tipo
        };
        db.Solicitudes.Add(solicitud);
        partida.IdSolicitud = solicitud.Id;
        partida.Fase = tipo == "carta" ? "esperandoRival" : "esperandoElector";
```

En Resolver, cartas temporales se convierten a dominio y el resultado se materializa antes de sumar la ronda. Los dos IDs y puntajes permiten publicar resultados completos, sin revelar elección antes de resolver:

```csharp
        var cartaElector = db.Cartas.Find(partida.Id, guardado.IdCartaElector!.Value)!;
        var elector = new Jugador(guardado.Elector);
        var rival = new Jugador(Otro(guardado.Elector));
        var personajeElector = cartaElector.Dominio();
        var personajeRival = cartaRival.Dominio();
        elector.RecibirCartas([personajeElector]);
        rival.RecibirCartas([personajeRival]);

        var atributo = atributos[guardado.Atributo!];
        var combate = new Combate(elector, rival);
        combate.SetAtributo(atributo);
        combate.SetCarta(elector, personajeElector);
        combate.SetCarta(rival, personajeRival);
        var ganador = combate.Resolver().GetNombre();

        guardado.IdCartaRival = cartaRival.IdPersonaje;
        guardado.PuntajeElector = personajeElector.GetAtributo(atributo);
        guardado.PuntajeRival = personajeRival.GetAtributo(atributo);
        guardado.Ganador = ganador;
        cartaElector.Ubicacion = cartaRival.Ubicacion = "jugada";
```

Finalizar retira la solicitud vigente y no toca revisión; así cancelación, abandono y victoria mantienen el incremento decidido por su llamador:

```csharp
        if (partida.IdSolicitud is Guid id)
            db.Solicitudes.Find(id)!.Consumida = true;
        partida.IdSolicitud = null;
        partida.Fase = "finalizada";
        partida.MotivoFin = motivo;
        partida.Ganador = ganador;
```

### Recorridos y ejemplos

Normal: tras iniciar seis cartas, revisión pasa 0→1 y J1 recibe solicitud S1. J1 elige carta propia A/Fuerza con revisión 1: se consume S1, revisión 2, solicitud S2 carta para J2. J2 responde B sin atributo, revisión 2: revisión 3 y combate resuelto; si A y B tienen fuerza 650 ambos, gana J2. Si J2 llevaba una victoria de combate suma la segunda y gana ronda. Si llevaba 14 rondas alcanza 15 y finaliza con victoria, sin otra solicitud.

Alternativo: reenviar exactamente operación de J1 recupera su recibo con revisión confirmada 2 aunque la partida esté en 3; cambiar solo la carta usando mismo IdOperacion falla operacionIncompatible. Primera operación con revisión 1 cuando actual es 3 falla revisionDesactualizada; mismo reintento auténtico no. Responder con atributo Fuerza falla 422. Recorridos de lectura, no peticiones ejecutadas en esta página.

### Errores, límites y efectos observables

Todas las mutaciones se agrupan con recibo en transacción. Se rechaza turno equivocado sin consumir solicitud. Presencia vencida impide Acceso de esa sesión; ausencia del otro jugador pausa nuevas elecciones. La carta del elector conserva ubicación propia hasta resolver, pero su solicitud ya fue consumida y no permite otra jugada. Los resultados aún incompletos no se publican por PresentacionPartida. No hay bot ni recuperación de esquema corrupto; los `!` no son comprobaciones en ejecución.

### Relaciones y verificación

[ApiHttpTests](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md) define privacidad/idempotencia, rollback, reinicio, quince rondas y plazo de presencia. [ServidorContratoTests](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md) define elecciones concurrentes, pausa y recuperación de solicitud, desempates y abandono repetido. [IntegracionRespuestaPerdidaTests](archivos/test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.md) define recuperación del recibo tras pérdida de respuesta. Consultar [plan](plan-de-pruebas.md) para ejecución real; esta lectura no acredita tests ejecutados.

</details>

<details>
<summary>src/servidor/Combate.cs</summary>

[Página individual](archivos/src/servidor/Combate.md)

## src/servidor/Combate.cs

Fuente: [src/servidor/Combate.cs](../src/servidor/Combate.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Consume una carta de cada participante, fija un atributo una sola vez y resuelve una comparación. Modela un combate local; no incorpora sesiones HTTP, persistencia ni revisiones de solicitudes. La ronda registra su ganador, pero este objeto no incrementa el marcador.

### Dependencias

[Jugador](archivos/src/servidor/Jugador.md) conserva la mano y permite retirar referencias; [Personaje y TipoAtributo](archivos/src/servidor/Personaje.md) proporcionan los puntajes. `Enum.IsDefined` rechaza valores de enum sin nombre; `ReferenceEquals` distingue participantes por identidad, no por nombre. Véanse [reglas](reglas-del-juego.md) y la coordinación distinta de la [API](archivos/src/servidor/Api/TransicionesPartida.md).

### Estructura

`internal class Combate`, sin base explícita, interfaces, atributos ni partial. No hay propiedades, eventos, operadores, funciones locales ni sobrecargas. Métodos públicos salvo los tres ayudantes de validación y registro.

### Estado y datos

| Campo privado | Ciclo de vida |
|---|---|
| `readonly Jugador jugadorQueElige`, `jugadorRival` | Se asignan al construir y no cambian. Deben ser referencias distintas no nulas. |
| `TipoAtributo? atributoElegido` | Empieza null, una elección válida lo fija para siempre. `?` permite distinguir Fuerza (0) de ausencia. |
| `Personaje? cartaQueElige`, `cartaRival` | Empiezan null; cada participante registra una carta que se retira de su mano. |
| `Jugador? ganador` | Null hasta Resolver; luego referencia del vencedor. |

No hay bloqueos ni reversión de cartas consumidas. Los signos `!` en lecturas posteriores silencian advertencias de nulabilidad después de comprobar completitud; no son validaciones nuevas.

### Miembros y ejecución

- `public Combate(Jugador quienElige, Jugador rival)`: comprueba null de ambos en orden mediante `ArgumentNullException`, luego igualdad por referencia (`ArgumentException`). Solo después guarda participantes. Dos jugadores con igual nombre pero distinta instancia se aceptan.
- `public void SetAtributo(TipoAtributo tipo)`: si ya existe atributo, lanza `InvalidOperationException` antes de inspeccionar tipo. Si es primera elección y `Enum.IsDefined(tipo)` es false, lanza `ArgumentOutOfRangeException`. En otro caso asigna. No exige que las cartas ya estén presentes.
- `public void SetCarta(Jugador jugador, Personaje personaje)`: rechaza primero jugador null y después personaje null; llama a ValidarParticipante y ValidarCartaDisponible. Solo si todas pasan, retira la carta mediante QuitarCarta y registra la referencia. No retorna resultado y no exige elegir atributo primero.
- `private void ValidarParticipante(Jugador jugador)`: si no es ninguna de las dos referencias, lanza `ArgumentException` con parámetro jugador; si coincide, no modifica nada.
- `private void ValidarCartaDisponible(Jugador jugador, Personaje personaje)`: selecciona el campo de carta según sea el elector. Si ya tiene carta, lanza `InvalidOperationException`. Si aún no tiene pero `TieneCarta` es false, lanza `ArgumentException` con parámetro personaje. La precedencia evita consumir una segunda carta incluso si está en su mano.
- `private void RegistrarCarta(Jugador jugador, Personaje personaje)`: si es elector asigna cartaQueElige y retorna inmediatamente; de lo contrario asigna cartaRival. Se llama después de validar que el participante pertenece al combate.
- `public bool EstaCompleto()`: true únicamente con atributo y ambas cartas; no exige que ya se resolviera. Sigue siendo true después de Resolver.
- `public Jugador Resolver()`: comprueba completitud y luego que ganador siga null; cada fallo lanza `InvalidOperationException`. Lee ambos valores del atributo; elector gana solo si su valor es estrictamente mayor. Menor o igual da ganador al rival. Guarda y retorna vencedor. No suma rondas ni devuelve cartas.
- `public Jugador GetGanador()`: retorna ganador; null produce `InvalidOperationException` sin cambiar estado.
- `public TipoAtributo GetAtributo()`: retorna atributo o lanza `InvalidOperationException` si aún no fue fijado, aunque ya existan cartas.
- `public Personaje GetCarta(Jugador jugador)`: null produce `ArgumentNullException`. Luego exige combate completo (`InvalidOperationException`), sin exigir resuelto. Devuelve la carta de la referencia coincidente; un ajeno produce `ArgumentException`. Por ese orden un ajeno en combate incompleto recibe primero el error de completitud.
- `public Jugador[] GetParticipantes()`: array nuevo con elector y rival, en ese orden. Reemplazar posiciones no cambia los campos; los jugadores siguen siendo objetos compartidos.

El orden de las validaciones de SetCarta evita consumo en los rechazos:

```csharp
            ArgumentNullException.ThrowIfNull(jugador);
            ArgumentNullException.ThrowIfNull(personaje);

            ValidarParticipante(jugador);
            ValidarCartaDisponible(jugador, personaje);

            jugador.QuitarCarta(personaje);
            RegistrarCarta(jugador, personaje);
```

En Resolver, tras sus guardas de completitud y resolución única, el código real compara:

```csharp
            int valorElector = cartaQueElige!.GetAtributo(atributoElegido!.Value);
            int valorRival = cartaRival!.GetAtributo(atributoElegido.Value);

            // Si los valores empatan, gana el rival de quien eligió el atributo.
            ganador = valorElector > valorRival
                ? jugadorQueElige
                : jugadorRival;

            return ganador;
```

#### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: guardas de referencias y asignaciones.

```csharp
        public Combate(Jugador quienElige, Jugador rival)
        {
            ArgumentNullException.ThrowIfNull(quienElige);
            ArgumentNullException.ThrowIfNull(rival);

            if (ReferenceEquals(quienElige, rival))
                throw new ArgumentException(
                    "Los participantes deben ser distintos."
                );

            jugadorQueElige = quienElige;
            jugadorRival = rival;
        }
```

SetAtributo: prioridad de repetición frente a enum inválido.

```csharp
        public void SetAtributo(TipoAtributo tipo)
        {
            if (atributoElegido.HasValue)
                throw new InvalidOperationException(
                    "El atributo ya fue elegido."
                );

            if (!Enum.IsDefined(tipo))
                throw new ArgumentOutOfRangeException(nameof(tipo));

            atributoElegido = tipo;
        }
```

ValidarParticipante: identidad exacta de alguna plaza.

```csharp
        private void ValidarParticipante(Jugador jugador)
        {
            if (!ReferenceEquals(jugador, jugadorQueElige) &&
                !ReferenceEquals(jugador, jugadorRival))
            {
                throw new ArgumentException(
                    "El jugador no participa en este combate.",
                    nameof(jugador)
                );
            }
        }
```

ValidarCartaDisponible: segunda carta se rechaza antes de consultar disponibilidad.

```csharp
        private void ValidarCartaDisponible(Jugador jugador, Personaje personaje)
        {
            bool esElector = ReferenceEquals(jugador, jugadorQueElige);
            Personaje? cartaRegistrada = esElector ? cartaQueElige : cartaRival;

            if (cartaRegistrada != null)
                throw new InvalidOperationException(
                    "El jugador ya registró su carta."
                );

            if (!jugador.TieneCarta(personaje))
                throw new ArgumentException(
                    "La carta no se encuentra en la mano del jugador.",
                    nameof(personaje)
                );
        }
```

RegistrarCarta: asignación por participante con retorno temprano.

```csharp
        private void RegistrarCarta(Jugador jugador, Personaje personaje)
        {
            if (ReferenceEquals(jugador, jugadorQueElige))
            {
                cartaQueElige = personaje;
                return;
            }

            cartaRival = personaje;
        }
```

GetCarta: primero completitud y después identidad.

```csharp
        public Personaje GetCarta(Jugador jugador)
        {
            ArgumentNullException.ThrowIfNull(jugador);

            if (!EstaCompleto())
                throw new InvalidOperationException(
                    "El combate todavía no está completo."
                );

            if (ReferenceEquals(jugador, jugadorQueElige))
                return cartaQueElige!;

            if (ReferenceEquals(jugador, jugadorRival))
                return cartaRival!;

            throw new ArgumentException(
                "El jugador no participa en este combate.",
                nameof(jugador)
            );
        }
```

### Recorridos y ejemplos

Normal: Ana tiene A con fuerza 750 y Bruno B con fuerza 650. Construir Combate(Ana,Bruno), fijar Fuerza y registrar A y B deja ambas manos sin esas cartas; EstaCompleto devuelve true. Resolver retorna Ana, GetGanador retorna la misma referencia y los contadores de rondas siguen iguales.

Alternativo: con fuerza 650 en ambas cartas gana Bruno porque es rival del elector. Registrar una carta de un tercer Jugador llamado Ana falla por identidad sin retirarla. Registrar dos cartas del elector falla antes de retirar la segunda. Resolver sin carta rival falla sin ganador; repetir Resolver después de un éxito falla y conserva el ganador anterior. Estos recorridos se contrastaron por lectura; la ejecución de suite se registra aparte.

### Errores, límites y efectos observables

Las cartas se retiran al registrarlas, no al resolver; un combate abandonado no las restaura por sí mismo. No limita combates por ronda ni comprueba orden de turnos de una sesión. No sincroniza accesos concurrentes; no hay transacción que cubra retirar y registrar. Una carta repetida en una mano se retira solo una vez. Las restricciones de puntajes pertenecen a Personaje.

### Relaciones y verificación

[Ronda](archivos/src/servidor/Ronda.md) cuenta vencedores; [Partida](archivos/src/servidor/Partida.md) recoge las referencias de cartas por participante. [CombateTests](archivos/test/proyecto-integrador-final-tests/CombateTests.md) define guardas, consulta prematura, una carta por jugador, completitud, identidad, copia del array, comparación y empate; algunas aserciones solo exigen que exista una excepción, no su tipo ni mensaje. El [plan de pruebas](plan-de-pruebas.md) distingue evidencia histórica de ejecución actual. Esta explicación no acredita ejecución concurrente ni de la API a partir de este modelo.

</details>

<details>
<summary>src/servidor/Jugador.cs</summary>

[Página individual](archivos/src/servidor/Jugador.md)

## src/servidor/Jugador.cs

Fuente: [src/servidor/Jugador.cs](../src/servidor/Jugador.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Representa el nombre, la mano y el contador de rondas de un participante del modelo local. Lo usan `Combate`, `Ronda` y `Partida`; no representa la sesión HTTP ni una fila de SQLite. Sus operaciones son síncronas y solo cambian memoria.

### Dependencias

[Personaje](archivos/src/servidor/Personaje.md) es el elemento de la mano. `List<T>` permite agregar y quitar; LINQ `ToArray` produce copias de la colección. `ArgumentException` y `ArgumentNullException` rechazan entradas inválidas. Véanse las [reglas](reglas-del-juego.md) para distinguir rondas de combates.

### Estructura

`internal class Jugador`, sin base explícita, interfaces, atributos ni porciones parciales. Sus métodos son públicos dentro de la visibilidad efectiva del tipo. No declara funciones locales, operadores ni eventos.

### Estado y datos

| Campo privado | Inicialización y cambios |
|---|---|
| `readonly string nombre` | Constructor; se conserva literalmente, sin recortar espacios. No se reasigna. |
| `readonly List<Personaje> mano` | Lista vacía; readonly impide reemplazarla, pero no agregar o borrar elementos. |
| `int rondasGanadas` | Cero; se incrementa sin límite de quince dentro de este tipo. |

Los elementos son referencias compartidas a personajes; las copias de array no clonan esos personajes. No hay sincronización para acceso concurrente.

### Miembros y ejecución

- `public Jugador(string nombre)`: primero rechaza null, vacío o solo espacios con `ArgumentException` y parámetro `nombre`. Si acepta, asigna nombre, crea mano vacía y deja contador cero. No exige nombre único.
- `public void RecibirCartas(Personaje[] personajes)`: null produce `ArgumentNullException` antes de mutar. `AddRange` agrega todas las referencias al final, conservando orden. No limita tamaño, comprueba elementos nulos ni evita repetidos. Array vacío no cambia la mano.
- `public void QuitarCarta(Personaje personaje)`: rechaza null y llama a `Remove`. Quita la primera coincidencia; como Personaje no redefine igualdad, coincide por referencia. Si no existe, no hace nada y no informa un bool al llamador.
- `public Personaje[] EntregarCartas()`: copia toda la mano al array de retorno y vacía la lista. Una segunda llamada devuelve array vacío. No afecta el marcador.
- `public Personaje[] GetCartas()`: devuelve copia de la colección actual sin vaciarla; modificar una posición del array no modifica la lista.
- `public void SumarRondaGanada()`: incrementa el entero en uno; no valida fin de partida ni asigna ganador.
- `public int GetRondasGanadas()` y `public string GetNombre()`: devuelven los campos correspondientes sin efectos secundarios.
- `public bool TieneCarta(Personaje personaje)`: rechaza null; devuelve `mano.Contains(personaje)`, por referencia; no consume la carta.

La secuencia decisiva de `EntregarCartas` es real:

```csharp
        public Personaje[] EntregarCartas()
        {
            Personaje[] cartas = this.mano.ToArray();
            this.mano.Clear();

            return cartas;
        }
```

La copia sucede antes del vaciado, por eso el llamador puede devolver las cartas al mazo sin perderlas. La lectura sin consumo usa:

```csharp
        public Personaje[] GetCartas() => this.mano.ToArray();

        public void SumarRondaGanada() => this.rondasGanadas++;

        public int GetRondasGanadas() => this.rondasGanadas;

        public string GetNombre() => this.nombre;
```

#### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: rechazo de nombre y estado inicial.

```csharp
        public Jugador(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre del jugador no puede estar vacío.",
                    nameof(nombre)
                );

            this.nombre = nombre;
            this.mano = new List<Personaje>();
            this.rondasGanadas = 0;
        }
```

RecibirCartas: se valida el contenedor y se agrega en orden.

```csharp
        public void RecibirCartas(Personaje[] personajes)
        {
            ArgumentNullException.ThrowIfNull(personajes);
            this.mano.AddRange(personajes);
        }
```

QuitarCarta: null se rechaza y Remove solo retira una coincidencia.

```csharp
        public void QuitarCarta(Personaje personaje)
        {
            ArgumentNullException.ThrowIfNull(personaje);
            this.mano.Remove(personaje);
        }
```

TieneCarta: null no equivale a carta ausente.

```csharp
        public bool TieneCarta(Personaje personaje)
        {
            ArgumentNullException.ThrowIfNull(personaje);
            return this.mano.Contains(personaje);
        }
```

### Recorridos y ejemplos

Normal: `new Jugador("Ana")` tiene cero cartas y rondas. Recibir `[A,B]` y después `[C]` deja `[A,B,C]`; quitar B deja `[A,C]`; entregar devuelve `[A,C]` y deja mano vacía. Dos llamadas a sumar dejan marcador 2.

Alternativo: quitar D cuando solo están A y C deja ambas referencias intactas. `RecibirCartas(null)` lanza antes de agregar; `new Jugador("   ")` no produce instancia válida. Estos recorridos se deducen por lectura; no son evidencia de ejecución propia de esta página.

### Errores, límites y efectos observables

Solo hay efectos en memoria. El tipo permite manos mayores a tres y contadores mayores a quince; son responsabilidades del coordinador. No devuelve cartas automáticamente al quitar: Combate guarda la referencia para recogerla al cerrar la ronda. Un array de entrada con elementos nulos no queda validado por la guarda del array.

### Relaciones y verificación

`JugadorTests.IniciaConNombreManoVaciaYSinVictorias` comprueba nombre, mano inicial y dos incrementos. `AcumulaCartasQuitaUnaYEntregaElResto` verifica acumulación, copias defensivas, retirada y entrega repetida. [Fuente de pruebas](archivos/test/proyecto-integrador-final-tests/JugadorTests.md). No cubren explícitamente todas las guardas nulas ni concurrencia. [Plan de pruebas](plan-de-pruebas.md): consultar allí evidencia histórica; esta página no implica que se haya ejecutado la suite de esta versión.

</details>

<details>
<summary>src/servidor/Mazo.cs</summary>

[Página individual](archivos/src/servidor/Mazo.md)

## src/servidor/Mazo.cs

Fuente: [src/servidor/Mazo.cs](../src/servidor/Mazo.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Contiene la reserva de cartas del modelo local: copia la lista inicial, mezcla, retira un prefijo y agrega devoluciones al final. `Partida` lo utiliza antes de repartir tres cartas a cada jugador y al recoger la ronda. No carga el catálogo ni persiste cambios.

### Dependencias

Los elementos son [Personaje](archivos/src/servidor/Personaje.md). `List<T>` mantiene el orden; `Take`/`ToArray` de LINQ construyen el array repartido; `Random.Shared` proporciona el generador compartido. Véanse las [reglas](reglas-del-juego.md).

### Estructura

`internal class Mazo`, sin herencia explícita, interfaces, atributos ni partial. No tiene eventos, propiedades, operadores ni funciones locales. Todos los métodos declarados son públicos.

### Estado y datos

`private List<Personaje> personajes` se asigna con `new List<Personaje>(personajes)` en el constructor. Copia el contenedor, no las instancias; limpiar la lista de origen no vacía el mazo. El campo no es readonly, aunque los métodos actuales no lo reasignan. El orden cambia por mezcla, retiro y devolución. No hay bloqueo concurrente.

### Miembros y ejecución

- `public Mazo(List<Personaje> personajes)`: copia elementos en orden; una lista null falla mediante el constructor de `List<T>`. Admite lista vacía, duplicados y referencias nulas sin inspeccionarlos.
- `public void Mezclar()`: parte de Count. Mientras n > 1 decrementa n, sortea k entre 0 y n inclusive e intercambia ambas posiciones. Termina tras Count−1 intercambios para Count ≥ 2; tamaños 0 y 1 no sortean. Mantiene tamaño y multiplicidad. No acepta semilla ni garantiza un orden distinto al original.
- `public Personaje[] Repartir(int cantidad)`: primero calcula `Take(cantidad).ToArray()`, después ejecuta `RemoveRange(0, cantidad)` y solo entonces retorna el array. Para 0 ≤ cantidad ≤ Count devuelve y elimina ese prefijo. Para cantidad negativa `Take` no aporta elementos pero `RemoveRange` lanza `ArgumentOutOfRangeException`; si supera Count, Take arma un array con lo disponible pero RemoveRange lanza `ArgumentException`, sin devolverlo al llamador ni retirar elementos. No aplica límite de tres por sí mismo.
- `public void Devolver(Personaje[] personajes)`: `AddRange` agrega en el orden recibido al final. Null lanza `ArgumentNullException` desde la biblioteca; un array vacío no cambia nada. No comprueba que las cartas procedan de este mazo.
- `public int GetCantidad()`: devuelve Count actual sin mutar.

Fragmento completo del bucle de `Mezclar`:

```csharp
            int n = this.personajes.Count;
            while (n > 1)
            {
                n--;
                // Genera un índice aleatorio entre 0 y n (inclusive)
                int k = Random.Shared.Next(n + 1);

                // Intercambia los elementos usando tuplas de C#
                (this.personajes[k], this.personajes[n]) = (this.personajes[n], this.personajes[k]);
            }
```

El límite exclusivo de `Next(n + 1)` permite k = n; intercambiar una posición consigo misma es válido. Es una mezcla de Fisher–Yates. `Repartir` conserva este orden y no vuelve a mezclar:

```csharp
        public Personaje[] Repartir(int cantidad)
        {
            Personaje[] repartidos = this.personajes.Take(cantidad).ToArray();
            this.personajes.RemoveRange(0, cantidad);

            return repartidos;
        }
```

#### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: copia del contenedor inicial. Se omite la firma y las llaves del constructor en este fragmento.

```csharp
            this.personajes = new List<Personaje>(personajes);
```

Devolver: anexado al final sin deduplicación.

```csharp
        public void Devolver(Personaje[] personajes)
        {
            this.personajes.AddRange(personajes);
        }
```

### Recorridos y ejemplos

Normal sin mezcla: reserva `[A,B,C,D]`, repartir 2 retorna `[A,B]` y deja `[C,D]`; devolver `[A,B]` deja `[C,D,A,B]`; Count es 4. Mezclar conserva las cuatro referencias pero su posición posterior no se predice.

Error: con dos cartas, repartir 3 lanza antes del retorno y deja Count 2. No hay reparto parcial entregado al llamador. Repartir 0 retorna vacío y conserva todo. Ejemplos contrastados por lectura, no ejecutados como comandos en esta página.

### Errores, límites y efectos observables

Solo altera la reserva en memoria. No asegura seis cartas al inicio de partida: esa guarda corresponde a Partida. No registra las cartas prestadas, y una devolución repetida duplica referencias. La aleatoriedad impide usar un orden específico como aserción de mezcla; debe comprobarse conservación de elementos.

### Relaciones y verificación

[Jugador](archivos/src/servidor/Jugador.md) recibe las cartas retiradas. `MazoTests.CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal` comprueba independencia de la lista, dos repartos de tres y devolución al final. `MezclarConservaTodasLasCartas` parametriza 0, 1 y 6 y exige cada referencia exactamente una vez. [Pruebas fuente](archivos/test/proyecto-integrador-final-tests/MazoTests.md). No ejercitan cantidades inválidas, null ni duplicados; las limitaciones anteriores provienen de lectura y semántica de List/LINQ. Véase el [plan de pruebas](plan-de-pruebas.md); definir pruebas no acredita ejecución en este worktree.

</details>

<details>
<summary>src/servidor/Partida.cs</summary>

[Página individual](archivos/src/servidor/Partida.md)

## src/servidor/Partida.cs

Fuente: [src/servidor/Partida.cs](../src/servidor/Partida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Es el coordinador local del modelo de clases: prevé repartir, jugar rondas, recoger cartas y terminar en quince victorias. En la versión documentada **no puede completar una partida**: JugarRonda crea un combate pero no fija atributo ni cartas y lanza al comprobar completitud. El servidor de red usa [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md) y [TransicionesPartida](archivos/src/servidor/Api/TransicionesPartida.md), no este bucle. Este límite se describe sin cambiar código ni interpretar el comentario de actividad 6 como funcionalidad disponible.

### Dependencias

[Jugador](archivos/src/servidor/Jugador.md) mantiene manos y marcador, [Mazo](archivos/src/servidor/Mazo.md) mezcla y reparte, [Ronda](archivos/src/servidor/Ronda.md) alterna electores y cuenta victorias, [Combate](archivos/src/servidor/Combate.md) compara cartas. `List<Personaje>` reúne cartas al recoger. No depende de HttpClient, consola ni SQLite. Véanse [arquitectura](arquitectura.md) y [reglas](reglas-del-juego.md).

### Estructura

`internal class Partida`, sin herencia explícita, interfaces, atributos ni partial. Constructor y Jugar públicos; los seis ayudantes restantes son privados. No declara propiedades, eventos, sobrecargas, funciones locales ni operadores. `internal const int RondasParaGanar = 15` fija el umbral.

### Estado y datos

Campos privados readonly `Jugador j1`, `Jugador j2`, `Mazo mazo` se asignan al construir. `Ronda? rondaActual` empieza null; `int numeroRonda` empieza 0. Iniciar fija número 1 y ronda con j1 como mano. El diseño posterior alternaría mano por paridad, pero no se alcanza en el flujo actual con seis o más cartas y jugadores distintos. No resetea los marcadores previos de los jugadores ni sus manos al comenzar.

### Miembros y ejecución

#### `public Partida(Jugador j1, Jugador j2, Mazo mazo)`

Rechaza null en j1, j2 y mazo, en ese orden, con `ArgumentNullException`; después guarda referencias y estado inicial. No rechaza j1 y j2 iguales aquí: con mazo suficiente fallará al construir Ronda en Iniciar. No copia los objetos; pueden ser compartidos externamente.

#### `public Jugador Jugar()`

Llama a Iniciar antes de entrar a `while (true)`. El orden previsto por iteración es JugarRonda, RecogerCartas y EstaTerminada. Si terminó retorna DeterminarGanadorPartida; si no incrementa numeroRonda, elige j1 para número impar o j2 para par, obtiene el otro rival por ReferenceEquals, crea nueva Ronda y reparte otra vez. El bucle no tiene cancelación ni máximo de rondas; terminaría por marcador. En esta versión la primera llamada a JugarRonda lanza, de modo que el retorno exitoso no se produce por la API pública normal.

#### `private void Iniciar()`

Si GetCantidad < 6, lanza `InvalidOperationException` con «No se puede jugar porque no hay suficientes cartas.» **antes** de cambiar número, ronda, mazo o manos. Con cantidad suficiente fija número 1, crea Ronda(j1,j2) y llama RepartirCartas. Si Ronda rechaza referencias iguales, número ya es 1 pero no hay reparto.

#### `private void RepartirCartas()`

Mezcla todo el mazo, retira tres para j1 y después tres para j2; recién entonces agrega ambos arrays a las manos. No limpia cartas previas. La aleatoriedad pertenece al Mazo y no puede configurarse aquí. No hay transacción ni rollback si alguna operación excepcional falla a mitad.

#### `private void JugarRonda()`

Mientras la ronda no esté definida obtiene elector, determina rival como el otro jugador y construye Combate. El comentario anuncia selección futura, pero no existe ninguna llamada a SetAtributo ni SetCarta. Por ello EstaCompleto es false y lanza «El combate no se completó.». Las líneas posteriores Resolver y AgregarCombate, así como obtener ganador y sumar una ronda al final del bucle, expresan el flujo previsto pero no se alcanzan con el estado que crea este método.

Fragmento real de JugarRonda que demuestra el límite:

```csharp
                Combate combate = new Combate(quienElige, rival);

                // La selección del atributo y de las cartas se completará
                // mediante la comunicación con los clientes en la actividad 6.

                if (!combate.EstaCompleto())
                    throw new InvalidOperationException(
                        "El combate no se completó."
                    );

                combate.Resolver();
                this.rondaActual.AgregarCombate(combate);
```

#### `private void RecogerCartas()`

Prevé recorrer combates en orden y, para cada uno, sus dos participantes, agregando GetCarta de cada jugador a una lista. Luego EntregarCartas vacía la mano de j1 y agrega sus sobrantes; hace lo mismo con j2; finalmente devuelve todo al Mazo. No mezcla durante recogida. Los bucles terminan por tamaño de las colecciones. Requiere ronda no nula y combates completos; la supresión `!` no evita una excepción si se incumple esa precondición. En el flujo actual no llega a ejecutarse tras el error de JugarRonda.

#### `private bool EstaTerminada()`

Retorna si j1 o j2 tiene al menos quince rondas ganadas. Cortocircuita el OR: con j1 ≥15 no consulta j2. No modifica estado.

#### `private Jugador DeterminarGanadorPartida()`

Devuelve j1 si su contador ≥15; si no, devuelve j2 sin comprobar su contador. Su precondición prevista es haber obtenido true en EstaTerminada. Si ambos tuvieran quince, priorizaría j1. No se expone públicamente.

La guarda previa a repartir de Iniciar es:

```csharp
            if (this.mazo.GetCantidad() < 6)
                throw new InvalidOperationException(
                    "No se puede jugar porque no hay suficientes cartas."
                );
```

#### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: referencias y estado inicial, sin validar igualdad de jugadores.

```csharp
        public Partida(Jugador j1, Jugador j2, Mazo mazo)
        {
            ArgumentNullException.ThrowIfNull(j1);
            ArgumentNullException.ThrowIfNull(j2);
            ArgumentNullException.ThrowIfNull(mazo);

            this.j1 = j1;
            this.j2 = j2;
            this.mazo = mazo;
            this.rondaActual = null;
            this.numeroRonda = 0;
        }
```

Jugar: secuencia prevista y punto de salida por marcador; el fallo de JugarRonda impide recorrerla por completo.

```csharp
        public Jugador Jugar()
        {
            this.Iniciar();

            while (true)
            {
                this.JugarRonda();
                this.RecogerCartas();

                if (this.EstaTerminada())
                    return this.DeterminarGanadorPartida();

                this.numeroRonda++;

                Jugador mano = this.numeroRonda % 2 == 1
                    ? this.j1
                    : this.j2;

                Jugador rival = ReferenceEquals(mano, this.j1)
                    ? this.j2
                    : this.j1;

                this.rondaActual = new Ronda(mano, rival);
                this.RepartirCartas();
            }
        }
```

Iniciar: insuficiencia se detecta antes de modificar estado.

```csharp
        private void Iniciar()
        {
            if (this.mazo.GetCantidad() < 6)
                throw new InvalidOperationException(
                    "No se puede jugar porque no hay suficientes cartas."
                );

            this.numeroRonda = 1;
            this.rondaActual = new Ronda(this.j1, this.j2);

            this.RepartirCartas();
        }
```

RepartirCartas: extrae ambos grupos antes de entregarlos.

```csharp
        private void RepartirCartas()
        {
            this.mazo.Mezclar();

            Personaje[] cartasJ1 = this.mazo.Repartir(3);
            Personaje[] cartasJ2 = this.mazo.Repartir(3);

            this.j1.RecibirCartas(cartasJ1);
            this.j2.RecibirCartas(cartasJ2);
        }
```

RecogerCartas: recorrido de jugadas y vaciado de sobrantes, no alcanzado en el flujo público actual.

```csharp
        private void RecogerCartas()
        {
            List<Personaje> cartas = new List<Personaje>();

            foreach (Combate combate in this.rondaActual!.GetCombates())
            {
                foreach (Jugador jugador in combate.GetParticipantes())
                    cartas.Add(combate.GetCarta(jugador));
            }

            cartas.AddRange(this.j1.EntregarCartas());
            cartas.AddRange(this.j2.EntregarCartas());

            this.mazo.Devolver(cartas.ToArray());
        }
```

EstaTerminada: umbral con OR de cortocircuito.

```csharp
        private bool EstaTerminada()
        {
            return this.j1.GetRondasGanadas() >= RondasParaGanar
                || this.j2.GetRondasGanadas() >= RondasParaGanar;
        }
```

DeterminarGanadorPartida: prioridad de j1 bajo su precondición.

```csharp
        private Jugador DeterminarGanadorPartida()
        {
            return this.j1.GetRondasGanadas() >= RondasParaGanar
                ? this.j1
                : this.j2;
        }
```

### Recorridos y ejemplos

Recorrido de entrada válida hasta el límite real: jugadores distintos con manos vacías y seis cartas válidas. Jugar establece ronda 1, mezcla, entrega tres a cada jugador y deja mazo vacío; luego lanza «El combate no se completó.». Las manos conservan sus tres cartas porque no se registró ninguna ni se alcanzó RecogerCartas; los marcadores no cambian. No se ofrece un ejemplo de victoria local, porque ese resultado no está implementado en este coordinador.

Error temprano: con cinco cartas, Jugar lanza por insuficiencia antes de repartir; mazo conserva cinco y manos vacías. Con cero cartas sucede lo mismo. Estos dos tamaños están definidos en PartidaTests; el recorrido de seis cartas se contrastó por lectura y no se confunde con ensayo de partida REST.

### Errores, límites y efectos observables

No es seguro reintentar Jugar suponiendo que hubo rollback: el error posterior al reparto deja cartas en manos y reduce el mazo. No valida manos iniciales vacías ni marcadores cero. No hay persistencia, sesiones, revisión idempotente, entrada de usuario ni cancelación; esas responsabilidades corresponden a la implementación de red separada.

### Relaciones y verificación

[PartidaTests](archivos/test/proyecto-integrador-final-tests/PartidaTests.md) prueba exclusivamente mazos de 0 y 5, texto exacto del error y conservación de mazo/manos. No demuestra victoria de este coordinador ni ejecución de sus ayudantes posteriores al fallo. Las pruebas de servidor de red verifican otro coordinador. Véase el [plan de pruebas](plan-de-pruebas.md) para resultados de ejecución, con rama y condiciones.

</details>

<details>
<summary>src/servidor/Personaje.cs</summary>

[Página individual](archivos/src/servidor/Personaje.md)

## src/servidor/Personaje.cs

Fuente: [src/servidor/Personaje.cs](../src/servidor/Personaje.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Representa una carta del modelo local con nombre, arte y seis puntajes. Construirla verifica las reglas numéricas; los getters permiten que Combate compare un atributo. No representa por sí misma el ID de catálogo, la descripción ni el indicador activo de la API.

### Dependencias

Utiliza `System.ArgumentException` y `ArgumentOutOfRangeException`. No requiere servicios, archivos ni configuración. Lo consumen el modelo de combate, las pruebas y la validación del catálogo. Véanse [reglas del juego](reglas-del-juego.md) y [persistencia](bases-de-datos.md) para las restricciones adicionales del catálogo.

### Estructura

`internal enum TipoAtributo` declara, en este orden, `Fuerza = 0`, `Velocidad = 1`, `Resistencia = 2`, `Inteligencia = 3`, `Energia = 4`, `Habilidad = 5` por numeración implícita de C#. No tiene flags ni métodos propios.

`internal class Personaje` no tiene base explícita, interfaces, atributos ni partial. No declara propiedades, eventos, operadores ni funciones locales. Los métodos públicos siguen siendo accesibles solo donde lo permita el tipo interno y la amistad con el ensamblado de pruebas.

### Estado y datos

Ocho campos privados: `string nombre`, `string ascii` y los enteros `fuerza`, `velocidad`, `resistencia`, `inteligencia`, `energia`, `habilidad`. El constructor los asigna; ningún método posterior los modifica, aunque no se declaran readonly. Una instancia construida conserva su estado durante toda su vida. No se recorta ni normaliza texto. El arte vacío se permite; no se comprueban aquí longitud, líneas ni controles de terminal.

### Miembros y ejecución

#### `public Personaje(string nombre, string ascii, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)`

Rechaza primero nombre null, vacío o solo espacios mediante `ArgumentException` con parámetro `nombre`. Después rechaza ASCII null mediante `ArgumentException` con parámetro `ascii`. Copia los ocho argumentos y llama a `Validar`. Una excepción de validación impide devolver una instancia construida. No hay efectos persistidos ni cambios sobre otros objetos.

#### `private void Validar()`

Calcula la suma entera de los seis campos. Recorre un array de seis tuplas `(campo, valor)` en el orden fuerza, velocidad, resistencia, inteligencia, energia, habilidad. Para el primer valor menor a 250 o mayor a 750 lanza `ArgumentException`, con el nombre de ese campo. Si todos están dentro del rango, comprueba en orden: suma exactamente 3000, existencia de un valor ≥650, existencia de uno ≤350. Cada incumplimiento lanza `ArgumentException` con parámetro `atributos`. El primer error interrumpe la validación; no devuelve una lista de errores ni corrige valores.

Fragmento real de la comprobación de suma y fortaleza; el bucle de rangos lo precede y la comprobación de debilidad lo sigue:

```csharp
            if (suma != 3000)
            {
                throw new ArgumentException(
                    "La suma de los atributos debe ser exactamente 3000.", "atributos");
            }

            if (fuerza < 650 && velocidad < 650 &&
                resistencia < 650 && inteligencia < 650 &&
                energia < 650 && habilidad < 650)
            {
                throw new ArgumentException(
                    "Al menos un atributo debe ser mayor o igual a 650.", "atributos");
            }
```

La conjunción exige que fallen los seis valores para rechazar por falta de fortaleza. Basta uno igual a 650 para superar esa comprobación. La condición de debilidad es simétrica: rechaza si los seis son mayores que 350; acepta un 350 exacto.

#### Getters

`public string GetNombre()` y `public string GetAscii()` retornan respectivamente nombre y arte sin transformar ni mutar. `public int GetAtributo(TipoAtributo tipo)` devuelve el campo seleccionado. No itera ni consulta una base de datos:

```csharp
            return tipo switch
            {
                TipoAtributo.Fuerza => fuerza,
                TipoAtributo.Velocidad => velocidad,
                TipoAtributo.Resistencia => resistencia,
                TipoAtributo.Inteligencia => inteligencia,
                TipoAtributo.Energia => energia,
                TipoAtributo.Habilidad => habilidad,

                _ => throw new ArgumentOutOfRangeException(nameof(tipo))
            };
```

El caso `_` cubre enteros convertidos al enum sin una constante declarada: `(TipoAtributo)6` lanza, en vez de devolver cero. El `switch` es una expresión cuyo resultado se retorna directamente.

#### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: las guardas preceden a las asignaciones y a Validar.

```csharp
        public Personaje(
            string nombre,
            string ascii,
            int fuerza,
            int velocidad,
            int resistencia,
            int inteligencia,
            int energia,
            int habilidad)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

            if (ascii == null)
                throw new ArgumentException("El ASCII no puede ser nulo.", nameof(ascii));

            this.nombre = nombre;
            this.ascii = ascii;

            this.fuerza = fuerza;
            this.velocidad = velocidad;
            this.resistencia = resistencia;
            this.inteligencia = inteligencia;
            this.energia = energia;
            this.habilidad = habilidad;

            Validar();
        }
```

Validar: recorrido de rangos y condiciones completas, incluidos umbrales inclusivos.

```csharp
        private void Validar()
        {
            // El total fija el presupuesto de puntos y los umbrales exigen una fortaleza y una debilidad.
            int suma = fuerza + velocidad + resistencia
                     + inteligencia + energia + habilidad;

            foreach (var (campo, valor) in new[]
            {
                (nameof(fuerza), fuerza), (nameof(velocidad), velocidad),
                (nameof(resistencia), resistencia), (nameof(inteligencia), inteligencia),
                (nameof(energia), energia), (nameof(habilidad), habilidad)
            })
            {
                if (valor < 250 || valor > 750)
                    throw new ArgumentException("El atributo debe estar entre 250 y 750.", campo);
            }

            if (suma != 3000)
            {
                throw new ArgumentException(
                    "La suma de los atributos debe ser exactamente 3000.", "atributos");
            }

            if (fuerza < 650 && velocidad < 650 &&
                resistencia < 650 && inteligencia < 650 &&
                energia < 650 && habilidad < 650)
            {
                throw new ArgumentException(
                    "Al menos un atributo debe ser mayor o igual a 650.", "atributos");
            }

            if (fuerza > 350 && velocidad > 350 &&
                resistencia > 350 && inteligencia > 350 &&
                energia > 350 && habilidad > 350)
            {
                throw new ArgumentException(
                    "Al menos un atributo debe ser menor o igual a 350.", "atributos");
            }
        }
```

### Recorridos y ejemplos

Normal: nombre `Carta`, arte vacío y puntajes `(650,350,500,500,500,500)` suman 3000, están en [250,750], tienen fortaleza 650 y debilidad 350. El constructor termina; consultar Fuerza devuelve 650 y Velocidad devuelve 350.

Alternativo: `(500,500,500,500,500,500)` cumple rango y suma pero falla fortaleza. `(650,351,499,500,500,500)` cumple rango, suma y fortaleza, pero todos sus valores superan 350: falla debilidad. `(750,249,501,500,500,500)` se rechaza por velocidad antes de evaluar la suma. Nombre solo espacios falla antes de asignar campos. Ejemplos contrastados por lectura y por los escenarios definidos en las pruebas, sin afirmar su ejecución en esta tarea.

### Errores, límites y efectos observables

No se admite null en nombre ni ASCII, pero los parámetros no anulables son una comprobación estática del compilador, no una barrera de ejecución; por eso hay guardas explícitas. No hay setters, persistencia, cancelación, asincronía ni transacciones. La clase no impone unicidad de nombre, límites de caracteres o tamaño del arte, ni reimplementa igualdad: se compara por referencia cuando se usa en las listas del modelo.

### Relaciones y verificación

[Jugador](archivos/src/servidor/Jugador.md) contiene referencias a las cartas y [Mazo](archivos/src/servidor/Mazo.md) las reparte. Combate consulta el atributo de sus cartas para determinar el resultado numérico; el desempate corresponde a Combate, no a este archivo.

[PersonajeTests.cs](archivos/test/proyecto-integrador-final-tests/PersonajeTests.md) define conservación de nombre/arte/seis atributos, umbrales inclusivos con arte vacío, siete combinaciones inválidas, tres variantes de nombre inválido y ASCII null. Las pruebas comprueban tipo de excepción, no todos sus mensajes ni parámetros; tampoco cubren directamente el enum fuera de rango. [Plan de pruebas](plan-de-pruebas.md): la evidencia histórica no sustituye una ejecución de esta referencia.

</details>

<details>
<summary>src/servidor/Program.cs</summary>

[Página individual](archivos/src/servidor/Program.md)

## src/servidor/Program.cs

Fuente: [src/servidor/Program.cs](../src/servidor/Program.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Es el punto de entrada del ejecutable servidor. Lee configuración, crea el host, arranca HTTP y la consola administrativa, y coordina su cierre. Reintenta el inicio ante errores de dirección/archivo sin cambiar código o persistencia para ocultarlos.

### Dependencias

[ConfiguracionServidor](archivos/src/servidor/Api/ConfiguracionServidor.md) obtiene datos; [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md) construye WebApplication; [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md) se resuelve por DI; [ConsolaServidor](archivos/src/servidor/Api/ConsolaServidor.md) atiende comandos. Microsoft.Data.Sqlite.SqliteException clasifica fallos de base. Task.Run, WhenAny y WaitForShutdownAsync coordinan trabajos. [Instalación y uso](instalacion-y-uso.md) describe ejecución.

### Estructura y estado

`internal static class Program` en namespace global (no declara namespace). Contiene solo `private static async Task<int> Main()`, sin argumentos de aplicación ni campos, propiedades, eventos, partial o constructores de instancia. Los datos de inicio, app, repositorio y tareas son variables locales de cada intento. `await using` libera host al salir del bloque incluso ante excepción.

### Miembros y ejecución

Main retorna código de proceso. Dentro de `while(true)`:

1. Lee configuración desde Console.In/Out. EndOfStreamException imprime «Fin de la entrada. Inicio cancelado.» y retorna 0; IOException retorna 1 sin diagnóstico adicional. Otros fallos de esa primera lectura no se capturan aquí.
2. Crea app mediante ServidorHttp.Crear con array de args vacío e inicio interactivo. Resuelve RepositorioJuego (que abre/crea SQLite) antes de StartAsync.
3. Espera StartAsync; después lanza ConsolaServidor.Ejecutar en Task.Run con token ApplicationStopping. Espera la primera tarea terminada entre consola y espera de cierre del host; luego hace await de esa tarea para propagar su error, llama StopAsync y retorna 0.
4. En el bloque de host captura IOException, UnauthorizedAccessException, SqliteException o InvalidOperationException. Imprime mensaje genérico de revisar dirección/SQLite y repite toda la configuración; no muestra secreto ni detalles internos. No limita intentos: termina por inicio/parada correctos o EOF/error de lectura.

Fragmento real de coordinación de vida del host:

```csharp
                await using var app = ServidorHttp.Crear([], inicio: inicio);
                var juego = app.Services.GetRequiredService<RepositorioJuego>();
                await app.StartAsync();
                var consola = Task.Run(() => ConsolaServidor.Ejecutar(juego, Console.In, Console.Out,
                    app.Lifetime.ApplicationStopping));
                var terminada = await Task.WhenAny(consola, app.WaitForShutdownAsync());
                await terminada;
                await app.StopAsync();
                return 0;
```

La tarea que no ganó WhenAny no se espera explícitamente aquí; StopAsync notifica parada y la disposición asíncrona libera host. ApplicationStopping no fuerza cancelación de una ReadLine ya bloqueada dentro de consola. La lambda de Task.Run solo llama al ejecutor con los flujos y token capturados.

#### Fragmentos de errores y reintento de inicio

EOF durante configuración termina con éxito local sin crear host; un fallo de lectura de consola retorna 1. Estos catch pertenecen al bloque de lectura, no al host:

```csharp
            catch (EndOfStreamException)
            {
                Console.WriteLine("Fin de la entrada. Inicio cancelado.");
                return 0;
            }
            catch (IOException)
            {
                return 1;
            }
```

La captura del host escribe diagnóstico genérico y cae al siguiente ciclo de configuración, sin credenciales ni excepción completa:

```csharp
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or SqliteException or InvalidOperationException)
            {
                Console.WriteLine("No se pudo iniciar el servidor. Revisá la dirección y el archivo SQLite. Volvé a ingresar la configuración.");
            }
```

### Recorridos y ejemplos

Normal: ingresar URL `http://127.0.0.1:5000`, archivo temporal válido y secreto ficticio; tras iniciar, la consola recibe EOF, su tarea termina, Main para el host y devuelve 0. Alternativo: directorio existente pero sin permiso efectivo de crear SQLite pasa la validación de ruta, falla al abrir base, imprime reintento y vuelve a preguntar. EOF en ese segundo intento devuelve 0 sin arrancar otra aplicación. Son recorridos de lectura, no apertura de una base real desde esta página.

### Errores, límites y efectos observables

El arranque puede crear SQLite y escuchar en red. El código no elimina la base al cerrar ni migra esquemas. La captura de InvalidOperationException cubre también errores de operación durante el bloque de host y los presenta con el mismo diagnóstico genérico. No implementa un parser CLI de opciones: Main no recibe args y llama Crear con `[]`.

### Relaciones y verificación

[EntradaProcesosContratoTests](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) define inicio sin argumentos, EOF, parada y corrección tras base inválida; [ServidorContratoTests](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md) usa proceso real y SQLite aislada. Véase [verificación de referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) para comandos/resultados reales y límites del entorno.

</details>

<details>
<summary>src/servidor/Ronda.cs</summary>

[Página individual](archivos/src/servidor/Ronda.md)

## src/servidor/Ronda.cs

Fuente: [src/servidor/Ronda.cs](../src/servidor/Ronda.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Agrupa combates locales y declara ganador al alcanzar dos victorias. Alterna quién elige por cantidad de combates, empezando por mano. No consume cartas, no incrementa marcadores de jugadores ni ejecuta por sí misma un combate.

### Dependencias

[Jugador](archivos/src/servidor/Jugador.md) aporta identidad de mano y rival; [Combate](archivos/src/servidor/Combate.md) aporta ganadores ya resueltos. `List<Combate>` mantiene el orden y `ToArray` permite consultar una copia del contenedor. Véanse las [reglas](reglas-del-juego.md).

### Estructura

`internal class Ronda` sin herencia explícita, interfaces, atributos ni partial. Todos los métodos son públicos; no hay propiedades, eventos, funciones locales ni operadores. `internal const int CombatesParaGanar = 2` establece el umbral en compilación.

### Estado y datos

Campos privados readonly `Jugador mano`, `Jugador rival` y `List<Combate> combates`. El constructor fija los participantes y crea lista vacía. La lista es mutable aunque no pueda reasignarse; sus elementos se comparten con el llamador. No mantiene contador cacheado: cada consulta calcula los triunfos recorriendo los combates.

### Miembros y ejecución

- `public Ronda(Jugador mano, Jugador rival)`: rechaza null de mano y rival en ese orden con `ArgumentNullException`. Si ambas referencias son iguales, `ArgumentException`. Guarda ambas y crea lista; no importa que sus nombres coincidan.
- `public void AgregarCombate(Combate combate)`: null falla primero. Después llama a EstaDefinida; si ya hay dos victorias de un lado, lanza `InvalidOperationException`. Luego rechaza Count ≥ 3 con otra `InvalidOperationException`; si pasa agrega al final. No inspecciona directamente que el combate nuevo esté resuelto ni que sus participantes sean los de esta ronda. Si los anteriores incluyen uno sin resolver, EstaDefinida ya puede lanzar antes del agregado.
- `public bool EstaDefinida()`: inicia contadores locales en cero. Recorre todos los elementos; si GetGanador es la referencia mano suma a mano; cualquier otra referencia suma a rival. Retorna true si algún contador ≥ 2. Un combate no resuelto propaga su excepción. Lista vacía retorna false. No retorna anticipadamente al llegar a dos: sigue recorriendo.
- `public Jugador GetGanador()`: exige EstaDefinida o lanza `InvalidOperationException`. Recuenta victorias de mano; con ≥2 retorna mano y de lo contrario rival. No incrementa rondasGanadas ni cambia la lista.
- `public Jugador GetQuienEligeProximo()`: si EstaDefinida, `InvalidOperationException`; si no, Count par retorna mano, impar rival. Alterna aunque un mismo participante haya ganado dos combates consecutivos; no usa el ganador anterior para elegir.
- `public Combate[] GetCombates()`: copia a array conservando orden; cambiar una posición no modifica la lista. Los objetos Combate no se clonan.

El cuerpo de EstaDefinida muestra tanto la ausencia de validación de identidad rival como la terminación por recorrido completo:

```csharp
            int victoriasMano = 0;
            int victoriasRival = 0;

            foreach (Combate combate in this.combates)
            {
                if (ReferenceEquals(combate.GetGanador(), this.mano))
                    victoriasMano++;
                else
                    victoriasRival++;
            }

            return victoriasMano >= CombatesParaGanar || victoriasRival >= CombatesParaGanar;
```

La selección siguiente, después de la guarda de ronda definida, usa la paridad:

```csharp
            // La elección empieza con el jugador mano y alterna después de cada combate.
            return this.combates.Count % 2 == 0
                ? this.mano
                : this.rival;
```

#### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: referencias distintas y lista vacía.

```csharp
        public Ronda(Jugador mano, Jugador rival)
        {
            ArgumentNullException.ThrowIfNull(mano);
            ArgumentNullException.ThrowIfNull(rival);

            if (ReferenceEquals(mano, rival))
                throw new ArgumentException(
                    "Los jugadores deben ser distintos."
                );

            this.mano = mano;
            this.rival = rival;
            this.combates = new List<Combate>();
        }
```

AgregarCombate: orden de guardas y único efecto de agregado.

```csharp
        public void AgregarCombate(Combate combate)
        {
            ArgumentNullException.ThrowIfNull(combate);

            if (this.EstaDefinida())
                throw new InvalidOperationException(
                    "La ronda ya está definida."
                );

            if (this.combates.Count >= 3)
                throw new InvalidOperationException(
                    "La ronda no puede tener más de tres combates."
                );

            this.combates.Add(combate);
        }
```

GetGanador: verifica definición antes de contar triunfos de mano.

```csharp
        public Jugador GetGanador()
        {
            if (!this.EstaDefinida())
                throw new InvalidOperationException(
                    "La ronda todavía no tiene ganador."
                );

            int victoriasMano = 0;

            foreach (Combate combate in this.combates)
            {
                if (ReferenceEquals(combate.GetGanador(), this.mano))
                    victoriasMano++;
            }

            return victoriasMano >= CombatesParaGanar
                ? this.mano
                : this.rival;
        }
```

### Recorridos y ejemplos

Normal: ronda con mano Ana y rival Bruno. Sin combates abre Ana. Tras un triunfo de Ana abre Bruno. Si Ana gana el segundo, EstaDefinida es true y GetGanador retorna Ana; pedir siguiente elector o agregar un tercero falla porque la ronda terminó.

Alternativo: primero gana Ana y segundo Bruno; Count 2 es par, aún no hay vencedor y abre Ana. El tercer combate da la segunda victoria a alguno y define la ronda. Error adicional: agregar un primer combate incompleto sí puede realizarse porque la lista anterior está vacía; la próxima consulta de definición propaga «El combate todavía no fue resuelto.». Son consecuencias leídas del código; no se presenta ese último caso como prueba ejecutada.

### Errores, límites y efectos observables

El límite tres es defensivo: con dos participantes válidos y tres resultados resueltos, alguno ya tiene dos victorias. El tipo confía en el llamador para participantes y resolución; un ganador ajeno se contabiliza como victoria rival. No hay reversión, persistencia, aleatoriedad, asincronía ni sincronización; solo cambia la lista al agregar.

### Relaciones y verificación

[Partida](archivos/src/servidor/Partida.md) usa el ganador para sumar una ronda al Jugador. [RondaTests](archivos/test/proyecto-integrador-final-tests/RondaTests.md) prepara combates resueltos, verifica comienzo, alternancia, cierre 2–0 para ambos lados, resolución 2–1 y copia defensiva del array. No acredita validación de participantes de cada combate ni rechazo anticipado de combates sin resolver: esas validaciones no existen aquí. La ejecución global se registra en el [plan de pruebas](plan-de-pruebas.md).

</details>

<details>
<summary>src/subir-personajes/Program.cs</summary>

[Página individual](archivos/src/subir-personajes/Program.md)

## src/subir-personajes/Program.cs

Fuente: [src/subir-personajes/Program.cs](../src/subir-personajes/Program.cs). Proyecto: `src/subir-personajes/subir-personajes.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.CargaPersonajes`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Lee un catálogo JSON local y propone reemplazar el catálogo completo del servidor. Consulta cantidad, pide confirmación explícita y envía un único PUT sin reintento automático si el resultado queda incierto. Conserva el formato exacto aceptado, sin asignar IDs ni completar datos faltantes.

### Dependencias

[EntradaConsola](archivos/src/comun/EntradaConsola.md) solicita URL y secreto; HttpClient, HttpRequestMessage y HttpClientHandler hacen HTTP sin redirección automática; System.Text.Json inspecciona JSON; JsonContent y ReadFromJsonAsync serializan/deserializan. [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md) publica GET resumen/PUT; [LotePersonajes](archivos/src/servidor/Api/LotePersonajes.md) y [AltaPersonaje](archivos/src/servidor/Api/PersonajeCatalogo.md) validan servidor. [Guía de la herramienta](../src/subir-personajes/README.md) explica archivo de ejemplo y uso.

### Estructura y estado

Namespace `Proyecto_Integrador_Final.CargaPersonajes`. Tipos `internal static class Program`, `internal static class ConsolaAlta`; dentro de ConsolaAlta, `private sealed record Resumen(int? Cantidad)` genera propiedad pública init, constructor e igualdad. No hay campos compartidos, partial, interfaces explícitas ni eventos. Cliente, documento JSON, mensajes y respuestas tienen vida limitada mediante using. `enviado` es local por envío; no es un recibo persistido.

### Miembros y ejecución

#### Arranque y lectura

`private static Task<int> Program.Main(string[] args)` delega a ConsolaAlta.Iniciar(Console.In,Console.Out,args); no interpreta opciones adicionales.

`internal static async Task<int> Iniciar(TextReader entrada,TextWriter salida,string[]? args = null)` usa solo FirstOrDefault como ruta. Lee/valida archivo **antes** de pedir URL (defecto http://localhost:5000) y credencial no vacía. Crea HttpClient con AllowAutoRedirect false y BaseAddress servidor; llama Enviar y retorna su código. Captura EsErrorEntrada, imprime «No se pudo cargar el archivo: {Message}» y retorna 1; no envía PUT si falla lectura. Un EOF de EntradaConsola es IOException y entra en esa clasificación.

`internal static async Task<int> Ejecutar(HttpClient cliente,string credencial,TextReader entrada,TextWriter salida,string? ruta = null)` es entrada inyectable para pruebas: lee JSON y llama Enviar con cliente existente, sin pedir URL/secreto ni disponer ese cliente prestado. Captura los mismos errores de entrada y retorna 1.

`private static bool EsErrorEntrada(Exception error)` reconoce IOException, UnauthorizedAccessException, ArgumentException, JsonException o FormatException; no captura cualquier excepción indiscriminadamente.

`private static async Task<JsonElement> Leer(string? ruta,TextReader entrada,TextWriter salida)` si ruta null imprime pregunta y ReadLine. Ruta null/blanca falla FormatException. Lee archivo completo asíncronamente, JsonDocument.Parse, ValidarFormato y retorna RootElement.Clone. El clon mantiene datos después de desechar documento. No recorta ruta ni reintenta un archivo inválido.

#### `private static void ValidarFormato(JsonElement raiz)`

Exige array no vacío. Recorre objetos con índice desde 1; tipo distinto de Object falla. Si nombre ya es string lo incorpora a ubicación diagnóstica. Recorre exactamente nueve campos: nombre/descripcion/ascii deben ser String; fuerza/velocidad/resistencia/inteligencia/energia/habilidad Number y TryGetInt32 exitoso. Falta, null, texto numérico o tipo distinto falla FormatException con personaje/campo. Prohíbe id/idPersonaje ignorando caso; nombres duplicados tras Trim con OrdinalIgnoreCase fallan. Campos obligatorios se buscan con minúsculas exactas; otros campos se ignoran. No evalúa rango 250–750, suma, fortaleza/debilidad ni límites del arte: el servidor lo hará antes de reemplazar.

Fragmento real del bucle de forma y duplicados:

```csharp
            foreach (string campo in new[] { "nombre", "descripcion", "ascii", "fuerza", "velocidad", "resistencia", "inteligencia", "energia", "habilidad" })
            {
                bool texto = campo is "nombre" or "descripcion" or "ascii";
                if (!objeto.TryGetProperty(campo, out var valor) || (texto ? valor.ValueKind != JsonValueKind.String : valor.ValueKind != JsonValueKind.Number || !valor.TryGetInt32(out _)))
                    throw new FormatException($"{ubicacion}: {campo} es obligatorio y debe ser {(texto ? "texto" : "entero")}.");
            }
            if (objeto.EnumerateObject().Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("idPersonaje", StringComparison.OrdinalIgnoreCase)))
                throw new FormatException($"{ubicacion}: el ID lo asigna el servidor.");
            if (!nombres.Add(nombre.GetString()!.Trim())) throw new FormatException($"{ubicacion}: nombre duplicado.");
```

#### `private static async Task<int> Enviar(HttpClient cliente,string credencial,TextReader entrada,TextWriter salida,JsonElement datos)`

Empieza enviado=false. Crea GET `/api/v1/personajes/resumen` con Authorization Bearer. Si no es éxito, retorna Rechazo. Lee Resumen; Cantidad null o negativa lanza JsonException. Imprime cantidad actual y número de objetos del archivo. Luego pregunta confirmación en bucle: EOF o `no` ignorando caso cancela, informa sin cambios y retorna 0; `sí` o `si` ignorando caso sale del bucle; otro valor imprime «Respondé sí o no.» y repite. Trim se aplica a respuesta.

Tras sí crea PUT `/api/v1/personajes`, mismo Bearer, JsonContent.Create(datos), y **antes de SendAsync** pone enviado=true. Rechazo HTTP va a Rechazo. Si es éxito, lee Resumen y exige Cantidad igual al tamaño del archivo; de lo contrario JsonException. Imprime catálogo reemplazado y retorna 0. No conserva token de operación ni repite PUT.

Captura HttpRequestException, OperationCanceledException, JsonException e IOException en este envío. Si enviado=false dice que no pudo consultar y no envió reemplazo; si true dice que no pudo confirmar, pudo haberse reemplazado y no reintentará. Retorna 1. Como la bandera se pone antes de SendAsync, puede advertir incertidumbre incluso si la conexión falló antes de transmitir: evita afirmar ausencia de efecto sin evidencia.

Fragmento real del envío y validación de confirmación:

```csharp
            using var solicitud = new HttpRequestMessage(HttpMethod.Put, "/api/v1/personajes");
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credencial);
            solicitud.Content = JsonContent.Create(datos);
            enviado = true;
            using var respuestaCarga = await cliente.SendAsync(solicitud);
            if (!respuestaCarga.IsSuccessStatusCode) return await Rechazo(respuestaCarga, salida);
            var resultado = await respuestaCarga.Content.ReadFromJsonAsync<Resumen>();
            if (resultado?.Cantidad != datos.GetArrayLength()) throw new JsonException("Confirmación inválida.");
            salida.WriteLine($"Catálogo reemplazado. Personajes cargados: {resultado.Cantidad}.");
            return 0;
```

Las rutas empiezan con `/`: se resuelven desde la raíz del servidor, descartando un prefijo de ruta de BaseAddress. Las respuestas y solicitudes se disponen; el HttpClient de Iniciar se dispone al terminar. No hay CancellationToken propio ni timeout configurado distinto del predeterminado de HttpClient.

#### `private static async Task<int> Rechazo(HttpResponseMessage respuesta,TextWriter salida)`

Usa mensaje de credencial inválida para 401 y rechazo genérico para otros estados. Intenta parsear cuerpo como JsonDocument y, si existe propiedad mensaje, usa GetString o conserva causa si null. JsonException se ignora para conservar causa; un valor mensaje de tipo no-string puede lanzar InvalidOperationException no capturada por este catch. Imprime `HTTP {número}: {causa}` y retorna 1. Nunca imprime la cabecera Authorization. Resumen solo contiene int? Cantidad: nulabilidad distingue falta de confirmación de cero.

#### Fragmentos de consulta, confirmación y resultado incierto

Antes de permitir PUT, Enviar exige un resumen HTTP válido y lee sí/no; EOF y no retornan antes de construir el reemplazo:

```csharp
            using var resumen = new HttpRequestMessage(HttpMethod.Get, "/api/v1/personajes/resumen");
            resumen.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credencial);
            using var consulta = await cliente.SendAsync(resumen);
            if (!consulta.IsSuccessStatusCode) return await Rechazo(consulta, salida);
            var actual = await consulta.Content.ReadFromJsonAsync<Resumen>();
            if (actual?.Cantidad is null or < 0) throw new JsonException("Resumen inválido.");
            salida.WriteLine($"Catálogo actual: {actual.Cantidad}. Personajes del JSON: {datos.GetArrayLength()}.");
            while (true)
            {
                salida.WriteLine("¿Reemplazar el catálogo? (sí/no)");
                string? respuesta = entrada.ReadLine()?.Trim();
                if (respuesta is null || respuesta.Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    salida.WriteLine("Carga cancelada. Catálogo sin cambios.");
                    return 0;
                }
                if (respuesta.Equals("sí", StringComparison.OrdinalIgnoreCase) || respuesta.Equals("si", StringComparison.OrdinalIgnoreCase)) break;
                salida.WriteLine("Respondé sí o no.");
            }
```

El catch distingue fallo previo de resultado incierto por la bandera enviado, sin repetir una escritura:

```csharp
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or JsonException or IOException)
        {
            salida.WriteLine(enviado ? "No se pudo confirmar el resultado de la carga. El catálogo puede haber sido reemplazado. No se reintentará automáticamente." : "No se pudo consultar el catálogo del servidor. No se envió el reemplazo.");
            return 1;
        }
```

Leer clona el elemento antes de disponer JsonDocument; ese clon permite enviar más tarde sin objeto ya liberado:

```csharp
        if (string.IsNullOrWhiteSpace(ruta)) throw new FormatException("Falta la ruta del archivo JSON.");
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(ruta));
        ValidarFormato(json.RootElement);
        return json.RootElement.Clone();
```

#### Fragmentos del arranque y rechazo HTTP

Iniciar materializa JSON antes de pedir datos de conexión; AllowAutoRedirect false evita reenviar la credencial siguiendo un redirect. El cliente creado es propiedad del método y se dispone al terminar:

```csharp
            var datos = await Leer(args?.FirstOrDefault(), entrada, salida);
            var lector = new EntradaConsola(entrada, salida);
            var servidor = new Uri(lector.Url("http://localhost:5000"));
            string credencial = lector.Secreto("Credencial de catálogo", permiteVacio: false);
            using var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = servidor };
            return await Enviar(cliente, credencial, entrada, salida, datos);
```

Rechazo conserva diagnóstico genérico si el cuerpo no es JSON válido. Solo JsonException se ignora aquí, no cualquier excepción de lectura ni un tipo incorrecto de mensaje:

```csharp
        string causa = respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized ? "La credencial de catálogo no es válida." : "El servidor rechazó la operación.";
        try
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            if (json.RootElement.TryGetProperty("mensaje", out var mensaje)) causa = mensaje.GetString() ?? causa;
        }
        catch (JsonException) { }
        salida.WriteLine($"HTTP {(int)respuesta.StatusCode}: {causa}");
        return 1;
    }
```

### Recorridos y ejemplos

Normal: archivo con seis personajes válidos, URL localhost y secreto ficticio; GET informa 8 existentes/6 del JSON; respuesta `si` produce PUT; confirmación `{cantidad:6}` informa seis y retorna 0. Cancelación: GET exitoso seguido de `no` o EOF retorna 0 sin enviar PUT.

Error: archivo `[7]` falla antes de pedir servidor. Tras PUT aceptado, una conexión perdida impide comprobar cantidad: retorna 1 con advertencia de posible reemplazo y no reintenta. GET 401 imprime causa y no pide confirmación. Lectura de estos recorridos no equivale a ejecutar una carga contra un servicio real.

### Errores, límites y efectos observables

Puede borrar/reemplazar catálogo solo después de confirmación, mediante servidor que valida transacción completa. No agrega individualmente ni asigna IDs en cliente. La verificación de formato local no sustituye reglas del dominio del servidor. Solo el primer argumento se usa como ruta, sin advertencia por argumentos extra. URL y secreto se solicitan cada vez; no se guardan en configuración local. Redirecciones no se siguen automáticamente.

### Relaciones y verificación

[ConsolaAltaTests](archivos/test/proyecto-integrador-final-tests/ConsolaAltaTests.md) define orden de preguntas, rechazo local, resumen, sí/no, PUT único y resultado incierto. [EntradaProcesosContratoTests](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) define proceso real y EOF. [CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md) valida atomicidad en servidor. Véase [verificación spec 004](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) para ejecución real y limitaciones.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ApiHttpTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ApiHttpTests.md)

## test/proyecto-integrador-final-tests/ApiHttpTests.cs

Fuente: [test/proyecto-integrador-final-tests/ApiHttpTests.cs](../test/proyecto-integrador-final-tests/ApiHttpTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Integración HTTP del host real en loopback: contrato de errores, catálogo, identidad, privacidad, idempotencia, transacciones, reinicio y finalización. No requiere DLL externas.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md), [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class ApiHttpTests`.

Declaración: `private sealed class RelojPruebas : TimeProvider`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

RelojPruebas privado deriva TimeProvider y guarda ahora=UnixEpoch; GetUtcNow lo retorna, Avanzar agrega segundos. No crea timers ni sincroniza. Cada test crea host/cliente/repositorio aislados; las pruebas de persistencia usan subdirectorio temporal y Pooling=false.

Declaraciones reales de estado de `RelojPruebas` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private DateTimeOffset ahora = DateTimeOffset.UnixEpoch;
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ApiHttpTests.PresentacionDePartidaDesconocidaDevuelveErrorDelProtocolo`

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

#### `ApiHttpTests.IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion`

Crea partida, usa Bearer J1 y PUT sesión con UUID/Ada; repite exactamente y después envía UUID nuevo. Exige200,200,409; primer body debe tener rolJ1 y mismo UUID. No examina el código de error del409 ni presencia posterior.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        var creada = app.Services.GetRequiredService<RepositorioJuego>().CrearPartida();
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

#### `ApiHttpTests.AltaExigeCredencialAdministrativaYValidaAtributos`

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

#### `ApiHttpTests.CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez`

Carga seis cartas iguales, incorpora ambas plazas e inicia. J2 intentando solicitud J1 recibe409 solicitudNoVigente; atributo Suerte de J1 recibe422 eleccionInvalida. Lanza dos POST de J1 con mismo turno y distinto ID operación/carta en paralelo: uno201 y otro409 revisionDesactualizada. Si ganó alternativa, la usa como operación aceptada. Cambiar carta conservando ID aceptado da409 operacionIncompatible; repetir cuerpo idéntico da200 y mismo JSON. J2 no puede consultar recibo J1 (404), no ve la carta elegida ni resultados antes de responder, y su solicitud carta no ofrece atributos. Responder da201 y un único resultado Fuerza ganado por J2 por empate; mano J1 queda2. La concurrencia puede elegir cualquiera de los POST: la prueba no fija orden.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez()
    {
        await using var app = ServidorHttp.Crear(["--ConnectionStrings:Juego=Data Source=:memory:",
            "--urls=http://127.0.0.1:0"]);
        var juego = app.Services.GetRequiredService<RepositorioJuego>();
        var partida = juego.CrearPartida();
        for (int i = 0; i < 6; i++)
            juego.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
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

#### `ApiHttpTests.ReinicioRecuperaSolicitudYRecibosYExigeSesionesNuevas`

Usa archivo en directorio temporal, Pooling=false. Primer host crea seis cartas/partida, incorpora plazas, acepta apertura J1 y guarda Location recibo y vista J2; desactiva una carta de mano de J2 para comprobar que snapshot no cambia. Reinicia mismo archivo: vieja sesión da409. Solo J2 reincorporado ve pausa y solicitud null; intentar elección previa da409 partidaPausada. Reincorporar J1 restaura vista J2 exactamente igual al JSON anterior y recibo GET200. Abandono J2 repetido da204 ambas veces, motivo abandono, sin propiedad ganador y revisión+1 una sola vez. finally elimina directorio.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
                var juego = app.Services.GetRequiredService<RepositorioJuego>();
                partida = juego.CrearPartida();
                for (int i = 0; i < 6; i++)
                    juego.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
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
                juego.Desactivar(antes.GetProperty("mano")[0].GetProperty("idPersonaje").GetInt32());
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

#### `ApiHttpTests.UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales`

Host y archivo temporal; acepta apertura J1 y prepara respuesta J2. Crea trigger SQLite BEFORE INSERT ON Operacion que aborta. POST J2 debe responder503 servicioNoDisponible; presentación JSON queda idéntica y recibo de esa operación devuelve404 recursoNoEncontrado. Elimina trigger y reenvía mismos datos:201 y un único resultado. Confirma atomicidad de estado+recibo para este fallo inducido. Detiene host y borra subdirectorio.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales()
    {
        var directorio = Directory.CreateTempSubdirectory();
        string conexion = $"Data Source={Path.Combine(directorio.FullName, "juego.db")};Pooling=false";
        try
        {
            await using var app = ServidorHttp.Crear([
                $"--ConnectionStrings:Juego={conexion}", "--urls=http://127.0.0.1:0"]);
            var juego = app.Services.GetRequiredService<RepositorioJuego>();
            var partida = juego.CrearPartida();
            for (int i = 0; i < 6; i++)
                juego.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
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

#### `ApiHttpTests.PresenciaRenuevaElPlazoSinCambiarLaRevision`

Usa RelojPruebas y memoria. Incorpora J1, guarda vista, avanza10s y PUT presencia exige204; avanza10s y vista debe ser idéntica (renovación prolongó plazo sin revisión). Avanza5s más: presencia llega exactamente al límite desde la renovación y debe devolver409 sesionVencida. No depende de esperar25 segundos reales.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task PresenciaRenuevaElPlazoSinCambiarLaRevision()
    {
        var reloj = new RelojPruebas();
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"], reloj);
        var juego = app.Services.GetRequiredService<RepositorioJuego>();
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

#### `ApiHttpTests.ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza`

Incorpora J1, cambia credencial por repositorio; consulta con vieja da401 credencialInvalida. Nueva credencial permite incorporar, pero CerrarSesion produce409 sesionVencida. Cancela partida y reincorpora con nueva credencial; vista conserva rolJ1, motivo cancelacion y revisión1. El test demuestra conservación del rol, motivo y revisión; no comprueba explícitamente la ausencia de un ganador.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza()
    {
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0"]);
        var juego = app.Services.GetRequiredService<RepositorioJuego>();
        var partida = juego.CrearPartida();
        await app.StartAsync();
        using var original = await Incorporar(app.Urls.Single(), partida.Id, partida.CredencialJ1, "Ada");
        var nueva = juego.ReemplazarCredencial(partida.Id, "J1");

        using var revocada = await original.GetAsync("presentacion");
        await Error(revocada, HttpStatusCode.Unauthorized, "credencialInvalida");
        using var j1 = await Incorporar(app.Urls.Single(), partida.Id, nueva, "Ada");
        juego.CerrarSesion(partida.Id, "J1");
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

#### `ApiHttpTests.PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra`

Host memoria con reloj detenido y logs Warning; seis cartas idénticas y ambas plazas. Dos bucles: rondas1..29 y combates1..3. Elector J1 si ronda+combate es par; cada vista debe indicar ronda/combate y mano de tamaño4−combate. Apertura con Energía y respuesta sin atributo dan201. Al final exige fase finalizada, motivo victoria, ganadorJ2, marcadorJ2=15, ronda29,87 resultados, mano vacía y solicitud null. Reloj fijo evita vencimiento; no valida tiempo de juego humano.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra()
    {
        await using var app = ServidorHttp.Crear([
            "--ConnectionStrings:Juego=Data Source=:memory:", "--urls=http://127.0.0.1:0",
            "--Logging:LogLevel:Default=Warning"], new RelojPruebas());
        var juego = app.Services.GetRequiredService<RepositorioJuego>();
        var partida = juego.CrearPartida();
        for (int i = 0; i < 6; i++)
            juego.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
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

#### `ApiHttpTests.AltaRechazaPuntajesRepresentadosComoTexto`

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

#### `ApiHttpTests.Error`

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

#### `RelojPruebas.GetUtcNow`

Override público retorna ahora sin avanzar ni consultar sistema; inicial UnixEpoch. Permite comparar expiraciones deterministas.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override DateTimeOffset GetUtcNow() => ahora;
```

#### `RelojPruebas.Avanzar`

internal void recibe segundos (sin validar negativos) y asigna ahora.AddSeconds(segundos). No dispara callbacks ni usa demora real.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Avanzar(int segundos) => ahora = ahora.AddSeconds(segundos);
```

#### `ApiHttpTests.DatosEleccion`

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

#### `ApiHttpTests.Incorporar`

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

#### `ApiHttpTests.Vista`

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

### Recorridos y ejemplos

Normal: Cartas iguales fuerzan empates: 29 rondas de tres combates producen87 resultados, victoria J2 con15 y sin reparto siguiente.

Alternativo o de error: Un trigger que aborta insertar Operacion debe causar503 sin cambiar presentación ni crear recibo; quitarlo permite reintentar la operación original.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ArranqueRestTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ArranqueRestTests.md)

## test/proyecto-integrador-final-tests/ArranqueRestTests.cs

Fuente: [test/proyecto-integrador-final-tests/ArranqueRestTests.cs](../test/proyecto-integrador-final-tests/ArranqueRestTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Contratos del ejecutable real cliente sobre stdin/stdout/stderr redirigidos. Requieren GUERRA_CLIENTE_DLL no vacía, dotnet disponible y DLL compilada de esta versión.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](archivos/test/proyecto-integrador-final-tests/ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ArranqueRestTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos. ProcessStartInfo usa dotnet con DLL de entorno; procesos y plazos se liberan con using. El primer caso mata el árbol en finally si sigue vivo; el segundo depende de timeout y salida normal.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ArranqueRestTests.TuberiaCerradaTerminaSinEsperarEntradaNiExponerCredencial`

ContratoFact GUERRA_CLIENTE_DLL. Lanza proceso, cierra su StandardOutput y escribe URL loopback puerto1, UUID fijo, secreto-tuberia y Ana. Espera máximo5s; stderr debe indicar No se pudo usar la consola, no incluir secreto y ExitCode1. finally mata árbol y espera si no salió. Acredita tubería cerrada, no servidor disponible.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task TuberiaCerradaTerminaSinEsperarEntradaNiExponerCredencial()
    {
        var inicio = new ProcessStartInfo("dotnet")
        { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
        using var proceso = Process.Start(inicio)!;
        try
        {
            proceso.StandardOutput.Close();
            await proceso.StandardInput.WriteAsync("http://127.0.0.1:1\n11223344-1122-3344-5566-778899aabbcc\nsecreto-tuberia\nAna\n");
            await proceso.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            string errores = await proceso.StandardError.ReadToEndAsync();
            Assert.Equal(1, proceso.ExitCode);
            Assert.Contains("No se pudo usar la consola", errores);
            Assert.DoesNotContain("secreto-tuberia", errores);
        }
        finally
        {
            if (!proceso.HasExited) { proceso.Kill(true); await proceso.WaitForExitAsync(); }
        }
    }
```

#### `ArranqueRestTests.SolicitaConexionCorrigeCamposYNoOfreceDemo`

ContratoFact GUERRA_CLIENTE_DLL. Lanza dotnet con tres flujos redirigidos y plazo10s; lee output concurrentemente, escribe URL inválida/válida con prefijo y UUID inválido/cero, cierra stdin y espera. Exige código0, prompts URL base y UUID, ausencia de Demostración y Credencial porque Guid.Empty no se acepta. No completa una incorporación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task SolicitaConexionCorrigeCamposYNoOfreceDemo()
    {
        var inicio = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
        using var proceso = Process.Start(inicio)!;
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var salida = proceso.StandardOutput.ReadToEndAsync(plazo.Token);
        await proceso.StandardInput.WriteAsync("invalida\nhttp://localhost:5000/prefijo/\ninvalido\n00000000-0000-0000-0000-000000000000\n");
        proceso.StandardInput.Close();
        await proceso.WaitForExitAsync(plazo.Token);
        var texto = await salida;
        Assert.Equal(0, proceso.ExitCode);
        Assert.Contains("URL base", texto);
        Assert.Contains("UUID", texto);
        Assert.DoesNotContain("Demostración", texto);
        Assert.DoesNotContain("Credencial", texto); // No avanza con un UUID vacío.
    }
```

### Recorridos y ejemplos

Normal: Input con URL y UUID inválidos vuelve a preguntar y, tras UUID vacío y EOF, termina0 sin pedir credencial.

Alternativo o de error: Cerrar la salida del proceso provoca diagnóstico de consola y código1 sin revelar el secreto.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/CargaJsonTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md)

## test/proyecto-integrador-final-tests/CargaJsonTests.cs

Fuente: [test/proyecto-integrador-final-tests/CargaJsonTests.cs](../test/proyecto-integrador-final-tests/CargaJsonTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Ensaya esquema de lote JSON, reglas de dominio, autenticación HTTP y reemplazo atómico del catálogo en SQLite. Incluye un trigger que induce fallo de inserción.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [AltaPersonaje](archivos/src/servidor/Api/PersonajeCatalogo.md), [ErrorApi](archivos/src/servidor/Api/ServidorHttp.md), [LotePersonajes](archivos/src/servidor/Api/LotePersonajes.md), [Personaje](archivos/src/servidor/Personaje.md), [PersonajeCatalogo](archivos/src/servidor/Api/PersonajeCatalogo.md), [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md), [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class CargaJsonTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Constante internal Uno es JSON de una carta válida (descripción vacía, ASCII *). No hay fixture compartido de base: cada prueba posee memoria o archivo temporal y conexiones con using.

Declaraciones reales de estado de `CargaJsonTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal const string Uno = """[{"nombre":"Ada","descripcion":"","ascii":"*","fuerza":250,"velocidad":350,"resistencia":450,"inteligencia":550,"energia":650,"habilidad":750}]""";
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `CargaJsonTests.RechazaFormatoIncompleto`

Theory [], null, {} y [{}]. Parse transforma JSON válido sintácticamente a RootElement; LotePersonajes.Leer debe lanzar ErrorApi. No cubre aquí JSON sintácticamente roto, pues JsonDocument.Parse lo rechazaría antes.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[{}]")]
    public void RechazaFormatoIncompleto(string json) =>
        Assert.Throws<ErrorApi>(() => LotePersonajes.Leer(JsonDocument.Parse(json).RootElement));
```

#### `CargaJsonTests.RechazaDuplicadosNormalizados`

Concatena dos objetos del JSON Uno cambiando Ada por espacio ada espacio en el segundo. Leer debe lanzar ErrorApi cuyo mensaje contenga nombre. Prueba normalización trim/case de duplicados dentro del lote, no duplicación contra catálogo persistido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RechazaDuplicadosNormalizados()
    {
        string json = Uno[..^1] + "," + Uno[1..].Replace("Ada", " ada ");
        Assert.Contains("nombre", Assert.Throws<ErrorApi>(() => LotePersonajes.Leer(JsonDocument.Parse(json).RootElement)).Message);
    }
```

#### `CargaJsonTests.ReemplazoRepiteSinAcumularYValidaAntesDeBorrar`

Repositorio memoria; da alta a Anterior, reemplaza por Uno y repite reemplazo. CantidadCatalogo debe ser1. Intenta reemplazar por la misma carta con Fuerza0; espera ErrorApi y cantidad todavía1. La aserción de cantidad no compara todos los campos: persistencia exacta se prueba aparte.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ReemplazoRepiteSinAcumularYValidaAntesDeBorrar()
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var lote = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement);
        juego.DarDeAlta(lote[0] with { Nombre = "Anterior" });
        juego.ReemplazarCatalogo(lote);
        juego.ReemplazarCatalogo(lote);
        Assert.Equal(1, juego.CantidadCatalogo());
        Assert.Throws<ErrorApi>(() => juego.ReemplazarCatalogo([lote[0] with { Fuerza = 0 }]));
        Assert.Equal(1, juego.CantidadCatalogo());
    }
```

#### `CargaJsonTests.ReemplazoPersisteDatosExactosYRevierteFalloDeInsercion`

Usa archivo temporal y Pooling=False. Da de alta Viejo e Inactivo, desactiva ID2 y exige cuenta2 (incluye inactivos); reemplaza por Uno y cierra repositorio. Consulta SQL y exige única fila [1,Ada,descripción vacía,*,250,350,450,550,650,750,activo1]. Crea trigger BEFORE INSERT para nombre Falla con RAISE(ABORT). Reemplazo de Ada+Falla debe lanzar DbUpdateException; nueva apertura conserva cantidad1 y SQL sigue retornando Ada. finally borra archivo. Demuestra rollback de ese fallo, no todos los errores posibles de disco.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ReemplazoPersisteDatosExactosYRevierteFalloDeInsercion()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"carga-{Guid.NewGuid()}.db");
        string cadena = $"Data Source={ruta};Pooling=False";
        try
        {
            var lote = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement);
            using (var juego = new RepositorioJuego(cadena, TimeProvider.System))
            {
                juego.DarDeAlta(lote[0] with { Nombre = "Viejo" });
                juego.DarDeAlta(lote[0] with { Nombre = "Inactivo" });
                juego.Desactivar(2);
                Assert.Equal(2, juego.CantidadCatalogo());
                juego.ReemplazarCatalogo(lote);
            }
            using var conexion = new Microsoft.Data.Sqlite.SqliteConnection(cadena);
            conexion.Open();
            using var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT Id, Nombre, Descripcion, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad, Activo FROM PersonajeCatalogo";
            using (var filas = comando.ExecuteReader())
            {
                Assert.True(filas.Read());
                object[] valores = new object[11];
                filas.GetValues(valores);
                Assert.Equal(new object[] { 1L, "Ada", "", "*", 250L, 350L, 450L, 550L, 650L, 750L, 1L }, valores);
                Assert.False(filas.Read());
            }
            comando.CommandText = "CREATE TRIGGER fallo BEFORE INSERT ON PersonajeCatalogo WHEN NEW.Nombre = 'Falla' BEGIN SELECT RAISE(ABORT, 'fallo inducido'); END;";
            comando.ExecuteNonQuery();
            using (var juego = new RepositorioJuego(cadena, TimeProvider.System))
                Assert.Throws<Microsoft.EntityFrameworkCore.DbUpdateException>(() => juego.ReemplazarCatalogo([lote[0], lote[0] with { Nombre = "Falla" }]));
            using (var juego = new RepositorioJuego(cadena, TimeProvider.System)) Assert.Equal(1, juego.CantidadCatalogo());
            comando.CommandText = "SELECT Nombre FROM PersonajeCatalogo";
            Assert.Equal("Ada", comando.ExecuteScalar());
        }
        finally { File.Delete(ruta); }
    }
```

#### `CargaJsonTests.HttpAutenticaResumenYReemplazoYConservaCatalogoAnteRechazo`

Host loopback puerto0, memoria y clave administrativa clave. Para token ausente, incorrecta y clave envía GET resumen y PUT lote: espera401,401,200 respectivamente. Con clave válida prueba [],null,{},[{}],atributo249 y duplicado Ada normalizado; cada PUT da400 y GET mantiene cantidad1. No invoca servicio externo; await using descarta host.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task HttpAutenticaResumenYReemplazoYConservaCatalogoAnteRechazo()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:", "--Catalogo:Credencial=clave"]);
        await app.StartAsync();
        using var cliente = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        foreach (string? clave in new string?[] { null, "incorrecta", "clave" })
        {
            cliente.DefaultRequestHeaders.Authorization = clave is null ? null : new("Bearer", clave);
            using var resumen = await cliente.GetAsync("/api/v1/personajes/resumen");
            using var carga = await cliente.PutAsync("/api/v1/personajes", new StringContent(Uno, System.Text.Encoding.UTF8, "application/json"));
            Assert.Equal(clave == "clave" ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Unauthorized, resumen.StatusCode);
            Assert.Equal(clave == "clave" ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Unauthorized, carga.StatusCode);
        }
        foreach (string json in new[] { "[]", "null", "{}", "[{}]", Uno.Replace("250", "249"), Uno[..^1] + "," + Uno[1..].Replace("Ada", " ada ") })
        {
            using var carga = await cliente.PutAsync("/api/v1/personajes", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, carga.StatusCode);
            using var resumen = await cliente.GetAsync("/api/v1/personajes/resumen");
            Assert.Contains("\"cantidad\":1", await resumen.Content.ReadAsStringAsync());
        }
    }
```

#### `CargaJsonTests.RechazaTextoDeDominioConUbicacion`

Theory nombre/espacios, ascii/tab, ascii/limite y ascii/lineas. Expande limite a121 asteriscos y lineas a26 líneas separadas por CR. Copia AltaPersonaje del fixture con campo inválido, intenta reemplazo en memoria y exige ErrorApi con Personaje1 y nombre de campo. No comprueba nombre del personaje en todos los diagnósticos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("nombre", "   ")]
    [InlineData("ascii", "\t")]
    [InlineData("ascii", "limite")]
    [InlineData("ascii", "lineas")]
    public void RechazaTextoDeDominioConUbicacion(string campo, string valor)
    {
        var datos = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement)[0];
        if (valor == "limite") valor = new string('*', 121);
        if (valor == "lineas") valor = string.Join('\r', Enumerable.Repeat("*", 26));
        datos = campo == "nombre" ? datos with { Nombre = valor } : datos with { Ascii = valor };
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var error = Assert.Throws<ErrorApi>(() => juego.ReemplazarCatalogo([datos]));
        Assert.Contains("Personaje 1", error.Message);
        Assert.Contains(campo, error.Message);
    }
```

#### `CargaJsonTests.CamposObligatoriosYTiposEstrictos`

Theory para los nueve campos nombre,descripcion,ascii,fuerza,velocidad,resistencia,inteligencia,energia,habilidad. Por campo usa JsonNode, primero asigna null, después elimina propiedad y por último asigna tipo incorrecto:42 a texto o cadena250 a número. Cada Leer debe lanzar ErrorApi mencionando campo. El mismo nodo se muta secuencialmente; no hay persistencia.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("nombre")]
    [InlineData("descripcion")]
    [InlineData("ascii")]
    [InlineData("fuerza")]
    [InlineData("velocidad")]
    [InlineData("resistencia")]
    [InlineData("inteligencia")]
    [InlineData("energia")]
    [InlineData("habilidad")]
    public void CamposObligatoriosYTiposEstrictos(string campo)
    {
        var objeto = System.Text.Json.Nodes.JsonNode.Parse(Uno)![0]!;
        objeto[campo] = null;
        Assert.Contains(campo, Assert.Throws<ErrorApi>(() => LotePersonajes.Leer(JsonDocument.Parse($"[{objeto}]").RootElement)).Message);
        objeto.AsObject().Remove(campo);
        Assert.Contains(campo, Assert.Throws<ErrorApi>(() => LotePersonajes.Leer(JsonDocument.Parse($"[{objeto}]").RootElement)).Message);
        objeto[campo] = campo is "nombre" or "descripcion" or "ascii" ? System.Text.Json.Nodes.JsonValue.Create(42) : System.Text.Json.Nodes.JsonValue.Create("250");
        Assert.Contains(campo, Assert.Throws<ErrorApi>(() => LotePersonajes.Leer(JsonDocument.Parse($"[{objeto}]").RootElement)).Message);
    }
```

#### `CargaJsonTests.RechazaReglasDeAtributos`

Theory con (249,350,450,550,650,751) fuera de rango; (250,350,450,550,650,749) suma2999; seis500 carece umbrales; (650,450,450,450,500,500) carece debilidad. Construye AltaPersonaje Ada y exige ErrorApi en Validar. No se comparan mensajes en este caso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(249,350,450,550,650,751)]
    [InlineData(250,350,450,550,650,749)]
    [InlineData(500,500,500,500,500,500)]
    [InlineData(650,450,450,450,500,500)]
    public void RechazaReglasDeAtributos(int f,int v,int r,int i,int e,int h)
    {
        var datos = new AltaPersonaje("Ada", "", "", f,v,r,i,e,h);
        Assert.Throws<ErrorApi>(() => datos.Validar());
    }
```

#### `CargaJsonTests.DiagnosticoIdentificaElAtributoFueraDeRango`

Copia el lote Uno con Energía751, ejecuta Validar y exige que ErrorApi.Message empiece exactamente energia:. Acredita atribución de campo del primer fallo de rango, no una lista de todos los errores.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void DiagnosticoIdentificaElAtributoFueraDeRango()
    {
        var datos = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement)[0] with { Energia = 751 };
        var error = Assert.Throws<ErrorApi>(() => datos.Validar());
        Assert.StartsWith("energia:", error.Message);
    }
```

### Recorridos y ejemplos

Normal: Lote Uno contiene Ada con atributos250/350/450/550/650/750. Reemplazar dos veces conserva una fila y reinicia IDs al estado esperado.

Alternativo o de error: Lote incompleto o nombres duplicados normalizados se rechazan; un trigger que aborta insertar Falla debe restaurar el catálogo Ada anterior.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/CartasDePrueba.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/CartasDePrueba.md)

## test/proyecto-integrador-final-tests/CartasDePrueba.cs

Fuente: [test/proyecto-integrador-final-tests/CartasDePrueba.cs](../test/proyecto-integrador-final-tests/CartasDePrueba.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Auxiliar interno para construir personajes válidos en los ensayos del modelo, evitando repetir ocho argumentos. No es una prueba descubierta por xUnit.

### Dependencias

Solo invoca el constructor del tipo interno Personaje del servidor. La referencia al proyecto y su InternalsVisibleTo permiten construirlo desde el ensamblado de pruebas. Este archivo no usa Assert, Fact ni Theory; los casos consumidores sí dependen de xUnit.

Relaciones usadas por este archivo: [Personaje](archivos/src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `internal static class CartasDePrueba`.

La clase estática no se instancia y no declara constructor propio; solo expone Crear dentro del ensamblado. No tiene herencia de clase explícita, interfaces, atributos, partial, propiedades, eventos ni operadores.

### Estado y datos

No conserva campos ni recursos; cada llamada devuelve una instancia nueva.

### Miembros y ejecución

Crear recibe su argumento de los casos consumidores, no de InlineData directamente. Su retorno es una referencia Personaje nueva; no hay asincronía, cancelación ni iteraciones porque la única instrucción construye y devuelve el objeto.

#### `CartasDePrueba.Crear`

Método internal static con fuerza opcional 650; calcula velocidad como 1000−fuerza y fija resistencia, inteligencia, energía y habilidad en 500. Retorna el Personaje recién construido. No normaliza fuerza ni evita excepciones: con 650 o 750 cumple ambas cotas y los umbrales; con 500 no. No modifica una carta anterior.

Implementación completa del auxiliar; el constructor invocado aplica las reglas de dominio:

```csharp
internal static Personaje Crear(int fuerza = 650)
    {
        return new Personaje("Carta", "", fuerza, 1000 - fuerza, 500, 500, 500, 500);
    }
```

### Recorridos y ejemplos

Normal: Crear() produce fuerza 650, velocidad 350 y los otros cuatro valores 500: suma 3000 y satisface los umbrales.

Alternativo o de error: Crear(500) produce seis atributos 500: incumple fortaleza y debilidad, pero el constructor lanza primero por falta de fortaleza y no alcanza la comprobación de debilidad. El auxiliar no captura esa excepción.

Recorridos deducidos por lectura del auxiliar y del constructor de Personaje; no son casos nuevos ejecutados durante la redacción.

### Errores, límites y efectos observables

Los errores proceden del constructor de Personaje y se propagan sin captura. No impone fuerza válida, ni conserva estado o recursos que liberar. No hay I/O, transacciones, hilos, cancelación ni limpieza de archivos aplicables: solo crea un objeto en memoria. El nombre fijo y el ASCII vacío son datos de ensayo, no una carga de catálogo.

### Relaciones y verificación

La firma y el fragmento se contrastaron literalmente contra la referencia común. [JugadorTests](archivos/test/proyecto-integrador-final-tests/JugadorTests.md), [MazoTests](archivos/test/proyecto-integrador-final-tests/MazoTests.md), [CombateTests](archivos/test/proyecto-integrador-final-tests/CombateTests.md), [RondaTests](archivos/test/proyecto-integrador-final-tests/RondaTests.md) y [PartidaTests](archivos/test/proyecto-integrador-final-tests/PartidaTests.md) consumen Crear. No existe un Fact dedicado a este archivo: su comportamiento se ejercita al construir cartas en esos casos. La [evidencia de suite](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) distingue ejecución del conjunto de la revisión por lectura de los errores no ensayados.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.md)

## test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs

Fuente: [test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs](../test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Contratos de salida del auxiliar DemostracionConsola conservado para enseñanza. Aunque el nombre dice contrato de consola, no arranca el ejecutable REST ni usa HTTP.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [DemostracionConsola](archivos/src/cliente/DemostracionConsola.md), [InterfazConsola](archivos/src/cliente/InterfazConsola.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ClienteConsolaContratoTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos; EjecutarDemo crea lector y escritor por llamada. Namespace global: el archivo no declara namespace.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ClienteConsolaContratoTests.Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince`

Ejecuta modo null con 16 saltos de línea. EnOrden exige mano, Lysander enfocado, confirmación, Fuerza enfocada, recepción, ganador, respuesta y cierres. Comprueba los seis nombres de atributos, Mateo15, dos apariciones de Atributo· y una solicitud de atributo. El marcador inicial de demo es histórico: no recorre quince rondas desde cero.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince()
    {
        var salida = await EjecutarDemo(null, new string('\n', 16));
        EnOrden(salida, "Tu mano", "> Lysander", "Enter para confirmar", "Atributo del combate", "> Fuerza",
            "Recepción confirmada", "Ganador", "Te toca responder", "Ganador",
            "gana la ronda", "gana la partida");
        Assert.Contains("Velocidad", salida);
        Assert.Contains("Resistencia", salida);
        Assert.Contains("Inteligencia", salida);
        Assert.Contains("Energía", salida);
        Assert.Contains("Habilidad", salida);
        Assert.Contains("Mateo 15", salida);
        Assert.Equal(2, salida.Split("Atributo · ", StringSplitOptions.None).Length - 1);
        Assert.Equal(1, salida.Split("Atributo del combate", StringSplitOptions.None).Length - 1);
    }
```

#### `ClienteConsolaContratoTests.Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta`

Entrada w/s seguida de 16 Enter. Exige secuencia de foco Lysander→Reched, solicitud de atributo, recepción, Fuerza y Lucía·Reched. Es input redirigido, no flechas de pseudoterminal.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta()
    {
        var salida = await EjecutarDemo(null, "w\ns\n" + new string('\n', 16));
        EnOrden(salida, "> Lysander", "> Reched", "Atributo del combate",
            "Recepción confirmada", "Atributo · Fuerza", "Lucía · Reched");
    }
```

#### `ClienteConsolaContratoTests.Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir`

Modo pausa y cinco Enter. Exige Sin comunicación, estado sin confirmar, reintentar o salir, último marcador Lucía14/Mateo14 y salida local con partida guardada. No simula transporte real ni verifica persistencia.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir()
    {
        var salida = await EjecutarDemo("pausa", "\n\n\n\n\n");
        EnOrden(salida, "Sin comunicación", "Estado actual sin confirmar",
            "reintentar o salir", "Último marcador confirmado", "Lucía 14 / Mateo 14",
            "Salida local", "La partida queda guardada");
    }
```

#### `ClienteConsolaContratoTests.Demo_DistingueAbandonoCancelacionYSalidaLocal`

Llama tres veces con modos abandono/cancelacion/salida y cuatro Enter. Los dos primeros deben tener Final confirmado y aclaración sin sumar victoria; la salida conserva partida y no debe contener Final confirmado. Cada llamada tiene flujos nuevos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_DistingueAbandonoCancelacionYSalidaLocal()
    {
        var abandono = await EjecutarDemo("abandono", "\n\n\n\n");
        var cancelacion = await EjecutarDemo("cancelacion", "\n\n\n\n");
        var salida = await EjecutarDemo("salida", "\n\n\n\n");

        EnOrden(abandono, "Final confirmado", "abandonó", "sin sumar una victoria");
        EnOrden(cancelacion, "Final confirmado", "canceló", "sin sumar una victoria");
        EnOrden(salida, "Salida local", "La partida queda guardada");
        Assert.DoesNotContain("Final confirmado", salida);
    }
```

#### `ClienteConsolaContratoTests.Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada`

Input vacío con modo normal. Exige mano, Enter para confirmar y Fin de la entrada en ese orden, sin Recepción confirmada. El texto EOF lo agrega el auxiliar al capturar EndOfStreamException.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada()
    {
        var salida = await EjecutarDemo(null, string.Empty);
        EnOrden(salida, "Tu mano", "Enter para confirmar", "Fin de la entrada");
        Assert.DoesNotContain("Recepción confirmada", salida);
    }
```

#### `ClienteConsolaContratoTests.EnOrden`

private static void: salida y params fragmentos. Comienza índice0, busca cada fragmento con IndexOf ordinal a partir del fin del anterior. Assert.True exige índice≥0 e incluye salida en diagnóstico. Avanza por longitud del fragmento, evitando que una aparición anterior satisfaga un paso posterior; termina tras todos. No exige igualdad completa de pantalla.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static void EnOrden(string salida, params string[] fragmentos)
    {
        var inicio = 0;
        foreach (var fragmento in fragmentos)
        {
            var indice = salida.IndexOf(fragmento, inicio, StringComparison.Ordinal);
            Assert.True(indice >= 0, $"Falta «{fragmento}» después de la posición {inicio}.\n{salida}");
            inicio = indice + fragmento.Length;
        }
    }
```

#### `ClienteConsolaContratoTests.EjecutarDemo`

private static Task<string>: con modo nullable e input crea StringReader/StringWriter e InterfazConsola. Ejecuta Recorrer con interactiva false; captura solo EndOfStreamException y agrega Fin de la entrada. Devuelve Task.FromResult del texto (no trabajo paralelo). using libera ambos flujos; otras excepciones fallan la prueba.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static Task<string> EjecutarDemo(string? modo, string entrada)
    {
        using var lector = new StringReader(entrada);
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(lector, salida);
        try { new DemostracionConsola(interfaz, lector, salida, false).Recorrer(modo); }
        catch (EndOfStreamException) { salida.WriteLine("Fin de la entrada"); }
        return Task.FromResult(salida.ToString());
    }
```

### Recorridos y ejemplos

Normal: Con 16 Enter la demostración muestra selección de carta/atributo, recepción, dos combates y victoria de Mateo15.

Alternativo o de error: EOF inicial termina sin recepción; modos pausa, abandono, cancelacion y salida muestran mensajes distintos sin atribuir victoria de juego.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md)

## test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs

Fuente: [test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs](../test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Arranca dos procesos cliente sucesivos contra un servidor local real para comprobar reapertura, plaza ocupada y ausencia de archivos de sesión. Requiere GUERRA_CLIENTE_DLL.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md), [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md).

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](archivos/test/proyecto-integrador-final-tests/ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ClienteProcesoRestTests`.

Declaración: `private sealed class Proceso : IAsyncDisposable`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Proceso privado implementa IAsyncDisposable: mantiene Process, ConcurrentQueue<string>, dos tareas lectoras y CTS15s. Texto une las líneas actuales; cada proceso posee su cola y plazo. El directorio temporal es común a ambas aperturas y se elimina al final.

Declaraciones reales de estado de `Proceso` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Process proceso;
```

```csharp
private readonly ConcurrentQueue<string> lineas = new();
```

```csharp
private readonly Task salida, errores;
```

```csharp
private readonly CancellationTokenSource plazo = new(TimeSpan.FromSeconds(15));
```

```csharp
internal string Texto => string.Join('\n', lineas);
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ClienteProcesoRestTests.ProcesoSeIncorporaReabreSinArchivosYEsperaPlazaHastaLiberacion`

Crea host loopback puerto0 con SQLite memoria y partida. En directorio temporal lanza primer Proceso, envía URL/UUID/credencialJ1/Ana, espera Esperando inicio, excluye credencial de output, cierra entrada y exige salida local. Segundo proceso con los mismos datos debe indicar sesión ocupada; libera J1 por repositorio, espera inicio, envía esc/s/Enter y termina sin Final confirmado. Exige directorio sin archivos y vista J2 esperandoInicio. Detiene app y finally elimina directorio; await using libera procesos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task ProcesoSeIncorporaReabreSinArchivosYEsperaPlazaHastaLiberacion()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        var juego = app.Services.GetRequiredService<RepositorioJuego>();
        var partida = juego.CrearPartida();
        await app.StartAsync();
        string directorio = Path.Combine(Path.GetTempPath(), $"guerra-proceso-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            await using (var primera = new Proceso(directorio))
            {
                await primera.Enviar($"{app.Urls.Single()}\n{partida.Id}\n{partida.CredencialJ1}\nAna\n");
                await primera.Esperar("Esperando inicio");
                Assert.DoesNotContain(partida.CredencialJ1, primera.Texto);
                primera.CerrarEntrada();
                await primera.Terminar();
                Assert.Contains("Salida local", primera.Texto);
            }
            await using (var segunda = new Proceso(directorio))
            {
                await segunda.Enviar($"{app.Urls.Single()}\n{partida.Id}\n{partida.CredencialJ1}\nAna\n");
                await segunda.Esperar("Sesión ocupada");
                juego.CerrarSesion(partida.Id, "J1");
                await segunda.Esperar("Esperando inicio");
                await segunda.Enviar("esc\ns\n\n");
                await segunda.Terminar();
                Assert.DoesNotContain("Final confirmado", segunda.Texto);
            }
            Assert.Empty(Directory.GetFiles(directorio));
            Assert.Equal("esperandoInicio", juego.Presentacion(partida.Id, partida.CredencialJ2,
                juego.Incorporar(partida.Id, partida.CredencialJ2, new(Guid.NewGuid(), "Ada")).IdSesion).Fase);
            await app.StopAsync();
        }
        finally { Directory.Delete(directorio, true); }
    }
```

#### `Proceso.Proceso`

Constructor internal con directorio: arma dotnet con cwd aislado y tres flujos redirigidos; agrega DLL de entorno, inicia proceso y arranca Leer en stdout y stderr. No comprueba File.Exists aquí; errores de lanzamiento se propagan.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal Proceso(string directorio)
        {
            var inicio = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directorio, RedirectStandardInput = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            inicio.ArgumentList.Add(Environment.GetEnvironmentVariable("GUERRA_CLIENTE_DLL")!);
            proceso = Process.Start(inicio)!;
            salida = Leer(proceso.StandardOutput);
            errores = Leer(proceso.StandardError);
        }
```

#### `Proceso.Leer`

private async Task: mientras ReadLineAsync con token15s no devuelva null, agrega cada línea a ConcurrentQueue. EOF termina normalmente; timeout o error propaga a la tarea. Mezcla stdout/stderr en la misma cola sin prometer orden entre flujos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private async Task Leer(StreamReader lector)
        {
            while (await lector.ReadLineAsync(plazo.Token) is { } linea) lineas.Enqueue(linea);
        }
```

#### `Proceso.Enviar`

internal Task: escribe texto tal cual en stdin de proceso y devuelve tarea. No agrega salto extra ni espera respuesta; el caller envía sus propios saltos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal Task Enviar(string texto) => proceso.StandardInput.WriteAsync(texto);
```

#### `Proceso.CerrarEntrada`

internal void: cierra stdin para producir EOF remoto; no abandona partida por HTTP ni mata proceso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void CerrarEntrada() => proceso.StandardInput.Close();
```

#### `Proceso.Esperar`

internal async Task: repite mientras Texto no contenga el fragmento ordinal. En cada vuelta exige proceso todavía vivo y retrasa20ms con token15s. Termina al encontrar el fragmento; muerte prematura/timeout falla.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal async Task Esperar(string texto)
        {
            while (!Texto.Contains(texto, StringComparison.Ordinal))
            {
                Assert.False(proceso.HasExited, Texto);
                await Task.Delay(20, plazo.Token);
            }
        }
```

#### `Proceso.Terminar`

internal async Task: espera salida con plazo, espera ambas tareas lectoras y exige ExitCode0 adjuntando Texto. No mata en este método.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal async Task Terminar()
        {
            await proceso.WaitForExitAsync(plazo.Token);
            await Task.WhenAll(salida, errores);
            Assert.True(proceso.ExitCode == 0, Texto);
        }
```

#### `Proceso.DisposeAsync`

public async ValueTask: si sigue vivo mata árbol y espera salida; después espera lectores y libera Process y CTS. Las excepciones de lectores se propagan; no borra directorio (lo hace el test). Asegura terminar el proceso antes de la limpieza de archivos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async ValueTask DisposeAsync()
        {
            if (!proceso.HasExited) { proceso.Kill(true); await proceso.WaitForExitAsync(); }
            await Task.WhenAll(salida, errores);
            proceso.Dispose(); plazo.Dispose();
        }
```

### Recorridos y ejemplos

Normal: Primer proceso entra como Ana, espera inicio y sale por EOF; el segundo recupera la plaza después de CerrarSesion y sale desde menú.

Alternativo o de error: Antes de liberar la sesión, el segundo muestra Sesión ocupada; la salida local no debe anunciar Final confirmado ni terminar la partida.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.md)

## test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs

Fuente: [test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs](../test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Integración de ClientePartidaHttp contra Kestrel loopback y SQLite temporal, sin ejecutar consola ni depender de variables DLL.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md), [ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md), [EleccionCliente](archivos/src/cliente/ContratosPartida.md), [EstadoCliente](archivos/src/cliente/EstadoCliente.md), [FalloCliente](archivos/src/cliente/ClientePartidaHttp.md), [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md), [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ClienteRestIntegracionTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos. Archivos SQLite con GUID por caso; finally ClearAllPools y borra archivo, -wal y -shm; await using libera hosts y using transportes.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ClienteRestIntegracionTests.ReinicioDelServidorConservaPartidaYRequiereReincorporarLaMismaSesion`

Crea base temporal y partida en primer host, incorpora cliente con UUID de sesión fijo y guarda vista. Detiene host y abre segundo con misma URL/base. Primera consulta del cliente debe lanzar FalloCliente codigo sesionVencida; Incorporar devuelve el mismo UUID, y vista recuperada conserva partida, revisión y Ana. Detiene host; limpieza elimina solo archivos de esta prueba. No juega una ronda antes del reinicio.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task ReinicioDelServidorConservaPartidaYRequiereReincorporarLaMismaSesion()
    {
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-reinicio-cliente-{Guid.NewGuid():N}.db");
        try
        {
            await using var primero = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            var creada = primero.Services.GetRequiredService<RepositorioJuego>().CrearPartida();
            await primero.StartAsync();
            string url = primero.Urls.Single();
            Guid sesion = Guid.NewGuid();
            using var cliente = new ClientePartidaHttp(new(new Uri(url + "/"), creada.Id, creada.CredencialJ1, "Ana"), sesion);
            await cliente.Incorporar(default);
            var anterior = await cliente.Presentacion(default);
            await primero.StopAsync();
            await using var segundo = ServidorHttp.Crear([$"--urls={url}", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            await segundo.StartAsync();
            var error = await Assert.ThrowsAsync<FalloCliente>(() => cliente.Presentacion(default));
            Assert.Equal("sesionVencida", error.Codigo);
            Assert.Equal(sesion, (await cliente.Incorporar(default)).IdSesion);
            var recuperada = await cliente.Presentacion(default);
            Assert.Equal(anterior.IdPartida, recuperada.IdPartida);
            Assert.Equal(anterior.Revision, recuperada.Revision);
            Assert.Equal("Ana", recuperada.Nombres["J1"]);
            await segundo.StopAsync();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }
```

#### `ClienteRestIntegracionTests.DosClientesJueganContraServidorLocalRecuperanReciboYSesion`

Da de alta seis cartas idénticas, crea dos configuraciones con credenciales diferentes (ambos nombres Ana), incorpora J1/J2 y renueva J1. Inicia por repositorio y comprueba manos sin intersección y validación de EstadoCliente. J1 elige primera carta/atributo; recibo por GET y reenvío idéntico debe ser igual. Vista J2 aún sin resultados y solicitud carta; al responder resulta un combate único. Cierra J1: Presentacion debe fallar sesionVencida; reincorpora mismo UUID y conserva recibo. Abandona J1; J2 ve motivo abandono y Ganador null. No recorre quince rondas aquí.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task DosClientesJueganContraServidorLocalRecuperanReciboYSesion()
    {
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-cliente-{Guid.NewGuid():N}.db");
        try
        {
            await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            var juego = app.Services.GetRequiredService<RepositorioJuego>();
            var partida = juego.CrearPartida();
            for (int i = 0; i < 6; i++) juego.DarDeAlta(new($"Carta {i}", "Descripción no visible", "ARTE NO VISIBLE", 750, 550, 500, 250, 500, 450));
            await app.StartAsync();
            var config1 = new ConfiguracionCliente(new Uri(app.Urls.Single() + "/"), partida.Id, partida.CredencialJ1, "Ana");
            var config2 = config1 with { Credencial = partida.CredencialJ2, Nombre = "Ana" };
            Guid sesion1 = Guid.NewGuid(), sesion2 = Guid.NewGuid();
            using var j1 = new ClientePartidaHttp(config1, sesion1);
            using var j2 = new ClientePartidaHttp(config2, sesion2);
            Assert.Equal("J1", (await j1.Incorporar(default)).Rol);
            Assert.Equal("J2", (await j2.Incorporar(default)).Rol);
            await j1.Renovar(default);
            juego.Iniciar(partida.Id);
            var v1 = await j1.Presentacion(default);
            var v2 = await j2.Presentacion(default);
            new EstadoCliente(partida.Id).Aplicar(v1);
            Assert.Empty(v1.Mano.Select(c => c.IdPersonaje).Intersect(v2.Mano.Select(c => c.IdPersonaje)));
            var s = v1.Solicitud!;
            var e = new EleccionCliente(Guid.NewGuid(), s.IdSolicitud, v1.Revision, s.IdsPersonajePermitidos[0], s.AtributosPermitidos[0]);
            var recibo = await j1.Elegir(e, default);
            Assert.Equal(recibo, await j1.Recibo(e.IdOperacion, default));
            Assert.Equal(recibo, await j1.Elegir(e, default));
            var oculta = await j2.Presentacion(default);
            Assert.Empty(oculta.Resultados);
            Assert.Equal("carta", oculta.Solicitud!.Tipo);
            await j2.Elegir(new(Guid.NewGuid(), oculta.Solicitud.IdSolicitud, oculta.Revision, oculta.Solicitud.IdsPersonajePermitidos[0], null), default);
            var resuelta = await j1.Presentacion(default);
            Assert.Single(resuelta.Resultados);
            new EstadoCliente(partida.Id).Aplicar(resuelta);
            juego.CerrarSesion(partida.Id, "J1");
            var vencida = await Assert.ThrowsAsync<FalloCliente>(() => j1.Presentacion(default));
            Assert.Equal("sesionVencida", vencida.Codigo);
            Assert.Equal(sesion1, (await j1.Incorporar(default)).IdSesion);
            Assert.Equal(recibo, await j1.Recibo(e.IdOperacion, default));
            await j1.Abandonar(default);
            var final = await j2.Presentacion(default);
            Assert.Equal("abandono", final.MotivoFin);
            Assert.Null(final.Ganador);
            await app.StopAsync();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }
```

### Recorridos y ejemplos

Normal: J1 y J2 incorporan, realizan un combate, recuperan recibo por GET y reenvío, reincorporan misma sesión y abandonan sin ganador.

Alternativo o de error: Reiniciar host invalida sesión anterior aunque conserva partida/revisión/nombre; Presentacion lanza sesionVencida hasta Incorporar.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/CombateTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/CombateTests.md)

## test/proyecto-integrador-final-tests/CombateTests.cs

Fuente: [test/proyecto-integrador-final-tests/CombateTests.cs](../test/proyecto-integrador-final-tests/CombateTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba guardas, consumo de cartas, estado incompleto, consulta de datos y resolución con desempate del modelo local. No ejecuta HTTP ni incrementa rondas de jugadores.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](archivos/test/proyecto-integrador-final-tests/CartasDePrueba.md), [Combate](archivos/src/servidor/Combate.md), [Jugador](archivos/src/servidor/Jugador.md), [TipoAtributo](archivos/src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class CombateTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Dos campos privados readonly elector=Ana y rival=Bruno, nuevos por instancia/caso xUnit. Las cartas se agregan durante cada test; no hay recursos externos ni limpieza de archivos.

Declaraciones reales de estado de `CombateTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Jugador elector = new("Ana");
```

```csharp
private readonly Jugador rival = new("Bruno");
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `CombateTests.ValidaParticipantesYDevuelveUnaCopiaEnOrden`

Exige alguna excepción al construir con elector null, rival null o la misma referencia en ambos roles mediante Record.Exception (no fija tipo exacto). Construye combate válido, sustituye posición0 del array GetParticipantes y exige que nueva consulta conserve [elector,rival]. Prueba copia del contenedor y orden, no clonación de objetos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ValidaParticipantesYDevuelveUnaCopiaEnOrden()
    {
        Assert.NotNull(Record.Exception(() => new Combate(null!, rival)));
        Assert.NotNull(Record.Exception(() => new Combate(elector, null!)));
        Assert.NotNull(Record.Exception(() => new Combate(elector, elector)));
        var combate = new Combate(elector, rival);

        combate.GetParticipantes()[0] = rival;

        Assert.Equal(new[] { elector, rival }, combate.GetParticipantes());
    }
```

#### `CombateTests.RechazaConsultasPrematurasYAtributosInvalidosORepetidos`

Combate nuevo no completo: consultar ganador, atributo o carta y Resolver deben arrojar alguna excepción. SetAtributo((TipoAtributo)99) también falla. Asigna Velocidad y comprueba getter; intentar Fuerza después falla sin reemplazar Velocidad. Sigue incompleto porque faltan cartas. Las aserciones no exigen mensajes ni tipos de excepción concretos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RechazaConsultasPrematurasYAtributosInvalidosORepetidos()
    {
        var combate = new Combate(elector, rival);

        Assert.False(combate.EstaCompleto());
        Assert.NotNull(Record.Exception(() => combate.GetGanador()));
        Assert.NotNull(Record.Exception(() => combate.GetAtributo()));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(elector)));
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
        Assert.NotNull(Record.Exception(() => combate.SetAtributo((TipoAtributo)99)));

        combate.SetAtributo(TipoAtributo.Velocidad);

        Assert.Equal(TipoAtributo.Velocidad, combate.GetAtributo());
        Assert.NotNull(Record.Exception(() => combate.SetAtributo(TipoAtributo.Fuerza)));
        Assert.Equal(TipoAtributo.Velocidad, combate.GetAtributo());
        Assert.False(combate.EstaCompleto());
    }
```

#### `CombateTests.RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador`

Crea dos cartas para elector y una para rival y un jugador ajeno llamado también Ana (identidad distinta). SetCarta rechaza jugador null, carta null, ajeno y carta del rival; manos quedan intactas. Registrar primera carta elector la retira, segunda elección del mismo participante falla sin quitar la otra. Rival registra su carta y mano queda vacía, pero falta atributo: aún incompleto. Tras Fuerza está completo, GetCarta retorna mismas referencias y rechaza null/ajeno. Demuestra identidad por referencia y consumo después de validar.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador()
    {
        var combate = new Combate(elector, rival);
        var cartaElector = CartasDePrueba.Crear();
        var otraCartaElector = CartasDePrueba.Crear();
        var cartaRival = CartasDePrueba.Crear();
        var ajeno = new Jugador("Ana");
        ajeno.RecibirCartas([cartaElector]);
        elector.RecibirCartas([cartaElector, otraCartaElector]);
        rival.RecibirCartas([cartaRival]);

        Assert.NotNull(Record.Exception(() => combate.SetCarta(null!, cartaElector)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, null!)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(ajeno, cartaElector)));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, cartaRival)));

        Assert.Equal(new[] { cartaElector, otraCartaElector }, elector.GetCartas());
        Assert.Same(cartaRival, Assert.Single(rival.GetCartas()));

        combate.SetCarta(elector, cartaElector);
        Assert.Same(otraCartaElector, Assert.Single(elector.GetCartas()));
        Assert.NotNull(Record.Exception(() => combate.SetCarta(elector, otraCartaElector)));
        Assert.Same(otraCartaElector, Assert.Single(elector.GetCartas()));

        combate.SetCarta(rival, cartaRival);
        Assert.Empty(rival.GetCartas());
        Assert.False(combate.EstaCompleto());

        combate.SetAtributo(TipoAtributo.Fuerza);
        Assert.True(combate.EstaCompleto());
        Assert.Same(cartaElector, combate.GetCarta(elector));
        Assert.Same(cartaRival, combate.GetCarta(rival));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(null!)));
        Assert.NotNull(Record.Exception(() => combate.GetCarta(ajeno)));
    }
```

#### `CombateTests.FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo`

Theory juegaElector false/true: elige participante, le da una carta, asigna Fuerza y registra solo esa. EstaCompleto debe false y Resolver debe lanzar alguna excepción. Verifica simétricamente que atributo+una carta no alcanza; no impone orden entre atributo y carta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo(bool juegaElector)
    {
        var combate = new Combate(elector, rival);
        var jugador = juegaElector ? elector : rival;
        var carta = CartasDePrueba.Crear();
        jugador.RecibirCartas([carta]);
        combate.SetAtributo(TipoAtributo.Fuerza);
        combate.SetCarta(jugador, carta);

        Assert.False(combate.EstaCompleto());
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
    }
```

#### `CombateTests.ResuelveSegunElAtributoElegidoYDaElEmpateAlRival`

Theory (fuerzaElector650,fuerzaRival750,ganaElector true),(750,650,false),(650,650,false). CartasDePrueba fija Velocidad=1000−Fuerza, y el combate compara Velocidad, no Fuerza. Registra atributo/carta elector y exige incompleto; registra rival, resuelve y compara misma referencia al ganador esperado tanto en Resolver como GetGanador. Marcadores de rondas de ambos siguen0; una segunda resolución debe lanzar alguna excepción. Acredita mayor atributo y empate al rival sin sumar ronda automáticamente.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(650, 750, true)]
    [InlineData(750, 650, false)]
    [InlineData(650, 650, false)]
    public void ResuelveSegunElAtributoElegidoYDaElEmpateAlRival(int fuerzaElector, int fuerzaRival, bool ganaElector)
    {
        var combate = new Combate(elector, rival);
        var cartaElector = CartasDePrueba.Crear(fuerzaElector);
        var cartaRival = CartasDePrueba.Crear(fuerzaRival);
        elector.RecibirCartas([cartaElector]);
        rival.RecibirCartas([cartaRival]);

        combate.SetAtributo(TipoAtributo.Velocidad);
        combate.SetCarta(elector, cartaElector);
        Assert.False(combate.EstaCompleto());
        combate.SetCarta(rival, cartaRival);

        Jugador ganadorEsperado = ganaElector ? elector : rival;
        Assert.Same(ganadorEsperado, combate.Resolver());
        Assert.Same(ganadorEsperado, combate.GetGanador());
        Assert.Equal(0, elector.GetRondasGanadas());
        Assert.Equal(0, rival.GetRondasGanadas());
        Assert.NotNull(Record.Exception(() => combate.Resolver()));
    }
```

### Recorridos y ejemplos

Normal: Con fuerza650 frente750 pero atributo Velocidad, gana elector: el auxiliar convierte a velocidades350/250.

Alternativo o de error: Con velocidades iguales gana rival; cartas ajenas o repetidas no deben consumirse y Resolver incompleto o repetido falla.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ConfiguracionClienteTests.md)

## test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs](../test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba reglas de URL, repetición de campos inválidos, conservación de espacios internos y ocultación de credencial en configuración.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ConfiguracionClienteTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin estado de fixture. StringReader y StringWriter son descartados mediante using.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ConfiguracionClienteTests.ValidaUrlSegunContrato`

Theory: http://localhost:5000 y https://localhost/prefijo deben retornar true; /relativa, ftp://localhost, http://usuario:clave@localhost, http://localhost/?consulta=1 y http://localhost/#fragmento retornan false. Solo llama EsUrl y compara bool; no prueba resolución DNS ni TLS.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("http://localhost:5000", true)]
    [InlineData("https://localhost/prefijo", true)]
    [InlineData("/relativa", false)]
    [InlineData("ftp://localhost", false)]
    [InlineData("http://usuario:clave@localhost", false)]
    [InlineData("http://localhost/?consulta=1", false)]
    [InlineData("http://localhost/#fragmento", false)]
    public void ValidaUrlSegunContrato(string url, bool valida) => Assert.Equal(valida, ConfiguracionCliente.EsUrl(url));
```

#### `ConfiguracionClienteTests.RecortaExterioresPreservaNombreYRepiteSoloCampoInvalido`

Genera UUID, alimenta URL inválida seguida de HTTPS válido, UUID inválido seguido del válido, credencial con espacios y nombre con doble espacio interior. Leer debe devolver Uri con / final, mismo UUID, secreto sin exteriores y Ana··Pérez con interiores intactos. Exige que secreto no aparezca ni en salida ni en ToString del record. No cuenta exactamente prompts ni mide ocultación por teclado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RecortaExterioresPreservaNombreYRepiteSoloCampoInvalido()
    {
        Guid id = Guid.NewGuid();
        using var entrada = new StringReader($"invalida\n https://localhost/prefijo \ninvalido\n{id}\n  secreto  \n  Ana  Pérez  \n");
        using var salida = new StringWriter();
        var config = ConfiguracionCliente.Leer(entrada, salida);
        Assert.Equal("Ana  Pérez", config.Nombre);
        Assert.Equal("secreto", config.Credencial);
        Assert.Equal(new Uri("https://localhost/prefijo/"), config.Url);
        Assert.Equal(id, config.Partida);
        Assert.DoesNotContain("secreto", salida.ToString() + config.ToString());
    }
```

### Recorridos y ejemplos

Normal: URL con prefijo HTTPS se normaliza con barra final, nombre exterior recortado conserva Ana··Pérez y credencial queda secreto.

Alternativo o de error: URL relativa, FTP, userinfo, query y fragment son rechazados. No ejecuta conexión ni verifica una terminal interactiva.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md)

## test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs](../test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba lectura administrativa sin iniciar host, valores por Enter, corrección por campo y fin de entrada.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ConfiguracionServidor](archivos/src/servidor/Api/ConfiguracionServidor.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ConfiguracionServidorTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos. La ruta temporal usada para validar configuración no se crea; Assert.False lo comprueba.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ConfiguracionServidorTests.Enter_ConservaValoresInicialesYDeshabilitaAltaSinCredencial`

Lee tres Enter con StringReader; compara dirección y archivo predeterminados, credencial vacía y mensaje de altas deshabilitadas. No demuestra apertura del puerto ni creación de SQLite.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void Enter_ConservaValoresInicialesYDeshabilitaAltaSinCredencial()
    {
        using var salida = new StringWriter();
        var config = ConfiguracionServidor.Leer(new StringReader("\n\n\n"), salida);
        Assert.Equal("http://0.0.0.0:5000", config.Direccion);
        Assert.Equal("guerra.db", config.Archivo);
        Assert.Equal("", config.Credencial);
        Assert.Contains("alta de personajes deshabilitada", salida.ToString());
    }
```

#### `ConfiguracionServidorTests.ConfiguracionInvalida_PermiteCorregirCadaCampoSinMostrarLaCredencial`

Alimenta ftp://localhost luego http://127.0.0.1:0; NUL luego ruta temporal; clave con espacio luego secreto-003. Exige los valores corregidos, ausencia de ambas credenciales en salida y que la ruta no exista. Permite puerto 0 para pruebas; no acredita que el directorio sea escribible al iniciar host.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ConfiguracionInvalida_PermiteCorregirCadaCampoSinMostrarLaCredencial()
    {
        using var salida = new StringWriter();
        var archivo = Path.Combine(Path.GetTempPath(), $"guerra-{Guid.NewGuid()}.db");
        var config = ConfiguracionServidor.Leer(new StringReader(
            $"ftp://localhost\nhttp://127.0.0.1:0\n\0\n{archivo}\nclave secreta\nsecreto-003\n"), salida);
        Assert.Equal("http://127.0.0.1:0", config.Direccion);
        Assert.Equal(archivo, config.Archivo);
        Assert.Equal("secreto-003", config.Credencial);
        Assert.DoesNotContain("secreto-003", salida.ToString());
        Assert.DoesNotContain("clave secreta", salida.ToString());
        Assert.False(File.Exists(archivo));
    }
```

#### `ConfiguracionServidorTests.FinDeEntrada_InterrumpeLaConfiguracion`

Theory con entrada vacía, un Enter, dos Enter e incorrecta seguido de EOF. Cada Leer debe lanzar EndOfStreamException en el campo incompleto o al repetir el inválido. No construye host ni aplica configuración parcial.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\n\n")]
    [InlineData("incorrecta\n")]
    public void FinDeEntrada_InterrumpeLaConfiguracion(string texto)
    {
        Assert.Throws<EndOfStreamException>(() =>
            ConfiguracionServidor.Leer(new StringReader(texto), new StringWriter()));
    }
```

### Recorridos y ejemplos

Normal: Tres líneas vacías devuelven escucha http://0.0.0.0:5000, archivo guerra.db y credencial vacía con aviso de altas deshabilitadas.

Alternativo o de error: FTP, ruta con NUL y credencial con espacio se corrigen con nuevas líneas sin exponer secretos; EOF aborta.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ConsolaAltaTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ConsolaAltaTests.md)

## test/proyecto-integrador-final-tests/ConsolaAltaTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConsolaAltaTests.cs](../test/proyecto-integrador-final-tests/ConsolaAltaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Prueba la carga JSON desde rutas con espacios, confirmación y tratamiento de errores mediante handlers HTTP controlados. El flujo vigente reemplaza catálogo, no pregunta personaje campo por campo.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md), [ConsolaAlta](archivos/src/subir-personajes/Program.md), [Personaje](archivos/src/servidor/Personaje.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class ConsolaAltaTests`.

Declaración: `private sealed class Fallo(bool perdida) : HttpMessageHandler`.

Declaración: `private sealed class Captura : HttpMessageHandler`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Clases anidadas privadas Fallo(bool perdida) y Captura heredan HttpMessageHandler. Solicitudes y Puts son enteros internal inicialmente0 que cuentan llamadas; cada test crea handler nuevo.

Declaraciones reales de estado de `Fallo` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal int Solicitudes;
```

Declaraciones reales de estado de `Captura` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal int Puts;
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ConsolaAltaTests.ListaPorRutaConfirmaAntesDeUnicoPut`

Theory (sí,1),( SI ,1),(no,0),(vacío,0),(quizás
si,1). El parámetro vacío se pasa a StringReader sin agregar un salto de línea: representa EOF, no Enter. Escribe CargaJsonTests.Uno en archivo temporal con espacios; Ejecutar con ruta explícita retorna0. Exige Puts esperado, actual7, JSON1 y ausencia de Nombre:. Fallo de confirmación repite hasta si. finally borra archivo y using libera cliente/handler/writer.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("sí", 1)]
    [InlineData(" SI ", 1)]
    [InlineData("no", 0)]
    [InlineData("", 0)]
    [InlineData("quizás\nsi", 1)]
    public async Task ListaPorRutaConfirmaAntesDeUnicoPut(string respuesta, int puts)
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"carga con espacios {Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            using var handler = new Captura();
            using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            using var salida = new StringWriter();
            Assert.Equal(0, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader(respuesta), salida, ruta));
            Assert.Equal(puts, handler.Puts);
            Assert.Contains("actual: 7", salida.ToString());
            Assert.Contains("JSON: 1", salida.ToString());
            Assert.DoesNotContain("Nombre:", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }
```

#### `ConsolaAltaTests.RutaInexistenteFinalizaSinConexion`

Llama Iniciar con argumento /no/existe.json, input vacío y writer; exige código1 y mensaje archivo. No levanta servidor ni comprueba contenido de una respuesta HTTP.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task RutaInexistenteFinalizaSinConexion()
    {
        using var salida = new StringWriter();
        Assert.Equal(1, await ConsolaAlta.Iniciar(new StringReader(""), salida, ["/no/existe.json"]));
        Assert.Contains("archivo", salida.ToString());
    }
```

#### `ConsolaAltaTests.ErrorHttpNoReintentaNiAfirmaRollback`

Theory perdida false/true. Archivo válido, respuesta si y handler Fallo. Debe retornar1 tras dos solicitudes totales (GET resumen y PUT). Nunca escribe Catálogo reemplazado. Para false imprime error de validación Personaje1(Ada):fuerza; para true advierte que no pudo confirmar resultado. No exige ni promete rollback ante pérdida de respuesta. finally borra archivo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ErrorHttpNoReintentaNiAfirmaRollback(bool perdida)
    {
        string ruta = Path.GetTempFileName();
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            using var handler = new Fallo(perdida);
            using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            using var salida = new StringWriter();
            Assert.Equal(1, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader("si"), salida, ruta));
            Assert.Equal(2, handler.Solicitudes);
            Assert.DoesNotContain("Catálogo reemplazado", salida.ToString());
            Assert.Contains(perdida ? "No se pudo confirmar el resultado" : "Personaje 1 (Ada): fuerza", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }
```

#### `ConsolaAltaTests.ArchivoInvalidoNoEnviaHttp`

Theory { malformado, [], null, [{}] y [1]. Ejecutar debe retornar1, dejar Puts0 y mencionar archivo. Sin BaseAddress en cliente, cualquier intento HTTP real también revelaría el fallo. No se valida aquí cada regla numérica de dominio.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("[{}]")]
    [InlineData("[1]")]
    public async Task ArchivoInvalidoNoEnviaHttp(string json)
    {
        string ruta = Path.GetTempFileName();
        await File.WriteAllTextAsync(ruta, json);
        try
        {
            using var handler = new Captura();
            using var cliente = new HttpClient(handler);
            using var salida = new StringWriter();
            Assert.Equal(1, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader(""), salida, ruta));
            Assert.Equal(0, handler.Puts);
            Assert.Contains("archivo", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }
```

#### `ConsolaAltaTests.RutaRelativaConEspaciosYFinDeEntradaEnConfirmacionCancela`

Escribe JSON válido en ruta temporal con espacios y calcula ruta relativa al cwd. La envía por input, luego EOF cuando falta confirmación. Ejecutar retorna0, Puts0 y Carga cancelada; finally borra archivo. No depende de una ruta fija del usuario.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task RutaRelativaConEspaciosYFinDeEntradaEnConfirmacionCancela()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"ruta con espacios {Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            using var handler = new Captura();
            using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            using var salida = new StringWriter();
            string relativa = Path.GetRelativePath(Environment.CurrentDirectory, ruta);
            Assert.Equal(0, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader(relativa + "\n"), salida));
            Assert.Equal(0, handler.Puts);
            Assert.Contains("Carga cancelada", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }
```

#### `Fallo.SendAsync`

override protected Task: incrementa Solicitudes para cada request. GET retorna200 con cantidad0. Para otros métodos, si perdida es true lanza HttpRequestException antes de devolver respuesta; si false retorna400 con mensaje Personaje1(Ada):fuerza. No transmite red ni modifica SQLite; token no se usa.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Solicitudes++;
            if (request.Method == HttpMethod.Get)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"cantidad\":0}") });
            if (perdida) throw new HttpRequestException("Respuesta perdida");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{\"mensaje\":\"Personaje 1 (Ada): fuerza\"}") });
        }
```

#### `Captura.SendAsync`

override protected: exige Authorization.Parameter igual a clave en todas las llamadas. Si Method es PUT incrementa Puts; retorna200 con cantidad7 para GET y cantidad1 para cualquier otro método. Es un doble: no demuestra autenticación real del servidor y no inspecciona el cuerpo del PUT.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("clave", request.Headers.Authorization!.Parameter);
            if (request.Method == HttpMethod.Put) Puts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(request.Method == HttpMethod.Get ? "{\"cantidad\":7}" : "{\"cantidad\":1}") });
        }
```

### Recorridos y ejemplos

Normal: Archivo de Ada con respuesta sí o SI genera un único PUT después de mostrar resumen actual7/JSON1.

Alternativo o de error: No o EOF cancela sin PUT; archivo inválido no envía, y error de PUT no se reintenta ni se presenta como reemplazo exitoso.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ConsolaServidorTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ConsolaServidorTests.md)

## test/proyecto-integrador-final-tests/ConsolaServidorTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConsolaServidorTests.cs](../test/proyecto-integrador-final-tests/ConsolaServidorTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba ayuda y diálogos administrativos con repositorio SQLite en memoria y texto redirigido. No arranca Kestrel.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ConsolaServidor](archivos/src/servidor/Api/ConsolaServidor.md), [Partida](archivos/src/servidor/Partida.md), [Personaje](archivos/src/servidor/Personaje.md), [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ConsolaServidorTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos. Cada caso/auxiliar posee su RepositorioJuego y flujos con using, sin base persistida.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ConsolaServidorTests.Ayuda_ExplicaLasAccionesYLosDatosQueSePiden`

Ejecuta ayuda y EOF; exige título, mención a los dos jugadores y seis activos, y excluye sintaxis antigua <idPartida>. No ejecuta inicio de partida.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void Ayuda_ExplicaLasAccionesYLosDatosQueSePiden()
    {
        string salida = Ejecutar("ayuda\n");
        Assert.Contains("Consola del servidor de Guerra", salida);
        Assert.Contains("los dos jugadores", salida);
        Assert.Contains("seis personajes activos", salida);
        Assert.DoesNotContain("<idPartida>", salida);
    }
```

#### `ConsolaServidorTests.Cancelar_PideUuidYReintentaSinPerderLaAccion`

Crea partida en repositorio memoria, alimenta cancelar/incorrecto/UUID real y ejecuta con CancellationToken.None. Exige prompt UUID, aviso válido y confirmación cancelada. No consulta fase final: el alcance de la aserción es salida del operador.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void Cancelar_PideUuidYReintentaSinPerderLaAccion()
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cancelar\nincorrecto\n{partida.Id}\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        Assert.Contains("UUID", salida.ToString());
        Assert.Contains("válido", salida.ToString());
        Assert.Contains("Partida cancelada.", salida.ToString());
    }
```

#### `ConsolaServidorTests.FinDeEntrada_NoConfirmaAccionIncompleta`

Theory: cancelar+EOF; cerrar-sesion+UUID fijo+EOF antes de plaza; desactivar+no+EOF tras inválido. Para cada salida excluye las tres confirmaciones. No demuestra que todo estado interno permanezca igual, solo que no se anuncia una acción incompleta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData("cancelar\n")]
    [InlineData("cerrar-sesion\n12345678-1234-4234-8234-123456789abc\n")]
    [InlineData("desactivar\nno\n")]
    public void FinDeEntrada_NoConfirmaAccionIncompleta(string comandos)
    {
        string salida = Ejecutar(comandos);
        Assert.DoesNotContain("Partida cancelada.", salida);
        Assert.DoesNotContain("Sesión cerrada.", salida);
        Assert.DoesNotContain("Personaje desactivado.", salida);
    }
```

#### `ConsolaServidorTests.CerrarSesion_PidePlazaYPermiteCorregirla`

Crea partida y envía cerrar-sesion, UUID, J3 y j2. Exige instrucción J1 o J2 y Sesión cerrada, mostrando corrección y aceptación sin mayúsculas. No incorpora una sesión activa en esta prueba.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void CerrarSesion_PidePlazaYPermiteCorregirla()
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        var partida = juego.CrearPartida();
        using var entrada = new StringReader($"cerrar-sesion\n{partida.Id}\nJ3\nj2\n");
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        Assert.Contains("J1 o J2", salida.ToString());
        Assert.Contains("Sesión cerrada.", salida.ToString());
    }
```

#### `ConsolaServidorTests.Ejecutar`

private static string: recibe secuencia de comandos, crea repositorio :memory:, StringReader y StringWriter, llama ConsolaServidor.Ejecutar sin cancelación externa y devuelve texto al alcanzar EOF. using libera recursos aunque la ejecución lance.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static string Ejecutar(string comandos)
    {
        using var juego = new RepositorioJuego("Data Source=:memory:", TimeProvider.System);
        using var entrada = new StringReader(comandos);
        using var salida = new StringWriter();
        ConsolaServidor.Ejecutar(juego, entrada, salida, CancellationToken.None);
        return salida.ToString();
    }
```

### Recorridos y ejemplos

Normal: cancelar, UUID inválido y UUID real conserva la acción hasta imprimir Partida cancelada.

Alternativo o de error: EOF durante cancelar/cerrar-sesion/desactivar no imprime confirmación de acción; J3 se corrige a j2.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ContratoFactAttribute.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ContratoFactAttribute.md)

## test/proyecto-integrador-final-tests/ContratoFactAttribute.cs

Fuente: [test/proyecto-integrador-final-tests/ContratoFactAttribute.cs](../test/proyecto-integrador-final-tests/ContratoFactAttribute.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Atributo público derivado de FactAttribute que condiciona los contratos de procesos a una variable de entorno. No ejecuta la aplicación ni comprueba la DLL.

### Dependencias

Usa System.Environment para consultar la variable y Xunit.FactAttribute como clase base. El runner de xUnit consume Skip durante el descubrimiento: este atributo no es un caso de prueba ni ejecuta Assert. No depende de la API, SQLite o bibliotecas HTTP.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ContratoFactAttribute : FactAttribute`.

La clase es pública y sellada: se puede usar como atributo de métodos pero no derivar de ella. Su único miembro declarado es el constructor público. Hereda el contrato de FactAttribute y la propiedad Skip; no declara partial, interfaces adicionales, eventos ni operadores.

### Estado y datos

No declara campos ni propiedades propios. Hereda Skip de FactAttribute; lo escribe durante construcción.

### Miembros y ejecución

El constructor recibe el nombre de variable al aplicar [ContratoFact(...)] a un método. No tiene valor de retorno ni acciones de juego: su efecto es establecer o conservar la condición Skip heredada. No hay preparación/acción/aserciones de un caso propio, porque este archivo define infraestructura de descubrimiento.

#### `ContratoFactAttribute.ContratoFactAttribute`

Constructor público: recibe el nombre de la variable, lee Environment.GetEnvironmentVariable(variableDll), y solo si IsNullOrWhiteSpace es true asigna Skip. El mensaje interpola el nombre de la variable, nunca su contenido. No existe rama de ejecución de procesos ni comprobación File.Exists aquí. Errores de nombre inválido de variable se propagan desde la biblioteca.

Implementación completa del constructor; el if determina si el runner omite el caso marcado:

```csharp
public ContratoFactAttribute(string variableDll)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variableDll)))
            Skip = $"Indicá la DLL compilada mediante {variableDll} para ejecutar esta prueba.";
    }
```

### Recorridos y ejemplos

Normal: Con GUERRA_CLIENTE_DLL definido con una ruta no vacía, el constructor no asigna Skip y xUnit puede ejecutar el contrato.

Alternativo o de error: Si la variable falta o solo contiene espacios, asigna Skip con el diagnóstico que indica qué variable configurar. Una ruta no vacía pero inexistente no se omite por este atributo.

Recorridos deducidos por lectura del constructor y de su condición; no se modificaron las variables del usuario para verificarlos.

### Errores, límites y efectos observables

Una variable en blanco omite el caso, pero una DLL inexistente con ruta no vacía se deja ejecutar y puede fallar posteriormente. No verifica compatibilidad de versión, SDK, permisos o ejecutable dotnet. No crea recursos, no cambia variables de entorno ni lanza procesos; por eso limpieza, asincronía y transacciones no aplican. El valor de la variable no se incorpora al mensaje Skip.

### Relaciones y verificación

La firma, herencia, condición y fragmento se contrastaron contra la referencia común. Lo consumen [ArranqueRestTests](archivos/test/proyecto-integrador-final-tests/ArranqueRestTests.md), [ClienteProcesoRestTests](archivos/test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md), [EntradaProcesosContratoTests](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) y [ServidorContratoTests](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md). No existe un Fact propio que compruebe todas las ramas de este constructor. La [evidencia de suite](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) ejecutó contratos con las variables DLL definidas; no debe interpretarse como ejecución de la rama Skip ausente.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md)

## test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs

Fuente: [test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs](../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Contratos de los tres ejecutables reales mediante stdin, con directorios temporales y host HTTP local para carga JSON. Cada ContratoFact requiere su variable DLL; el nombre histórico de algunos métodos no describe ya todos los pasos vigentes.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CargaJsonTests](archivos/test/proyecto-integrador-final-tests/CargaJsonTests.md), [Personaje](archivos/src/servidor/Personaje.md), [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](archivos/test/proyecto-integrador-final-tests/ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class EntradaProcesosContratoTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos de instancia. Ejecutar devuelve tupla (Codigo,Salida,Errores,CreoBase); elimina variables de configuración heredadas en el hijo y crea su cwd aislado. Cantidad es función local de Carga_RecorridoCompleto y captura HttpClient.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `EntradaProcesosContratoTests.Servidor_FinDeEntradaDuranteConfiguracionNoIniciaHostNiCreaBase`

Contrato GUERRA_SERVIDOR_DLL. Itera input vacío, Enter, dos Enter e invalida+Enter; Ejecutar debe retornar0, stdout sin Now listening y CreoBase false. Comprueba EOF y corrección sin configuración completa; no depende de un puerto externo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_SERVIDOR_DLL")]
    public async Task Servidor_FinDeEntradaDuranteConfiguracionNoIniciaHostNiCreaBase()
    {
        foreach (string entrada in new[] { "", "\n", "\n\n", "invalida\n" })
        {
            var resultado = await Ejecutar("GUERRA_SERVIDOR_DLL", entrada);
            Assert.Equal(0, resultado.Codigo);
            Assert.DoesNotContain("Now listening", resultado.Salida);
            Assert.False(resultado.CreoBase);
        }
    }
```

#### `EntradaProcesosContratoTests.Servidor_SinCredencialRechazaAltasYFinDeEntradaDetieneElHost`

Contrato servidor. Alimenta loopback puerto0, archivo predeterminado y credencial vacía, luego EOF. Exige0, aviso alta deshabilitada, escucha loopback real, creación de guerra.db y Application is shutting down. Pese al nombre, no envía un POST para verificar401: acredita mensaje y ciclo de host.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_SERVIDOR_DLL")]
    public async Task Servidor_SinCredencialRechazaAltasYFinDeEntradaDetieneElHost()
    {
        var resultado = await Ejecutar("GUERRA_SERVIDOR_DLL", "http://127.0.0.1:0\n\n\n");
        Assert.Equal(0, resultado.Codigo);
        Assert.Contains("alta de personajes deshabilitada", resultado.Salida);
        Assert.Contains("Now listening on: http://127.0.0.1:", resultado.Salida);
        Assert.True(resultado.CreoBase);
        Assert.Contains("Application is shutting down", resultado.Salida);
    }
```

#### `EntradaProcesosContratoTests.Servidor_BaseInvalidaPermiteCorregirLaConfiguracionSinExponerCredencial`

Contrato servidor. Ejecutar prepara guerra.db con texto no SQLite. Input primera configuración con secreto-003 y luego configuración nueva con corregida.db y credencial vacía. Exige0, Volvé a ingresar, escucha y ausencia de secreto en stdout+stderr. No modifica una base de usuario.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_SERVIDOR_DLL")]
    public async Task Servidor_BaseInvalidaPermiteCorregirLaConfiguracionSinExponerCredencial()
    {
        var resultado = await Ejecutar("GUERRA_SERVIDOR_DLL",
            "http://127.0.0.1:0\n\nsecreto-003\nhttp://127.0.0.1:0\ncorregida.db\n\n", baseInvalida: true);
        Assert.Equal(0, resultado.Codigo);
        Assert.Contains("Volvé a ingresar", resultado.Salida);
        Assert.Contains("Now listening", resultado.Salida);
        Assert.DoesNotContain("secreto-003", resultado.Salida + resultado.Errores);
    }
```

#### `EntradaProcesosContratoTests.Cliente_FinDeEntradaEnMenuSaleSinSimularUnaVictoria`

Contrato cliente. Ejecuta con input vacío y exige0, Fin de la entrada y ausencia de gana la partida. El nombre menciona menú histórico: realmente EOF ocurre al inicio de la configuración REST.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CLIENTE_DLL")]
    public async Task Cliente_FinDeEntradaEnMenuSaleSinSimularUnaVictoria()
    {
        var resultado = await Ejecutar("GUERRA_CLIENTE_DLL", "");
        Assert.Equal(0, resultado.Codigo);
        Assert.Contains("Fin de la entrada", resultado.Salida);
        Assert.DoesNotContain("gana la partida", resultado.Salida);
    }
```

#### `EntradaProcesosContratoTests.Carga_SinArgumentosNiCredencialDeEntornoEnviaElContratoExistente`

Contrato carga. Host memoria/loopback con secreto-003 y archivo temporal Uno; ejecutable sin argumentos recibe ruta, URL, credencial y si por stdin. Exige0, Catálogo reemplazado y secreto ausente en ambas salidas. finally borra JSON. No usa carga por campos individuales.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CARGA_DLL")]
    public async Task Carga_SinArgumentosNiCredencialDeEntornoEnviaElContratoExistente()
    {
        await using var app = ServidorHttp.Crear([
            "--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:", "--Catalogo:Credencial=secreto-003"
        ]);
        await app.StartAsync();
        string ruta = Path.Combine(Path.GetTempPath(), $"personajes {Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            var resultado = await Ejecutar("GUERRA_CARGA_DLL",
                $"{ruta}\n{app.Urls.Single()}\nsecreto-003\nsi\n");
            Assert.Equal(0, resultado.Codigo);
            Assert.Contains("Catálogo reemplazado", resultado.Salida);
            Assert.DoesNotContain("secreto-003", resultado.Salida + resultado.Errores);
        }
        finally { File.Delete(ruta); }
    }
```

#### `EntradaProcesosContratoTests.Carga_FinDeEntradaDuranteConexionOPersonajeSaleSinAlta`

Contrato carga. Inputs vacío, Enter, Enter+secreto y una secuencia histórica de campos. Todos carecen de ruta JSON válida: exige código1 y ausencia de Personaje cargado y secreto en salida. El nombre conserva historia; no acredita EOF en cada campo del flujo JSON actual.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CARGA_DLL")]
    public async Task Carga_FinDeEntradaDuranteConexionOPersonajeSaleSinAlta()
    {
        foreach (string entrada in new[] { "", "\n", "\nsecreto-003\n", "\nsecreto-003\nAda\n\n0\n250\n" })
        {
            var resultado = await Ejecutar("GUERRA_CARGA_DLL", entrada);
            Assert.Equal(1, resultado.Codigo);
            Assert.DoesNotContain("Personaje cargado", resultado.Salida);
            Assert.DoesNotContain("secreto-003", resultado.Salida + resultado.Errores);
        }
    }
```

#### `EntradaProcesosContratoTests.Carga_RecorridoCompletoDesdeProcesoConBaseTemporal`

Contrato carga. Crea SQLite y archivo Uno temporales; resuelve ejemplos/personajes.json respecto de DLL tres niveles arriba. Host con clave y Pooling=False. Verifica cuenta0 y pasos (ejemplos,si,6),(uno,no,6),(uno,si,1),(uno,si,1),(ejemplos,si,6) lanzando proceso con ruta por argumento; exige0, sin prompt de ruta y resumen esperado. Luego cambia fuerza250 a249 en archivo, exige1 y diagnóstico Personaje1(Ada), conservando seis. finally borra JSON/base. Requiere conservar ubicación de ejemplos junto al árbol del proyecto.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[ContratoFact("GUERRA_CARGA_DLL")]
    public async Task Carga_RecorridoCompletoDesdeProcesoConBaseTemporal()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"recorrido-{Guid.NewGuid()}.db");
        string archivo = Path.Combine(Path.GetTempPath(), $"un personaje {Guid.NewGuid()}.json");
        string ejemplos = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Environment.GetEnvironmentVariable("GUERRA_CARGA_DLL"))!, "../../../ejemplos/personajes.json"));
        await File.WriteAllTextAsync(archivo, CargaJsonTests.Uno);
        try
        {
            await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={ruta};Pooling=False", "--Catalogo:Credencial=clave"]);
            await app.StartAsync();
            using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            http.DefaultRequestHeaders.Authorization = new("Bearer", "clave");
            async Task Cantidad(int esperada)
            {
                using var respuesta = await http.GetAsync("/api/v1/personajes/resumen");
                Assert.Contains($"\"cantidad\":{esperada}", await respuesta.Content.ReadAsStringAsync());
            }
            await Cantidad(0);
            foreach (var paso in new[] { (ejemplos,"si",6), (archivo,"no",6), (archivo,"si",1), (archivo,"si",1), (ejemplos,"si",6) })
            {
                var resultado = await Ejecutar("GUERRA_CARGA_DLL", $"{app.Urls.Single()}\nclave\n{paso.Item2}\n", argumentos: [paso.Item1]);
                Assert.Equal(0, resultado.Codigo);
                Assert.DoesNotContain("Ruta del archivo JSON:", resultado.Salida);
                await Cantidad(paso.Item3);
            }
            await File.WriteAllTextAsync(archivo, CargaJsonTests.Uno.Replace("250", "249"));
            var rechazo = await Ejecutar("GUERRA_CARGA_DLL", $"{app.Urls.Single()}\nclave\nsi\n", argumentos: [archivo]);
            Assert.Equal(1, rechazo.Codigo);
            Assert.Contains("Personaje 1 (Ada)", rechazo.Salida);
            await Cantidad(6);
        }
        finally { File.Delete(archivo); File.Delete(ruta); }
    }
```

#### `EntradaProcesosContratoTests.Cantidad`

Función local async que captura http. GET /api/v1/personajes/resumen y exige substring JSON cantidad:<esperada>. Libera respuesta con using; no comprueba por separado StatusCode ni parsea JSON, por lo que su garantía es el texto observado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
async Task Cantidad(int esperada)
            {
                using var respuesta = await http.GetAsync("/api/v1/personajes/resumen");
                Assert.Contains($"\"cantidad\":{esperada}", await respuesta.Content.ReadAsStringAsync());
            }
```

#### `EntradaProcesosContratoTests.Ejecutar`

private static async Task de tupla. Crea directorio GUID, opcionalmente escribe base inválida; arma dotnet con DLL absoluta de la variable y argumentos opcionales. Borra del entorno hijo Catalogo__Credencial, ConnectionStrings__Juego y ASPNETCORE_URLS. Lee stdout/stderr concurrentemente con plazo15s, escribe input, cierra stdin, espera salida y retorna código/textos y existencia de guerra.db antes de limpiar. finally mata árbol si sigue vivo, espera y borra solo directorio temporal. Una DLL no vacía pero inválida falla al lanzar y no queda omitida por ContratoFact.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static async Task<(int Codigo, string Salida, string Errores, bool CreoBase)> Ejecutar(string variable, string entrada, bool baseInvalida = false, string[]? argumentos = null)
    {
        string directorio = Path.Combine(Path.GetTempPath(), $"guerra-entrada-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        if (baseInvalida) File.WriteAllText(Path.Combine(directorio, "guerra.db"), "esto no es SQLite");
        var inicio = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directorio, UseShellExecute = false,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.ArgumentList.Add(Path.GetFullPath(Environment.GetEnvironmentVariable(variable)!));
        foreach (var argumento in argumentos ?? []) inicio.ArgumentList.Add(argumento);
        inicio.Environment.Remove("Catalogo__Credencial");
        inicio.Environment.Remove("ConnectionStrings__Juego");
        inicio.Environment.Remove("ASPNETCORE_URLS");
        using var proceso = Process.Start(inicio)!;
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            var salida = proceso.StandardOutput.ReadToEndAsync(plazo.Token);
            var errores = proceso.StandardError.ReadToEndAsync(plazo.Token);
            await proceso.StandardInput.WriteAsync(entrada.AsMemory(), plazo.Token);
            proceso.StandardInput.Close();
            await proceso.WaitForExitAsync(plazo.Token);
            return (proceso.ExitCode, await salida, await errores, File.Exists(Path.Combine(directorio, "guerra.db")));
        }
        finally
        {
            if (!proceso.HasExited)
            {
                proceso.Kill(entireProcessTree: true);
                await proceso.WaitForExitAsync();
            }
            Directory.Delete(directorio, recursive: true);
        }
    }
```

### Recorridos y ejemplos

Normal: Carga ejecutable reemplaza catálogo vacío por seis ejemplos, cancela un reemplazo, reemplaza por uno dos veces y vuelve a seis; cada resumen coincide.

Alternativo o de error: EOF durante configuración no arranca host o no anuncia alta. Base inválida solicita configuración nueva sin imprimir secreto.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/EstadoClienteTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/EstadoClienteTests.md)

## test/proyecto-integrador-final-tests/EstadoClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/EstadoClienteTests.cs](../test/proyecto-integrador-final-tests/EstadoClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Verifica aplicación de presentaciones y cola local de resultados sin transporte. Los auxiliares Vista y Resultado son fixtures de contrato, no datos validados por el servidor.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [EstadoCliente](archivos/src/cliente/EstadoCliente.md), [FalloCliente](archivos/src/cliente/ClientePartidaHttp.md), [PresentacionCliente](archivos/src/cliente/ContratosPartida.md), [ResultadoCliente](archivos/src/cliente/ContratosPartida.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class EstadoClienteTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos de fixture. Cada EstadoCliente conserva su vista y cola solo dentro del caso.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `EstadoClienteTests.PrimeraVistaOmiteHistoriaYReconectarConservaResultadosNuevosEnOrden`

Crea partida, estado, resultado anterior (ronda1/combate1) y nuevo (1/2). Aplica anterior y exige cola vacía. Marca pérdida y aplica nuevo/anterior, luego anterior/nuevo; Assert.Single debe ser la misma referencia nuevo y Actual debe ser true. Prueba deduplicación e historia inicial, no un reinicio de proceso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PrimeraVistaOmiteHistoriaYReconectarConservaResultadosNuevosEnOrden()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        var anterior = Resultado(1, 1);
        var nuevo = Resultado(1, 2);
        estado.Aplicar(Vista(partida) with { Resultados = [anterior] });
        Assert.Empty(estado.Resultados);
        estado.PerderComunicacion();
        estado.Aplicar(Vista(partida) with { Resultados = [nuevo, anterior] });
        estado.Aplicar(Vista(partida) with { Resultados = [anterior, nuevo] });
        Assert.Same(nuevo, Assert.Single(estado.Resultados));
        Assert.True(estado.Actual);
    }
```

#### `EstadoClienteTests.PausaYNombresSeAplicanSinCambiarRevisionYErroresNoDestruyenUltimoEstado`

Aplica vista inicial y luego record con pausa y nombres J1=Ana/J2=Ada sin aumentar revisión; exige que Vista sea esa misma instancia. PerderComunicacion baja Actual pero conserva vista. Aplicar copia con fase desconocida lanza FalloCliente y no sustituye la vista pausada.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PausaYNombresSeAplicanSinCambiarRevisionYErroresNoDestruyenUltimoEstado()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        var inicial = Vista(partida);
        estado.Aplicar(inicial);
        var pausa = inicial with { Pausada = true, Nombres = new() { ["J1"] = "Ana", ["J2"] = "Ada" } };
        estado.Aplicar(pausa);
        Assert.Same(pausa, estado.Vista);
        estado.PerderComunicacion();
        Assert.False(estado.Actual);
        Assert.Same(pausa, estado.Vista);
        Assert.Throws<FalloCliente>(() => estado.Aplicar(pausa with { Fase = "desconocida" }));
        Assert.Same(pausa, estado.Vista);
    }
```

#### `EstadoClienteTests.ManoVaciaEsValidaEnEsperaPeroNoConOpcionesImposibles`

Aplica Vista de espera con mano vacía sin excepción. Después agrega una solicitud tipo carta con ID permitido 999, conservando Fase=esperandoInicio y mano vacía; Aplicar debe lanzar FalloCliente. La validación rechaza primero que exista solicitud en esa fase no electora, antes de inspeccionar los IDs permitidos. Este caso no aísla la guarda de pertenencia de 999 a la mano y no observa un rechazo HTTP.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ManoVaciaEsValidaEnEsperaPeroNoConOpcionesImposibles()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        estado.Aplicar(Vista(partida));
        Assert.Throws<FalloCliente>(() => estado.Aplicar(Vista(partida) with
        {
            Solicitud = new(Guid.NewGuid(), "carta", [999], [])
        }));
    }
```

#### `EstadoClienteTests.Vista`

Auxiliar internal static retorna PresentacionCliente con id recibido, revisión1, fase esperandoInicio, sin pausa, rol J1, nombres Ana/null, ronda/combate null, marcador 0/0, mano/resultados vacíos, solicitud y cierre null. Cada llamada crea un record nuevo; no hace I/O.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal static PresentacionCliente Vista(Guid id) => new(id, 1, "esperandoInicio", false, "J1",
        new() { ["J1"] = "Ana", ["J2"] = null }, null, null, new() { ["J1"] = 0, ["J2"] = 0 }, [], null, [], null, null);
```

#### `EstadoClienteTests.Resultado`

Auxiliar internal static retorna resultado para ronda y combate recibidos: cartas J1=(1,A) y J2=(2,B) con mapas vacíos, Fuerza, valores 10/20, ganador J2 y sin ganador de ronda. Es un fixture deliberadamente mínimo para cola/presentación, no una carta de dominio válida.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal static ResultadoCliente Resultado(int ronda, int combate) => new(ronda, combate,
        new() { ["J1"] = new(1, "A", new Dictionary<string, int>()), ["J2"] = new(2, "B", new Dictionary<string, int>()) },
        "Fuerza", new() { ["J1"] = 10, ["J2"] = 20 }, "J2", null);
```

### Recorridos y ejemplos

Normal: Primera vista con resultado (1,1) omite historia; después de corte llega (1,2) y la cola lo contiene una sola vez pese al orden recibido.

Alternativo o de error: Una fase desconocida debe lanzar FalloCliente y conservar la última vista; una solicitud de carta 999 sin esa carta es inválida.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.md)

## test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs

Fuente: [test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs](../test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Ensaya recuperación del bucle cliente real sobre HTTP loopback y SQLite temporal mediante intermediario que corta una única respuesta antes o después del commit. También recorre una partida completa con dos transportes.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md), [ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md), [EleccionCliente](archivos/src/cliente/ContratosPartida.md), [EntradaCliente](archivos/src/cliente/EntradaCliente.md), [InterfazConsola](archivos/src/cliente/InterfazConsola.md), [PresentacionCliente](archivos/src/cliente/ContratosPartida.md), [RepositorioJuego](archivos/src/servidor/Api/RepositorioJuego.md), [ServidorHttp](archivos/src/servidor/Api/ServidorHttp.md), [SesionPartidaCliente](archivos/src/cliente/SesionPartidaCliente.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class IntegracionRespuestaPerdidaTests`.

Declaración: `private sealed class CorteHttp(bool despues, string rutaCorte = "elecciones") : DelegatingHandler(new HttpClientHandler())`.

Declaración: `private sealed class Teclas : TextReader`.

Declaración: `private sealed class Salida : StringWriter`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

CorteHttp(bool despues,string rutaCorte="elecciones") privado hereda DelegatingHandler(new HttpClientHandler()); cortada inicialmente false limita el corte a uno, ConfirmadaAntesDelCorte false solo cambia después de respuesta real exitosa y Cuerpos/Rutas son colas concurrentes. Teclas privado deriva TextReader con Channel<string> ilimitado. Salida privado deriva StringWriter y serializa accesos con bloqueo; Longitud es accesor con lock que devuelve ToString().Length.

Declaraciones reales de estado de `CorteHttp` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private bool cortada;
```

```csharp
internal bool ConfirmadaAntesDelCorte;
```

```csharp
internal ConcurrentQueue<string> Cuerpos = new();
```

```csharp
internal ConcurrentQueue<string> Rutas = new();
```

Declaraciones reales de estado de `Teclas` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Channel<string> canal = Channel.CreateUnbounded<string>();
```

Declaraciones reales de estado de `Salida` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly object bloqueo = new();
```

```csharp
internal int Longitud { get { lock (bloqueo) return ToString().Length; } }
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `IntegracionRespuestaPerdidaTests.ClienteResuelveEleccionInciertaContraSQLiteReal`

Theory confirmarAntesDelCorte true/false. Base temporal con seis cartas, host loopback y transportes separados; J2 se incorpora, J1 usa CorteHttp, se inicia partida. Ejecuta SesionPartidaCliente con canal de teclas y CTS12s; espera turno, confirma carta, espera atributo y confirma. Tras Recepción confirmada, vista rival debe tener revisión inicial+1, sin resultados y solicitud carta. Exige un cuerpo si corte posterior o dos si previo, todos idénticos. Rutas deben mostrar GET elecciones/<id> después del primer POST y antes del reenvío cuando lo hay. Responder J2 produce un solo resultado y revisión inicial+2. finally interno cancela bucle y espera3s antes de detener host; externo limpia conexiones y archivos SQLite/-wal/-shm.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `IntegracionRespuestaPerdidaTests.DosTransportesCompletanPartidaNormalHastaQuinceSinRepartoPosterior`

Base temporal y seis cartas de mismos atributos. Incorpora transportes J1/J2 e inicia. Bucle limitado a menos300 elecciones renueva ambas presencias, consulta ambas vistas y exige misma revisión, marcador ordenado, JSON de resultados y manos disjuntas. Si finalizada corta; si activa exactamente una solicitud debe existir. En apertura verifica elector por paridad, manos4−combate y exclusión de cartas ya reveladas en la ronda. Elige primera opción y atributo solo en cartaYAtributo, exige recibo revisión+1 y consulta igual. Final: victoria, ganador compartido con15, ronda29,87 resultados, tres por ronda y dos para ganador de ronda; solicitudes null y nueva consulta estable sin reparto. finally limpia base y pools.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `IntegracionRespuestaPerdidaTests.AbandonoInciertoSoloMuestraFinConfirmadoSinSumarVictoria`

Theory corte antes/después. Partida distinta, SQLite y host real, intermediario corta solo ruta abandono. Bucle de consola con CTS15s: abre menú esc/s/s/Enter, exige cero PUT abandono; Enter en No vuelve sin enviar. Reabre, baja a Sí y confirma; espera fin por abandono. Vista rival debe tener motivo abandono, ganador/solicitud null, revisión+1, marcador igual al inicial y flag de corte acorde. Cuenta PUT1 si aplicado o2 si previo; nunca POST elecciones. Repetir abandono conserva revisión. Cancelación del bucle precede al cierre del host y a limpiar archivos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `IntegracionRespuestaPerdidaTests.Tecla`

Función local async del caso abandono: captura salida, entrada y tarea. Guarda Longitud, envía tecla y espera que longitud crezca para no adelantar la navegación a un dibujo anterior. No comprueba por sí misma cuál pantalla se dibujó; las aserciones del caso verifican el resultado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
async Task Tecla(string tecla)
            {
                int longitud = salida.Longitud;
                entrada.Enviar(tecla);
                await Esperar(() => salida.Longitud > longitud, tarea);
            }
```

#### `IntegracionRespuestaPerdidaTests.Esperar`

private static async Task: recibe predicado y tarea sesión; CTS8s. Mientras predicado false, si sesión ya completó espera su resultado (propaga fallos) y falla explícitamente porque terminó antes de confirmación. Si sigue viva retrasa20ms con token. True retorna; timeout cancela y falla. No reinicia tarea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static async Task Esperar(Func<bool> condicion, Task sesion)
    {
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        while (!condicion())
        {
            if (sesion.IsCompleted) { await sesion; Assert.Fail("La sesión terminó antes de confirmar la elección."); }
            await Task.Delay(20, limite.Token);
        }
    }
```

#### `CorteHttp.SendAsync`

Override protected async: extrae de AbsolutePath la ruta después de /partidas/<UUID>/ y registra método+ruta sin cabeceras. Si es POST elecciones registra cuerpo. Si ruta coincide con rutaCorte, método no es GET y aún no cortó, marca cortada; con despues true transmite al handler real, exige éxito y consume bytes antes de marcar ConfirmadaAntesDelCorte; después lanza HttpRequestException. Con despues false lanza sin transmitir. GET y todo tráfico posterior pasan al handler real con ct. No inventa recibos ni respuestas de éxito.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `Teclas.Enviar`

internal void con tecla predeterminada vacía: TryWrite al Channel; simula Enter enviando una línea vacía. No cierra canal ni agrega un salto de línea físico.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Enviar(string tecla = "") => canal.Writer.TryWrite(tecla);
```

#### `Teclas.ReadLineAsync`

Override público devuelve Leer(ct); permite que EntradaCliente lea de modo asíncrono mientras sigue el tráfico.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override ValueTask<string?> ReadLineAsync(CancellationToken ct) => Leer(ct);
```

#### `Teclas.Leer`

private async ValueTask<string?>: espera canal.Reader.ReadAsync(ct) y devuelve línea; cancelación se propaga. El canal nunca se completa aquí, de modo que EOF no es el escenario de este auxiliar.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private async ValueTask<string?> Leer(CancellationToken ct) => await canal.Reader.ReadAsync(ct);
```

#### `Salida.WriteLine`

Override público: lock sobre bloqueo, delega a base.WriteLine(value). Evita que la aserción de Longitud/Contiene lea mientras se añade una línea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override void WriteLine(string? value) { lock (bloqueo) base.WriteLine(value); }
```

#### `Salida.Write`

Override público con el mismo bloqueo y base.Write(value); serializa también escrituras sin salto. No filtra contenido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override void Write(string? value) { lock (bloqueo) base.Write(value); }
```

#### `Salida.Contiene`

internal bool: con lock devuelve ToString().Contains(texto), sensible a mayúsculas por defecto. No espera ni consume contenido; Esperar controla repetición y plazo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal bool Contiene(string texto) { lock (bloqueo) return ToString().Contains(texto); }
```

### Recorridos y ejemplos

Normal: Corte posterior a confirmar elección: solo un POST, consulta de recibo y revisión+1. Corte previo: GET recibo antes del segundo POST, cuerpo idéntico y misma revisión+1.

Alternativo o de error: Abandono incierto requiere Sí: No no envía; perder respuesta no suma victoria ni cambia marcador y repetir abandono ya confirmado no aumenta revisión.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/InterfazConsolaTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/InterfazConsolaTests.md)

## test/proyecto-integrador-final-tests/InterfazConsolaTests.cs

Fuente: [test/proyecto-integrador-final-tests/InterfazConsolaTests.cs](../test/proyecto-integrador-final-tests/InterfazConsolaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Ensaya selección, renderizado adaptable, mensajes y filtrado de controles con entrada/salida en memoria. No usa una terminal real.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartaVisible](archivos/src/cliente/VistasConsola.md), [InterfazConsola](archivos/src/cliente/InterfazConsola.md), [JugadorVisible](archivos/src/cliente/VistasConsola.md), [Ronda](archivos/src/servidor/Ronda.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class InterfazConsolaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos; auxiliar Carta produce modelos visibles. Cada writer/reader de los casos se descarta donde se declara using.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `InterfazConsolaTests.Carta`

private static: recibe id y nombre y devuelve CartaVisible con esos valores y diccionario fijo Fuerza750, Velocidad550, Resistencia500, Inteligencia250, Energía500, Habilidad450. No crea Personaje de dominio ni comprueba nombre.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static CartaVisible Carta(int id, string nombre) => new(id, nombre,
        new Dictionary<string, int>
        {
            ["Fuerza"] = 750, ["Velocidad"] = 550, ["Resistencia"] = 500,
            ["Inteligencia"] = 250, ["Energía"] = 500, ["Habilidad"] = 450
        });
```

#### `InterfazConsolaTests.PedirCarta_ReintentaEntradasInvalidasYConservaLaMano`

Configura ancho30 y mano IDs15 Maga/30 Arquero. StringReader contiene 2, texto, l, Enter y sobra. PedirCarta debe retornar la segunda referencia, conservar longitud2 y dejar sobra sin leer. La salida debe mostrar foco > Arquero e Inteligencia. Así demuestra reintento y consumo exacto hasta confirmación, no selección por número.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PedirCarta_ReintentaEntradasInvalidasYConservaLaMano()
    {
        using var entrada = new StringReader("2\ntexto\nl\n\nsobra\n");
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(entrada, salida, ancho: 30);
        CartaVisible[] mano = [Carta(15, "Maga"), Carta(30, "Arquero")];

        Assert.Same(mano[1], interfaz.PedirCarta(mano));
        Assert.Equal(2, mano.Length);
        Assert.Equal("sobra", entrada.ReadLine());
        Assert.Contains("> Arquero", salida.ToString());
        Assert.Contains("Inteligencia", salida.ToString());
    }
```

#### `InterfazConsolaTests.ManoVacia_SePuedeMostrarPeroNoSolicitar`

Con TextReader.Null, MostrarMano([]) debe imprimir No hay cartas disponibles. PedirCarta([]) debe lanzar InvalidOperationException antes de esperar input. No se prueba el arte ni ancho.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ManoVacia_SePuedeMostrarPeroNoSolicitar()
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida);

        interfaz.MostrarMano([]);

        Assert.Contains("No hay cartas disponibles", salida.ToString());
        Assert.Throws<InvalidOperationException>(() => interfaz.PedirCarta([]));
    }
```

#### `InterfazConsolaTests.PedirAtributo_OfreceLosSeisAtributos`

Theory con pasos/esperado (0,Fuerza),(1,Velocidad),(2,Resistencia),(3,Inteligencia),(4,Energía),(5,Habilidad). Genera pasos líneas s seguidas de Enter; PedirAtributo sin lista explícita debe confirmar la posición correspondiente. No usa flechas reales ni valida otras teclas.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(0, "Fuerza")]
    [InlineData(1, "Velocidad")]
    [InlineData(2, "Resistencia")]
    [InlineData(3, "Inteligencia")]
    [InlineData(4, "Energía")]
    [InlineData(5, "Habilidad")]
    public void PedirAtributo_OfreceLosSeisAtributos(int pasos, string esperado)
    {
        var interfaz = new InterfazConsola(new StringReader(string.Concat(Enumerable.Repeat("s\n", pasos)) + "\n"), TextWriter.Null);

        Assert.Equal(esperado, interfaz.PedirAtributo());
    }
```

#### `InterfazConsolaTests.PedirAtributo_RespetaLasOpcionesDeLaSolicitud`

Ofrece solo Habilidad e introduce 2 y Enter; la entrada no válida no agrega opciones y el resultado es Habilidad. No prueba una lista vacía ni todas las permutaciones.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PedirAtributo_RespetaLasOpcionesDeLaSolicitud()
    {
        var interfaz = new InterfazConsola(new StringReader("2\n\n"), TextWriter.Null);

        Assert.Equal("Habilidad", interfaz.PedirAtributo(["Habilidad"]));
    }
```

#### `InterfazConsolaTests.FinDeEntrada_TerminaLaSolicitud`

Usa lector s seguido de EOF. PedirCarta con una carta debe lanzar por ausencia de confirmación; reutilizar el lector agotado para PedirAtributo también lanza EndOfStreamException. No se envía jugada de red.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void FinDeEntrada_TerminaLaSolicitud()
    {
        var interfaz = new InterfazConsola(new StringReader("s\n"), TextWriter.Null);

        Assert.Throws<EndOfStreamException>(() => interfaz.PedirCarta([Carta(1, "Maga")]));
        Assert.Throws<EndOfStreamException>(() => interfaz.PedirAtributo());
    }
```

#### `InterfazConsolaTests.MostrarMano_AdaptaLasFichasAlAncho`

Theory (25,false),(100,true). Renderiza Maga con ESC y Arquero. Exige que ESC desaparezca y Habilidad siga presente. Comprueba si alguna línea contiene ambos nombres: solo debe ocurrir a ancho100. No asegura el ancho de cada carácter Unicode ni terminal específica.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(25, false)]
    [InlineData(100, true)]
    public void MostrarMano_AdaptaLasFichasAlAncho(int ancho, bool enParalelo)
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida, ancho);

        interfaz.MostrarMano([Carta(1, "Maga\u001b"), Carta(2, "Arquero")]);

        string resultado = salida.ToString();
        Assert.DoesNotContain('\u001b', resultado);
        Assert.Contains("Habilidad", resultado);
        Assert.Equal(enParalelo, resultado.Split('\n').Any(
            linea => linea.Contains("Maga") && linea.Contains("Arquero")));
    }
```

#### `InterfazConsolaTests.MostrarCombate_PresentaElResultadoConfirmado`

Theory anchos30 y100. Envía duelo Fuerza de Maga/Arquero ambos750, ganador Mateo, marcador 0/1 y próximo elector Mateo. Exige atributo, ambas cartas, valor750, Ganador, Lucía0/Mateo1 y texto de elector. La vista no calcula el empate: recibe ganador ya confirmado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(30)]
    [InlineData(100)]
    public void MostrarCombate_PresentaElResultadoConfirmado(int ancho)
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida, ancho);

        interfaz.MostrarCombate(new("Fuerza", new("Lucía", "Maga", 750),
            new("Mateo", "Arquero", 750), "Mateo", 0, 1, "Mateo"));

        string resultado = salida.ToString();
        Assert.Contains("Fuerza", resultado);
        Assert.Contains("Maga", resultado);
        Assert.Contains("Arquero", resultado);
        Assert.Contains("750", resultado);
        Assert.Contains("Ganador", resultado);
        Assert.Contains("Lucía 0 / Mateo 1", resultado);
        Assert.Contains("Mateo elige el atributo", resultado);
    }
```

#### `InterfazConsolaTests.PresentaMarcadorEstadosYCierresConLosNombresRecibidos`

Muestra Envío/Confirmación pendiente, dos esperas idénticas, ronda29 con Lucía14/Mateo15, ganadores de ronda/partida y error. Exige texto de estado, una sola línea de espera (deduplicación), marcador y nombres, y prefijo ERROR. No verifica colores ni un cierre de proceso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PresentaMarcadorEstadosYCierresConLosNombresRecibidos()
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida);
        var lucia = new JugadorVisible("Lucía", 14);
        var mateo = new JugadorVisible("Mateo", 15);

        interfaz.MostrarEstado("Envío", "Confirmación pendiente");
        interfaz.MostrarEsperando("Esperando a Mateo");
        interfaz.MostrarEsperando("Esperando a Mateo");
        interfaz.MostrarMarcador(lucia, mateo, 29);
        interfaz.MostrarGanadorRonda(mateo);
        interfaz.MostrarGanadorPartida(mateo);
        interfaz.MostrarError("Servidor inaccesible");

        string resultado = salida.ToString();
        Assert.Contains("Confirmación pendiente", resultado);
        Assert.Single(resultado.Split('\n').Where(linea => linea.Contains("Esperando a Mateo")));
        Assert.Contains("Ronda 29", resultado);
        Assert.Contains("Lucía 14 - 15 Mateo", resultado);
        Assert.Contains("Ganador de la ronda: Mateo", resultado);
        Assert.Contains("Ganador de la partida: Mateo", resultado);
        Assert.Contains("ERROR: Servidor inaccesible", resultado);
    }
```

### Recorridos y ejemplos

Normal: Con dos cartas y entrada 2/texto/l/Enter, las líneas numérica y de texto no confirman; l mueve foco a Arquero y Enter devuelve esa referencia.

Alternativo o de error: Mostrar mano vacía es válido pero pedir carta vacía lanza; EOF antes de confirmación lanza EndOfStreamException.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/JugadorTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/JugadorTests.md)

## test/proyecto-integrador-final-tests/JugadorTests.cs

Fuente: [test/proyecto-integrador-final-tests/JugadorTests.cs](../test/proyecto-integrador-final-tests/JugadorTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba el estado inicial y la propiedad de las colecciones de la mano del modelo local. Dos Facts síncronos sin red ni SQLite.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](archivos/test/proyecto-integrador-final-tests/CartasDePrueba.md), [Jugador](archivos/src/servidor/Jugador.md), [Personaje](archivos/src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class JugadorTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos de fixture; cada prueba crea objetos nuevos y xUnit no comparte jugadores.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `JugadorTests.IniciaConNombreManoVaciaYSinVictorias`

Construye Jugador("Ana"), comprueba nombre literal, mano vacía y cero rondas; llama dos veces SumarRondaGanada y exige 2. No acredita límite de quince ni validación de nombre: solo la inicialización y el incremento.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void IniciaConNombreManoVaciaYSinVictorias()
    {
        var jugador = new Jugador("Ana");

        Assert.Equal("Ana", jugador.GetNombre());
        Assert.Empty(jugador.GetCartas());
        Assert.Equal(0, jugador.GetRondasGanadas());

        jugador.SumarRondaGanada();
        jugador.SumarRondaGanada();

        Assert.Equal(2, jugador.GetRondasGanadas());
    }
```

#### `JugadorTests.AcumulaCartasQuitaUnaYEntregaElResto`

Construye tres referencias distintas con CartasDePrueba; recibe las dos primeras y altera el array origen para comprobar que la mano no cambia. Agrega la tercera y compara el orden [primera,segunda,tercera]. Modifica el array de GetCartas y exige que la primera referencia interna siga igual. Quita segunda, compara [primera,tercera], entrega ese orden y exige vaciado; una segunda entrega debe ser vacía. No prueba duplicados, null ni igualdad por contenido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void AcumulaCartasQuitaUnaYEntregaElResto()
    {
        var jugador = new Jugador("Ana");
        var primera = CartasDePrueba.Crear();
        var segunda = CartasDePrueba.Crear();
        var tercera = CartasDePrueba.Crear();

        Personaje[] recibidas = [primera, segunda];
        jugador.RecibirCartas(recibidas);
        recibidas[0] = tercera;
        jugador.RecibirCartas([tercera]);
        Assert.Equal(new[] { primera, segunda, tercera }, jugador.GetCartas());

        jugador.GetCartas()[0] = tercera;
        Assert.Same(primera, jugador.GetCartas()[0]);

        jugador.QuitarCarta(segunda);
        Assert.Equal(new[] { primera, tercera }, jugador.GetCartas());
        Assert.Equal(new[] { primera, tercera }, jugador.EntregarCartas());
        Assert.Empty(jugador.GetCartas());
        Assert.Empty(jugador.EntregarCartas());
    }
```

### Recorridos y ejemplos

Normal: Ana recibe A y B, luego C, retira B y entrega [A,C]; la mano termina vacía.

Alternativo o de error: Modificar el array recibido originalmente o el devuelto por GetCartas no reemplaza las posiciones de la mano; entregar otra vez retorna vacío. No hay un caso explícito de constructor inválido en este archivo.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/MazoTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/MazoTests.md)

## test/proyecto-integrador-final-tests/MazoTests.cs

Fuente: [test/proyecto-integrador-final-tests/MazoTests.cs](../test/proyecto-integrador-final-tests/MazoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Verifica copia, prefijos repartidos, devolución y conservación de cartas durante mezcla. Las aserciones usan identidad de objeto para no depender del orden aleatorio.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](archivos/test/proyecto-integrador-final-tests/CartasDePrueba.md), [Mazo](archivos/src/servidor/Mazo.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class MazoTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin estado de instancia; cada caso crea su lista y mazo.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `MazoTests.CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal`

Crea siete cartas únicas, copia a una lista, construye Mazo y limpia la lista de origen. Count debe seguir en 7. Dos Repartir(3) deben retornar exactamente los slices [0..3] y [3..6] y dejar 4 y 1. Devolver concatena los seis al final; Repartir(7) debe producir carta[6] seguida de cartas[0..6] y vaciar. No invoca Mezclar: el orden esperado es determinista.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal()
    {
        var cartas = Enumerable.Range(0, 7).Select(_ => CartasDePrueba.Crear()).ToArray();
        var origen = cartas.ToList();
        var mazo = new Mazo(origen);
        origen.Clear();

        Assert.Equal(7, mazo.GetCantidad());
        var paraElPrimero = mazo.Repartir(3);
        Assert.Equal(cartas[..3], paraElPrimero);
        Assert.Equal(4, mazo.GetCantidad());

        var paraElSegundo = mazo.Repartir(3);
        Assert.Equal(cartas[3..6], paraElSegundo);
        Assert.Equal(1, mazo.GetCantidad());

        mazo.Devolver([.. paraElPrimero, .. paraElSegundo]);
        Assert.Equal(7, mazo.GetCantidad());
        Assert.Equal(new[] { cartas[6] }.Concat(cartas[..6]), mazo.Repartir(7));
        Assert.Equal(0, mazo.GetCantidad());
    }
```

#### `MazoTests.MezclarConservaTodasLasCartas`

Theory con cantidades 0,1,6. Crea cantidad referencias, mezcla y exige mismo Count. Reparte todas y, para cada carta original, Assert.Single busca ReferenceEquals: no debe perder ni duplicar ninguna. No exige que el resultado cambie de orden, ni mide uniformidad estadística.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    public void MezclarConservaTodasLasCartas(int cantidad)
    {
        var cartas = Enumerable.Range(0, cantidad).Select(_ => CartasDePrueba.Crear()).ToList();
        var mazo = new Mazo(cartas);

        mazo.Mezclar();

        Assert.Equal(cantidad, mazo.GetCantidad());
        var repartidas = mazo.Repartir(cantidad);
        Assert.All(cartas, carta => Assert.Single(repartidas, repartida => ReferenceEquals(carta, repartida)));
    }
```

### Recorridos y ejemplos

Normal: Siete cartas: repartir 3 deja 4; repartir 3 deja 1; devolver seis deja 7 y el siguiente reparto empieza por la carta que nunca salió.

Alternativo o de error: Con 0 o 1 carta Mezclar conserva la misma colección sin requerir un intercambio. Las cantidades inválidas de Repartir no están ensayadas.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/PantallasConsolaTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/PantallasConsolaTests.md)

## test/proyecto-integrador-final-tests/PantallasConsolaTests.cs

Fuente: [test/proyecto-integrador-final-tests/PantallasConsolaTests.cs](../test/proyecto-integrador-final-tests/PantallasConsolaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba composición de paneles de preparación, turno, envío, pausa y cierres usando una mesa y tres cartas visibles fijas.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartaVisible](archivos/src/cliente/VistasConsola.md), [Combate](archivos/src/servidor/Combate.md), [EstadoEnvio](archivos/src/cliente/VistasConsola.md), [InterfazConsola](archivos/src/cliente/InterfazConsola.md), [Jugador](archivos/src/servidor/Jugador.md), [MesaVisible](archivos/src/cliente/VistasConsola.md), [Ronda](archivos/src/servidor/Ronda.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class PantallasConsolaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Mesa static readonly: local Lucía, marcador4/3, ronda8, combate1 y combates0/0. Cartas static readonly: Lysander fuerza700/velocidad550, Reched350/700 y Vigía500/350. No se mutan en los casos; with crea copias de Mesa.

Declaraciones reales de estado de `PantallasConsolaTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private static readonly MesaVisible Mesa = new("Lucía", new("Lucía", 4), new("Mateo", 3), 8, 1, 0, 0);
```

```csharp
private static readonly CartaVisible[] Cartas = [
        new(1, "Lysander", new Dictionary<string, int> { ["Fuerza"] = 700, ["Velocidad"] = 550 }),
        new(2, "Reched", new Dictionary<string, int> { ["Fuerza"] = 350, ["Velocidad"] = 700 }),
        new(3, "Vigía", new Dictionary<string, int> { ["Fuerza"] = 500, ["Velocidad"] = 350 })
    ];
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `PantallasConsolaTests.PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles`

Dibuja preparación de Lucía, espera inicial con Mateo ausente e inicio de ronda. Exige etiquetas Jugador local, Estado, Participantes, A cargo del operador, apertura de Lucía, mano y rondas ganadas. Solo inspecciona texto, no red ni cambios de fase.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarPreparacion("Lucía", "Preparando la partida…");
        interfaz.MostrarEsperaInicial("Lucía", "Lucía presente · Mateo por incorporarse", "Falta Mateo");
        interfaz.MostrarInicioRonda(Mesa, "Lucía", Cartas);

        string salida = texto.ToString();
        Assert.Contains("Jugador local", salida);
        Assert.Contains("Estado", salida);
        Assert.Contains("Participantes", salida);
        Assert.Contains("A cargo del operador", salida);
        Assert.Contains("Lucía abre la ronda", salida);
        Assert.Contains("Tu mano", salida);
        Assert.Contains("Rondas ganadas", salida);
    }
```

#### `PantallasConsolaTests.Apertura_UsaMesaConFichasEncabezadoYPie`

Theory (100,true),(35,false). Lee d y Enter en PedirCarta con abre true; retorna Cartas[1]. Exige encabezado GUERRA/Lucía, ronda8/combate1, mano, Tu turno, aviso de atributo posterior y borde. Any sobre líneas identifica si Lysander/Reched están juntos según ancho. La comparación de aviso normaliza saltos de línea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(100, true)]
    [InlineData(35, false)]
    public void Apertura_UsaMesaConFichasEncabezadoYPie(int ancho, bool paralelas)
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader("d\n\n"), texto, ancho);

        Assert.Same(Cartas[1], interfaz.PedirCarta(Mesa, Cartas, abre: true));

        string salida = texto.ToString();
        Assert.Contains("GUERRA / Lucía", salida);
        Assert.Contains("Ronda 8 · Combate 1", salida);
        Assert.Contains("Tu mano", salida);
        Assert.Contains("Tu turno", salida);
        Assert.Contains("Después elegís el atributo", salida.ReplaceLineEndings(" "));
        Assert.Contains("┌", salida);
        Assert.Equal(paralelas, salida.Split('\n').Any(l => l.Contains("Lysander") && l.Contains("Reched")));
    }
```

#### `PantallasConsolaTests.EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo`

Dibuja espera por Mateo y pide respuesta con abre false, confirmando primera carta por Enter. Exige mensajes de espera, disponibles, Fuerza/Velocidad, foco y marcador; excluye Atributo del combate y aviso de elegir atributo después. Demuestra distinción visual entre apertura y respuesta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader("\n"), texto, 100);

        interfaz.MostrarEsperaRival(Mesa, "Mateo", Cartas);
        Assert.Same(Cartas[0], interfaz.PedirCarta(Mesa, Cartas, abre: false));

        string salida = texto.ToString();
        Assert.Contains("Mateo está eligiendo", salida);
        Assert.Contains("Esperá la solicitud para responder", salida);
        Assert.Contains("Cartas disponibles", salida);
        Assert.Contains("Fuerza", salida);
        Assert.Contains("Velocidad", salida);
        Assert.Contains("> Lysander", salida);
        Assert.Contains("Te toca responder", salida);
        Assert.Contains("Marcador", salida);
        Assert.DoesNotContain("Atributo del combate", salida);
        Assert.DoesNotContain("Después elegís el atributo", salida);
    }
```

#### `PantallasConsolaTests.EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados`

Renderiza los tres EstadoEnvio, dos suspensiones (confirmada true y false) y dos interrupciones (remota confirmada y salida local). Recorre ocho textos esperados: Envío en curso, Confirmación pendiente, Recepción confirmada, Pausa confirmada, Estado actual sin confirmar, Último marcador confirmado, Final confirmado y Salida local. No comprueba que la comunicación haya ocurrido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Enviando);
        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Pendiente);
        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Recibido);
        interfaz.MostrarSuspension(Mesa, "Mateo perdió la sesión", true);
        interfaz.MostrarSuspension(Mesa, "No se pudo contactar al servidor", false);
        interfaz.MostrarInterrupcion("Lucía", "Mateo abandonó la partida", "La partida terminó", true);
        interfaz.MostrarInterrupcion("Lucía", "Cierre solicitado", "La partida queda guardada", false);

        string salida = texto.ToString();
        foreach (string esperado in new[] { "Envío en curso", "Confirmación pendiente", "Recepción confirmada",
            "Pausa confirmada", "Estado actual sin confirmar", "Último marcador confirmado", "Final confirmado", "Salida local" })
            Assert.Contains(esperado, salida);
    }
```

#### `PantallasConsolaTests.ResultadoYRondas_UsanDueloVictoriaYPlanilla`

Muestra combate Fuerza Lysander700 frente Reched350 con Lucía vencedora; cierra ronda con combates2/1 y próxima mano Mateo; cierra partida Lucía15/Mateo11. Exige cartas, atributo, ganador y próximo elector, ronda9, victoria de Lucía, ambos totales y Enter para salir. No acredita la lógica de puntuación: son datos de presentación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ResultadoYRondas_UsanDueloVictoriaYPlanilla()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarCombate(Mesa, new("Fuerza", new("Lucía", "Lysander", 700),
            new("Mateo", "Reched", 350), "Lucía", 1, 0, "Mateo"));
        interfaz.MostrarCierreRonda(Mesa with { CombatesJ1 = 2, CombatesJ2 = 1 }, "Lucía", "Mateo");
        interfaz.MostrarCierrePartida("Lucía", "Lucía", new("Lucía", 15), new("Mateo", 11));

        string salida = texto.ToString();
        Assert.Contains("Atributo · Fuerza", salida);
        Assert.Contains("Lucía · Lysander", salida);
        Assert.Contains("Mateo · Reched", salida);
        Assert.Contains("Ganador", salida);
        Assert.Contains("Mateo elige el atributo", salida);
        Assert.Contains("Lucía gana la ronda", salida);
        Assert.Contains("Mateo abre la ronda 9", salida);
        Assert.Contains("Lucía gana la partida", salida);
        Assert.Contains("15 rondas ganadas", salida);
        Assert.Contains("11 rondas ganadas", salida);
        Assert.Contains("Pulsá Enter para salir", salida);
    }
```

### Recorridos y ejemplos

Normal: En ronda8/combate1, d mueve el foco a Reched y Enter la confirma; ancho100 coloca fichas en paralelo.

Alternativo o de error: A ancho35 las fichas no comparten línea; al responder no se solicita atributo. La suspensión no confirmada usa un mensaje diferente de la pausa confirmada.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/PartidaTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/PartidaTests.md)

## test/proyecto-integrador-final-tests/PartidaTests.cs

Fuente: [test/proyecto-integrador-final-tests/PartidaTests.cs](../test/proyecto-integrador-final-tests/PartidaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Protege la precondición de seis cartas antes del reparto en el coordinador local. No prueba una partida completa ni la API.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](archivos/test/proyecto-integrador-final-tests/CartasDePrueba.md), [Jugador](archivos/src/servidor/Jugador.md), [Mazo](archivos/src/servidor/Mazo.md), [Partida](archivos/src/servidor/Partida.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class PartidaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Dos campos privados readonly inicializan jugadores Ana y Bruno para cada instancia de prueba. Los casos de Theory reciben fixtures nuevos.

Declaraciones reales de estado de `PartidaTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Jugador primero = new("Ana");
```

```csharp
private readonly Jugador segundo = new("Bruno");
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `PartidaTests.RechazaUnMazoInsuficienteAntesDeRepartir`

Theory con 0 y 5. Crea exactamente cantidad personajes mediante Enumerable.Range y CartasDePrueba, construye Partida con ambos jugadores y captura InvalidOperationException de Jugar. Compara mensaje exacto y luego Count y manos para demostrar que la guarda antecede al reparto. No comprueba el comportamiento con 6 ni ejecuta una ronda.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void RechazaUnMazoInsuficienteAntesDeRepartir(int cantidad)
    {
        var mazo = new Mazo(Enumerable.Range(0, cantidad).Select(_ => CartasDePrueba.Crear()).ToList());
        var partida = new Partida(primero, segundo, mazo);

        var error = Assert.Throws<InvalidOperationException>(() => partida.Jugar());

        Assert.Equal("No se puede jugar porque no hay suficientes cartas.", error.Message);

        Assert.Equal(cantidad, mazo.GetCantidad());
        Assert.Empty(primero.GetCartas());
        Assert.Empty(segundo.GetCartas());
    }
```

### Recorridos y ejemplos

Normal: Con cantidad 5, Jugar lanza el mensaje de mazo insuficiente, conserva cinco cartas y ambas manos vacías.

Alternativo o de error: Cantidad 0 recorre la misma guarda. No hay recorrido exitoso de Jugar en este archivo; el coordinador local no incorpora elecciones de clientes.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/PersonajeTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/PersonajeTests.md)

## test/proyecto-integrador-final-tests/PersonajeTests.cs

Fuente: [test/proyecto-integrador-final-tests/PersonajeTests.cs](../test/proyecto-integrador-final-tests/PersonajeTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba conservación de datos y guardas de construcción del personaje local, incluidos los límites de atributos.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [Personaje](archivos/src/servidor/Personaje.md), [TipoAtributo](archivos/src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class PersonajeTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos ni recursos externos. Cada lambda de Assert.Throws construye una instancia descartada.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `PersonajeTests.ConservaNombreAsciiYLosSeisAtributos`

Construye Lysander con arte de barra y diagonal y valores 250,750,350,650,450,550; compara nombre, ASCII y el getter de cada constante de TipoAtributo. Acredita mapeo de los seis campos, no una comparación de combate.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void ConservaNombreAsciiYLosSeisAtributos()
    {
        var carta = new Personaje("Lysander", " /\\", 250, 750, 350, 650, 450, 550);

        Assert.Equal("Lysander", carta.GetNombre());
        Assert.Equal(" /\\", carta.GetAscii());
        Assert.Equal(250, carta.GetAtributo(TipoAtributo.Fuerza));
        Assert.Equal(750, carta.GetAtributo(TipoAtributo.Velocidad));
        Assert.Equal(350, carta.GetAtributo(TipoAtributo.Resistencia));
        Assert.Equal(650, carta.GetAtributo(TipoAtributo.Inteligencia));
        Assert.Equal(450, carta.GetAtributo(TipoAtributo.Energia));
        Assert.Equal(550, carta.GetAtributo(TipoAtributo.Habilidad));
    }
```

#### `PersonajeTests.AceptaUmbralesInclusivosYAsciiVacio`

Construye Carta con (650,350,500,500,500,500), exige arte vacío y getters fuerza 650 y velocidad 350. Demuestra inclusión de los dos umbrales, no todos los límites exteriores.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void AceptaUmbralesInclusivosYAsciiVacio()
    {
        var carta = new Personaje("Carta", "", 650, 350, 500, 500, 500, 500);

        Assert.Empty(carta.GetAscii());
        Assert.Equal(650, carta.GetAtributo(TipoAtributo.Fuerza));
        Assert.Equal(350, carta.GetAtributo(TipoAtributo.Velocidad));
    }
```

#### `PersonajeTests.RechazaAtributosQueIncumplenUnaRegla`

Theory fija energía y habilidad en 500 y varía (fuerza,velocidad,resistencia,inteligencia): (649,350,501,500) carece de ≥650; (650,351,499,500) carece de ≤350; (650,350,499,500) suma 2999; (650,350,501,500) suma 3001; (750,249,501,500) y (751,250,499,500) salen de [250,750]; (500,500,500,500) no alcanza umbrales. Cada construcción debe lanzar ArgumentException; no se verifican mensaje ni nombre de parámetro.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(649, 350, 501, 500)]
    [InlineData(650, 351, 499, 500)]
    [InlineData(650, 350, 499, 500)]
    [InlineData(650, 350, 501, 500)]
    [InlineData(750, 249, 501, 500)]
    [InlineData(751, 250, 499, 500)]
    [InlineData(500, 500, 500, 500)]
    public void RechazaAtributosQueIncumplenUnaRegla(int fuerza, int velocidad, int resistencia, int inteligencia)
    {
        Assert.Throws<ArgumentException>(() =>
            new Personaje("Carta", "", fuerza, velocidad, resistencia, inteligencia, 500, 500));
    }
```

#### `PersonajeTests.RechazaNombreSinTexto`

Theory con null, cadena vacía y tres espacios. La supresión ! solo evita advertencia estática: el null llega al constructor. Con atributos válidos y arte vacío espera ArgumentException; no comprueba trim ni longitud máxima.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RechazaNombreSinTexto(string? nombre)
    {
        Assert.Throws<ArgumentException>(() => new Personaje(nombre!, "", 650, 350, 500, 500, 500, 500));
    }
```

#### `PersonajeTests.RechazaAsciiNulo`

Con nombre y atributos válidos pasa null! como arte y exige ArgumentException. No comprueba otros caracteres ni límites de tamaño del arte.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void RechazaAsciiNulo()
    {
        Assert.Throws<ArgumentException>(() => new Personaje("Carta", null!, 650, 350, 500, 500, 500, 500));
    }
```

### Recorridos y ejemplos

Normal: (650,350,500,500,500,500) se acepta y ASCII vacío se conserva.

Alternativo o de error: Los seis valores 500 suman 3000 pero son rechazados por los umbrales; nombre null, vacío o espacios y ASCII null también se rechazan.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/PresentacionRedTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/PresentacionRedTests.md)

## test/proyecto-integrador-final-tests/PresentacionRedTests.cs

Fuente: [test/proyecto-integrador-final-tests/PresentacionRedTests.cs](../test/proyecto-integrador-final-tests/PresentacionRedTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Verifica que la vista de red reproduce el marcador histórico del resultado leído y neutraliza controles del nombre remoto.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [EstadoClienteTests](archivos/test/proyecto-integrador-final-tests/EstadoClienteTests.md), [InterfazConsola](archivos/src/cliente/InterfazConsola.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class PresentacionRedTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Sin campos; fixtures de EstadoClienteTests y records with producen datos locales sin HTTP.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `PresentacionRedTests.MarcadorDelResultadoCorrespondeASuRondaYNeutralizaTextoRemoto`

Theory anchos38/100. Construye resultado (1,1), cierre (1,2) con ganador ronda J2 y posterior (2,1) J1. Presentación actual ronda2/combate2 y marcador7/9, nombre remoto con ESC. MostrarRed recibe explícitamente cierre, sin selección (-1) ni menús. Exige gana la ronda y Ada1; excluye Ada9, ESC y Tu turno. Acredita reconstrucción histórica y sanitización, no resultado calculado por servidor.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(38)]
    [InlineData(100)]
    public void MarcadorDelResultadoCorrespondeASuRondaYNeutralizaTextoRemoto(int ancho)
    {
        Guid id = Guid.NewGuid();
        var primero = EstadoClienteTests.Resultado(1, 1);
        var cierre = EstadoClienteTests.Resultado(1, 2) with { GanadorRonda = "J2" };
        var posterior = EstadoClienteTests.Resultado(2, 1) with { GanadorCombate = "J1" };
        var vista = EstadoClienteTests.Vista(id) with
        {
            NumeroRonda = 2, NumeroCombate = 2, Marcador = new() { ["J1"] = 7, ["J2"] = 9 },
            Nombres = new() { ["J1"] = "Ana\u001b[31m", ["J2"] = "Ada" }, Resultados = [primero, cierre, posterior]
        };
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader(""), salida, ancho);
        interfaz.MostrarRed("Ana", vista, true, "En línea", cierre, -1, false, 0, false, false, false);
        string texto = salida.ToString();
        Assert.Contains("gana la ronda", texto);
        Assert.Contains("Ada 1", texto);
        Assert.DoesNotContain("Ada 9", texto);
        Assert.DoesNotContain('\u001b', texto);
        Assert.DoesNotContain("Tu turno", texto);
    }
```

### Recorridos y ejemplos

Normal: Resultados de ronda1 con cierre J2 y posterior de ronda2: mostrar el cierre de ronda1 debe decir Ada1, aunque la presentación actual lleva Ada9.

Alternativo o de error: Con nombre Ana seguido de ESC[31m no debe salir ESC; mostrar resultado no habilita Tu turno. Se ensayan anchos38 y100.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/RondaTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/RondaTests.md)

## test/proyecto-integrador-final-tests/RondaTests.cs

Fuente: [test/proyecto-integrador-final-tests/RondaTests.cs](../test/proyecto-integrador-final-tests/RondaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Comprueba elección inicial, alternancia por combate y cierre al alcanzar dos victorias, con combates reales del modelo ya resueltos.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](archivos/test/proyecto-integrador-final-tests/CartasDePrueba.md), [Combate](archivos/src/servidor/Combate.md), [Jugador](archivos/src/servidor/Jugador.md), [Ronda](archivos/src/servidor/Ronda.md), [TipoAtributo](archivos/src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public class RondaTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Campos privados readonly mano=Ana y rival=Bruno, nuevos por caso; el auxiliar agrega cartas y los combates las consumen.

Declaraciones reales de estado de `RondaTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Jugador mano = new("Ana");
```

```csharp
private readonly Jugador rival = new("Bruno");
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `RondaTests.EmpiezaSinCombatesYEligeElJugadorMano`

Construye Ronda(mano,rival); exige GetCombates vacío, EstaDefinida false y que el próximo elector sea la misma referencia mano. No consulta GetGanador antes de tiempo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void EmpiezaSinCombatesYEligeElJugadorMano()
    {
        var ronda = new Ronda(mano, rival);

        Assert.Empty(ronda.GetCombates());
        Assert.False(ronda.EstaDefinida());
        Assert.Same(mano, ronda.GetQuienEligeProximo());
    }
```

#### `RondaTests.DosVictoriasSeguidasCierranLaRonda`

Theory true/false elige como ganador mano o rival. Agrega primer combate resuelto y exige ronda abierta y próximo elector rival. El segundo invierte elector pero conserva vencedor; exige cierre y ganador por referencia. Reemplaza una posición del array devuelto por GetCombates y confirma que la lista interna conserva primero/segundo: copia defensiva.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DosVictoriasSeguidasCierranLaRonda(bool ganaMano)
    {
        var ronda = new Ronda(mano, rival);
        var ganador = ganaMano ? mano : rival;
        var primero = ResolverCombate(mano, rival, ganador);
        ronda.AgregarCombate(primero);

        Assert.False(ronda.EstaDefinida());
        Assert.Same(rival, ronda.GetQuienEligeProximo());

        var segundo = ResolverCombate(rival, mano, ganador);
        ronda.AgregarCombate(segundo);

        Assert.True(ronda.EstaDefinida());
        Assert.Same(ganador, ronda.GetGanador());
        ronda.GetCombates()[0] = segundo;
        Assert.Equal(new[] { primero, segundo }, ronda.GetCombates());
    }
```

#### `RondaTests.ElTercerCombateDefineLaRondaCuandoVanUnoAUno`

Theory true/false. Fuerza primer triunfo de mano y segundo de rival; exige ronda abierta y elector mano. Agrega tercer combate con vencedor elegido por parámetro y exige cierre, ganador idéntico y longitud tres. No acredita marcador de Partida.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ElTercerCombateDefineLaRondaCuandoVanUnoAUno(bool ganaMano)
    {
        var ronda = new Ronda(mano, rival);
        ronda.AgregarCombate(ResolverCombate(mano, rival, mano));
        ronda.AgregarCombate(ResolverCombate(rival, mano, rival));

        Assert.False(ronda.EstaDefinida());
        Assert.Same(mano, ronda.GetQuienEligeProximo());

        var ganador = ganaMano ? mano : rival;
        ronda.AgregarCombate(ResolverCombate(mano, rival, ganador));

        Assert.True(ronda.EstaDefinida());
        Assert.Same(ganador, ronda.GetGanador());
        Assert.Equal(3, ronda.GetCombates().Length);
    }
```

#### `RondaTests.ResolverCombate`

Auxiliar private static: recibe elector, oponente y ganador esperado, crea carta de fuerza 750 para el participante idéntico al ganador y 650 para el otro. Agrega cada carta a su jugador, selecciona Fuerza, registra ambas, resuelve y devuelve Combate. Asume que ganador es uno de los dos; no lo valida. La comparación por referencia determina las dos ramas de asignación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static Combate ResolverCombate(Jugador elector, Jugador oponente, Jugador ganador)
    {
        var cartaElector = CartasDePrueba.Crear(ReferenceEquals(elector, ganador) ? 750 : 650);
        var cartaOponente = CartasDePrueba.Crear(ReferenceEquals(oponente, ganador) ? 750 : 650);
        elector.RecibirCartas([cartaElector]);
        oponente.RecibirCartas([cartaOponente]);

        var combate = new Combate(elector, oponente);
        combate.SetAtributo(TipoAtributo.Fuerza);
        combate.SetCarta(elector, cartaElector);
        combate.SetCarta(oponente, cartaOponente);
        combate.Resolver();
        return combate;
    }
```

### Recorridos y ejemplos

Normal: Dos victorias de Ana cierran la ronda tras el segundo combate; si quedan uno a uno, el tercero decide.

Alternativo o de error: Cada cierre se parametriza para que también gane Bruno. No hay pruebas de agregar combate no resuelto ni participantes extraños.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/ServidorContratoTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/ServidorContratoTests.md)

## test/proyecto-integrador-final-tests/ServidorContratoTests.cs

Fuente: [test/proyecto-integrador-final-tests/ServidorContratoTests.cs](../test/proyecto-integrador-final-tests/ServidorContratoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Contratos externos del ejecutable servidor: consola operativa, Kestrel y SQLite en directorio temporal. Requieren GUERRA_SERVIDOR_DLL; no se sustituyen por llamadas directas al repositorio.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [Jugador](archivos/src/servidor/Jugador.md), [Partida](archivos/src/servidor/Partida.md), [Personaje](archivos/src/servidor/Personaje.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](archivos/test/proyecto-integrador-final-tests/ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class ServidorContratoTests`.

Declaración: `private sealed record Eleccion(Guid idOperacion, Guid idSolicitud, int revisionEsperada, int idPersonaje, string? atributo);`.

Declaración: `private sealed record Jugador(string Rol, string Credencial, Guid Sesion);`.

Declaración: `private sealed class Servidor : IAsyncDisposable`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

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

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `ServidorContratoTests.Incorporacion_ValidaCredencialYSesionYNoCambiaRevisionConPresencia`

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

#### `ServidorContratoTests.Elecciones_ProtegenManosYDevuelvenElMismoReciboAlReintentar`

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

#### `ServidorContratoTests.EleccionesConcurrentes_ConsumenUnaSolaSolicitud`

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

#### `ServidorContratoTests.SesionAusente_PausaYRecuperaLaMismaSolicitudSinCambiarRevision`

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

#### `ServidorContratoTests.Partida_AlternaElElectorEnEmpatesYFinalizaAlLlegarAQuinceRondas`

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

#### `ServidorContratoTests.Reinicio_InvalidaSesionesYConservaEstadoYReciboConfirmados`

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

#### `ServidorContratoTests.Abandono_RepetirloConservaMarcadorYRevisionFinal`

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

#### `ServidorContratoTests.Revision`

private static int: extrae propiedad revision con GetInt32; JSON faltante o no entero falla, no usa valor por defecto.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static int Revision(JsonElement vista) => vista.GetProperty("revision").GetInt32();
```

#### `ServidorContratoTests.Mano`

private static int[]: enumera mano, proyecta cada idPersonaje a Int32 y materializa nuevo array conservando orden de presentación. No filtra ni consulta catálogo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private static int[] Mano(JsonElement vista) => vista.GetProperty("mano").EnumerateArray()
        .Select(carta => carta.GetProperty("idPersonaje").GetInt32()).ToArray();
```

#### `ServidorContratoTests.Elegir`

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

#### `ServidorContratoTests.Leer`

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

#### `ServidorContratoTests.Error`

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

#### `Servidor.Servidor`

Constructor público del tipo privado: crea directorio de ensayo y asigna proceso=Lanzar(). No crea partida automáticamente; Crear o Iniciar debe hacerlo antes de usar J1/J2.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public Servidor()
        {
            Directory.CreateDirectory(directorio);
            proceso = Lanzar();
        }
```

#### `Servidor.Lanzar`

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

#### `Servidor.Reiniciar`

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

#### `Servidor.Crear`

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

#### `Servidor.Iniciar`

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

#### `Servidor.Renovar`

public async Task: PUT participantes/me/presencia mediante Enviar y exige204; using libera respuesta. No cambia fixture ni revisión.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async Task Renovar(Jugador jugador)
        {
            using var presencia = await Enviar(HttpMethod.Put, "participantes/me/presencia", jugador);
            Assert.Equal(HttpStatusCode.NoContent, presencia.StatusCode);
        }
```

#### `Servidor.CerrarSesion`

public Task: escribe cerrar-sesion, UUID de partida y Rol del record, separados por saltos. No espera confirmación textual; el caller espera efecto con EsperarVista.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public Task CerrarSesion(Jugador jugador) =>
            proceso.StandardInput.WriteLineAsync($"cerrar-sesion\n{partida}\n{jugador.Rol}");
```

#### `Servidor.EsperarVista`

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

#### `Servidor.Incorporar`

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

#### `Servidor.Vista`

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

#### `Servidor.Post`

public Task<HttpResponseMessage>: delega Enviar POST elecciones con record de operación y devuelve respuesta sin consumir. Caller debe liberar.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public Task<HttpResponseMessage> Post(Jugador jugador, Eleccion eleccion) =>
            Enviar(HttpMethod.Post, "elecciones", jugador, eleccion);
```

#### `Servidor.Enviar`

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

#### `Servidor.LeerDato`

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

#### `Servidor.DisposeAsync`

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

#### `Servidor.Detener`

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

### Recorridos y ejemplos

Normal: Dos plazas con tres cartas disjuntas alternan electores; empates dan29 rondas/87 combates y final J2.

Alternativo o de error: Credencial incorrecta401, sesión ajena409, elección inválida422, recibo ajeno404; reinicio conserva solicitud/recibo pero invalida sesiones.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/SesionClienteTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/SesionClienteTests.md)

## test/proyecto-integrador-final-tests/SesionClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/SesionClienteTests.cs](../test/proyecto-integrador-final-tests/SesionClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Ensaya el coordinador asíncrono del cliente con transporte controlado, entrada por canal y reloj sustituible. Permite observar concurrencia, recuperación y navegación sin servidor ni SQLite reales.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md), [ConfiguracionCliente](archivos/src/cliente/ConfiguracionCliente.md), [EleccionCliente](archivos/src/cliente/ContratosPartida.md), [EntradaCliente](archivos/src/cliente/EntradaCliente.md), [EstadoClienteTests](archivos/test/proyecto-integrador-final-tests/EstadoClienteTests.md), [InterfazConsola](archivos/src/cliente/InterfazConsola.md), [PresentacionCliente](archivos/src/cliente/ContratosPartida.md), [ReciboCliente](archivos/src/cliente/ContratosPartida.md), [SesionConfirmada](archivos/src/cliente/ContratosPartida.md), [SesionPartidaCliente](archivos/src/cliente/SesionPartidaCliente.md), [TransporteClienteTests](archivos/test/proyecto-integrador-final-tests/TransporteClienteTests.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class SesionClienteTests`.

Declaración: `internal sealed class Lineas : TextReader`.

Declaración: `internal sealed class Escenario : IAsyncDisposable`.

Declaración: `internal sealed class RelojCliente : TimeProvider`.

Declaración: `private sealed class Temporizador(RelojCliente reloj, TimerCallback callback, object? state) : ITimer`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Lineas internal sealed deriva TextReader y posee Channel<string?> ilimitado; null puede simular EOF. Escenario internal sealed implementa IAsyncDisposable: Reloj readonly, Vista mutable, Entrada readonly, contadores Consultas/Presencias/Elecciones/Recibos/Abandonos inicialmente0, Perdida nullable y función Interceptar nullable, lista Cuerpos, recibo nullable, CTS cierre, StringWriter salida, transporte http y `Task<bool> Tarea`. Texto proyecta writer sin bloqueo propio. RelojCliente internal sealed deriva TimeProvider: ahora=UtcNow inicial y lista de Temporizador; ambos se consultan bajo lock de lista. Temporizador privado con constructor primario captura reloj/callback/state e implementa ITimer, guarda Cerrado inicialmente false, Vence, Callback y State; no modela periodicidad automáticamente.

Declaraciones reales de estado de `Lineas` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private readonly Channel<string?> canal = Channel.CreateUnbounded<string?>();
```

Declaraciones reales de estado de `Escenario` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal readonly RelojCliente Reloj = new();
```

```csharp
internal PresentacionCliente Vista;
```

```csharp
internal readonly Lineas Entrada = new();
```

```csharp
internal int Consultas, Presencias, Elecciones, Recibos, Abandonos;
```

```csharp
internal string? Perdida;
```

```csharp
internal Func<HttpRequestMessage, Task<HttpResponseMessage?>>? Interceptar;
```

```csharp
internal readonly List<string> Cuerpos = [];
```

```csharp
private ReciboCliente? recibo;
```

```csharp
private readonly CancellationTokenSource cierre = new();
```

```csharp
private readonly StringWriter salida = new();
```

```csharp
private readonly ClientePartidaHttp http;
```

```csharp
internal Task<bool> Tarea;
```

```csharp
internal string Texto => salida.ToString();
```

Declaraciones reales de estado de `RelojCliente` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
private DateTimeOffset ahora = DateTimeOffset.UtcNow;
```

```csharp
private readonly List<Temporizador> temporizadores = [];
```

Declaraciones reales de estado de `Temporizador` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal bool Cerrado;
```

```csharp
internal DateTimeOffset Vence;
```

```csharp
internal TimerCallback Callback = callback;
```

```csharp
internal object? State = state;
```

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `SesionClienteTests.ConsultaYPresenciaContinuanEnMenuYResultadoSinCambiarRevision`

Escenario inicial espera inicio; abre menú con esc y espera salida. Avanza reloj5s y exige consultas nuevas/presencia. Aplica pausa sin variar revisión y avanza1s para mostrarla. Cierra menú, agrega resultado y quita pausa, avanza1s y espera Enter para continuar. Otros5s deben aumentar consultas y presencias durante lectura; Elecciones queda0. Muestra tareas de fondo independientes de navegación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.RespuestaPerdidaConsultaReciboYReenviaSoloElMismoCuerpo`

Theory aplicado true/false. Prepara turno y Perdida despues o antes; confirma carta y atributo, espera primer envío. Avanza1s y espera consulta de recibo. Si no aplicado, avanza otro1s, exige segundo envío y cuerpos[0]==cuerpos[1]. Siempre espera recepción confirmada y total1/2 según parámetro. Es un doble de commit, no prueba transacción SQLite (integración separada).

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.EnterDeResultadoNoEligeTurnoSiguienteYFinDetienePresencia`

Tras primera consulta prepara turno y resultado nuevo; al avanzar1s muestra resultado. Enter debe llevar a Tu turno con Elecciones0, sin confirmar carta automáticamente. Cambia vista a finalizada/cancelacion, avanza1s y espera mensaje del operador. Guarda contadores; tras20s simulados y100ms reales deben mantenerse. Enter final termina Tarea.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.AbandonoExigeSiYSalidaLocalNoLoEnvia`

Espera consulta, abre confirmación de abandono y exige Abandonos0. Enter sobre No por defecto mantiene0 tras80ms. Reabre, mueve s a Sí y Enter debe llevar Abandonos1. No tiene rama que seleccione explícitamente Salir sin abandonar pese al nombre; su garantía directa es confirmación No/Sí.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task AbandonoExigeSiYSalidaLocalNoLoEnvia()
    {
        await using var f = new Escenario();
        await f.Esperar(() => f.Consultas > 0);
        await f.AbrirAbandono();
        await f.Esperar(() => f.Texto.Contains("¿Abandonar partida?"));
        Assert.Equal(0, f.Abandonos);
        f.Tecla(""); // No por defecto
        await Task.Delay(80);
        Assert.Equal(0, f.Abandonos);
        await f.AbrirAbandono();
        await f.TeclaConPantalla("s");
        f.Tecla("");
        await f.Esperar(() => f.Abandonos == 1);
    }
```

#### `SesionClienteTests.UnaConsultaLentaNoSolapaConsultasNiRetrasaPresencia`

Intercepta presentación y devuelve TaskCompletionSource pendiente, contando iniciadas con Interlocked. Avanza1s hasta una consulta y10s hasta presencia; iniciadas debe seguir1, sin solapamiento de consultas. Quita interceptor, libera respuesta JSON actual y espera Consultas>1. No bloquea renovación mientras hay lectura HTTP pendiente.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.SesionVencidaReincorporaConMismoUuidYNoReproduceHistoria`

Interceptor hace fallar primera presentación con409 sesionVencida; en PUT sesión lee JSON, cuenta incorporaciones y exige idSesion igual a X-Sesion. Avanza reloj y espera reincorporación/consulta posterior; UUID capturado no null y no aparece Enter para continuar. Demuestra consistencia cuerpo/cabecera, no compara aquí una variable guardada de la incorporación inicial.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.ProtocoloIncompatibleExigeReintentoExplicitoYNoAplicaVistaParcial`

Interceptor responde presentación con objeto fase inventada. Al avanzar1s debe mostrar Respuesta incompatible. Tras20s y50ms exige solo un fallo y cero elecciones: no reintenta automáticamente. Quita interceptor, Enter habilita nueva consulta. No convierte la vista parcial en estado válido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.PausaDuranteSeleccionCancelaAtributoYRechazoPermiteOperacionNueva`

Prepara turno, confirma carta y espera atributo. Guarda solicitud; cambia vista Pausada true/Solicitud null, avanza1s y espera pausa. Enter y más tiempo no envían (Elecciones0). Restaura misma solicitud, quita pausa y avanza: Tu turno debe aparecer después del aviso de pausa. Dos Enter entonces producen una elección. No simula un rechazo HTTP concreto pese al nombre.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.AbandonoInciertoConsultaVistaAntesDeRepetirYNoInventaCierre`

Interceptor lanza HttpRequestException solo para primer abandono. Abre confirmación, selecciona Sí y confirma; intentos1 pero Abandonos0 porque el doble no aplicó. Avanza1s hasta que se aplique abandono1 y muestre fin confirmado. Garantiza no mostrar cierre antes de recuperar; este caso no inspecciona explícitamente secuencia de rutas (la integración real sí).

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
        await f.AbrirAbandono();
        await f.TeclaConPantalla("s");
        f.Tecla("");
        await f.Esperar(() => intentos == 1);
        Assert.Equal(0, f.Abandonos);
        f.Reloj.Avanzar(TimeSpan.FromSeconds(1));
        await f.Esperar(() => f.Abandonos == 1);
        await f.Esperar(() => f.Texto.Contains("Partida finalizada por abandono"));
    }
```

#### `SesionClienteTests.EleccionPendienteEnPausaConservaOperacionYNoEnviaAbandonoConcurrente`

Intercepta POST elección guardando JSON y bloqueando en TCS. Confirma jugada, luego solicita abandono Sí mientras sigue en vuelo. Tras5s presencia sigue y abandono0. Marca pausa sin solicitud, completa envío con excepción, quita interceptor y avanza1s: debe consultar recibo, mostrar pausa y mantener Elecciones/Abandonos0 (contador solo mide doble aplicado). PrepararTurno crea solicitud nueva: pendiente obsoleta no se reenvía, abandono ya solicitado puede ejecutarse y Elecciones sigue0.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
        await f.AbrirAbandono();
        await f.TeclaConPantalla("s");
        f.Tecla("");
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
```

#### `SesionClienteTests.ConflictoNoReenviaOperacionYVistaIdenticaNoReiniciaFoco`

Ofrece cartas A/B y opciones1/2; mueve s a B y espera foco. Guarda longitud y consultas, avanza1s y exige consulta nueva con salida sin cambios (no redibuja ni pierde foco). Intercepta elecciones devolviendo409 operacionIncompatible; confirmar carta/atributo muestra rechazo. Tras5s y nuevas consultas, intentos sigue1: no reenvío automático.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.VictoriaEsperaLecturaDelUltimoCombateYNoReproduceResultadosIniciales`

Tras primera consulta agrega resultado ganador de ronda J2 y final victoria/J2. Avanza1s y espera Enter para continuar; antes no debe mostrar gana la partida. Enter entonces permite Ada/J2 gana la partida y Elecciones0. El método comprueba orden de lectura del último resultado; omisión de historia inicial se acredita principalmente en EstadoClienteTests.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.RespuestaTardiaDePresenciaAnteriorNoInvalidaSesionRecuperada`

Interceptor deja presencia pendiente y hace vencer próxima presentación una sola vez; cuenta reincorporaciones. Avanza5s hasta presencia, luego1s para vencimiento y reincorporación; espera Estado actualizado posterior a Sesión vencida. Quita interceptor y resuelve presencia vieja con400 entradaInvalida; nueva consulta debe seguir y no aparecer Enter para corregir los datos. Prueba descarte de respuesta de generación anterior.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `SesionClienteTests.PendienteSinReciboEsperaPausaYReenviaCuerpoOriginalAlReanudar`

En primer POST guarda JSON, marca vista pausada/sin solicitud y lanza excepción sin aplicar. Tras avance espera recibo consultado y pausa. Otros5s mantienen presencia pero Elecciones0. Restaura misma solicitud y quita pausa;1s después el doble aplica una elección y su único cuerpo coincide exactamente con original. No crea un nuevo UUID de operación.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `Lineas.Agregar`

internal void recibe string nullable y TryWrite al canal; null simula fin de entrada, cadena vacía Enter. No espera consumidor ni completa writer.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Agregar(string? linea) => canal.Writer.TryWrite(linea);
```

#### `Lineas.ReadLineAsync`

Override público retorna ReadAsync con el token recibido; espera una línea disponible y propaga cancelación. Un null agregado retorna null sin cerrar el canal.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken) => canal.Reader.ReadAsync(cancellationToken);
```

#### `Escenario.Escenario`

Constructor internal: crea configuración localhost ficticia, UUID partida/sesión y Vista de EstadoClienteTests. Instala Handler async: primero da oportunidad a Interceptar; respuesta no null sustituye, excepción se propaga, null continúa. Ruta /sesion retorna J1/mismo UUID/vencimientoUtc+15s; /presencia incrementa y204; /presentacion incrementa y JSON Vista. POST /elecciones captura cuerpo, incrementa contador y deserializa: Perdida antes se limpia y lanza sin recibo; si no crea recibo revisión+1 y cambia Vista revisión/solicitud null; Perdida despues se limpia y lanza tras aplicar; normal retorna recibo. GET /elecciones/<id> incrementa Recibos y da recibo existente o404. /abandono incrementa Abandonos, revisión+1, finalizada/motivoabandono/solicitud null y204. Cualquier otra ruta lanza InvalidOperationException. Finalmente inicia SesionPartidaCliente con Entrada, writer, reloj y token cierre. Es simulación controlada, no repositorio real.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `Escenario.PrepararTurno`

internal void: copia Vista a esperandoElector, sin pausa, ronda1/combate1, mano de CartaA ID1 con seis puntajes100 y solicitud nueva UUID tipo cartaYAtributo permitiendo1/Fuerza. Avanza reloj1s para provocar consulta. Puntajes son fixture de presentación, no cartas de dominio válidas (no pasan reglas250..750).

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `Escenario.Tecla`

internal void delega Entrada.Agregar(linea), sin esperar a que dibuje ni validar tecla.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Tecla(string linea) => Entrada.Agregar(linea);
```

#### `Escenario.TeclaConPantalla`

internal async Task: guarda longitud Texto, envía tecla y llama Esperar hasta longitud mayor. Serializa navegación respecto a un dibujo; no comprueba por sí sola texto de pantalla.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal async Task TeclaConPantalla(string linea)
        {
            int longitud = Texto.Length;
            Tecla(linea);
            await Esperar(() => Texto.Length > longitud);
        }
```

#### `Escenario.AbrirAbandono`

internal async Task: cuatro pasos con TeclaConPantalla: esc, s, s, Enter. Abre menú, recorre dos opciones y entra en confirmación todavía sobre No. No manda Sí ni abandono.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal async Task AbrirAbandono()
        {
            await TeclaConPantalla("esc");
            await TeclaConPantalla("s");
            await TeclaConPantalla("s");
            await TeclaConPantalla("");
        }
```

#### `Escenario.Esperar`

internal async Task con predicado: máximo150 vueltas. Si Tarea.IsFaulted espera Tarea para propagar excepción; si predicado true retorna. Si no, avanza reloj25ms y demora10ms reales. Al agotar exige predicado con Texto como diagnóstico. No trata toda Tarea completada como fallo: solo faulted.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
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
```

#### `Escenario.DisposeAsync`

public async ValueTask: cancela CTS, avanza reloj1s para despertar esperas, espera Tarea máximo3s, libera http y CTS. No libera explícitamente StringWriter ni canal; ambos son recursos de memoria y no hay proceso externo.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public async ValueTask DisposeAsync()
        {
            cierre.Cancel();
            Reloj.Avanzar(TimeSpan.FromSeconds(1));
            await Tarea.WaitAsync(TimeSpan.FromSeconds(3));
            http.Dispose(); cierre.Dispose();
        }
```

#### `Escenario.Json`

internal static: recibe objeto y status opcional200; serializa con JsonSerializerDefaults.Web y arma HttpResponseMessage con UTF8/application/json. No valida esquema ni hace I/O. Caller/transporte posee la respuesta.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal static HttpResponseMessage Json(object cuerpo, HttpStatusCode codigo = HttpStatusCode.OK) => new(codigo)
        { Content = new StringContent(JsonSerializer.Serialize(cuerpo, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json") };
```

#### `RelojCliente.GetUtcNow`

Override público: lock temporizadores y retorna ahora, sin avanzar. Sincroniza consulta con Avanzar y creación de timers.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override DateTimeOffset GetUtcNow() { lock (temporizadores) return ahora; }
```

#### `RelojCliente.CreateTimer`

Override público recibe callback,state,dueTime,period; con lock crea Temporizador capturando reloj/callback/state, agrega a lista, llama Change y retorna ITimer. Period se recibe pero el timer no lo usa para reprogramar automáticamente.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            lock (temporizadores)
            {
                var timer = new Temporizador(this, callback, state);
                temporizadores.Add(timer); timer.Change(dueTime, period); return timer;
            }
        }
```

#### `RelojCliente.Avanzar`

internal void: con lock suma intervalo a ahora; recorre una copia de temporizadores para tolerar cambios de lista por callbacks. Si no cerrado y Vence<=ahora, primero pone Vence en MaxValue y luego invoca callback(State). Cada vencimiento dispara una vez salvo Change desde callback. No valida intervalos negativos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void Avanzar(TimeSpan intervalo)
        {
            lock (temporizadores)
            {
                ahora += intervalo;
                foreach (var t in temporizadores.ToArray())
                    if (!t.Cerrado && t.Vence <= ahora) { t.Vence = DateTimeOffset.MaxValue; t.Callback(t.State); }
            }
        }
```

#### `Temporizador.Change`

public bool: asigna Vence=MaxValue si dueTime InfiniteTimeSpan, o reloj.ahora+dueTime en otro caso; retorna !Cerrado. Ignora period. Incluso cerrado asigna Vence pero Avanzar lo omite. No es una implementación completa de temporizador periódico de sistema.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                Vence = dueTime == Timeout.InfiniteTimeSpan ? DateTimeOffset.MaxValue : reloj.ahora + dueTime;
                return !Cerrado;
            }
```

#### `Temporizador.Dispose`

public void: marca Cerrado=true. No quita de lista ni libera un recurso OS; Avanzar excluye los cerrados.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public void Dispose() => Cerrado = true;
```

#### `Temporizador.DisposeAsync`

public ValueTask: llama Dispose y retorna default ValueTask completado; no trabajo asíncrono ni espera adicional.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
public ValueTask DisposeAsync() { Dispose(); return default; }
```

### Recorridos y ejemplos

Normal: Respuesta perdida antes de aplicar produce consulta de recibo y segundo envío del cuerpo original; después de aplicar solo se consulta recibo.

Alternativo o de error: Pausa, protocolo incompatible o conflicto no deben enviar operación obsoleta. Un abandono espera la elección incierta en vuelo; respuesta tardía de presencia vieja no invalida sesión recuperada.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>

<details>
<summary>test/proyecto-integrador-final-tests/TransporteClienteTests.cs</summary>

[Página individual](archivos/test/proyecto-integrador-final-tests/TransporteClienteTests.md)

## test/proyecto-integrador-final-tests/TransporteClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/TransporteClienteTests.cs](../test/proyecto-integrador-final-tests/TransporteClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](archivos/README.md#versión-documentada).

### Propósito y responsabilidad

Pruebas del transporte ClientePartidaHttp con HttpMessageHandler controlado: preservación de prefijo/identidad y clasificación de respuestas. No conectan a localhost real.

### Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](archivos/src/cliente/ClientePartidaHttp.md), [FalloCliente](archivos/src/cliente/ClientePartidaHttp.md).

Guías: [plan de pruebas y ejecución de esta referencia](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](instalacion-y-uso.md), [protocolo HTTP](protocolo-de-partida.md).

### Estructura

Declaración: `public sealed class TransporteClienteTests`.

Declaración: `internal sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

### Estado y datos

Handler internal sealed con constructor primario Func<HttpRequestMessage,Task<HttpResponseMessage>>; hereda HttpMessageHandler. Guarda responder como captura del constructor; no tiene contadores propios. El caso de identidad cuenta llamadas en una variable capturada.

### Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

#### `TransporteClienteTests.ConservaPrefijoSesionYCredencialSinPonerlaEnUrl`

Genera partida y sesión, arma handler async que incrementa llamadas y exige ruta /juego/api/v1/partidas/<id>/participantes/me/sesion, Authorization.Parameter secreto y ausencia de secreto en URL. Lee JSON y comprueba UUID y Ana··Pérez. Responde200 con J2, sesión y vencimiento fijo. Dos Incorporar deben devolver J2 y mismo UUID; exige llamadas2. No comprueba vencimiento relativo al reloj ni red real.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public async Task ConservaPrefijoSesionYCredencialSinPonerlaEnUrl()
    {
        Guid partida = Guid.NewGuid(), sesion = Guid.NewGuid();
        int llamadas = 0;
        var handler = new Handler(async solicitud =>
        {
            llamadas++;
            Assert.Equal($"/juego/api/v1/partidas/{partida}/participantes/me/sesion", solicitud.RequestUri!.AbsolutePath);
            Assert.Equal("secreto", solicitud.Headers.Authorization!.Parameter);
            Assert.DoesNotContain("secreto", solicitud.RequestUri.ToString());
            string json = await solicitud.Content!.ReadAsStringAsync();
            Assert.Contains(sesion.ToString(), json);
            Assert.Contains("Ana  Pérez", System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("nombre").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"{{\"rol\":\"J2\",\"idSesion\":\"{sesion}\",\"venceUtc\":\"2026-11-21T12:00:00Z\"}}", Encoding.UTF8, "application/json") };
        });
        using var cliente = new ClientePartidaHttp(new(new Uri("http://localhost/juego/"), partida, "secreto", "Ana  Pérez"), sesion, handler);
        Assert.Equal("J2", (await cliente.Incorporar(default)).Rol);
        Assert.Equal(sesion, (await cliente.Incorporar(default)).IdSesion);
        Assert.Equal(2, llamadas);
    }
```

#### `TransporteClienteTests.ClasificaErroresSinExponerCuerpos`

Theory:503/no json→servicioNoDisponible/transitorio true;302/vacío→protocoloIncompatible/false;200/{→protocoloIncompatible/false;409/codigo sesionOcupada→sesionOcupada/false. Incorporar debe lanzar FalloCliente con código y bandera exactos y mensaje sin secreto. El título alude a cuerpos, pero la aserción explícita solo excluye la credencial de Message.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, "no json", "servicioNoDisponible", true)]
    [InlineData(HttpStatusCode.Redirect, "", "protocoloIncompatible", false)]
    [InlineData(HttpStatusCode.OK, "{", "protocoloIncompatible", false)]
    [InlineData(HttpStatusCode.Conflict, "{\"codigo\":\"sesionOcupada\"}", "sesionOcupada", false)]
    public async Task ClasificaErroresSinExponerCuerpos(HttpStatusCode estado, string cuerpo, string codigo, bool transitorio)
    {
        using var cliente = new ClientePartidaHttp(new(new Uri("http://localhost/"), Guid.NewGuid(), "secreto", "Ana"), Guid.NewGuid(),
            new Handler(_ => Task.FromResult(new HttpResponseMessage(estado) { Content = new StringContent(cuerpo, Encoding.UTF8, "application/json") })));
        var error = await Assert.ThrowsAsync<FalloCliente>(() => cliente.Incorporar(default));
        Assert.Equal(codigo, error.Codigo);
        Assert.Equal(transitorio, error.Transitorio);
        Assert.DoesNotContain("secreto", error.Message);
    }
```

#### `Handler.SendAsync`

protected override Task: delega directamente responder(request). El token de cancelación no se remite a la función; este doble no simula cancelación de socket ni redirects automáticos. Las excepciones de la función se propagan al transporte.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => responder(request);
```

### Recorridos y ejemplos

Normal: Dos incorporaciones con mismo UUID envían prefijo /juego/, Bearer secreto y nombre con doble espacio; retornan rolJ2 y misma sesión.

Alternativo o de error: 503 de texto es transitorio; redirect, JSON roto y conflicto de sesión se clasifican sin exponer la credencial en mensaje.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

### Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

### Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

</details>
