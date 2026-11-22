# Estructura del repositorio

## Contenido del repositorio

| Ubicación | Contenido |
|---|---|
| [`src/`](../src) | Proyectos `cliente`, `servidor` y `subir-personajes`, y código de consola compartido en `comun`. |
| [`test/`](../test) | Proyecto de pruebas automatizadas. |
| [`docs/`](.) | Documentación y diagramas. |

Las convenciones de ramas, commits y Pull Requests están en la [guía de colaboración](../CONTRIBUTING.md).

## Archivos de referencia

| Ubicación | Contenido |
|---|---|
| `src/` | Código fuente de la aplicación. |
| `test/` | Proyectos de pruebas automatizadas. |
| `docs/` | Documentación y diagramas. |
| `CONTRIBUTING.md` | Guía de colaboración de este repositorio. |
| `.gitignore` | Archivos y carpetas que Git debe ignorar. |
| `.sln` | Solución de Visual Studio que agrupa los proyectos. |
| `README.md` | Presentación del proyecto e índice de documentación. |

Guardá el código de la aplicación en `src/` y las pruebas automatizadas en `test/`. Documentá la consigna, arquitectura y uso en `docs/`. Enlazá cada documento nuevo desde el [índice de documentación](../README.md#documentación). Mantené fuera del control de versiones los directorios generados `bin/` y `obj/`.

## Organización del servidor y sus pruebas

`src/servidor/` contiene el dominio: `Partida.cs`, su estado puro `EstadoPartida.cs` y los modelos `Combate`, `Ronda`, `Jugador`, `Mazo` y `Personaje`. `src/servidor/Api/` contiene coordinación, sesiones, catálogo, vista, HTTP/consola y persistencia. `Api/EstadoPartida.cs` contiene entidades persistidas y DTO de elecciones; es distinto del estado de dominio situado un nivel arriba. `PresentacionPartida.cs` conserva los DTO de salida y `VistaPartida.cs` construye su contenido.

`SqliteJuego.cs` concentra infraestructura compartida; `UnidadPartida.cs` delimita el estado y los recibos de una transacción. No quedan `RepositorioJuego.cs` ni `TransicionesPartida.cs`. Las pruebas de dominio, coordinación, vista y compatibilidad se ubican en el proyecto existente. `test/proyecto-integrador-final-tests/Fixtures/partida-e18a050.sql` conserva la base previa usada para la regresión de compatibilidad.

## Documentación por archivo

El [catálogo de C#](archivos/README.md) organiza una página por cada fuente propio versionado, incluidas pruebas y auxiliares. Se replica la ruta desde la raíz bajo `docs/archivos/` y solo se cambia `.cs` por `.md`: `src/cliente/Program.cs` corresponde a `docs/archivos/src/cliente/Program.md`, mientras que `src/servidor/Program.cs` tiene su propia página independiente.

Al mover o retirar un fuente, actualizar su página, catálogo y relaciones en la misma tarea documental. `bin/`, `obj/`, resultados de pruebas y dependencias no versionados quedan fuera; un generado que se versionara deliberadamente sí se inventariaría. El catálogo fija la versión documentada; los fragmentos deben contrastarse contra ella. Los [modelos](modelos.md) reúnen el desarrollo de tipos y miembros y enlazan las páginas individuales.
