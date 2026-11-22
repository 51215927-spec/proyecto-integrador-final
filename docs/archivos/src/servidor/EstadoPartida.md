# EstadoPartida.cs

Fuente: [src/servidor/EstadoPartida.cs](../../../../src/servidor/EstadoPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](../../README.md#versión-documentada).

## Propósito y responsabilidad

Estado reanudable e independiente de persistencia.

## Estado, miembros y ejecución

EstadoPartida contiene ID, revisión, fase, ronda/combate, solicitud actual y cierre, más listas de participantes, cartas, rondas, combates y solicitudes. ParticipantePartida contiene rol/nombre/marcador sin hash. CartaPartida conserva identidad, metadatos, seis atributos, ubicación y orden; Dominio crea un Personaje. RondaPartida conserva mano, victorias y ganador; CombatePartida elecciones y resultado; SolicitudPartida rol, tipo, números y consumida. ExcepcionPartida expresa código y mensaje de negocio sin HTTP.

## Errores, límites y efectos

Los setters permiten reconstrucción explícita desde SQLite; reconstruir no reparte ni resuelve. No contiene sesiones, credenciales o recibos. No es una entidad EF ni un DTO HTTP. La clase Partida gobierna las acciones.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
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
```

## Relaciones y verificación

Véanse [arquitectura](../../../arquitectura.md), [modelos](../../../modelos.md) y [auditoría del spec 006](../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
