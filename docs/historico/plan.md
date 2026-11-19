# Plan de trabajo — Juego Guerra

Plan basado en la [consigna](../wiki/Consigna.md). El usuario confirmó la entrega de TI. Las casillas registran los entregables de diseño y el estado del trabajo de Taller.

| Actividades de TI | Estado |
|---|---|
| Personajes, requisitos, modelo, métodos y archivos | Actividades 1 a 5 completadas en TI. |
| Comunicación HTTP | Diseño entregado en TI. Verificación entre computadoras pendiente en Taller. |
| Casos de prueba | Escenarios y resultados esperados entregados en TI. El estado de ejecución se registra en Taller. |

## TI — Análisis de Sistemas

### Actividad 1 — Análisis y diseño de los personajes

- [x] Definir nombres, descripciones y distribuciones de Lysander y Reched.
- [x] Validar los seis atributos, su total y los umbrales de especialización.
- [x] Identificar los atributos principal y de menor valor.

#### Fichas de personajes

Valores en orden: Fuerza, Velocidad, Resistencia, Inteligencia, Energía y Habilidad.

|  | Valores | Total | Principal / menor | Resultado |
|---|---|---|---|---|
| Lysander | 750, 550, 500, 250, 500, 550 | 3000 | Fuerza / Inteligencia | Cumple rango 250–750 y umbrales 650 y 350. |
| Reched | 250, 700, 350, 750, 450, 500 | 3000 | Inteligencia / Fuerza | Cumple rango 250–750 y umbrales 650 y 350. |

### Actividad 2 — Relevamiento de requerimientos

- [x] Documentar participantes, reglas de combate, ronda y partida en [Reglas del juego](../wiki/Reglas-del-juego.md).
- [x] Definir catálogo, mazo, reparto, duplicados y mínimo de cartas.
- [x] Especificar datos de ejecución, validaciones, errores y condiciones técnicas.
- [x] Preparar el texto para que el usuario lo copie al documento de entrega.

### Actividad 3 — Modelo de clases

- [x] Definir responsabilidades, encapsulamiento, dependencias y relaciones del dominio en [Arquitectura](wiki/Arquitectura.md) y el [diagrama UML incluido en los descargables](wiki/README.md).
- [x] Modelar la carga del catálogo, la coordinación de la partida y la presentación de consola.

La integración HTTP se documenta en la actividad 6.

### Actividad 4 — Especificación de métodos

- [x] Especificar las firmas, precondiciones, efectos, retornos, validaciones y errores de cada operación en [Arquitectura](wiki/Arquitectura.md#especificación-de-clases-y-métodos--actividad-4).

Para este proyecto se acordó documentar cada método por escrito, sin diagramas Nassi–Schneiderman.

### Actividad 5 — Datos y validación de personajes

- [x] Definir los [datos de los personajes](wiki/Arquitectura.md#datos-y-validación-de-personajes--actividad-5) y sus validaciones.
- [x] Incorporar ID, descripción y ASCII al catálogo y a los contratos de Personaje.

La decisión vigente es cargar los personajes manualmente mediante `subir-personajes`. Esta decisión sustituye la lectura de archivos de personajes prevista en la consigna original. El catálogo conserva el arte ASCII y la descripción. El juego prescinde de ambos durante la partida.

### Actividad 6 — Comunicación HTTP en red local

**Diseño entregado en TI.** El 17 de noviembre de 2026 el usuario informó que se aprobó HTTP para la entrega en lugar del archivo compartido indicado en la consigna original. Se adopta el diseño de [API REST](wiki/Protocolo-de-partida.md), con dos clientes de consola y un servidor que aplica las reglas y conserva los datos.

- [x] Documentar tecnologías, despliegue, sesiones y reincorporación en el [protocolo](wiki/Protocolo-de-partida.md).
- [x] Especificar rutas, DTO, errores, estados, presencia, pausa y abandono.
- [x] Diseñar el control de revisión de estado, la idempotencia y la persistencia transaccional.
- [x] Documentar el modelo de integración y vincular los casos de prueba HTTP.

### Actividad 7 — Casos de prueba

- [x] Documentar condiciones iniciales, datos, acción y resultados esperados de caja negra y blanca en el [plan de pruebas](../wiki/Plan-de-pruebas.md).
- [x] Cubrir personajes, formato del catálogo, mazo, reglas de combate y finalización.
- [x] Cubrir indisponibilidad, turno incorrecto, pausa, recuperación y concurrencia según el contrato HTTP de la actividad 6.

La ejecución y la evidencia de resultados se registran en Taller de Programación.

## Taller de Programación — C#

TI ya fue entregado. Para completar Taller, primero hay que cerrar las decisiones de consola e integrar el cliente REST. Después se puede validar el juego completo en red local y preparar la entrega de Taller. Las casillas marcadas registran avances comprobados en el repositorio.

### Etapa 1 — Implementación de Personaje

- [ ] Acordar con los Analistas si la descripción queda exclusivamente en el catálogo o si se mantiene `GetDescripcion` en el contrato de `Personaje`. Actualizar contrato, código y pruebas según lo acordado. El arte ASCII y la descripción se conservan sin mostrarse durante el juego.
- [x] Implementar el constructor con nombre, seis atributos y ASCII Art.
- [x] Implementar las validaciones de los atributos según los contratos de TI.
- [x] Implementar GetNombre, GetAscii y GetAtributo conservando el encapsulamiento.
- [x] Agregar en `test/` pruebas de personajes válidos y de cada restricción de puntajes.

### Etapa 2 — Carga manual de personajes

- [x] Definir el [contrato de alta individual](wiki/Protocolo-de-partida.md#alta-individual-de-personajes).
- [x] Pedir los datos en `subir-personajes`, enviarlos por HTTP y permitir otra alta tras confirmar el ID.
- [x] Verificar dos altas consecutivas y el rechazo con una credencial incorrecta.


### Etapa 3 — Jugador e interacción de consola

El [diseño de la interfaz](../wiki/Diseno-de-interfaz.md) registra las pantallas, los controles implementados en el cliente y las decisiones pendientes del flujo cliente.

- [x] Crear Jugador con nombre, mano de personajes y marcador de rondas ganadas.
- [x] Implementar recepción, consulta, retiro y entrega de personajes sin exponer la colección interna.
- [x] Implementar la composición y los menús propios de InterfazConsola según el [diseño de la interfaz](../wiki/Diseno-de-interfaz.md), con renderizado mediante Spectre.Console.
- [x] Validar la carta elegida contra las opciones de mano recibidas y el atributo contra las opciones de la solicitud.
- [x] Presentar mano, marcador, esperas, resultados y errores según los contratos de TI.
- [ ] Definir cómo descartar las entradas sobrantes entre solicitudes y cómo tratar una elección en curso cuando cambia la solicitud.
- [ ] Definir cómo presentar la incorporación, el rol asignado, la recuperación y la confirmación de salida. Cerrar si el diseño visual incluye la consola del operador.
- [ ] Verificar con ambos clientes que cada jugador vea su propia mano y que la carta y el atributo del rival solo aparezcan cuando se resuelva el combate. Comprobar que el arte y la descripción queden fuera de las pantallas de juego.

### Etapa 4 — Administración del mazo

- [x] Crear Mazo con su colección privada de Personaje.
- [x] Implementar Mezclar, Repartir, Devolver y GetCantidad conforme a las firmas del UML.
- [x] Comprobar que el reparto retire los elementos entregados y respete su disponibilidad.
- [ ] Verificar que, al cerrar una ronda, vuelvan al mazo todas las cartas jugadas y sin jugar, sin pérdidas ni duplicados.
- [ ] Verificar que el flujo real rechace iniciar con menos de seis personajes válidos y que la consola del operador muestre el mensaje acordado.
- [ ] Contrastar la cobertura de mazo vacío, reparto insuficiente, mezcla y recuperación completa con el plan de pruebas. Completar los casos que falten.

### Etapa 5 — Implementación de las reglas

- [x] Implementar `Ronda` y `Combate` y probar sus reglas en el dominio.
- [ ] Revisar el contrato de `Partida` frente al flujo REST. El servidor ejecuta la partida mediante sus transiciones y `Partida.Jugar()` aún carece del paso de selección de cartas y atributo.
- [ ] Iniciar con J1 como jugador mano y repartir tres personajes a cada jugador.
- [ ] Registrar carta y atributo del elector y después la carta del rival según la secuencia definida.
- [x] Resolver cada combate por el atributo elegido; ante empate gana quien no eligió el atributo.
- [x] Retirar de la mano los personajes jugados para impedir reutilizarlos durante la ronda.
- [x] Alternar el elector de atributo entre combates, independientemente del ganador.
- [ ] Cerrar la ronda al alcanzar dos combates ganados y sumar una ronda al marcador del vencedor.
- [ ] Recuperar todos los personajes y alternar el jugador mano al terminar cada ronda.
- [x] Verificar que el servidor finalice al alcanzar 15 rondas ganadas y omita el reparto de otra ronda.
- [x] Ejecutar las pruebas automatizadas de combates, empates, alternancia, cierre anticipado de ronda y fin de partida.

### Etapa 6 — Comunicación HTTP entre computadoras

#### Servidor REST

- [x] Implementar y probar la API REST del servidor según el contrato de partidas.
- [x] Conectar la API con el dominio y guardar las transiciones en SQLite conforme al contrato de persistencia.
- [x] Implementar vistas de jugador, validación autoritativa, concurrencia y reintentos idempotentes.
- [x] Aplicar la política acordada de presencia, abandono y recuperación en el servidor.

#### Cliente REST

El cliente de consola dispone de una demostración local. Para jugar una partida real falta aplicar el [protocolo de partida](wiki/Protocolo-de-partida.md).

- [ ] Leer la URL del servidor desde la configuración local y permitir la incorporación con el ID de partida, la credencial de plaza y el nombre del jugador. Mostrar el rol asignado y esperar el inicio del operador.
- [ ] Mantener la sesión activa y consultar la presentación según los intervalos del protocolo, incluso mientras el jugador elige o la partida está pausada.
- [ ] Mostrar mano, turno, marcador y resultados desde la presentación del servidor. Aplicar las actualizaciones de pausa y reanudación aunque se conserve la revisión y mostrar cada resultado resuelto una sola vez.
- [ ] Enviar carta y atributo, o solo carta, según lo que pida la solicitud vigente. Antes del envío, comprobar que la solicitud y la revisión sigan vigentes.
- [ ] Si el servidor rechaza una elección, distinguir entre datos inválidos, solicitud vencida, revisión desactualizada y partida pausada. Actualizar la vista o volver a pedir la entrada según corresponda.
- [ ] Cuando la respuesta de una jugada se pierda o sea incierta, consultar el recibo y, si corresponde, reintentar con el mismo identificador y los datos originales para evitar una jugada duplicada.
- [ ] Gestionar servidor inaccesible, sesión vencida u ocupada, pausa por ausencia y reincorporación tras una desconexión o reinicio. Recuperar la vista sin perder el turno o la operación pendiente que siga vigente.
- [ ] Distinguir cierre del cliente, abandono confirmado, cancelación del operador y victoria. Mostrar el motivo de cierre indicado por el servidor.
- [ ] Agregar pruebas automatizadas del flujo cliente, incluidos incorporación, cambios de turno, jugada confirmada, solicitud desactualizada, reintento, pausa y recuperación.

### Etapa 7 — Integración

#### Prueba de punta a punta en red local

- [ ] Preparar ambos equipos con la misma versión del proyecto y .NET 8. Arrancar el servidor, registrar su IPv4 y puerto TCP 5000, y comprobar el acceso desde el segundo equipo.
- [ ] Cargar al menos seis personajes válidos de los alumnos, crear la partida desde la consola del servidor e incorporar a J1 y J2 desde equipos distintos con sus credenciales.
- [ ] Jugar hasta las 15 rondas ganadas. Comprobar el reparto de tres cartas por jugador, las manos privadas, la secuencia de los combates, la alternancia y la coincidencia del marcador y del resultado final en ambas consolas.
- [ ] Ejecutar los escenarios de red y consistencia HTTP aplicables del [plan de pruebas](../wiki/Plan-de-pruebas.md#red-local-y-consistencia-http), incluidos pérdida de respuesta, desconexión, recuperación, reinicio del servidor y abandono. Ejecutar los casos con fallos inducidos en un entorno de pruebas.
- [ ] Registrar la versión, los equipos, la IP y el puerto del servidor, el resultado observado por caso y la evidencia. Resolver los fallos y repetir los casos afectados.

#### Entrega de Taller

- [ ] Contrastar las firmas y responsabilidades implementadas con los contratos de TI, en particular `Personaje` y `Partida`. Resolver las diferencias con los Analistas y actualizar los documentos afectados.
- [x] Verificar en `main` (`eac766b`) que la solución compila sin advertencias ni errores y que las 85 pruebas automatizadas pasan al habilitar las pruebas de contrato.
- [ ] Repetir `dotnet build proyecto-integrador-final.sln` y `dotnet test proyecto-integrador-final.sln` con las pruebas de contrato habilitadas después de integrar el cliente REST.
- [ ] Actualizar la guía de instalación y uso con los pasos verificados para iniciar el servidor, cargar el catálogo, crear la partida y conectar ambos clientes.
- [x] Registrar la entrega de TI confirmada por el usuario.
- [ ] Verificar los entregables de Taller, la disponibilidad del código integrado en `src/`, las pruebas en `test/` y la documentación en la [wiki](wiki/README.md).
- [ ] Preparar la defensa de Taller con las decisiones de diseño, las estrategias de personajes, las clases y métodos de cada integrante y la evidencia de la partida en red.

Los Analistas coordinan los entregables y comunican los cambios a Taller. El flujo de GitHub está en la [guía de colaboración](../../CONTRIBUTING.md).
