using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

internal sealed record PartidaCreada(Guid Id, string CredencialJ1, string CredencialJ2);
internal sealed record Incorporacion(Guid IdSesion, string? Nombre);
internal sealed record Sesion(string Rol, Guid IdSesion, DateTimeOffset VenceUtc);

internal sealed partial class RepositorioJuego : IDisposable
{
    private readonly string cadena;
    private readonly SqliteConnection? memoria;
    private readonly TimeProvider reloj;
    private readonly object sincronizacion = new();
    private readonly Dictionary<(Guid, string), Sesion> sesiones = [];

    internal RepositorioJuego(string cadena, TimeProvider reloj)
    {
        this.cadena = cadena;
        this.reloj = reloj;
        if (new SqliteConnectionStringBuilder(cadena).DataSource == ":memory:")
        {
            memoria = new SqliteConnection(cadena);
            memoria.Open();
        }

        using var db = Abrir();
        db.Database.EnsureCreated();
    }

    internal int DarDeAlta(AltaPersonaje datos)
    {
        var personaje = datos.Validar();
        return Ejecutar(true, db =>
        {
            int mayor = db.Catalogo.Where(p => p.Id > 0).Select(p => (int?)p.Id).Max() ?? 0;
            if (mayor == int.MaxValue)
                throw new ErrorApi(409, "operacionIncompatible", "Se agotaron los IDs de alta automática.");
            personaje.Id = mayor + 1;
            db.Catalogo.Add(personaje);
            return personaje.Id;
        });
    }

    internal PartidaCreada CrearPartida()
    {
        var id = Guid.NewGuid();
        var j1 = NuevaCredencial();
        var j2 = NuevaCredencial();
        return Ejecutar(true, db =>
        {
            db.Partidas.Add(new PartidaGuardada { Id = id });
            db.Participantes.AddRange(
                new ParticipanteGuardado { IdPartida = id, Rol = "J1", Hash = Hash(j1) },
                new ParticipanteGuardado { IdPartida = id, Rol = "J2", Hash = Hash(j2) });
            return new PartidaCreada(id, j1, j2);
        });
    }


    internal int Importar(string ruta)
    {
        var lote = FilaCatalogo.Leer(ruta);
        return Ejecutar(true, db =>
        {
            foreach (var personaje in lote)
            {
                var anterior = db.Catalogo.Find(personaje.Id);
                if (anterior is null)
                    db.Catalogo.Add(personaje);
                else
                {
                    personaje.Activo = anterior.Activo;
                    db.Entry(anterior).CurrentValues.SetValues(personaje);
                }
            }
            return lote.Length;
        });
    }

    internal void Desactivar(int id) => Ejecutar(true, db =>
    {
        var personaje = db.Catalogo.Find(id) ?? throw ErrorApi.NoEncontrado();
        personaje.Activo = false;
        return true;
    });

    internal void CerrarSesion(Guid id, string rol)
    {
        lock (sincronizacion)
        {
            Ejecutar(false, db => BuscarParticipante(db, id, rol));
            sesiones.Remove((id, rol));
        }
    }

    internal string ReemplazarCredencial(Guid id, string rol)
    {
        lock (sincronizacion)
        {
            var nueva = NuevaCredencial();
            Ejecutar(true, db => BuscarParticipante(db, id, rol).Hash = Hash(nueva));
            sesiones.Remove((id, rol));
            return nueva;
        }
    }

    internal void Cancelar(Guid id) => Ejecutar(true, db =>
    {
        var partida = Buscar(db, id);
        if (partida.Fase == "finalizada")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.");
        partida.Revision++;
        Finalizar(db, partida, "cancelacion");
        return true;
    });

    private static ParticipanteGuardado BuscarParticipante(BaseJuego db, Guid id, string rol) =>
        db.Participantes.Find(id, rol) ?? throw ErrorApi.NoEncontrado();

    internal Sesion Incorporar(Guid id, string? credencial, Incorporacion datos)
    {
        if (datos.IdSesion == Guid.Empty || string.IsNullOrWhiteSpace(datos.Nombre))
            throw ErrorApi.Entrada();

        lock (sincronizacion)
        {
            var sesion = Ejecutar(true, db =>
            {
                Buscar(db, id);
                var participante = Autenticar(db, id, credencial);
                var actual = Vigente(id, participante.Rol);
                if (actual is not null && actual.IdSesion != datos.IdSesion)
                    throw new ErrorApi(409, "sesionOcupada", "La plaza tiene una sesión vigente.");
                if (participante.Nombre is not null && participante.Nombre != datos.Nombre)
                    throw new ErrorApi(403, "sinPermiso", "El nombre registrado debe coincidir.");

                participante.Nombre ??= datos.Nombre;
                return new Sesion(participante.Rol, datos.IdSesion, reloj.GetUtcNow().AddSeconds(15));
            });
            sesiones[(id, sesion.Rol)] = sesion;
            return sesion;
        }
    }

    internal void Renovar(Guid id, string? credencial, Guid idSesion)
    {
        lock (sincronizacion)
        {
            var sesion = Ejecutar(false, db =>
            {
                Buscar(db, id);
                var jugador = Autenticar(db, id, credencial);
                return ExigirSesion(id, jugador.Rol, idSesion);
            });
            sesiones[(id, sesion.Rol)] = sesion with { VenceUtc = reloj.GetUtcNow().AddSeconds(15) };
        }
    }

    private T Ejecutar<T>(bool escritura, Func<BaseJuego, T> operacion)
    {
        lock (sincronizacion)
        {
            using var db = Abrir();
            db.Database.OpenConnection();
            using var transaccion = ((SqliteConnection)db.Database.GetDbConnection())
                .BeginTransaction(deferred: !escritura);
            db.Database.UseTransaction(transaccion);
            var resultado = operacion(db);
            if (escritura)
                db.SaveChanges();
            transaccion.Commit();
            return resultado;
        }
    }

    private BaseJuego Abrir()
    {
        var opciones = new DbContextOptionsBuilder<BaseJuego>();
        if (memoria is null)
            opciones.UseSqlite(cadena);
        else
            opciones.UseSqlite(memoria);

        return new BaseJuego(opciones.Options);
    }

    private static PartidaGuardada Buscar(BaseJuego db, Guid id) =>
        db.Partidas.Find(id) ?? throw ErrorApi.NoEncontrado();

    private static ParticipanteGuardado Autenticar(BaseJuego db, Guid id, string? credencial)
    {
        if (!string.IsNullOrWhiteSpace(credencial))
        {
            var hash = Hash(credencial);
            foreach (var jugador in db.Participantes.Where(p => p.IdPartida == id))
            {
                if (CryptographicOperations.FixedTimeEquals(jugador.Hash, hash))
                    return jugador;
            }
        }

        throw new ErrorApi(401, "credencialInvalida", "La credencial no es válida.");
    }

    private Sesion? Vigente(Guid id, string rol) =>
        sesiones.TryGetValue((id, rol), out var sesion) && sesion.VenceUtc > reloj.GetUtcNow()
            ? sesion : null;

    private Sesion ExigirSesion(Guid id, string rol, Guid idSesion) =>
        Vigente(id, rol) is { } sesion && sesion.IdSesion == idSesion
            ? sesion : throw new ErrorApi(409, "sesionVencida", "Se requiere una sesión vigente.");

    private static byte[] Hash(string credencial) => SHA256.HashData(Encoding.UTF8.GetBytes(credencial));
    private static string NuevaCredencial() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    public void Dispose() => memoria?.Dispose();
}
