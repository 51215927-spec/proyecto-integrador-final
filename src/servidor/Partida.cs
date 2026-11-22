namespace Proyecto_Integrador_Final;

internal sealed class Partida(EstadoPartida estado)
{
    internal const int RondasParaGanar = 15;
    internal EstadoPartida Estado { get; } = estado ?? throw new ArgumentNullException(nameof(estado));
    private static readonly Dictionary<string, TipoAtributo> atributos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fuerza"] = TipoAtributo.Fuerza, ["Velocidad"] = TipoAtributo.Velocidad,
        ["Resistencia"] = TipoAtributo.Resistencia, ["Inteligencia"] = TipoAtributo.Inteligencia,
        ["Energía"] = TipoAtributo.Energia, ["Habilidad"] = TipoAtributo.Habilidad
    };
    internal static string[] AtributosPermitidos => atributos.Keys.ToArray();

    internal void Iniciar(IEnumerable<CartaPartida> cartas)
    {
        if (Estado.Fase != "esperandoInicio")
            throw new ExcepcionPartida("operacionIncompatible", "La partida ya se inició o finalizó.");
        var copia = cartas.ToArray();
        if (copia.Length < 6)
            throw new ExcepcionPartida("operacionIncompatible", "Se necesitan seis personajes activos.");
        Estado.Cartas.AddRange(copia);
        Repartir(1, "J1");
        Estado.Revision++;
    }

    internal void RegistrarEleccion(string rol, Guid idSolicitud, int revision, int idCarta, string? atributo)
    {
        if (Estado.Revision != revision)
            throw new ExcepcionPartida("revisionDesactualizada", "La revisión ya cambió.");
        var solicitud = Estado.IdSolicitud == idSolicitud ? Estado.Solicitudes.SingleOrDefault(s => s.Id == idSolicitud) : null;
        if (solicitud is null || solicitud.Consumida || solicitud.Rol != rol)
            throw new ExcepcionPartida("solicitudNoVigente", "La solicitud no está vigente para esta plaza.");
        var carta = Estado.Cartas.SingleOrDefault(c => c.IdPersonaje == idCarta);
        var esElector = solicitud.Tipo == "cartaYAtributo";
        atributo = NormalizarAtributo(atributo);
        if (carta is null || carta.Ubicacion != rol ||
            (esElector ? atributo is null || !atributos.ContainsKey(atributo) : atributo is not null))
            throw new ExcepcionPartida("eleccionInvalida", "La carta o el atributo no está entre las opciones.");
        var combate = Estado.Combates.Single(c => c.NumeroRonda == solicitud.NumeroRonda && c.Numero == solicitud.NumeroCombate);
        solicitud.Consumida = true;
        Estado.Revision++;
        if (esElector)
        {
            combate.IdCartaElector = idCarta;
            combate.Atributo = atributo;
            AbrirSolicitud(combate, Otro(rol), "carta");
        }
        else Resolver(combate, carta);
    }

    internal void Abandonar(string rol)
    {
        if (Estado.AbandonoDe == rol) return;
        ExigirAbierta();
        Estado.AbandonoDe = rol;
        Estado.Revision++;
        Finalizar("abandono");
    }

    internal void Cancelar()
    {
        ExigirAbierta();
        Estado.Revision++;
        Finalizar("cancelacion");
    }

    private void ExigirAbierta()
    {
        if (Estado.Fase == "finalizada")
            throw new ExcepcionPartida("operacionIncompatible", "La partida ya finalizó.");
    }

    internal static string? NormalizarAtributo(string? atributo)
    {
        if (atributo is null) return null;
        var texto = atributo.Trim().Normalize();
        if (texto.Equals("Energia", StringComparison.OrdinalIgnoreCase)) return "Energía";
        return atributos.Keys.FirstOrDefault(a => a.Equals(texto, StringComparison.OrdinalIgnoreCase)) ?? texto;
    }

    private static string Otro(string rol) => rol == "J1" ? "J2" : "J1";

    private void Repartir(int numero, string mano)
    {
        var cartas = Estado.Cartas.ToArray();
        // Mazo hace la mezcla y el reparto; las cartas conservan su identidad en el estado.
        var personajes = cartas.Select(c => c.Dominio()).ToList();
        var correspondencia = personajes.Select((p, i) => (p, carta: cartas[i])).ToDictionary(x => x.p, x => x.carta);
        var mazo = new Mazo(personajes);
        mazo.Mezclar();
        var ordenadas = mazo.Repartir(3).Concat(mazo.Repartir(3)).Concat(mazo.Repartir(mazo.GetCantidad())).ToArray();
        for (int i = 0; i < ordenadas.Length; i++)
        {
            var carta = correspondencia[ordenadas[i]];
            carta.Orden = i;
            carta.Ubicacion = i < 3 ? "J1" : i < 6 ? "J2" : "mazo";
        }
        Estado.Rondas.Add(new RondaPartida { Numero = numero, Mano = mano });
        Estado.NumeroRonda = numero;
        AbrirCombate(1, mano);
    }

    private void AbrirCombate(int numero, string elector)
    {
        var combate = new CombatePartida { NumeroRonda = Estado.NumeroRonda!.Value, Numero = numero, Elector = elector };
        Estado.Combates.Add(combate);
        Estado.NumeroCombate = numero;
        AbrirSolicitud(combate, elector, "cartaYAtributo");
    }

    private void AbrirSolicitud(CombatePartida combate, string rol, string tipo)
    {
        var solicitud = new SolicitudPartida { Id = Guid.NewGuid(), NumeroRonda = combate.NumeroRonda,
            NumeroCombate = combate.Numero, Rol = rol, Tipo = tipo };
        Estado.Solicitudes.Add(solicitud);
        Estado.IdSolicitud = solicitud.Id;
        Estado.Fase = tipo == "carta" ? "esperandoRival" : "esperandoElector";
    }

    private void Resolver(CombatePartida guardado, CartaPartida cartaRival)
    {
        var cartaElector = Estado.Cartas.Single(c => c.IdPersonaje == guardado.IdCartaElector);
        var elector = new Jugador(guardado.Elector);
        var rival = new Jugador(Otro(guardado.Elector));
        var personajeElector = cartaElector.Dominio();
        var personajeRival = cartaRival.Dominio();
        elector.RecibirCartas([personajeElector]);
        rival.RecibirCartas([personajeRival]);
        var atributo = atributos[guardado.Atributo!];
        var combate = new Combate(elector, rival);
        combate.SetAtributo(atributo);
        combate.SetCarta(elector, personajeElector);
        combate.SetCarta(rival, personajeRival);
        var ganador = combate.Resolver().GetNombre();
        guardado.IdCartaRival = cartaRival.IdPersonaje;
        guardado.PuntajeElector = personajeElector.GetAtributo(atributo);
        guardado.PuntajeRival = personajeRival.GetAtributo(atributo);
        guardado.Ganador = ganador;
        cartaElector.Ubicacion = cartaRival.Ubicacion = "jugada";
        var ronda = Estado.Rondas.Single(r => r.Numero == guardado.NumeroRonda);
        if (ganador == "J1") ronda.VictoriasJ1++;
        else ronda.VictoriasJ2++;
        var vencedor = Ronda.GanadorPorVictorias(ronda.VictoriasJ1, ronda.VictoriasJ2);
        if (vencedor is null)
        {
            AbrirCombate(guardado.Numero + 1, Ronda.EsTurnoMano(guardado.Numero) ? ronda.Mano : Otro(ronda.Mano));
            return;
        }
        ganador = vencedor == 1 ? "J1" : "J2";
        ronda.Ganador = ganador;
        var jugador = Estado.Participantes.Single(p => p.Rol == ganador);
        if (++jugador.RondasGanadas >= RondasParaGanar) Finalizar("victoria", ganador);
        else Repartir(ronda.Numero + 1, Otro(ronda.Mano));
    }

    private void Finalizar(string motivo, string? ganador = null)
    {
        if (Estado.IdSolicitud is Guid id) Estado.Solicitudes.Single(s => s.Id == id).Consumida = true;
        Estado.IdSolicitud = null;
        Estado.Fase = "finalizada";
        Estado.MotivoFin = motivo;
        Estado.Ganador = ganador;
    }
}
