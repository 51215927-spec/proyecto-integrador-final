# Proyecto integrador final

Proyecto académico de juego de cartas para dos jugadores, con cliente de consola en C# y .NET 8, servidor REST y catálogo cargado desde JSON. Reúne requisitos, diseño, implementación y pruebas.

El juego consiste en enfrentar personajes comparando sus atributos. Cada jugador recibe tres cartas por ronda; gana la ronda quien consigue dos combates y la partida quien alcanza 15 rondas ganadas. Las reglas completas están en [Reglas del juego](docs/reglas-del-juego.md).

## Estado actual

La etapa de **TI (análisis y diseño)** ya fue entregada, con el diseño REST y los casos de prueba documentados.

La solución incluye dominio, servidor REST y cliente de consola. Los jugadores eligen **Jugar**, esperan rival y confirman **Listo**: el servidor inicia automáticamente con seis personajes activos. El cliente permite crear varios perfiles desde el mismo usuario del sistema, elige automáticamente el único libre, recuerda la dirección y los accesos de invitado, recibe avisos de cambios y permite retomar después de cerrar. El operador conserva la carga y administración del catálogo. Las reglas y los recibos siguen separados de la coordinación de acceso. La [guía de uso](docs/instalacion-y-uso.md) describe el recorrido y la recuperación excepcional de partidas anteriores. La comprobación física entre dos computadoras sigue pendiente; los ensayos locales están en el [plan de pruebas](docs/plan-de-pruebas.md).


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
