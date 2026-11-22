# ConsolaAltaTests.cs

Fuente: [test/proyecto-integrador-final-tests/ConsolaAltaTests.cs](../../../../test/proyecto-integrador-final-tests/ConsolaAltaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Prueba la carga JSON desde rutas con espacios, confirmación y tratamiento de errores mediante handlers HTTP controlados. El flujo vigente reemplaza catálogo, no pregunta personaje campo por campo.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CargaJsonTests](CargaJsonTests.md), [ConsolaAlta](../../src/subir-personajes/Program.md), [Personaje](../../src/servidor/Personaje.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class ConsolaAltaTests`.

Declaración: `private sealed class Fallo(bool perdida) : HttpMessageHandler`.

Declaración: `private sealed class Captura : HttpMessageHandler`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Clases anidadas privadas Fallo(bool perdida) y Captura heredan HttpMessageHandler. Solicitudes y Puts son enteros internal inicialmente0 que cuentan llamadas; cada test crea handler nuevo.

Declaraciones reales de estado de `Fallo` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal int Solicitudes;
```

Declaraciones reales de estado de `Captura` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal int Puts;
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ConsolaAltaTests.ListaPorRutaConfirmaAntesDeUnicoPut`

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

### `ConsolaAltaTests.RutaInexistenteFinalizaSinConexion`

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

### `ConsolaAltaTests.ErrorHttpNoReintentaNiAfirmaRollback`

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

### `ConsolaAltaTests.ArchivoInvalidoNoEnviaHttp`

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

### `ConsolaAltaTests.RutaRelativaConEspaciosYFinDeEntradaEnConfirmacionCancela`

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

### `Fallo.SendAsync`

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

### `Captura.SendAsync`

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

## Recorridos y ejemplos

Normal: Archivo de Ada con respuesta sí o SI genera un único PUT después de mostrar resumen actual7/JSON1.

Alternativo o de error: No o EOF cancela sin PUT; archivo inválido no envía, y error de PUT no se reintenta ni se presenta como reemplazo exitoso.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
