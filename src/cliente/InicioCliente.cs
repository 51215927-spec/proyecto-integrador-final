using System.Text.Json;
using System.Text;

namespace Proyecto_Integrador_Final.Cliente;

internal sealed class InicioCliente(TextReader entrada, TextWriter salida, AccesoLocal local)
{
    private readonly InterfazConsola interfaz = new();
    private Uri servidor = new("http://localhost:5000/");

    internal async Task Ejecutar(CancellationToken ct)
    {
        while (true)
        {
            try { servidor = new(local.Conexion().Direccion.TrimEnd('/') + "/"); break; }
            catch (IOException)
            {
                Mensaje("Configuración local", "No se pudo leer la conexión guardada.");
                if (await Menu(ct, "Configuración local", "Elegí cómo continuar.", ["Reintentar", "Salir"]) != "Reintentar") return;
            }
        }
        while (!ct.IsCancellationRequested)
        {
            var opcion = (await Menu(ct, "Inicio", "Elegí qué querés hacer",
                ["Jugar", "Crear perfil", "Administrar perfiles", "Conexión", "Salir"])).ToLowerInvariant();
            if (opcion is "salir" or "x") return;
            if (opcion == "administrar perfiles") { await AdministrarPerfiles(ct); continue; }
            if (opcion is "conexión" or "conexion" or "c") { await CambiarConexion(ct); continue; }
            bool crearPerfil = opcion is "crear perfil" or "nuevo";
            if (!crearPerfil && opcion is not ("jugar" or "j" or "")) continue;
            InvitadoLocal? invitado = null;
            Guid? reemplazar = null;
            while (true)
            {
                try
                {
                    if (invitado is null && reemplazar is null && !crearPerfil)
                    {
                        var seleccion = await ElegirPerfil(ct);
                        if (seleccion.Volver) break;
                        invitado = seleccion.Perfil;
                        crearPerfil = invitado is null;
                    }
                    local.Preparar();
                    if (invitado is null)
                    {
                        var nombre = (await PreguntarTexto(ct, "Nuevo perfil", "Nombre del jugador",
                            "Escribí un nombre o dejalo vacío para usar Invitado.", "Invitado")).Trim();
                        if (nombre.Length == 0) nombre = "Invitado";
                        if (nombre.Length > 80 || nombre.Any(char.IsControl))
                        { Mensaje("Nombre inválido", "Usá un nombre de hasta 80 caracteres."); continue; }
                        invitado = await ClienteAccesoHttp.Crear(servidor, nombre, ct);
                    }
                    // Si guardar falla, se conserva este acceso en memoria para reintentar, sin crear otro invitado.
                    if (reemplazar is { } anterior) local.Reemplazar(servidor, anterior, invitado);
                    else local.Agregar(servidor, invitado);
                    reemplazar = null;
                    local.GuardarConexion(servidor);
                    using var http = new ClienteAccesoHttp(servidor, invitado);
                    var estado = await http.Accion("sesion", ct);
                    Mensaje("Perfil", invitado.Nombre);
                    if (estado.Estado == "partida")
                    {
                        var siguiente = await Menu(ct, "Partida activa", "Encontramos una partida para este perfil.",
                            ["Retomar partida", "Volver al inicio"], invitado.Nombre);
                        if (siguiente != "Retomar partida")
                        { await http.Accion("salir", ct); break; }
                    }
                    using var fin = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    var avisos = http.Escuchar(fin.Token);
                    try
                    {
                        if (estado.Estado != "partida") estado = await http.Accion("jugar", ct);
                        estado = await Sala(http, estado, ct);
                        if (estado.Partida is { } partida)
                        {
                            var config = new ConfiguracionCliente(servidor, partida, invitado.Secreto, invitado.Nombre);
                            using var transporte = new ClientePartidaHttp(config, http.Sesion, acceso: http);
                            using var teclado = new EntradaCliente(entrada);
                            await new SesionPartidaCliente(config, transporte, new InterfazConsola(), avisos: http).Ejecutar(teclado, ct);
                        }
                    }
                    finally
                    {
                        fin.Cancel();
                        try { await avisos; } catch (OperationCanceledException) { }
                        using var liberar = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                        try { await http.Accion("salir", liberar.Token); } catch (Exception ex) when (ex is FalloCliente or OperationCanceledException) { }
                    }
                    break;
                }
                catch (IOException)
                {
                    Mensaje("Acceso local", "No se pudo leer o guardar. Tu perfil sigue intacto.");
                    if (await Menu(ct, "Acceso local", "Elegí cómo continuar.", ["Reintentar", "Salir"]) != "Reintentar") return;
                }
                catch (UnauthorizedAccessException)
                {
                    Mensaje("Acceso local", "No se puede acceder al almacenamiento del usuario.");
                    if (await Menu(ct, "Acceso local", "Elegí cómo continuar.", ["Reintentar", "Salir"]) != "Reintentar") return;
                }
                catch (FalloCliente error)
                {
                    if (error.Codigo == "invitadoInvalido")
                    {
                        Mensaje("Perfil rechazado", "Crear otro perfil no recupera las partidas anteriores.");
                        if (await Menu(ct, "Confirmar perfil nuevo", "¿Querés crear otro perfil?", ["Crear perfil nuevo", "Volver"]) != "Crear perfil nuevo") return;
                        // Se conserva el archivo anterior hasta emitir y guardar el reemplazo.
                        reemplazar = invitado!.Id;
                        invitado = null;
                        continue;
                    }
                    if (error.Codigo == "sesionOcupada")
                    {
                    Mensaje("Perfil ocupado", "Otro cliente tomó este perfil. Actualizando disponibilidad…");
                        invitado = null; crearPerfil = false; reemplazar = null;
                        continue;
                    }
                    Mensaje("Conexión", "No se pudo conectar con el servidor.");
                    var accion = await Menu(ct, "Conexión", "Elegí cómo continuar.", ["Reintentar", "Cambiar dirección", "Salir"]);
                    if (accion == "Reintentar") continue;
                    if (accion == "Cambiar dirección") { await CambiarConexion(ct); break; }
                    return;
                }
            }
        }
    }

    private async Task<(InvitadoLocal? Perfil, bool Volver)> ElegirPerfil(CancellationToken ct)
    {
        while (true)
        {
            var perfiles = local.Perfiles(servidor);
            if (perfiles.Length == 0) return (null, false);
            var libres = new List<InvitadoLocal>();
            var rechazados = new List<InvitadoLocal>();
            foreach (var perfil in perfiles)
            {
                using var http = new ClienteAccesoHttp(servidor, perfil);
                try { if (await http.Disponible(ct)) libres.Add(perfil); }
                catch (FalloCliente error) when (error.Codigo == "invitadoInvalido") { rechazados.Add(perfil); }
            }
            if (libres.Count == 1) return (libres[0], false);
            // Un acceso rechazado se conserva y solo se reemplaza mediante confirmación explícita.
            var elegibles = libres.Count > 0 ? libres : rechazados;
            if (libres.Count == 0 && rechazados.Count == 1) return (rechazados[0], false);
            var opciones = elegibles.Select(p => $"Usar perfil · {p.Nombre}").ToList();
            opciones.Add("Crear perfil"); opciones.Add("Reintentar disponibilidad"); opciones.Add("Volver");
            var (opcion, borrar) = await MenuPerfiles(ct, elegibles.Count == 0 ? "Perfiles ocupados" : "Elegir perfil",
                elegibles.Count == 0 ? "Todos los perfiles están en uso" : "Delete o Backspace elimina el perfil seleccionado sin confirmación.",
                opciones, elegibles.Count == 0 ? "Ocupado" : elegibles[0].Nombre, elegibles.Count);
            if (borrar is { } indiceBorrar)
            {
                local.Quitar(servidor, elegibles[indiceBorrar].Id);
                if (local.Perfiles(servidor).Length == 0) return (null, true);
                continue;
            }
            if (opcion is null) continue;
            int indice = opciones.IndexOf(opcion);
            if (indice >= 0 && indice < elegibles.Count) return (elegibles[indice], false);
            if (opcion == "Crear perfil") return (null, false);
            if (opcion == "Volver") return (null, true);
        }
    }

    private async Task AdministrarPerfiles(CancellationToken ct)
    {
        while (true)
        {
            var perfiles = local.Perfiles(servidor);
            if (perfiles.Length == 0)
            {
                await Menu(ct, "Administrar perfiles", "No hay perfiles guardados.", ["Volver"]);
                return;
            }
            var opciones = perfiles.Select(p => $"Perfil · {p.Nombre}").Append("Volver").ToArray();
            var (opcion, borrar) = await MenuPerfiles(ct, "Administrar perfiles",
                "Seleccioná un perfil y pulsá Delete o Backspace para borrarlo sin confirmación.",
                opciones, perfiles[0].Nombre, perfiles.Length);
            if (borrar is { } indiceBorrar)
            {
                local.Quitar(servidor, perfiles[indiceBorrar].Id);
                continue;
            }
            if (opcion == "Volver") return;
        }
    }

    private async Task<(string? Opcion, int? Borrar)> MenuPerfiles(CancellationToken ct, string titulo,
        string detalle, IReadOnlyList<string> opciones, string? perfil, int cantidadPerfiles)
    {
        int foco = 0;
        var texto = new StringBuilder();
        string? errorEntrada = null;
        while (true)
        {
            string? ayuda = texto.Length == 0 ? detalle : $"{detalle} · Opción: {texto}";
            if (errorEntrada is not null) ayuda = $"{errorEntrada} · Opción: {texto}";
            interfaz.MostrarInicio(perfil, titulo, opciones, foco, servidor.ToString(), ayuda);
            var evento = await LeerEntradaMenu(ct, opciones);
            if (evento.Opcion is { } opcion) return (opcion, null);
            var tecla = evento.Tecla;
            if ((tecla is ConsoleKey.Delete or ConsoleKey.Backspace) && foco < cantidadPerfiles)
                return (null, foco);
            if (tecla == ConsoleKey.Enter)
            {
                int directa = EntradaTeclado.IndiceOpcion(texto.ToString(), opciones);
                if (directa < 0 && texto.Length > 0 && !EntradaTeclado.EsSecuenciaAtajosNavegacion(texto.ToString()))
                { errorEntrada = "Opción no reconocida"; continue; }
                return (opciones[directa >= 0 ? directa : foco], null);
            }
            errorEntrada = null;
            if (tecla is ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.K or ConsoleKey.H)
                foco = Math.Max(0, foco - 1);
            else if (tecla is ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L)
                foco = Math.Min(opciones.Count - 1, foco + 1);
            else if (tecla == ConsoleKey.Escape) return (opciones[^1], null);
            if (tecla == ConsoleKey.Backspace && texto.Length > 0) texto.Length--;
            else if (!char.IsControl(evento.Caracter)) texto.Append(evento.Caracter);
            if (evento.Caracter == '\0' && tecla is ConsoleKey.UpArrow or ConsoleKey.DownArrow or ConsoleKey.LeftArrow or ConsoleKey.RightArrow)
                texto.Clear();
        }
    }

    private async Task<string> Menu(CancellationToken ct, string titulo, string detalle,
        IReadOnlyList<string> opciones, string? perfil = null)
    {
        int foco = 0;
        var texto = new StringBuilder();
        string? errorEntrada = null;
        while (true)
        {
            string? ayuda = texto.Length == 0 ? detalle : $"{detalle} · Opción: {texto}";
            if (errorEntrada is not null) ayuda = $"{errorEntrada} · Opción: {texto}";
            interfaz.MostrarInicio(perfil, titulo, opciones, foco, servidor.ToString(), ayuda);
            var evento = await LeerEntradaMenu(ct, opciones);
            if (evento.Opcion is { } opcion) return opcion;
            var tecla = evento.Tecla;
            if (tecla == ConsoleKey.Enter)
            {
                int directa = EntradaTeclado.IndiceOpcion(texto.ToString(), opciones);
                if (directa < 0 && texto.Length > 0 && !EntradaTeclado.EsSecuenciaAtajosNavegacion(texto.ToString()))
                { errorEntrada = "Opción no reconocida"; continue; }
                return opciones[directa >= 0 ? directa : foco];
            }
            errorEntrada = null;
            if (tecla is ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.K or ConsoleKey.H)
                foco = Math.Max(0, foco - 1);
            else if (tecla is ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L)
                foco = Math.Min(opciones.Count - 1, foco + 1);
            else if (tecla == ConsoleKey.Escape) return opciones[^1];
            if (tecla == ConsoleKey.Backspace && texto.Length > 0) texto.Length--;
            else if (!char.IsControl(evento.Caracter)) texto.Append(evento.Caracter);
            if (evento.Caracter == '\0' && tecla is ConsoleKey.UpArrow or ConsoleKey.DownArrow or ConsoleKey.LeftArrow or ConsoleKey.RightArrow)
                texto.Clear();
        }
    }

    private readonly record struct EntradaMenu(ConsoleKey? Tecla, string? Opcion, char Caracter = '\0');

    private async Task<EntradaMenu> LeerEntradaMenu(CancellationToken ct, IReadOnlyList<string> opciones)
    {
        if (!ReferenceEquals(entrada, Console.In) || Console.IsInputRedirected)
        {
            var linea = await entrada.ReadLineAsync(ct) ?? throw new EndOfStreamException();
            int indice = EntradaTeclado.IndiceOpcion(linea, opciones);
            return indice >= 0 ? new(null, opciones[indice]) : new(EntradaTeclado.Tecla(linea), null);
        }
        while (!Console.KeyAvailable) await Task.Delay(25, ct);
        var tecla = Console.ReadKey(true);
        if (tecla.KeyChar is '\u0004' or '\u001a') throw new EndOfStreamException("Fin de la entrada de consola.");
        return new(tecla.Key, null, tecla.KeyChar);
    }

    private async Task CambiarConexion(CancellationToken ct)
    {
        var valor = (await PreguntarTexto(ct, "Conexión", "Dirección del servidor",
            "Ingresá una dirección HTTP o HTTPS.", "http://localhost:5000")).Trim();
        if (valor.Length == 0) valor = "http://localhost:5000";
        if (!ConfiguracionCliente.EsUrl(valor)) { Mensaje("Dirección inválida", "Ingresá una URL HTTP o HTTPS válida."); return; }
        var nueva = new Uri(valor.TrimEnd('/') + "/");
        try { local.GuardarConexion(nueva); servidor = nueva; }
        catch (IOException) { Mensaje("Conexión", "No se pudo guardar la dirección. Reintentá desde Conexión."); }
    }

    private async Task<string> PreguntarTexto(CancellationToken ct, string titulo, string campo, string ayuda, string defecto)
    {
        if (!ReferenceEquals(entrada, Console.In) || Console.IsInputRedirected)
        {
            interfaz.MostrarCampo(null, titulo, campo, "", servidor.ToString(), ayuda + $" · Enter: {defecto}");
            return await Linea(ct);
        }

        var texto = new StringBuilder();
        while (true)
        {
            interfaz.MostrarCampo(null, titulo, campo, texto.ToString(), servidor.ToString(), ayuda + $" · Enter: {defecto}");
            while (!Console.KeyAvailable) await Task.Delay(25, ct);
            var tecla = Console.ReadKey(true);
            if (tecla.Key == ConsoleKey.Enter) return texto.ToString();
            if (tecla.Key == ConsoleKey.Escape) return "";
            if (tecla.KeyChar is '\u0004' or '\u001a') throw new EndOfStreamException("Fin de la entrada de consola.");
            if (tecla.Key == ConsoleKey.Backspace)
            { if (texto.Length > 0) texto.Length--; }
            else if (!char.IsControl(tecla.KeyChar)) texto.Append(tecla.KeyChar);
        }
    }

    private void Mensaje(string titulo, string detalle) =>
        interfaz.MostrarInicio(null, titulo, ["Continuar"], 0, servidor.ToString(), detalle);

    private async Task<EstadoAccesoCliente> Sala(ClienteAccesoHttp http, EstadoAccesoCliente estado, CancellationToken ct)
    {
        using var fin = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task<EntradaMenu>? lectura = null;
        int foco = 0;
        long version = -1;
        string? huella = null;
        string? opcionesAnteriores = null;
        string? errorEntrada = null;
        var texto = new StringBuilder();
        var presencia = DateTimeOffset.UtcNow.AddSeconds(5);
        var reintento = DateTimeOffset.MinValue;
        int fallos = 0;
        try
        {
            while (estado.Estado != "partida")
            {
                string situacion = estado.Estado switch
                {
                    "sala" => "Rival encontrado",
                    "libre" => "Búsqueda finalizada",
                    _ => http.Conectado ? "Buscando oponente" : "Reconectando…"
                };
                string[] opciones = estado.Estado == "sala"
                    ? ["Esperando inicio automático", "Cancelar búsqueda", "Salir sin abandonar"]
                    : estado.Estado == "libre" ? ["Buscar oponente", "Salir sin abandonar"]
                    : ["Cancelar búsqueda", "Salir sin abandonar"];
                string claveOpciones = JsonSerializer.Serialize(opciones);
                if (claveOpciones != opcionesAnteriores)
                {
                    foco = Math.Clamp(foco, 0, opciones.Length - 1);
                    texto.Clear();
                    errorEntrada = null;
                    huella = null;
                    opcionesAnteriores = claveOpciones;
                }
                if (huella != JsonSerializer.Serialize(new { estado, situacion, opciones, http.Conectado, foco, texto = texto.ToString(), errorEntrada }))
                {
                    string? detalle = texto.Length == 0 ? estado.Aviso : errorEntrada is not null
                        ? $"{errorEntrada} · Opción: {texto}"
                        : string.IsNullOrWhiteSpace(estado.Aviso) ? $"Opción: {texto}" : $"{estado.Aviso} · Opción: {texto}";
                    interfaz.MostrarSala(estado.Nombre, situacion, estado.Rival ?? "Esperando rival",
                        detalle, opciones, foco, servidor.ToString());
                    if (!http.Conectado) salida.WriteLine("Reconectando…");
                    huella = JsonSerializer.Serialize(new { estado, situacion, opciones, http.Conectado, foco, texto = texto.ToString(), errorEntrada });
                }
                lectura ??= LeerEntradaMenu(fin.Token, opciones);
                try
                {
                    if (lectura.IsCompleted)
                    {
                        var evento = await lectura; lectura = null;
                        var tecla = evento.Tecla;
                        if (tecla is ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.K or ConsoleKey.H)
                            foco = Math.Max(0, foco - 1);
                        else if (tecla is ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L)
                            foco = Math.Min(opciones.Length - 1, foco + 1);
                        string? opcionActual = evento.Opcion is { } opcionEscrita &&
                            EntradaTeclado.IndiceOpcion(opcionEscrita, opciones) is int indiceActual && indiceActual >= 0
                                ? opciones[indiceActual] : null;
                        string accion = opcionActual?.ToLowerInvariant() ??
                            (tecla == ConsoleKey.Enter ? opciones[foco].ToLowerInvariant() :
                            tecla == ConsoleKey.C && evento.Caracter == '\0' ? "cancelar búsqueda" : "");
                        if (tecla == ConsoleKey.Enter && texto.Length > 0)
                        {
                            int directa = EntradaTeclado.IndiceOpcion(texto.ToString(), opciones);
                            if (directa >= 0) accion = opciones[directa].ToLowerInvariant();
                            else if (EntradaTeclado.EsIgual(texto.ToString(), "c") || EntradaTeclado.EsIgual(texto.ToString(), "cancelar"))
                                accion = "cancelar búsqueda";
                            else if (EntradaTeclado.EsIgual(texto.ToString(), "x") || EntradaTeclado.EsIgual(texto.ToString(), "salir"))
                                accion = "salir sin abandonar";
                            else if (!EntradaTeclado.EsSecuenciaAtajosNavegacion(texto.ToString()))
                            { accion = "opción inválida"; errorEntrada = "Opción no reconocida"; }
                            if (accion != "opción inválida") { texto.Clear(); errorEntrada = null; }
                        }
                        // Las líneas de texto se aceptan en pruebas y terminales redirigidas.
                        if (tecla == ConsoleKey.NoName) accion = "";
                        if (tecla == ConsoleKey.Escape) accion = "salir sin abandonar";
                        if (tecla is ConsoleKey.UpArrow or ConsoleKey.DownArrow or ConsoleKey.LeftArrow or ConsoleKey.RightArrow)
                        { texto.Clear(); errorEntrada = null; }
                        else if (tecla != ConsoleKey.Enter && evento.Caracter == '\0' && tecla is not ConsoleKey.C)
                            texto.Clear();
                        else if (!char.IsControl(evento.Caracter)) { texto.Append(evento.Caracter); errorEntrada = null; }
                        if (accion is "salir" or "salir sin abandonar" or "x")
                            return await http.Accion("salir", ct);
                        if (accion is "cancelar" or "cancelar búsqueda" or "c")
                        {
                            estado = await http.Accion("cancelar", ct);
                            if (estado.Estado != "partida") return estado;
                        }
                        else if (tecla == ConsoleKey.Enter && (accion is "" or "esperando inicio automático") && estado.Estado == "sala")
                            estado = await http.Accion(null, ct);
                        else if ((accion is "jugar" or "j" or "buscar oponente") && estado.Estado == "libre")
                            estado = await http.Accion("jugar", ct);
                    }
                    if (DateTimeOffset.UtcNow >= presencia)
                    { await http.Renovar(ct); presencia = DateTimeOffset.UtcNow.AddSeconds(5); }
                    if (http.Version != version && http.Conectado && DateTimeOffset.UtcNow >= reintento)
                    {
                        var consultada = http.Version;
                        estado = await http.Accion(null, ct);
                        version = consultada; fallos = 0;
                    }
                }
                catch (FalloCliente error) when (error.Transitorio || error.Codigo == "sesionVencida")
                {
                    salida.WriteLine("Reconectando…");
                    reintento = DateTimeOffset.UtcNow.AddSeconds(++fallos switch { 1 => 1, 2 => 2, _ => 5 });
                    presencia = reintento;
                }
                await Task.Delay(25, ct);
            }
            return estado;
        }
        finally
        {
            fin.Cancel();
            if (lectura is not null) try { await lectura; } catch (OperationCanceledException) { }
        }
    }

    private async Task<string> Linea(CancellationToken ct)
    {
        if (!ReferenceEquals(entrada, Console.In) || Console.IsInputRedirected)
            return await entrada.ReadLineAsync(ct) ?? throw new EndOfStreamException();
        var texto = new StringBuilder();
        while (true)
        {
            while (!Console.KeyAvailable) await Task.Delay(25, ct);
            var tecla = Console.ReadKey(true);
            if (tecla.Key == ConsoleKey.Enter) { salida.WriteLine(); return texto.ToString(); }
            if (tecla.KeyChar is '\u0004' or '\u001a') throw new EndOfStreamException("Fin de la entrada de consola.");
            if (tecla.Key == ConsoleKey.Backspace)
            { if (texto.Length > 0) { texto.Length--; salida.Write("\b \b"); } }
            else if (!char.IsControl(tecla.KeyChar)) { texto.Append(tecla.KeyChar); salida.Write(tecla.KeyChar); }
        }
    }
}
