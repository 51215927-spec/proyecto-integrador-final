using System.Text.Json.Serialization;

namespace Proyecto_Integrador_Final.Cliente;

// Estos mensajes son propios del cliente y no dependen del ensamblado del servidor.
internal sealed record IncorporacionCliente(Guid IdSesion, string Nombre);
internal sealed record SesionConfirmada(
    [property: JsonRequired] string Rol,
    [property: JsonRequired] Guid IdSesion,
    [property: JsonRequired] DateTimeOffset VenceUtc);
// Las opciones se reciben del servidor; JsonRequired impide aceptar campos omitidos como valores por defecto.
internal sealed record SolicitudCliente(
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] string Tipo,
    [property: JsonRequired] int[] IdsPersonajePermitidos,
    [property: JsonRequired] string[] AtributosPermitidos);
// Las cartas rivales solo aparecen en el historial de combates ya resueltos.
internal sealed record ResultadoCliente(
    [property: JsonRequired] int NumeroRonda,
    [property: JsonRequired] int NumeroCombate,
    [property: JsonRequired] Dictionary<string, CartaVisible> Cartas,
    [property: JsonRequired] string Atributo,
    [property: JsonRequired] Dictionary<string, int> Puntajes,
    [property: JsonRequired] string GanadorCombate,
    [property: JsonRequired] string? GanadorRonda);
// Cada presentación reemplaza el modelo completo, incluso cuando conserva la revisión.
internal sealed record PresentacionCliente(
    [property: JsonRequired] Guid IdPartida,
    [property: JsonRequired] int Revision,
    [property: JsonRequired] string Fase,
    [property: JsonRequired] bool Pausada,
    [property: JsonRequired] string RolPropio,
    [property: JsonRequired] Dictionary<string, string?> Nombres,
    [property: JsonRequired] int? NumeroRonda,
    [property: JsonRequired] int? NumeroCombate,
    // Mano y solicitud son privadas de la plaza; los motivos de fin pueden omitirse antes del cierre.
    [property: JsonRequired] Dictionary<string, int> Marcador,
    [property: JsonRequired] CartaVisible[] Mano,
    [property: JsonRequired] SolicitudCliente? Solicitud,
    [property: JsonRequired] ResultadoCliente[] Resultados,
    string? MotivoFin,
    string? Ganador);
// Una elección es inmutable: el mismo UUID nunca se reutiliza con un cuerpo distinto.
internal sealed record EleccionCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionEsperada, int IdPersonaje, string? Atributo);
// El recibo acredita aceptación de la elección, sin anticipar el resultado del combate.
internal sealed record ReciboCliente(
    [property: JsonRequired] Guid IdOperacion,
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] int RevisionConfirmada);
internal sealed record ErrorCliente(string Codigo, string Mensaje, string IdSeguimiento);
