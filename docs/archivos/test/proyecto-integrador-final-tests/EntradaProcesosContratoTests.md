# EntradaProcesosContratoTests.cs

Fuente: [test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs](../../../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Contratos de los tres ejecutables reales mediante stdin, con directorios temporales y host HTTP local para carga JSON. Cada ContratoFact requiere su variable DLL; el nombre histórico de algunos métodos no describe ya todos los pasos vigentes.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CargaJsonTests](CargaJsonTests.md), [Personaje](../../src/servidor/Personaje.md), [ServidorHttp](../../src/servidor/Api/ServidorHttp.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Las pruebas con `ContratoFact` se omiten si la variable DLL indicada falta o está en blanco; una ruta no vacía no garantiza que exista ni que corresponda a esta versión. Véase [ContratoFactAttribute](ContratoFactAttribute.md). Los Facts/Theories ordinarios del mismo proyecto no dependen de esa condición.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class EntradaProcesosContratoTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos de instancia. Ejecutar devuelve tupla (Codigo,Salida,Errores,CreoBase); elimina variables de configuración heredadas en el hijo y crea su cwd aislado. Cantidad es función local de Carga_RecorridoCompleto y captura HttpClient.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `EntradaProcesosContratoTests.Servidor_FinDeEntradaDuranteConfiguracionNoIniciaHostNiCreaBase`

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

### `EntradaProcesosContratoTests.Servidor_SinCredencialRechazaAltasYFinDeEntradaDetieneElHost`

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

### `EntradaProcesosContratoTests.Servidor_BaseInvalidaPermiteCorregirLaConfiguracionSinExponerCredencial`

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

### `EntradaProcesosContratoTests.Cliente_FinDeEntradaEnMenuSaleSinSimularUnaVictoria`

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

### `EntradaProcesosContratoTests.Carga_SinArgumentosNiCredencialDeEntornoEnviaElContratoExistente`

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

### `EntradaProcesosContratoTests.Carga_FinDeEntradaDuranteConexionOPersonajeSaleSinAlta`

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

### `EntradaProcesosContratoTests.Carga_RecorridoCompletoDesdeProcesoConBaseTemporal`

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

### `EntradaProcesosContratoTests.Cantidad`

Función local async que captura http. GET /api/v1/personajes/resumen y exige substring JSON cantidad:<esperada>. Libera respuesta con using; no comprueba por separado StatusCode ni parsea JSON, por lo que su garantía es el texto observado.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
async Task Cantidad(int esperada)
            {
                using var respuesta = await http.GetAsync("/api/v1/personajes/resumen");
                Assert.Contains($"\"cantidad\":{esperada}", await respuesta.Content.ReadAsStringAsync());
            }
```

### `EntradaProcesosContratoTests.Ejecutar`

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

## Recorridos y ejemplos

Normal: Carga ejecutable reemplaza catálogo vacío por seis ejemplos, cancela un reemplazo, reemplaza por uno dos veces y vuelve a seis; cada resumen coincide.

Alternativo o de error: EOF durante configuración no arranca host o no anuncia alta. Base inválida solicita configuración nueva sin imprimir secreto.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
