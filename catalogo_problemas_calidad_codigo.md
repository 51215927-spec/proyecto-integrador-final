# Catálogo de problemas y errores de calidad del código

Este documento reúne problemas frecuentes que pueden aparecer en
proyectos de software. Está pensado como base para un estándar (SPEC) de
revisión y refactorización asistida por IA.

No todos los elementos son errores funcionales: algunos son problemas de
diseño, mantenibilidad, rendimiento o calidad. La presencia de un
indicador no demuestra por sí sola que exista un defecto; cada caso debe
evaluarse según el contexto, el lenguaje y los requisitos del proyecto.

## 1. Errores funcionales (Bugs)

Son defectos que hacen que el programa produzca resultados incorrectos o
no cumpla el comportamiento esperado.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  BUG-001                 Lógica incorrecta       Una condición o
                                                  algoritmo produce
                                                  resultados equivocados.

  BUG-002                 Condiciones incompletas Faltan casos en un
                                                  `if`, `switch` u otra
                                                  estructura condicional.

  BUG-003                 Errores de límites      Se accede a posiciones
                                                  fuera de los límites de
                                                  una colección.

  BUG-004                 Errores de              Se utilizan variables u
                          inicialización          objetos antes de
                                                  inicializarlos
                                                  correctamente.

  BUG-005                 Errores de estado       El programa termina en
                                                  un estado inválido o
                                                  inconsistente.

  BUG-006                 Errores de cálculo      Hay operaciones
                                                  matemáticas
                                                  incorrectas,
                                                  desbordamientos o
                                                  pérdida de precisión.

  BUG-007                 Errores de conversión   Las conversiones de
                                                  tipos son incorrectas o
                                                  pierden información
                                                  inesperadamente.

  BUG-008                 Casos extremos          No se contemplan
                          ignorados               valores nulos, vacíos,
                                                  negativos, máximos u
                                                  otras entradas límite.

  BUG-009                 Errores de orden        Las operaciones se
                                                  ejecutan en un orden
                                                  incorrecto.

  BUG-010                 Errores de              Varias operaciones
                          sincronización          producen resultados
                                                  inconsistentes por su
                                                  orden de ejecución.

  BUG-011                 Efectos secundarios     Una operación modifica
                          inesperados             información que no
                                                  debería modificar.

  BUG-012                 Errores de concurrencia Se producen condiciones
                                                  de carrera, deadlocks o
                                                  accesos concurrentes
                                                  inseguros.
  -----------------------------------------------------------------------

## 2. Problemas de arquitectura y diseño

Son problemas relacionados con la organización del programa y las
responsabilidades de sus componentes.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  DES-001                 Violación de            Una clase o función
                          responsabilidad única   tiene demasiadas
                                                  responsabilidades
                                                  independientes.

  DES-002                 Acoplamiento excesivo   Los componentes
                                                  dependen demasiado unos
                                                  de otros.

  DES-003                 Baja cohesión           Una clase agrupa
                                                  elementos que no
                                                  comparten una
                                                  responsabilidad clara.

  DES-004                 Duplicación de lógica   El mismo comportamiento
                                                  está implementado en
                                                  varios lugares.

  DES-005                 Abstracciones           Se crean interfaces,
                          innecesarias            clases o capas que no
                                                  aportan valor.

  DES-006                 Falta de abstracciones  Se repite lógica que
                                                  podría encapsularse en
                                                  un componente común.

  DES-007                 Dependencias ocultas    Una clase depende de
                                                  servicios o estados que
                                                  no están declarados
                                                  claramente.

  DES-008                 Responsabilidades mal   Una operación está
                          ubicadas                implementada en un
                                                  componente que no
                                                  debería encargarse de
                                                  ella.

  DES-009                 Jerarquías incorrectas  La herencia representa
                                                  relaciones que no
                                                  corresponden al dominio
                                                  o al diseño.

  DES-010                 Violación de            Se expone información
                          encapsulamiento         interna que debería
                                                  permanecer protegida.

  DES-011                 Inversión de            Los componentes de alto
                          dependencias incorrecta nivel dependen
                                                  directamente de
                                                  implementaciones
                                                  concretas.

  DES-012                 Complejidad accidental  La solución tiene una
                                                  estructura más
                                                  complicada de lo
                                                  necesario.

  DES-013                 Máquina de estados      El estado del sistema
                          implícita               está distribuido entre
                                                  múltiples variables y
                                                  condiciones.

  DES-014                 Dependencias circulares Dos o más componentes
                                                  dependen mutuamente
                                                  entre sí.
  -----------------------------------------------------------------------

## 3. Problemas de legibilidad y mantenibilidad

Dificultan comprender, modificar, depurar y extender el código.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  LEG-001                 Nombres poco            Variables, métodos o
                          descriptivos            clases tienen nombres
                                                  ambiguos.

  LEG-002                 Métodos demasiado       Una función realiza
                          largos                  demasiadas operaciones
                                                  o responsabilidades.

  LEG-003                 Clases demasiado        Una clase concentra
                          grandes                 demasiada lógica o
                                                  estado.

  LEG-004                 Anidamiento excesivo    La cantidad de bloques
                                                  y condiciones anidadas
                                                  dificulta seguir el
                                                  flujo.

  LEG-005                 Comentarios redundantes Los comentarios
                                                  explican algo que ya
                                                  resulta evidente en el
                                                  código.

  LEG-006                 Comentarios             Los comentarios
                          desactualizados         contradicen el
                                                  comportamiento actual.

  LEG-007                 Código muerto           Existen variables,
                                                  métodos o instrucciones
                                                  que no se utilizan o no
                                                  pueden ejecutarse.

  LEG-008                 Código duplicado        Hay fragmentos
                                                  similares que podrían
                                                  compartir una
                                                  implementación.

  LEG-009                 Números mágicos         Se utilizan valores
                                                  literales sin expresar
                                                  su significado.

  LEG-010                 Complejidad ciclomática Una función contiene
                          elevada                 demasiados caminos de
                                                  ejecución.

  LEG-011                 Complejidad cognitiva   Resulta difícil
                          elevada                 comprender el flujo de
                                                  una función o bloque.

  LEG-012                 Formato inconsistente   El estilo de escritura
                                                  cambia sin una
                                                  convención clara.

  LEG-013                 Parámetros excesivos    Un método recibe
                                                  demasiados argumentos o
                                                  datos relacionados que
                                                  podrían agruparse.

  LEG-014                 Variables temporales    Se crean variables que
                          innecesarias            no aportan claridad ni
                                                  funcionalidad.

  LEG-015                 Abreviaciones ambiguas  Se utilizan
                                                  abreviaciones difíciles
                                                  de interpretar.
  -----------------------------------------------------------------------

## 4. Problemas de tipado y manejo de datos

Se relacionan con cómo se representan, validan y modifican los datos.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  TYP-001                 Tipos demasiado         Se utilizan tipos como
                          genéricos               `object` o `any` sin
                                                  una necesidad
                                                  justificada.

  TYP-002                 Conversiones inseguras  Se realizan
                                                  conversiones sin
                                                  comprobar que sean
                                                  válidas.

  TYP-003                 Uso incorrecto de null  No se contemplan
                                                  valores nulos o se
                                                  utilizan de forma
                                                  incorrecta.

  TYP-004                 Tipos primitivos mal    Se representan
                          utilizados              conceptos del dominio
                                                  mediante strings,
                                                  números o booleanos sin
                                                  encapsular sus reglas.

  TYP-005                 Estado inválido         El diseño permite
                          representable           construir objetos que
                                                  deberían ser imposibles
                                                  o inválidos.

  TYP-006                 Mutabilidad innecesaria Los datos pueden
                                                  modificarse cuando no
                                                  deberían o no necesitan
                                                  hacerlo.

  TYP-007                 Colecciones incorrectas Se utiliza una
                                                  estructura de datos
                                                  poco apropiada para las
                                                  operaciones requeridas.

  TYP-008                 Falta de validación     Se aceptan datos que no
                                                  cumplen las reglas del
                                                  sistema.

  TYP-009                 Inconsistencia de datos Distintas partes del
                                                  programa mantienen
                                                  representaciones
                                                  diferentes de la misma
                                                  información.

  TYP-010                 Pérdida de precisión    Se utilizan tipos
                                                  numéricos que no
                                                  ofrecen la precisión
                                                  necesaria.
  -----------------------------------------------------------------------

## 5. Manejo de errores y excepciones

Evalúa cómo el programa detecta, comunica y recupera los fallos.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  ERR-001                 Excepciones ignoradas   Se captura una
                                                  excepción sin hacer
                                                  nada con ella ni
                                                  comunicarla.

  ERR-002                 Captura demasiado       Se captura cualquier
                          general                 excepción sin
                                                  distinguir su origen o
                                                  tratamiento.

  ERR-003                 Excepciones mal         Se utilizan excepciones
                          utilizadas              para controlar el flujo
                                                  normal del programa.

  ERR-004                 Falta de manejo de      Una operación puede
                          errores                 fallar sin una
                                                  estrategia definida
                                                  para tratar el fallo.

  ERR-005                 Errores ocultos         Se informa que una
                                                  operación tuvo éxito
                                                  cuando realmente falló.

  ERR-006                 Pérdida de contexto     Se reemplaza una
                                                  excepción sin conservar
                                                  información relevante
                                                  de la original.

  ERR-007                 Mensajes poco           Los errores no explican
                          descriptivos            qué sucedió ni aportan
                                                  información útil.

  ERR-008                 Recuperación incorrecta El programa continúa
                                                  después de un fallo y
                                                  deja el sistema
                                                  inconsistente.

  ERR-009                 Validación tardía       Un error se detecta
                                                  mucho después del punto
                                                  en el que podría
                                                  haberse evitado.

  ERR-010                 Errores no propagados   Una función oculta un
                                                  error que debería
                                                  comunicar a quien la
                                                  llamó.
  -----------------------------------------------------------------------

## 6. Rendimiento y uso de recursos

Busca operaciones innecesariamente costosas y uso inadecuado de memoria,
CPU, red o almacenamiento.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  PER-001                 Algoritmos ineficientes Se utilizan algoritmos
                                                  con una complejidad
                                                  temporal o espacial
                                                  innecesariamente alta.

  PER-002                 Operaciones redundantes Se realizan cálculos o
                                                  tareas que podrían
                                                  evitarse o
                                                  reutilizarse.

  PER-003                 Asignaciones            Se crean objetos o
                          innecesarias            estructuras que no
                                                  hacen falta.

  PER-004                 Consultas repetidas     Se solicita varias
                                                  veces información que
                                                  podría reutilizarse.

  PER-005                 Estructuras de datos    Se utilizan colecciones
                          incorrectas             poco apropiadas para
                                                  las operaciones
                                                  realizadas.

  PER-006                 Bloqueos innecesarios   Se impide la ejecución
                                                  concurrente o paralela
                                                  cuando no es necesario.

  PER-007                 Recursos no liberados   Se mantienen archivos,
                                                  conexiones u otros
                                                  recursos abiertos más
                                                  tiempo del necesario.

  PER-008                 Fugas de memoria        La memoria ocupada no
                                                  puede recuperarse
                                                  cuando debería.

  PER-009                 Trabajo en el hilo      Se realizan operaciones
                          principal               costosas que bloquean
                                                  la interfaz o el
                                                  procesamiento
                                                  principal.

  PER-010                 Serialización           Se convierten datos a
                          innecesaria             formatos como JSON
                                                  repetidamente sin
                                                  necesidad.

  PER-011                 Uso inadecuado de caché Se recalculan datos
                                                  reutilizables o se
                                                  almacenan datos sin una
                                                  estrategia adecuada.

  PER-012                 Complejidad de          Se ejecutan consultas
                          consultas               de base de datos
                                                  innecesariamente
                                                  costosas.
  -----------------------------------------------------------------------

## 7. Concurrencia y asincronía

Agrupa problemas propios de la ejecución simultánea o asíncrona. Pueden
ser difíciles de reproducir porque dependen del orden y del momento en
que se ejecutan las operaciones.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  CON-001                 Race condition          El resultado depende
                                                  del orden en que se
                                                  ejecutan varias
                                                  operaciones
                                                  concurrentes.

  CON-002                 Deadlock                Dos o más operaciones
                                                  esperan indefinidamente
                                                  recursos que otras
                                                  mantienen bloqueados.

  CON-003                 Starvation              Una operación no
                                                  consigue los recursos
                                                  necesarios para
                                                  avanzar.

  CON-004                 Acceso concurrente      Varios hilos modifican
                          inseguro                información compartida
                                                  sin protección
                                                  adecuada.

  CON-005                 Sincronización          Se utilizan mecanismos
                          incorrecta              de sincronización de
                                                  forma equivocada.

  CON-006                 Tareas sin supervisión  Se inician tareas
                                                  asíncronas sin
                                                  controlar sus
                                                  resultados o
                                                  excepciones.

  CON-007                 Cancelación incorrecta  Las operaciones no
                                                  respetan las
                                                  solicitudes de
                                                  cancelación.

  CON-008                 Recursos compartidos    Se accede a recursos
                          sin protección          comunes de manera
                                                  insegura.

  CON-009                 Reintentos duplicados   Una operación se
                                                  ejecuta más de una vez
                                                  cuando no debería.

  CON-010                 Operaciones fuera de    Las respuestas llegan o
                          orden                   se aplican en un orden
                                                  diferente al esperado.

  CON-011                 Falta de idempotencia   Repetir una operación
                                                  produce efectos
                                                  adicionales no
                                                  deseados.
  -----------------------------------------------------------------------

## 8. Pruebas y calidad

Analiza si existen pruebas suficientes y si estas verifican el
comportamiento esperado.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  TST-001                 Falta de pruebas        No existen pruebas para
                                                  funcionalidades
                                                  importantes.

  TST-002                 Cobertura insuficiente  Hay partes relevantes
                                                  del código que no se
                                                  prueban.

  TST-003                 Casos extremos sin      No se prueban entradas
                          probar                  vacías, límites o
                                                  situaciones
                                                  excepcionales.

  TST-004                 Pruebas dependientes    Una prueba necesita que
                                                  otra se ejecute
                                                  primero.

  TST-005                 Pruebas frágiles        Cambios irrelevantes
                                                  rompen las pruebas.

  TST-006                 Pruebas que no          Las pruebas ejecutan
                          verifican resultados    código, pero no
                                                  comprueban el
                                                  comportamiento
                                                  esperado.

  TST-007                 Acoplamiento excesivo   Es difícil probar un
                          en pruebas              componente sin ejecutar
                                                  otros componentes
                                                  innecesarios.

  TST-008                 Falta de pruebas de     No se verifica la
                          integración             interacción entre
                                                  componentes.

  TST-009                 Falta de pruebas de     No se comprueba que las
                          regresión               modificaciones
                                                  mantengan el
                                                  comportamiento
                                                  anterior.

  TST-010                 Datos de prueba poco    Los casos de prueba no
                          representativos         reflejan situaciones
                                                  relevantes o realistas.
  -----------------------------------------------------------------------

## 9. Modularidad y reutilización

Evalúa cómo se dividen los componentes y cómo se comunican entre sí.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  MOD-001                 Código difícil de       La lógica está
                          reutilizar              demasiado ligada a un
                                                  contexto específico.

  MOD-002                 Dependencias            Un módulo conoce
                          innecesarias            componentes que no
                                                  necesita.

  MOD-003                 Interfaces demasiado    Una interfaz obliga a
                          grandes                 implementar operaciones
                                                  que no se utilizan.

  MOD-004                 Duplicación entre       Diferentes módulos
                          módulos                 implementan la misma
                                                  funcionalidad.

  MOD-005                 Límites mal definidos   No está claro qué
                                                  responsabilidad
                                                  pertenece a cada
                                                  módulo.

  MOD-006                 API pública excesiva    Se exponen operaciones
                                                  que deberían permanecer
                                                  internas.

  MOD-007                 Dependencias de         Los módulos dependen de
                          implementación          detalles internos en
                                                  lugar de contratos
                                                  estables.
  -----------------------------------------------------------------------

## 10. Configuración, infraestructura y persistencia

Revisa la relación entre el código y los elementos externos necesarios
para ejecutarlo, como configuración, archivos, bases de datos y
servicios.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  INF-001                 Configuración           Direcciones, rutas o
                          hardcodeada             parámetros están
                                                  escritos directamente
                                                  en el código.

  INF-002                 Configuración duplicada La misma configuración
                                                  aparece en varios
                                                  lugares.

  INF-003                 Configuración insegura  La configuración
                                                  contiene información
                                                  sensible o permisos
                                                  inadecuados.

  INF-004                 Falta de validación de  El programa inicia con
                          configuración           parámetros inválidos o
                                                  incompletos.

  INF-005                 Persistencia            Los datos guardados no
                          inconsistente           representan
                                                  correctamente el estado
                                                  del sistema.

  INF-006                 Transacciones           Una operación
                          incorrectas             parcialmente ejecutada
                                                  deja datos
                                                  inconsistentes.

  INF-007                 Falta de recuperación   No existe una
                                                  estrategia ante fallos
                                                  de almacenamiento o
                                                  conexión.

  INF-008                 Migraciones incorrectas Los cambios en el
                                                  esquema de datos no se
                                                  aplican de forma segura
                                                  o coherente.

  INF-009                 Acoplamiento a          La lógica del dominio
                          infraestructura         depende directamente de
                                                  detalles de bases de
                                                  datos, archivos o red.
  -----------------------------------------------------------------------

## 11. Documentación y convenciones

Comprueba si el proyecto explica cómo utilizarse y si mantiene
convenciones coherentes.

  -----------------------------------------------------------------------
  Código                  Problema                Descripción
  ----------------------- ----------------------- -----------------------
  DOC-001                 Falta de documentación  No se explica cómo
                                                  utilizar o configurar
                                                  el proyecto.

  DOC-002                 Documentación           Los documentos no
                          desactualizada          coinciden con el
                                                  comportamiento actual
                                                  del código.

  DOC-003                 Contratos poco claros   No se documentan las
                                                  entradas, salidas y
                                                  errores de una API o
                                                  componente.

  DOC-004                 Convenciones            Se mezclan estilos de
                          inconsistentes          nombres y organización
                                                  sin una razón clara.

  DOC-005                 Falta de instrucciones  No se puede reproducir
                          de instalación          fácilmente el entorno
                                                  de desarrollo o
                                                  ejecución.

  DOC-006                 Dependencias no         No se especifican las
                          documentadas            bibliotecas o
                                                  herramientas
                                                  necesarias.
  -----------------------------------------------------------------------

## Consideraciones de uso

-   **No todo indicador demuestra un error.** La IA debe analizar el
    contexto y justificar cada hallazgo.
-   **Distinguir defectos de preferencias.** Una forma de escribir
    código no es incorrecta solo porque exista otra alternativa.
-   **Respetar el lenguaje y el proyecto.** Algunas reglas dependen del
    lenguaje, del framework o de la arquitectura.
-   **Evitar refactorizaciones innecesarias.** Cada cambio debe resolver
    un problema concreto y aportar una mejora justificable.
-   **Preservar el comportamiento.** Las refactorizaciones deben
    mantener el comportamiento observable, salvo que se autorice
    explícitamente corregir un bug.
-   **Verificar los cambios.** Siempre que sea posible, ejecutar las
    pruebas existentes y agregar pruebas para los casos relevantes.

Este catálogo es una base inicial para un SPEC de revisión y
refactorización. Cada regla puede ampliarse posteriormente con
indicadores más precisos, ejemplos, criterios de severidad, riesgos y
métodos de verificación.
