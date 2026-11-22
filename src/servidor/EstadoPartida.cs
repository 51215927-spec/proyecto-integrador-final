namespace Proyecto_Integrador_Final;

// Estado reanudable del dominio. No contiene credenciales, presencia ni recibos de transporte.
internal sealed class EstadoPartida
{
    public Guid Id { get; set; }
    public int Revision { get; set; }
    public int? NumeroRonda { get; set; }
    public int? NumeroCombate { get; set; }
    public Guid? IdSolicitud { get; set; }
    public string? MotivoFin { get; set; }
    public string? Ganador { get; set; }
    public string? AbandonoDe { get; set; }
    public string Fase { get; set; } = "esperandoInicio";
    public List<ParticipantePartida> Participantes { get; set; } = [new() { Rol = "J1" }, new() { Rol = "J2" }];
    public List<CartaPartida> Cartas { get; set; } = [];
    public List<RondaPartida> Rondas { get; set; } = [];
    public List<CombatePartida> Combates { get; set; } = [];
    public List<SolicitudPartida> Solicitudes { get; set; } = [];
}

internal sealed class ParticipantePartida
{
    public required string Rol { get; set; }
    public string? Nombre { get; set; }
    public int RondasGanadas { get; set; }
}

internal sealed class CartaPartida
{
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
    internal Personaje Dominio() => new(Nombre, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);
}

internal sealed class RondaPartida
{
    public int Numero { get; set; }
    public required string Mano { get; set; }
    public int VictoriasJ1 { get; set; }
    public int VictoriasJ2 { get; set; }
    public string? Ganador { get; set; }
}

internal sealed class CombatePartida
{
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

internal sealed class SolicitudPartida
{
    public Guid Id { get; set; }
    public int NumeroRonda { get; set; }
    public int NumeroCombate { get; set; }
    public required string Rol { get; set; }
    public required string Tipo { get; set; }
    public bool Consumida { get; set; }
}

internal sealed class ExcepcionPartida(string codigo, string mensaje) : InvalidOperationException(mensaje)
{
    internal string Codigo { get; } = codigo;
}
