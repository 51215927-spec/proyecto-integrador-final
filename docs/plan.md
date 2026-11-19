# Plan de continuidad

La etapa de análisis y diseño (TI) ya fue entregada. La solución cuenta con el dominio del juego, el servidor REST con persistencia en SQLite y una demostración local del cliente. El próximo objetivo es jugar una partida desde dos computadoras en la misma red. Los cambios de código corresponden a Taller de Programación y quedan sujetos a la autorización para iniciar esa etapa. [Arquitectura actual](arquitectura.md)

## Entrada interactiva

El servidor, el cliente y la herramienta de carga deberán pedir sus datos por consola durante la ejecución. El servidor solicitará la dirección de escucha, el archivo de base de datos y la credencial de catálogo. Cuando el operador elija una acción, la consola pedirá los identificadores que esa acción requiera. El cliente permitirá elegir la demostración y la herramienta de carga solicitará la URL, la credencial y los datos del personaje. Las entradas inválidas podrán corregirse antes de continuar. [Instalación y uso](instalacion-y-uso.md#entrada-de-datos-en-ejecución)

## Cliente conectado al servidor

Al elegir jugar en red, el cliente solicitará la URL del servidor, el identificador de la partida, la credencial de la plaza y el nombre del jugador. Después de incorporarse, renovará la presencia y mostrará la mano propia, el turno y el marcador según la presentación del servidor. Las elecciones se enviarán con la solicitud y la revisión vigentes. Si se pierde una respuesta, el cliente consultará el recibo de la operación antes de decidir si corresponde reenviarla. También deberá recuperar la vista tras una reconexión y mostrar el motivo de cierre de la partida. [Contrato HTTP](protocolo-de-partida.md#contrato-http)

## Verificación y entrega

Con el cliente conectado, se verificará una partida completa entre dos equipos con la misma versión. La prueba comprobará el reparto, la privacidad de las manos, la alternancia y la coincidencia del marcador hasta que un jugador alcance 15 rondas ganadas. También se comprobará la recuperación tras una desconexión o un reinicio y el comportamiento ante un reenvío o un abandono. Los resultados se registrarán junto con los casos de prueba, y la guía de uso se actualizará con el recorrido verificado. [Plan de pruebas](plan-de-pruebas.md) · [Verificación en dos equipos](protocolo-de-partida.md#verificación-en-dos-equipos)
