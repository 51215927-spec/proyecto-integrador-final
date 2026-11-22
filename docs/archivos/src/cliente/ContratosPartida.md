# ContratosPartida.cs

Fuente: [src/cliente/ContratosPartida.cs](../../../../src/cliente/ContratosPartida.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Declara los mensajes del cliente REST sin depender del ensamblado del servidor. El transporte los serializa/deserializa; EstadoCliente y SesionPartidaCliente verifican coherencia antes de habilitar interacción. No contienen reglas de combate ni operaciones de E/S.

## Dependencias

`System.Text.Json.Serialization.JsonRequiredAttribute` marca propiedades obligatorias al deserializar. Las cartas usan [CartaVisible](VistasConsola.md). [ClientePartidaHttp](ClientePartidaHttp.md) usa defaults Web, y [EstadoCliente](EstadoCliente.md) verifica los valores. Véase [protocolo](../../../protocolo-de-partida.md).

## Estructura, estado y miembros declarativos

Todos son `internal sealed record`, sin base explícita, interfaces ni partial. El constructor posicional crea propiedades init; el compilador genera igualdad, deconstrucción, copia `with` y representación textual. No se declaran métodos con lógica, campos mutables ni eventos. Los records no validan por sí mismos un argumento de constructor; arrays y diccionarios referenciados siguen siendo mutables.

| Tipo y firma posicional | Significado de cada dato y restricciones aplicadas fuera del record |
|---|---|
| `IncorporacionCliente(Guid IdSesion, string Nombre)` | UUID estable del proceso y nombre registrado exacto; cuerpo de PUT sesión. Sin JsonRequired porque se usa para envío. |
| `SesionConfirmada(string Rol, Guid IdSesion, DateTimeOffset VenceUtc)` | Rol J1/J2, identidad confirmada, vencimiento UTC. Las tres propiedades son JsonRequired. El coordinador contrasta UUID y rol; no usa VenceUtc para programar renovación. |
| `SolicitudCliente(Guid IdSolicitud, string Tipo, int[] IdsPersonajePermitidos, string[] AtributosPermitidos)` | UUID de solicitud, carta/cartaYAtributo, IDs ofrecidos, atributos ofrecidos; todos Required. EstadoCliente exige no vacío y coherencia con mano. |
| `ResultadoCliente(int NumeroRonda, int NumeroCombate, Dictionary<string,CartaVisible> Cartas, string Atributo, Dictionary<string,int> Puntajes, string GanadorCombate, string? GanadorRonda)` | Coordenadas del historial, ambas cartas reveladas por J1/J2, atributo comparado, puntajes por rol, ganador de combate y posible ganador de ronda. Todas Required: GanadorRonda debe existir en JSON aunque su valor pueda ser null. |
| `PresentacionCliente(Guid IdPartida, int Revision, string Fase, bool Pausada, string RolPropio, Dictionary<string,string?> Nombres, int? NumeroRonda, int? NumeroCombate, Dictionary<string,int> Marcador, CartaVisible[] Mano, SolicitudCliente? Solicitud, ResultadoCliente[] Resultados, string? MotivoFin, string? Ganador)` | Estado completo privado de plaza. Las primeras doce propiedades son Required, incluidas las que aceptan null; MotivoFin y Ganador pueden omitirse antes de cierre. Nombres usa roles y admite nombres null de plazas aún no incorporadas. Mano no contiene cartas rivales ocultas. |
| `EleccionCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionEsperada, int IdPersonaje, string? Atributo)` | UUID idempotente del envío, solicitud y revisión de origen, carta elegida, atributo null al responder. El coordinador conserva el mismo cuerpo al recuperar. |
| `ReciboCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionConfirmada)` | Aceptación persistida de elección, no resultado del combate. Todo Required; el coordinador exige coincidencias e incremento de revisión. |
| `ErrorCliente(string Codigo, string Mensaje, string IdSeguimiento)` | Código remoto, texto e identificador diagnóstico. Transporte solo publica un código conocido; no expone Mensaje ni cuerpo en su excepción. |

## Ejecución y fragmentos

No hay ejecución propia salvo constructores/accesores generados: asignan los argumentos sin normalización. La deserialización sí aplica atributos Required; son atributos de la propiedad (`property:`), no del parámetro primario.

```csharp
internal sealed record SolicitudCliente(
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] string Tipo,
    [property: JsonRequired] int[] IdsPersonajePermitidos,
    [property: JsonRequired] string[] AtributosPermitidos);
```

Los arrays obligatorios evitan aceptar omisiones como null de forma silenciosa; Required exige presencia, no rango semántico ni valor no null. EstadoCliente añade esas comprobaciones. La elección y el recibo real son:

```csharp
internal sealed record EleccionCliente(Guid IdOperacion, Guid IdSolicitud, int RevisionEsperada, int IdPersonaje, string? Atributo);
// El recibo acredita aceptación de la elección, sin anticipar el resultado del combate.
internal sealed record ReciboCliente(
    [property: JsonRequired] Guid IdOperacion,
    [property: JsonRequired] Guid IdSolicitud,
    [property: JsonRequired] int RevisionConfirmada);
```

Conservar IdOperacion y cuerpo permite consultar un recibo después de perder una respuesta sin consumir otra solicitud.

## Recorridos y ejemplos

Normal: una solicitud tipo carta ofrece ID 7, atributos `[]`; el cliente construye elección con nueva operación O, UUID S, revisión 4, carta 7, atributo null. Un recibo O/S/revisión 5 confirma recepción; no afirma que ese jugador ganó.

Alternativo: JSON de SesionConfirmada sin rol falla por propiedad obligatoria. GanadorRonda null explícito es válido, pero omitido incumple Required. Tipo de solicitud desconocido puede construirse como record, aunque EstadoCliente lo rechaza antes de aplicarlo.

## Errores, límites y efectos observables

Nulabilidad C# es ayuda estática; no sustituye validación de JSON. Igualdad record de arrays/diccionarios no compara profundamente su contenido; el coordinador usa JSON para huellas de pantalla/selección. No hay liberación de recursos, cancelación ni persistencia en estos tipos.

## Relaciones y verificación

[TransporteClienteTests](../../test/proyecto-integrador-final-tests/TransporteClienteTests.md), [EstadoClienteTests](../../test/proyecto-integrador-final-tests/EstadoClienteTests.md) y [SesionClienteTests](../../test/proyecto-integrador-final-tests/SesionClienteTests.md) verifican partes diferentes del contrato. No confundir un DTO declarable con una respuesta semánticamente aceptable. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.
