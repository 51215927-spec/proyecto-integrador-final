# Spectre.Console

Spectre.Console transforma objetos de presentación en texto y estilos de terminal. El cliente declara el paquete 0.57.2.

1. `AnsiConsole.Create` recibe el escritor de salida y configura la detección de ANSI. ANSI permite enviar estilos y controles de terminal, como el color o la limpieza de pantalla.
2. `InterfazConsola` crea objetos `Text`, `Panel`, `Grid` y `Rows` para describir texto, bordes y distribución. Usa `Markup.Escape` en títulos que se insertan en marcado de Spectre.
3. `terminal.Write` dibuja esos componentes. El foco y las teclas se resuelven en los métodos del proyecto, que vuelven a dibujar al mover la selección.
4. En las pruebas se pasa un escritor de texto y un ancho fijo para inspeccionar la salida sin una terminal interactiva.
[Implementación en el cliente](Codigo-del-cliente.md#interfazconsolacs) · [Documentación de Spectre.Console](https://spectreconsole.net/console/)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
