using Microsoft.EntityFrameworkCore;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public class AtomicidadInicioTests
{
    [Fact]
    public void FallaDeAsignacionRevierteInicioCompletoYPermiteReintentar()
    {
        using var almacen = new AlmacenPartidas("Data Source=:memory:");
        var invitados = new Invitados(almacen); var reloj = TimeProvider.System;
        var catalogo = new CatalogoPersonajes(almacen.Infraestructura);
        var acceso = new Emparejamiento(invitados, almacen, new(almacen, reloj), catalogo, reloj, new());
        var a = invitados.Crear(null); var b = invitados.Crear(null);
        var sa = Guid.NewGuid(); var sb = Guid.NewGuid();
        acceso.Ejecutar(a.Id, a.Secreto, sa, "sesion"); acceso.Ejecutar(b.Id, b.Secreto, sb, "sesion");
        acceso.Ejecutar(a.Id, a.Secreto, sa, "jugar");
        almacen.Infraestructura.Ejecutar(true, db => db.Database.ExecuteSqlRaw("CREATE TRIGGER falla_asignacion BEFORE INSERT ON AsignacionInvitado BEGIN SELECT RAISE(ABORT, 'fallo controlado'); END"));
        Assert.Throws<DbUpdateException>(() => acceso.Ejecutar(b.Id, b.Secreto, sb, "jugar"));
        almacen.Infraestructura.Ejecutar(false, db =>
        {
            Assert.Empty(db.Partidas); Assert.Empty(db.Asignaciones); Assert.Empty(db.Cartas); Assert.Empty(db.Solicitudes);
            return true;
        });
        almacen.Infraestructura.Ejecutar(true, db => db.Database.ExecuteSqlRaw("DROP TRIGGER falla_asignacion"));
        for (int i = 0; i < 6; i++) catalogo.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
        Assert.Equal("partida", acceso.Ejecutar(b.Id, b.Secreto, sb, "estado").Estado);
    }

    [Fact]
    public async Task CancelacionConcurrenteConInicioAutomaticoNoDejaPartidasHuerfanas()
    {
        for (int intento = 0; intento < 8; intento++)
        {
            using var almacen = new AlmacenPartidas("Data Source=:memory:");
            var invitados = new Invitados(almacen); var reloj = TimeProvider.System;
            var catalogo = new CatalogoPersonajes(almacen.Infraestructura);
            var acceso = new Emparejamiento(invitados, almacen, new(almacen, reloj), catalogo, reloj, new());
            var a = invitados.Crear(null); var b = invitados.Crear(null);
            var sa = Guid.NewGuid(); var sb = Guid.NewGuid();
            acceso.Ejecutar(a.Id, a.Secreto, sa, "sesion"); acceso.Ejecutar(b.Id, b.Secreto, sb, "sesion");
            acceso.Ejecutar(a.Id, a.Secreto, sa, "jugar"); acceso.Ejecutar(b.Id, b.Secreto, sb, "jugar");
            var carga = Task.Run(() =>
            {
                for (int i = 0; i < 6; i++) catalogo.DarDeAlta(new($"Carta {i}-{intento}", "", "*", 750, 550, 500, 250, 500, 450));
            });
            await Task.WhenAll(carga, Task.Run(() => acceso.Ejecutar(a.Id, a.Secreto, sa, "cancelar")),
                Task.Run(() => acceso.Ejecutar(b.Id, b.Secreto, sb, "estado")));
            var estado = acceso.Ejecutar(a.Id, a.Secreto, sa, "estado");
            almacen.Infraestructura.Ejecutar(false, db =>
            {
                if (estado.Estado == "partida")
                { Assert.Single(db.Partidas); Assert.Equal(2, db.Asignaciones.Count()); Assert.Null(db.Partidas.Single().AbandonoDe); }
                else { Assert.Equal("libre", estado.Estado); Assert.Empty(db.Partidas); Assert.Empty(db.Asignaciones); }
                return true;
            });
        }
    }
}
