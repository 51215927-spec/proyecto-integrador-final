# Servidor

Aloja la API REST, administra el catálogo y las partidas y persiste el estado en SQLite. Los jugadores usan el contrato HTTP y la herramienta de carga usa el endpoint de catálogo. Los invitados se emparejan y confirman Listo desde sus clientes; el operador prepara el catálogo y conserva las herramientas de administración.

`Partida` ejecuta inicio, elecciones y cierres sobre `EstadoPartida`, sin HTTP ni EF Core. `CoordinadorPartidas` valida acceso y organiza cada acción; `AlmacenPartidas` reconstruye y guarda el dominio y los recibos. `AdministradorSesiones` conserva presencia temporal, `CatalogoPersonajes` administra el catálogo y `VistaPartida` construye la vista privada de cada plaza. `ServidorHttp` expone exclusivamente rutas HTTP; el proceso no ofrece consola administrativa.

El spec 007 añade las tablas Invitado y AsignacionInvitado automáticamente, conservando las tablas, credenciales y partidas anteriores. Las sesiones vencen a los quince segundos. [Evidencia y límites](../../docs/plan-de-pruebas.md#separación-de-responsabilidades-spec-006).

[Arquitectura](../../docs/arquitectura.md) · [Instalación y uso](../../docs/instalacion-y-uso.md) · [Pruebas](../../docs/plan-de-pruebas.md)

## Inicio automático

`Invitados` emite y autentica identidades persistentes sin guardar secretos en claro. `Emparejamiento` coordina cola FIFO, salas, preparación, exclusión de sesiones y cancelación bajo la misma sincronización de las partidas. `AlmacenPartidas.CrearAutomatica` guarda inicio y asignaciones en una transacción. `Avisos` entrega invalidaciones SSE privadas; `VigilanciaAcceso` libera reservas vencidas. Reiniciar elimina cola, salas y presencia, conservando las partidas iniciadas y sus invitados.

El host filtra mensajes rutinarios de ASP.NET Core y EF Core a nivel Warning; advertencias y errores permanecen visibles. Crear e iniciar manualmente sigue disponible para recuperación excepcional y clientes v1. No hace falta ejecutarlos para atender una partida nueva.
