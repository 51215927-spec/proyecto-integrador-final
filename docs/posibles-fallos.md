# Actividad 7 — Plan de pruebas y posibles fallos

Este plan de TI especifica resultados esperados. La ejecución y el registro de resultados corresponden a Taller de Programación. Los casos se ejecutan con datos aislados y se restaura el estado inicial antes de repetirlos.

**Caja negra** comprueba resultados observables por consola, API o contrato público del dominio. **Caja blanca** inspecciona el estado persistido o controla un punto de fallo para comprobar efectos que la respuesta por sí sola no demuestra.

Los resultados esperados se basan en la [consigna](consigna.md), las [reglas](reglas-del-juego.md), la [validación de personajes](arquitectura.md#datos-y-validación-de-personajes--actividad-5) y el [contrato HTTP](protocolo-partida-web.md).

## Personajes y catálogo

Los vectores siguen el orden Fuerza, Velocidad, Resistencia, Inteligencia, Energía y Habilidad. En cada caso se mantiene válido el resto de los campos. Ejecutar cada vector de un caso por separado.

| Caso y técnica | Condición inicial y acción | Datos | Resultado esperado |
|---|---|---|---|
| P01 · Caja negra | Construir un personaje con nombre y ASCII válidos. | (600, 450, 700, 500, 400, 350) | Acepta el personaje y devuelve los seis puntajes ingresados. |
| P02 · Caja negra | Construir con total incorrecto. | (500, 450, 700, 500, 400, 350), total 2900 | Rechaza por el total distinto de 3000. |
| P03 · Caja negra | Construir con los límites exactos de rango. | (250, 750, 500, 500, 500, 500) | Acepta 250 y 750, ambos inclusive. |
| P04 · Caja negra | Construir con un solo valor fuera de rango. | (249, 750, 501, 500, 500, 500) y (250, 751, 499, 500, 500, 500) | Rechaza por el rango. Ambos vectores suman 3000 y cumplen los umbrales de especialización. |
| P05 · Caja negra | Construir en los umbrales exactos de especialización. | (650, 350, 500, 500, 500, 500) | Acepta 650 como alto y 350 como bajo. |
| P06 · Caja negra | Construir sin cumplir uno o ambos umbrales. | (649, 350, 501, 500, 500, 500), (650, 351, 499, 500, 500, 500) y (500, 500, 500, 500, 500, 500) | Rechaza, respectivamente, por falta de atributo alto, bajo o ambos. |
| P07 · Caja negra | Enviar un alta incompleta. | Solicitud sin un campo obligatorio | Responde con un error de entrada y conserva el catálogo vigente. |
| P08 · Caja negra | Cargar el catálogo obligatorio. | Ruta inexistente y archivo ilegible, en ejecuciones separadas | Informa el error de carga y el juego no comienza con un catálogo parcial. |
| P09 · Caja negra | Dar de alta un personaje con descripción vacía. | Descripción vacía, ASCII válido y puntajes P01 | Acepta el alta y conserva la descripción vacía. |
| P10 · Caja negra | Ingresar un puntaje no numérico en la consola. | Escribir `abc` en un atributo | Vuelve a pedir el atributo antes de enviar el alta. |

El catálogo de referencia se prepara sin duplicados, según la actividad 5. La ausencia de duplicados se verifica al preparar los datos de prueba.

## Mazo, combate y finalización

| Caso y técnica | Condición inicial y acción | Datos | Resultado esperado |
|---|---|---|---|
| J01 · Caja negra | Intentar iniciar con plazas presentes y un catálogo insuficiente. | 0 y 5 personajes válidos | Se informa que hacen falta seis personajes. La partida sigue esperando inicio y no entrega manos. |
| J02 · Caja negra | Con el mazo ya vacío, intentar repartir una carta más. | Mazo sin cartas y solicitud de reparto de 1 | El reparto se rechaza sin entregar cartas ni alterar la cantidad disponible. |
| J03 · Caja negra | Resolver un combate normal de la primera ronda. | J1 elige Fuerza, puntajes de J1 y J2 de 600 y 500 | Gana J1. Se consumen ambas cartas y J2 elige el atributo del combate siguiente si continúa la ronda. |
| J04 · Caja negra | Resolver el empate con J1 como elector y repetir con J2 como elector. | Ambos puntajes de Fuerza son 500 | Gana el jugador que no eligió el atributo, J2 y J1 respectivamente. |
| J05 · Caja negra | Resolver el tercer combate de una ronda empatada 1–1 usando las últimas cartas. | Marcador de partida 3–2, J1 gana el tercer combate | J1 suma la ronda. Ambas manos se reponen en la ronda siguiente y cambia el jugador mano. La partida continúa con marcador 4–2. |
| J06 · Caja blanca | Resolver dos combates seguidos a favor de J1 con ocho personajes iniciales. | Dos personajes quedaron en el mazo y cada jugador conserva una carta de su mano | Se cierra al segundo combate. Antes de repartir, la recuperación contiene exactamente los ocho identificadores iniciales una vez cada uno. |
| J07 · Caja negra | Resolver el combate que da la segunda victoria de una ronda al líder de la partida. | J1 tiene 14 rondas y gana la ronda actual | J1 alcanza 15 y la partida finaliza por victoria. No se abre otra ronda ni se emite una nueva solicitud. |
| J08 · Caja blanca | Iniciar con exactamente seis personajes y controlar las decisiones de mezcla para obtener un orden distinto del original. | Seis IDs diferentes y secuencia de mezcla que intercambia el primero con el último | El orden resultante refleja los intercambios fijados. Cada mano recibe tres IDs distintos, las manos son disjuntas y el mazo queda vacío. |

## Red local y consistencia HTTP

Usar dos plazas, una sesión por cliente y la URL configurada del servidor. Capturar la revisión y la solicitud de la presentación propia antes de cada envío. En los casos de pérdida de respuesta, el cliente conserva el identificador de la operación original.

| Caso y técnica | Condición inicial y acción | Datos | Resultado esperado |
|---|---|---|---|
| H01 · Caja negra | Intentar conectar sin servidor y luego iniciarlo. | Dirección configurada y TCP 5000 | El cliente informa la indisponibilidad y ofrece reintentar o salir. Tras iniciar el servidor, reintentar permite incorporarse. |
| H02 · Caja negra | J1 intenta elegir cuando la solicitud corresponde a J2. | Credencial válida de J1 y solicitud vigente de J2 | `409 solicitudNoVigente`. La solicitud de J2 y la revisión permanecen vigentes. |
| H03 · Caja negra | Enviar una jugada de un combate anterior con un identificador nuevo. | Solicitud consumida y revisión anterior | `409 revisionDesactualizada` o `solicitudNoVigente`, según la validación que detecte el desfase. La vista actual conserva mano, marcador y solicitud. |
| H04 · Caja blanca | Interrumpir la respuesta tras la confirmación de la elección y reenviar el mismo cuerpo. | Mismos UUID de operación y solicitud, revisión y elección originales | La consulta del recibo devuelve `200`. El reenvío devuelve el mismo recibo. Hay una sola operación persistida y un único incremento de revisión. |
| H05 · Caja negra | Reutilizar el UUID de una operación aceptada cambiando la selección. | Mismo UUID y otro identificador de personaje | `409 operacionIncompatible`. El recibo original permanece disponible. |
| H06 · Caja blanca | Enviar dos elecciones válidas simultáneas desde el titular de una solicitud. | UUID de operación distintos, misma solicitud y revisión, dos cartas permitidas | Una elección se confirma. La otra devuelve `409` si la solicitud ya se consumió, o `503` si la escritura está temporalmente ocupada. Si devuelve `503`, reenviar con el mismo UUID tras el primer commit devuelve `409`. Persiste un recibo y una sola transición, sin pérdida ni doble retirada de cartas. |
| H07 · Caja negra | Desconectar a J2 durante una elección pendiente de J1 durante más de 15 segundos. | Ambas plazas estaban activas. J1 mantiene su presencia | J1 ve la pausa con la misma revisión y sin solicitud visible. Un envío directo durante la pausa recibe `409 partidaPausada`. Al reincorporarse J2, la consola de J1 vuelve a mostrar la misma solicitud y mano, con la revisión intacta. |
| H08 · Caja blanca | Interrumpir el proceso servidor antes de confirmar la segunda elección del combate. | Fallo inducido después de preparar cambios y antes del commit | Al reiniciar permanecen el estado, la revisión y la solicitud previos. No hay recibo ni resultado parcial. Una vez reincorporados ambos clientes, reenviar confirma una sola elección. |
| H09 · Caja negra | Consultar ambas presentaciones antes y después de que el rival elija. | J1 ya eligió carta y atributo. J2 todavía debe elegir | Antes de resolver, J2 solo ve la mano propia y no puede deducir la decisión de J1 a partir de la respuesta. Después ambos ven las dos cartas, el atributo y el resultado. |
| H10 · Caja negra | Intentar tomar la plaza de J1 mientras su sesión sigue activa. | Credencial de J1 válida, UUID de sesión diferente | `409 sesionOcupada`. La sesión original conserva su plaza y su mano. Al vencer, J1 puede reincorporarse sin cambiar de rol. |
| H11 · Caja negra | Abandonar de forma explícita durante una partida activa y repetir el envío. | Solicitud de abandono de J2 | Devuelve `204` en ambos envíos. La partida queda finalizada por abandono, sin sumar una victoria ni emitir más solicitudes. |
| H12 · Caja blanca | Actualizar o desactivar un personaje del catálogo usado en una partida y recuperar el estado. | En una partida iniciada, el personaje sigue en una mano. Se cambia el catálogo y se reinicia el servidor | La partida conserva la copia y puntajes originales. Una nueva partida usa los datos vigentes y excluye los personajes desactivados. |

Para la ejecución registrar el ID del caso, versión probada, entorno, resultado observado, conformidad y evidencia. Para H01 y los demás casos que se ejecuten en red, registrar también los dos equipos, dirección del servidor y puerto. Los casos con fallo inducido requieren un entorno de pruebas controlado.
