# Bases de datos

El servidor crea el esquema con `EnsureCreated` al abrir el repositorio. La configuración `ConnectionStrings__Juego` permite seleccionar la cadena SQLite. Por defecto usa `Data Source=guerra.db;Default Timeout=1`. Una ruta relativa se interpreta desde el directorio de trabajo del servidor.

| Tabla | Clave primaria | Datos y relaciones |
|---|---|---|
| PersonajeCatalogo | Id | Nombre, descripción, ASCII, seis atributos y estado activo. El repositorio asigna el ID. |
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

`AbrirSolicitud` crea la fila con `Consumida = false` y actualiza `Partida.IdSolicitud` y la fase. `Elegir` exige que la solicitud coincida con ese puntero, pertenezca a la plaza autenticada y siga sin consumir. Al aceptar la elección, marca la fila como consumida. Al finalizar una partida con una solicitud activa, `Finalizar` también la consume y limpia el puntero. La persistencia del puntero y de la fila permite retomar la acción vigente después de reincorporar las sesiones.

### Operacion

`OperacionGuardada` representa una elección confirmada. Su clave primaria `(IdPartida, Rol, IdOperacion)` limita la identidad de la operación a una plaza de una partida. `(IdPartida, Rol)` apunta a `Participante` e `IdSolicitud` apunta a `Solicitud`. Se guardan `IdPersonaje`, el `Atributo` normalizado y anulable, `RevisionEsperada` y `RevisionConfirmada`.

`Datos()` reconstruye la elección para comparar un reintento. `Recibo()` proyecta `IdOperacion`, `IdSolicitud` y `RevisionConfirmada`. El consumo de la solicitud, el avance de la partida y la fila de operación se confirman en la misma transacción de escritura. Esto permite recuperar el recibo tras un reinicio y evita que un reenvío compatible aplique otra jugada.

[Transiciones y consultas del repositorio](../src/servidor/Api/TransicionesPartida.cs) · [Contrato de reintento](protocolo-de-partida.md#persistencia-concurrencia-y-catálogo)

[Configuración de entidades](../src/servidor/Api/BaseJuego.cs) · [Entidades de partida](../src/servidor/Api/EstadoPartida.cs) · [Entidad del catálogo](../src/servidor/Api/PersonajeCatalogo.cs)
