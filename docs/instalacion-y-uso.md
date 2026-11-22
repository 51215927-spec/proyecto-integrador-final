# Instalación y uso

## Entrada de datos en ejecución

El servidor y el cliente solicitan su configuración por consola. La herramienta de carga acepta una ruta JSON como argumento opcional; si falta, la solicita. No se necesitan variables de entorno de usuario. La configuración de conexión permite corregir campos; un error de archivo o carga finaliza sin reintento automático. El fin de entrada cancela las acciones incompletas. Las credenciales ingresadas no se muestran en una terminal interactiva ni en los diagnósticos. Con entrada redirigida se leen por líneas.

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

El cliente solicita URL base, UUID de partida, credencial de plaza y nombre. Necesitás una partida creada en el servidor para jugar. Las pruebas incluyen transporte HTTP, coordinación de consola y servidor local con SQLite temporal. Para incluir los contratos que arrancan ejecutables, compilá primero y configurá las tres rutas `GUERRA_*_DLL` como indica el [README de pruebas](../test/proyecto-integrador-final-tests/README.md). Sin esas rutas, los casos condicionales se omiten; un resultado verde no implica que hayan corrido todos.

Las dependencias y los comandos del servidor están en [API y operación del servidor](protocolo-de-partida.md#operación-del-servidor).

## Cargar personajes desde JSON

Con el servidor en ejecución y una credencial de catálogo no vacía, ejecutá desde la raíz:

```sh
dotnet run --project src/subir-personajes -- "src/subir-personajes/ejemplos/personajes.json"
```

Podés indicar otro archivo de personajes reales u omitir el argumento para que se solicite la ruta. Luego se piden URL y credencial. La herramienta muestra ambas cantidades y pide confirmación antes de **reemplazar todo el catálogo** con el JSON. Cargá al menos seis personajes válidos y distintos para jugar. No se cargan ejemplos automáticamente. Consultá el [formato, validaciones, cancelación y errores](../src/subir-personajes/README.md).

## Cómo iniciar una partida

Desde la raíz del repositorio, abrí PowerShell y ejecutá el servidor.

```powershell
dotnet run --project src/servidor/servidor.csproj
```

- Respondé las preguntas de inicio: dirección de escucha (Enter: `http://0.0.0.0:5000`), archivo SQLite (Enter: `guerra.db`) y credencial de catálogo. Una credencial vacía deshabilita las altas y se informa en pantalla. La ruta relativa de la base se interpreta desde el directorio de ejecución; su directorio debe existir y permitir escritura.
- Con el servidor en ejecución, usá otra terminal para [cargar personajes](#cargar-personajes-desde-json) con un JSON de al menos seis personajes en el catálogo.
- Escribí `crear` y pulsá Enter en la consola del servidor. Copiá el UUID que aparece junto a `Partida` y entregale a cada jugador la credencial de su plaza (`J1` o `J2`).
- En otras dos terminales, ejecutá `dotnet run --project src/cliente`. Cada jugador ingresa la URL del servidor, el UUID de partida, su credencial de plaza y su nombre. Ambos esperan el inicio; las sesiones se renuevan automáticamente.
- En la consola del servidor, escribí `iniciar`, pulsá Enter y respondé con el UUID que mostró `crear`. Si aparece «Partida iniciada.», el servidor repartió tres cartas a cada jugador y J1 puede elegir la primera carta y el atributo.

La comprobación entre dos computadoras corresponde al ensayo de red pendiente. Para localhost, usá `http://127.0.0.1:5000` en ambos clientes. Desde otra computadora, usá la IP del servidor. [Conexión en red local](protocolo-de-partida.md#conexión-en-red-local)

## Recorrer una partida

Elegí la carta con flechas, WASD o HJKL y confirmá con Enter. Al abrir, elegí también un atributo; al responder, elegí solo carta. El servidor confirma la recepción y resuelve el combate. Enter en un resultado confirma su lectura local y muestra el siguiente resultado pendiente o el estado actual. No envía una elección del turno siguiente. Fuera de selecciones, resultados y menús, Enter vuelve a mostrar el estado.

Esc abre el menú: Continuar, Salir sin abandonar y Abandonar partida. La confirmación de abandono comienza en No. Ctrl+C o fin de entrada salen sin abandonar. Una salida local no otorga una victoria; la plaza puede continuar ocupada hasta quince segundos o hasta que el operador cierre la sesión.

Ante una interrupción de red, esperá la reconexión automática o salí desde Esc. Mientras se recupera, se conserva el último estado confirmado y se bloquean nuevas jugadas. Una elección incierta se comprueba por su recibo antes de reenviar. Si el abandono no pudo confirmarse, el cliente lo advierte al salir.

Para retomar tras cerrar, volvé a ejecutar el cliente e ingresá los mismos datos y el nombre exacto. El proceso crea una sesión nueva y puede tener que esperar la liberación de la plaza. No guarda datos locales ni reproduce resultados anteriores; muestra el estado persistido por el servidor.

## Problemas frecuentes

- Si falla la restauración, comprobá el SDK con `dotnet --list-sdks` y la conexión a los orígenes NuGet.
- Si falla la conexión, verificá que el servidor informe la escucha y que coincidan la IP y el puerto. En red local, comprobá que ambos equipos estén en la misma red y que el puerto configurado permita conexiones entrantes.
- Si el alta devuelve 401, comprobá que la credencial de catálogo coincida con la ingresada al iniciar el servidor.
- Si la sesión vence, el cliente se reincorpora automáticamente. Si indica sesión ocupada, esperá su vencimiento o pedí al operador que use `cerrar-sesion`.
- Si rechaza credencial, nombre o partida, Enter permite volver a ingresar los datos. Si cambiaste el destino o plaza, una operación anterior puede haberse aplicado: su recuperación requiere los datos originales.

## Bases existentes tras la separación del servidor

El spec 006 conserva el esquema SQLite: usá el archivo habitual y las credenciales de las plazas existentes. El servidor reconstruye el dominio desde las filas guardadas y conserva recibos, manos y solicitudes. Al reiniciar, las sesiones temporales dejan de estar vigentes y ambos clientes deben reincorporarse. La compatibilidad automatizada se comprobó con una base creada por la versión anterior; no hace falta una migración para este cambio. [Referencia y límites de validación](plan-de-pruebas.md#separación-de-responsabilidades-spec-006).
