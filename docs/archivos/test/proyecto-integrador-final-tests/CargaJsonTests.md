# CargaJsonTests.cs

Fuente: [test/proyecto-integrador-final-tests/CargaJsonTests.cs](../../../../test/proyecto-integrador-final-tests/CargaJsonTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Ensaya esquema de lote JSON, reglas de dominio, autenticación HTTP y reemplazo atómico del catálogo en SQLite. Incluye un trigger que induce fallo de inserción.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [AltaPersonaje](../../src/servidor/Api/PersonajeCatalogo.md), [ErrorApi](../../src/servidor/Api/ServidorHttp.md), [LotePersonajes](../../src/servidor/Api/LotePersonajes.md), [Personaje](../../src/servidor/Personaje.md), [PersonajeCatalogo](../../src/servidor/Api/PersonajeCatalogo.md), [CoordinadorPartidas](../../src/servidor/Api/CoordinadorPartidas.md), [CatalogoPersonajes](../../src/servidor/Api/CatalogoPersonajes.md), [AdministradorSesiones](../../src/servidor/Api/AdministradorSesiones.md), [ServidorHttp](../../src/servidor/Api/ServidorHttp.md).

`HttpClient`/`HttpMessageHandler` representan solicitudes y respuestas; `JsonDocument`, `JsonSerializer` o `JsonContent` leen o escriben cuerpos donde aparecen. Los handlers anidados descritos abajo son dobles salvo los intermediarios que delegan a un `HttpClientHandler` real.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class CargaJsonTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Constante internal Uno es JSON de una carta válida (descripción vacía, ASCII *). No hay fixture compartido de base: cada prueba posee memoria o archivo temporal y conexiones con using.

Declaraciones reales de estado de `CargaJsonTests` (la mutación y el ciclo de vida se explican aquí y en sus miembros):

```csharp
internal const string Uno = """[{"nombre":"Ada","descripcion":"","ascii":"*","fuerza":250,"velocidad":350,"resistencia":450,"inteligencia":550,"energia":650,"habilidad":750}]""";
```

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `CargaJsonTests.RechazaFormatoIncompleto`

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

### `CargaJsonTests.RechazaDuplicadosNormalizados`

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

### `CargaJsonTests.ReemplazoRepiteSinAcumularYValidaAntesDeBorrar`

Repositorio memoria; da alta a Anterior, reemplaza por Uno y repite reemplazo. CantidadCatalogo debe ser1. Intenta reemplazar por la misma carta con Fuerza0; espera ErrorApi y cantidad todavía1. La aserción de cantidad no compara todos los campos: persistencia exacta se prueba aparte.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
    public void ReemplazoRepiteSinAcumularYValidaAntesDeBorrar()
    {
        using var almacen = new AlmacenPartidas("Data Source=:memory:");
        var juego = new CatalogoPersonajes(almacen.Infraestructura);
        var lote = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement);
        new CatalogoPersonajes(almacen.Infraestructura).DarDeAlta(lote[0] with { Nombre = "Anterior" });
        new CatalogoPersonajes(almacen.Infraestructura).ReemplazarCatalogo(lote);
        new CatalogoPersonajes(almacen.Infraestructura).ReemplazarCatalogo(lote);
        Assert.Equal(1, new CatalogoPersonajes(almacen.Infraestructura).CantidadCatalogo());
        Assert.Throws<ErrorApi>(() => new CatalogoPersonajes(almacen.Infraestructura).ReemplazarCatalogo([lote[0] with { Fuerza = 0 }]));
        Assert.Equal(1, new CatalogoPersonajes(almacen.Infraestructura).CantidadCatalogo());
    }
```

### `CargaJsonTests.ReemplazoPersisteDatosExactosYRevierteFalloDeInsercion`

Usa archivo temporal y Pooling=False. Da de alta Viejo e Inactivo, desactiva ID2 y exige cuenta2 (incluye inactivos); reemplaza por Uno y cierra AlmacenPartidas. Consulta SQL y exige única fila [1,Ada,descripción vacía,*,250,350,450,550,650,750,activo1]. Crea trigger BEFORE INSERT para nombre Falla con RAISE(ABORT). Reemplazo de Ada+Falla debe lanzar DbUpdateException; nueva apertura conserva cantidad1 y SQL sigue retornando Ada. finally borra archivo. Demuestra rollback de ese fallo, no todos los errores posibles de disco.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
    public void ReemplazoPersisteDatosExactosYRevierteFalloDeInsercion()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"carga-{Guid.NewGuid()}.db");
        string cadena = $"Data Source={ruta};Pooling=False";
        try
        {
            var lote = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement);
            using (var almacen = new AlmacenPartidas(cadena))
            {
                new CatalogoPersonajes(almacen.Infraestructura).DarDeAlta(lote[0] with { Nombre = "Viejo" });
                new CatalogoPersonajes(almacen.Infraestructura).DarDeAlta(lote[0] with { Nombre = "Inactivo" });
                new CatalogoPersonajes(almacen.Infraestructura).Desactivar(2);
                Assert.Equal(2, new CatalogoPersonajes(almacen.Infraestructura).CantidadCatalogo());
                new CatalogoPersonajes(almacen.Infraestructura).ReemplazarCatalogo(lote);
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
            using (var almacen = new AlmacenPartidas(cadena))
                Assert.Throws<Microsoft.EntityFrameworkCore.DbUpdateException>(() => new CatalogoPersonajes(almacen.Infraestructura).ReemplazarCatalogo([lote[0], lote[0] with { Nombre = "Falla" }]));
            using (var almacen = new AlmacenPartidas(cadena)) Assert.Equal(1, new CatalogoPersonajes(almacen.Infraestructura).CantidadCatalogo());
            comando.CommandText = "SELECT Nombre FROM PersonajeCatalogo";
            Assert.Equal("Ada", comando.ExecuteScalar());
        }
        finally { File.Delete(ruta); }
    }
```

### `CargaJsonTests.HttpAutenticaResumenYReemplazoYConservaCatalogoAnteRechazo`

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

### `CargaJsonTests.RechazaTextoDeDominioConUbicacion`

Theory nombre/espacios, ascii/tab, ascii/limite y ascii/lineas. Expande limite a121 asteriscos y lineas a26 líneas separadas por CR. Copia AltaPersonaje del fixture con campo inválido, intenta reemplazo en memoria y exige ErrorApi con Personaje1 y nombre de campo. No comprueba nombre del personaje en todos los diagnósticos.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
    public void RechazaTextoDeDominioConUbicacion(string campo, string valor)
    {
        var datos = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement)[0];
        if (valor == "limite") valor = new string('*', 121);
        if (valor == "lineas") valor = string.Join('\r', Enumerable.Repeat("*", 26));
        datos = campo == "nombre" ? datos with { Nombre = valor } : datos with { Ascii = valor };
        using var almacen = new AlmacenPartidas("Data Source=:memory:");
        var juego = new CatalogoPersonajes(almacen.Infraestructura);
        var error = Assert.Throws<ErrorApi>(() => new CatalogoPersonajes(almacen.Infraestructura).ReemplazarCatalogo([datos]));
        Assert.Contains("Personaje 1", error.Message);
        Assert.Contains(campo, error.Message);
    }
```

### `CargaJsonTests.CamposObligatoriosYTiposEstrictos`

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

### `CargaJsonTests.RechazaReglasDeAtributos`

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

### `CargaJsonTests.DiagnosticoIdentificaElAtributoFueraDeRango`

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

## Recorridos y ejemplos

Normal: Lote Uno contiene Ada con atributos250/350/450/550/650/750. Reemplazar dos veces conserva una fila y reinicia IDs al estado esperado.

Alternativo o de error: Lote incompleto o nombres duplicados normalizados se rechazan; un trigger que aborta insertar Falla debe restaurar el catálogo Ada anterior.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.

## Actualización del spec 006

Preparación y fragmentos corresponden al código final: el catálogo, las sesiones y las acciones se resuelven por sus servicios separados con un AlmacenPartidas compartido. Las aserciones del contrato se conservan. Véase [auditoría006](../../verificacion.md#auditoría-del-spec-006--2026-11-22).
