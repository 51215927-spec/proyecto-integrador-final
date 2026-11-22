# Program.cs

Fuente: [src/subir-personajes/Program.cs](../../../../src/subir-personajes/Program.cs). Proyecto: `src/subir-personajes/subir-personajes.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.CargaPersonajes`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Lee un catálogo JSON local y propone reemplazar el catálogo completo del servidor. Consulta cantidad, pide confirmación explícita y envía un único PUT sin reintento automático si el resultado queda incierto. Conserva el formato exacto aceptado, sin asignar IDs ni completar datos faltantes.

## Dependencias

[EntradaConsola](../comun/EntradaConsola.md) solicita URL y secreto; HttpClient, HttpRequestMessage y HttpClientHandler hacen HTTP sin redirección automática; System.Text.Json inspecciona JSON; JsonContent y ReadFromJsonAsync serializan/deserializan. [ServidorHttp](../servidor/Api/ServidorHttp.md) publica GET resumen/PUT; [LotePersonajes](../servidor/Api/LotePersonajes.md) y [AltaPersonaje](../servidor/Api/PersonajeCatalogo.md) validan servidor. [Guía de la herramienta](../../../../src/subir-personajes/README.md) explica archivo de ejemplo y uso.

## Estructura y estado

Namespace `Proyecto_Integrador_Final.CargaPersonajes`. Tipos `internal static class Program`, `internal static class ConsolaAlta`; dentro de ConsolaAlta, `private sealed record Resumen(int? Cantidad)` genera propiedad pública init, constructor e igualdad. No hay campos compartidos, partial, interfaces explícitas ni eventos. Cliente, documento JSON, mensajes y respuestas tienen vida limitada mediante using. `enviado` es local por envío; no es un recibo persistido.

## Miembros y ejecución

### Arranque y lectura

`private static Task<int> Program.Main(string[] args)` delega a ConsolaAlta.Iniciar(Console.In,Console.Out,args); no interpreta opciones adicionales.

`internal static async Task<int> Iniciar(TextReader entrada,TextWriter salida,string[]? args = null)` usa solo FirstOrDefault como ruta. Lee/valida archivo **antes** de pedir URL (defecto http://localhost:5000) y credencial no vacía. Crea HttpClient con AllowAutoRedirect false y BaseAddress servidor; llama Enviar y retorna su código. Captura EsErrorEntrada, imprime «No se pudo cargar el archivo: {Message}» y retorna 1; no envía PUT si falla lectura. Un EOF de EntradaConsola es IOException y entra en esa clasificación.

`internal static async Task<int> Ejecutar(HttpClient cliente,string credencial,TextReader entrada,TextWriter salida,string? ruta = null)` es entrada inyectable para pruebas: lee JSON y llama Enviar con cliente existente, sin pedir URL/secreto ni disponer ese cliente prestado. Captura los mismos errores de entrada y retorna 1.

`private static bool EsErrorEntrada(Exception error)` reconoce IOException, UnauthorizedAccessException, ArgumentException, JsonException o FormatException; no captura cualquier excepción indiscriminadamente.

`private static async Task<JsonElement> Leer(string? ruta,TextReader entrada,TextWriter salida)` si ruta null imprime pregunta y ReadLine. Ruta null/blanca falla FormatException. Lee archivo completo asíncronamente, JsonDocument.Parse, ValidarFormato y retorna RootElement.Clone. El clon mantiene datos después de desechar documento. No recorta ruta ni reintenta un archivo inválido.

### `private static void ValidarFormato(JsonElement raiz)`

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

### `private static async Task<int> Enviar(HttpClient cliente,string credencial,TextReader entrada,TextWriter salida,JsonElement datos)`

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

### `private static async Task<int> Rechazo(HttpResponseMessage respuesta,TextWriter salida)`

Usa mensaje de credencial inválida para 401 y rechazo genérico para otros estados. Intenta parsear cuerpo como JsonDocument y, si existe propiedad mensaje, usa GetString o conserva causa si null. JsonException se ignora para conservar causa; un valor mensaje de tipo no-string puede lanzar InvalidOperationException no capturada por este catch. Imprime `HTTP {número}: {causa}` y retorna 1. Nunca imprime la cabecera Authorization. Resumen solo contiene int? Cantidad: nulabilidad distingue falta de confirmación de cero.

### Fragmentos de consulta, confirmación y resultado incierto

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

### Fragmentos del arranque y rechazo HTTP

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

## Recorridos y ejemplos

Normal: archivo con seis personajes válidos, URL localhost y secreto ficticio; GET informa 8 existentes/6 del JSON; respuesta `si` produce PUT; confirmación `{cantidad:6}` informa seis y retorna 0. Cancelación: GET exitoso seguido de `no` o EOF retorna 0 sin enviar PUT.

Error: archivo `[7]` falla antes de pedir servidor. Tras PUT aceptado, una conexión perdida impide comprobar cantidad: retorna 1 con advertencia de posible reemplazo y no reintenta. GET 401 imprime causa y no pide confirmación. Lectura de estos recorridos no equivale a ejecutar una carga contra un servicio real.

## Errores, límites y efectos observables

Puede borrar/reemplazar catálogo solo después de confirmación, mediante servidor que valida transacción completa. No agrega individualmente ni asigna IDs en cliente. La verificación de formato local no sustituye reglas del dominio del servidor. Solo el primer argumento se usa como ruta, sin advertencia por argumentos extra. URL y secreto se solicitan cada vez; no se guardan en configuración local. Redirecciones no se siguen automáticamente.

## Relaciones y verificación

[ConsolaAltaTests](../../test/proyecto-integrador-final-tests/ConsolaAltaTests.md) define orden de preguntas, rechazo local, resumen, sí/no, PUT único y resultado incierto. [EntradaProcesosContratoTests](../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) define proceso real y EOF. [CargaJsonTests](../../test/proyecto-integrador-final-tests/CargaJsonTests.md) valida atomicidad en servidor. Véase [verificación spec 004](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) para ejecución real y limitaciones.
