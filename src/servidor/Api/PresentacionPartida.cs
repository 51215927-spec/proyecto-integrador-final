using System.Text.Json.Serialization;

namespace Proyecto_Integrador_Final.Api;

internal sealed record SolicitudVisible(Guid IdSolicitud, string Tipo,
    int[] IdsPersonajePermitidos, string[] AtributosPermitidos);
internal sealed record ResultadoVisible(int NumeroRonda, int NumeroCombate,
    Dictionary<string, CartaVisible> Cartas, string Atributo, Dictionary<string, int> Puntajes,
    string GanadorCombate, string? GanadorRonda);
internal sealed record PresentacionVisible(Guid IdPartida, int Revision, string Fase, bool Pausada,
    string RolPropio, Dictionary<string, string?> Nombres, int? NumeroRonda, int? NumeroCombate,
    Dictionary<string, int> Marcador, CartaVisible[] Mano, SolicitudVisible? Solicitud,
    ResultadoVisible[] Resultados,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MotivoFin,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Ganador);
