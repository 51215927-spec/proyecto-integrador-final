using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Proyecto_Integrador_Final.Api;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class SeleccionPerfilesTests
{
    [Theory]
    [InlineData(1, 0, 1, false)]
    [InlineData(2, 0, 2, true)]
    [InlineData(3, 2, 3, false)]
    public async Task EligeUnicoLibreAutomaticamenteOVariosMedianteSelector(int cantidad, int ocupados, int elegido, bool pregunta)
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        var adquiridos = new ConcurrentQueue<string>();
        app.Use(async (c, siguiente) =>
        { if (c.Request.Path == "/api/v2/acceso/sesion") adquiridos.Enqueue(c.Request.Headers["X-Invitado"].ToString()); await siguiente(c); });
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "seleccion-" + Guid.NewGuid());
        var clientesOcupados = new List<ClienteAccesoHttp>();
        try
        {
            var local = new AccesoLocal(carpeta); var uri = new Uri(app.Urls.Single() + "/");
            var perfiles = new List<InvitadoLocal>();
            for (int i = 0; i < cantidad; i++)
            {
                var perfil = await ClienteAccesoHttp.Crear(uri, "Perfil " + (i+1), default);
                perfiles.Add(perfil); local.Agregar(uri, perfil);
                if (i < ocupados)
                {
                    var http = new ClienteAccesoHttp(uri, perfil); clientesOcupados.Add(http);
                    await http.Accion("sesion", default);
                }
            }
            local.GuardarConexion(uri); adquiridos.Clear();
            var entrada = "jugar\n" + (pregunta ? elegido + "\n" : "") + "cancelar\nsalir\n";
            var salida = new StringWriter();
            await new InicioCliente(new StringReader(entrada), salida, local).Ejecutar(default);
            Assert.Contains("Buscando oponente", salida.ToString());
            Assert.Equal(pregunta, salida.ToString().Contains("Elegí un perfil"));
            Assert.NotEmpty(adquiridos);
            Assert.All(adquiridos, id => Assert.Equal(perfiles[elegido-1].Id.ToString(), id));
            foreach (var perfil in perfiles) Assert.DoesNotContain(perfil.Secreto, salida.ToString());
        }
        finally { foreach (var http in clientesOcupados) http.Dispose(); if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    [Fact]
    public async Task TodosOcupadosOfreceCrearOtroYConservaLosPerfilesAnteriores()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "ocupados-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta); var uri = new Uri(app.Urls.Single() + "/");
            var anterior = await ClienteAccesoHttp.Crear(uri, "Anterior", default); local.Agregar(uri, anterior); local.GuardarConexion(uri);
            using var ocupado = new ClienteAccesoHttp(uri, anterior); await ocupado.Accion("sesion", default);
            var salida = new StringWriter();
            await new InicioCliente(new StringReader("jugar\ncrear perfil\nNuevo\ncancelar\nsalir\n"), salida, local).Ejecutar(default);
            Assert.Contains("Todos los perfiles están en uso", salida.ToString());
            Assert.Contains("Buscando oponente", salida.ToString());
            Assert.Equal(2, local.Perfiles(uri).Length); Assert.Contains(anterior, local.Perfiles(uri));
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    [Fact]
    public async Task CrearPerfilEstaDisponibleAunqueHayaUnoLibre()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "crear-perfil-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta); var uri = new Uri(app.Urls.Single() + "/");
            var anterior = await ClienteAccesoHttp.Crear(uri, "Anterior", default); local.Agregar(uri, anterior); local.GuardarConexion(uri);
            await new InicioCliente(new StringReader("crear perfil\nNuevo\ncancelar\nsalir\n"), new StringWriter(), local).Ejecutar(default);
            Assert.Equal(2, local.Perfiles(uri).Length); Assert.Contains(anterior, local.Perfiles(uri));
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    [Fact]
    public async Task SiOtroClienteTomaElElegidoActualizaDisponibilidadSinRobarSesion()
    {
        await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", "--ConnectionStrings:Juego=Data Source=:memory:"]);
        InvitadoLocal? primero = null;
        int carrera = 1;
        var sesionRival = Guid.NewGuid();
        app.Use(async (c, siguiente) =>
        {
            if (primero is not null && c.Request.Path == "/api/v2/acceso/sesion" &&
                c.Request.Headers["X-Invitado"].ToString() == primero.Id.ToString() && Interlocked.Exchange(ref carrera, 0) == 1)
                app.Services.GetRequiredService<Emparejamiento>().Ejecutar(primero.Id, primero.Secreto, sesionRival, "sesion");
            await siguiente(c);
        });
        await app.StartAsync();
        var carpeta = Path.Combine(Path.GetTempPath(), "carrera-perfil-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta); var uri = new Uri(app.Urls.Single() + "/");
            primero = await ClienteAccesoHttp.Crear(uri, "Primero", default);
            var segundo = await ClienteAccesoHttp.Crear(uri, "Segundo", default);
            local.Agregar(uri, primero); local.Agregar(uri, segundo); local.GuardarConexion(uri);
            var salida = new StringWriter();
            await new InicioCliente(new StringReader("jugar\n1\ncancelar\nsalir\n"), salida, local).Ejecutar(default);
            Assert.Contains("acaba de ocuparse", salida.ToString());
            Assert.Contains("Perfil: Segundo", salida.ToString());
            var acceso = app.Services.GetRequiredService<Emparejamiento>();
            Assert.Equal("libre", acceso.Ejecutar(primero.Id, primero.Secreto, sesionRival, "estado").Estado);
            Assert.Equal("sesionOcupada", Assert.Throws<ErrorApi>(() => acceso.Ejecutar(primero.Id, primero.Secreto, Guid.NewGuid(), "sesion")).Codigo);
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }
}
