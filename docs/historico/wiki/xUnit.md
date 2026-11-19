# xUnit

El paquete xUnit 2.5.3 define cómo se declaran los casos y se comprueban sus resultados. Está referenciado en el proyecto de pruebas.

1. `[Fact]` declara un escenario. `[Theory]` y `[InlineData]` repiten la comprobación con distintas entradas.
2. Los métodos preparan objetos o procesos, ejecutan la operación y afirman el resultado. `Assert.Equal` compara valores y `Assert.Throws` comprueba que se produce una excepción específica.
3. `PersonajeTests` prueba reglas y límites de puntajes. `ApiHttpTests` prueba respuestas y efectos persistentes. Ambos usan el mismo marco para comprobar resultados.
4. `ContratoFactAttribute` amplía `FactAttribute` para omitir escenarios que dependen de una DLL si su ruta no está configurada.
[Casos de prueba en código](Codigo-de-las-pruebas.md) · [Guía de xUnit v2](https://xunit.net/docs/getting-started/v2/getting-started)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
