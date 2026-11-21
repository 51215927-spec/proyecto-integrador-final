using System.Text.Json.Serialization;

namespace Proyecto_Integrador_Final.Cliente;

internal sealed record IncorporacionCliente(Guid IdSesion, string Nombre);
internal sealed record SesionConfirmada(
    [property: JsonRequired] string Rol,
    [property: JsonRequired] Guid IdSesion,
    [property: JsonRequired] DateTimeOffset VenceUtc);
internal sealed record SolicitudCliente(
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] string Tipo,
    [property: JsonRequired] int[] IdsPersonajePermitidos,
    [property: JsonRequired] string[] AtributosPermitidos);
internal sealed record ResultadoCliente(
    [property: JsonRequired] int NumeroRonda,
    [property: JsonRequired] int NumeroCombate,
    [property: JsonRequired] Dictionary<string, CartaVisible> Cartas,
    [property: JsonRequired] string Atributo,
    [property: JsonRequired] Dictionary<string, int> Puntajes,
    [property: JsonRequired] string GanadorCombate,
    [property: JsonRequired] string? GanadorRonda);
internal sealed record PresentacionCliente(
    [property: JsonRequired] Guid IdPartida,
    [property: JsonRequired] int Revision,
    [property: JsonRequired] string Fase,
    [property: JsonRequired] bool Pausada,
    [property: JsonRequired] string RolPropio,
    [property: JsonRequired] Dictionary<string, string?> Nombres,
    [property: JsonRequired] int? NumeroRonda,
    [property: JsonRequired] int? NumeroCombate,
    [property: JsonRequired] Dictionary<string, int> Marcador,
    [property: JsonRequired] CartaVisible[] Mano,
    [property: JsonRequired] SolicitudCliente? Solicitud,
    [property: JsonRequired] ResultadoCliente[] Resultados,
    string? MotivoFin,
    string? Ganador);
internal sealed record EleccionCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionEsperada, int IdPersonaje, string? Atributo);
internal sealed record ReciboCliente(
    [property: JsonRequired] Guid IdOperacion,
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] int RevisionConfirmada);
internal sealed record ErrorCliente(string Codigo, string Mensaje, string IdSeguimiento);
