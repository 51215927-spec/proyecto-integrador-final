# Servidor

Aloja la API REST, administra el catálogo y las partidas y persiste el estado en SQLite. Los jugadores usan el contrato HTTP y la herramienta de carga usa el endpoint de catálogo. El operador crea e inicia partidas desde esta consola.

`Partida` ejecuta inicio, elecciones y cierres sobre `EstadoPartida`, sin HTTP ni EF Core. `CoordinadorPartidas` valida acceso y organiza cada acción; `AlmacenPartidas` reconstruye y guarda el dominio y los recibos. `AdministradorSesiones` conserva presencia temporal, `CatalogoPersonajes` administra el catálogo y `VistaPartida` construye la vista privada de cada plaza. `ServidorHttp` conserva rutas y respuestas; `ConsolaServidor` delega la administración en esos componentes.

La separación conserva el esquema SQLite, las credenciales, las rutas y los tiempos de sesión. No requiere migrar las partidas existentes. [Evidencia y límites](../../docs/plan-de-pruebas.md#separación-de-responsabilidades-spec-006).

[Arquitectura](../../docs/arquitectura.md) · [Instalación y uso](../../docs/instalacion-y-uso.md) · [Pruebas](../../docs/plan-de-pruebas.md)
