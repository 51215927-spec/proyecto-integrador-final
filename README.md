# Proyecto integrador final

Proyecto académico de juego de cartas para dos jugadores, con cliente de consola en C# y .NET 8, servidor REST y catálogo cargado desde JSON. Reúne requisitos, diseño, implementación y pruebas.

El juego consiste en enfrentar personajes comparando sus atributos. Cada jugador recibe tres cartas por ronda; gana la ronda quien consigue dos combates y la partida quien alcanza 15 rondas ganadas. Las reglas completas están en [Reglas del juego](docs/reglas-del-juego.md).

## Estado actual

La etapa de **TI (análisis y diseño)** ya fue entregada, con el diseño REST y los casos de prueba documentados.

La solución incluye el dominio del juego, el servidor REST y el cliente de consola conectado por HTTP. Los tres programas solicitan sus datos por consola; la carga también acepta la ruta JSON como argumento opcional. No requieren variables de entorno de usuario. El cliente permite incorporarse, jugar y retomar partidas. La validación entre dos computadoras sigue pendiente. `Partida` concentra las reglas por acciones sucesivas, separadas de coordinación, sesiones, catálogo, persistencia y presentación. La [verificación del spec 006](docs/plan-de-pruebas.md#separación-de-responsabilidades-spec-006) aprobó 190 pruebas locales sin omisiones. La [guía del servidor](docs/protocolo-de-partida.md#operación-del-servidor) describe el inicio y la administración del catálogo y las partidas.

## Documentación

- [Consigna](docs/consigna.md).
- [Reglas del juego](docs/reglas-del-juego.md).
- [Arquitectura](docs/arquitectura.md).
- [Modelos](docs/modelos.md).
- [Catálogo de archivos C#](docs/archivos/README.md).
- [Tecnologías](docs/tecnologias.md).
- [Bases de datos](docs/bases-de-datos.md).
- [API](docs/protocolo-de-partida.md).
- [Pruebas](docs/plan-de-pruebas.md).
- [Diseño de interfaz](docs/diseno-de-interfaz.md).
- [Estructura del repositorio](docs/estructura-del-repositorio.md).
- [Instalación y uso](docs/instalacion-y-uso.md).
- [Plan de continuidad](docs/plan.md).
- [Colaboración con Git y GitHub](CONTRIBUTING.md).

## Autores

- [51215927-spec](https://github.com/51215927-spec)
- [50509098-JC](https://github.com/50509098-JC)
- [Chino18cm](https://github.com/Chino18cm)
