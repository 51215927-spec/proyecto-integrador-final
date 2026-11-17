# Plan de trabajo — Juego Guerra

Plan basado en [la consigna](Consigna.md). Marcar una tarea cuando su resultado esté disponible y revisado. Las actividades de TI definen los contratos que se usarán en Taller; las decisiones pendientes deben resolverse antes de implementar el componente correspondiente. Por decisión del usuario, no se utilizarán diagramas Nassi-Schneiderman (NS); el comportamiento de los métodos se documentará por escrito con sus entradas, resultados, precondiciones y validaciones. La consigna original se conserva como referencia sin modificar.

## Estado revisado — 17 de noviembre de 2026

Alcance actual: TI (análisis y diseño). Taller queda pendiente. El punto de entrada contiene `Hello, World!` y la única prueba está vacía. `main` incluye Personaje, Mazo, Jugador, Combate, Ronda y Partida. En `d6d81c1`, la compilación y las pruebas quedaron bloqueadas por dos llamadas a métodos ausentes de Mazo en Partida. La comunicación HTTP sigue en diseño.

Se revisaron el documento de avance aportado por el usuario y el [UML existente](diagramas/UML.uxf). El documento se utilizó como referencia local y no forma parte de la entrega del repositorio. Cada casilla de TI refleja esa revisión y las confirmaciones posteriores del usuario: `[x]` indica un resultado comprobado o confirmado, `[ ]` indica trabajo pendiente y **En curso** señala un avance parcial con el cierre que falta. Los valores verificados de personajes corresponden a Lysander y Reched. El usuario confirmó que la actividad 1 está completada; se retiran los pendientes de alternativas, comparación, justificación y verificación de otras fichas como condiciones para reabrirla. La imagen se trabaja en Arte. Las demás actividades conservan su estado de revisión. Completar una tarea de diseño no implica implementar código.

| Actividad de TI | Estado y evidencia | Qué falta para cerrarla |
|---|---|---|
| 1. Personajes | Completada por confirmación del usuario. Lysander y Reched tienen nombre, descripción, seis valores, total y atributos principal y menor; ambos cumplen las restricciones. | Sin pendientes para esta actividad. |
| 2. Requerimientos | Documento completo y revisado en [Reglas del juego](Reglas-del-juego.md), listo para copiar a la entrega. Incluye reglas, participantes, datos de ejecución, comunicación, errores y separación de requisitos funcionales y técnicos. | Sin tareas de redacción pendientes. El [PR #7](https://github.com/51215927-spec/Proyecto-Integrador-Final/pull/7) está integrado; el protocolo detallado corresponde a la actividad 6. |
| 3. Modelo de clases | Completada por confirmación del usuario. UML y Arquitectura revisados, con firmas, relaciones, responsabilidades y justificación. | Sin pendientes propios. El modelado de comunicación y su integración se realizan en la actividad 6. |
| 4. Métodos | Completada por confirmación del usuario. Las 58 especificaciones están redactadas en Arquitectura, con un párrafo por método y sin validaciones repetidas. | Sin pendientes propios. Los formatos y la integración corresponden a las actividades 5 y 6. |
| 5. Archivos de personajes | Completada por confirmación del usuario. Formato de DataBases, columnas, descripción, ASCII y errores documentados en Arquitectura; UML y contratos de Personaje actualizados. | Sin condiciones pendientes para el cierre aceptado. Conversión al CSV postergada; ejemplos propios no elaborados. |
| 6. Comunicación HTTP | HTTP aprobado para la entrega. Diseño [REST](Protocolo-partida-web.md) en curso. | Completar contratos, UML, casos y verificaciones en red. Revisión de compañero pendiente. |
| 7. Pruebas | No se encontró un plan de casos desarrollado en el material revisado. | Diseñar condiciones iniciales, datos y resultados esperados para casos normales, límites y errores. |

### Verificación de personajes

Valores en orden: Fuerza, Velocidad, Resistencia, Inteligencia, Energía y Habilidad.

|  | Valores | Total | Principal / menor | Resultado |
|---|---|---|---|---|
| Lysander | 750, 550, 500, 250, 500, 550 | 3000 | Fuerza / Inteligencia | Cumple rango 250–750 y umbrales 650 y 350. |
| Reched | 250, 700, 350, 750, 450, 500 | 3000 | Inteligencia / Fuerza | Cumple rango 250–750 y umbrales 650 y 350. |

### Reglas ya redactadas

- Participan dos jugadores y se reparten tres cartas a cada uno desde un mazo común.
- El jugador mano elige el primer atributo; ambos jugadores eligen su carta y la muestran simultáneamente. Gana el mayor valor y, ante empate, quien no eligió el atributo del combate.
- La elección del atributo se alterna entre los jugadores en cada combate, independientemente del ganador. La ronda se gana con dos combates ganados, por lo que puede terminar sin jugar la tercera carta.
- Al terminar la ronda se restaura el mazo completo, incluidas las cartas no jugadas. Si nadie alcanzó 15 rondas ganadas, se mezcla y se reparte nuevamente; de lo contrario finaliza la partida.

### Decisión confirmada — jugador mano y terminología

El usuario confirmó que al iniciar la partida se identifican J1 y J2. J1 es el jugador mano de la primera ronda. Al terminar cada ronda, el rol pasa al otro jugador: J1, J2, J1, J2, hasta finalizar la partida, independientemente de quién gane la ronda. Dentro de una ronda, la elección del atributo se alterna entre los jugadores en cada combate, independientemente del ganador; esto no cambia quién es el jugador mano de esa ronda.

El documento de entrega debe distinguir estos conceptos:

- **Partida:** juego completo; termina cuando un jugador alcanza 15 rondas ganadas.
- **Ronda:** serie de hasta tres combates; termina cuando un jugador gana dos combates. Equivale a lo que coloquialmente se llama «mano» en el truco.
- **Combate:** enfrentamiento de una carta de cada jugador mediante un atributo elegido.
- **Jugador mano:** jugador que elige el atributo del primer combate de la ronda.
- **Mano de cartas:** cartas disponibles de un jugador; no equivale a una ronda ni al rol de jugador mano.

Esta decisión y las reglas confirmadas están redactadas en [Reglas del juego](Reglas-del-juego.md). El usuario indicó que copiará este archivo a su documento de entrega; se completó y revisó el contenido necesario para la actividad 2, sin dar por realizada esa copia externa.

### Precisiones pendientes antes de cerrar el diseño

- Contratos de dominio revisados frente a las reglas; falta completar su integración con el protocolo de la actividad 6.
- Restauración completa del mazo especificada en UML y Arquitectura, incluidas las cartas no jugadas y la devolución única al cerrar la ronda.
- El servidor forma el mazo a partir del catálogo permanente previsto en SQLite. El formato CSV de origen está definido en la actividad 5 y su importación queda pendiente. Se requieren al menos seis cartas válidas y el catálogo se prepara sin duplicados.
- Especificar cómo se mantienen ocultas carta y atributo hasta registrar ambas elecciones; el orden de elección ya está definido en Reglas-del-juego.md.
- Las especificaciones de la actividad 4 se redactan directamente desde el UML y Arquitectura. Se corrige la referencia anterior a un borrador: el usuario confirmó que no existe.
- Completar los contratos HTTP de partida, turno, ronda y combate, y definir el tratamiento de reintentos y elecciones fuera de turno.

### Orden de trabajo para TI

1. Actividades 1 y 2 completadas en su documentación e integradas mediante los PR #6 y #7. La actividad 2 está lista para copiar a la entrega; esa copia externa no se ha comprobado.
2. Actividad 3 completada: UML y justificación revisados. La ampliación del modelo para comunicación corresponde a la actividad 6.
3. Actividad 4 completada: especificaciones de métodos revisadas y aceptadas por el usuario.
4. Actividad 5 completada por confirmación del usuario. Continuar con el protocolo HTTP de partida (actividad 6).
5. Completar el plan de pruebas y revisar que todos los entregables de TI sean coherentes (actividad 7).

Las actividades 1, 2, 3 y 4 tienen sus entregables documentados; el usuario confirmó el cierre de las actividades 3 y 4. El modelado e integración de comunicación se completarán en la actividad 6. Los demás entregables de TI no se consideran cerrados solo por contar con documentación parcial.

## Aprobación de HTTP — 17 de noviembre de 2026

El [protocolo HTTP](Protocolo-partida-web.md) documenta el intercambio de elecciones y resultados de la actividad 6. El formato CSV de personajes corresponde a la actividad 5.

## TI — Análisis de Sistemas

### Actividad 1 — Análisis y diseño de los personajes

**Completada por confirmación del usuario.** Se conserva el registro de los datos y las validaciones revisadas, sin tareas adicionales pendientes para esta actividad.

- [x] Escribir el nombre y la descripción de Lysander y Reched (fichas del documento revisado).
- [x] Elegir una distribución para Lysander y Reched (valores registrados en la tabla de verificación).
- [x] Comprobar que los seis valores de ambos s estén entre 250 y 750 y sumen exactamente 3000.
- [x] Comprobar en ambos s que al menos un atributo sea mayor o igual a 650 y otro menor o igual a 350; ninguno tiene seis valores de 500.
- [x] Identificar el atributo principal y el de menor valor de ambos s.
- [x] Dar por completada la actividad 1 según la confirmación del usuario.

### Actividad 2 — Relevamiento de requerimientos

- [x] Escribir el objetivo del juego: alcanzar 15 rondas ganadas.
- [x] Identificar a los dos participantes y sus acciones: elegir atributo cuando corresponde y elegir una carta de su mano.
- [x] Definir el reparto de tres cartas a cada jugador desde un mazo común y la mezcla entre rondas.
- [x] Definir el origen del mazo inicial: catálogo administrado por el servidor, con formato CSV inicial definido en la actividad 5 e importación por especificar.
- [x] Registrar que carta y personaje son el mismo elemento y que no se utilizará una clase Carta adicional.
- [x] Definir que menos de seis cartas válidas impiden iniciar la partida y muestran el error «No se puede jugar porque no hay suficientes cartas».
- [x] Prohibir personajes repetidos en el mazo.
- [x] Registrar que el catálogo inicial se prepara sin duplicados.
- [x] Definir quién es jugador mano al iniciar la partida: J1, según confirmación del usuario.
- [x] Reunir las reglas confirmadas en [Reglas del juego](Reglas-del-juego.md), diferenciando partida, ronda, combate, jugador mano y mano de cartas y señalando las decisiones pendientes.
- [x] Preparar Reglas-del-juego.md como contenido de entrega que copiará el usuario, con J1 como jugador mano inicial y alternancia J1/J2 al terminar cada ronda.
- [x] Diferenciar en todo el documento partida, ronda, combate, jugador mano y mano de cartas; usar «ronda ganada» y aclarar que se juegan hasta tres combates.
- [x] Definir quién elige el atributo: el jugador mano en el primer combate; después se alterna entre jugadores, independientemente del ganador. Reemplaza la regla anterior basada en el ganador.
- [x] Especificar la comparación por el atributo elegido y la victoria de ronda al alcanzar dos combates ganados.
- [x] Resolver la regla de empate: gana quien no eligió el atributo.
- [x] Definir que las cartas jugadas vuelven al mazo común al terminar la ronda.
- [x] Definir la restauración del mazo completo al terminar cada ronda, incluidas las cartas no jugadas; se mezcla antes del nuevo reparto.
- [x] Definir la alternancia del jugador mano entre rondas y del jugador que elige el atributo entre combates.
- [x] Definir la secuencia: quien comienza el combate elige carta y atributo; el rival elige sin conocer esas elecciones; luego se revelan ambas cartas y el atributo.
- [x] Reservar la carta y el atributo elegidos hasta que ambos jugadores hayan respondido.
- [x] Establecer la victoria y finalización normal de la partida al alcanzar 15 rondas ganadas.
- [x] Completar los datos de ejecución: personajes y mazo, cartas, jugadores, rondas, combates, elecciones, marcadores y resultados.
- [x] Definir la comunicación general: los clientes envían elecciones por HTTP y el servidor valida, resuelve y entrega las presentaciones.
- [x] Asignar al cliente la validación de entrada y al servidor la validación de cada elección contra el estado vigente.
- [x] Registrar que el manejo de errores HTTP, presencia y recuperación se especifica en la actividad 6.
- [x] Impedir que se inicie el juego con menos de seis cartas válidas y mostrar el mensaje acordado.
- [x] Separar los requerimientos funcionales de las condiciones técnicas de ejecución en Reglas-del-juego.md.
- [x] Revisar la cobertura de la consigna y preparar el documento de requerimientos y reglas, listo para que el usuario lo copie a su entrega. No se afirma que ya haya realizado la copia ni presentado la entrega.

El [PR #7](https://github.com/51215927-spec/Proyecto-Integrador-Final/pull/7) de esta actividad se integró el 8 de noviembre de 2026. Las tareas documentales están completas; no se da por realizada la copia externa al documento de entrega. Los nuevos PR mantienen la revisión de un compañero prevista en [CONTRIBUTING](../CONTRIBUTING.md).

### Actividad 3 — Modelo de clases

**Completada por confirmación del usuario.** El UML y su justificación están revisados. No hay implementación. El detalle vigente está en [Arquitectura](Arquitectura.md). Las tareas de modelado e integración de comunicación se trasladan a la actividad 6.

- [x] Representar Personaje, Mazo, Jugador, Partida, Ronda y Combate con atributos privados, firmas tipadas y relaciones.
- [x] Adaptar todas las referencias al concepto unificado de Personaje.
- [x] Asignar coordinación del dominio al servidor y presentación a los clientes, sin comunicación directa entre rivales.
- [x] Incorporar Datos estática y las dependencias de DataBases sin duplicar la lectura CSV.
- [x] Definir en Datos constantes de rutas y Configurar(TipoProceso), con identidad fija de servidor, cliente J1 o cliente J2 antes de Iniciar.
- [x] Describir InterfazConsola delegando entradas, menús y presentación en Utilidades.
- [x] Justificar responsabilidades, encapsulamiento, delegación y decisiones en Arquitectura.
- [x] Reflejar J1 como mano inicial, alternancia por ronda y combate, empate, dos victorias por ronda y 15 rondas por partida.
- [x] Conservar el jugador mano solo en Ronda; Partida calcula el rol al construir cada ronda según su número.
- [x] Revisar consultas necesarias para la presentación, multiplicidades de inicialización y contratos que evitan registrar combates repetidos o ajenos.
- [x] Documentar recuperación de todas las cartas y ocultación en interfaz hasta completar ambas elecciones.
- [x] Unificar las firmas de referencia en UML y Arquitectura, base de las especificaciones de la actividad 4.
- [x] Revisar la notación frente a UML 2.5.1, exportar con UMLet y comprobar que las 16 relaciones conecten a bordes sin atravesar cajas.
- [x] Dar por terminada la actividad 3 según confirmación del usuario; trasladar el modelado de comunicación a la actividad 6.

### Actividad 4 — Especificación de métodos

Completada por confirmación del usuario el 9 de noviembre de 2026, después de revisar y simplificar las especificaciones de [Arquitectura](Arquitectura.md). El código incluido es ilustrativo y no constituye implementación. No se utilizan diagramas NS, según la decisión registrada.

- [x] Identificar las clases y métodos vigentes del UML, incluidas las operaciones externas representadas.
- [x] Organizar Arquitectura con una sección por clase y una subsección por método o constructor.
- [x] Explicar responsabilidad y motivo de existencia de cada método.
- [x] Precisar accesibilidad, carácter de instancia o estático, parámetros y retorno.
- [x] Especificar precondiciones, resultados, efectos, validaciones y errores, incluidas las dependencias de DataBases y Utilidades.
- [x] Describir las secuencias de combate, ronda, partida y recuperación de cartas, con un ejemplo ilustrativo de comparación.
- [x] Distinguir las especificaciones actuales de los formatos y protocolo pendientes de las actividades 5 y 6.
- [x] Revisar las especificaciones con el usuario y resolver observaciones antes de dar la actividad por cerrada.

Verificación documental: las 58 operaciones del UML tienen una subsección de especificación, incluidos constructores y contratos externos. `git diff --check` no detectó errores y `dotnet build Proyecto-Integrador-Final.sln` terminó sin errores ni advertencias. La compilación verifica la base existente, no acredita implementación del diseño. Se precisó la retirada de cartas desde Combate.SetCarta y su recuperación única al cerrar la ronda. Formatos de archivos, protocolo e integración permanecen pendientes en las actividades 5 y 6.

### Actividad 5 — Formato de los archivos de personajes

**Completada por confirmación del usuario.** El cierre corresponde a la especificación acordada en Arquitectura y a la actualización del modelo; no implica implementación.

- [x] Definir estructura y reconocimiento de datos mediante el formato propio de DataBases y columnas correspondientes a los campos de Personaje.
- [x] Especificar nombre y seis atributos, con las restricciones numéricas y su validación al construir Personaje.
- [x] Incorporar descripcion al campo privado, constructor, GetDescripcion y columna del archivo: opcional, sin límite y ausente como texto vacío ("").
- [x] Definir ASCII: máximo 25 líneas de 120 caracteres por línea y sin tabulaciones. Se entrega con contenido, sin validar texto vacío; se mantiene el rechazo de null.
- [x] Documentar errores de formato, campos faltantes o incorrectos, archivos inexistentes e ilegibles y finalización sin recuperación.
- [x] Registrar que el usuario garantiza el catálogo sin duplicados y que la conversión al CSV se definirá después.
- [x] Registrar el cierre por confirmación del usuario.

No se elaboraron ejemplos propios válidos e inválidos; el cierre confirmado no los presenta como realizados ni los conserva como condición para reabrir esta actividad. La presentación de la descripción en consola sigue sin definir y no condiciona este cierre. La conversión al CSV queda postergada por indicación del usuario.

### Actividad 6 — Comunicación HTTP en red local

**En curso.** El 17 de noviembre de 2026 el usuario informó que se aprobó HTTP para la entrega en lugar del archivo compartido indicado en la consigna original. Se adopta el diseño de [API REST](Protocolo-partida-web.md), con dos clientes de consola y un servidor que aplica las reglas y conserva los datos.

- [x] Registrar la aprobación de HTTP y retirar el protocolo por archivos de la documentación vigente.
- [x] Documentar responsabilidades, despliegue y la secuencia propuesta de combate.

- [x] **Definir versiones y distribución de ejecución.** Terminado por confirmación del usuario: net8.0; ASP.NET Core y EF Core estables de la línea 8.0 (referencia 8.0.34); ambas computadoras Windows; servidor y cliente J1 en la primera y cliente J2 en la segunda. Primero se abre el servidor y después los clientes. La comprobación de instalaciones queda pendiente en las verificaciones, sin reabrir esta tarea.
- [x] Definir conexión, disponibilidad, administración, sesiones y reincorporación.
- [x] Especificar rutas, DTO, errores, estados, presencia, pausa y abandono.
- [x] Diseñar revisiones, idempotencia, concurrencia y persistencia transaccional.
- [x] Diseñar importación y mantenimiento del catálogo y copias para partidas iniciadas.
- [x] Documentar contratos de integración y UML de componentes web.
- [x] Documentar el procedimiento de verificación en dos equipos.
- [x] Vincular el protocolo con los [casos de la actividad 7](posibles-fallos.md#red-local-y-consistencia-http).
- [ ] Obtener revisión del PR por un compañero e integrar el diseño.

**Evidencia disponible:** el usuario informó comunicación correcta entre dos procesos mediante localhost. No aportó una ejecución reproducible del proyecto ni una prueba entre computadoras; no se presenta como implementación del juego o validación de la red escolar. No tiene los equipos disponibles para ejecutar las comprobaciones ahora.

**Verificaciones pendientes de instalaciones y comunicación entre computadoras:**

- [ ] Comprobar SDK y runtimes .NET/ASP.NET Core 8 en ambas computadoras y registrar resultados.
- [ ] Comprobar dirección real del servidor y disponibilidad del puerto TCP 5000.
- [ ] Verificar que el servidor acepte conexiones por su dirección de red y no solamente por localhost.
- [ ] Probar los dos clientes distribuidos en las dos computadoras con HTTP y dirección configurable.
- [ ] Comprobar si el firewall o la red escolar bloquean la comunicación; si hace falta una excepción, coordinarla con el responsable y limitarla al servicio y equipos participantes, sin desactivar el firewall ni abrir el router a Internet.
- [ ] Verificar servidor no iniciado, pérdida de conexión, reintento y salida sin duplicar elecciones; vincular estos escenarios con el diseño de idempotencia y concurrencia.
- [ ] Registrar condiciones de la red, configuración utilizada y resultados; no marcar exitosas pruebas todavía no ejecutadas.

### Actividad 7 — Casos de prueba

- [x] Documentar condiciones iniciales, datos, acción y resultados esperados de caja negra y blanca en el [plan de pruebas](posibles-fallos.md).
- [x] Cubrir personajes, formato del catálogo, mazo, reglas de combate y finalización.
- [x] Cubrir indisponibilidad, turno incorrecto, pausa, recuperación y concurrencia según el contrato HTTP de la actividad 6.
- [ ] Completar la revisión del plan por un compañero antes de integrarlo.

La ejecución y la evidencia de resultados quedan para Taller de Programación.

## Taller de Programación — C#

Etapas conservadas como referencia futura. No se implementarán en el alcance actual, limitado a TI. Las casillas marcadas registran código ya presente, no etapas terminadas ni cumplimiento de TDD: no hay pruebas de comportamiento que lo acrediten.

### Etapa 1 — Implementación de Personaje

- [ ] Completar Personaje conforme al modelo acordado: la clase existe, pero faltan descripcion y GetDescripcion.
- [x] Implementar el constructor con nombre, seis atributos y ASCII Art.
- [x] Implementar las validaciones de los atributos según los contratos de TI.
- [x] Implementar GetNombre, GetAscii y GetAtributo conservando el encapsulamiento. GetDescripcion permanece pendiente en la tarea de completar Personaje.
- [ ] Agregar en `test/` pruebas de personajes válidos y de cada restricción de puntajes.

### Etapa 2 — Carga de personajes con Datos y DataBases

- [x] Referenciar DataBases y Utilidades desde la aplicación e incluir sus proyectos en la solución.
- [ ] Implementar Personaje.Configurar con el formato definido en TI: el método existe, pero su cuerpo está vacío.
- [ ] Preparar la carga inicial de personajes en el servidor de acuerdo con el contrato de importación que se defina.
- [ ] Definir la ubicación del CSV de origen y en qué momento se importa al catálogo SQLite.
- [ ] Cargar el catálogo obligatorio mediante Modelo.Cargar<Personaje>(exigirArchivo: true).
- [ ] Obtener los personajes de la tabla cargada para construir el mazo inicial.
- [ ] Mantener la validación de puntajes en el constructor de Personaje y la interpretación del CSV en DataBases.
- [ ] Aplicar la finalización prevista para archivos inexistentes, ilegibles o inválidos.
- [ ] Probar la carga con los ejemplos de la actividad 5, sin reimplementar el lector CSV.

### Etapa 3 — Jugador e interacción de consola

- [ ] Crear Jugador con nombre, mano de personajes y marcador de rondas ganadas.
- [ ] Implementar recepción, consulta, retiro y entrega de personajes sin exponer la colección interna.
- [ ] Implementar InterfazConsola utilizando Funciones, DecoracionConsola y MyConsole de Utilidades.
- [ ] Seleccionar un Personaje de la mano disponible mediante PedirOpcionMenu y el atributo mediante PedirEnum<TipoAtributo>.
- [ ] Presentar mano, marcador, esperas, resultados y errores según los contratos de TI.
- [ ] Comprobar que la interfaz no revele las elecciones rivales hasta completar ambas elecciones.

### Etapa 4 — Administración del mazo

- [x] Crear Mazo con su colección privada de Personaje.
- [x] Implementar Mezclar, Repartir, Devolver y GetCantidad conforme a las firmas del UML.
- [ ] Comprobar que el reparto retire los elementos entregados y respete su disponibilidad.
- [ ] Restaurar el mazo completo al cerrar cada ronda, incluyendo personajes jugados y no jugados, sin pérdidas ni repeticiones.
- [ ] Impedir iniciar una partida con menos de seis personajes válidos y mostrar el mensaje acordado.
- [ ] Probar mazo vacío, reparto insuficiente, mezcla y recuperación completa.

### Etapa 5 — Implementación de las reglas

- [ ] Implementar Partida, Ronda y Combate con los contratos acordados en TI.
- [ ] Iniciar con J1 como jugador mano y repartir tres personajes a cada jugador.
- [ ] Registrar carta y atributo del elector y después la carta del rival según la secuencia definida.
- [ ] Resolver cada combate por el atributo elegido; ante empate gana quien no eligió el atributo.
- [ ] Retirar de la mano los personajes jugados para impedir reutilizarlos durante la ronda.
- [ ] Alternar el elector de atributo entre combates, independientemente del ganador.
- [ ] Cerrar la ronda al alcanzar dos combates ganados y sumar una ronda al marcador del vencedor.
- [ ] Recuperar todos los personajes y alternar el jugador mano al terminar cada ronda.
- [ ] Finalizar al alcanzar 15 rondas ganadas, sin iniciar otra ronda.
- [ ] Ejecutar las pruebas de combates, empates, alternancia, cierre anticipado de ronda y fin de partida.

### Etapa 6 — Comunicación HTTP entre computadoras

- [ ] Implementar la API REST y los clientes según las rutas, DTO y reglas de sesión aprobados en TI.
- [ ] Conectar la API con el dominio y guardar las transiciones en SQLite conforme al contrato de persistencia.
- [ ] Implementar vistas de jugador, validación autoritativa, concurrencia y reintentos idempotentes.
- [ ] Aplicar la política acordada de presencia, abandono y recuperación.
- [ ] Probar el intercambio en dos computadoras y registrar los resultados.

### Etapa 7 — Integración

- [ ] Integrar la carga de personajes, mazo, jugadores, reglas, consola y comunicación en la aplicación de consola.
- [ ] Comprobar que las firmas y responsabilidades implementadas coincidan con los contratos de TI.
- [ ] Resolver con los Analistas las diferencias detectadas y actualizar la documentación afectada.
- [ ] Ejecutar `dotnet build Proyecto-Integrador-Final.sln` y corregir los errores de compilación.
- [ ] Ejecutar `dotnet test Proyecto-Integrador-Final.sln` y corregir los fallos de las pruebas.
- [ ] Iniciar la aplicación en ambas computadoras con los personajes entregados por los alumnos.
- [ ] Completar una partida en red local y comprobar su resultado en ambas computadoras.
- [ ] Registrar los resultados del plan de pruebas y resolver los fallos pendientes.
- [ ] Revisar que el código esté en `src/`, las pruebas en `test/` y la documentación en `docs/`.
- [ ] Comprobar que estén disponibles todos los entregables de TI y el código fuente integrado.
- [ ] Preparar la defensa de las decisiones de diseño, estrategias de personajes, clases y métodos de cada integrante.

Los Analistas coordinan los entregables y comunican los cambios a Taller. Los cambios para GitHub siguen las ramas, los commits y la revisión por un compañero establecidos en [CONTRIBUTING](../CONTRIBUTING.md).

## Revisión del estado real — 13 de noviembre de 2026

Se revisaron main (`dcfc9eb`), las dos ramas de protocolos, los worktrees limpios, las referencias remotas tras `git fetch`, el código, los proyectos y el submódulo. Main ya estaba sincronizado con origin/main y no requirió actualización. Los avances de las ramas se registran sin integrarlos ni alterar sus directorios.

Personaje contiene constructor, las cuatro restricciones numéricas y consultas de nombre, ASCII y atributo. No contiene descripción ni configuración de persistencia efectiva. Mazo contiene constructor, mezcla Fisher–Yates, reparto, devolución y consulta de cantidad. Program conserva el ejemplo inicial; UnitTest1.Test1 no tiene aserciones. No existen todavía Jugador, Partida, Ronda, Combate, Datos ni InterfazConsola en src. DataBases y Utilidades sí están referenciadas e incluidas en la solución, aunque no hay flujo de carga ni juego conectado a ellas.

Verificación: `dotnet build Proyecto-Integrador-Final.sln` terminó sin errores; se observó CA1416 en Utilidades/MyConsole.cs por una API de consola exclusiva de Windows. La primera ejecución simultánea de compilación y pruebas produjo reintentos MSB3026 de copia; se repitieron secuencialmente para eliminar esa interferencia. `dotnet test Proyecto-Integrador-Final.sln` pasó una única prueba vacía, lo que no valida reglas ni comunicación. `git diff --check` verificó los cambios documentales.

Se mantienen los cierres confirmados de las actividades 1 a 5 y los pendientes de la actividad 7. Los registros históricos de compilación y diseño que siguen describen sus revisiones originales; no sustituyen esta comprobación ni acreditan el comportamiento del juego.

## Revisión de documentación — 9 de noviembre de 2026

Se contrastaron los diez documentos Markdown del repositorio y del submódulo con el código, los proyectos, el UML y el estado de los PR #6, #7, #9 y #10 en GitHub. Se actualizaron las referencias a PR ya integrados, la estructura publicada y el flujo de sincronización de ramas. README y Arquitectura aclaran que las bibliotecas todavía no están referenciadas por la aplicación ni incluidas en la solución principal. La consigna original y las decisiones de diseño se conservan; las actividades 5 a 7 siguen pendientes. No se detectaron correcciones necesarias en los README del submódulo para esta revisión y se conservó su versión registrada.

Verificación: los 33 enlaces locales apuntan a archivos o directorios existentes; el XML del UML es válido y sus 58 operaciones tienen 58 subsecciones de especificación en Arquitectura. `git diff --check` no encontró problemas. La solución compiló sin advertencias ni errores y `dotnet test Proyecto-Integrador-Final.sln --no-build` ejecutó correctamente la única prueba existente, que está vacía. La ejecución de la aplicación mostró `Hello, World!`. Estos resultados verifican la base inicial; no acreditan funcionamiento del juego, pruebas de las bibliotecas ni comunicación en red.

## Resultado de la revisión del modelo — 8 de noviembre de 2026

Se actualizó el UML y se justificaron contratos y responsabilidades en Arquitectura, incluyendo consultas de combate y ronda necesarias para presentar resultados y recuperar cartas sin exponer colecciones privadas. Se comprobó el XML del diagrama y se revisaron firmas, tipos y dependencias contra las bibliotecas. `git diff --check` no encontró problemas y `dotnet build Proyecto-Integrador-Final.sln` terminó con cero errores y advertencias. Esta compilación solo verifica la base inicial existente; no acredita implementación ni funcionamiento del juego. No se agregaron pruebas de código para un cambio documental. La revisión visual se completó mediante exportación con UMLet 15.1 y se verificaron geométricamente las 16 relaciones, sin tramos que atraviesen cajas y con extremos conectados a bordes. Se consultó UML 2.5.1 y se corrigió la notación de enumeraciones, miembros estáticos y realización de interfaz. La integración del protocolo sigue pendiente; el PR #9 del modelo ya está integrado desde el 8 de noviembre de 2026 (hora de Argentina).

Corrección de notación solicitada por el usuario: referencias y colecciones se muestran solo como asociaciones, con roles separados, multiplicidad y visibilidad explícita. Datos.Modelo conserva lectura pública, escritura privada y carácter estático en la relación.

Por indicación del usuario, se retiraron las cuatro flechas de dependencia `.>`; el uso de las bibliotecas y entidades se explica en Arquitectura. Las asociaciones y la realización de la interfaz de DataBases se conservan.

Datos se amplió con constantes de rutas y configuración explícita de TipoProceso. Los valores reales de red y las entidades de intercambio siguen pendientes; no se implementó código.

Se ajustaron las dimensiones de las cajas al texto y se verificaron las exportaciones PNG y SVG de UMLet: sin textos recortados ni relaciones atravesando cajas.

Se compactó la distribución de clases y se recalcularon los recorridos de las relaciones, conservando las cajas ajustadas al texto. La exportación de UMLet se revisó visualmente.

Revisión final del dominio: jugador mano pertenece solo a Ronda; Combate ofrece GetParticipantes para presentar sus elecciones y Datos.Modelo tiene multiplicidad 0..1 hasta inicializarse. Se revisaron firmas, encapsulamiento y coherencia de reglas. Por decisión posterior del usuario, la actividad 3 se da por terminada y el modelado de entidades, relaciones y firmas de intercambio queda asignado exclusivamente a la actividad 6. Los detalles de métodos, formato del catálogo y pruebas permanecen en sus actividades respectivas; la revisión de un compañero sigue siendo requisito para integrar el PR.

Cierre de la actividad 3 por confirmación del usuario: sin tareas propias pendientes. El modelado de elecciones y publicaciones, su configuración en Datos y los métodos de integración de clientes y Partida quedan en la actividad 6. El cierre documental quedó incorporado mediante el PR #9; no implica implementación.

## Ampliación documental de Personaje — actividad 5

Por indicación del usuario se agregó descripcion: string al modelo de Personaje y a sus columnas, se amplió el constructor y se especificó GetDescripcion. La descripción es opcional y no tiene límite de longitud por confirmación del usuario; su ausencia se representa con texto vacío ("") y su presentación sigue pendiente de decisión. La conversión al CSV se resolverá posteriormente y el usuario garantiza un catálogo preparado sin duplicados. No se implementó código. Posteriormente el usuario confirmó el cierre de la actividad 5 con esta especificación. Los registros anteriores de 58 operaciones corresponden a la revisión previa a esta ampliación.

## Decisiones reemplazadas de actividad 6

Se aprobaron inicialmente Accion CSV con doce campos y diecisiete tipos, y luego se eligió JSON para Accion y Estado por simplicidad. Estas decisiones pertenecen al diseño histórico por archivos, retirado tras la aprobación de HTTP. El ID permanente de Personaje continúa vigente. No quedan pendientes de configurar Accion en DataBases ni de reconstruir la partida desde logs. Los mecanismos de sesión y publicación deben verificarse como diseño propuesto.
