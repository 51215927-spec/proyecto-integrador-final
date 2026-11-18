# Plan de trabajo — Juego Guerra

Plan basado en la [consigna](consigna.md). Las casillas registran entregables de diseño en TI y trabajo futuro en Taller.

| Actividades de TI | Estado |
|---|---|
| Personajes, requisitos, modelo, métodos y archivos | Actividades 1 a 5 completadas en TI. |
| Comunicación HTTP | Diseño documentado. Verificación entre computadoras pendiente. |
| Casos de prueba | Escenarios y resultados esperados documentados. Ejecución prevista en Taller. |

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

- [x] Documentar participantes, reglas de combate, ronda y partida en [Reglas del juego](reglas-del-juego.md).
- [x] Definir catálogo, mazo, reparto, duplicados y mínimo de cartas.
- [x] Especificar datos de ejecución, validaciones, errores y condiciones técnicas.
- [x] Preparar el texto para que el usuario lo copie al documento de entrega.

### Actividad 3 — Modelo de clases

- [x] Definir responsabilidades, encapsulamiento, dependencias y relaciones del dominio en [Arquitectura](arquitectura.md) y el [diagrama UML](diagramas/uml.uxf).
- [x] Modelar la carga del catálogo, la coordinación de la partida y la presentación de consola.

La integración HTTP se documenta en la actividad 6.

### Actividad 4 — Especificación de métodos

- [x] Especificar las firmas, precondiciones, efectos, retornos, validaciones y errores de cada operación en [Arquitectura](arquitectura.md#especificación-de-clases-y-métodos--actividad-4).

Para este proyecto se acordó documentar cada método por escrito, sin diagramas Nassi–Schneiderman.

### Actividad 5 — Formato de los archivos de personajes

- [x] Definir el [formato de personajes](arquitectura.md#formato-de-archivos-de-personajes--actividad-5), incluidas sus columnas, validaciones y errores de lectura.
- [x] Incorporar ID, descripción y ASCII al catálogo y a los contratos de Personaje.

La actividad está cerrada. Quedan postergados los ejemplos propios y la conversión al CSV. La presentación de la descripción se trata en [Interfaz](interfaz.md#decisiones-pendientes).

### Actividad 6 — Comunicación HTTP en red local

**En curso.** El 17 de noviembre de 2026 el usuario informó que se aprobó HTTP para la entrega en lugar del archivo compartido indicado en la consigna original. Se adopta el diseño de [API REST](protocolo-partida-web.md), con dos clientes de consola y un servidor que aplica las reglas y conserva los datos.

- [x] Documentar tecnologías, despliegue, sesiones y reincorporación en el [protocolo](protocolo-partida-web.md).
- [x] Especificar rutas, DTO, errores, estados, presencia, pausa y abandono.
- [x] Diseñar el control de revisión de estado, la idempotencia y la persistencia transaccional.
- [x] Documentar el modelo de integración y vincular los casos de prueba HTTP.

### Actividad 7 — Casos de prueba

- [x] Documentar condiciones iniciales, datos, acción y resultados esperados de caja negra y blanca en el [plan de pruebas](posibles-fallos.md).
- [x] Cubrir personajes, formato del catálogo, mazo, reglas de combate y finalización.
- [x] Cubrir indisponibilidad, turno incorrecto, pausa, recuperación y concurrencia según el contrato HTTP de la actividad 6.

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
- [ ] Definir el contrato de la API del servidor para el alta individual de personajes.
- [ ] Implementar el [alta por consola en `subir-personajes`](arquitectura.md#alta-de-personajes-desde-consola). Pedir los datos con `PedirString` y `PedirInt`, enviarlos por la API y preguntar si se desea cargar otro personaje tras la confirmación.
- [ ] Definir la ubicación del CSV de origen y en qué momento se importa al catálogo SQLite.
- [ ] Cargar el catálogo obligatorio mediante Modelo.Cargar<Personaje>(exigirArchivo: true).
- [ ] Obtener los personajes de la tabla cargada para construir el mazo inicial.
- [ ] Mantener la validación de puntajes en el constructor de Personaje y la interpretación del CSV en DataBases.
- [ ] Aplicar la finalización prevista para archivos inexistentes, ilegibles o inválidos.
- [ ] Probar la carga con los ejemplos de la actividad 5, sin reimplementar el lector CSV.

### Etapa 3 — Jugador e interacción de consola

El [diseño de la interfaz](interfaz.md) registra el inventario inicial preparado en TI y las decisiones pendientes antes de implementar la interacción.

- [ ] Crear Jugador con nombre, mano de personajes y marcador de rondas ganadas.
- [ ] Implementar recepción, consulta, retiro y entrega de personajes sin exponer la colección interna.
- [ ] Implementar la composición y los menús propios de InterfazConsola según el [diseño de la interfaz](interfaz.md), con reutilización puntual de Utilidades.
- [ ] Validar la carta elegida contra la mano disponible y el atributo contra las opciones del combate.
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
- [ ] Ejecutar `dotnet build proyecto-integrador-final.sln` y corregir los errores de compilación.
- [ ] Ejecutar `dotnet test proyecto-integrador-final.sln` y corregir los fallos de las pruebas.
- [ ] Iniciar la aplicación en ambas computadoras con los personajes entregados por los alumnos.
- [ ] Completar una partida en red local y comprobar su resultado en ambas computadoras.
- [ ] Registrar los resultados del plan de pruebas y resolver los fallos pendientes.
- [ ] Revisar que el código esté en `src/`, las pruebas en `test/` y la documentación en `docs/`.
- [ ] Comprobar que estén disponibles todos los entregables de TI y el código fuente integrado.
- [ ] Preparar la defensa de las decisiones de diseño, estrategias de personajes, clases y métodos de cada integrante.

Los Analistas coordinan los entregables y comunican los cambios a Taller. El flujo de GitHub está en la [guía de colaboración](../CONTRIBUTING.md).
