# Catálogo de problemas de calidad del código

Catálogo general para revisar código y detectar problemas que podrían
justificar una corrección o refactorización. Los hallazgos deben
evaluarse según el contexto: que una regla aparezca en esta tabla no
significa que todo uso relacionado sea un error.

## Tabla general

| ID      | Categoría                       | Problema                              | Descripción                                                                                           |
|:--------|:--------------------------------|:--------------------------------------|:------------------------------------------------------------------------------------------------------|
| BUG-001 | Errores funcionales             | Lógica incorrecta                     | Una condición o algoritmo produce resultados equivocados.                                             |
| BUG-002 | Errores funcionales             | Condiciones incompletas               | Faltan casos en una condición o estructura de selección.                                              |
| BUG-003 | Errores funcionales             | Errores de límites                    | Se accede a posiciones fuera de los límites de una colección.                                         |
| BUG-004 | Errores funcionales             | Errores de inicialización             | Se utilizan variables u objetos antes de inicializarlos correctamente.                                |
| BUG-005 | Errores funcionales             | Errores de estado                     | El programa termina en un estado inválido o inconsistente.                                            |
| BUG-006 | Errores funcionales             | Errores de cálculo                    | Hay cálculos incorrectos, desbordamientos o pérdida de precisión.                                     |
| BUG-007 | Errores funcionales             | Errores de conversión                 | Las conversiones de tipos son incorrectas o pierden información inesperadamente.                      |
| BUG-008 | Errores funcionales             | Casos extremos ignorados              | No se contemplan valores nulos, vacíos, negativos, máximos u otras entradas límite.                   |
| BUG-009 | Errores funcionales             | Errores de orden                      | Las operaciones se ejecutan en un orden incorrecto.                                                   |
| BUG-010 | Errores funcionales             | Errores de sincronización             | Varias operaciones producen resultados inconsistentes por su orden de ejecución.                      |
| BUG-011 | Errores funcionales             | Efectos secundarios inesperados       | Una operación modifica información que no debería modificar.                                          |
| BUG-012 | Errores funcionales             | Errores de concurrencia               | Se producen condiciones de carrera, deadlocks o accesos concurrentes inseguros.                       |
| DES-001 | Arquitectura y diseño           | Violación de responsabilidad única    | Una clase o función tiene demasiadas responsabilidades independientes.                                |
| DES-002 | Arquitectura y diseño           | Acoplamiento excesivo                 | Los componentes dependen demasiado unos de otros.                                                     |
| DES-003 | Arquitectura y diseño           | Baja cohesión                         | Una clase agrupa elementos que no comparten una responsabilidad clara.                                |
| DES-004 | Arquitectura y diseño           | Duplicación de lógica                 | El mismo comportamiento está implementado en varios lugares.                                          |
| DES-005 | Arquitectura y diseño           | Abstracciones innecesarias            | Se crean interfaces, clases o capas que no aportan valor.                                             |
| DES-006 | Arquitectura y diseño           | Falta de abstracciones                | Se repite lógica que podría encapsularse en un componente común.                                      |
| DES-007 | Arquitectura y diseño           | Dependencias ocultas                  | Una clase depende de servicios o estados que no están declarados claramente.                          |
| DES-008 | Arquitectura y diseño           | Responsabilidades mal ubicadas        | Una operación está implementada en un componente que no debería encargarse de ella.                   |
| DES-009 | Arquitectura y diseño           | Jerarquías incorrectas                | La herencia representa relaciones que no corresponden al dominio o al diseño.                         |
| DES-010 | Arquitectura y diseño           | Violación de encapsulamiento          | Se expone información interna que debería permanecer protegida.                                       |
| DES-011 | Arquitectura y diseño           | Inversión de dependencias incorrecta  | Los componentes de alto nivel dependen directamente de implementaciones concretas.                    |
| DES-012 | Arquitectura y diseño           | Complejidad accidental                | La solución tiene una estructura más complicada de lo necesario.                                      |
| DES-013 | Arquitectura y diseño           | Máquina de estados implícita          | El estado del sistema está distribuido entre múltiples variables y condiciones.                       |
| DES-014 | Arquitectura y diseño           | Dependencias circulares               | Dos o más componentes dependen mutuamente entre sí.                                                   |
| LEG-001 | Legibilidad y mantenibilidad    | Nombres poco descriptivos             | Variables, métodos o clases tienen nombres ambiguos.                                                  |
| LEG-002 | Legibilidad y mantenibilidad    | Métodos demasiado largos              | Una función realiza demasiadas operaciones o responsabilidades.                                       |
| LEG-003 | Legibilidad y mantenibilidad    | Clases demasiado grandes              | Una clase concentra demasiada lógica o estado.                                                        |
| LEG-004 | Legibilidad y mantenibilidad    | Anidamiento excesivo                  | La cantidad de bloques y condiciones anidadas dificulta seguir el flujo.                              |
| LEG-005 | Legibilidad y mantenibilidad    | Comentarios redundantes               | Los comentarios explican algo que ya resulta evidente en el código.                                   |
| LEG-006 | Legibilidad y mantenibilidad    | Comentarios desactualizados           | Los comentarios contradicen el comportamiento actual.                                                 |
| LEG-007 | Legibilidad y mantenibilidad    | Código muerto                         | Existen variables, métodos o instrucciones que no se utilizan o no pueden ejecutarse.                 |
| LEG-008 | Legibilidad y mantenibilidad    | Código duplicado                      | Hay fragmentos similares que podrían compartir una implementación.                                    |
| LEG-009 | Legibilidad y mantenibilidad    | Números mágicos                       | Se utilizan valores literales sin expresar su significado.                                            |
| LEG-010 | Legibilidad y mantenibilidad    | Complejidad ciclomática elevada       | Una función contiene demasiados caminos de ejecución.                                                 |
| LEG-011 | Legibilidad y mantenibilidad    | Complejidad cognitiva elevada         | Resulta difícil comprender el flujo de una función o bloque.                                          |
| LEG-012 | Legibilidad y mantenibilidad    | Formato inconsistente                 | El estilo de escritura cambia sin una convención clara.                                               |
| LEG-013 | Legibilidad y mantenibilidad    | Parámetros excesivos                  | Un método recibe demasiados argumentos o datos relacionados que podrían agruparse.                    |
| LEG-014 | Legibilidad y mantenibilidad    | Variables temporales innecesarias     | Se crean variables que no aportan claridad ni funcionalidad.                                          |
| LEG-015 | Legibilidad y mantenibilidad    | Abreviaciones ambiguas                | Se utilizan abreviaciones difíciles de interpretar.                                                   |
| TYP-001 | Tipado y datos                  | Tipos demasiado genéricos             | Se utilizan tipos como `object` o `any` sin una necesidad justificada.                                |
| TYP-002 | Tipado y datos                  | Conversiones inseguras                | Se realizan conversiones sin comprobar que sean válidas.                                              |
| TYP-003 | Tipado y datos                  | Uso incorrecto de null                | No se contemplan valores nulos o se utilizan de forma incorrecta.                                     |
| TYP-004 | Tipado y datos                  | Tipos primitivos mal utilizados       | Se representan conceptos del dominio mediante strings, números o booleanos sin encapsular sus reglas. |
| TYP-005 | Tipado y datos                  | Estado inválido representable         | El diseño permite construir objetos que deberían ser imposibles o inválidos.                          |
| TYP-006 | Tipado y datos                  | Mutabilidad innecesaria               | Los datos pueden modificarse cuando no deberían o no necesitan hacerlo.                               |
| TYP-007 | Tipado y datos                  | Colecciones incorrectas               | Se utiliza una estructura de datos poco apropiada para las operaciones requeridas.                    |
| TYP-008 | Tipado y datos                  | Falta de validación                   | Se aceptan datos que no cumplen las reglas del sistema.                                               |
| TYP-009 | Tipado y datos                  | Inconsistencia de datos               | Distintas partes del programa mantienen representaciones diferentes de la misma información.          |
| TYP-010 | Tipado y datos                  | Pérdida de precisión                  | Se utilizan tipos numéricos que no ofrecen la precisión necesaria.                                    |
| ERR-001 | Errores y excepciones           | Excepciones ignoradas                 | Se captura una excepción sin hacer nada con ella ni comunicarla.                                      |
| ERR-002 | Errores y excepciones           | Captura demasiado general             | Se captura cualquier excepción sin distinguir su origen o tratamiento.                                |
| ERR-003 | Errores y excepciones           | Excepciones mal utilizadas            | Se utilizan excepciones para controlar el flujo normal del programa.                                  |
| ERR-004 | Errores y excepciones           | Falta de manejo de errores            | Una operación puede fallar sin una estrategia definida para tratar el fallo.                          |
| ERR-005 | Errores y excepciones           | Errores ocultos                       | Se informa que una operación tuvo éxito cuando realmente falló.                                       |
| ERR-006 | Errores y excepciones           | Pérdida de contexto                   | Se reemplaza una excepción sin conservar información relevante de la original.                        |
| ERR-007 | Errores y excepciones           | Mensajes poco descriptivos            | Los errores no explican qué sucedió ni aportan información útil.                                      |
| ERR-008 | Errores y excepciones           | Recuperación incorrecta               | El programa continúa después de un fallo y deja el sistema inconsistente.                             |
| ERR-009 | Errores y excepciones           | Validación tardía                     | Un error se detecta mucho después del punto en el que podría haberse evitado.                         |
| ERR-010 | Errores y excepciones           | Errores no propagados                 | Una función oculta un error que debería comunicar a quien la llamó.                                   |
| PER-001 | Rendimiento y recursos          | Algoritmos ineficientes               | Se utilizan algoritmos con una complejidad temporal o espacial innecesariamente alta.                 |
| PER-002 | Rendimiento y recursos          | Operaciones redundantes               | Se realizan cálculos o tareas que podrían evitarse o reutilizarse.                                    |
| PER-003 | Rendimiento y recursos          | Asignaciones innecesarias             | Se crean objetos o estructuras que no hacen falta.                                                    |
| PER-004 | Rendimiento y recursos          | Consultas repetidas                   | Se solicita varias veces información que podría reutilizarse.                                         |
| PER-005 | Rendimiento y recursos          | Estructuras de datos incorrectas      | Se utilizan colecciones poco apropiadas para las operaciones realizadas.                              |
| PER-006 | Rendimiento y recursos          | Bloqueos innecesarios                 | Se impide la ejecución concurrente o paralela cuando no es necesario.                                 |
| PER-007 | Rendimiento y recursos          | Recursos no liberados                 | Se mantienen archivos, conexiones u otros recursos abiertos más tiempo del necesario.                 |
| PER-008 | Rendimiento y recursos          | Fugas de memoria                      | La memoria ocupada no puede recuperarse cuando debería.                                               |
| PER-009 | Rendimiento y recursos          | Trabajo en el hilo principal          | Se realizan operaciones costosas que bloquean la interfaz o el procesamiento principal.               |
| PER-010 | Rendimiento y recursos          | Serialización innecesaria             | Se convierten datos a formatos como JSON repetidamente sin necesidad.                                 |
| PER-011 | Rendimiento y recursos          | Uso inadecuado de caché               | Se recalculan datos reutilizables o se almacenan datos sin una estrategia adecuada.                   |
| PER-012 | Rendimiento y recursos          | Complejidad de consultas              | Se ejecutan consultas de base de datos innecesariamente costosas.                                     |
| CON-001 | Concurrencia y asincronía       | Race condition                        | El resultado depende del orden en que se ejecutan varias operaciones concurrentes.                    |
| CON-002 | Concurrencia y asincronía       | Deadlock                              | Dos o más operaciones esperan indefinidamente recursos que otras mantienen bloqueados.                |
| CON-003 | Concurrencia y asincronía       | Starvation                            | Una operación no consigue los recursos necesarios para avanzar.                                       |
| CON-004 | Concurrencia y asincronía       | Acceso concurrente inseguro           | Varios hilos modifican información compartida sin protección adecuada.                                |
| CON-005 | Concurrencia y asincronía       | Sincronización incorrecta             | Se utilizan mecanismos de sincronización de forma equivocada.                                         |
| CON-006 | Concurrencia y asincronía       | Tareas sin supervisión                | Se inician tareas asíncronas sin controlar sus resultados o excepciones.                              |
| CON-007 | Concurrencia y asincronía       | Cancelación incorrecta                | Las operaciones no respetan las solicitudes de cancelación.                                           |
| CON-008 | Concurrencia y asincronía       | Recursos compartidos sin protección   | Se accede a recursos comunes de manera insegura.                                                      |
| CON-009 | Concurrencia y asincronía       | Reintentos duplicados                 | Una operación se ejecuta más de una vez cuando no debería.                                            |
| CON-010 | Concurrencia y asincronía       | Operaciones fuera de orden            | Las respuestas llegan o se aplican en un orden diferente al esperado.                                 |
| CON-011 | Concurrencia y asincronía       | Falta de idempotencia                 | Repetir una operación produce efectos adicionales no deseados.                                        |
| TST-001 | Pruebas y calidad               | Falta de pruebas                      | No existen pruebas para funcionalidades importantes.                                                  |
| TST-002 | Pruebas y calidad               | Cobertura insuficiente                | Hay partes relevantes del código que no se prueban.                                                   |
| TST-003 | Pruebas y calidad               | Casos extremos sin probar             | No se prueban entradas vacías, límites o situaciones excepcionales.                                   |
| TST-004 | Pruebas y calidad               | Pruebas dependientes                  | Una prueba necesita que otra se ejecute primero.                                                      |
| TST-005 | Pruebas y calidad               | Pruebas frágiles                      | Cambios irrelevantes rompen las pruebas.                                                              |
| TST-006 | Pruebas y calidad               | Pruebas que no verifican resultados   | Las pruebas ejecutan código, pero no comprueban el comportamiento esperado.                           |
| TST-007 | Pruebas y calidad               | Acoplamiento excesivo en pruebas      | Es difícil probar un componente sin ejecutar otros componentes innecesarios.                          |
| TST-008 | Pruebas y calidad               | Falta de pruebas de integración       | No se verifica la interacción entre componentes.                                                      |
| TST-009 | Pruebas y calidad               | Falta de pruebas de regresión         | No se comprueba que las modificaciones mantengan el comportamiento anterior.                          |
| TST-010 | Pruebas y calidad               | Datos de prueba poco representativos  | Los casos de prueba no reflejan situaciones relevantes o realistas.                                   |
| MOD-001 | Modularidad y reutilización     | Código difícil de reutilizar          | La lógica está demasiado ligada a un contexto específico.                                             |
| MOD-002 | Modularidad y reutilización     | Dependencias innecesarias             | Un módulo conoce componentes que no necesita.                                                         |
| MOD-003 | Modularidad y reutilización     | Interfaces demasiado grandes          | Una interfaz obliga a implementar operaciones que no se utilizan.                                     |
| MOD-004 | Modularidad y reutilización     | Duplicación entre módulos             | Diferentes módulos implementan la misma funcionalidad.                                                |
| MOD-005 | Modularidad y reutilización     | Límites mal definidos                 | No está claro qué responsabilidad pertenece a cada módulo.                                            |
| MOD-006 | Modularidad y reutilización     | API pública excesiva                  | Se exponen operaciones que deberían permanecer internas.                                              |
| MOD-007 | Modularidad y reutilización     | Dependencias de implementación        | Los módulos dependen de detalles internos en lugar de contratos estables.                             |
| INF-001 | Configuración e infraestructura | Configuración hardcodeada             | Direcciones, rutas o parámetros están escritos directamente en el código.                             |
| INF-002 | Configuración e infraestructura | Configuración duplicada               | La misma configuración aparece en varios lugares.                                                     |
| INF-003 | Configuración e infraestructura | Configuración insegura                | La configuración contiene información sensible o permisos inadecuados.                                |
| INF-004 | Configuración e infraestructura | Falta de validación de configuración  | El programa inicia con parámetros inválidos o incompletos.                                            |
| INF-005 | Configuración e infraestructura | Persistencia inconsistente            | Los datos guardados no representan correctamente el estado del sistema.                               |
| INF-006 | Configuración e infraestructura | Transacciones incorrectas             | Una operación parcialmente ejecutada deja datos inconsistentes.                                       |
| INF-007 | Configuración e infraestructura | Falta de recuperación                 | No existe una estrategia ante fallos de almacenamiento o conexión.                                    |
| INF-008 | Configuración e infraestructura | Migraciones incorrectas               | Los cambios en el esquema de datos no se aplican de forma segura o coherente.                         |
| INF-009 | Configuración e infraestructura | Acoplamiento a infraestructura        | La lógica del dominio depende directamente de detalles de bases de datos, archivos o red.             |
| DOC-001 | Documentación y convenciones    | Falta de documentación                | No se explica cómo utilizar o configurar el proyecto.                                                 |
| DOC-002 | Documentación y convenciones    | Documentación desactualizada          | Los documentos no coinciden con el comportamiento actual del código.                                  |
| DOC-003 | Documentación y convenciones    | Contratos poco claros                 | No se documentan las entradas, salidas y errores de una API o componente.                             |
| DOC-004 | Documentación y convenciones    | Convenciones inconsistentes           | Se mezclan estilos de nombres y organización sin una razón clara.                                     |
| DOC-005 | Documentación y convenciones    | Falta de instrucciones de instalación | No se puede reproducir fácilmente el entorno de desarrollo o ejecución.                               |
| DOC-006 | Documentación y convenciones    | Dependencias no documentadas          | No se especifican las bibliotecas o herramientas necesarias.                                          |

## Criterios para evaluar un hallazgo

| Criterio        | Descripción                                                                     |
|:----------------|:--------------------------------------------------------------------------------|
| Evidencia       | Fragmento, archivo, comportamiento o prueba que permite justificar el hallazgo. |
| Severidad       | Impacto potencial: crítica, alta, media o baja.                                 |
| Confianza       | Grado de certeza de que el problema existe: alta, media o baja.                 |
| Contexto        | Requisitos, lenguaje, framework y arquitectura que condicionan la evaluación.   |
| Consecuencia    | Qué problema concreto puede provocar.                                           |
| Acción sugerida | Corrección o refactorización que podría resolverlo.                             |
| Verificación    | Pruebas, análisis o comprobaciones para confirmar la solución.                  |

## Reglas generales de revisión

| Regla                               | Descripción                                                                                                           |
|:------------------------------------|:----------------------------------------------------------------------------------------------------------------------|
| No inventar errores                 | Cada hallazgo debe estar respaldado por evidencia del código o de su comportamiento.                                  |
| Distinguir defectos de preferencias | Una alternativa de estilo no convierte automáticamente al código existente en incorrecto.                             |
| Respetar el contexto                | Evaluar las reglas según el lenguaje, el framework y los requisitos del proyecto.                                     |
| Evitar cambios innecesarios         | Cada refactorización debe resolver un problema concreto y aportar una mejora justificable.                            |
| Preservar el comportamiento         | Una refactorización debe mantener el comportamiento observable, salvo autorización explícita para corregir un bug.    |
| Verificar los cambios               | Ejecutar las pruebas existentes y agregar pruebas cuando sea necesario.                                               |
| No asumir contratos                 | Si falta código relacionado o documentación, señalar qué información hace falta antes de afirmar que existe un error. |
