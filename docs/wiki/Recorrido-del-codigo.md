# Recorrido del código

Cada página sigue los archivos y explica los métodos escritos en el proyecto. Los enlaces apuntan al código publicado en la revisión `4fd6986`.

1. [Código del dominio](Codigo-del-dominio.md). Personajes, cartas, jugadores, combates y el bucle en memoria.
2. [Código del cliente de consola](Codigo-del-cliente.md). Punto de entrada, datos de pantalla, demostración y lectura de teclas.
3. [Código de la herramienta de alta](Codigo-de-carga.md). Formulario de alta y solicitud HTTP de principio a fin.
4. [Código del servidor HTTP](Codigo-del-servidor.md). Arranque, API, persistencia, confirmación de elecciones y proyección de vistas.
5. [Código de las pruebas](Codigo-de-las-pruebas.md). Casos automatizados, utilidades y pruebas que lanzan los ejecutables.
6. [Archivos de compilación](Archivos-de-compilacion.md). La solución, referencias entre proyectos y manifiestos de paquetes.

## Cómo se conectan las piezas

- **Demo local.** `cliente/Program.Main` → `DemostracionConsola.Recorrer` → `InterfazConsola` → terminal.
- **Alta real.** `subir-personajes/Program.Main` → `ConsolaAlta.Ejecutar` → HTTP → handler de alta en `ServidorHttp` → `RepositorioJuego.DarDeAlta` → catálogo SQLite.
- **Partida en el servidor.** `ServidorHttp` → `RepositorioJuego.Elegir` → `Resolver` → `Combate.Resolver` → escritura de resultado y recibo. `Presentacion` lee el estado confirmado para cada plaza.
- **Integración pendiente.** El cliente de juego muestra la demo local. Las pruebas HTTP y por proceso ejercitan el servidor y su contrato en una computadora.

[Apéndice de bibliotecas y herramientas](Bibliotecas-y-herramientas.md) · [Índice de documentación](README.md)
