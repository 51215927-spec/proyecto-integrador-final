namespace Proyecto_Integrador_Final.Api;

internal sealed class PersonajePartida
{
    public Guid IdPartida { get; set; }
    public int IdPersonaje { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public required string Ascii { get; set; }
    public int Fuerza { get; set; }
    public int Velocidad { get; set; }
    public int Resistencia { get; set; }
    public int Inteligencia { get; set; }
    public int Energia { get; set; }
    public int Habilidad { get; set; }
    public string Ubicacion { get; set; } = "mazo";
    public int Orden { get; set; }

    internal static PersonajePartida Copiar(Guid partida, PersonajeCatalogo origen) => new()
    {
        IdPartida = partida, IdPersonaje = origen.Id, Nombre = origen.Nombre,
        Descripcion = origen.Descripcion, Ascii = origen.Ascii, Fuerza = origen.Fuerza,
        Velocidad = origen.Velocidad, Resistencia = origen.Resistencia,
        Inteligencia = origen.Inteligencia, Energia = origen.Energia, Habilidad = origen.Habilidad
    };

    internal Personaje Dominio() =>
        new(Nombre, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);

    internal CartaVisible Vista() => new(IdPersonaje, Nombre, Descripcion, Ascii, new()
    {
        ["Fuerza"] = Fuerza, ["Velocidad"] = Velocidad, ["Resistencia"] = Resistencia,
        ["Inteligencia"] = Inteligencia, ["Energía"] = Energia, ["Habilidad"] = Habilidad
    });
}

internal sealed record CartaVisible(int IdPersonaje, string Nombre, string Descripcion, string Ascii,
    Dictionary<string, int> Atributos);

internal sealed class RondaGuardada
{
    public Guid IdPartida { get; set; }
    public int Numero { get; set; }
    public required string Mano { get; set; }
    public int VictoriasJ1 { get; set; }
    public int VictoriasJ2 { get; set; }
    public string? Ganador { get; set; }
}

internal sealed class CombateGuardado
{
    public Guid IdPartida { get; set; }
    public int NumeroRonda { get; set; }
    public int Numero { get; set; }
    public required string Elector { get; set; }
    public int? IdCartaElector { get; set; }
    public int? IdCartaRival { get; set; }
    public string? Atributo { get; set; }
    public int? PuntajeElector { get; set; }
    public int? PuntajeRival { get; set; }
    public string? Ganador { get; set; }
}

internal sealed class SolicitudGuardada
{
    public Guid Id { get; set; }
    public Guid IdPartida { get; set; }
    public int NumeroRonda { get; set; }
    public int NumeroCombate { get; set; }
    public required string Rol { get; set; }
    public required string Tipo { get; set; }
    public bool Consumida { get; set; }
}

internal sealed record Eleccion(Guid IdOperacion, Guid IdSolicitud, int? RevisionEsperada,
    int? IdPersonaje, string? Atributo);
internal sealed record Recibo(Guid IdOperacion, Guid IdSolicitud, int RevisionConfirmada);
internal sealed record Confirmacion(Recibo Recibo, bool Repetida);

internal sealed class OperacionGuardada
{
    public Guid IdPartida { get; set; }
    public required string Rol { get; set; }
    public Guid IdOperacion { get; set; }
    public Guid IdSolicitud { get; set; }
    public int RevisionEsperada { get; set; }
    public int IdPersonaje { get; set; }
    public string? Atributo { get; set; }
    public int RevisionConfirmada { get; set; }

    internal Eleccion Datos() => new(IdOperacion, IdSolicitud, RevisionEsperada, IdPersonaje, Atributo);
    internal Recibo Recibo() => new(IdOperacion, IdSolicitud, RevisionConfirmada);
}
