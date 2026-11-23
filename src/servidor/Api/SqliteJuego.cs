using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

// Infraestructura compartida: ninguna regla de partidas, catálogo o sesiones.
internal sealed class SqliteJuego : IDisposable
{
    private readonly string cadena;
    private readonly SqliteConnection? memoria;
    internal object Sincronizacion { get; } = new();

    internal SqliteJuego(string cadena)
    {
        this.cadena = cadena;
        if (new SqliteConnectionStringBuilder(cadena).DataSource == ":memory:")
        {
            memoria = new SqliteConnection(cadena);
            memoria.Open();
        }
        using var db = Abrir();
        db.Database.EnsureCreated();
        // EnsureCreated no evoluciona bases anteriores. Solo añadimos tablas, sin tocar las existentes.
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Invitado (Id TEXT NOT NULL PRIMARY KEY, Hash BLOB NOT NULL, Nombre TEXT NOT NULL)");
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS AsignacionInvitado (IdInvitado TEXT NOT NULL, IdPartida TEXT NOT NULL, Rol TEXT NOT NULL, PRIMARY KEY (IdInvitado, IdPartida), UNIQUE (IdPartida, Rol))");
    }

    internal T Ejecutar<T>(bool escritura, Func<BaseJuego, T> operacion)
    {
        lock (Sincronizacion)
        {
            using var db = Abrir();
            db.Database.OpenConnection();
            using var transaccion = ((SqliteConnection)db.Database.GetDbConnection())
                .BeginTransaction(deferred: !escritura);
            db.Database.UseTransaction(transaccion);
            var resultado = operacion(db);
            if (escritura) db.SaveChanges();
            transaccion.Commit();
            return resultado;
        }
    }

    private BaseJuego Abrir()
    {
        var opciones = new DbContextOptionsBuilder<BaseJuego>();
        if (memoria is null) opciones.UseSqlite(cadena);
        else opciones.UseSqlite(memoria);
        return new BaseJuego(opciones.Options);
    }

    public void Dispose() => memoria?.Dispose();
}
