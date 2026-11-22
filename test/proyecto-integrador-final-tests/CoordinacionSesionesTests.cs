using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class CoordinacionSesionesTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidarSesionEsperaLaAccionQueYaComproboAcceso(bool rotarCredencial)
    {
        using var almacen = new AlmacenPartidas("Data Source=:memory:");
        using var reloj = new RelojConBarrera();
        var sesiones = new AdministradorSesiones(almacen, reloj);
        var catalogo = new CatalogoPersonajes(almacen.Infraestructura);
        var juego = new CoordinadorPartidas(almacen, sesiones, catalogo);
        var creada = juego.CrearPartida();
        var idSesion = Guid.NewGuid();
        sesiones.Incorporar(creada.Id, creada.CredencialJ1, new(idSesion, "Ada"));
        sesiones.Incorporar(creada.Id, creada.CredencialJ2, new(Guid.NewGuid(), "Ana"));
        for (int i = 0; i < 6; i++)
            catalogo.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
        juego.Iniciar(creada.Id);
        var inicial = juego.Presentacion(creada.Id, creada.CredencialJ1, idSesion);
        var solicitud = inicial.Solicitud!;
        var eleccion = new Eleccion(Guid.NewGuid(), solicitud.IdSolicitud, inicial.Revision,
            solicitud.IdsPersonajePermitidos[0], "Fuerza");

        // El reloj detiene exactamente la comprobación de vigencia dentro del acceso.
        reloj.Activar();
        var accion = Task.Run(() => juego.Elegir(creada.Id, creada.CredencialJ1, idSesion, eleccion));
        Assert.True(reloj.EnAcceso.Wait(TimeSpan.FromSeconds(5)));
        using var invalidezIntentada = new ManualResetEventSlim();
        var invalidar = Task.Run(() =>
        {
            invalidezIntentada.Set();
            if (rotarCredencial) sesiones.ReemplazarCredencial(creada.Id, "J1");
            else sesiones.CerrarSesion(creada.Id, "J1");
        });
        try
        {
            Assert.True(invalidezIntentada.Wait(TimeSpan.FromSeconds(5)));
            Assert.False(invalidar.IsCompleted);
        }
        finally { reloj.Continuar.Set(); }

        var confirmacion = await accion.WaitAsync(TimeSpan.FromSeconds(5));
        await invalidar.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(inicial.Revision + 1, confirmacion.Recibo.RevisionConfirmada);
        var error = Assert.Throws<ErrorApi>(() => juego.Presentacion(creada.Id, creada.CredencialJ1, idSesion));
        Assert.Equal(rotarCredencial ? "credencialInvalida" : "sesionVencida", error.Codigo);
        var nuevaCredencial = rotarCredencial ? sesiones.ReemplazarCredencial(creada.Id, "J1") : creada.CredencialJ1;
        var nuevaSesion = Guid.NewGuid();
        sesiones.Incorporar(creada.Id, nuevaCredencial, new(nuevaSesion, "Ada"));
        Assert.Equal(confirmacion.Recibo, juego.ConsultarRecibo(creada.Id, nuevaCredencial, nuevaSesion, eleccion.IdOperacion));
    }

    private sealed class RelojConBarrera : TimeProvider, IDisposable
    {
        private int activo;
        internal ManualResetEventSlim EnAcceso { get; } = new();
        internal ManualResetEventSlim Continuar { get; } = new();
        internal void Activar() => Interlocked.Exchange(ref activo, 1);
        public override DateTimeOffset GetUtcNow()
        {
            if (Interlocked.Exchange(ref activo, 0) == 1)
            {
                EnAcceso.Set();
                if (!Continuar.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("No se liberó la barrera del acceso.");
            }
            return new DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero);
        }
        public void Dispose() { EnAcceso.Dispose(); Continuar.Dispose(); }
    }
}
