namespace Proyecto_Integrador_Final.Cliente;

internal sealed class EstadoCliente(Guid partida)
{
    // Los combates conocidos evitan repetir resultados después de una reconexión.
    private readonly HashSet<(int Ronda, int Combate)> conocidos = [];
    internal PresentacionCliente? Vista { get; private set; }
    internal bool Actual { get; private set; }
    internal Queue<ResultadoCliente> Resultados { get; } = new();
    internal void PerderComunicacion() => Actual = false;

    internal void Aplicar(PresentacionCliente vista)
    {
        // La vista se valida completa antes de sustituir el último estado confirmado.
        try { Validar(vista); }
        catch (Exception ex) when (ex is NullReferenceException or KeyNotFoundException or ArgumentException)
        { throw new FalloCliente("protocoloIncompatible"); }
        // La primera vista registra la historia sin obligar a reproducirla en pantalla.
        bool inicial = Vista is null;
        foreach (var resultado in vista.Resultados.OrderBy(r => r.NumeroRonda).ThenBy(r => r.NumeroCombate))
            if (conocidos.Add((resultado.NumeroRonda, resultado.NumeroCombate)) && !inicial)
                Resultados.Enqueue(resultado);
        Vista = vista;
        Actual = true;
    }

    private void Validar(PresentacionCliente vista)
    {
        // Las comprobaciones impiden habilitar acciones sobre un contrato inconsistente.
        static bool Rol(string? v) => v is "J1" or "J2";
        static void Exigir(bool condicion) { if (!condicion) throw new FalloCliente("protocoloIncompatible"); }
        Exigir(vista.IdPartida == partida && vista.Revision >= 0 && Rol(vista.RolPropio));
        Exigir(vista.Fase is "esperandoInicio" or "esperandoElector" or "esperandoRival" or "finalizada");
        Exigir(vista.Nombres.ContainsKey("J1") && vista.Nombres.ContainsKey("J2") &&
            vista.Marcador.ContainsKey("J1") && vista.Marcador.ContainsKey("J2"));
        Exigir(vista.Mano is not null && vista.Resultados is not null);
        Exigir(vista.Mano!.All(c => c.IdPersonaje > 0 && c.Nombre is not null && c.Atributos is not null));
        Exigir(vista.Mano!.Select(c => c.IdPersonaje).Distinct().Count() == vista.Mano!.Length);
        // Un cierre no admite una solicitud nueva y una victoria debe identificar al ganador.
        if (vista.Fase == "finalizada")
            Exigir(vista.Solicitud is null && vista.MotivoFin is "victoria" or "abandono" or "cancelacion" &&
                (vista.MotivoFin != "victoria" || Rol(vista.Ganador)));
        // Las opciones recibidas deben pertenecer a la mano y a sus atributos visibles.
        if (vista.Solicitud is { } s)
        {
            Exigir(!vista.Pausada && vista.Fase is "esperandoElector" or "esperandoRival");
            Exigir(s.IdSolicitud != Guid.Empty && s.Tipo is "carta" or "cartaYAtributo");
            Exigir(s.IdsPersonajePermitidos.Length > 0 && s.IdsPersonajePermitidos.All(id => vista.Mano.Any(c => c.IdPersonaje == id)));
            Exigir(s.Tipo == "carta" ? s.AtributosPermitidos.Length == 0 : s.AtributosPermitidos.Length > 0 &&
                s.AtributosPermitidos.All(a => vista.Mano.Where(c => s.IdsPersonajePermitidos.Contains(c.IdPersonaje)).All(c => c.Atributos.ContainsKey(a))));
        }
        // Cada resultado necesita datos de ambos participantes antes de poder dibujarse.
        foreach (var r in vista.Resultados!)
            Exigir(r.NumeroRonda > 0 && r.NumeroCombate > 0 && Rol(r.GanadorCombate) &&
                (r.GanadorRonda is null || Rol(r.GanadorRonda)) && r.Cartas.ContainsKey("J1") && r.Cartas.ContainsKey("J2") &&
                r.Puntajes.ContainsKey("J1") && r.Puntajes.ContainsKey("J2") && r.Atributo is not null);
    }
}
