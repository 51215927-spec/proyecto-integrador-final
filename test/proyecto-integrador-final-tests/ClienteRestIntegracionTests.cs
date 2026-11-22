using Microsoft.Extensions.DependencyInjection;
using Proyecto_Integrador_Final.Api;
using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class ClienteRestIntegracionTests
{
    [Fact]
    public async Task ReinicioDelServidorConservaPartidaYRequiereReincorporarLaMismaSesion()
    {
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-reinicio-cliente-{Guid.NewGuid():N}.db");
        try
        {
            await using var primero = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            var creada = primero.Services.GetRequiredService<CoordinadorPartidas>().CrearPartida();
            await primero.StartAsync();
            string url = primero.Urls.Single();
            Guid sesion = Guid.NewGuid();
            using var cliente = new ClientePartidaHttp(new(new Uri(url + "/"), creada.Id, creada.CredencialJ1, "Ana"), sesion);
            await cliente.Incorporar(default);
            var anterior = await cliente.Presentacion(default);
            await primero.StopAsync();
            await using var segundo = ServidorHttp.Crear([$"--urls={url}", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            await segundo.StartAsync();
            var error = await Assert.ThrowsAsync<FalloCliente>(() => cliente.Presentacion(default));
            Assert.Equal("sesionVencida", error.Codigo);
            Assert.Equal(sesion, (await cliente.Incorporar(default)).IdSesion);
            var recuperada = await cliente.Presentacion(default);
            Assert.Equal(anterior.IdPartida, recuperada.IdPartida);
            Assert.Equal(anterior.Revision, recuperada.Revision);
            Assert.Equal("Ana", recuperada.Nombres["J1"]);
            await segundo.StopAsync();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }

    [Fact]
    public async Task DosClientesJueganContraServidorLocalRecuperanReciboYSesion()
    {
        string archivo = Path.Combine(Path.GetTempPath(), $"guerra-cliente-{Guid.NewGuid():N}.db");
        try
        {
            await using var app = ServidorHttp.Crear(["--urls=http://127.0.0.1:0", $"--ConnectionStrings:Juego=Data Source={archivo}"]);
            var juego = app.Services.GetRequiredService<CoordinadorPartidas>();
            var partida = juego.CrearPartida();
            for (int i = 0; i < 6; i++) app.Services.GetRequiredService<CatalogoPersonajes>().DarDeAlta(new($"Carta {i}", "Descripción no visible", "ARTE NO VISIBLE", 750, 550, 500, 250, 500, 450));
            await app.StartAsync();
            var config1 = new ConfiguracionCliente(new Uri(app.Urls.Single() + "/"), partida.Id, partida.CredencialJ1, "Ana");
            var config2 = config1 with { Credencial = partida.CredencialJ2, Nombre = "Ana" };
            Guid sesion1 = Guid.NewGuid(), sesion2 = Guid.NewGuid();
            using var j1 = new ClientePartidaHttp(config1, sesion1);
            using var j2 = new ClientePartidaHttp(config2, sesion2);
            Assert.Equal("J1", (await j1.Incorporar(default)).Rol);
            Assert.Equal("J2", (await j2.Incorporar(default)).Rol);
            await j1.Renovar(default);
            juego.Iniciar(partida.Id);
            var v1 = await j1.Presentacion(default);
            var v2 = await j2.Presentacion(default);
            new EstadoCliente(partida.Id).Aplicar(v1);
            Assert.Empty(v1.Mano.Select(c => c.IdPersonaje).Intersect(v2.Mano.Select(c => c.IdPersonaje)));
            var s = v1.Solicitud!;
            var e = new EleccionCliente(Guid.NewGuid(), s.IdSolicitud, v1.Revision, s.IdsPersonajePermitidos[0], s.AtributosPermitidos[0]);
            var recibo = await j1.Elegir(e, default);
            Assert.Equal(recibo, await j1.Recibo(e.IdOperacion, default));
            Assert.Equal(recibo, await j1.Elegir(e, default));
            var oculta = await j2.Presentacion(default);
            Assert.Empty(oculta.Resultados);
            Assert.Equal("carta", oculta.Solicitud!.Tipo);
            await j2.Elegir(new(Guid.NewGuid(), oculta.Solicitud.IdSolicitud, oculta.Revision, oculta.Solicitud.IdsPersonajePermitidos[0], null), default);
            var resuelta = await j1.Presentacion(default);
            Assert.Single(resuelta.Resultados);
            new EstadoCliente(partida.Id).Aplicar(resuelta);
            app.Services.GetRequiredService<AdministradorSesiones>().CerrarSesion(partida.Id, "J1");
            var vencida = await Assert.ThrowsAsync<FalloCliente>(() => j1.Presentacion(default));
            Assert.Equal("sesionVencida", vencida.Codigo);
            Assert.Equal(sesion1, (await j1.Incorporar(default)).IdSesion);
            Assert.Equal(recibo, await j1.Recibo(e.IdOperacion, default));
            await j1.Abandonar(default);
            var final = await j2.Presentacion(default);
            Assert.Equal("abandono", final.MotivoFin);
            Assert.Null(final.Ganador);
            await app.StopAsync();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (string sufijo in new[] { "", "-wal", "-shm" }) File.Delete(archivo + sufijo);
        }
    }
}
