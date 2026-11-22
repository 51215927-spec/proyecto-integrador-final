# Plan de continuidad

La etapa de análisis y diseño (TI) ya fue entregada. La solución cuenta con el dominio del juego, el servidor REST con persistencia en SQLite y el cliente REST de consola. El próximo objetivo es jugar una partida desde dos computadoras en la misma red. La implementación del cliente se realizó como parte de Taller de Programación. [Arquitectura actual](arquitectura.md)

## Entrada interactiva

El servidor, el cliente y la herramienta de carga ya piden sus datos por consola durante la ejecución. El servidor solicita la configuración antes del host y cada identificador al elegir una acción. El cliente solicita la conexión y la identidad de la plaza. La carga recibe una ruta JSON por argumento o pregunta, solicita URL y credencial, muestra cantidades y requiere confirmación para reemplazar todo el catálogo. Un error de archivo o carga finaliza sin reintento automático. La evidencia y el estado de revisión están en [la evidencia del spec 003](plan-de-pruebas.md#evidencia-del-spec-003). [Instalación y uso](instalacion-y-uso.md#entrada-de-datos-en-ejecución)

## Cliente conectado al servidor

El cliente ofrece juego en red y solicita URL, UUID de partida, credencial y nombre. Renueva presencia, muestra la presentación del servidor, envía elecciones con operación estable y recupera recibos ante respuestas inciertas. La reconexión es automática; Esc permite salir sin abandonar o confirmar abandono. Los resultados se leen con Enter en pantallas separadas. La implementación y las comprobaciones locales se describen en [las pruebas del cliente REST](plan-de-pruebas.md#cliente-rest-spec-001). La verificación entre dos equipos permanece pendiente. [Contrato HTTP](protocolo-de-partida.md#contrato-http)

## Separación de responsabilidades

El spec 006 convirtió `Partida` en el dominio por acciones utilizado por REST y separó coordinación, almacenamiento, sesiones, catálogo y vista. Conserva contrato público, reglas y esquema SQLite. La implementación está en `refactor/006-separacion-responsabilidades`, referencia `f0f292b`; la actualización de guías se prepara separadamente en `docs/006-separacion-responsabilidades`. [Responsabilidades actuales](arquitectura.md#responsabilidades-del-servidor) · [190 pruebas locales aprobadas](plan-de-pruebas.md#separación-de-responsabilidades-spec-006).

La compatibilidad con una base anterior y los contratos locales están comprobados. La integración física del spec 002 entre dos Windows permanece pendiente; esta refactorización no la acredita. Los specs canónicos se mantienen solo en la copia principal de `main`, fuera del control de versiones.

## Verificación y entrega

Con el cliente conectado, se verificará una partida completa entre dos equipos con la misma versión. La prueba comprobará el reparto, la privacidad de las manos, la alternancia y la coincidencia del marcador hasta que un jugador alcance 15 rondas ganadas. También se comprobará la recuperación tras una desconexión o un reinicio y el comportamiento ante un reenvío o un abandono. Los resultados se registrarán junto con los casos de prueba, y la guía de uso se actualizará con el recorrido verificado. [Plan de pruebas](plan-de-pruebas.md) · [Verificación en dos equipos](protocolo-de-partida.md#verificación-en-dos-equipos)
