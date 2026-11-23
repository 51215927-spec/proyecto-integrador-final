namespace Proyecto_Integrador_Final.Api;

internal sealed record EstadoAcceso(string Estado, string Nombre, string? Rival,
    string? Aviso, Guid? Partida);

// Solo coordina acceso y reservas. Todas las transiciones de juego pertenecen a Partida.
internal sealed class Emparejamiento(Invitados invitados, AlmacenPartidas almacen,
    AdministradorSesiones sesiones, CatalogoPersonajes catalogo, TimeProvider reloj, Avisos avisos)
{
    private sealed class Presencia(InvitadoGuardado invitado, Guid sesion, DateTimeOffset vence)
    {
        internal InvitadoGuardado Invitado = invitado;
        internal Guid Sesion = sesion;
        internal DateTimeOffset Vence = vence;
        internal bool Buscando;
        internal long Orden;
        internal Guid? Rival;
        internal string? Aviso;
    }
    private long orden;
    private readonly Dictionary<Guid, Presencia> presentes = [];
    private readonly LinkedList<Guid> cola = [];
    internal object Sincronizacion => sesiones.Sincronizacion;

    internal bool Disponible(Guid id, string? secreto)
    {
        lock (Sincronizacion)
        {
            invitados.Autenticar(id, secreto);
            Vencer();
            return !presentes.ContainsKey(id);
        }
    }

    internal EstadoAcceso Ejecutar(Guid id, string? secreto, Guid sesion, string accion)
    {
        lock (Sincronizacion)
        {
            var invitado = invitados.Autenticar(id, secreto);
            if (sesion == Guid.Empty) throw ErrorApi.Entrada();
            Vencer();
            presentes.TryGetValue(id, out var actual);
            if (actual is not null && actual.Sesion != sesion)
                throw new ErrorApi(409, "sesionOcupada", "Este invitado está abierto en otro cliente. Esperá su cierre o hasta quince segundos.");
            if (accion == "sesion")
            {
                if (actual is null) presentes[id] = actual = new(invitado, sesion, reloj.GetUtcNow().AddSeconds(15));
                else actual.Vence = reloj.GetUtcNow().AddSeconds(15);
            }
            if (actual is null) throw new ErrorApi(409, "sesionVencida", "Reconectá el invitado.");
            bool estabaPresente = PartidaPresente(id);
            var partida = almacen.Activa(id);
            if (accion is "sesion" or "presencia")
            {
                actual.Vence = reloj.GetUtcNow().AddSeconds(15);
                if (partida is not null)
                {
                    if (accion == "sesion") sesiones.Incorporar(partida.IdPartida, secreto, new(sesion, actual.Invitado.Nombre));
                    else sesiones.Renovar(partida.IdPartida, secreto, sesion);
                }
            }
            if ((accion is "presencia" or "estado") && partida is null && actual.Rival is not null)
            {
                IntentarIniciar(actual);
                partida = almacen.Activa(id);
            }
            switch (accion)
            {
                case "jugar" when partida is null:
                    if (!actual.Buscando && actual.Rival is null) Encolar(id, actual);
                    Emparejar();
                    partida = almacen.Activa(id);
                    break;
                case "cancelar" when partida is null:
                    Retirar(id); Emparejar(); break;
                case "salir":
                    Retirar(id);
                    presentes.Remove(id);
                    if (partida is not null) sesiones.CerrarSesion(partida.IdPartida, partida.Rol);
                    Emparejar();
                    avisos.Publicar(id);
                    if (partida is not null) PublicarPartida(partida.IdPartida);
                    return new("libre", invitado.Nombre, null, null, null);
            }
            if (accion is "jugar" or "cancelar" or "presencia" || estabaPresente != PartidaPresente(id))
            {
                avisos.Publicar(id);
                if (actual.Rival is { } rivalActual) avisos.Publicar(rivalActual);
                if (partida is not null) PublicarPartida(partida.IdPartida);
            }
            return Vista(actual, partida);
        }
    }

    private void IntentarIniciar(Presencia participante)
    {
        if (participante.Rival is not { } rival || !presentes.TryGetValue(rival, out var otro)) return;
        try
        {
            var primero = participante.Orden < otro.Orden ? participante : otro;
            var segundo = ReferenceEquals(primero, participante) ? otro : participante;
            var nueva = almacen.CrearAutomatica(primero.Invitado, segundo.Invitado, catalogo);
            participante.Rival = otro.Rival = null;
            participante.Aviso = otro.Aviso = null;
            sesiones.IncorporarAutenticado(nueva, "J1", primero.Sesion);
            sesiones.IncorporarAutenticado(nueva, "J2", segundo.Sesion);
            PublicarPartida(nueva);
        }
        catch (ExcepcionPartida error) when (error.Codigo == "operacionIncompatible")
        { participante.Aviso = otro.Aviso = "catalogoInsuficiente"; }
    }

    private bool PartidaPresente(Guid id) => almacen.Activa(id) is { } partida && sesiones.AmbosPresentes(partida.IdPartida);

    internal void PublicarPartida(Guid partida) => avisos.PublicarPartida(almacen, partida);

    internal void Vencer()
    {
        lock (Sincronizacion)
        {
            foreach (var id in presentes.Where(p => p.Value.Vence <= reloj.GetUtcNow()).Select(p => p.Key).ToArray())
            {
                var partida = almacen.Activa(id);
                Retirar(id);
                presentes.Remove(id);
                avisos.Publicar(id);
                if (partida is not null) PublicarPartida(partida.IdPartida);
            }
            Emparejar();
        }
    }
    private void Encolar(Guid id, Presencia p)
    { p.Buscando = true; p.Orden = ++orden; cola.AddLast(id); }
    private void Emparejar()
    {
        while (cola.Count >= 2)
        {
            var a = cola.First!.Value; cola.RemoveFirst();
            var b = cola.First!.Value; cola.RemoveFirst();
            presentes[a].Buscando = presentes[b].Buscando = false;
            presentes[a].Rival = b; presentes[b].Rival = a;
            avisos.Publicar(a); avisos.Publicar(b);
            IntentarIniciar(presentes[a]);
        }
    }
    private void Retirar(Guid id)
    {
        if (!presentes.TryGetValue(id, out var p)) return;
        cola.Remove(id); p.Buscando = false;
        if (p.Rival is { } rival && presentes.TryGetValue(rival, out var otro))
        {
            otro.Rival = null; otro.Aviso = "rivalSalio";
            avisos.Publicar(rival);
            if (otro.Vence > reloj.GetUtcNow()) Encolar(rival, otro);
        }
        p.Rival = null;
    }
    private EstadoAcceso Vista(Presencia p, AsignacionInvitado? partida)
    {
        var otro = p.Rival is { } rival ? presentes.GetValueOrDefault(rival) : null;
        return new(partida is not null ? "partida" : otro is not null ? "sala" : p.Buscando ? "buscando" : "libre",
            p.Invitado.Nombre, otro?.Invitado.Nombre, p.Aviso, partida?.IdPartida);
    }
}
