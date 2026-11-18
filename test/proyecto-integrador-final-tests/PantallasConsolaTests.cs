using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class PantallasConsolaTests
{
    private static readonly MesaVisible Mesa = new("Lucía", new("Lucía", 4), new("Mateo", 3), 8, 1, 0, 0);
    private static readonly CartaVisible[] Cartas = [
        new(1, "Lysander", new Dictionary<string, int> { ["Fuerza"] = 700, ["Velocidad"] = 550 }),
        new(2, "Reched", new Dictionary<string, int> { ["Fuerza"] = 350, ["Velocidad"] = 700 }),
        new(3, "Vigía", new Dictionary<string, int> { ["Fuerza"] = 500, ["Velocidad"] = 350 })
    ];

    [Fact]
    public void PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarPreparacion("Lucía", "Preparando la partida…");
        interfaz.MostrarEsperaInicial("Lucía", "Lucía presente · Mateo por incorporarse", "Falta Mateo");
        interfaz.MostrarInicioRonda(Mesa, "Lucía", Cartas);

        string salida = texto.ToString();
        Assert.Contains("Jugador local", salida);
        Assert.Contains("Estado", salida);
        Assert.Contains("Participantes", salida);
        Assert.Contains("A cargo del operador", salida);
        Assert.Contains("Lucía abre la ronda", salida);
        Assert.Contains("Tu mano", salida);
        Assert.Contains("Rondas ganadas", salida);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(35, false)]
    public void Apertura_UsaMesaConFichasEncabezadoYPie(int ancho, bool paralelas)
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader("2"), texto, ancho);

        Assert.Same(Cartas[1], interfaz.PedirCarta(Mesa, Cartas, abre: true));

        string salida = texto.ToString();
        Assert.Contains("GUERRA / Lucía", salida);
        Assert.Contains("Ronda 8 · Combate 1", salida);
        Assert.Contains("Tu mano", salida);
        Assert.Contains("Tu turno", salida);
        Assert.Contains("Después elegís el atributo", salida.ReplaceLineEndings(" "));
        Assert.Contains("┌", salida);
        Assert.Equal(paralelas, salida.Split('\n').Any(l => l.Contains("Lysander") && l.Contains("Reched")));
    }

    [Fact]
    public void EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(new StringReader("1"), texto, 100);

        interfaz.MostrarEsperaRival(Mesa, "Mateo", Cartas);
        Assert.Same(Cartas[0], interfaz.PedirCarta(Mesa, Cartas, abre: false));

        string salida = texto.ToString();
        Assert.Contains("Mateo está eligiendo", salida);
        Assert.Contains("Esperá la solicitud para responder", salida);
        Assert.Contains("Cartas disponibles", salida);
        Assert.Contains("Te toca responder", salida);
        Assert.Contains("Marcador", salida);
        Assert.DoesNotContain("atributo", salida, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Enviando);
        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Pendiente);
        interfaz.MostrarEnvio(Mesa, EstadoEnvio.Recibido);
        interfaz.MostrarSuspension(Mesa, "Mateo perdió la sesión", true);
        interfaz.MostrarSuspension(Mesa, "No se pudo contactar al servidor", false);
        interfaz.MostrarInterrupcion("Lucía", "Mateo abandonó la partida", "La partida terminó", true);
        interfaz.MostrarInterrupcion("Lucía", "Cierre solicitado", "La partida queda guardada", false);

        string salida = texto.ToString();
        foreach (string esperado in new[] { "Envío en curso", "Confirmación pendiente", "Recepción confirmada",
            "Pausa confirmada", "Estado actual sin confirmar", "Último marcador confirmado", "Final confirmado", "Salida local" })
            Assert.Contains(esperado, salida);
    }

    [Fact]
    public void ResultadoYRondas_UsanDueloVictoriaYPlanilla()
    {
        using var texto = new StringWriter();
        var interfaz = new InterfazConsola(TextReader.Null, texto, 100);

        interfaz.MostrarCombate(Mesa, new("Fuerza", new("Lucía", "Lysander", 700),
            new("Mateo", "Reched", 350), "Lucía", 1, 0, "Mateo"));
        interfaz.MostrarCierreRonda(Mesa with { CombatesJ1 = 2, CombatesJ2 = 1 }, "Lucía", "Mateo");
        interfaz.MostrarCierrePartida("Lucía", "Lucía", new("Lucía", 15), new("Mateo", 11));

        string salida = texto.ToString();
        Assert.Contains("Atributo · Fuerza", salida);
        Assert.Contains("Lucía · Lysander", salida);
        Assert.Contains("Mateo · Reched", salida);
        Assert.Contains("Ganador", salida);
        Assert.Contains("Mateo elige el atributo", salida);
        Assert.Contains("Lucía gana la ronda", salida);
        Assert.Contains("Mateo abre la ronda 9", salida);
        Assert.Contains("Lucía gana la partida", salida);
        Assert.Contains("15 rondas ganadas", salida);
        Assert.Contains("11 rondas ganadas", salida);
        Assert.Contains("Pulsá Enter para salir", salida);
    }
}
