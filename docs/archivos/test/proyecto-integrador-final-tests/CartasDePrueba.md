# CartasDePrueba.cs

Fuente: [test/proyecto-integrador-final-tests/CartasDePrueba.cs](../../../../test/proyecto-integrador-final-tests/CartasDePrueba.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Auxiliar interno para construir personajes válidos en los ensayos del modelo, evitando repetir ocho argumentos. No es una prueba descubierta por xUnit.

## Dependencias

Solo invoca el constructor del tipo interno Personaje del servidor. La referencia al proyecto y su InternalsVisibleTo permiten construirlo desde el ensamblado de pruebas. Este archivo no usa Assert, Fact ni Theory; los casos consumidores sí dependen de xUnit.

Relaciones usadas por este archivo: [Personaje](../../src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `internal static class CartasDePrueba`.

La clase estática no se instancia y no declara constructor propio; solo expone Crear dentro del ensamblado. No tiene herencia de clase explícita, interfaces, atributos, partial, propiedades, eventos ni operadores.

## Estado y datos

No conserva campos ni recursos; cada llamada devuelve una instancia nueva.

## Miembros y ejecución

Crear recibe su argumento de los casos consumidores, no de InlineData directamente. Su retorno es una referencia Personaje nueva; no hay asincronía, cancelación ni iteraciones porque la única instrucción construye y devuelve el objeto.

### `CartasDePrueba.Crear`

Método internal static con fuerza opcional 650; calcula velocidad como 1000−fuerza y fija resistencia, inteligencia, energía y habilidad en 500. Retorna el Personaje recién construido. No normaliza fuerza ni evita excepciones: con 650 o 750 cumple ambas cotas y los umbrales; con 500 no. No modifica una carta anterior.

Implementación completa del auxiliar; el constructor invocado aplica las reglas de dominio:

```csharp
internal static Personaje Crear(int fuerza = 650)
    {
        return new Personaje("Carta", "", fuerza, 1000 - fuerza, 500, 500, 500, 500);
    }
```

## Recorridos y ejemplos

Normal: Crear() produce fuerza 650, velocidad 350 y los otros cuatro valores 500: suma 3000 y satisface los umbrales.

Alternativo o de error: Crear(500) produce seis atributos 500: incumple fortaleza y debilidad, pero el constructor lanza primero por falta de fortaleza y no alcanza la comprobación de debilidad. El auxiliar no captura esa excepción.

Recorridos deducidos por lectura del auxiliar y del constructor de Personaje; no son casos nuevos ejecutados durante la redacción.

## Errores, límites y efectos observables

Los errores proceden del constructor de Personaje y se propagan sin captura. No impone fuerza válida, ni conserva estado o recursos que liberar. No hay I/O, transacciones, hilos, cancelación ni limpieza de archivos aplicables: solo crea un objeto en memoria. El nombre fijo y el ASCII vacío son datos de ensayo, no una carga de catálogo.

## Relaciones y verificación

La firma y el fragmento se contrastaron literalmente contra la referencia común. [JugadorTests](JugadorTests.md), [MazoTests](MazoTests.md), [CombateTests](CombateTests.md), [RondaTests](RondaTests.md) y [PartidaTests](PartidaTests.md) consumen Crear. No existe un Fact dedicado a este archivo: su comportamiento se ejercita al construir cartas en esos casos. La [evidencia de suite](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) distingue ejecución del conjunto de la revisión por lectura de los errores no ensayados.
