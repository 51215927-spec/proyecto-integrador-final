using System.Net;
using Proyecto_Integrador_Final.CargaPersonajes;

namespace Proyecto_Integrador_Final.Tests;

public class ConsolaAltaTests
{
    [Theory]
    [InlineData("sí", 1)]
    [InlineData(" SI ", 1)]
    [InlineData("no", 0)]
    [InlineData("", 0)]
    [InlineData("quizás\nsi", 1)]
    public async Task ListaPorRutaConfirmaAntesDeUnicoPut(string respuesta, int puts)
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"carga con espacios {Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            using var handler = new Captura();
            using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            using var salida = new StringWriter();
            Assert.Equal(0, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader(respuesta), salida, ruta));
            Assert.Equal(puts, handler.Puts);
            Assert.Contains("actual: 7", salida.ToString());
            Assert.Contains("JSON: 1", salida.ToString());
            Assert.DoesNotContain("Nombre:", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }

    [Fact]
    public async Task RutaInexistenteFinalizaSinConexion()
    {
        using var salida = new StringWriter();
        Assert.Equal(1, await ConsolaAlta.Iniciar(new StringReader(""), salida, ["/no/existe.json"]));
        Assert.Contains("archivo", salida.ToString());
    }


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ErrorHttpNoReintentaNiAfirmaRollback(bool perdida)
    {
        string ruta = Path.GetTempFileName();
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            using var handler = new Fallo(perdida);
            using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            using var salida = new StringWriter();
            Assert.Equal(1, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader("si"), salida, ruta));
            Assert.Equal(2, handler.Solicitudes);
            Assert.DoesNotContain("Catálogo reemplazado", salida.ToString());
            Assert.Contains(perdida ? "No se pudo confirmar el resultado" : "Personaje 1 (Ada): fuerza", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("[{}]")]
    [InlineData("[1]")]
    public async Task ArchivoInvalidoNoEnviaHttp(string json)
    {
        string ruta = Path.GetTempFileName();
        await File.WriteAllTextAsync(ruta, json);
        try
        {
            using var handler = new Captura();
            using var cliente = new HttpClient(handler);
            using var salida = new StringWriter();
            Assert.Equal(1, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader(""), salida, ruta));
            Assert.Equal(0, handler.Puts);
            Assert.Contains("archivo", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }

    [Fact]
    public async Task RutaRelativaConEspaciosYFinDeEntradaEnConfirmacionCancela()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"ruta con espacios {Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(ruta, CargaJsonTests.Uno);
        try
        {
            using var handler = new Captura();
            using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            using var salida = new StringWriter();
            string relativa = Path.GetRelativePath(Environment.CurrentDirectory, ruta);
            Assert.Equal(0, await ConsolaAlta.Ejecutar(cliente, "clave", new StringReader(relativa + "\n"), salida));
            Assert.Equal(0, handler.Puts);
            Assert.Contains("Carga cancelada", salida.ToString());
        }
        finally { File.Delete(ruta); }
    }

    private sealed class Fallo(bool perdida) : HttpMessageHandler
    {
        internal int Solicitudes;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Solicitudes++;
            if (request.Method == HttpMethod.Get)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"cantidad\":0}") });
            if (perdida) throw new HttpRequestException("Respuesta perdida");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{\"mensaje\":\"Personaje 1 (Ada): fuerza\"}") });
        }
    }

    private sealed class Captura : HttpMessageHandler
    {
        internal int Puts;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("clave", request.Headers.Authorization!.Parameter);
            if (request.Method == HttpMethod.Put) Puts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(request.Method == HttpMethod.Get ? "{\"cantidad\":7}" : "{\"cantidad\":1}") });
        }
    }
}
