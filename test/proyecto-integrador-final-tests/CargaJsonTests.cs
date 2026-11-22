using System.Text.Json;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class CargaJsonTests
{
    internal const string Uno = """[{"nombre":"Ada","descripcion":"","ascii":"*","fuerza":250,"velocidad":350,"resistencia":450,"inteligencia":550,"energia":650,"habilidad":750}]""";

    [Theory]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[{}]")]
    public void RechazaFormatoIncompleto(string json) =>
        Assert.Throws<ErrorApi>(() => LotePersonajes.Leer(JsonDocument.Parse(json).RootElement));

    [Fact]
    public void RechazaDuplicadosNormalizados()
    {
        string json = Uno[..^1] + "," + Uno[1..].Replace("Ada", " ada ");
        Assert.Contains("nombre", Assert.Throws<ErrorApi>(() => LotePersonajes.Leer(JsonDocument.Parse(json).RootElement)).Message);
    }

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

    [Fact]
    public void DiagnosticoIdentificaElAtributoFueraDeRango()
    {
        var datos = LotePersonajes.Leer(JsonDocument.Parse(Uno).RootElement)[0] with { Energia = 751 };
        var error = Assert.Throws<ErrorApi>(() => datos.Validar());
        Assert.StartsWith("energia:", error.Message);
    }
}
