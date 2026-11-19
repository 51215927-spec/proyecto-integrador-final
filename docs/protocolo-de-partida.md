# API REST y operación del servidor

HTTP en red local está aprobado para la entrega de la [actividad 6](consigna.md). Este documento describe el contrato REST y la operación del servidor de la solución. Los clientes de consola y la comprobación en dos computadoras siguen pendientes.

- [Despliegue y conexión](#organización-propuesta)
- [Operación del servidor](#operación-del-servidor)
- [Identidad y sesiones](#administración-e-identidad)
- [Operaciones, datos y errores HTTP](#contrato-http)
- [Estados y recuperación](#estados-presencia-y-recuperación)
- [Persistencia y catálogo](#persistencia-concurrencia-y-catálogo)
- [Verificación en dos equipos](#verificación-en-dos-equipos)

## Organización propuesta

El diseño prevé dos clientes de consola que se comunican con el servidor mediante una API, la interfaz que expone sus operaciones. ASP.NET Core recibe las solicitudes HTTP y devuelve respuestas. JSON es el formato de los datos que se intercambian. El servidor dirige el juego y es la única autoridad sobre cartas, turnos, resultados y marcadores. Cada cliente recibe exclusivamente la información que puede ver y devuelve las elecciones solicitadas.

EF Core administra la persistencia en una base SQLite local al servidor, tanto del catálogo permanente de personajes (nombre, descripción, ASCII y seis atributos) como de los datos de partidas. Los clientes acceden exclusivamente a la API. El esquema y las transacciones se definen en este documento.

En el primer equipo Windows se ejecutan el servidor y el cliente J1 como procesos separados. En el segundo se ejecuta el cliente J2. Se abre primero el servidor y después ambos clientes. Las versiones acordadas están en [Tecnologías REST](tecnologias.md).

La API y el dominio tienen responsabilidades distintas: la API identifica al jugador y valida el contrato recibido; el dominio decide si la elección es válida y resuelve el juego; la persistencia confirma los cambios. Los objetos de transferencia de datos (DTO) representan los mensajes de red. Las entidades de EF Core representan los registros que el servidor guarda en SQLite.

## Conexión en red local

El diseño prevé conexiones HTTP en la red local por el puerto TCP 5000. Cada cliente de juego deberá leer la dirección base del servidor de su configuración local al iniciar. Una URL de ejemplo es `http://192.168.1.20:5000`. En la instalación se usa la IP asignada al equipo servidor.

Localhost identifica la computadora del propio proceso: sirve para probar clientes y servidor en el mismo equipo. El cliente de otra computadora debe usar la dirección de red del servidor. Este debe aceptar conexiones en una interfaz accesible desde esa red, no limitarse a localhost; El servidor escucha por defecto en `0.0.0.0:5000`. La opción `--urls` permite elegir otra dirección de escucha.

Si el servidor no está disponible, el cliente informa y ofrece reintentar o salir. Conserva una elección pendiente y no la transforma en una operación nueva al reintentar. La confirmación de una elección cuya respuesta se perdió se resolverá mediante los contratos de operaciones e idempotencia de este documento; no se asume que falló por un error de conexión.

HTTP no cifra el intercambio. Su elección se limita al entorno local de prueba acordado; no se define publicación en Internet. Primero se probará la comunicación para determinar si existe un bloqueo, sin asumir que sea necesario abrir el firewall. De requerirse una excepción en un equipo escolar, se coordinará con el responsable y se limitará al servicio, puerto y equipos participantes. No se requiere desactivar el firewall ni abrir puertos del router hacia Internet.

## Operación del servidor

El siguiente comando inicia el servidor desde la raíz del repositorio.

```powershell
dotnet run --project src/servidor/servidor.csproj
```

Por defecto el servidor crea `guerra.db` en el directorio de ejecución. Se puede cambiar con `ConnectionStrings__Juego`, por ejemplo `Data Source=C:/datos/guerra.db;Default Timeout=1`. Si se elige otro directorio, debe existir y permitir escritura al proceso.

| Comando en la consola del servidor | Efecto |
|---|---|
| `crear` | Crea una partida y muestra su UUID y las dos credenciales. Entregar cada credencial solo al jugador de su plaza. |
| `iniciar <idPartida>` | Inicia cuando ambas sesiones están presentes y hay al menos seis personajes activos. |
| `cancelar <idPartida>` | Finaliza una partida pendiente o activa con motivo `cancelacion`. |
| `cerrar-sesion <idPartida> <J1|J2>` | Libera la plaza para otra sesión y conserva su estado de juego. |
| `reemplazar-credencial <idPartida> <J1|J2>` | Revoca el acceso anterior y muestra una credencial nueva para la misma plaza. |
| `desactivar <idPersonaje>` | Excluye el personaje de futuros repartos de partidas nuevas. |
| `ayuda` | Muestra los comandos disponibles. |

Para iniciar una partida, cargar al menos seis personajes activos con `subir-personajes`. Ejecutar `crear`, incorporar a ambos jugadores y luego ejecutar `iniciar` con el UUID. El servidor conserva los datos de juego al reiniciar. Para seguir una partida, ambos jugadores se reincorporan con sus credenciales.

### Prueba del servidor en una sola computadora

Desde la raíz del repositorio, usá una base de prueba para que el catálogo y las partidas del ensayo queden en un archivo propio. En PowerShell, ejecutá:

```powershell
$env:ConnectionStrings__Juego = 'Data Source=guerra-prueba.db;Default Timeout=1'
dotnet run --project src/servidor/servidor.csproj -- --urls http://127.0.0.1:5000
```

- Esperá el mensaje de escucha en `http://127.0.0.1:5000`. En esa misma terminal, escribí `crear`. La consola imprime el UUID de la partida y una credencial para cada plaza.
- Mantené abierta esa terminal mientras probás la API desde otra. Para iniciar el juego, cargá primero al menos seis personajes activos con [subir-personajes](instalacion-y-uso.md#cargar-personajes-manualmente).
- Incorporá ambos participantes mediante `PUT /participantes/me/sesion` según el contrato de abajo. Usá un UUID de sesión diferente por jugador y renová la presencia con `PUT /participantes/me/presencia` mientras realizás la prueba. Después, escribí `iniciar <idPartida>` en la consola del servidor. El resultado esperado es `Partida iniciada.`.
- Consultá `GET /presentacion` con la credencial y sesión de cada plaza. Al inicio, ambas vistas muestran tres cartas propias. La vista de J1 incluye una solicitud `cartaYAtributo` y la de J2 tiene `solicitud: null` hasta que J1 juegue.

Si el inicio falla, verificá que el UUID corresponda a la partida creada, que ambas sesiones sigan vigentes y que el catálogo tenga suficientes personajes activos. Al terminar, detené el servidor con Ctrl+C. Si volvés a iniciarlo con el mismo archivo, conserva la partida y requiere que ambos participantes vuelvan a incorporarse.

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
- **Solicitud** es nula si el jugador debe esperar o la partida está pausada o terminada. Si existe, contiene `idSolicitud`, `tipo` (`cartaYAtributo` o `carta`), `idsPersonajePermitidos` y `atributosPermitidos`, vacío para `carta`. Los nombres publicados de los atributos son `Fuerza`, `Velocidad`, `Resistencia`, `Inteligencia`, `Energía` y `Habilidad`. Al recibir una elección se aceptan diferencias de mayúsculas, espacios exteriores y la variante `Energia`. Para `carta`, omitir `atributo` o enviarlo como `null`. El rival desconoce carta y atributo hasta la resolución.
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

Una cabecera `X-Sesion` ausente, inválida o sin sesión vigente devuelve `409 sesionVencida`. Durante una incorporación, un nombre distinto del registrado devuelve `403 sinPermiso` cuando la plaza está disponible para esa instancia.

El error incluye `codigo`, `mensaje` e `idSeguimiento`; sus campos excluyen elecciones ocultas y detalles internos.

## Estados, presencia y recuperación

1. En `esperandoInicio`, el operador inicia solo con ambas plazas presentes y seis personajes válidos como mínimo. El servidor copia el catálogo activo para la partida, mezcla, reparte tres personajes por jugador y abre `esperandoElector` con J1 como mano de la primera ronda.
2. Una elección válida del elector reserva su carta y atributo y cambia a `esperandoRival`. La elección rival resuelve el combate, retira ambas cartas y publica el resultado. Un historial persistido permite que los clientes muestren cada resultado una vez, sin exigir confirmación para avanzar.
3. Si continúa la ronda, alterna el elector. Al alcanzar dos combates ganados se suma la ronda, se recupera el mazo completo y se alterna la mano para el nuevo reparto. Al alcanzar quince rondas se pasa a `finalizada`, sin repartir de nuevo.
4. Mientras la partida está activa, el cliente consulta la presentación cada segundo, incluso mientras el jugador elige, y renueva presencia cada cinco segundos. Las consultas y las renovaciones continúan durante la pausa. El plazo de sesión es de quince segundos desde la última renovación. Estos valores son iniciales para la prueba de red. Si falta alguna sesión durante el juego, se pausa la admisión de nuevas elecciones y se conserva la solicitud vigente. Las consultas siguen disponibles para el participante presente.
5. Tras un reinicio del servidor se invalidan las sesiones activas y se restaura la partida desde SQLite. Los dos participantes se reincorporan con sus credenciales y la partida retoma su fase y mano guardadas. Cerrar un cliente o perder la conexión conserva la partida. El abandono explícito finaliza por abandono sin sumar una victoria de juego. El operador puede cancelar una partida detenida.
6. La revisión aumenta al iniciar, aceptar cada elección, abandonar o cancelar. La elección que confirma la victoria y finaliza la partida produce un solo incremento. Renovar presencia, pausar o reincorporarse conserva la revisión. El cliente aplica cada presentación completa y detecta la pausa o reanudación mediante `pausada` y `solicitud`, aunque `revision` sea la misma. Mientras la partida está pausada, conserva cualquier elección pendiente y espera a que la solicitud vuelva a estar disponible. Una operación pendiente conserva su identificador y puede retomarse tras la pausa si la solicitud y revisión siguen vigentes.

## Persistencia, concurrencia y catálogo

Para una elección nueva, el servidor autentica la plaza y verifica la sesión vigente. Luego comprueba presencia, revisión, solicitud y opciones, en ese orden. Si acepta la elección, incrementa la revisión y persiste el recibo junto con la transición. El `POST` responde `201` con la ubicación del recibo.

Para un reenvío, el servidor busca primero el `idOperacion` dentro de la partida y la plaza autenticada. Si el cuerpo coincide con la elección guardada tras normalizar el atributo, responde `200` con el recibo original, aunque hayan cambiado la revisión o el turno. Si alguno de los datos difiere, responde `409 operacionIncompatible`. La consulta `GET /elecciones/{idOperacion}` requiere sesión vigente y devuelve `404` si no existe un recibo para esa plaza.

[Esquema y persistencia de la elección](bases-de-datos.md#solicitudes-y-operaciones)

### Alta individual de personajes

`POST /api/v1/personajes` recibe JSON UTF-8 con `nombre`, `descripcion`, `ascii`, `fuerza`, `velocidad`, `resistencia`, `inteligencia`, `energia` y `habilidad`. La descripción admite texto vacío. Los puntajes son enteros y el servidor aplica las [validaciones de Personaje](reglas-del-juego.md#personajes-y-preparación). El ASCII admite hasta 25 líneas, con un máximo de 120 caracteres por línea y sin tabulaciones.

El operador configura una credencial de catálogo en el entorno del servidor mediante `Catalogo__Credencial`. El cliente la envía en `Authorization: Bearer`. El alta devuelve `401 credencialInvalida` si falta la configuración o la credencial recibida no coincide. Las credenciales de las plazas se usan para la partida. El cliente [`subir-personajes`](../src/subir-personajes/README.md) usa esta ruta.

| Respuesta | Significado |
|---|---|
| `201` | Guarda el personaje activo y devuelve `{"idPersonaje":17}`. El ID es un entero positivo asignado por el servidor. |
| `400 entradaInvalida` | Los campos, tipos o valores del personaje no son válidos. |
| `401 credencialInvalida` | El alta requiere la credencial de catálogo. |
| `409 operacionIncompatible` | Se agotó el rango para asignar un ID de alta automática. |
| `503 servicioNoDisponible` | La base de datos no pudo confirmar el alta. |

Cada solicitud de alta válida crea un personaje. La idempotencia por `idOperacion` se aplica a las elecciones de partida.

### Mantenimiento del catálogo

El operador retira personajes desactivándolos por ID. Las partidas iniciadas conservan su copia de los personajes después de reiniciar o desactivar un personaje.

Cada escritura de juego usa una transacción SQLite con adquisición del turno de escritura antes de leer el estado para validar. Se comprueba primero si existe el recibo propio. Si los campos normalizados coinciden se devuelve el recibo guardado sin aplicar otra transición. Si difieren, se rechaza con `operacionIncompatible`. Para una operación nueva se comprueban presencia, fase, revisión, titular, solicitud, carta y atributo. Se confirman juntos estado, solicitud consumida, revisión y recibo. Si la escritura no está disponible se devuelve `503`; el cliente reutiliza el identificador original. Los recibos se conservan junto con la partida, incluso finalizada. Inicio y cancelación locales validan la transición desde la fase persistida antes de modificarla.

## Verificación en dos equipos

El usuario informó que dos procesos se comunicaron por localhost. La prueba entre equipos sigue pendiente.


1. En ambos equipos registrar `dotnet --info` y `dotnet --list-runtimes`. En el servidor registrar la IPv4 local con `ipconfig` y comprobar que TCP 5000 esté libre antes de iniciar y en escucha en la interfaz de red después.
2. Desde el segundo equipo ejecutar `Test-NetConnection <IPv4-del-servidor> -Port 5000`. Si falla, comprobar primero la IP, la escucha y las condiciones de red indicadas en el apartado de conexión.
3. Iniciar J1 y J2 con sus credenciales y direcciones configuradas. Comprobar incorporación, manos privadas, secuencia de combate y presentación de resultados desde cada equipo. Registrar fecha, equipos, dirección, versión probada, resultado por paso y evidencia.
4. Usar los escenarios de fallos de la [actividad 7](plan-de-pruebas.md) para cubrir indisponibilidad, desconexión, reintento y elecciones concurrentes. La ejecución queda pendiente hasta disponer del servidor y ambos equipos.

## Ejemplo de elección

En una partida iniciada, consultá primero `GET /presentacion` con la credencial de la plaza y `X-Sesion`. Usá la revisión, la solicitud y las opciones de esa respuesta. Con las mismas cabeceras enviá `POST /elecciones` con `Content-Type: application/json`. Este ejemplo muestra una solicitud de carta y atributo con valores ilustrativos.

```json
{
  "idOperacion": "d2f03b38-3cdf-45c1-933f-0b336ffbb79c",
  "idSolicitud": "33c32892-c5bc-4a95-8c0a-36bd1c3c1195",
  "revisionEsperada": 1,
  "idPersonaje": 2,
  "atributo": "Fuerza"
}
```

Una elección aceptada devuelve `201 Created` y un recibo como el siguiente.

```json
{
  "idOperacion": "d2f03b38-3cdf-45c1-933f-0b336ffbb79c",
  "idSolicitud": "33c32892-c5bc-4a95-8c0a-36bd1c3c1195",
  "revisionConfirmada": 2
}
```

Si se pierde la respuesta, consultá `GET /elecciones/{idOperacion}` con la misma plaza. Si la operación ya se confirmó y reenviás los datos originales, el servidor devuelve `200` con el mismo recibo.
