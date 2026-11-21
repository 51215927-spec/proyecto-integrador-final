# Plan de continuidad

La etapa de análisis y diseño (TI) ya fue entregada. La solución cuenta con el dominio del juego, el servidor REST con persistencia en SQLite y el cliente REST de consola. El próximo objetivo es jugar una partida desde dos computadoras en la misma red. La implementación del cliente se realizó como parte de Taller de Programación. [Arquitectura actual](arquitectura.md)

## Entrada interactiva

El servidor, el cliente y la herramienta de carga ya piden sus datos por consola durante la ejecución. El servidor solicita la configuración antes del host y cada identificador al elegir una acción. El cliente solicita la conexión y la identidad de la plaza. La carga solicita URL, credencial y personaje, con corrección de datos inválidos. La evidencia y el estado de revisión están en [la evidencia del spec 003](plan-de-pruebas.md#evidencia-del-spec-003). [Instalación y uso](instalacion-y-uso.md#entrada-de-datos-en-ejecución)

## Cliente conectado al servidor

El cliente ofrece juego en red y solicita URL, UUID de partida, credencial y nombre. Renueva presencia, muestra la presentación del servidor, envía elecciones con operación estable y recupera recibos ante respuestas inciertas. La reconexión es automática; Esc permite salir sin abandonar o confirmar abandono. Los resultados se leen con Enter en pantallas separadas. La implementación y las comprobaciones locales se describen en [las pruebas del cliente REST](plan-de-pruebas.md#cliente-rest-spec-001). La verificación entre dos equipos permanece pendiente. [Contrato HTTP](protocolo-de-partida.md#contrato-http)

## Verificación y entrega

Con el cliente conectado, se verificará una partida completa entre dos equipos con la misma versión. La prueba comprobará el reparto, la privacidad de las manos, la alternancia y la coincidencia del marcador hasta que un jugador alcance 15 rondas ganadas. También se comprobará la recuperación tras una desconexión o un reinicio y el comportamiento ante un reenvío o un abandono. Los resultados se registrarán junto con los casos de prueba, y la guía de uso se actualizará con el recorrido verificado. [Plan de pruebas](plan-de-pruebas.md) · [Verificación en dos equipos](protocolo-de-partida.md#verificación-en-dos-equipos)
