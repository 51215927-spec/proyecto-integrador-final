# VistasConsola.cs

Fuente: [src/cliente/VistasConsola.cs](../../../../src/cliente/VistasConsola.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Son valores de presentación para la consola local y REST. Separan la información visible de las entidades del servidor. No calculan victoria ni aplican reglas; las pantallas consumen sus datos.

## Dependencias

`IReadOnlyDictionary<string,int>` ofrece lectura de atributos sin exponer una API de escritura por esa referencia, aunque el diccionario subyacente puede ser mutable. [InterfazConsola](InterfazConsola.md), [InterfazConsola.Red](InterfazConsola.Red.md), [DemostracionConsola](DemostracionConsola.md) y [ContratosPartida](ContratosPartida.md) usan estos tipos. Véase [diseño de interfaz](../../../diseno-de-interfaz.md).

## Estructura, estado y miembros

Cinco `internal sealed record` sin herencia explícita, interfaces declaradas, atributos ni partial; un enum internal. No hay métodos con cuerpo ni eventos. Los constructores posicionales almacenan datos en propiedades init, con igualdad/copia/deconstrucción generadas por C#. No validan null, rangos, identidad ni consistencia.

| Tipo y constructor | Datos y ciclo de vida |
|---|---|
| `CartaVisible(int IdPersonaje, string Nombre, IReadOnlyDictionary<string,int> Atributos)` | ID de catálogo, nombre visible, mapa atributo→puntaje. Se comparte entre mano, opciones y resultados. El ID distingue cartas con mismo nombre. |
| `JugadorVisible(string Nombre, int RondasGanadas)` | Nombre mostrado y marcador de rondas; `with` crea un valor actualizado en demostración. |
| `CartaJugada(string Jugador, string Carta, int Valor)` | Nombre de participante, nombre de carta revelada y valor del atributo ya elegido. |
| `CombateVisible(string Atributo, CartaJugada J1, CartaJugada J2, string Ganador, int CombatesGanadosJ1, int CombatesGanadosJ2, string? ProximoEnElegir, bool? GanaJ1 = null)` | Duelo, ganador y marcador. ProximoEnElegir null omite el anuncio siguiente. GanaJ1 explícito evita ambigüedad con nombres iguales; si null, la interfaz compara nombre J1 con Ganador. |
| `MesaVisible(string Local, JugadorVisible J1, JugadorVisible J2, int Ronda, int Combate, int CombatesJ1, int CombatesJ2)` | Contexto local, marcador, coordenadas de ronda/combate y victorias parciales. |
| `EstadoEnvio` | `Enviando=0`, `Recibido=1`, `Pendiente=2` por orden implícito; distingue envío, recibo confirmado y recuperación incierta. No es flags. |

## Ejecución y fragmentos

El constructor de CombateVisible incluye una opción explícita de ganador:

```csharp
internal sealed record CombateVisible(
    string Atributo,
    CartaJugada J1,
    CartaJugada J2,
    string Ganador,
    int CombatesGanadosJ1,
    int CombatesGanadosJ2,
    string? ProximoEnElegir,
    bool? GanaJ1 = null);
```

No hay comparación de puntajes en este record. El campo nullable permite al adaptador REST pasar identidad por rol en vez de inferir por nombre. La distinción de envío se declara así:

```csharp
internal enum EstadoEnvio
{
    Enviando,
    Recibido,
    Pendiente
}
```

## Recorridos y ejemplos

Normal: una CartaVisible ID 7 llamada Vigía con Fuerza=650 se muestra en la mano. Al revelar, CartaJugada(Ana,Vigía,650) y CartaJugada(Beto,Oráculo,350) forman un CombateVisible de Fuerza con GanaJ1=true y marcador parcial 1/0. La pantalla resalta J1; no recalcula 650>350.

Alternativo: ambos participantes se llaman Ana. El adaptador pasa GanaJ1=false para resaltar J2 aunque Ganador también sea Ana. ProximoEnElegir=null no muestra turno siguiente. Construir un record con valores inconsistentes no lanza por sí mismo: su llamador debe validarlos.

## Errores, límites y efectos observables

No hay E/S, mutación posterior por setters ordinarios, transacciones ni cancelación. Propiedades init no vuelven inmutables los objetos referenciados. EstadoEnvio convertido desde un entero desconocido existe como valor enum, pero MostrarEnvio lo rechaza.

## Relaciones y verificación

[InterfazConsolaTests](../../test/proyecto-integrador-final-tests/InterfazConsolaTests.md), [PantallasConsolaTests](../../test/proyecto-integrador-final-tests/PantallasConsolaTests.md) y [PresentacionRedTests](../../test/proyecto-integrador-final-tests/PresentacionRedTests.md) usan estas vistas para verificar representación, marcadores y nombres. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md). La lectura de esta página no acredita una ejecución.
