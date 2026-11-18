using System.Globalization;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Proyecto_Integrador_Final.Cliente;

internal sealed class InterfazConsola
{
    private static readonly string[] Atributos =
        ["Fuerza", "Velocidad", "Resistencia", "Inteligencia", "Energía", "Habilidad"];

    private static readonly Color Verde = new(189, 232, 140);
    private static readonly Style Titulo = new(Verde, decoration: Decoration.Bold);
    private static readonly Style Barra = new(Verde, new Color(32, 44, 36), Decoration.Bold);
    private static readonly Style Borde = new(new Color(95, 120, 105));

    private const string Controles = "Flechas, WASD o HJKL\nEnter para confirmar.";

    private readonly TextReader entrada;
    private readonly bool leerTeclas;
    private readonly bool limpiarPantalla;
    private readonly IAnsiConsole terminal;
    private readonly bool salidaLineal;
    private string? ultimaEspera;

    public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)
    {
        this.entrada = entrada ?? Console.In;
        bool interactiva = salida is null && !Console.IsOutputRedirected;
        limpiarPantalla = interactiva;
        leerTeclas = interactiva && entrada is null && !Console.IsInputRedirected;
        salidaLineal = !interactiva && ancho is null;
        terminal = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(salida ?? Console.Out),
            Ansi = interactiva ? AnsiSupport.Detect : AnsiSupport.No
        });

        if (ancho is { } fijo)
        {
            terminal.Profile.Width = fijo;
            terminal.Profile.Capabilities.Unicode = true;
        }
    }

    public void MostrarPreparacion(string? local, string estado)
    {
        Encabezado(local, "Preparación");
        Paneles(PanelInfo("Jugador local", local ?? "Identidad pendiente"), PanelInfo("Estado", TextoVisible(estado)));
    }

    public void MostrarEsperaInicial(string local, string participantes, string motivo)
    {
        Encabezado(local, "Antes de la partida", motivo);
        Paneles(PanelInfo("Participantes", TextoVisible(participantes)), PanelInfo("Inicio", "A cargo del operador"));
        Pie("Esperá al rival y al inicio de la partida.");
    }

    public void MostrarInicioRonda(MesaVisible mesa, string mano, IReadOnlyList<CartaVisible> cartas)
    {
        Encabezado(mesa, $"{TextoVisible(mano)} abre la ronda");
        Paneles(PanelInfo("Tu mano", ResumenMano(cartas)), PanelInfo("Rondas ganadas", Rondas(mesa)));
        Pie(mesa.Local == mano ? "Elegí una carta y después un atributo." : $"Esperá a {TextoVisible(mano)}.");
    }

    public void MostrarMano(IReadOnlyList<CartaVisible> mano) => MostrarMano(mano, foco: null);

    private void MostrarMano(IReadOnlyList<CartaVisible> mano, int? foco, string titulo = "Tu mano")
    {
        Escribir($"{titulo} · {mano.Count} {(mano.Count == 1 ? "carta" : "cartas")}", Titulo);
        if (mano.Count == 0)
        {
            Escribir("No hay cartas disponibles.");
            return;
        }

        Paneles(mano.Select((carta, i) => Ficha(carta, i == foco)).ToArray());
    }

    public void MostrarEsperaRival(MesaVisible mesa, string rival, IReadOnlyList<CartaVisible> cartas)
    {
        Encabezado(mesa, $"{TextoVisible(rival)} está eligiendo");
        Paneles(PanelInfo("Tu mano", ResumenMano(cartas)), PanelInfo("Marcador", Marcador(mesa)));
        Pie("Esperá la solicitud para responder.");
    }

    public CartaVisible PedirCarta(IReadOnlyList<CartaVisible> mano)
    {
        CartaVisible[] opciones = CopiarOpciones(mano);
        int elegida = Elegir(opciones.Length, foco =>
        {
            Encabezado((string?)null, "Elegí una carta");
            MostrarMano(opciones, foco);
            Pie(Controles);
        });
        return opciones[elegida];
    }

    public CartaVisible PedirCarta(MesaVisible mesa, IReadOnlyList<CartaVisible> cartas, bool abre)
    {
        CartaVisible[] opciones = CopiarOpciones(cartas);
        int elegida = Elegir(opciones.Length, foco =>
        {
            string estado = abre
                ? $"Rondas ganadas · {Rondas(mesa)}"
                : $"Te toca responder · Marcador · Combates {Combates(mesa)} · Rondas {Rondas(mesa)}";
            Encabezado(mesa, estado);
            MostrarMano(opciones, foco, abre ? "Tu mano" : "Cartas disponibles");
            Pie(abre ? $"Tu turno\nDespués elegís el atributo.\n{Controles}" : Controles);
        });
        return opciones[elegida];
    }

    public string PedirAtributo(IReadOnlyList<string>? permitidos = null, MesaVisible? mesa = null)
    {
        string[] opciones = permitidos?.ToArray() ?? Atributos;
        if (opciones.Length == 0)
            throw new InvalidOperationException("Se solicitó un atributo sin opciones disponibles.");

        int elegida = Elegir(opciones.Length, foco =>
        {
            if (mesa is null)
                Encabezado((string?)null, "Elegí un atributo");
            else
                Encabezado(mesa, "Elegí un atributo");

            Paneles(PanelInfo("Atributo del combate", ListaOpciones(opciones, foco)));
            Pie(Controles);
        });
        return opciones[elegida];
    }

    public void MostrarEnvio(MesaVisible mesa, EstadoEnvio estado)
    {
        Encabezado(mesa);
        (string etiqueta, string mensaje) = estado switch
        {
            EstadoEnvio.Enviando => ("Envío en curso", "Enviando tu elección…"),
            EstadoEnvio.Recibido => ("Recepción confirmada", "Elección recibida. Esperando el resultado."),
            EstadoEnvio.Pendiente => ("Confirmación pendiente", "Comprobando el recibo. La elección sigue en trámite."),
            _ => throw new ArgumentOutOfRangeException(nameof(estado))
        };
        Escribir("Elección en curso", Titulo);
        if (estado == EstadoEnvio.Pendiente)
            Pie($"{etiqueta} · {TextoVisible(mensaje)}");
        else
            Escribir($"{etiqueta} · {TextoVisible(mensaje)}");
    }

    public void MostrarCombate(MesaVisible mesa, CombateVisible combate)
    {
        Encabezado(mesa, $"Rondas · {Rondas(mesa)}");
        MostrarCombate(combate);
    }

    public void MostrarCombate(CombateVisible combate)
    {
        Escribir($"Atributo · {TextoVisible(combate.Atributo)}", Titulo);
        bool ganaJ1 = combate.GanaJ1 ?? combate.J1.Jugador == combate.Ganador;
        Paneles(FichaJugada(combate.J1, ganaJ1), FichaJugada(combate.J2, !ganaJ1));

        string separador = terminal.Profile.Width < 54 ? "\n" : " · ";
        string siguiente = combate.ProximoEnElegir is { } nombre
            ? $"\nPróximo combate{separador}{TextoVisible(nombre)} elige el atributo" : "";
        Pie($"Combates de la ronda{separador}{TextoVisible(combate.J1.Jugador)} {combate.CombatesGanadosJ1} / {TextoVisible(combate.J2.Jugador)} {combate.CombatesGanadosJ2}{siguiente}");
    }

    public void MostrarCierreRonda(MesaVisible mesa, string ganador, string? proximoMano)
    {
        Encabezado(mesa.Local, $"Ronda {mesa.Ronda} finalizada");
        Escribir($"{TextoVisible(ganador)} gana la ronda", Titulo);
        Escribir($"Combates · {Combates(mesa)}");
        Escribir($"Rondas ganadas · {Rondas(mesa)} · Meta 15");
        if (proximoMano is not null)
            Pie($"{TextoVisible(proximoMano)} abre la ronda {mesa.Ronda + 1}");
    }

    public void MostrarCierrePartida(string local, string ganador, JugadorVisible j1, JugadorVisible j2)
    {
        Encabezado(local, "Partida finalizada");
        Escribir($"{TextoVisible(ganador)} gana la partida", Titulo);
        var planilla = new Table { Border = TableBorder.Square, BorderStyle = Borde, Expand = true };
        planilla.AddColumn("Jugador").AddColumn("Resultado");
        planilla.AddRow(new Text(TextoVisible(j1.Nombre)), new Text($"{j1.RondasGanadas} rondas ganadas"));
        planilla.AddRow(new Text(TextoVisible(j2.Nombre)), new Text($"{j2.RondasGanadas} rondas ganadas"));
        Renderizar(planilla);
        Pie("Pulsá Enter para salir.");
    }

    public void MostrarSuspension(MesaVisible mesa, string motivo, bool confirmada)
    {
        Encabezado(mesa);
        Escribir("Jugada temporalmente indisponible", Titulo);
        string estado = confirmada ? "Pausa confirmada" : "Sin comunicación · Estado actual sin confirmar";
        Escribir($"{TextoVisible(estado)} · {TextoVisible(motivo)}");
        Pie($"Último marcador confirmado · {Marcador(mesa)}");
    }

    public void MostrarInterrupcion(string local, string causa, string efecto, bool confirmada)
    {
        Encabezado(local, $"Sesión de {TextoVisible(local)}");
        Escribir("Cierre de sesión o partida", Titulo);
        Escribir($"{(confirmada ? "Final confirmado" : "Salida local")} · {TextoVisible(causa)}");
        Pie(TextoVisible(efecto));
    }

    public void MostrarMarcador(JugadorVisible j1, JugadorVisible j2, int ronda) =>
        Encabezado(null, $"Ronda {ronda}", $"Rondas ganadas · {TextoVisible(j1.Nombre)} {j1.RondasGanadas} - {j2.RondasGanadas} {TextoVisible(j2.Nombre)}");

    public void MostrarEsperando(string mensaje)
    {
        if (ultimaEspera == mensaje)
            return;

        MostrarEstado("En espera", mensaje);
        ultimaEspera = mensaje;
    }

    public void MostrarGanadorRonda(JugadorVisible ganador) =>
        MostrarEstado($"Ganador de la ronda: {TextoVisible(ganador.Nombre)}", $"{ganador.RondasGanadas} rondas ganadas.");

    public void MostrarGanadorPartida(JugadorVisible ganador) =>
        MostrarEstado($"Ganador de la partida: {TextoVisible(ganador.Nombre)}", $"{ganador.RondasGanadas} rondas ganadas.");

    public void MostrarError(string mensaje) =>
        Escribir($"ERROR: {TextoVisible(mensaje)}", new Style(Color.Red, decoration: Decoration.Bold));

    public void MostrarEstado(string titulo, string mensaje)
    {
        Encabezado((string?)null, titulo);
        Escribir(TextoVisible(mensaje));
    }

    private void Encabezado(MesaVisible mesa, string? detalle = null) =>
        Encabezado(mesa.Local, $"Ronda {mesa.Ronda} · Combate {mesa.Combate}", detalle);

    private void Encabezado(string? local, string situacion, string? detalle = null)
    {
        if (limpiarPantalla)
            terminal.Clear(home: true);

        string identidad = local is null ? "GUERRA" : $"GUERRA / {TextoVisible(local)}";
        string[] partes = detalle is null ? [identidad, TextoVisible(situacion)] : [identidad, TextoVisible(situacion), TextoVisible(detalle)];
        string linea = string.Join("  │  ", partes);
        terminal.WriteLine();
        Escribir(linea.Length >= terminal.Profile.Width ? string.Join('\n', partes) : linea, Barra);
        Renderizar(new Rule { Style = Borde });
    }

    private void Pie(string mensaje)
    {
        Renderizar(new Rule { Style = Borde });
        Escribir(mensaje, Barra);
    }

    private void Paneles(params Panel[] paneles)
    {
        int ancho = terminal.Profile.Width;
        if (!salidaLineal && paneles.Length > 1 && ancho >= paneles.Length * 27)
        {
            int espacio = ancho - paneles.Length + 1;
            for (int indice = 0; indice < paneles.Length; indice++)
                paneles[indice].Width = espacio / paneles.Length + (indice < espacio % paneles.Length ? 1 : 0);

            Renderizar(new Columns(paneles) { Padding = new Padding(0, 0, 1, 0) });
            return;
        }

        foreach (Panel panel in paneles)
            Renderizar(panel);
    }

    private static Panel PanelInfo(string titulo, string detalle) =>
        PanelInfo(titulo, new Rows(detalle.Split('\n').Select(linea => new Text(TextoVisible(linea)))));

    private static Panel PanelInfo(string titulo, IRenderable detalle) => new(detalle)
    {
        Header = new PanelHeader($"[bold #bde88c]{Markup.Escape(TextoVisible(titulo))}[/]"),
        Border = BoxBorder.Square,
        BorderStyle = Borde,
        Expand = true
    };

    private static Panel Ficha(CartaVisible carta, bool enfocada)
    {
        var valores = new Grid { Expand = true };
        valores.AddColumn(new GridColumn());
        valores.AddColumn(new GridColumn { Alignment = Justify.Right });
        foreach ((string atributo, int valor) in carta.Atributos)
            valores.AddRow(new Text(TextoVisible(atributo)), new Text(valor.ToString(CultureInfo.InvariantCulture), Titulo));

        var ficha = new Rows(Opcion(carta.Nombre, enfocada), valores);
        return new Panel(ficha) { Border = BoxBorder.Square, BorderStyle = enfocada ? Titulo : Borde, Expand = true };
    }

    private static Panel FichaJugada(CartaJugada jugada, bool ganadora)
    {
        var resultado = new Rows(
            new Text(TextoVisible($"{jugada.Jugador} · {jugada.Carta}")),
            new Text(jugada.Valor.ToString(CultureInfo.InvariantCulture), Titulo),
            new Text(ganadora ? "Ganador" : "", Titulo));
        return new Panel(resultado)
        {
            Border = BoxBorder.Square,
            BorderStyle = ganadora ? Titulo : Borde,
            Expand = true
        };
    }

    private static string ResumenMano(IReadOnlyList<CartaVisible> cartas) => cartas.Count == 0
        ? "No hay cartas disponibles."
        : string.Join(" · ", cartas.Select(c => TextoVisible(c.Nombre)));

    private static string Rondas(MesaVisible mesa) =>
        $"{TextoVisible(mesa.J1.Nombre)} {mesa.J1.RondasGanadas} / {TextoVisible(mesa.J2.Nombre)} {mesa.J2.RondasGanadas}";

    private static string Combates(MesaVisible mesa) =>
        $"{TextoVisible(mesa.J1.Nombre)} {mesa.CombatesJ1} / {TextoVisible(mesa.J2.Nombre)} {mesa.CombatesJ2}";

    private static string Marcador(MesaVisible mesa) =>
        $"Combates · {Combates(mesa)}\nRondas · {Rondas(mesa)}";

    private static CartaVisible[] CopiarOpciones(IReadOnlyList<CartaVisible> mano) =>
        mano.Count > 0 ? mano.ToArray() : throw new InvalidOperationException("Se solicitó una carta con la mano vacía.");

    private static Rows ListaOpciones(IEnumerable<string> opciones, int foco) =>
        new(opciones.Select((opcion, i) => Opcion(opcion, i == foco)));

    private static Text Opcion(string opcion, bool enfocada) =>
        enfocada ? new Text($"> {TextoVisible(opcion)}", Barra) : new Text($"  {TextoVisible(opcion)}");

    private int Elegir(int cantidad, Action<int> dibujar)
    {
        int foco = 0;
        dibujar(foco);
        while (true)
        {
            ConsoleKey tecla = LeerTecla();
            if (tecla == ConsoleKey.Enter)
                return foco;

            int paso = tecla switch
            {
                ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.K or ConsoleKey.H => -1,
                ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L => 1,
                _ => 0
            };

            int nuevo = Math.Clamp(foco + paso, 0, cantidad - 1);
            if (nuevo != foco)
            {
                foco = nuevo;
                dibujar(foco);
            }
        }
    }

    private ConsoleKey LeerTecla()
    {
        if (leerTeclas)
            return Console.ReadKey(intercept: true).Key;

        // Con entrada por líneas, una línea vacía equivale a presionar Enter.
        string linea = entrada.ReadLine()
            ?? throw new EndOfStreamException("Se cerró la entrada de consola.");

        return linea.Trim().ToLowerInvariant() switch
        {
            "" => ConsoleKey.Enter,
            "w" => ConsoleKey.W,
            "a" => ConsoleKey.A,
            "s" => ConsoleKey.S,
            "d" => ConsoleKey.D,
            "h" => ConsoleKey.H,
            "j" => ConsoleKey.J,
            "k" => ConsoleKey.K,
            "l" => ConsoleKey.L,
            _ => ConsoleKey.NoName
        };
    }

    // Los caracteres de control y formato se reemplazan por espacios al mostrar texto.
    private static string TextoVisible(string texto) =>
        string.Concat(texto.Select(caracter =>
            char.IsControl(caracter) || char.GetUnicodeCategory(caracter) is UnicodeCategory.Format
                or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator
                ? ' ' : caracter));

    private void Escribir(string texto, Style? estilo = null) =>
        Renderizar(new Rows(texto.Split('\n').Select(linea => new Text(TextoVisible(linea), estilo))));

    private void Renderizar(IRenderable contenido)
    {
        terminal.Write(contenido);
        ultimaEspera = null;
    }
}
