# Instalación y uso

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

### 3. Descargar el proyecto

En la nueva ventana de PowerShell, estos comandos crean una carpeta `proyectos` dentro de tu carpeta personal y descargan el repositorio allí. Si ya lo descargaste, abrí la carpeta de tu copia:

```powershell
New-Item -ItemType Directory -Path "$env:USERPROFILE\proyectos" -Force | Out-Null
Set-Location "$env:USERPROFILE\proyectos"
git clone https://github.com/51215927-spec/proyecto-integrador-final.git
Set-Location .\proyecto-integrador-final
```

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

El programa muestra el panel de preparación de Juego Guerra. Usá `dotnet run --project src/cliente -- --demo` para recorrer las pantallas con una [partida simulada](Diseno-de-interfaz.md#demostración-local). Las pruebas automatizadas cubren la base de dominio y los métodos de presentación y selección de la consola.

Las dependencias y los comandos del servidor están en [Tecnologías REST](Tecnologias-REST.md#preparación-del-servidor).

## Autores

- [51215927-spec](https://github.com/51215927-spec)
- [50509098-JC](https://github.com/50509098-JC)
- [Chino18cm](https://github.com/Chino18cm)

## Cargar personajes manualmente

Configurá `Catalogo__Credencial` con la misma credencial de catálogo en el entorno del servidor y en la terminal de carga. Con el servidor en ejecución, iniciá la consola desde la raíz del repositorio.

```sh
dotnet run --project src/subir-personajes
```

La consola usa `http://localhost:5000` por defecto. Si el servidor está en otra computadora, pasá su URL después de `--`. Indicá cuántas líneas de arte ASCII vas a ingresar. Tras validar y guardar el personaje, el servidor devuelve su ID. Cargá al menos seis personajes activos antes de iniciar una partida.

## Cómo iniciar una partida

Desde la raíz del repositorio, abrí PowerShell y ejecutá el servidor. Si vas a cargar personajes, configurá primero `Catalogo__Credencial` en esa terminal.

```powershell
dotnet run --project src/servidor/servidor.csproj
```

- Con el servidor en ejecución, usá otra terminal para [cargar personajes](#cargar-personajes-manualmente) hasta tener al menos seis activos en el catálogo.
- Escribí `crear` y pulsá Enter en la consola del servidor. Copiá el UUID que aparece junto a `Partida` y entregale a cada jugador la credencial de su plaza (`J1` o `J2`).
- Incorporá a ambos jugadores mediante `PUT /api/v1/partidas/{id}/participantes/me/sesion`, con la credencial de su plaza y un UUID de sesión distinto para cada jugador. Mantené vigentes las dos sesiones con la solicitud de presencia. [Contrato HTTP de la partida](Protocolo-de-partida.md#contrato-http)
- En la consola del servidor, escribí `iniciar <idPartida>` sustituyendo `<idPartida>` por el UUID que mostró `crear`. Si aparece «Partida iniciada.», el servidor repartió tres cartas a cada jugador y J1 puede elegir la primera carta y el atributo.

La conexión del cliente de consola al servidor sigue pendiente. Podés verificar el inicio mediante la API en una computadora o recorrer las pantallas con la demostración local. [Prueba del servidor en una sola computadora](Protocolo-de-partida.md#prueba-del-servidor-en-una-sola-computadora) · [Demostración local](Diseno-de-interfaz.md#demostración-local)
