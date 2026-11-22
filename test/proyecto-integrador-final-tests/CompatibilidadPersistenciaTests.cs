using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class CompatibilidadPersistenciaTests
{
    [Fact]
    public async Task BaseDeE18a050ContinuaSinMigracionYRecuperaReciboAnterior()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"anterior-{Guid.NewGuid()}.db");
        string cadena = $"Data Source={ruta};Pooling=False";
        string fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures");
        using var datos = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixtures, "partida-e18a050.json")));
        var d = datos.RootElement;
        Guid id = d.GetProperty("id").GetGuid();
        var eleccion = d.GetProperty("eleccion").Deserialize<Eleccion>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        try
        {
            using (var db = new SqliteConnection(cadena))
            {
                db.Open();
                using var command = db.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys=OFF;\n" + File.ReadAllText(Path.Combine(fixtures, "partida-e18a050.sql"));
                command.ExecuteNonQuery();
            }
            string esquema = Esquema(cadena);
            await using var app = ServidorHttp.Crear([$"--ConnectionStrings:Juego={cadena}", "--urls=http://127.0.0.1:0"]);
            await app.StartAsync();
            using var j1 = new HttpClient { BaseAddress = new Uri($"{app.Urls.Single()}/api/v1/partidas/{id}/") };
            using var j2 = new HttpClient { BaseAddress = j1.BaseAddress };
            foreach (var (cliente, rol, nombre) in new[] { (j1, "J1", "Ada"), (j2, "J2", "Bruno") })
            {
                cliente.DefaultRequestHeaders.Authorization = new("Bearer", d.GetProperty($"credencial{rol}").GetString());
                Guid sesion = Guid.NewGuid();
                using var respuesta = await cliente.PutAsJsonAsync("participantes/me/sesion", new { idSesion = sesion, nombre });
                Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
                cliente.DefaultRequestHeaders.Add("X-Sesion", sesion.ToString());
            }
            using var duplicada = await j1.PostAsJsonAsync("elecciones", eleccion);
            Assert.Equal(HttpStatusCode.OK, duplicada.StatusCode);
            var recibo = await duplicada.Content.ReadFromJsonAsync<Recibo>();
            Assert.Equal(d.GetProperty("recibo").GetProperty("revisionConfirmada").GetInt32(), recibo!.RevisionConfirmada);
            var anterior = await j2.GetFromJsonAsync<JsonElement>("presentacion");
            Assert.Equal("esperandoRival", anterior.GetProperty("fase").GetString());
            var mano = anterior.GetProperty("mano").ToString();
            // El catálogo nuevo no sustituye las copias que ya estaban en juego.
            var catalogo = app.Services.GetRequiredService<CatalogoPersonajes>();
            catalogo.ReemplazarCatalogo([new("Nueva", "", "*", 750, 550, 500, 250, 500, 450)]);
            var actual = await j2.GetFromJsonAsync<JsonElement>("presentacion");
            Assert.Equal(mano, actual.GetProperty("mano").ToString());
            var solicitud = actual.GetProperty("solicitud");
            using var respuestaRival = await j2.PostAsJsonAsync("elecciones", new Eleccion(Guid.NewGuid(),
                solicitud.GetProperty("idSolicitud").GetGuid(), actual.GetProperty("revision").GetInt32(),
                solicitud.GetProperty("idsPersonajePermitidos")[0].GetInt32(), null));
            Assert.Equal(HttpStatusCode.Created, respuestaRival.StatusCode);
            var despues = await j1.GetFromJsonAsync<JsonElement>("presentacion");
            Assert.Equal(recibo.RevisionConfirmada + 1, despues.GetProperty("revision").GetInt32());
            Assert.Equal(esquema, Esquema(cadena));
        }
        finally { File.Delete(ruta); }
    }

    private static string Esquema(string cadena)
    {
        using var db = new SqliteConnection(cadena);
        db.Open();
        using var command = db.CreateCommand();
        command.CommandText = "SELECT group_concat(sql, char(10)) FROM (SELECT sql FROM sqlite_master WHERE sql IS NOT NULL ORDER BY name)";
        return (string)command.ExecuteScalar()!;
    }
}
