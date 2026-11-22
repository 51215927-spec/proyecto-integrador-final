using System.Security.Cryptography;
using System.Text;

namespace Proyecto_Integrador_Final.Api;

internal sealed record PartidaCreada(Guid Id, string CredencialJ1, string CredencialJ2);

internal sealed class AlmacenPartidas : IDisposable
{
    internal SqliteJuego Infraestructura { get; }
    internal AlmacenPartidas(string cadena) : this(new SqliteJuego(cadena)) { }
    internal AlmacenPartidas(SqliteJuego infraestructura) => Infraestructura = infraestructura;

    internal PartidaCreada CrearPartida()
    {
        var id = Guid.NewGuid();
        var j1 = NuevaCredencial();
        var j2 = NuevaCredencial();
        return Infraestructura.Ejecutar(true, db =>
        {
            db.Partidas.Add(new PartidaGuardada { Id = id });
            db.Participantes.AddRange(
                new ParticipanteGuardado { IdPartida = id, Rol = "J1", Hash = Hash(j1) },
                new ParticipanteGuardado { IdPartida = id, Rol = "J2", Hash = Hash(j2) });
            return new PartidaCreada(id, j1, j2);
        });
    }

    internal T Ejecutar<T>(Guid id, bool escritura, Func<UnidadPartida, T> accion) =>
        Infraestructura.Ejecutar(escritura, db =>
        {
            var guardada = Buscar(db, id);
            var participantes = db.Participantes.Where(p => p.IdPartida == id).ToArray();
            var estado = Cargar(db, guardada, participantes);
            var operaciones = db.Operaciones.Where(o => o.IdPartida == id).ToList();
            var unidad = new UnidadPartida(new Partida(estado), participantes,
                operaciones, catalogo => catalogo.CopiarActivos(db));
            var resultado = accion(unidad);
            if (escritura)
            {
                Guardar(db, guardada, participantes, estado);
                foreach (var operacion in unidad.NuevasOperaciones) db.Operaciones.Add(operacion);
            }
            return resultado;
        });

    internal string Autenticar(Guid id, string? credencial) => Infraestructura.Ejecutar(false, db =>
    {
        Buscar(db, id);
        return Autenticar(db.Participantes.Where(p => p.IdPartida == id), credencial).Rol;
    });

    internal string RegistrarNombre(Guid id, string? credencial, string nombre, Action<string> comprobar) =>
        Infraestructura.Ejecutar(true, db =>
        {
            Buscar(db, id);
            var jugador = Autenticar(db.Participantes.Where(p => p.IdPartida == id), credencial);
            comprobar(jugador.Rol);
            if (jugador.Nombre is not null && jugador.Nombre != nombre)
                throw new ErrorApi(403, "sinPermiso", "El nombre registrado debe coincidir.");
            jugador.Nombre ??= nombre;
            return jugador.Rol;
        });

    internal void ExigirParticipante(Guid id, string rol) => Infraestructura.Ejecutar(false,
        db => db.Participantes.Find(id, rol) ?? throw ErrorApi.NoEncontrado());

    internal string ReemplazarCredencial(Guid id, string rol)
    {
        var nueva = NuevaCredencial();
        Infraestructura.Ejecutar(true, db =>
        {
            var jugador = db.Participantes.Find(id, rol) ?? throw ErrorApi.NoEncontrado();
            jugador.Hash = Hash(nueva);
            return true;
        });
        return nueva;
    }

    private static PartidaGuardada Buscar(BaseJuego db, Guid id) =>
        db.Partidas.Find(id) ?? throw ErrorApi.NoEncontrado();

    internal static ParticipanteGuardado Autenticar(IEnumerable<ParticipanteGuardado> jugadores, string? credencial)
    {
        if (!string.IsNullOrWhiteSpace(credencial))
        {
            var hash = Hash(credencial);
            foreach (var jugador in jugadores)
                if (CryptographicOperations.FixedTimeEquals(jugador.Hash, hash)) return jugador;
        }
        throw new ErrorApi(401, "credencialInvalida", "La credencial no es válida.");
    }

    private static byte[] Hash(string credencial) => SHA256.HashData(Encoding.UTF8.GetBytes(credencial));
    private static string NuevaCredencial() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    public void Dispose() => Infraestructura.Dispose();

    private static EstadoPartida Cargar(BaseJuego db, PartidaGuardada p, ParticipanteGuardado[] participantes) => new()
    {
        Id = p.Id,
        Revision = p.Revision,
        NumeroRonda = p.NumeroRonda,
        NumeroCombate = p.NumeroCombate,
        IdSolicitud = p.IdSolicitud,
        MotivoFin = p.MotivoFin,
        Ganador = p.Ganador,
        AbandonoDe = p.AbandonoDe,
        Fase = p.Fase,
        Participantes = participantes.Select(p => new ParticipantePartida { Rol = p.Rol, Nombre = p.Nombre, RondasGanadas = p.RondasGanadas }).ToList(),
        Cartas = db.Cartas.Where(c => c.IdPartida == p.Id).ToArray().Select(c => new CartaPartida
        {
            IdPersonaje = c.IdPersonaje,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Ascii = c.Ascii,
            Fuerza = c.Fuerza,
            Velocidad = c.Velocidad,
            Resistencia = c.Resistencia,
            Inteligencia = c.Inteligencia,
            Energia = c.Energia,
            Habilidad = c.Habilidad,
            Ubicacion = c.Ubicacion,
            Orden = c.Orden,
        }).ToList(),
        Rondas = db.Rondas.Where(c => c.IdPartida == p.Id).ToArray().Select(c => new RondaPartida
        {
            Numero = c.Numero,
            Mano = c.Mano,
            VictoriasJ1 = c.VictoriasJ1,
            VictoriasJ2 = c.VictoriasJ2,
            Ganador = c.Ganador,
        }).ToList(),
        Combates = db.Combates.Where(c => c.IdPartida == p.Id).ToArray().Select(c => new CombatePartida
        {
            NumeroRonda = c.NumeroRonda,
            Numero = c.Numero,
            Elector = c.Elector,
            IdCartaElector = c.IdCartaElector,
            IdCartaRival = c.IdCartaRival,
            Atributo = c.Atributo,
            PuntajeElector = c.PuntajeElector,
            PuntajeRival = c.PuntajeRival,
            Ganador = c.Ganador,
        }).ToList(),
        Solicitudes = db.Solicitudes.Where(c => c.IdPartida == p.Id).ToArray().Select(c => new SolicitudPartida
        {
            Id = c.Id,
            NumeroRonda = c.NumeroRonda,
            NumeroCombate = c.NumeroCombate,
            Rol = c.Rol,
            Tipo = c.Tipo,
            Consumida = c.Consumida,
        }).ToList(),
    };

    private static void Guardar(BaseJuego db, PartidaGuardada p, ParticipanteGuardado[] participantes, EstadoPartida estado)
    {
        p.Id = estado.Id;
        p.Revision = estado.Revision;
        p.NumeroRonda = estado.NumeroRonda;
        p.NumeroCombate = estado.NumeroCombate;
        p.IdSolicitud = estado.IdSolicitud;
        p.MotivoFin = estado.MotivoFin;
        p.Ganador = estado.Ganador;
        p.AbandonoDe = estado.AbandonoDe;
        p.Fase = estado.Fase;
        foreach (var jugador in participantes)
        {
            var actual = estado.Participantes.Single(j => j.Rol == jugador.Rol);
            jugador.Nombre = actual.Nombre;
            jugador.RondasGanadas = actual.RondasGanadas;
        }
        foreach (var actual in estado.Cartas)
        {
            var fila = db.Cartas.Find(estado.Id, actual.IdPersonaje);
            if (fila is null)
            {
                fila = new PersonajePartida
                {
                    IdPartida = estado.Id,
                    IdPersonaje = actual.IdPersonaje,
                    Nombre = actual.Nombre,
                    Descripcion = actual.Descripcion,
                    Ascii = actual.Ascii,
                    Fuerza = actual.Fuerza,
                    Velocidad = actual.Velocidad,
                    Resistencia = actual.Resistencia,
                    Inteligencia = actual.Inteligencia,
                    Energia = actual.Energia,
                    Habilidad = actual.Habilidad,
                    Ubicacion = actual.Ubicacion,
                    Orden = actual.Orden,
                };
                db.Cartas.Add(fila);
            }
            else
            {
                fila.IdPersonaje = actual.IdPersonaje;
                fila.Nombre = actual.Nombre;
                fila.Descripcion = actual.Descripcion;
                fila.Ascii = actual.Ascii;
                fila.Fuerza = actual.Fuerza;
                fila.Velocidad = actual.Velocidad;
                fila.Resistencia = actual.Resistencia;
                fila.Inteligencia = actual.Inteligencia;
                fila.Energia = actual.Energia;
                fila.Habilidad = actual.Habilidad;
                fila.Ubicacion = actual.Ubicacion;
                fila.Orden = actual.Orden;
            }
        }
        foreach (var actual in estado.Rondas)
        {
            var fila = db.Rondas.Find(estado.Id, actual.Numero);
            if (fila is null)
            {
                fila = new RondaGuardada
                {
                    IdPartida = estado.Id,
                    Numero = actual.Numero,
                    Mano = actual.Mano,
                    VictoriasJ1 = actual.VictoriasJ1,
                    VictoriasJ2 = actual.VictoriasJ2,
                    Ganador = actual.Ganador,
                };
                db.Rondas.Add(fila);
            }
            else
            {
                fila.Numero = actual.Numero;
                fila.Mano = actual.Mano;
                fila.VictoriasJ1 = actual.VictoriasJ1;
                fila.VictoriasJ2 = actual.VictoriasJ2;
                fila.Ganador = actual.Ganador;
            }
        }
        foreach (var actual in estado.Combates)
        {
            var fila = db.Combates.Find(estado.Id, actual.NumeroRonda, actual.Numero);
            if (fila is null)
            {
                fila = new CombateGuardado
                {
                    IdPartida = estado.Id,
                    NumeroRonda = actual.NumeroRonda,
                    Numero = actual.Numero,
                    Elector = actual.Elector,
                    IdCartaElector = actual.IdCartaElector,
                    IdCartaRival = actual.IdCartaRival,
                    Atributo = actual.Atributo,
                    PuntajeElector = actual.PuntajeElector,
                    PuntajeRival = actual.PuntajeRival,
                    Ganador = actual.Ganador,
                };
                db.Combates.Add(fila);
            }
            else
            {
                fila.NumeroRonda = actual.NumeroRonda;
                fila.Numero = actual.Numero;
                fila.Elector = actual.Elector;
                fila.IdCartaElector = actual.IdCartaElector;
                fila.IdCartaRival = actual.IdCartaRival;
                fila.Atributo = actual.Atributo;
                fila.PuntajeElector = actual.PuntajeElector;
                fila.PuntajeRival = actual.PuntajeRival;
                fila.Ganador = actual.Ganador;
            }
        }
        foreach (var actual in estado.Solicitudes)
        {
            var fila = db.Solicitudes.Find(actual.Id);
            if (fila is null)
            {
                fila = new SolicitudGuardada
                {
                    IdPartida = estado.Id,
                    Id = actual.Id,
                    NumeroRonda = actual.NumeroRonda,
                    NumeroCombate = actual.NumeroCombate,
                    Rol = actual.Rol,
                    Tipo = actual.Tipo,
                    Consumida = actual.Consumida,
                };
                db.Solicitudes.Add(fila);
            }
            else
            {
                fila.Id = actual.Id;
                fila.NumeroRonda = actual.NumeroRonda;
                fila.NumeroCombate = actual.NumeroCombate;
                fila.Rol = actual.Rol;
                fila.Tipo = actual.Tipo;
                fila.Consumida = actual.Consumida;
            }
        }
    }

}
