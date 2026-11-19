# Diseño de la interfaz de Juego Guerra

Trabajo de análisis y diseño para TI. La base es el cliente de consola previsto en la [consigna](consigna.md), las [reglas](reglas-del-juego.md) y la [arquitectura](arquitectura.md#proyectos).

[El comparador de propuestas](juego-guerra-propuestas.html), reúne cuatro maquetas estáticas para cada uno de los doce estados del cliente que se detallan abajo.

## Estados del cliente

| Estado | Información visible y acciones |
|---|---|
| Inicio y preparación | Identidad local cuando se asigne, preparación y configuración requerida por el protocolo. |
| Espera inicial | Motivo conocido de la espera, por ejemplo, inicio de partida o llegada del rival. |
| Inicio de ronda | Número de ronda, marcador acumulado, jugador mano y tres cartas propias. |
| Turno para abrir | Elegir una carta propia disponible y uno de los seis atributos. |
| Espera de la apertura rival | Mano propia, marcador y aviso de espera. Las elecciones rivales permanecen ocultas. |
| Turno para responder | Elegir una carta propia disponible en la misma mesa de fichas que se usa al abrir. La carta y el atributo elegidos por el rival permanecen ocultos. |
| Envío o espera de resultado | Distinguir envío, recepción confirmada y confirmación pendiente. Evitar una segunda jugada sobre la misma solicitud. |
| Combate resuelto | Mostrar ambas cartas jugadas, atributo, valores, ganador y marcador de combates. Si empatan, gana quien respondió. Si sigue la ronda, alterna quien elige atributo. |
| Ronda resuelta | Mostrar ganador y marcador acumulado. Con dos victorias de combate se cierra la ronda, aunque quede una carta. Si sigue la partida, alterna el jugador mano y se vuelven a repartir tres cartas por jugador. |
| Partida ganada | Mostrar ganador y marcador al alcanzar 15 rondas ganadas. Habilitar la salida. |
| Estado sin confirmar o suspendido | Mostrar el motivo conocido y distinguir el último marcador confirmado del estado actual. Esperar, reintentar o salir según las opciones que admita el protocolo. |
| Interrupción o abandono | Informar la causa conocida y el efecto local o en toda la partida cuando esté confirmado. |

Si cambia el estado mientras se introduce una opción, volver a comprobar si la acción sigue vigente antes de aplicarla. Una mano vacía durante la espera puede ser válida. Si se solicita una carta con la mano vacía, informar una inconsistencia. La interfaz presenta las acciones y resultados del flujo de partida; este determina las jugadas válidas y actualiza los marcadores.

## Consola e interacción

Las siguientes respuestas son propuestas de diseño.

| Caso | Respuesta propuesta |
|---|---|
| Ventana angosta o baja | Usar bloques verticales con desplazamiento. Priorizar opciones, valores y acción requerida. |
| Redimensionamiento o tamaño desconocido | Adaptar la próxima presentación sin cambiar el significado de las opciones vigentes. Usar texto lineal si no se conocen las dimensiones. |
| Cursor, limpieza o colores no disponibles | Presentar por líneas y distinguir acciones y resultados mediante texto. |
| Arte ancho, texto largo o caracteres incompatibles | Ofrecer una ficha separada y conservar nombre y atributos aunque el arte no pueda mostrarse. Usar símbolos simples como alternativa y neutralizar caracteres de control de los datos. |
| Salida redirigida | Emitir texto lineal sin repintado. Permitir la selección por líneas si la entrada sigue disponible y evitar repetir mensajes de espera sin cambios. |
| Entrada redirigida o sin teclado | Definir si se permiten selecciones desde un flujo de líneas. Si no hay entrada utilizable, informar y cerrar sin quedar esperando. |
| Fin de entrada | Distinguirlo de una línea vacía y terminar la solicitud. Una jugada en tránsito sigue pendiente hasta que el protocolo determine su estado. |
| Entrada inválida, vacía, extensa o fuera de rango | Indicar las opciones válidas y volver a pedir una opción si el turno sigue vigente. |
| Doble Enter, tecla repetida o texto pegado | Admitir una elección por solicitud y evitar que la entrada sobrante confirme la siguiente. |
| Error de escritura o tubería cerrada | Intentar informar por la salida de errores y cerrar si el cliente pierde la capacidad de interacción. |
| Ctrl+C, cierre de ventana o pérdida de sesión | Tramitar una salida controlada si se recibe aviso. Ante un cierre abrupto, el protocolo determina cómo se detecta la ausencia. |
| Cambio de estado mientras se escribe | Presentar el cambio sin mezclarlo con la entrada. Retirar el menú si quedó obsoleto. |

## Errores de la aplicación

- **Preparación.** Catálogo ausente, ilegible o inválido, configuración incorrecta y comunicación inaccesible. Para menos de seis cartas válidas, mostrar «No se puede jugar porque no hay suficientes cartas».
- **Elección.** Carta no disponible, turno cambiado, solicitud consumida, rechazo o confirmación perdida. Una pérdida de comunicación tras el envío deja el resultado incierto hasta comprobarlo.
- **Espera.** Distinguir ausencia del rival, servidor inaccesible, estado desactualizado y respuesta inválida cuando el protocolo permita identificarlos.
- **Cierre.** Distinguir victoria, salida solicitada, abandono y fallo fatal. El [protocolo HTTP](protocolo-de-partida.md#estados-presencia-y-recuperación) define pausa, abandono y cancelación.

## Controles de la implementación

La consola limpia la pantalla al cambiar de vista en una terminal. Para elegir, mové el foco con las flechas, WASD o HJKL y pulsá Enter. La opción activa se marca con `>` y un resaltado si hay color disponible. El foco se detiene en los extremos de la lista. Con entrada redirigida, enviá una letra de movimiento por línea y una línea vacía para confirmar. El fin de entrada termina la solicitud mediante `EndOfStreamException`. La salida redirigida apila las fichas y conserva el historial de pantallas.

El flujo cliente debe comprobar que la solicitud siga vigente antes de enviar una opción. También debe gestionar el cambio de estado durante la escritura, la entrada sobrante entre solicitudes, las confirmaciones, la recuperación y la salida.

## Decisiones pendientes

- Definir cómo se descarta la entrada interactiva sobrante entre solicitudes.
- Definir cuándo mostrar el arte y la descripción. La propuesta es consultarlos en una ficha separada.
- Definir cómo se presentan la incorporación, el rol asignado, la recuperación y la confirmación de salida. Decidir si el diseño visual abarca la consola del servidor.

## Aspecto de las pantallas

Las maquetas del HTML muestran una consola con espacio suficiente y cubren el flujo de juego, las esperas y los cierres. Lucía y Mateo son nombres de ejemplo. En la aplicación se muestran los nombres reales en encabezados, turnos, marcadores, esperas y resultados. J1 y J2 son identificadores internos.

### Propuestas elegidas

| Estado | Propuesta |
|---|---|
| Inicio y preparación | 02 · Panel de preparación |
| Espera inicial | 02 · Paneles de información |
| Inicio de ronda | 02 · Paneles de información |
| Mano y elección | 01 · Mesa de cartas |
| Espera de la apertura rival | 02 · Paneles de información |
| Turno para responder | Mesa de cartas, con el marcador y sin elección de atributo |
| Envío y espera de resultado | 01 · Mensaje principal |
| Resultado de combate | 01 · Duelo en paralelo |
| Cierre de ronda | 01 · Victoria destacada |
| Cierre de partida | 03 · Resumen en planilla |
| Estado sin confirmar o suspendido | 01 · Mensaje principal |
| Interrupción o abandono | 01 · Mensaje principal |

### Mano y elección · Mesa de cartas

En la maqueta «Mesa de cartas», el encabezado muestra el nombre del jugador local, la ronda, el combate y el marcador acumulado. Las fichas aparecen en paralelo con el nombre y los seis atributos de cada carta. El pie indica la acción disponible durante el turno.

### Resultado de combate · Duelo en paralelo

En la maqueta «Duelo paralelo», el atributo comparado aparece encima de dos paneles enfrentados. Cada panel muestra el nombre del jugador, la carta jugada y el valor del atributo. El valor tiene más énfasis que el resto del contenido y el panel ganador lleva la etiqueta «Ganador». El pie muestra el marcador de combates y, si la ronda continúa, quién elige el próximo atributo.

## Composición con Spectre.Console

El cliente conserva la composición de las doce pantallas elegidas y usa [Spectre.Console](https://spectreconsole.net/console/) para renderizar las barras de estado, los paneles, las fichas y la planilla final sobre .NET 8. Aplica los acentos verdes de las maquetas cuando la terminal admite color. En pantallas angostas o con salida redirigida, apila los paneles y conserva el orden de lectura. El menú resalta la opción activa y actualiza la vista al mover el foco.

## Demostración local

Desde la raíz de la rama que contiene el cliente actualizado, ejecutá:

```bash
dotnet run --project src/cliente -- --demo
```

La demo comienza con Lucía y Mateo empatados 14–14. Usa un mazo de ejemplo y ejecuta la ronda decisiva localmente.

1. Pulsá Enter en los paneles de preparación, espera e inicio de ronda.
2. En la apertura, mové el foco hasta una carta y confirmá con Enter. Repetí la selección en el menú de atributos.
3. Avanzá por los estados de envío y revisá el duelo. La pantalla muestra ambas cartas, sus puntajes en el atributo usado y el marcador de combates.
4. Cuando abre Mateo, avanzá desde la espera y elegí solo una carta. La demo usa Inteligencia para comparar este combate y muestra el atributo en el resultado.
5. Al ganar dos combates, el vencedor de la ronda alcanza 15 victorias. El resumen final muestra quién ganó y el marcador 15–14.

Para probar un cierre específico, agregá uno de estos argumentos después de `--demo`. Por ejemplo, ejecutá `dotnet run --project src/cliente -- --demo pausa`.

| Argumento | Pantalla esperada |
|---|---|
| `pausa` | Suspensión por pérdida de sesión, seguida de un estado de comunicación sin confirmar. |
| `abandono` | Interrupción confirmada porque el rival abandonó. |
| `cancelacion` | Interrupción confirmada porque el operador canceló la partida. |
| `salida` | Cierre local del cliente con la partida guardada en el escenario simulado. |

Con entrada y salida redirigidas, los paneles se imprimen en orden, las pantallas informativas avanzan automáticamente y solo las selecciones consumen líneas de entrada. Una línea vacía confirma la opción activa. La demostración simula las pantallas y sus transiciones. La conexión HTTP, los recibos reales y la recuperación siguen pendientes en el cliente.

## Escenarios de prueba de la interfaz

- Recorrer la partida como J1 y J2. Validar que el rival no vea la carta ni el atributo antes de la resolución.
- Probar una ronda 2–0, una ronda 2–1 con empate de atributos y el cierre al alcanzar 15 rondas ganadas.
- Combinar una ventana estrecha y baja con cartas extensas, redimensionamiento, redirección y fin de entrada.
- Cambiar el turno mientras se escribe y perder la comunicación justo después de enviar una elección.
