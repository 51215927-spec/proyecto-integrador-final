# ClienteRestIntegracionTests.cs

Fuente: [test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs](../../../../test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Integración de ClientePartidaHttp contra Kestrel loopback y SQLite temporal, sin ejecutar consola ni depender de variables DLL.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [ClientePartidaHttp](../../src/cliente/ClientePartidaHttp.md), [ConfiguracionCliente](../../src/cliente/ConfiguracionCliente.md), [EleccionCliente](../../src/cliente/ContratosPartida.md), [EstadoCliente](../../src/cliente/EstadoCliente.md), [FalloCliente](../../src/cliente/ClientePartidaHttp.md), [RepositorioJuego](../../src/servidor/Api/RepositorioJuego.md), [ServidorHttp](../../src/servidor/Api/ServidorHttp.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ClienteRestIntegracionTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos. Archivos SQLite con GUID por caso; finally ClearAllPools y borra archivo, -wal y -shm; await using libera hosts y using transportes.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ClienteRestIntegracionTests.ReinicioDelServidorConservaPartidaYRequiereReincorporarLaMismaSesion`

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

### `ClienteRestIntegracionTests.DosClientesJueganContraServidorLocalRecuperanReciboYSesion`

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

## Recorridos y ejemplos

Normal: J1 y J2 incorporan, realizan un combate, recuperan recibo por GET y reenvío, reincorporan misma sesión y abandonan sin ganador.

Alternativo o de error: Reiniciar host invalida sesión anterior aunque conserva partida/revisión/nombre; Presentacion lanza sesionVencida hasta Incorporar.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
