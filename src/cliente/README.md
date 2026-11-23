# Cliente

Desde la raíz ejecutá `dotnet run --project src/cliente`. El menú ofrece **Jugar**, **Crear perfil**, **Conexión** y **Salir**. Escribí la opción y Enter. Usa `http://localhost:5000/` por defecto; Conexión permite cambiar y recordar la dirección. En otra computadora usá la dirección del servidor.

La primera vez que elegís Jugar se pide un nombre opcional (Enter: Invitado). Cada perfil tiene su propio acceso y partidas. Podés crear varios desde el mismo usuario del sistema con Crear perfil. El cliente conserva automáticamente esos accesos por servidor en `GuerraCartas`, dentro de los datos locales del usuario (`%LOCALAPPDATA%` en Windows y el directorio de datos del usuario en Unix). La carpeta y los archivos se restringen al usuario. No compartas esos archivos: contienen el acceso para retomar. Cambiar de equipo o borrar el acceso no permite recuperar una partida por nombre.

Al elegir Jugar, el cliente consulta cuáles de tus perfiles están libres. Si hay uno solo libre, entra automáticamente; si hay varios, muestra sus nombres numerados para elegir. Los ocupados no se ofrecen para entrar. Si todos están en uso, podés crear otro perfil, reintentar o volver. La consulta no ocupa perfiles ni extiende sesiones ajenas. Si otro cliente toma el perfil durante la selección, se actualiza la disponibilidad. El acceso único guardado por la versión anterior se conserva automáticamente como primer perfil.

Jugar muestra **Buscando oponente**; cuando llega otro invitado aparece la sala. Escribí **Listo** en ambos clientes. Con seis personajes activos se inicia una sola partida, sin comandos del operador. Si falta catálogo, ambos ven el bloqueo; podés escribir Listo para reintentar después de cargarlo o Cancelar búsqueda. Si sale el rival antes del inicio volvés a buscar sin conservar Listo.

Durante el juego usá flechas, WASD o HJKL y Enter para elegir carta y atributo. Cada resultado se lee por separado. Esc abre Continuar, Salir sin abandonar y Abandonar partida. Abandonar requiere confirmar Sí; No es la opción inicial. Ctrl+C y fin de entrada salen sin abandonar. La salida normal libera la sesión; después de un corte puede seguir ocupada hasta quince segundos.

Los cambios llegan por avisos autenticados; no se consulta periódicamente la presentación. Al cortar la conexión aparece Reconectando… y se reintenta con esperas de 1, 2 y 5 segundos. Una elección con respuesta perdida se comprueba por su recibo antes de reenviarse. Al reabrir, elegí Jugar y **Retomar** para continuar tu partida activa. No se inicia otra búsqueda al terminar una partida.

Un error de archivo local ofrece Reintentar o Salir sin reemplazar la identidad. Si el servidor rechaza definitivamente el acceso, escribir NUEVO confirma crear otro invitado; esa acción no recupera partidas del anterior. Cada perfil admite una sesión simultánea; distintas terminales del mismo usuario pueden jugar usando perfiles distintos.


Con entrada redirigida se envía una acción por línea; en el juego, línea vacía es Enter y `esc` abre el menú. EOF cierra sin abandonar.

[Instalación y uso](../../docs/instalacion-y-uso.md) · [Protocolo](../../docs/protocolo-de-partida.md) · [Pruebas](../../docs/plan-de-pruebas.md)
