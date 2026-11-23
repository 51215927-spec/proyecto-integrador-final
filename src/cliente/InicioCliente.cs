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
                ["Jugar", "Crear perfil", "Conexión", "Recuperar acceso antiguo", "Salir"])).ToLowerInvariant();
            if (opcion is "salir" or "x") return;
            if (opcion is "conexión" or "conexion" or "c") { await CambiarConexion(ct); continue; }
            if (opcion == "recuperar acceso antiguo")
            {
                // Flujo excepcional v1; nunca adjudica por nombre ni reemplaza identidades locales.
                    interfaz.MostrarInicio(null, "Recuperar acceso", ["URL", "Partida", "Credencial", "Nombre"], 0, servidor.ToString(), "Acceso excepcional de una partida anterior");
                    var config = await ConfiguracionCliente.LeerAsync(entrada, salida, ct);
                using var anterior = new ClientePartidaHttp(config, Guid.NewGuid());
                using var teclado = new EntradaCliente(entrada);
                await new SesionPartidaCliente(config, anterior, new InterfazConsola()).Ejecutar(teclado, ct);
                return;
            }
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
            var opcion = await Menu(ct, elegibles.Count == 0 ? "Perfiles ocupados" : "Elegir perfil",
                elegibles.Count == 0 ? "Todos los perfiles están en uso" : "Seleccioná con las flechas y confirmá",
                opciones, elegibles.Count == 0 ? "Ocupado" : elegibles[0].Nombre);
            int indice = opciones.IndexOf(opcion);
            if (indice >= 0 && indice < elegibles.Count) return (elegibles[indice], false);
            if (opcion == "Crear perfil") return (null, false);
            if (opcion == "Volver") return (null, true);
        }
    }

    private async Task<string> Menu(CancellationToken ct, string titulo, string detalle,
        IReadOnlyList<string> opciones, string? perfil = null)
    {
        int foco = 0;
        while (true)
        {
            interfaz.MostrarInicio(perfil, titulo, opciones, foco, servidor.ToString(), detalle);
            var tecla = await LeerTecla(ct);
            if (tecla is ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.K or ConsoleKey.H)
                foco = Math.Max(0, foco - 1);
            else if (tecla is ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L)
                foco = Math.Min(opciones.Count - 1, foco + 1);
            else if (tecla == ConsoleKey.Enter) return opciones[foco];
            else if (tecla == ConsoleKey.Escape) return opciones[^1];
        }
    }

    private async Task<ConsoleKey?> LeerTecla(CancellationToken ct)
    {
        if (!ReferenceEquals(entrada, Console.In) || Console.IsInputRedirected)
            return (await entrada.ReadLineAsync(ct))?.Trim().ToLowerInvariant() switch
            {
                null => null, "" or "enter" => ConsoleKey.Enter, "esc" => ConsoleKey.Escape,
                "w" => ConsoleKey.W, "a" => ConsoleKey.A, "s" => ConsoleKey.S, "d" => ConsoleKey.D,
                "h" => ConsoleKey.H, "j" => ConsoleKey.J, "k" => ConsoleKey.K, "l" => ConsoleKey.L,
                _ => ConsoleKey.NoName
            };
        while (!Console.KeyAvailable) await Task.Delay(25, ct);
        return Console.ReadKey(true).Key;
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
        interfaz.MostrarInicio(null, titulo, [campo], 0, servidor.ToString(), ayuda + $"\nValor predeterminado: {defecto}");
        return await Linea(ct);
    }

    private void Mensaje(string titulo, string detalle) =>
        interfaz.MostrarInicio(null, titulo, ["Continuar"], 0, servidor.ToString(), detalle);

    private async Task<EstadoAccesoCliente> Sala(ClienteAccesoHttp http, EstadoAccesoCliente estado, CancellationToken ct)
    {
        using var fin = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task<string>? lectura = null;
        long version = -1;
        string? huella = null;
        var presencia = DateTimeOffset.UtcNow.AddSeconds(5);
        var reintento = DateTimeOffset.MinValue;
        int fallos = 0;
        try
        {
            while (estado.Estado != "partida")
            {
                string situacion = estado.Estado switch
                {
                    "sala" => "Sala de preparación",
                    "libre" => "Búsqueda finalizada",
                    _ => http.Conectado ? "Buscando oponente" : "Reconectando…"
                };
                string[] opciones = estado.Estado == "sala"
                    ? [estado.Listo ? "Esperando al rival" : "Listo", "Cancelar búsqueda", "Salir sin abandonar"]
                    : estado.Estado == "libre" ? ["Buscar oponente", "Salir sin abandonar"]
                    : ["Cancelar búsqueda", "Salir sin abandonar"];
                if (huella != JsonSerializer.Serialize(new { estado, situacion, opciones, http.Conectado }))
                {
                    interfaz.MostrarSala(estado.Nombre, situacion, estado.Rival ?? "Esperando rival",
                        estado.Listo, estado.RivalListo, estado.Aviso, opciones, 0, servidor.ToString());
                    if (!http.Conectado) salida.WriteLine("Reconectando…");
                    huella = JsonSerializer.Serialize(new { estado, situacion, opciones, http.Conectado });
                }
                lectura ??= Linea(fin.Token);
                try
                {
                    if (lectura.IsCompleted)
                    {
                        var accion = (await lectura).Trim().ToLowerInvariant(); lectura = null;
                        if (accion is "salir" or "salir sin abandonar" or "x")
                            return await http.Accion("salir", ct);
                        if (accion is "cancelar" or "cancelar búsqueda" or "c")
                        {
                            estado = await http.Accion("cancelar", ct);
                            if (estado.Estado != "partida") return estado;
                        }
                        else if ((accion is "listo" or "l" or "") && estado.Estado == "sala")
                            estado = await http.Accion("listo", ct);
                        else if ((accion is "jugar" or "j") && estado.Estado == "libre")
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
            if (tecla.KeyChar is '\u0004' or '\u001a') throw new EndOfStreamException();
            if (tecla.Key == ConsoleKey.Backspace)
            { if (texto.Length > 0) { texto.Length--; salida.Write("\b \b"); } }
            else if (!char.IsControl(tecla.KeyChar)) { texto.Append(tecla.KeyChar); salida.Write(tecla.KeyChar); }
        }
    }
}
