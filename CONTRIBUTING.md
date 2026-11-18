# Reglas para la colaboración del proyecto

## Flujo de trabajo con Git

### Preparar la copia

Seguí la [instalación](README.md#instalación-de-herramientas-y-dependencias) para obtener el repositorio y sus bibliotecas. Configurá tu nombre y correo si todavía no están definidos en Git.

```bash
git config user.name "Tu nombre"
git config user.email "tu-correo@example.com"
```

### Empezar una tarea

Comprobá los cambios locales y los worktrees antes de actualizar la base.

```bash
git status --short --branch
git worktree list
git fetch origin
git switch main
git merge --ff-only origin/main
```

Trabajá en la rama asignada a la tarea. Para tareas nuevas creá una rama desde `main` actualizado, por ejemplo con `git switch -c docs/actualizar-arquitectura`. Si hay cambios locales o divergencias, conservalos y revisalos antes de sincronizar. Los cambios documentales van en ramas `docs/`, en commits exclusivamente documentales.

| Tipo de trabajo | Rama de ejemplo | Prefijo del commit |
|---|---|---|
| Funcionalidad | `feature/sistema-de-combate` | `feat` |
| Corrección | `fix/validacion-de-atributos` | `fix` |
| Documentación | `docs/actualizar-arquitectura` | `docs` |
| Refactorización | `refactor/organizacion-de-clases` | `refactor` |
| Pruebas | `test/pruebas-de-personajes` | `test` |

Usá una descripción en minúsculas y sin tildes. Separá las palabras de la rama con guiones. Escribí los commits con el formato `tipo: descripcion breve`, sin punto final.

### Preparar y compartir cambios

Comprobá el diff, ejecutá las verificaciones pertinentes y agregá únicamente los archivos de la tarea. Por ejemplo, para un cambio en el plan:

```bash
git diff
git add docs/plan.md
git diff --cached
git commit -m "docs: actualizar plan de trabajo"
git push -u origin HEAD
```

### Integrar con un Pull Request

En GitHub, abrí **Pull requests → New pull request**. Seleccioná `main` como base y tu rama para comparar. Comprobá el diff antes de crear el PR. En la descripción indicá el propósito, los cambios y las verificaciones realizadas. Antes de integrar, obtené la revisión de un compañero y resolvé las observaciones en la misma rama. Los commits que publiques en esa rama se agregan al PR existente. Integrá cuando el compañero apruebe y estén resueltos los conflictos y las comprobaciones pendientes.

### Sincronizar después de integrar

Actualizá `main` con `git fetch origin` y `git merge --ff-only origin/main` desde esa rama. Incorporá el nuevo `main` mediante merge en las ramas vigentes y actualizá las que ya estén publicadas. Si aparecen conflictos, revisá ambos cambios y coordiná la resolución con el compañero. Aplicá el [procedimiento de worktrees](AGENTS.md#trabajo-en-paralelo-con-worktrees) para conservar el trabajo y limpiar las ramas de tareas cerradas.

## Desarrollo guiado por pruebas (TDD)

TDD (*Test-Driven Development*) consiste en escribir una prueba antes de implementar el comportamiento que verifica. Se aplicará con xUnit cuando se autorice avanzar con Taller de Programación. Durante TI se diseñan los casos y sus resultados esperados; esta guía no adelanta la implementación ni da por completado el plan de pruebas de la actividad 7.

### Preparar la estructura

Respetá los contratos de [Arquitectura](docs/arquitectura.md). Creá las clases, atributos, constructores y firmas necesarios antes de implementar la lógica. Los métodos pendientes deben lanzar `NotImplementedException`, sin lógica de producción. Esta estructura no reemplaza la escritura previa de pruebas para cada comportamiento.

### Ciclo rojo–verde–refactorización

Trabajá con un comportamiento pequeño por vez:

1. **Rojo (RED):** escribí una prueba en `test/` con los datos iniciales, la acción y el resultado esperado. Ejecutala y comprobá que falle por el comportamiento todavía no satisfecho. Corregí antes cualquier problema de configuración que impida comprobarlo.
2. **Verde (GREEN):** implementá en `src/` únicamente la lógica necesaria para satisfacer la prueba. Ejecutá las pruebas y comprobá que también sigan pasando las anteriores.
3. **Refactorización (REFACTOR):** mejorá los nombres, la organización y las duplicaciones sin cambiar el comportamiento. Volvé a ejecutar las pruebas después de cada cambio.

Repetí el ciclo para el siguiente caso. Usá `[Fact]` para un escenario individual y `[Theory]` con `[InlineData]` cuando varios conjuntos de datos comprueben la misma regla.

### Ejemplo: validación de personajes

Una distribución válida es `750, 550, 500, 250, 500, 550`: suma 3000, todos sus valores están entre 250 y 750, alguno alcanza 650 y alguno no supera 350.

Agregá pruebas para aceptar esa distribución y rechazar, por separado, un total distinto de 3000, valores fuera del rango y distribuciones que incumplan cada umbral de especialización. Incluí los límites 250, 750, 650 y 350. Para cada caso, repetí el ciclo antes de avanzar al siguiente y verificá el resultado o la excepción especificados en el diseño.

### Mantener y verificar las pruebas

- Escribí pruebas independientes, con nombres que expliquen el escenario y el resultado esperado, sin depender del orden de ejecución.
- Ante un error, agregá primero una prueba que lo reproduzca y después corregí la implementación.
- Conservá las pruebas anteriores. No las elimines, desactives ni cambies solo para conseguir que pasen; si cambió un requisito o la prueba era incorrecta, justificá el ajuste.
- Usá mocks solo cuando haga falta aislar dependencias externas o colaboradoras. La cobertura por sí sola no demuestra la calidad de las pruebas.
- Antes de finalizar una tarea de desarrollo, compilá y ejecutá la suite completa:

```bash
dotnet build proyecto-integrador-final.sln
dotnet test proyecto-integrador-final.sln
```

No des por terminado el desarrollo con pruebas fallidas ni reemplaces las pruebas automatizadas por comprobaciones manuales. En el Pull Request, indicá los comportamientos verificados y los resultados de los comandos para que un compañero pueda revisarlos.

## Estructura del proyecto

### ¿Dónde guardar cada archivo?

| Ubicación | Contenido |
|---|---|
| `src/` | Código fuente de la aplicación. |
| `test/` | Proyectos de pruebas automatizadas. |
| `docs/` | Documentación del proyecto, consignas, arquitectura, planificación y guías. |
| `CONTRIBUTING.md` | Guía de colaboración de este repositorio. |
| `AGENTS.md` | Instrucciones de trabajo para agentes. |
| `.gitignore` | Archivos y carpetas que Git debe ignorar. |
| `.sln` | Solución de Visual Studio que agrupa los proyectos. |
| `README.md` | Presentación del proyecto e índice de documentación. |

### Reglas para organizar los archivos

- Guardar el código de la aplicación dentro de `src/`, no directamente en la raíz del repositorio.
- Guardar las pruebas dentro de `test/`.
- Guardar la documentación del proyecto en `docs/`. En la raíz se mantienen `README.md`, `CONTRIBUTING.md` y `AGENTS.md`.
- No subir carpetas generadas automáticamente, como `bin/` y `obj/`.
- Si se agrega un nuevo proyecto de código, ubicarlo dentro de `src/` o `test/`, según corresponda.
- Si se agrega documentación, incluir un enlace a ella en el `README.md`.
