# ClienteProcesoRestTests.cs

Fuente: [test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs](../../../../test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Arranca dos procesos cliente sucesivos contra un servidor local real para comprobar reapertura, plaza ocupada y ausencia de archivos de sesión. Requiere GUERRA_CLIENTE_DLL.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [RepositorioJuego](../../src/servidor/Api/RepositorioJuego.md), [ServidorHttp](../../src/servidor/Api/ServidorHttp.md).

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ClienteProcesoRestTests`.

Declaración: `private sealed class Proceso : IAsyncDisposable`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

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

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `ClienteProcesoRestTests.ProcesoSeIncorporaReabreSinArchivosYEsperaPlazaHastaLiberacion`

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

### `Proceso.Proceso`

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

### `Proceso.Leer`

private async Task: mientras ReadLineAsync con token15s no devuelva null, agrega cada línea a ConcurrentQueue. EOF termina normalmente; timeout o error propaga a la tarea. Mezcla stdout/stderr en la misma cola sin prometer orden entre flujos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
private async Task Leer(StreamReader lector)
        {
            while (await lector.ReadLineAsync(plazo.Token) is { } linea) lineas.Enqueue(linea);
        }
```

### `Proceso.Enviar`

internal Task: escribe texto tal cual en stdin de proceso y devuelve tarea. No agrega salto extra ni espera respuesta; el caller envía sus propios saltos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal Task Enviar(string texto) => proceso.StandardInput.WriteAsync(texto);
```

### `Proceso.CerrarEntrada`

internal void: cierra stdin para producir EOF remoto; no abandona partida por HTTP ni mata proceso.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
internal void CerrarEntrada() => proceso.StandardInput.Close();
```

### `Proceso.Esperar`

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

### `Proceso.Terminar`

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

### `Proceso.DisposeAsync`

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

## Recorridos y ejemplos

Normal: Primer proceso entra como Ana, espera inicio y sale por EOF; el segundo recupera la plaza después de CerrarSesion y sale desde menú.

Alternativo o de error: Antes de liberar la sesión, el segundo muestra Sesión ocupada; la salida local no debe anunciar Final confirmado ni terminar la partida.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
