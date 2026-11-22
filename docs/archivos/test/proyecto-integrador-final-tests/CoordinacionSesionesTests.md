# CoordinacionSesionesTests.cs

Fuente: [test/proyecto-integrador-final-tests/CoordinacionSesionesTests.cs](../../../../test/proyecto-integrador-final-tests/CoordinacionSesionesTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj` (`net8.0`), namespace `Proyecto_Integrador_Final.Tests`. Referencia: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Exclusión entre acceso, acción e invalidación.

## Preparación, miembros, acciones y aserciones

InvalidarSesionEsperaLaAccionQueYaComproboAcceso usa SQLite :memory:, seis cartas, dos presencias y elección inicial. Un reloj con barrera detiene exactamente la comprobación de vigencia; mientras tanto otra tarea intenta cerrar sesión o rotar credencial (dos InlineData). Exige que la invalidación espere, libera barrera, comprueba una única revisión y luego rechazo del acceso previo y recuperación del recibo con sesión nueva. RelojConBarrera conserva activo, EnAcceso y Continuar; Activar arma la interrupción una vez mediante Interlocked; GetUtcNow publica la barrera y espera como máximo diez segundos antes de devolver hora fija; Dispose libera eventos.

## Errores, recursos y límites

Las esperas de tareas y barreras tienen límites de cinco/diez segundos. No usa retardos arbitrarios para suponer un orden. La hora controlada no prueba sincronización entre procesos independientes. xUnit descubre Fact/Theory y crea cada caso con sus datos; una aserción fallida invalida el caso. La suite final tiene 190 aprobadas, cero fallidas/omitidas; no acredita dos Windows físicos.

## Código contrastado

Archivo completo: incluye auxiliares, datos, parámetros y limpieza; no se omiten aserciones.

```csharp
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
```

Véase [auditoría006](../../verificacion.md#auditoría-del-spec-006--2026-11-22).
