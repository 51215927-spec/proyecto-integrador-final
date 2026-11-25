# Bases de datos

El servidor crea el esquema con `EnsureCreated` al inicializar `SqliteJuego`. La consola de inicio pide el archivo SQLite y usa `guerra.db` por defecto. La interfaz interna del host permite configurar la cadena SQLite para pruebas aisladas; usa un tiempo de espera de un segundo. Una ruta relativa se interpreta desde el directorio de trabajo del servidor.

| Tabla | Clave primaria | Datos y relaciones |
|---|---|---|
| PersonajeCatalogo | Id | Nombre, descripción, ASCII, seis atributos y estado activo. `CatalogoPersonajes` asigna el ID. |
| Partida | Id | Fase, revisión, ronda y combate actuales, solicitud activa, motivo de fin, ganador y abandono. |
| Participante | IdPartida, Rol | Referencia a Partida. Guarda nombre, hash de credencial y rondas ganadas. |
| PersonajePartida | IdPartida, IdPersonaje | Referencia a Partida. Copia nombre, descripción, ASCII y atributos. Guarda ubicación y orden en la partida. |
| Ronda | IdPartida, Numero | Referencia a Partida. Guarda mano inicial, victorias por plaza y ganador. |
| Combate | IdPartida, NumeroRonda, Numero | Referencia a Ronda. Guarda elector, cartas elegidas, atributo, puntajes y ganador. |
| Solicitud | Id | Referencia a Combate. Guarda plaza, tipo y estado de consumo. Tiene índice por IdPartida. |
| Operacion | IdPartida, Rol, IdOperacion | Referencias a Participante y Solicitud. Guarda elección, revisión esperada y confirmada para devolver el mismo recibo ante un reintento compatible. |

Las claves de la partida, solicitudes y operaciones son UUID. Los números de ronda, combate, personaje, orden, atributos y revisiones son enteros. Los resultados y elecciones pendientes admiten valores nulos según la etapa del juego. Las reglas de atributos se validan en el dominio. Las sesiones y sus vencimientos viven en memoria.

## Solicitudes y operaciones

### Solicitud

`SolicitudGuardada` representa una acción habilitada para una plaza en un combate. Su clave primaria es `Id` (`Guid`). La clave foránea `(IdPartida, NumeroRonda, NumeroCombate)` apunta a `Combate`, y `IdPartida` tiene un índice. `Rol` identifica la plaza y `Tipo` distingue `cartaYAtributo` para el elector y `carta` para el rival.

`Partida.AbrirSolicitud` crea la solicitud de dominio, que `AlmacenPartidas` mapea a la fila con `Consumida = false` y actualiza `Partida.IdSolicitud` y la fase. `Partida.RegistrarEleccion` exige que la solicitud coincida con ese puntero, pertenezca a la plaza autenticada y siga sin consumir. Al aceptar la elección, marca la fila como consumida. Al finalizar una partida con una solicitud activa, `Partida.Finalizar` también la consume y limpia el puntero. La persistencia del puntero y de la fila permite retomar la acción vigente después de reincorporar las sesiones.

### Operacion

`OperacionGuardada` representa una elección confirmada. Su clave primaria `(IdPartida, Rol, IdOperacion)` limita la identidad de la operación a una plaza de una partida. `(IdPartida, Rol)` apunta a `Participante` e `IdSolicitud` apunta a `Solicitud`. Se guardan `IdPersonaje`, el `Atributo` normalizado y anulable, `RevisionEsperada` y `RevisionConfirmada`.

`Datos()` reconstruye la elección para comparar un reintento. `Recibo()` proyecta `IdOperacion`, `IdSolicitud` y `RevisionConfirmada`. El consumo de la solicitud, el avance de la partida y la fila de operación se confirman en la misma transacción de escritura. Esto permite recuperar el recibo tras un reinicio y evita que un reenvío compatible aplique otra jugada.

[Coordinación de elecciones](../src/servidor/Api/CoordinadorPartidas.cs) · [Mapeo y guardado](../src/servidor/Api/AlmacenPartidas.cs) · [Contrato de reintento](protocolo-de-partida.md#persistencia-concurrencia-y-catálogo)

[Configuración de entidades](../src/servidor/Api/BaseJuego.cs) · [Entidades de partida](../src/servidor/Api/EstadoPartida.cs) · [Entidad del catálogo](../src/servidor/Api/PersonajeCatalogo.cs)

## Reemplazo del catálogo

La carga JSON valida el lote completo antes de escribir. `ReemplazarCatalogo` borra y vuelve a insertar únicamente `PersonajeCatalogo` dentro de una transacción; asigna IDs de 1 a N, en orden, y marca todos activos. Un fallo al guardar revierte el borrado y las inserciones. Las tablas de partidas conservan sus copias; el GET de resumen cuenta activos e inactivos. El POST individual permanece disponible, pero la herramienta JSON no lo utiliza. [Catálogo](archivos/src/servidor/Api/CatalogoPersonajes.md) · [Validación del lote](archivos/src/servidor/Api/LotePersonajes.md).

## Reconstrucción y compatibilidad

`AlmacenPartidas.Cargar` reconstruye `EstadoPartida` y sus participantes, cartas, rondas, combates y solicitudes. El dominio no recibe entidades EF ni hashes de credencial. `Guardar` actualiza las filas existentes e incorpora las nuevas; `SqliteJuego.Ejecutar` confirma esos cambios y los recibos en una transacción. Un fallo anterior al commit revierte el conjunto. La conexión mantenida abierta permite usar `:memory:` en pruebas; en operación se conserva SQLite en archivo.

El spec 006 no cambia tablas ni columnas. `CompatibilidadPersistenciaTests` reabre una base producida por el servidor anterior (`e18a050`), recupera una operación confirmada y continúa el combate sin modificar el esquema. La prueba usa la [fixture SQL](../test/proyecto-integrador-final-tests/Fixtures/partida-e18a050.sql), no una migración. [Evidencia](plan-de-pruebas.md#separación-de-responsabilidades-spec-006).

## Evolución aditiva del spec 007

Después de EnsureCreated, SqliteJuego crea idempotentemente Invitado (Id, Hash, Nombre) y AsignacionInvitado (IdInvitado, IdPartida, Rol; unicidad de plaza). No altera las tablas anteriores. El servidor guarda SHA256 del secreto aleatorio de 256 bits; el acceso de invitado acredita la plaza asignada mediante el hash de Participante. No hay secretos persistidos en claro en servidor. Partida, cartas y asignaciones se escriben en la misma transacción; una falla revierte todo el inicio.

Solo se persisten asignaciones de partidas iniciadas. Presencia, búsquedas y salas previas desaparecen al reiniciar. Una consulta de asignación filtra partidas finalizadas. Las plazas antiguas conservan su credencial y no se adjudican por nombre. La recuperación excepcional no crea vínculos con invitados nuevos.


Al arrancar el ejecutable del servidor, `CatalogoPredeterminado` carga el JSON de ejemplos embebido si la tabla de catálogo no tiene filas. La operación valida y reemplaza el lote en una transacción. Si ya hay personajes, no modifica ni agrega ninguno; esto conserva las ediciones y evita duplicados en reinicios.
