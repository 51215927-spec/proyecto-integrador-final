# Plan de trabajo — Juego Guerra

Plan basado en la [consigna](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Consigna). El usuario confirmó la entrega de TI. Las casillas registran los entregables de diseño y el estado del trabajo de Taller.

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

- [x] Documentar participantes, reglas de combate, ronda y partida en [Reglas del juego](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Reglas-del-juego).
- [x] Definir catálogo, mazo, reparto, duplicados y mínimo de cartas.
- [x] Especificar datos de ejecución, validaciones, errores y condiciones técnicas.
- [x] Preparar el texto para que el usuario lo copie al documento de entrega.

### Actividad 3 — Modelo de clases

- [x] Definir responsabilidades, encapsulamiento, dependencias y relaciones del dominio en [Arquitectura](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Arquitectura) y el [diagrama UML incluido en los descargables](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Home).
- [x] Modelar la carga del catálogo, la coordinación de la partida y la presentación de consola.

La integración HTTP se documenta en la actividad 6.

### Actividad 4 — Especificación de métodos

- [x] Especificar las firmas, precondiciones, efectos, retornos, validaciones y errores de cada operación en [Arquitectura](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Arquitectura#especificación-de-clases-y-métodos--actividad-4).

Para este proyecto se acordó documentar cada método por escrito, sin diagramas Nassi–Schneiderman.

### Actividad 5 — Datos y validación de personajes

- [x] Definir los [datos de los personajes](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Arquitectura#datos-y-validación-de-personajes--actividad-5) y sus validaciones.
- [x] Incorporar ID, descripción y ASCII al catálogo y a los contratos de Personaje.

La decisión vigente es cargar los personajes manualmente mediante `subir-personajes`. Esta decisión sustituye la lectura de archivos de personajes prevista en la consigna original. El catálogo conserva el arte ASCII y la descripción. El juego prescinde de ambos durante la partida. La decisión se registra en [Interfaz](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Diseno-de-interfaz#arte-y-descripción-de-personajes).

### Actividad 6 — Comunicación HTTP en red local

**Diseño entregado en TI.** El 17 de noviembre de 2026 el usuario informó que se aprobó HTTP para la entrega en lugar del archivo compartido indicado en la consigna original. Se adopta el diseño de [API REST](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Protocolo-de-partida), con dos clientes de consola y un servidor que aplica las reglas y conserva los datos.

- [x] Documentar tecnologías, despliegue, sesiones y reincorporación en el [protocolo](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Protocolo-de-partida).
- [x] Especificar rutas, DTO, errores, estados, presencia, pausa y abandono.
- [x] Diseñar el control de revisión de estado, la idempotencia y la persistencia transaccional.
- [x] Documentar el modelo de integración y vincular los casos de prueba HTTP.

### Actividad 7 — Casos de prueba

- [x] Documentar condiciones iniciales, datos, acción y resultados esperados de caja negra y blanca en el [plan de pruebas](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Plan-de-pruebas).
- [x] Cubrir personajes, formato del catálogo, mazo, reglas de combate y finalización.
- [x] Cubrir indisponibilidad, turno incorrecto, pausa, recuperación y concurrencia según el contrato HTTP de la actividad 6.

La ejecución y la evidencia de resultados se registran en Taller de Programación.

## Taller de Programación — C#

El alcance de TI reúne análisis y diseño. El desarrollo de la API autorizado para Taller se registra en esta etapa. Las casillas registran avances concretos de cada etapa.

### Etapa 1 — Implementación de Personaje

- [ ] Completar Personaje conforme al modelo acordado: la clase existe, pero faltan descripcion y GetDescripcion.
- [x] Implementar el constructor con nombre, seis atributos y ASCII Art.
- [x] Implementar las validaciones de los atributos según los contratos de TI.
- [x] Implementar GetNombre, GetAscii y GetAtributo conservando el encapsulamiento. GetDescripcion permanece pendiente en la tarea de completar Personaje.
- [x] Agregar en `test/` pruebas de personajes válidos y de cada restricción de puntajes.

### Etapa 2 — Carga manual de personajes

- [x] Definir el [contrato de alta individual](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Protocolo-de-partida#alta-individual-de-personajes).
- [x] Pedir los datos en `subir-personajes`, enviarlos por HTTP y permitir otra alta tras confirmar el ID.
- [x] Verificar dos altas consecutivas y el rechazo con una credencial incorrecta.


### Etapa 3 — Jugador e interacción de consola

El [diseño de la interfaz](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Diseno-de-interfaz) registra las pantallas, los controles implementados en el cliente y las decisiones pendientes del flujo cliente.

- [x] Crear Jugador con nombre, mano de personajes y marcador de rondas ganadas.
- [x] Implementar recepción, consulta, retiro y entrega de personajes sin exponer la colección interna.
- [x] Implementar la composición y los menús propios de InterfazConsola según el [diseño de la interfaz](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Diseno-de-interfaz), con renderizado mediante Spectre.Console.
- [x] Validar la carta elegida contra las opciones de mano recibidas y el atributo contra las opciones de la solicitud.
- [x] Presentar mano, marcador, esperas, resultados y errores según los contratos de TI.
- [ ] Comprobar que la interfaz no revele las elecciones rivales hasta completar ambas elecciones.

### Etapa 4 — Administración del mazo

- [x] Crear Mazo con su colección privada de Personaje.
- [x] Implementar Mezclar, Repartir, Devolver y GetCantidad conforme a las firmas del UML.
- [x] Comprobar que el reparto retire los elementos entregados y respete su disponibilidad.
- [ ] Restaurar el mazo completo al cerrar cada ronda, incluyendo personajes jugados y no jugados, sin pérdidas ni repeticiones.
- [ ] Impedir iniciar una partida con menos de seis personajes válidos y mostrar el mensaje acordado.
- [ ] Probar mazo vacío, reparto insuficiente, mezcla y recuperación completa.

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

- [x] Implementar y probar la API REST del servidor según el contrato de partidas.
- [ ] Conectar los clientes de consola con la API.
- [x] Conectar la API con el dominio y guardar las transiciones en SQLite conforme al contrato de persistencia.
- [x] Implementar vistas de jugador, validación autoritativa, concurrencia y reintentos idempotentes.
- [x] Aplicar la política acordada de presencia, abandono y recuperación en el servidor.
- [ ] Probar el intercambio en dos computadoras y registrar los resultados.

### Etapa 7 — Integración

- [ ] Integrar la carga de personajes, mazo, jugadores, reglas, consola y comunicación en la aplicación de consola.
- [ ] Comprobar que las firmas y responsabilidades implementadas coincidan con los contratos de TI.
- [ ] Resolver con los Analistas las diferencias detectadas y actualizar la documentación afectada.
- [x] Verificar en `main` (`eac766b`) que la solución compila sin advertencias ni errores y que las 85 pruebas automatizadas pasan al habilitar las pruebas de contrato.
- [ ] Repetir la compilación y las pruebas después de integrar el cliente REST.
- [ ] Iniciar la aplicación en ambas computadoras con los personajes entregados por los alumnos.
- [ ] Completar una partida en red local y comprobar su resultado en ambas computadoras.
- [ ] Registrar los resultados del plan de pruebas y resolver los fallos pendientes.
- [ ] Revisar que el código esté en `src/`, las pruebas en `test/` y la documentación en la [wiki](https://github.com/51215927-spec/proyecto-integrador-final/wiki/Home).
- [x] Registrar la entrega de TI confirmada por el usuario.
- [ ] Verificar los entregables y el código fuente integrado para Taller.
- [ ] Preparar la defensa de las decisiones de diseño, estrategias de personajes, clases y métodos de cada integrante.

Los Analistas coordinan los entregables y comunican los cambios a Taller. El flujo de GitHub está en la [guía de colaboración](../CONTRIBUTING.md).
