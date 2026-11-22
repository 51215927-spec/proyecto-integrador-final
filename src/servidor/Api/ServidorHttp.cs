using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

internal sealed class ErrorApi(int estado, string codigo, string mensaje) : Exception(mensaje)
{
    internal int Estado { get; } = estado;
    internal string Codigo { get; } = codigo;

    internal static ErrorApi Entrada() => new(400, "entradaInvalida", "Los datos de entrada no son válidos.");
    internal static ErrorApi NoEncontrado() => new(404, "recursoNoEncontrado", "El recurso no existe.");
    internal static ErrorApi Indisponible() => new(503, "servicioNoDisponible", "El almacenamiento no está disponible.");

    internal Task Responder(HttpContext contexto)
    {
        contexto.Response.StatusCode = Estado;
        return contexto.Response.WriteAsJsonAsync(new
        {
            Codigo,
            mensaje = Message,
            idSeguimiento = contexto.TraceIdentifier
        });
    }
}

internal static class ServidorHttp
{
    internal static WebApplication Crear(string[] args, TimeProvider? reloj = null, ConfiguracionServidor? inicio = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        if (inicio is not null)
        {
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["urls"] = inicio.Direccion,
                ["ConnectionStrings:Juego"] = new SqliteConnectionStringBuilder
                {
                    DataSource = inicio.Archivo, DefaultTimeout = 1
                }.ToString(),
                ["Catalogo:Credencial"] = inicio.Credencial
            });
            builder.WebHost.UseUrls(inicio.Direccion);
        }
        if (builder.Configuration["urls"] is null)
            builder.WebHost.UseUrls("http://0.0.0.0:5000");
        builder.Services.AddSingleton(_ => new AlmacenPartidas(
            builder.Configuration.GetConnectionString("Juego") ?? "Data Source=guerra.db;Default Timeout=1"));
        builder.Services.AddSingleton(sp => new CatalogoPersonajes(sp.GetRequiredService<AlmacenPartidas>().Infraestructura));
        builder.Services.AddSingleton<AdministradorSesiones>();
        builder.Services.AddSingleton<CoordinadorPartidas>();
        builder.Services.AddSingleton(reloj ?? TimeProvider.System);
        builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
        builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);

        var app = builder.Build();
        app.Use(async (contexto, siguiente) =>
        {
            try
            {
                await siguiente(contexto);
            }
            catch (ErrorApi error)
            {
                await error.Responder(contexto);
            }
            catch (BadHttpRequestException)
            {
                await ErrorApi.Entrada().Responder(contexto);
            }
            catch (SqliteException)
            {
                await ErrorApi.Indisponible().Responder(contexto);
            }
            catch (DbUpdateException error) when (error.InnerException is SqliteException)
            {
                await ErrorApi.Indisponible().Responder(contexto);
            }
        });

        app.MapPost("/api/v1/personajes",
            (AltaPersonaje datos, HttpContext contexto, CatalogoPersonajes juego) =>
            {
                ExigirCatalogo(contexto);

                return Results.Json(new { idPersonaje = juego.DarDeAlta(datos) }, statusCode: 201);
            });
        void ExigirCatalogo(HttpContext contexto)
        {
            var esperada = app.Configuration["Catalogo:Credencial"];
            var recibida = Credencial(contexto);
            if (string.IsNullOrWhiteSpace(esperada) || recibida is null ||
                !CryptographicOperations.FixedTimeEquals(
                    SHA256.HashData(Encoding.UTF8.GetBytes(esperada)),
                    SHA256.HashData(Encoding.UTF8.GetBytes(recibida))))
                throw new ErrorApi(401, "credencialInvalida", "La credencial no es válida.");
        }
        app.MapGet("/api/v1/personajes/resumen", (HttpContext contexto, CatalogoPersonajes juego) =>
        {
            ExigirCatalogo(contexto);
            return Results.Ok(new { cantidad = juego.CantidadCatalogo() });
        });
        app.MapPut("/api/v1/personajes", (System.Text.Json.JsonElement datos, HttpContext contexto, CatalogoPersonajes juego) =>
        {
            ExigirCatalogo(contexto);
            return Results.Ok(new { cantidad = juego.ReemplazarCatalogo(LotePersonajes.Leer(datos)) });
        });
        var partidas = app.MapGroup("/api/v1/partidas/{id}");
        partidas.MapPut("/participantes/me/sesion",
            (Guid id, Incorporacion datos, HttpContext contexto, AdministradorSesiones juego) =>
                Results.Ok(juego.Incorporar(id, Credencial(contexto), datos)));
        partidas.MapPut("/participantes/me/presencia",
            (Guid id, HttpContext contexto, AdministradorSesiones juego) =>
            {
                juego.Renovar(id, Credencial(contexto), IdSesion(contexto));
                return Results.NoContent();
            });
        partidas.MapGet("/presentacion", (Guid id, HttpContext contexto, CoordinadorPartidas juego) =>
        {
            contexto.Response.Headers.CacheControl = "no-store";
            return Results.Ok(juego.Presentacion(id, Credencial(contexto), IdSesion(contexto)));
        });
        partidas.MapPost("/elecciones",
            (Guid id, Eleccion datos, HttpContext contexto, CoordinadorPartidas juego) =>
            {
                var resultado = juego.Elegir(id, Credencial(contexto), IdSesion(contexto), datos);
                return resultado.Repetida ? Results.Ok(resultado.Recibo)
                    : Results.Created($"/api/v1/partidas/{id}/elecciones/{datos.IdOperacion}", resultado.Recibo);
            });
        partidas.MapGet("/elecciones/{idOperacion}",
            (Guid id, Guid idOperacion, HttpContext contexto, CoordinadorPartidas juego) =>
                Results.Ok(juego.ConsultarRecibo(id, Credencial(contexto), IdSesion(contexto), idOperacion)));
        partidas.MapPut("/participantes/me/abandono", (Guid id, HttpContext contexto, CoordinadorPartidas juego) =>
        {
            juego.Abandonar(id, Credencial(contexto), IdSesion(contexto));
            return Results.NoContent();
        });
        return app;
    }

    private static string? Credencial(HttpContext contexto) =>
        AuthenticationHeaderValue.TryParse(contexto.Request.Headers.Authorization, out var cabecera)
            && cabecera.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)
            ? cabecera.Parameter : null;

    private static Guid IdSesion(HttpContext contexto) =>
        Guid.TryParse(contexto.Request.Headers["X-Sesion"], out var id) ? id : Guid.Empty;
}
