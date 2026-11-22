# PresentacionPartida.cs

Fuente: [src/servidor/Api/PresentacionPartida.cs](../../../../../src/servidor/Api/PresentacionPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión: [catálogo común](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Contratos JSON de presentación.

## Estado, miembros y ejecución

SolicitudVisible contiene UUID, tipo, IDs y atributos permitidos. ResultadoVisible conserva número de ronda/combate, cartas, atributo, puntajes y ganadores. PresentacionVisible reúne identidad, revisión, fase, pausa, rol, nombres, números, marcador, mano, solicitud, resultados y cierre. Sus records producen propiedades posicionales e igualdad por valor; no declaran métodos de negocio. JsonIgnore omite MotivoFin y Ganador cuando son null.

## Errores, límites y efectos

Este archivo solo declara DTO; VistaPartida construye sus valores. No queda una clase partial ni acceso al almacenamiento. La forma JSON externa permanece igual.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
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
```

## Relaciones y verificación

Véanse [arquitectura](../../../../arquitectura.md), [modelos](../../../../modelos.md) y [auditoría del spec 006](../../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
