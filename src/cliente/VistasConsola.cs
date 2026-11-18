namespace Proyecto_Integrador_Final.Cliente;

internal sealed record CartaVisible(
    int IdPersonaje,
    string Nombre,
    IReadOnlyDictionary<string, int> Atributos);

internal sealed record JugadorVisible(string Nombre, int RondasGanadas);

internal sealed record CartaJugada(string Jugador, string Carta, int Valor);

internal sealed record CombateVisible(
    string Atributo,
    CartaJugada J1,
    CartaJugada J2,
    string Ganador,
    int CombatesGanadosJ1,
    int CombatesGanadosJ2,
    string? ProximoEnElegir);
