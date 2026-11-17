# Diseño de la interfaz de Juego Guerra

Trabajo de análisis y diseño para TI. La base es el cliente de consola previsto en la [consigna](Consigna.md), las [reglas](Reglas-del-juego.md) y la [arquitectura](Arquitectura.md#interfazconsola).

## Estados del cliente

| Estado | Información visible y acciones |
|---|---|
| Inicio y preparación | Identidad local cuando se asigne, preparación y configuración requerida por el protocolo. |
| Espera inicial | Motivo conocido de la espera, por ejemplo, inicio de partida o llegada del rival. |
| Inicio de ronda | Número de ronda, marcador acumulado, jugador mano y tres cartas propias. |
| Turno para abrir | Elegir una carta propia disponible y uno de los seis atributos. |
| Espera de la apertura rival | Mano propia, marcador y aviso de espera. Las elecciones rivales permanecen ocultas. |
| Turno para responder | Elegir una carta propia disponible. La carta y el atributo elegidos por el rival permanecen ocultos. |
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
- **Espera.** Distinguir ausencia del rival, recurso compartido o servidor inaccesible, estado desactualizado y respuesta inválida cuando el protocolo permita identificarlos.
- **Cierre.** Distinguir victoria, salida solicitada, abandono y fallo fatal. La política ante fallos sigue pendiente de conciliación entre las reglas integradas y los diseños de la [actividad 6](Plan.md#actividad-6--archivo-de-partida-compartida).

## Decisiones pendientes

- Confirmar números y Enter como control principal y decidir si también se admitirán flechas.
- Decidir si se admite entrada redirigida para jugar y probar.
- Definir si los resultados avanzan automáticamente, requieren confirmación local o permanecen visibles junto a la siguiente acción.
- Definir cuándo mostrar el arte y la descripción. La propuesta es consultarlos en una ficha separada.
- Definir inicio, asignación del rol, confirmación de salida y recuperación según el protocolo. Confirmar si el diseño también abarca la consola del servidor.

## Escenarios para revisar el diseño

- Recorrer la partida como J1 y J2. Validar que el rival no vea la carta ni el atributo antes de la resolución.
- Revisar una ronda 2–0, una ronda 2–1 con empate de atributos y el cierre al alcanzar 15 rondas ganadas.
- Combinar una ventana estrecha y baja con cartas extensas, redimensionamiento, redirección y fin de entrada.
- Cambiar el turno mientras se escribe y perder la comunicación justo después de enviar una elección.
