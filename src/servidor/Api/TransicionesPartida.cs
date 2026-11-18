namespace Proyecto_Integrador_Final.Api;

internal sealed partial class RepositorioJuego
{
    private static readonly Dictionary<string, TipoAtributo> atributos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fuerza"] = TipoAtributo.Fuerza,
        ["Velocidad"] = TipoAtributo.Velocidad,
        ["Resistencia"] = TipoAtributo.Resistencia,
        ["Inteligencia"] = TipoAtributo.Inteligencia,
        ["Energía"] = TipoAtributo.Energia,
        ["Habilidad"] = TipoAtributo.Habilidad
    };

    internal void Iniciar(Guid id) => Ejecutar(true, db =>
    {
        var partida = Buscar(db, id);
        if (partida.Fase != "esperandoInicio")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya se inició o finalizó.");
        if (!AmbosPresentes(id))
            throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");

        // Cada partida conserva su propia copia de los personajes activos al iniciar.
        var cartas = db.Catalogo.Where(p => p.Activo).ToArray()
            .Select(p => PersonajePartida.Copiar(id, p)).ToArray();
        if (cartas.Length < 6)
            throw new ErrorApi(409, "operacionIncompatible", "Se necesitan seis personajes activos.");
        db.Cartas.AddRange(cartas);
        Repartir(db, partida, cartas, 1, "J1");
        partida.Revision++;
        return true;
    });

    internal Confirmacion Elegir(Guid id, string? credencial, Guid idSesion, Eleccion datos)
    {
        if (datos.IdOperacion == Guid.Empty || datos.IdSolicitud == Guid.Empty ||
            datos.RevisionEsperada is null or < 0 || datos.IdPersonaje is null)
            throw ErrorApi.Entrada();

        datos = datos with { Atributo = NormalizarAtributo(datos.Atributo) };
        return Ejecutar(true, db =>
        {
            var (partida, jugador) = Acceso(db, id, credencial, idSesion);
            // Un reintento idéntico recupera su recibo aunque la revisión o el turno hayan cambiado.
            var anterior = db.Operaciones.Find(id, jugador.Rol, datos.IdOperacion);
            if (anterior is not null)
            {
                if (anterior.Datos() != datos)
                    throw new ErrorApi(409, "operacionIncompatible", "La operación ya tiene otros datos.");
                return new Confirmacion(anterior.Recibo(), true);
            }

            if (Pausada(partida))
                throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");
            if (partida.Revision != datos.RevisionEsperada)
                throw new ErrorApi(409, "revisionDesactualizada", "La revisión ya cambió.");
            var solicitud = partida.IdSolicitud == datos.IdSolicitud
                ? db.Solicitudes.Find(datos.IdSolicitud) : null;
            if (solicitud is null || solicitud.Consumida || solicitud.Rol != jugador.Rol)
                throw new ErrorApi(409, "solicitudNoVigente", "La solicitud no está vigente para esta plaza.");

            var carta = db.Cartas.Find(id, datos.IdPersonaje.Value);
            bool esElector = solicitud.Tipo == "cartaYAtributo";
            if (carta is null || carta.Ubicacion != jugador.Rol ||
                (esElector ? datos.Atributo is null || !atributos.ContainsKey(datos.Atributo) : datos.Atributo is not null))
                throw new ErrorApi(422, "eleccionInvalida", "La carta o el atributo no está entre las opciones.");

            var combate = db.Combates.Find(id, solicitud.NumeroRonda, solicitud.NumeroCombate)!;
            solicitud.Consumida = true;
            partida.Revision++;
            if (esElector)
            {
                combate.IdCartaElector = carta.IdPersonaje;
                combate.Atributo = datos.Atributo;
                AbrirSolicitud(db, partida, combate, Otro(jugador.Rol), "carta");
            }
            else
            {
                Resolver(db, partida, combate, carta);
            }

            // El recibo se guarda en la misma transacción que la jugada que confirma.
            var operacion = new OperacionGuardada
            {
                IdPartida = id, Rol = jugador.Rol, IdOperacion = datos.IdOperacion,
                IdSolicitud = datos.IdSolicitud, RevisionEsperada = datos.RevisionEsperada.Value,
                IdPersonaje = carta.IdPersonaje, Atributo = datos.Atributo,
                RevisionConfirmada = partida.Revision
            };
            db.Operaciones.Add(operacion);
            return new Confirmacion(operacion.Recibo(), false);
        });
    }

    internal Recibo ConsultarRecibo(Guid id, string? credencial, Guid idSesion, Guid idOperacion) =>
        Ejecutar(false, db =>
        {
            var (_, jugador) = Acceso(db, id, credencial, idSesion);
            return db.Operaciones.Find(id, jugador.Rol, idOperacion)?.Recibo()
                ?? throw ErrorApi.NoEncontrado();
        });

    internal void Abandonar(Guid id, string? credencial, Guid idSesion) => Ejecutar(true, db =>
    {
        var (partida, jugador) = Acceso(db, id, credencial, idSesion);
        if (partida.AbandonoDe == jugador.Rol)
            return true;
        if (partida.Fase == "finalizada")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.");
        partida.AbandonoDe = jugador.Rol;
        partida.Revision++;
        Finalizar(db, partida, "abandono");
        return true;
    });

    private (PartidaGuardada Partida, ParticipanteGuardado Jugador) Acceso(
        BaseJuego db, Guid id, string? credencial, Guid idSesion)
    {
        var partida = Buscar(db, id);
        var jugador = Autenticar(db, id, credencial);
        ExigirSesion(id, jugador.Rol, idSesion);
        return (partida, jugador);
    }

    private bool AmbosPresentes(Guid id) => Vigente(id, "J1") is not null && Vigente(id, "J2") is not null;

    private bool Pausada(PartidaGuardada partida) =>
        partida.Fase is "esperandoElector" or "esperandoRival" && !AmbosPresentes(partida.Id);

    private static string Otro(string rol) => rol == "J1" ? "J2" : "J1";

    private static string? NormalizarAtributo(string? atributo)
    {
        if (atributo is null)
            return null;
        var texto = atributo.Trim().Normalize();
        if (texto.Equals("Energia", StringComparison.OrdinalIgnoreCase))
            return "Energía";
        return atributos.Keys.FirstOrDefault(a => a.Equals(texto, StringComparison.OrdinalIgnoreCase)) ?? texto;
    }

    private static void Repartir(BaseJuego db, PartidaGuardada partida, PersonajePartida[] cartas,
        int numero, string mano)
    {
        // Al empezar cada ronda se mezclan todas las cartas y se reparten tres por jugador.
        Random.Shared.Shuffle(cartas);
        for (int i = 0; i < cartas.Length; i++)
        {
            cartas[i].Orden = i;
            cartas[i].Ubicacion = i < 3 ? "J1" : i < 6 ? "J2" : "mazo";
        }
        db.Rondas.Add(new RondaGuardada { IdPartida = partida.Id, Numero = numero, Mano = mano });
        partida.NumeroRonda = numero;
        AbrirCombate(db, partida, 1, mano);
    }

    private static void AbrirCombate(BaseJuego db, PartidaGuardada partida, int numero, string elector)
    {
        var combate = new CombateGuardado
        {
            IdPartida = partida.Id, NumeroRonda = partida.NumeroRonda!.Value,
            Numero = numero, Elector = elector
        };
        db.Combates.Add(combate);
        partida.NumeroCombate = numero;
        AbrirSolicitud(db, partida, combate, elector, "cartaYAtributo");
    }

    private static void AbrirSolicitud(BaseJuego db, PartidaGuardada partida, CombateGuardado combate,
        string rol, string tipo)
    {
        var solicitud = new SolicitudGuardada
        {
            Id = Guid.NewGuid(), IdPartida = partida.Id, NumeroRonda = combate.NumeroRonda,
            NumeroCombate = combate.Numero, Rol = rol, Tipo = tipo
        };
        db.Solicitudes.Add(solicitud);
        partida.IdSolicitud = solicitud.Id;
        partida.Fase = tipo == "carta" ? "esperandoRival" : "esperandoElector";
    }

    private static void Resolver(BaseJuego db, PartidaGuardada partida, CombateGuardado guardado,
        PersonajePartida cartaRival)
    {
        // La API reconstruye las dos jugadas para aplicar las reglas de Combate.
        var cartaElector = db.Cartas.Find(partida.Id, guardado.IdCartaElector!.Value)!;
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

        var ronda = db.Rondas.Find(partida.Id, guardado.NumeroRonda)!;
        var victorias = ganador == "J1" ? ++ronda.VictoriasJ1 : ++ronda.VictoriasJ2;
        if (victorias < Ronda.CombatesParaGanar)
        {
            AbrirCombate(db, partida, guardado.Numero + 1, Otro(guardado.Elector));
            return;
        }

        ronda.Ganador = ganador;
        var jugador = db.Participantes.Find(partida.Id, ganador)!;
        if (++jugador.RondasGanadas >= Partida.RondasParaGanar)
        {
            Finalizar(db, partida, "victoria", ganador);
            return;
        }

        var cartas = db.Cartas.Where(p => p.IdPartida == partida.Id).ToArray();
        Repartir(db, partida, cartas, ronda.Numero + 1, Otro(ronda.Mano));
    }

    private static void Finalizar(BaseJuego db, PartidaGuardada partida, string motivo, string? ganador = null)
    {
        if (partida.IdSolicitud is Guid id)
            db.Solicitudes.Find(id)!.Consumida = true;
        partida.IdSolicitud = null;
        partida.Fase = "finalizada";
        partida.MotivoFin = motivo;
        partida.Ganador = ganador;
    }
}
