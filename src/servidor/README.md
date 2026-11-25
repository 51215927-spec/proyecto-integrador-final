# Servidor

Aloja la API REST, administra el catálogo y las partidas y persiste el estado en SQLite. Los jugadores usan el contrato HTTP y la herramienta de carga usa el endpoint de catálogo. Al emparejar dos invitados, inicia la partida automáticamente cuando el catálogo tiene seis personajes activos.

`Partida` ejecuta inicio, elecciones y cierres sobre `EstadoPartida`, sin HTTP ni EF Core. `CoordinadorPartidas` valida acceso y organiza cada acción; `AlmacenPartidas` reconstruye y guarda el dominio y los recibos. `AdministradorSesiones` conserva presencia temporal, `CatalogoPersonajes` administra el catálogo y `VistaPartida` construye la vista privada de cada plaza. `ServidorHttp` expone exclusivamente rutas HTTP; el proceso no ofrece consola administrativa.

El spec 007 añade las tablas Invitado y AsignacionInvitado automáticamente, conservando las tablas, credenciales y partidas anteriores. Las sesiones vencen a los quince segundos. [Evidencia y límites](../../docs/plan-de-pruebas.md#separación-de-responsabilidades-spec-006).

[Arquitectura](../../docs/arquitectura.md) · [Instalación y uso](../../docs/instalacion-y-uso.md) · [Pruebas](../../docs/plan-de-pruebas.md)

## Inicio automático

`Invitados` emite y autentica identidades persistentes sin guardar secretos en claro. `Emparejamiento` coordina cola FIFO, salas, exclusión de sesiones y cancelación bajo la misma sincronización de las partidas. Al aparecer la pareja inicia sin confirmación de los clientes; si el catálogo no alcanza seis personajes, reintenta con la próxima actualización de estado o presencia. `AlmacenPartidas.CrearAutomatica` guarda inicio y asignaciones en una transacción. `Avisos` entrega invalidaciones SSE privadas; `VigilanciaAcceso` libera reservas vencidas. Reiniciar elimina cola, salas y presencia, conservando las partidas iniciadas y sus invitados.

El host filtra mensajes rutinarios de ASP.NET Core y EF Core a nivel Warning; advertencias y errores permanecen visibles. Crear e iniciar manualmente sigue disponible para recuperación excepcional y clientes v1. No hace falta ejecutarlos para atender una partida nueva.
