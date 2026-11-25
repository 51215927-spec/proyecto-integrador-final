using Proyecto_Integrador_Final.Api;
using Microsoft.Data.Sqlite;

namespace Proyecto_Integrador_Final.Tests;

public class RecuperacionInvitadoTests
{
    private sealed class Reloj : TimeProvider
    {
        internal DateTimeOffset Ahora = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Ahora;
    }
    [Fact]
    public void VencimientoLiberaSalaYReinicioConservaSoloAsignacionesIniciadas()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "invitados-" + Guid.NewGuid() + ".db");
        try
        {
            var reloj = new Reloj();
            AccesoInvitado a, b;
            Guid partida;
            using (var almacen = new AlmacenPartidas($"Data Source={ruta};Pooling=False"))
            {
                var invitados = new Invitados(almacen);
                var sesiones = new AdministradorSesiones(almacen, reloj);
                var catalogo = new CatalogoPersonajes(almacen.Infraestructura);
                var acceso = new Emparejamiento(invitados, almacen, sesiones, catalogo, reloj, new Avisos());
                a = invitados.Crear(null); b = invitados.Crear(null);
                var sa = Guid.NewGuid(); var sb = Guid.NewGuid();
                EstadoAcceso Accion(AccesoInvitado i, Guid s, string accion) => acceso.Ejecutar(i.Id, i.Secreto, s, accion);
                Accion(a, sa, "sesion"); Accion(b, sb, "sesion");
                Accion(a, sa, "jugar"); Accion(b, sb, "jugar");
                reloj.Ahora += TimeSpan.FromSeconds(10); Accion(b, sb, "presencia");
                reloj.Ahora += TimeSpan.FromSeconds(5); acceso.Vencer();
                var espera = Accion(b, sb, "estado");
                Assert.Equal("buscando", espera.Estado);
                Assert.Equal("rivalSalio", espera.Aviso);
                Accion(a, sa, "sesion"); Assert.Equal("libre", Accion(a, sa, "estado").Estado);
                for (int i = 0; i < 6; i++) catalogo.DarDeAlta(new($"Carta {i}", "", "*", 750, 550, 500, 250, 500, 450));
                partida = Accion(a, sa, "jugar").Partida!.Value;
                reloj.Ahora += TimeSpan.FromSeconds(15); acceso.Vencer();
                Assert.False(sesiones.AmbosPresentes(partida));
                Accion(a, sa, "sesion"); Accion(b, sb, "sesion");
                Assert.True(sesiones.AmbosPresentes(partida));
                Assert.Equal(partida, Accion(a, sa, "jugar").Partida);
            }
            using (var almacen = new AlmacenPartidas($"Data Source={ruta};Pooling=False"))
            {
                var invitados = new Invitados(almacen);
                var sesiones = new AdministradorSesiones(almacen, reloj);
                var acceso = new Emparejamiento(invitados, almacen, sesiones, new(almacen.Infraestructura), reloj, new Avisos());
                Assert.False(sesiones.AmbosPresentes(partida));
                Assert.Equal(partida, acceso.Ejecutar(a.Id, a.Secreto, Guid.NewGuid(), "sesion").Partida);
                Assert.Equal(partida, acceso.Ejecutar(b.Id, b.Secreto, Guid.NewGuid(), "sesion").Partida);
                Assert.True(sesiones.AmbosPresentes(partida));
                Assert.Equal(a.Id, invitados.Autenticar(a.Id, a.Secreto).Id);
                using var db = new SqliteConnection($"Data Source={ruta};Pooling=False"); db.Open();
                using var consulta = db.CreateCommand(); consulta.CommandText = "SELECT hex(Hash) FROM Invitado";
                using var lector = consulta.ExecuteReader();
                while (lector.Read()) { Assert.NotEqual(a.Secreto, lector.GetString(0)); Assert.NotEqual(b.Secreto, lector.GetString(0)); }
            }
        }
        finally { File.Delete(ruta); }
    }
}
