namespace Proyecto_Integrador_Final.Api;

// La presencia es una entrada: presentar nunca modifica el dominio ni renueva sesiones.
internal sealed class VistaPartida
{
    internal PresentacionVisible Construir(EstadoPartida partida, string rol, bool ambosPresentes)
    {
        bool pausada = partida.Fase is "esperandoElector" or "esperandoRival" && !ambosPresentes;
        var cartas = partida.Cartas.OrderBy(c => c.Orden).ToArray();
        var mano = cartas.Where(c => c.Ubicacion == rol).Select(Carta).ToArray();
        SolicitudVisible? visible = null;
        if (!pausada && partida.IdSolicitud is Guid idSolicitud)
        {
            var solicitud = partida.Solicitudes.Single(s => s.Id == idSolicitud);
            if (solicitud.Rol == rol)
                visible = new(solicitud.Id, solicitud.Tipo, mano.Select(c => c.IdPersonaje).ToArray(),
                    solicitud.Tipo == "cartaYAtributo" ? Partida.AtributosPermitidos : []);
        }

        var historial = partida.Combates.Where(c => c.Ganador != null)
            .OrderBy(c => c.NumeroRonda).ThenBy(c => c.Numero).ToArray();
        var ganadores = partida.Rondas.Where(r => r.Ganador != null)
            .ToDictionary(r => r.Numero, r => r.Ganador);
        var ultimos = historial.GroupBy(c => c.NumeroRonda).ToDictionary(g => g.Key, g => g.Last().Numero);
        var cartasPorId = cartas.ToDictionary(c => c.IdPersonaje);
        var resultados = historial.Select(c => new ResultadoVisible(
            c.NumeroRonda, c.Numero,
            new()
            {
                [c.Elector] = Carta(cartasPorId[c.IdCartaElector!.Value]),
                [Otro(c.Elector)] = Carta(cartasPorId[c.IdCartaRival!.Value])
            },
            c.Atributo!,
            new()
            {
                [c.Elector] = c.PuntajeElector!.Value,
                [Otro(c.Elector)] = c.PuntajeRival!.Value
            },
            c.Ganador!,
            ultimos[c.NumeroRonda] == c.Numero ? ganadores.GetValueOrDefault(c.NumeroRonda) : null)).ToArray();

        return new(partida.Id, partida.Revision, partida.Fase, pausada, rol,
            partida.Participantes.ToDictionary(p => p.Rol, p => p.Nombre), partida.NumeroRonda,
            partida.NumeroCombate, partida.Participantes.ToDictionary(p => p.Rol, p => p.RondasGanadas),
            mano, visible, resultados, partida.MotivoFin, partida.Ganador);
    }

    private static string Otro(string rol) => rol == "J1" ? "J2" : "J1";

    private static CartaVisible Carta(CartaPartida carta) => new(carta.IdPersonaje, carta.Nombre,
        carta.Descripcion, carta.Ascii, new()
        {
            ["Fuerza"] = carta.Fuerza, ["Velocidad"] = carta.Velocidad,
            ["Resistencia"] = carta.Resistencia, ["Inteligencia"] = carta.Inteligencia,
            ["Energía"] = carta.Energia, ["Habilidad"] = carta.Habilidad
        });
}
