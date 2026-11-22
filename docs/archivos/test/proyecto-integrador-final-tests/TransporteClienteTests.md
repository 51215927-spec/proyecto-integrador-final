# TransporteClienteTests.cs

Fuente: [test/proyecto-integrador-final-tests/TransporteClienteTests.cs](../../../../test/proyecto-integrador-final-tests/TransporteClienteTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Pruebas del transporte ClientePartidaHttp con HttpMessageHandler controlado: preservación de prefijo/identidad y clasificación de respuestas. No conectan a localhost real.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](../../src/cliente/ClientePartidaHttp.md), [FalloCliente](../../src/cliente/ClientePartidaHttp.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class TransporteClienteTests`.

Declaración: `internal sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Handler internal sealed con constructor primario Func<HttpRequestMessage,Task<HttpResponseMessage>>; hereda HttpMessageHandler. Guarda responder como captura del constructor; no tiene contadores propios. El caso de identidad cuenta llamadas en una variable capturada.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `TransporteClienteTests.ConservaPrefijoSesionYCredencialSinPonerlaEnUrl`

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

### `TransporteClienteTests.ClasificaErroresSinExponerCuerpos`

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

### `Handler.SendAsync`

protected override Task: delega directamente responder(request). El token de cancelación no se remite a la función; este doble no simula cancelación de socket ni redirects automáticos. Las excepciones de la función se propagan al transporte.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => responder(request);
```

## Recorridos y ejemplos

Normal: Dos incorporaciones con mismo UUID envían prefijo /juego/, Bearer secreto y nombre con doble espacio; retornan rolJ2 y misma sesión.

Alternativo o de error: 503 de texto es transitorio; redirect, JSON roto y conflicto de sesión se clasifican sin exponer la credencial en mensaje.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
