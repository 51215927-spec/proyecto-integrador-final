# Reglas para la colaboración del proyecto

## Flujo de trabajo con Git

### Preparar la copia

Seguí la [instalación](docs/wiki/Instalacion-y-uso.md#instalación-de-herramientas-y-dependencias) para obtener el repositorio y sus bibliotecas. Configurá tu nombre y correo si todavía no están definidos en Git.

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

## Desarrollo y estructura

- [Desarrollo con TDD](docs/wiki/Desarrollo-con-TDD.md).
- [Estructura del repositorio](docs/wiki/Estructura-del-repositorio.md).
