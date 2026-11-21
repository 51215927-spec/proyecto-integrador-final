using System.Text.Json;

namespace Proyecto_Integrador_Final.Cliente;

// El bucle es el único dueño del modelo y de la consola. HTTP nunca modifica la interfaz.
internal sealed class SesionPartidaCliente(ConfiguracionCliente configuracion, ClientePartidaHttp http,
    InterfazConsola interfaz, TimeProvider? reloj = null)
{
    private readonly TimeProvider tiempo = reloj ?? TimeProvider.System;
    private readonly EstadoCliente estado = new(configuracion.Partida);
    private bool incorporado, terminal, protocolo, corregir, abandono, salidaFinal;
    private string aviso = "Conectando…";
    private string? huella, seleccion;
    private int foco, carta, menu = -1;
    private bool atributo, confirmarAbandono;
    private EleccionCliente? pendiente;
    private bool reciboAusente;
    private ResultadoCliente? leyendo;
    private DateTimeOffset proximaVista, proximaPresencia, proximaEscritura;
    private int fallosVista, fallosPresencia, fallosEscritura;
    private Task<PresentacionCliente>? consulta;
    private Task? presencia;
    private Task<object?>? escritura;
    private string claseEscritura = "";
    private int revisionMinima;
    private int generacion, generacionVista, generacionPresencia;

    private DateTimeOffset Ahora => tiempo.GetUtcNow();
    private TimeSpan Espera(ref int fallos) => TimeSpan.FromSeconds(++fallos switch { 1 => 1, 2 => 2, _ => 5 });

    internal async Task<bool> Ejecutar(EntradaCliente entrada, CancellationToken ct)
    {
        using var cierre = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = cierre.Token;
        Task<(ConsoleKey? Tecla, int Version)>? tecla = null;
        try
        {
            Dibujar();
            while (!ct.IsCancellationRequested && !corregir)
            {
                if (escritura?.IsCompleted == true) await CompletarEscritura();
                if (consulta?.IsCompleted == true) await CompletarConsulta();
                if (presencia?.IsCompleted == true) await CompletarPresencia();
                if (!terminal)
                {
                    if (incorporado)
                    {
                        if (!protocolo && consulta is null && Ahora >= proximaVista)
                        {
                            generacionVista = generacion;
                            consulta = http.Presentacion(token);
                            proximaVista = Ahora.AddSeconds(1);
                        }
                        if (!corregible && !presenciaBloqueada && presencia is null && Ahora >= proximaPresencia)
                        {
                            generacionPresencia = generacion;
                            presencia = http.Renovar(token);
                            proximaPresencia = Ahora.AddSeconds(5);
                        }
                    }
                    if (!protocolo && escritura is null && Ahora >= proximaEscritura)
                    {
                        if (!incorporado) IniciarEscritura("sesion", async () => await http.Incorporar(token));
                        else if (pendiente is not null)
                        {
                            if (!reciboAusente) IniciarEscritura("recibo", async () => await http.Recibo(pendiente.IdOperacion, token));
                            else if (estado.Actual && estado.Vista is { } v)
                            {
                                if (v.Pausada) { /* La operación se conserva hasta reanudación. */ }
                                else if (!Vigente(pendiente, v)) { pendiente = null; reciboAusente = false; aviso = "La solicitud cambió; no se reenvió la elección."; Dibujar(); }
                                else IniciarEscritura("eleccion", async () => await http.Elegir(pendiente, token));
                            }
                        }
                        else if (abandono && estado.Actual)
                            IniciarEscritura("abandono", async () => { await http.Abandonar(token); return null; });
                    }
                }
                tecla ??= LeerEvento(entrada, token);
                if (tecla.IsCompleted)
                {
                    var evento = await tecla;
                    tecla = null;
                    if (evento.Tecla is null) { aviso = "Fin de la entrada."; break; }
                    if (evento.Version == versionPantalla && Procesar(evento.Tecla.Value)) break;
                }
                if (descartarEntrada) { entrada.DescartarSobrante(); descartarEntrada = false; }
                await Task.Delay(TimeSpan.FromMilliseconds(25), tiempo, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally
        {
            cierre.Cancel();
            var tareas = new Task?[] { consulta, presencia, escritura, tecla }.Where(t => t is not null).Cast<Task>();
            foreach (var tarea in tareas)
                try { await tarea; } catch (Exception ex) when (ex is FalloCliente or OperationCanceledException or IOException) { }
        }
        if (!corregir && !salidaFinal)
            interfaz.MostrarInterrupcion(configuracion.Nombre, "Salida local", abandono
                ? "Abandono sin confirmar. La plaza puede seguir ocupada hasta quince segundos."
                : "La partida queda guardada. La plaza puede seguir ocupada hasta quince segundos o hasta que el operador cierre la sesión.", false);
        return corregir;
    }

    private bool descartarEntrada;
    private void IniciarEscritura(string clase, Func<Task<object?>> iniciar)
    {
        claseEscritura = clase;
        if (clase == "eleccion") aviso = "Envío en curso";
        escritura = iniciar();
    }

    private async Task CompletarEscritura()
    {
        var tarea = escritura!;
        escritura = null;
        try
        {
            object? respuesta = await tarea;
            fallosEscritura = 0;
            proximaEscritura = Ahora;
            switch (claseEscritura)
            {
                case "sesion":
                    var s = (SesionConfirmada)respuesta!;
                    if (s.IdSesion != http.IdSesion || s.Rol is not ("J1" or "J2")) throw new FalloCliente("protocoloIncompatible");
                    incorporado = true;
                    aviso = $"{configuracion.Nombre} · Plaza confirmada: {s.Rol}";
                    proximaVista = Ahora;
                    proximaPresencia = Ahora.AddSeconds(5);
                    break;
                case "eleccion":
                case "recibo":
                    var r = (ReciboCliente)respuesta!;
                    if (pendiente is null || r.IdOperacion != pendiente.IdOperacion || r.IdSolicitud != pendiente.IdSolicitud || r.RevisionConfirmada <= pendiente.RevisionEsperada)
                        throw new FalloCliente("protocoloIncompatible");
                    revisionMinima = Math.Max(revisionMinima, r.RevisionConfirmada);
                    pendiente = null;
                    reciboAusente = false;
                    estado.PerderComunicacion(); // Un recibo no es una presentación del turno siguiente.
                    proximaVista = Ahora;
                    aviso = "Recepción confirmada. Esperando presentación del servidor.";
                    break;
                case "abandono":
                    revisionMinima = Math.Max(revisionMinima, (estado.Vista?.Revision ?? 0) + (terminal ? 0 : 1));
                    abandono = false;
                    estado.PerderComunicacion();
                    aviso = "Abandono confirmado. Consultando motivo final.";
                    proximaVista = Ahora;
                    break;
            }
        }
        catch (FalloCliente error)
        {
            if (claseEscritura == "recibo" && error.Codigo == "recursoNoEncontrado")
            {
                reciboAusente = true;
                estado.PerderComunicacion();
                proximaVista = Ahora;
            }
            else if (claseEscritura == "eleccion" && error.Codigo is "entradaInvalida" or "eleccionInvalida" or "revisionDesactualizada" or "solicitudNoVigente" or "operacionIncompatible")
            {
                pendiente = null;
                reciboAusente = false;
                aviso = $"Elección rechazada: {error.Codigo}. Actualizando opciones.";
                estado.PerderComunicacion();
                proximaVista = Ahora;
            }
            else
            {
                if (claseEscritura == "eleccion") reciboAusente = false;
                Tratar(error);
                proximaEscritura = Ahora + Espera(ref fallosEscritura);
            }
        }
        Dibujar();
    }

    private async Task CompletarConsulta()
    {
        var tarea = consulta!;
        consulta = null;
        try
        {
            var vista = await tarea;
            if (generacionVista != generacion || terminal) return;
            if (vista.Revision < revisionMinima || vista.Revision < (estado.Vista?.Revision ?? 0))
            { proximaVista = Ahora; return; }
            estado.Aplicar(vista);
            fallosVista = 0;
            if (seleccion != ClaveSeleccion(vista))
            {
                seleccion = ClaveSeleccion(vista); foco = 0; atributo = false; descartarEntrada = true; versionPantalla++;
            }
            if (leyendo is null && estado.Resultados.Count > 0)
            { leyendo = estado.Resultados.Dequeue(); descartarEntrada = true; versionPantalla++; }
            if (vista.Fase == "finalizada")
            {
                terminal = true;
                abandono = false;
                aviso = "Final confirmado por el servidor";
            }
            else if (pendiente is null && !abandono) aviso = "Estado actualizado";
            Dibujar();
        }
        catch (FalloCliente error)
        {
            if (generacionVista != generacion || terminal) return;
            Tratar(error);
            proximaVista = Ahora + Espera(ref fallosVista);
            Dibujar();
        }
    }

    private async Task CompletarPresencia()
    {
        var tarea = presencia!;
        presencia = null;
        try { await tarea; fallosPresencia = 0; }
        catch (FalloCliente error)
        {
            if (generacionPresencia != generacion || terminal) return;
            Tratar(error);
            if (!error.Transitorio && error.Codigo != "sesionVencida") presenciaBloqueada = true;
            proximaPresencia = Ahora + Espera(ref fallosPresencia);
            Dibujar();
        }
    }

    private void Tratar(FalloCliente error)
    {
        estado.PerderComunicacion();
        seleccion = null;
        atributo = false;
        aviso = error.Transitorio ? "Sin comunicación · Último estado confirmado. Reconectando…" : error.Codigo;
        if (error.Codigo == "sesionVencida")
        {
            incorporado = false; generacion++; reciboAusente = false;
            aviso = "Sesión vencida. Reincorporando con la misma identidad…";
        }
        else if (error.Codigo is "entradaInvalida" or "credencialInvalida" or "sinPermiso" or "recursoNoEncontrado")
        {
            protocolo = true;
            aviso += " · Enter para corregir los datos o Esc para salir. Una elección anterior puede haberse aplicado; recuperar requiere sus datos originales.";
            corregible = true;
        }
        else if (error.Codigo == "protocoloIncompatible")
        {
            protocolo = true;
            aviso = "Respuesta incompatible o inconsistente · Enter para reintentar o Esc para salir.";
        }
        else if (error.Codigo == "sesionOcupada")
            aviso = "Sesión ocupada. Esperando liberación o vencimiento (hasta quince segundos). Esc para salir.";
    }

    private bool corregible, presenciaBloqueada;
    private int versionPantalla;

    private async Task<(ConsoleKey? Tecla, int Version)> LeerEvento(EntradaCliente entrada, CancellationToken ct)
    {
        var tecla = await entrada.Leer(ct);
        return (tecla, Volatile.Read(ref versionPantalla));
    }
    private static bool Vigente(EleccionCliente e, PresentacionCliente v) =>
        v.Revision == e.RevisionEsperada && v.Solicitud is { } s && s.IdSolicitud == e.IdSolicitud &&
        s.IdsPersonajePermitidos.Contains(e.IdPersonaje) && (s.Tipo == "carta" ? e.Atributo is null : s.AtributosPermitidos.Contains(e.Atributo));
    private static string ClaveSeleccion(PresentacionCliente v) => JsonSerializer.Serialize(new { v.Revision, v.Solicitud, v.Pausada });

    private bool Procesar(ConsoleKey tecla)
    {
        if (tecla == ConsoleKey.Escape)
        {
            menu = menu < 0 ? 0 : -1;
            confirmarAbandono = false;
            Dibujar(true);
            return false;
        }
        int paso = tecla switch
        {
            ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.H or ConsoleKey.K => -1,
            ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L => 1,
            _ => 0
        };
        if (menu >= 0)
        {
            if (paso != 0) menu = Math.Clamp(menu + paso, 0, confirmarAbandono ? 1 : 2);
            if (tecla == ConsoleKey.Enter)
            {
                if (confirmarAbandono)
                {
                    if (menu == 1 && !terminal) { abandono = true; aviso = "Abandono pendiente de confirmación del servidor."; }
                    confirmarAbandono = false; menu = -1;
                }
                else if (menu == 1) return true;
                else if (menu == 2 && !terminal) { confirmarAbandono = true; menu = 0; }
                else menu = -1;
            }
            Dibujar(true);
            return false;
        }
        if (tecla == ConsoleKey.Enter && protocolo)
        {
            if (corregible) { corregir = true; return false; }
            protocolo = false; presenciaBloqueada = false; incorporado = false; generacion++; reciboAusente = false; proximaEscritura = Ahora;
            Dibujar(true); return false;
        }
        if (leyendo is not null)
        {
            if (tecla == ConsoleKey.Enter)
            {
                leyendo = estado.Resultados.Count > 0 ? estado.Resultados.Dequeue() : null;
                descartarEntrada = true; versionPantalla++;
                Dibujar(true);
            }
            return false;
        }
        if (terminal)
        {
            salidaFinal = tecla == ConsoleKey.Enter;
            return salidaFinal;
        }
        if (!estado.Actual || pendiente is not null || abandono || escritura is not null)
        { if (tecla == ConsoleKey.Enter) Dibujar(true); return false; }
        var v = estado.Vista!;
        if (v.Pausada || v.Solicitud is not { } s)
        { if (tecla == ConsoleKey.Enter) Dibujar(true); return false; }
        if (seleccion != ClaveSeleccion(v)) { seleccion = ClaveSeleccion(v); foco = 0; atributo = false; Dibujar(true); return false; }
        if (paso != 0)
        {
            foco = Math.Clamp(foco + paso, 0, (atributo ? s.AtributosPermitidos.Length : s.IdsPersonajePermitidos.Length) - 1);
            Dibujar(true);
        }
        if (tecla == ConsoleKey.Enter)
        {
            if (!atributo)
            {
                carta = s.IdsPersonajePermitidos[foco];
                if (s.Tipo == "cartaYAtributo") { atributo = true; foco = 0; Dibujar(true); return false; }
            }
            var e = new EleccionCliente(Guid.NewGuid(), s.IdSolicitud, v.Revision, carta,
                atributo ? s.AtributosPermitidos[foco] : null);
            if (!Vigente(e, v)) return false;
            pendiente = e; reciboAusente = true; atributo = false; foco = 0;
            aviso = "Envío en curso"; descartarEntrada = true; versionPantalla++;
            Dibujar(true);
        }
        return false;
    }

    private void Dibujar(bool forzar = false)
    {
        var v = estado.Vista;
        string nueva = JsonSerializer.Serialize(new { v, estado.Actual, aviso, menu, confirmarAbandono, leyendo, foco, atributo, pendiente, terminal });
        if (!forzar && huella == nueva) return;
        huella = nueva;
        interfaz.MostrarRed(configuracion.Nombre, v, estado.Actual, aviso, leyendo, menu, confirmarAbandono,
            foco, atributo, pendiente is not null, abandono);
    }
}
