# Diseño de la interfaz de Juego Guerra

Trabajo de análisis y diseño para TI. La base es el cliente de consola previsto en la [consigna](Consigna.md), las [reglas](Reglas-del-juego.md) y la [arquitectura](Arquitectura.md#interfazconsola).

El [comparador de propuestas](diagramas/juego-guerra-propuestas.html) reúne las maquetas estáticas del inicio, la mano, el resultado de combate y el cierre de ronda.

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
- **Espera.** Distinguir ausencia del rival, servidor inaccesible, estado desactualizado y respuesta inválida cuando el protocolo permita identificarlos.
- **Cierre.** Distinguir victoria, salida solicitada, abandono y fallo fatal. El [protocolo HTTP](Protocolo-partida-web.md#estados-presencia-y-recuperación) define pausa, abandono y cancelación.

## Decisiones pendientes

- Confirmar números y Enter como control principal y decidir si también se admitirán flechas.
- Decidir si se admite entrada redirigida para jugar y probar.
- Definir si los resultados avanzan automáticamente, requieren confirmación local o permanecen visibles junto a la siguiente acción.
- Definir cuándo mostrar el arte y la descripción. La propuesta es consultarlos en una ficha separada.
- Definir cómo se presentan la incorporación, el rol asignado, la recuperación y la confirmación de salida. Decidir si el diseño visual abarca la consola del servidor.

## Aspecto de las pantallas

Las maquetas del HTML muestran una consola con espacio suficiente y un flujo de juego normal. Lucía y Mateo son nombres de ejemplo. En la aplicación se muestran los nombres reales en encabezados, turnos, marcadores, esperas y resultados. J1 y J2 son identificadores internos.

### Inicio y preparación · Pendiente de elección

Las [cuatro maquetas](diagramas/juego-guerra-propuestas.html#inicio) muestran el título del juego, el nombre local cuando está disponible y el estado de preparación. Bienvenida centrada da prioridad al título y al saludo, panel de preparación separa la identidad del estado, arranque por líneas presenta la información en secuencia y estado destacado pone la preparación en primer plano. Los datos de configuración y las acciones de inicio dependen del protocolo.

### Mano y elección · Lista con ficha

En la [maqueta elegida](diagramas/juego-guerra-propuestas.html#detalle), el encabezado muestra el nombre del jugador local, la ronda, el combate y el marcador acumulado. La mano se presenta en dos bloques alineados.

- **Izquierda.** Lista numerada de cartas disponibles, con la carta en foco identificada junto a su nombre.
- **Derecha.** Caja con el nombre y los seis atributos de la carta en foco. Las etiquetas se alinean a la izquierda y los puntajes a la derecha. Solo la ficha lleva borde.
- **Pie.** Instrucción breve sobre la acción disponible. El texto del HTML representa la elección de carta al abrir el combate. Los controles definitivos siguen pendientes.

Para componer la vista, se preparan ambos bloques en memoria, se mide su ancho en columnas visibles de terminal y se unen fila por fila con una separación fija. La lista se rellena con espacios cuando tiene menos filas que la ficha. Los textos largos se ajustan dentro de su bloque para mantener la alineación de la caja.

### Resultado de combate · Duelo en paralelo

En la [maqueta elegida](diagramas/juego-guerra-propuestas.html#duelo-paralelo), el atributo comparado aparece encima de dos paneles enfrentados. Cada panel muestra el nombre del jugador, la carta jugada y el valor del atributo. El valor tiene más énfasis que el resto del contenido y el panel ganador lleva la etiqueta «Ganador». El pie muestra el marcador de combates y, si la ronda continúa, quién elige el próximo atributo.

### Cierre de ronda · Pendiente de elección

Las [cuatro maquetas](diagramas/juego-guerra-propuestas.html#cierre-ronda) presentan el mismo resultado y cambian qué se lee primero. Victoria destacada pone al ganador arriba, marcador enfrentado da una columna a cada jugador, resumen en planilla separa combates y rondas en dos filas, y transición pone primero quién abre la ronda siguiente. Si un jugador alcanza 15 rondas ganadas, corresponde presentar el cierre de partida.

## Composición propia y uso de Utilidades

La interfaz tiene una distribución y una interacción personalizadas. La composición de pantallas, cajas, columnas y resaltados se resolverá con métodos propios. El uso directo de `Utilidades.Funciones` y `Utilidades.DecoracionConsola` será excepcional y se evaluará caso por caso, cuando un método resuelva una necesidad concreta y encaje con el diseño.

El menú de la mano tendrá su propio código para mantener coordinados el foco de la lista, la ficha visible y la instrucción. Los métodos de Utilidades pueden servir como referencia para resolver partes similares, como leer teclas, validar opciones, recorrer la lista o actualizar el elemento en foco. La navegación definitiva se decide por separado. `MyConsole` puede seguir sirviendo como mecanismo de escritura de las líneas compuestas.

## Escenarios de prueba de la interfaz

- Recorrer la partida como J1 y J2. Validar que el rival no vea la carta ni el atributo antes de la resolución.
- Probar una ronda 2–0, una ronda 2–1 con empate de atributos y el cierre al alcanzar 15 rondas ganadas.
- Combinar una ventana estrecha y baja con cartas extensas, redimensionamiento, redirección y fin de entrada.
- Cambiar el turno mientras se escribe y perder la comunicación justo después de enviar una elección.
