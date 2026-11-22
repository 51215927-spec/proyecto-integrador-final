# ContratoFactAttribute.cs

Fuente: [test/proyecto-integrador-final-tests/ContratoFactAttribute.cs](../../../../test/proyecto-integrador-final-tests/ContratoFactAttribute.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `global (sin declaración namespace)`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Atributo público derivado de FactAttribute que condiciona los contratos de procesos a una variable de entorno. No ejecuta la aplicación ni comprueba la DLL.

## Dependencias

Usa System.Environment para consultar la variable y Xunit.FactAttribute como clase base. El runner de xUnit consume Skip durante el descubrimiento: este atributo no es un caso de prueba ni ejecuta Assert. No depende de la API, SQLite o bibliotecas HTTP.

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public sealed class ContratoFactAttribute : FactAttribute`.

La clase es pública y sellada: se puede usar como atributo de métodos pero no derivar de ella. Su único miembro declarado es el constructor público. Hereda el contrato de FactAttribute y la propiedad Skip; no declara partial, interfaces adicionales, eventos ni operadores.

## Estado y datos

No declara campos ni propiedades propios. Hereda Skip de FactAttribute; lo escribe durante construcción.

## Miembros y ejecución

El constructor recibe el nombre de variable al aplicar [ContratoFact(...)] a un método. No tiene valor de retorno ni acciones de juego: su efecto es establecer o conservar la condición Skip heredada. No hay preparación/acción/aserciones de un caso propio, porque este archivo define infraestructura de descubrimiento.

### `ContratoFactAttribute.ContratoFactAttribute`

Constructor público: recibe el nombre de la variable, lee Environment.GetEnvironmentVariable(variableDll), y solo si IsNullOrWhiteSpace es true asigna Skip. El mensaje interpola el nombre de la variable, nunca su contenido. No existe rama de ejecución de procesos ni comprobación File.Exists aquí. Errores de nombre inválido de variable se propagan desde la biblioteca.

Implementación completa del constructor; el if determina si el runner omite el caso marcado:

```csharp
public ContratoFactAttribute(string variableDll)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variableDll)))
            Skip = $"Indicá la DLL compilada mediante {variableDll} para ejecutar esta prueba.";
    }
```

## Recorridos y ejemplos

Normal: Con GUERRA_CLIENTE_DLL definido con una ruta no vacía, el constructor no asigna Skip y xUnit puede ejecutar el contrato.

Alternativo o de error: Si la variable falta o solo contiene espacios, asigna Skip con el diagnóstico que indica qué variable configurar. Una ruta no vacía pero inexistente no se omite por este atributo.

Recorridos deducidos por lectura del constructor y de su condición; no se modificaron las variables del usuario para verificarlos.

## Errores, límites y efectos observables

Una variable en blanco omite el caso, pero una DLL inexistente con ruta no vacía se deja ejecutar y puede fallar posteriormente. No verifica compatibilidad de versión, SDK, permisos o ejecutable dotnet. No crea recursos, no cambia variables de entorno ni lanza procesos; por eso limpieza, asincronía y transacciones no aplican. El valor de la variable no se incorpora al mensaje Skip.

## Relaciones y verificación

La firma, herencia, condición y fragmento se contrastaron contra la referencia común. Lo consumen [ArranqueRestTests](ArranqueRestTests.md), [ClienteProcesoRestTests](ClienteProcesoRestTests.md), [EntradaProcesosContratoTests](EntradaProcesosContratoTests.md) y [ServidorContratoTests](ServidorContratoTests.md). No existe un Fact propio que compruebe todas las ramas de este constructor. La [evidencia de suite](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) ejecutó contratos con las variables DLL definidas; no debe interpretarse como ejecución de la rama Skip ausente.
