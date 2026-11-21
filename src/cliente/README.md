# Cliente REST

Conecta una plaza de una partida al servidor y muestra cartas, turnos y resultados con Spectre.Console. Desde la raíz ejecutá `dotnet run --project src/cliente`.

Ingresá la URL base HTTP/HTTPS, el UUID de la partida, la credencial de tu plaza y tu nombre. Al retomar, el nombre debe coincidir exactamente. La credencial se oculta en terminal interactiva. El operador crea e inicia la partida desde el servidor.

Usá flechas, WASD o HJKL y Enter para elegir carta y, si corresponde, atributo. Cada resultado se lee en una pantalla separada: Enter presenta el siguiente resultado o el estado vigente. Durante la lectura continúan las consultas y la presencia. Las cartas muestran nombre y atributos.

Esc abre Continuar, Salir sin abandonar y Abandonar partida. Abandonar requiere confirmar Sí; No es la opción inicial. Ctrl+C y fin de entrada salen localmente sin abandonar. La plaza puede seguir ocupada hasta quince segundos desde su última renovación o hasta que el operador cierre la sesión.

La reconexión es automática. Una elección con respuesta perdida se comprueba por su recibo antes de reenviar el mismo cuerpo. Al cerrar no se guardan credenciales ni operaciones: para retomar, ingresá nuevamente los datos. El historial previo no se reproduce.

Con entrada redirigida, enviá una acción por línea: letras de movimiento, línea vacía para Enter y `esc` para el menú. EOF cierra sin abandonar. La salida redirigida conserva las pantallas por líneas.

[Arquitectura](../../docs/arquitectura.md) · [Instalación y uso](../../docs/instalacion-y-uso.md) · [Pruebas](../../docs/plan-de-pruebas.md)
