# Protocolo de partida web — API REST

HTTP en red local está aprobado para la entrega de la [actividad 6](Consigna.md). Este documento especifica el diseño REST. La implementación y las comprobaciones en dos computadoras siguen pendientes.

- [Despliegue y conexión](#organización-propuesta)
- [Identidad y sesiones](#administración-e-identidad)
- [Operaciones, datos y errores HTTP](#contrato-http)
- [Estados y recuperación](#estados-presencia-y-recuperación)
- [Persistencia y catálogo](#persistencia-concurrencia-y-catálogo)
- [Verificación en dos equipos](#verificación-en-dos-equipos)

## Organización propuesta

Dos clientes de consola se comunican con una API ASP.NET Core mediante HTTP y JSON. El servidor dirige el juego y es la única autoridad sobre cartas, turnos, resultados y marcadores. Cada cliente recibe exclusivamente la información que puede ver y devuelve las elecciones solicitadas.

EF Core administrará la persistencia en una base SQLite local al servidor, tanto del catálogo permanente de personajes (nombre, descripción, ASCII y seis atributos) como de los datos de partidas. Los clientes acceden exclusivamente a la API. El esquema y las transacciones se definen en este documento.

En el primer equipo Windows se ejecutan el servidor y el cliente J1 como procesos separados. En el segundo se ejecuta el cliente J2. Se abre primero el servidor y después ambos clientes. Las versiones acordadas están en [Tecnologías REST](Tecnologias-rest.md).

La API y el dominio tienen responsabilidades distintas: la API identifica al jugador y valida el contrato recibido; el dominio decide si la elección es válida y resuelve el juego; la persistencia confirma los cambios. Los DTO de red no son entidades EF ni objetos vivos de Partida.

## Conexión en red local

Para las pruebas en la red local se utiliza HTTP, puerto TCP 5000 y dirección del servidor configurable en cada cliente. Ejemplo ilustrativo: `http://192.168.1.20:5000`; la IP no está asignada ni comprobada. Cada cliente lee la dirección base de su archivo local de configuración al iniciar.

Localhost identifica la computadora del propio proceso: sirve para probar clientes y servidor en el mismo equipo. El cliente de otra computadora debe usar la dirección de red del servidor. Este debe aceptar conexiones en una interfaz accesible desde esa red, no limitarse a localhost; no hay configuración implementada que lo acredite.

Si el servidor no está disponible, el cliente informa y ofrece reintentar o salir. Conserva una elección pendiente y no la transforma en una operación nueva al reintentar. La confirmación de una elección cuya respuesta se perdió se resolverá mediante los contratos de operaciones e idempotencia de este documento; no se asume que falló por un error de conexión.

HTTP no cifra el intercambio. Su elección se limita al entorno local de prueba acordado; no se define publicación en Internet. Primero se probará la comunicación para determinar si existe un bloqueo, sin asumir que sea necesario abrir el firewall. De requerirse una excepción en un equipo escolar, se coordinará con el responsable y se limitará al servicio, puerto y equipos participantes. No se requiere desactivar el firewall ni abrir puertos del router hacia Internet.

## Administración e identidad

El operador crea, inicia y cancela partidas mediante la consola local del servidor. La creación genera un UUID de partida y dos credenciales aleatorias independientes, una por plaza J1/J2, que el operador entrega a cada jugador fuera de la API. Solo se almacena el hash de cada credencial. El cliente envía su credencial en `Authorization: Bearer` y el servidor deduce la plaza. El nombre visible se registra al incorporarse y queda fijo. El operador puede reemplazar una credencial desde el servidor y revocar la sesión anterior sin cambiar de plaza ni perder la mano.

Cada instancia genera un UUID de sesión y lo conserva mientras se ejecuta. La primera incorporación reserva la plaza para esa sesión. Una instancia posterior con la misma credencial debe esperar al vencimiento de la anterior o pedir al operador que la cierre. Un reintento de incorporación con la misma sesión renueva su plazo. Si el nombre ya está registrado, debe coincidir. Las credenciales se usan únicamente en la red local acordada, fuera de URLs y registros.

## Contrato HTTP

Las rutas llevan el prefijo `/api/v1/partidas/{id}`. Los cuerpos son JSON UTF-8 con propiedades camelCase. `id`, `idSesion`, `idSolicitud` e `idOperacion` son UUID. Los identificadores de personaje, números de combate y ronda, puntajes y revisión son enteros. Todas las operaciones salvo la incorporación requieren la sesión vigente en la cabecera `X-Sesion`. La creación, el inicio y la cancelación administrativa se ejecutan localmente en el servidor.

| Método y ruta relativa | Entrada | Éxito |
|---|---|---|
| `PUT /participantes/me/sesion` | `idSesion`, `nombre` no vacío | `200` con `rol`, `idSesion` y `venceUtc` |
| `PUT /participantes/me/presencia` | Sin cuerpo | `204`. Renueva la sesión vigente |
| `GET /presentacion` | Sin cuerpo | `200` con vista propia. Usar `Cache-Control: no-store` |
| `POST /elecciones` | `idOperacion`, `idSolicitud`, `revisionEsperada`, `idPersonaje`, `atributo` si se solicita | `201` con recibo y `Location` al consultar el recibo. Una repetición aceptada devuelve `200` con el mismo recibo |
| `GET /elecciones/{idOperacion}` | Sin cuerpo | `200` con recibo propio o `404` |
| `PUT /participantes/me/abandono` | Sin cuerpo | `204`. Finaliza la partida y repetirlo para el mismo participante conserva el resultado |

- **Presentación** contiene `idPartida`, `revision`, `fase`, `pausada`, `rolPropio`, nombres de ambas plazas cuando estén registradas, `numeroRonda`, `numeroCombate`, `marcador` por plaza, `mano`, `solicitud` y `resultados`. Los números de ronda y combate son nulos antes del juego. Cada elemento de mano contiene `idPersonaje`, `nombre`, `descripcion`, `ascii` y los seis atributos como mapa de nombres canónicos a puntajes.
- **Solicitud** es nula si el jugador debe esperar o la partida está pausada o terminada. Si existe, contiene `idSolicitud`, `tipo` (`cartaYAtributo` o `carta`), `idsPersonajePermitidos` y `atributosPermitidos`, vacío para `carta`. Los atributos usan los nombres de la consigna. El rival desconoce carta y atributo hasta la resolución.
- **Recibo** contiene `idOperacion`, `idSolicitud` y `revisionConfirmada`. **Resultados** es el historial ordenado de combates ya resueltos, con ronda, combate, cartas reveladas por plaza, atributo, puntajes y ganadores de combate y ronda si corresponde. En fase final, la vista incluye `motivoFin` (`victoria`, `abandono` o `cancelacion`) y `ganador` cuando corresponde a una victoria por las reglas.

| Estado | Código de error y tratamiento |
|---|---|
| 400 | `entradaInvalida` por JSON, campos o tipos inválidos. Corregir datos antes de reenviar |
| 401 | `credencialInvalida`. Revisar credencial |
| 403 | `sinPermiso`. La identidad autenticada carece de autorización |
| 404 | `recursoNoEncontrado`. También se usa para recibos de otro participante |
| 409 | `sesionOcupada`, `sesionVencida`, `partidaPausada`, `revisionDesactualizada`, `solicitudNoVigente` u `operacionIncompatible`. Actualizar sesión o presentación según el código |
| 422 | `eleccionInvalida` por carta o atributo fuera de las opciones. Solicitar otra entrada |
| 503 | `servicioNoDisponible`. Consultar recibo o reintentar con la misma operación |

El error incluye `codigo`, `mensaje` e `idSeguimiento`; sus campos excluyen elecciones ocultas y detalles internos.

## Estados, presencia y recuperación

1. En `esperandoInicio`, el operador inicia solo con ambas plazas presentes y seis personajes válidos como mínimo. El servidor copia el catálogo activo para la partida, mezcla, reparte tres personajes por jugador y abre `esperandoElector` con J1 como mano de la primera ronda.
2. Una elección válida del elector reserva su carta y atributo y cambia a `esperandoRival`. La elección rival resuelve el combate, retira ambas cartas y publica el resultado. Un historial persistido permite que los clientes muestren cada resultado una vez, sin exigir confirmación para avanzar.
3. Si continúa la ronda, alterna el elector. Al alcanzar dos combates ganados se suma la ronda, se recupera el mazo completo y se alterna la mano para el nuevo reparto. Al alcanzar quince rondas se pasa a `finalizada`, sin repartir de nuevo.
4. Mientras la partida está activa, el cliente consulta la presentación cada segundo, incluso mientras el jugador elige, y renueva presencia cada cinco segundos. Las consultas y las renovaciones continúan durante la pausa. El plazo de sesión es de quince segundos desde la última renovación. Estos valores son iniciales para la prueba de red. Si falta alguna sesión durante el juego, se pausa la admisión de nuevas elecciones y se conserva la solicitud vigente. Las consultas siguen disponibles para el participante presente.
5. Tras un reinicio del servidor se invalidan las sesiones activas y se restaura la partida desde SQLite. Los dos participantes se reincorporan con sus credenciales y la partida retoma su fase y mano guardadas. Cerrar un cliente o perder la conexión conserva la partida. El abandono explícito finaliza por abandono sin sumar una victoria de juego. El operador puede cancelar una partida detenida.
6. La revisión aumenta al iniciar, aceptar cada elección o finalizar. Renovar presencia, pausar o reincorporarse conserva la revisión. El cliente aplica cada presentación completa y detecta la pausa o reanudación mediante `pausada` y `solicitud`, aunque `revision` sea la misma. Mientras la partida está pausada, conserva cualquier elección pendiente y espera a que la solicitud vuelva a estar disponible. Una operación pendiente conserva su identificador y puede retomarse tras la pausa si la solicitud y revisión siguen vigentes.

## Persistencia, concurrencia y catálogo

| Tabla | Identidad, relaciones e información persistida |
|---|---|
| PersonajeCatalogo | ID entero, datos completos, seis puntajes y estado activo |
| Partida | UUID, fase, revisión, ronda/combate actuales, motivo de fin y ganador |
| Participante | Clave partida y plaza, hash de credencial, nombre y marcador de rondas |
| PersonajePartida | Clave partida e ID de personaje, copia inmutable de los datos y estado de ubicación (mazo, mano J1/J2 o jugada), con orden de mazo |
| Ronda | Clave partida y número, mano inicial, victorias por jugador y ganador |
| Combate | Clave partida, ronda y número, elector, cartas, atributo, puntajes y ganador |
| Solicitud | UUID, partida, participante, combate, tipo y estado vigente o consumida |
| Operacion | Clave partida, participante e ID de operación, datos normalizados de la elección y recibo |

El operador carga o actualiza el catálogo mediante importación local del formato DataBases definido en la actividad 5. Se valida todo el lote antes de confirmar altas y cambios por ID en una sola transacción. Un error deja intacto el catálogo vigente y se informa el registro inválido. El operador retira personajes desactivándolos por ID. Las partidas iniciadas usan únicamente su copia, incluso después de reiniciar o desactivar un personaje. La conversión al CSV conserva el pendiente acordado en la actividad 5.

Cada escritura de juego usa una transacción SQLite con adquisición del turno de escritura antes de leer el estado para validar. Se comprueba primero si existe el recibo propio. Si los campos normalizados coinciden se devuelve el recibo guardado sin aplicar otra transición. Si difieren, se rechaza con `operacionIncompatible`. Para una operación nueva se comprueban presencia, fase, revisión, titular, solicitud, carta y atributo. Se confirman juntos estado, solicitud consumida, revisión y recibo. Si la escritura no está disponible se devuelve `503`; el cliente reutiliza el identificador original. Los recibos se conservan junto con la partida, incluso finalizada. Inicio y cancelación locales validan la transición desde la fase persistida antes de modificarla.

## Verificación en dos equipos

El usuario informó que dos procesos se comunicaron por localhost. La prueba entre equipos sigue pendiente.


1. En ambos equipos registrar `dotnet --info` y `dotnet --list-runtimes`. En el servidor registrar la IPv4 local con `ipconfig` y comprobar que TCP 5000 esté libre antes de iniciar y en escucha en la interfaz de red después.
2. Desde el segundo equipo ejecutar `Test-NetConnection <IPv4-del-servidor> -Port 5000`. Si falla, comprobar primero la IP, la escucha y las condiciones de red indicadas en el apartado de conexión.
3. Iniciar J1 y J2 con sus credenciales y direcciones configuradas. Comprobar incorporación, manos privadas, secuencia de combate y presentación de resultados desde cada equipo. Registrar fecha, equipos, dirección, versión probada, resultado por paso y evidencia.
4. Usar los escenarios de fallos de la [actividad 7](Plan.md#actividad-7--casos-de-prueba) para cubrir indisponibilidad, desconexión, reintento y elecciones concurrentes. La ejecución queda pendiente hasta disponer del servidor y ambos equipos.
