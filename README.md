# Proyecto integrador final

Repositorio de un proyecto académico de juego de cartas para dos jugadores, previsto como aplicación de consola en C# y .NET 8. Reúne los requisitos, el diseño y la base de código para organizar el trabajo del equipo y su futura implementación.

El juego consiste en enfrentar personajes comparando sus atributos. Cada jugador recibe tres cartas por ronda; gana la ronda quien consigue dos combates y la partida quien alcanza 15 rondas ganadas. Las reglas completas están en [Reglas del juego](docs/reglas-del-juego.md).

## Estado actual

El alcance es **TI (análisis y diseño)**. Las actividades 1 a 5 están cerradas y las actividades 6 y 7 contienen el diseño REST y los casos de prueba. La implementación de red y su validación entre dos computadoras corresponden al trabajo pendiente del [plan](docs/plan.md).

La base de código incluye Personaje, Mazo, Jugador, Combate, Ronda y Partida, con pruebas de dominio en xUnit. La interfaz de consola está implementada en `feature/interfaz-consola`. La conexión a una partida continúa pendiente.

## Contenido del repositorio

| Ubicación | Contenido |
|---|---|
| [`src/`](src/) | Proyectos `cliente`, `servidor` y `subir-personajes`. |
| [`test/`](test/) | Proyecto de pruebas automatizadas. |
| [`docs/`](docs/) | Requisitos, reglas, planificación y diseño. |
| [`mis-librerias/`](mis-librerias/) | Submódulo de bibliotecas auxiliares independientes, con documentación propia. |

## Documentación

| Tema | Documentos |
|---|---|
| Qué debe hacer el juego | [Consigna](docs/consigna.md) y [reglas](docs/reglas-del-juego.md) |
| Entregables y pendientes | [Plan de trabajo](docs/plan.md) |
| Dominio y catálogo de personajes | [Arquitectura](docs/arquitectura.md) y [diagrama UML](docs/diagramas/uml.uxf) |
| Comunicación entre equipos | [Protocolo HTTP](docs/protocolo-partida-web.md) y [tecnologías REST](docs/tecnologias-rest.md) |
| Pantallas y controles | [Diseño de consola](docs/interfaz.md) y [maquetas](docs/diagramas/juego-guerra-propuestas.html) |
| Validación y colaboración | [Plan de pruebas](docs/posibles-fallos.md) y [guía de colaboración](CONTRIBUTING.md) |

## Instalación de herramientas y dependencias

Esta guía prepara una computadora para compilar y ejecutar la base actual. Está orientada a Windows, el entorno previsto para jugar. Para consultar solamente la documentación no hace falta instalar estas herramientas.

### 1. Instalar Git y el SDK de .NET 8 desde PowerShell

Abrí **PowerShell** y ejecutá estos comandos uno por uno. WinGet es la herramienta de Windows que descarga e instala programas; aceptá la solicitud de permisos si aparece.

```powershell
winget --version
winget install --id Git.Git --exact --source winget --accept-source-agreements --accept-package-agreements
winget install --id Microsoft.DotNet.SDK.8 --exact --source winget --accept-source-agreements --accept-package-agreements
```

Si `winget` no se reconoce, abrí la página de **Instalador de aplicación** en Microsoft Store con este comando, instalalo o actualizalo y volvé a abrir PowerShell antes de repetir los comandos:

```powershell
Start-Process "ms-windows-store://pdp/?ProductId=9NBLGGH4NNS1"
```

Los identificadores de instalación corresponden a las guías oficiales de [Git para Windows](https://git-scm.com/install/windows) y [.NET en Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows). El SDK incluye las herramientas para compilar y los componentes para ejecutar .NET y ASP.NET Core. No hace falta instalar los runtimes por separado para desarrollar.

### 2. Abrir una nueva ventana de PowerShell y verificar

Cerrá PowerShell y abrilo otra vez para que reconozca los programas instalados. Ejecutá:

```powershell
git --version
dotnet --list-sdks
dotnet --list-runtimes
```

Debe aparecer una versión de Git, un SDK `8.0.xxx` y los runtimes `Microsoft.NETCore.App 8.0.x` y `Microsoft.AspNetCore.App 8.0.x`. El proyecto mantiene `net8.0` por decisión del equipo. Los números de versión del SDK y del runtime son distintos; pueden coexistir con otras versiones.

### 3. Descargar el proyecto con sus bibliotecas

En la nueva ventana de PowerShell, estos comandos crean una carpeta `proyectos` dentro de tu carpeta personal y descargan el repositorio allí. Si ya lo descargaste, usá la alternativa de abajo:

```powershell
New-Item -ItemType Directory -Path "$env:USERPROFILE\proyectos" -Force | Out-Null
Set-Location "$env:USERPROFILE\proyectos"
git clone --recurse-submodules https://github.com/51215927-spec/proyecto-integrador-final.git
Set-Location .\proyecto-integrador-final
```

Si ya habías descargado el repositorio, ejecutá lo siguiente reemplazando la ruta del ejemplo por la ubicación real de tu copia:

```powershell
Set-Location "C:\ruta\a\proyecto-integrador-final"
git submodule update --init --recursive
```

Esto recupera DataBases y Utilidades en `mis-librerias/`, usando la versión registrada por el proyecto. Son bibliotecas del repositorio; no se instalan mediante NuGet.

### 4. Restaurar los paquetes

Desde la carpeta que contiene `proyecto-integrador-final.sln`, ejecutá:

```powershell
dotnet restore proyecto-integrador-final.sln
```

Este comando descarga las versiones de los paquetes declaradas en los archivos de proyecto, incluyendo xUnit y las herramientas de pruebas. Necesitás conexión a Internet si todavía no están descargados. No hace falta instalar esos paquetes uno por uno. Consultá la [documentación de dotnet restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore) si falla la descarga.

### 5. Comprobar que la base funciona

Ejecutá estos comandos uno después del otro:

```powershell
dotnet build proyecto-integrador-final.sln
dotnet test proyecto-integrador-final.sln
dotnet run --project src/cliente
```

Con `feature/interfaz-consola`, el programa muestra el panel de preparación de Juego Guerra. Usá `dotnet run --project src/cliente -- --demo` para recorrer las pantallas con una [partida simulada](docs/interfaz.md#demostración-local). Las pruebas automatizadas cubren la base de dominio y los métodos de presentación y selección de la consola.

Las dependencias y los comandos del futuro servidor están en [Tecnologías REST](docs/tecnologias-rest.md#instalación-prevista-en-taller).

## Autores

- [51215927-spec](https://github.com/51215927-spec)
- [50509098-JC](https://github.com/50509098-JC)
- [Chino18cm](https://github.com/Chino18cm)
