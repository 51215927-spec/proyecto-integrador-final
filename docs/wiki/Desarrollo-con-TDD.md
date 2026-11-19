# Desarrollo con TDD

TDD, o desarrollo guiado por pruebas, consiste en escribir una prueba automatizada antes de implementar el comportamiento que verifica. El proyecto prevé usar xUnit, la biblioteca de pruebas para .NET, durante Taller de Programación. En TI se definen los casos y los resultados esperados.

## Estructura inicial

Los contratos de [Arquitectura](Arquitectura.md) especifican las clases y las firmas de sus métodos. Al preparar esa estructura, los métodos pendientes lanzan `NotImplementedException`. Cada comportamiento comienza con su propia prueba.

## Ciclo rojo, verde y refactorización

1. **Rojo.** Se escribe una prueba en `test/` que fija los datos iniciales, ejecuta la operación y comprueba el resultado esperado. La primera ejecución debe fallar porque el comportamiento todavía falta. Un error de configuración se corrige antes de interpretar el resultado.
2. **Verde.** Se implementa en `src/` lo necesario para que la prueba pase. Se ejecutan también las pruebas previas para detectar cambios de comportamiento involuntarios.
3. **Refactorización.** Se mejora la estructura interna del código conservando el comportamiento comprobado. Se vuelven a ejecutar las pruebas después de cada cambio.

En xUnit, `[Fact]` identifica un escenario individual y `[Theory]` permite evaluar el mismo comportamiento con varios conjuntos de datos. Los casos normales, inválidos y de límite para personajes están en el [plan de pruebas](Plan-de-pruebas.md#personajes-y-catálogo).

## Mantener y verificar las pruebas

- Escribí pruebas independientes, con nombres que expliquen el escenario y el resultado esperado, sin depender del orden de ejecución.
- Ante un error, agregá primero una prueba que lo reproduzca y después corregí la implementación.
- Conservá las pruebas anteriores. No las elimines, desactives ni cambies solo para conseguir que pasen; si cambió un requisito o la prueba era incorrecta, justificá el ajuste.
- Usá dobles de prueba, como los mocks, cuando haga falta aislar una dependencia del código bajo prueba. La cobertura por sí sola no demuestra la calidad de las pruebas.
- Antes de finalizar una tarea de desarrollo, compilá y ejecutá la suite completa:

```bash
dotnet build proyecto-integrador-final.sln
dotnet test proyecto-integrador-final.sln
```

No des por terminado el desarrollo con pruebas fallidas ni reemplaces las pruebas automatizadas por comprobaciones manuales. En el Pull Request, indicá los comportamientos verificados y los resultados de los comandos para que un compañero pueda revisarlos.
