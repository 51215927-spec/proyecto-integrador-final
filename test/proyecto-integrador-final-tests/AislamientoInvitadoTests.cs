using Microsoft.AspNetCore.Builder;
using Proyecto_Integrador_Final.Api;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class AislamientoInvitadoTests
{
    [Fact]
    public async Task CambiarConexionEmiteOtroInvitadoSinEnviarElSecretoDelAnterior()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        var autorizaciones = new System.Collections.Concurrent.ConcurrentQueue<string>();
        app.Use(async (c, siguiente) => { autorizaciones.Enqueue(c.Request.Headers.Authorization.ToString()); await siguiente(c); });
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "aislamiento-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta);
            var anterior = new InvitadoLocal(Guid.NewGuid(), new string('A', 64), "Ada");
            var a = new Uri("http://localhost:5000/");
            var b = new Uri(app.Urls.Single() + "/");
            local.Guardar(a, anterior); local.GuardarConexion(a);
            using var entrada = new StringReader($"conexion\n{b}\njugar\n\ncancelar\nsalir\n");
            using var salida = new StringWriter();
            using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await new InicioCliente(entrada, salida, local).Ejecutar(plazo.Token);
            Assert.Equal(anterior, local.Leer(a));
            Assert.NotEqual(anterior.Id, local.Leer(b)!.Id);
            Assert.DoesNotContain(autorizaciones, valor => valor.Contains(anterior.Secreto));
            Assert.DoesNotContain(anterior.Secreto, salida.ToString());
            Assert.DoesNotContain(local.Leer(b)!.Secreto, salida.ToString());
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    [Fact]
    public async Task IdentidadRechazadaSoloSeReemplazaTrasConfirmacionExplicita()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        int altas = 0;
        app.Use(async (c, siguiente) =>
        { if (c.Request.Path == "/api/v2/invitados") Interlocked.Increment(ref altas); await siguiente(c); });
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "rechazo-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta); var servidor = new Uri(app.Urls.Single() + "/");
            var anterior = new InvitadoLocal(Guid.NewGuid(), new string('A', 64), "Ada");
            local.Guardar(servidor, anterior); local.GuardarConexion(servidor);
            var salida = new StringWriter();
            await new InicioCliente(new StringReader("jugar\nsalir\n"), salida, local).Ejecutar(default);
            Assert.Equal(0, altas); Assert.Equal(anterior, local.Leer(servidor));
            Assert.Contains("no recupera las partidas anteriores", salida.ToString());
            await new InicioCliente(new StringReader("jugar\nNUEVO\n\ncancelar\nsalir\n"), salida, local).Ejecutar(default);
            Assert.Equal(1, altas); Assert.NotEqual(anterior.Id, local.Leer(servidor)!.Id);
            Assert.DoesNotContain(anterior.Secreto, salida.ToString());
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }
}
