# Juego Guerra — Requerimientos y reglas del juego

## Objetivo y participantes

Participan dos jugadores, J1 y J2. Gana la partida quien alcanza primero 15 rondas ganadas. Cada jugador utiliza un cliente para elegir sus cartas y, cuando le corresponde, el atributo del combate. El servidor analiza las elecciones y comunica los resultados.

## Conceptos

| Término | Significado |
|---|---|
| Partida | Juego completo entre J1 y J2, hasta que uno gana 15 rondas. |
| Ronda | Serie de hasta tres combates; la gana quien consigue dos combates ganados. |
| Combate | Enfrentamiento de una carta de cada jugador mediante un atributo elegido. |
| Jugador mano | Quien elige carta y atributo en el primer combate de la ronda. |
| Mano de cartas | Cartas disponibles de un jugador; recibe tres al comenzar cada ronda. |
| Mazo común | Conjunto completo de personajes con el que se realizan los repartos. |

## Requerimientos funcionales

### Personajes y preparación

Carta y personaje son el mismo elemento. Cada personaje tiene nombre, representación ASCII y seis atributos: Fuerza, Velocidad, Resistencia, Inteligencia, Energía y Habilidad. Los atributos deben sumar exactamente 3000 puntos; cada uno debe estar entre 250 y 750, al menos uno debe ser de 650 o más y al menos uno de 350 o menos. No se permiten seis valores de 500.

El servidor conserva en SQLite el catálogo de personajes dados de alta manualmente con `subir-personajes`.

Se reparten tres cartas a cada jugador desde el mazo común. J1 es el jugador mano de la primera ronda. Al terminar cada ronda, ese rol pasa al otro jugador, independientemente del ganador: J1, J2, J1, J2, hasta finalizar la partida.

### Desarrollo de los combates

1. En el primer combate comienza el jugador mano. Mira sus cartas y elige qué carta jugar y qué atributo utilizar.
2. El rival elige una carta de su mano sin que la interfaz le muestre la carta ni el atributo seleccionados por el contrincante.
3. Cuando ambos han elegido, se muestran las dos cartas y el atributo del combate.
4. Gana el combate quien tenga el valor más alto en ese atributo. Si hay empate, gana quien no eligió el atributo. Se suma un combate ganado al vencedor.
5. Si la ronda continúa, quien no eligió el atributo pasa a elegir carta y atributo en el siguiente combate, independientemente del resultado anterior.

Las cartas jugadas no vuelven a utilizarse durante la misma ronda. La elección del atributo se alterna en cada combate y no cambia quién es el jugador mano de esa ronda. La API entrega a cada jugador su propia mano y reserva la carta y el atributo rivales hasta que ambos hayan elegido.

### Finalización de ronda y partida

Gana la ronda quien consigue dos combates ganados y suma una ronda a su marcador. Si gana los dos primeros combates, no se juega el tercero.

Al finalizar la ronda se restaura el mazo completo, incluidas las cartas no jugadas. No se acumulan cartas ganadas ni se pierden cartas entre rondas. Antes de la siguiente ronda se mezcla el mazo, se reparten tres cartas a cada jugador, se alterna el jugador mano y se reinician los contadores de combates ganados. Las rondas ganadas se acumulan durante toda la partida.

Cuando un jugador alcanza 15 rondas ganadas, se comunica el ganador y finaliza la partida sin iniciar otra ronda.

### Información conservada durante la ejecución

- Datos de los personajes y composición del mazo completo.
- Identificación de J1 y J2 y cartas disponibles de cada uno.
- Cartas jugadas durante la ronda.
- Ronda y combate actuales.
- Jugador mano y jugador al que corresponde elegir el atributo.
- Carta elegida por cada jugador, atributo seleccionado y si ambos han completado su elección.
- Combates ganados por cada jugador en la ronda y rondas ganadas en la partida.
- Resultados de combates y rondas y ganador de la partida cuando corresponda.
- Estado de la partida y si ha finalizado normalmente o por un error.

### Comunicación y validación

Dos clientes de consola se conectan al servidor mediante HTTP en la red local e intercambian datos JSON. Cada cliente muestra la presentación de su jugador y envía las elecciones solicitadas. El servidor valida el participante, la solicitud, la carta y el atributo, aplica las reglas y publica el resultado para ambos después de completar las elecciones.

### Situaciones de error

| Situación | Respuesta prevista |
|---|---|
| Menos de seis cartas válidas | Informar «No se puede jugar porque no hay suficientes cartas» e impedir el inicio de la partida. |
| Datos inválidos al dar de alta un personaje | Informar el rechazo sin guardar el personaje. |
| Servidor inaccesible o conexión interrumpida | Informar y ofrecer reintentar o salir. Si hay una elección pendiente, conservar su identificación para el reintento. |
| Elección inválida, solicitud consumida o estado incompatible | Rechazar sin modificar el juego. Los códigos HTTP y la actualización de la presentación se detallan en la propuesta de protocolo. |
| Cliente ausente o proceso reiniciado | Aplicar la política de presencia, pausa, abandono y recuperación del [protocolo HTTP](protocolo-partida-web.md#estados-presencia-y-recuperación). |

## Condiciones técnicas de ejecución

- Dos computadoras Windows en la red local. La primera ejecuta servidor y cliente J1 y la segunda ejecuta cliente J2.
- Se inicia el servidor antes de abrir los clientes de consola. La dirección HTTP es configurable y se acordó TCP 5000.
- El servidor administra las partidas y el catálogo en SQLite. Los clientes consultan la API.

Los contratos de intercambio y sus pendientes están en el [protocolo HTTP](protocolo-partida-web.md).
