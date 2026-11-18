using System.Globalization;
using Utilidades;

namespace Proyecto_Integrador_Final.Cliente;

internal sealed class InterfazConsola
{
    private static readonly string[] Atributos =
        ["Fuerza", "Velocidad", "Resistencia", "Inteligencia", "Energía", "Habilidad"];

    private readonly TextReader entrada;
    private readonly TextWriter? salida;
    private readonly int? ancho;
    private string? ultimaEspera;

    public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)
    {
        this.entrada = entrada ?? Console.In;
        this.salida = salida;
        this.ancho = ancho;
    }

    public void MostrarMano(IReadOnlyList<CartaVisible> mano)
    {
        Escribir("— Mano propia —");
        if (mano.Count == 0)
        {
            Escribir("No hay cartas disponibles.");
            return;
        }

        var fichas = mano.Select((carta, indice) =>
            new[] { $"[{indice + 1}] {TextoVisible(carta.Nombre)}" }
                .Concat(carta.Atributos.Select(atributo =>
                    $"{TextoVisible(atributo.Key)}: {atributo.Value}"))
                .ToArray())
            .ToArray();

        MostrarPaneles(fichas);
    }

    public CartaVisible PedirCarta(IReadOnlyList<CartaVisible> mano)
    {
        CartaVisible[] opciones = mano.ToArray();
        if (opciones.Length == 0)
            throw new InvalidOperationException("Se solicitó una carta con la mano vacía.");

        MostrarMano(opciones);
        return opciones[LeerOpcion("Elegí una carta", opciones.Length)];
    }

    public string PedirAtributo(IReadOnlyList<string>? permitidos = null)
    {
        string[] opciones = permitidos?.ToArray() ?? Atributos;
        if (opciones.Length == 0)
            throw new InvalidOperationException("Se solicitó un atributo sin opciones disponibles.");

        for (int indice = 0; indice < opciones.Length; indice++)
            Escribir($"[{indice + 1}] {TextoVisible(opciones[indice])}");

        return opciones[LeerOpcion("Elegí un atributo", opciones.Length)];
    }

    public void MostrarMarcador(JugadorVisible j1, JugadorVisible j2, int ronda) =>
        Escribir($"Ronda {ronda} | Rondas ganadas: {TextoVisible(j1.Nombre)} {j1.RondasGanadas} - {j2.RondasGanadas} {TextoVisible(j2.Nombre)}");

    public void MostrarEsperando(string mensaje)
    {
        string texto = TextoVisible(mensaje);
        if (ultimaEspera == texto)
            return;

        Escribir(texto);
        ultimaEspera = texto;
    }

    public void MostrarCombate(CombateVisible combate)
    {
        Escribir($"— Resultado del combate · {TextoVisible(combate.Atributo)} —");
        MostrarPaneles([FichaJugada(combate.J1), FichaJugada(combate.J2)]);
        Escribir($"Ganador: {TextoVisible(combate.Ganador)}");
        Escribir($"Combates: {TextoVisible(combate.J1.Jugador)} {combate.CombatesGanadosJ1} - {combate.CombatesGanadosJ2} {TextoVisible(combate.J2.Jugador)}");
        if (combate.ProximoEnElegir is { } nombre)
            Escribir($"Próximo atributo: {TextoVisible(nombre)}");
    }

    public void MostrarGanadorRonda(JugadorVisible ganador) =>
        Escribir($"Ganador de la ronda: {TextoVisible(ganador.Nombre)}");

    public void MostrarGanadorPartida(JugadorVisible ganador) =>
        Escribir($"Ganador de la partida: {TextoVisible(ganador.Nombre)}");

    public void MostrarError(string mensaje)
    {
        string texto = TextoVisible(mensaje);
        if (salida is null && !Console.IsOutputRedirected)
        {
            ultimaEspera = null;
            DecoracionConsola.ImprimirMesajeDeError(texto);
            return;
        }

        Escribir($"ERROR: {texto}");
    }

    public void MostrarEstado(string titulo, string mensaje)
    {
        Escribir($"— {TextoVisible(titulo)} —");
        Escribir(TextoVisible(mensaje));
    }

    private int LeerOpcion(string accion, int cantidad)
    {
        while (true)
        {
            Escribir($"{accion} [1–{cantidad}]:");
            string linea = entrada.ReadLine()
                ?? throw new EndOfStreamException("Se cerró la entrada de consola.");

            if (int.TryParse(linea, NumberStyles.Integer, CultureInfo.InvariantCulture, out int opcion)
                && opcion >= 1 && opcion <= cantidad)
                return opcion - 1;

            Escribir($"Elegí un número entre 1 y {cantidad}.");
        }
    }

    private void MostrarPaneles(string[][] paneles)
    {
        const string separador = "  |  ";
        int[] columnas = paneles.Select(panel => panel.Max(linea => linea.Length)).ToArray();
        int anchoNecesario = columnas.Sum() + (paneles.Length - 1) * separador.Length;
        if (paneles.Length == 1 || anchoNecesario >= AnchoDisponible()
            || paneles.Any(panel => panel.Any(linea => linea.Any(caracter => caracter > '\u00ff'))))
        {
            foreach (string[] panel in paneles)
            {
                Escribir("");
                foreach (string linea in panel)
                    Escribir(linea);
            }
            return;
        }

        int filas = paneles.Max(panel => panel.Length);
        for (int fila = 0; fila < filas; fila++)
        {
            var celdas = paneles.Select((panel, columna) =>
                (fila < panel.Length ? panel[fila] : "").PadRight(columnas[columna]));
            Escribir(string.Join(separador, celdas).TrimEnd());
        }
    }

    private int AnchoDisponible()
    {
        if (ancho is { } fijo)
            return fijo;
        if (salida is not null || Console.IsOutputRedirected)
            return 0;

        try
        {
            return Console.WindowWidth;
        }
        catch (IOException)
        {
            return 0;
        }
    }

    private static string[] FichaJugada(CartaJugada jugada) =>
        [TextoVisible(jugada.Jugador), TextoVisible(jugada.Carta), $"Valor: {jugada.Valor}"];

    private static string TextoVisible(string texto) =>
        string.Concat(texto.Select(caracter =>
            char.IsControl(caracter) || char.GetUnicodeCategory(caracter) is
                UnicodeCategory.Format or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator
                ? ' ' : caracter));

    private void Escribir(string texto)
    {
        if (salida is null)
            MyConsole.WriteLine(texto);
        else
            salida.WriteLine(texto);

        ultimaEspera = null;
    }
}
