using System.Text.Json.Serialization;

namespace Proyecto_Integrador_Final.Api;

internal sealed record SolicitudVisible(Guid IdSolicitud, string Tipo,
    int[] IdsPersonajePermitidos, string[] AtributosPermitidos);
internal sealed record ResultadoVisible(int NumeroRonda, int NumeroCombate,
    Dictionary<string, CartaVisible> Cartas, string Atributo, Dictionary<string, int> Puntajes,
    string GanadorCombate, string? GanadorRonda);
internal sealed record PresentacionVisible(Guid IdPartida, int Revision, string Fase, bool Pausada,
    string RolPropio, Dictionary<string, string?> Nombres, int? NumeroRonda, int? NumeroCombate,
    Dictionary<string, int> Marcador, CartaVisible[] Mano, SolicitudVisible? Solicitud,
    ResultadoVisible[] Resultados,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MotivoFin,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Ganador);

internal sealed partial class RepositorioJuego
{
    internal PresentacionVisible Presentacion(Guid id, string? credencial, Guid idSesion) =>
        Ejecutar(false, db =>
        {
            var (partida, jugador) = Acceso(db, id, credencial, idSesion);
            bool pausada = Pausada(partida);
            var participantes = db.Participantes.Where(p => p.IdPartida == id).ToArray();
            var cartas = db.Cartas.Where(c => c.IdPartida == id).OrderBy(c => c.Orden).ToArray();
            var mano = cartas.Where(c => c.Ubicacion == jugador.Rol).Select(c => c.Vista()).ToArray();

            // Solo se envían opciones a quien tiene el turno mientras ambos jugadores están presentes.
            SolicitudVisible? visible = null;
            if (!pausada && partida.IdSolicitud is Guid idSolicitud)
            {
                var solicitud = db.Solicitudes.Find(idSolicitud)!;
                if (solicitud.Rol == jugador.Rol)
                    visible = new(solicitud.Id, solicitud.Tipo, mano.Select(c => c.IdPersonaje).ToArray(),
                        solicitud.Tipo == "cartaYAtributo" ? atributos.Keys.ToArray() : []);
            }

            // Las cartas de cada combate se muestran una vez que se conoce su ganador.
            var historial = db.Combates.Where(c => c.IdPartida == id && c.Ganador != null)
                .OrderBy(c => c.NumeroRonda).ThenBy(c => c.Numero).ToArray();
            var ganadores = db.Rondas.Where(r => r.IdPartida == id && r.Ganador != null)
                .ToDictionary(r => r.Numero, r => r.Ganador);
            var ultimos = historial.GroupBy(c => c.NumeroRonda).ToDictionary(g => g.Key, g => g.Last().Numero);
            var cartasPorId = cartas.ToDictionary(c => c.IdPersonaje);
            var resultados = historial.Select(c => new ResultadoVisible(
                c.NumeroRonda, c.Numero,
                new()
                {
                    [c.Elector] = cartasPorId[c.IdCartaElector!.Value].Vista(),
                    [Otro(c.Elector)] = cartasPorId[c.IdCartaRival!.Value].Vista()
                },
                c.Atributo!,
                new()
                {
                    [c.Elector] = c.PuntajeElector!.Value,
                    [Otro(c.Elector)] = c.PuntajeRival!.Value
                },
                c.Ganador!,
                ultimos[c.NumeroRonda] == c.Numero ? ganadores.GetValueOrDefault(c.NumeroRonda) : null))
                .ToArray();

            return new PresentacionVisible(id, partida.Revision, partida.Fase, pausada, jugador.Rol,
                participantes.ToDictionary(p => p.Rol, p => p.Nombre), partida.NumeroRonda,
                partida.NumeroCombate, participantes.ToDictionary(p => p.Rol, p => p.RondasGanadas),
                mano, visible, resultados, partida.MotivoFin, partida.Ganador);
        });
}
