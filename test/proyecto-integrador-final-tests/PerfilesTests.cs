using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Proyecto_Integrador_Final.Api;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class PerfilesTests
{
    [Fact]
    public void MigraAccesoAnteriorYAgregaPerfilesConcurrentementeSinPerderNinguno()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "perfiles-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta); var uri = new Uri("http://localhost:5000/");
            var anterior = new InvitadoLocal(Guid.NewGuid(), new string('A', 64), "Anterior");
            local.Guardar(uri, anterior);
            // El formato de una única identidad de la versión anterior debe seguir siendo legible.
            File.WriteAllText(Directory.GetFiles(carpeta).Single(), JsonSerializer.Serialize(anterior));
            var nuevos = Enumerable.Range(0, 6).Select(i => new InvitadoLocal(Guid.NewGuid(), new string((char)('B'+i), 64), "Perfil " + i)).ToArray();
            Parallel.ForEach(nuevos, nuevo => new AccesoLocal(carpeta).Agregar(uri, nuevo));
            var perfiles = new AccesoLocal(carpeta).Perfiles(uri);
            Assert.Equal(7, perfiles.Length); Assert.Contains(anterior, perfiles);
            Assert.All(nuevos, nuevo => Assert.Contains(nuevo, perfiles));
            var reemplazo = new InvitadoLocal(Guid.NewGuid(), new string('Z', 64), "Reemplazo");
            local.Reemplazar(uri, anterior.Id, reemplazo);
            Assert.DoesNotContain(anterior, local.Perfiles(uri)); Assert.Contains(reemplazo, local.Perfiles(uri));
            Assert.Equal(7, local.Perfiles(uri).Length);
            Assert.Empty(local.Perfiles(new Uri("http://localhost:5001/")));
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    [Fact]
    public async Task DisponibilidadNoReservaNiRenuevaSesionYRespetaVencimiento()
    {
        var reloj = new Reloj();
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"], reloj);
        await app.StartAsync();
        using var http = new HttpClient { BaseAddress = new(app.Urls.Single()) };
        using var alta = await http.PostAsJsonAsync("api/v2/invitados", new { });
        var acceso = await alta.Content.ReadFromJsonAsync<JsonElement>();
        http.DefaultRequestHeaders.Authorization = new("Bearer", acceso.GetProperty("secreto").GetString());
        http.DefaultRequestHeaders.Add("X-Invitado", acceso.GetProperty("id").GetString());
        http.DefaultRequestHeaders.Add("X-Sesion", Guid.NewGuid().ToString());
        async Task<bool> Libre()
        {
            using var respuesta = await http.PutAsJsonAsync("api/v2/acceso/disponibilidad", new { });
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            return (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("disponible").GetBoolean();
        }
        Assert.True(await Libre()); Assert.True(await Libre());
        using var sinSesion = await http.GetAsync("api/v2/acceso");
        Assert.Equal(HttpStatusCode.Conflict, sinSesion.StatusCode);
        using var sesion = await http.PutAsJsonAsync("api/v2/acceso/sesion", new { });
        sesion.EnsureSuccessStatusCode();
        Assert.False(await Libre());
        reloj.Ahora += TimeSpan.FromSeconds(14); Assert.False(await Libre());
        reloj.Ahora += TimeSpan.FromSeconds(1); Assert.True(await Libre());
    }
    private sealed class Reloj : TimeProvider
    {
        internal DateTimeOffset Ahora = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Ahora;
    }
}
