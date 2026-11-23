namespace Proyecto_Integrador_Final.Api;

internal sealed record EstadoAcceso(string Estado, string Nombre, bool Listo, string? Rival,
    bool RivalListo, string? Aviso, Guid? Partida);

// Solo coordina acceso y reservas. Todas las transiciones de juego pertenecen a Partida.
internal sealed class Emparejamiento(Invitados invitados, AlmacenPartidas almacen,
    AdministradorSesiones sesiones, CatalogoPersonajes catalogo, TimeProvider reloj, Avisos avisos)
{
    private sealed class Presencia(InvitadoGuardado invitado, Guid sesion, DateTimeOffset vence)
    {
        internal InvitadoGuardado Invitado = invitado;
        internal Guid Sesion = sesion;
        internal DateTimeOffset Vence = vence;
        internal bool Buscando, Listo;
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
            switch (accion)
            {
                case "jugar" when partida is null:
                    if (!actual.Buscando && actual.Rival is null) Encolar(id, actual);
                    Emparejar();
                    break;
                case "listo" when partida is null:
                    if (actual.Rival is not { } rival || !presentes.TryGetValue(rival, out var otro))
                        throw new ErrorApi(409, "operacionIncompatible", "No hay una sala vigente.");
                    actual.Listo = true;
                    if (otro.Listo)
                    {
                        try
                        {
                            // Se conserva el orden FIFO asignado a las plazas de la sala.
                            var primero = actual.Orden < otro.Orden ? actual : otro;
                            var segundo = ReferenceEquals(primero, actual) ? otro : actual;
                            var nueva = almacen.CrearAutomatica(primero.Invitado, segundo.Invitado, catalogo);
                            actual.Rival = otro.Rival = null;
                            actual.Listo = otro.Listo = false;
                            actual.Aviso = otro.Aviso = null;
                            // La credencial ya fue validada; ambas sesiones reciben acceso sin secretos en memoria.
                            sesiones.IncorporarAutenticado(nueva, "J1", primero.Sesion);
                            sesiones.IncorporarAutenticado(nueva, "J2", segundo.Sesion);
                            partida = almacen.Activa(id);
                        }
                        catch (ExcepcionPartida error) when (error.Codigo == "operacionIncompatible")
                        { actual.Aviso = otro.Aviso = "catalogoInsuficiente"; }
                    }
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
                    return new("libre", invitado.Nombre, false, null, false, null, null);
            }
            if (accion is "jugar" or "listo" or "cancelar" || estabaPresente != PartidaPresente(id))
            {
                avisos.Publicar(id);
                if (actual.Rival is { } rivalActual) avisos.Publicar(rivalActual);
                if (partida is not null) PublicarPartida(partida.IdPartida);
            }
            return Vista(actual, partida);
        }
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
    { p.Buscando = true; p.Listo = false; p.Orden = ++orden; cola.AddLast(id); }
    private void Emparejar()
    {
        while (cola.Count >= 2)
        {
            var a = cola.First!.Value; cola.RemoveFirst();
            var b = cola.First!.Value; cola.RemoveFirst();
            presentes[a].Buscando = presentes[b].Buscando = false;
            presentes[a].Rival = b; presentes[b].Rival = a;
            avisos.Publicar(a); avisos.Publicar(b);
        }
    }
    private void Retirar(Guid id)
    {
        if (!presentes.TryGetValue(id, out var p)) return;
        cola.Remove(id); p.Buscando = false; p.Listo = false;
        if (p.Rival is { } rival && presentes.TryGetValue(rival, out var otro))
        {
            otro.Rival = null; otro.Listo = false; otro.Aviso = "rivalSalio";
            avisos.Publicar(rival);
            if (otro.Vence > reloj.GetUtcNow()) Encolar(rival, otro);
        }
        p.Rival = null;
    }
    private EstadoAcceso Vista(Presencia p, AsignacionInvitado? partida)
    {
        var otro = p.Rival is { } rival ? presentes.GetValueOrDefault(rival) : null;
        return new(partida is not null ? "partida" : otro is not null ? "sala" : p.Buscando ? "buscando" : "libre",
            p.Invitado.Nombre, p.Listo, otro?.Invitado.Nombre, otro?.Listo ?? false, p.Aviso, partida?.IdPartida);
    }
}
