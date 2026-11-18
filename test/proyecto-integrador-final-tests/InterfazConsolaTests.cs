using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class InterfazConsolaTests
{
    private static CartaVisible Carta(int id, string nombre) => new(id, nombre,
        new Dictionary<string, int>
        {
            ["Fuerza"] = 750, ["Velocidad"] = 550, ["Resistencia"] = 500,
            ["Inteligencia"] = 250, ["Energía"] = 500, ["Habilidad"] = 450
        });

    [Fact]
    public void PedirCarta_ReintentaEntradasInvalidasYConservaLaMano()
    {
        using var entrada = new StringReader("2\ntexto\nl\n\nsobra\n");
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(entrada, salida, ancho: 30);
        CartaVisible[] mano = [Carta(15, "Maga"), Carta(30, "Arquero")];

        Assert.Same(mano[1], interfaz.PedirCarta(mano));
        Assert.Equal(2, mano.Length);
        Assert.Equal("sobra", entrada.ReadLine());
        Assert.Contains("> Arquero", salida.ToString());
        Assert.Contains("Inteligencia", salida.ToString());
    }

    [Fact]
    public void ManoVacia_SePuedeMostrarPeroNoSolicitar()
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida);

        interfaz.MostrarMano([]);

        Assert.Contains("No hay cartas disponibles", salida.ToString());
        Assert.Throws<InvalidOperationException>(() => interfaz.PedirCarta([]));
    }

    [Theory]
    [InlineData(0, "Fuerza")]
    [InlineData(1, "Velocidad")]
    [InlineData(2, "Resistencia")]
    [InlineData(3, "Inteligencia")]
    [InlineData(4, "Energía")]
    [InlineData(5, "Habilidad")]
    public void PedirAtributo_OfreceLosSeisAtributos(int pasos, string esperado)
    {
        var interfaz = new InterfazConsola(new StringReader(string.Concat(Enumerable.Repeat("s\n", pasos)) + "\n"), TextWriter.Null);

        Assert.Equal(esperado, interfaz.PedirAtributo());
    }

    [Fact]
    public void PedirAtributo_RespetaLasOpcionesDeLaSolicitud()
    {
        var interfaz = new InterfazConsola(new StringReader("2\n\n"), TextWriter.Null);

        Assert.Equal("Habilidad", interfaz.PedirAtributo(["Habilidad"]));
    }

    [Fact]
    public void FinDeEntrada_TerminaLaSolicitud()
    {
        var interfaz = new InterfazConsola(new StringReader("s\n"), TextWriter.Null);

        Assert.Throws<EndOfStreamException>(() => interfaz.PedirCarta([Carta(1, "Maga")]));
        Assert.Throws<EndOfStreamException>(() => interfaz.PedirAtributo());
    }

    [Theory]
    [InlineData(25, false)]
    [InlineData(100, true)]
    public void MostrarMano_AdaptaLasFichasAlAncho(int ancho, bool enParalelo)
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida, ancho);

        interfaz.MostrarMano([Carta(1, "Maga\u001b"), Carta(2, "Arquero")]);

        string resultado = salida.ToString();
        Assert.DoesNotContain('\u001b', resultado);
        Assert.Contains("Habilidad", resultado);
        Assert.Equal(enParalelo, resultado.Split('\n').Any(
            linea => linea.Contains("Maga") && linea.Contains("Arquero")));
    }

    [Theory]
    [InlineData(30)]
    [InlineData(100)]
    public void MostrarCombate_PresentaElResultadoConfirmado(int ancho)
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida, ancho);

        interfaz.MostrarCombate(new("Fuerza", new("Lucía", "Maga", 750),
            new("Mateo", "Arquero", 750), "Mateo", 0, 1, "Mateo"));

        string resultado = salida.ToString();
        Assert.Contains("Fuerza", resultado);
        Assert.Contains("Maga", resultado);
        Assert.Contains("Arquero", resultado);
        Assert.Contains("750", resultado);
        Assert.Contains("Ganador", resultado);
        Assert.Contains("Lucía 0 / Mateo 1", resultado);
        Assert.Contains("Mateo elige el atributo", resultado);
    }

    [Fact]
    public void PresentaMarcadorEstadosYCierresConLosNombresRecibidos()
    {
        using var salida = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, salida);
        var lucia = new JugadorVisible("Lucía", 14);
        var mateo = new JugadorVisible("Mateo", 15);

        interfaz.MostrarEstado("Envío", "Confirmación pendiente");
        interfaz.MostrarEsperando("Esperando a Mateo");
        interfaz.MostrarEsperando("Esperando a Mateo");
        interfaz.MostrarMarcador(lucia, mateo, 29);
        interfaz.MostrarGanadorRonda(mateo);
        interfaz.MostrarGanadorPartida(mateo);
        interfaz.MostrarError("Servidor inaccesible");

        string resultado = salida.ToString();
        Assert.Contains("Confirmación pendiente", resultado);
        Assert.Single(resultado.Split('\n').Where(linea => linea.Contains("Esperando a Mateo")));
        Assert.Contains("Ronda 29", resultado);
        Assert.Contains("Lucía 14 - 15 Mateo", resultado);
        Assert.Contains("Ganador de la ronda: Mateo", resultado);
        Assert.Contains("Ganador de la partida: Mateo", resultado);
        Assert.Contains("ERROR: Servidor inaccesible", resultado);
    }
}
