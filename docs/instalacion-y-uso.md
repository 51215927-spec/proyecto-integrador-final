# Instalación y uso

## Entrada de datos en ejecución

El servidor solicita su configuración por consola. El cliente ofrece Jugar y recuerda su conexión y acceso por servidor. La herramienta de carga acepta una ruta JSON como argumento opcional; si falta, la solicita. No se necesitan variables de entorno de usuario. La configuración de conexión permite corregir campos; un error de archivo o carga finaliza sin reintento automático. El fin de entrada cancela las acciones incompletas. Las credenciales ingresadas no se muestran en una terminal interactiva ni en los diagnósticos. Con entrada redirigida se leen por líneas.

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

El cliente ofrece Jugar, Crear perfil, Conexión y Salir. No pide UUID ni credenciales en el recorrido normal. Las pruebas incluyen transporte HTTP, coordinación de consola y SQLite temporal. Para ejecutar también los contratos de procesos, configurá las tres rutas `GUERRA_*_DLL` del mismo worktree según el [README de pruebas](../test/proyecto-integrador-final-tests/README.md).


Las dependencias y los comandos del servidor están en [API y operación del servidor](protocolo-de-partida.md#operación-del-servidor).

## Cargar personajes desde JSON

Con el servidor en ejecución y una credencial de catálogo no vacía, ejecutá desde la raíz:

```sh
dotnet run --project src/subir-personajes -- "src/subir-personajes/ejemplos/personajes.json"
```

Podés indicar otro archivo de personajes reales u omitir el argumento para que se solicite la ruta. Luego se piden URL y credencial. La herramienta muestra ambas cantidades y pide confirmación antes de **reemplazar todo el catálogo** con el JSON. Cargá al menos seis personajes válidos y distintos para jugar. Al iniciar, el servidor carga estos seis personajes de ejemplo únicamente si el catálogo está vacío. Consultá el [formato, validaciones, cancelación y errores](../src/subir-personajes/README.md).

## Cómo iniciar una partida

Desde la raíz del repositorio, abrí PowerShell y ejecutá el servidor.

```powershell
dotnet run --project src/servidor/servidor.csproj
```

- Respondé las preguntas de inicio: dirección de escucha (Enter: `http://0.0.0.0:5000`), archivo SQLite (Enter: `guerra.db`) y credencial de catálogo. Una credencial vacía deshabilita las altas y se informa en pantalla. La ruta relativa de la base se interpreta desde el directorio de ejecución; su directorio debe existir y permitir escritura.
- Con el servidor en ejecución, usá otra terminal para [cargar personajes](#cargar-personajes-desde-json) con un JSON de al menos seis personajes en el catálogo.
- Abrí dos terminales del mismo usuario y ejecutá `dotnet run --project src/cliente` en cada una. En la primera elegí Jugar y creá tu primer perfil. En la segunda, elegí Crear perfil e ingresá otro nombre; también podés elegir Jugar y crear otro cuando informe que todos están en uso. Cada perfil conserva sus propias partidas y credenciales.
- Usá Conexión para recordar otra dirección si corresponde. Elegí Jugar, omití o escribí el nombre y esperá rival. Escribí Listo y Enter en cada cliente. Con catálogo suficiente el servidor inicia y reparte sin comandos administrativos.
- Si faltan seis personajes activos, ambos ven el aviso y pueden cancelar o reintentar con Listo cuando se complete el catálogo.

La comprobación entre dos computadoras corresponde al ensayo de red pendiente. Para localhost, usá `http://127.0.0.1:5000` en ambos clientes. Desde otra computadora, usá la IP del servidor. [Conexión en red local](protocolo-de-partida.md#conexión-en-red-local)

## Recorrer una partida

Elegí la carta con flechas, WASD o HJKL y confirmá con Enter. Al abrir, elegí también un atributo; al responder, elegí solo carta. El servidor confirma la recepción y resuelve el combate. Enter en un resultado confirma su lectura local y muestra el siguiente resultado pendiente o el estado actual. No envía una elección del turno siguiente. Fuera de selecciones, resultados y menús, Enter vuelve a mostrar el estado.

Esc abre el menú: Continuar, Salir sin abandonar y Abandonar partida. La confirmación de abandono comienza en No. Ctrl+C o fin de entrada salen sin abandonar. Una salida local no otorga una victoria; la plaza puede continuar ocupada hasta quince segundos o hasta que el operador cierre la sesión.

Ante una interrupción de red, esperá la reconexión automática o salí desde Esc. Mientras se recupera, se conserva el último estado confirmado y se bloquean nuevas jugadas. Una elección incierta se comprueba por su recibo antes de reenviar. Si el abandono no pudo confirmarse, el cliente lo advierte al salir.

Para retomar, abrí el cliente en el mismo usuario del sistema y elegí Jugar. Si queda un solo perfil libre lo usa automáticamente; si hay varios libres, elegí el correspondiente por número. Después elegí Retomar. El acceso guardado localiza la partida activa en ese servidor. Una sesión anterior puede seguir ocupada hasta quince segundos. El cliente consulta el estado persistido; no reconstruye salas antiguas ni busca otra partida automáticamente. [Almacenamiento y errores de acceso](../src/cliente/README.md).

## Problemas frecuentes

- Si falla la restauración, comprobá el SDK con `dotnet --list-sdks` y la conexión a los orígenes NuGet.
- Si falla la conexión, verificá que el servidor informe la escucha y que coincidan la IP y el puerto. En red local, comprobá que ambos equipos estén en la misma red y que el puerto configurado permita conexiones entrantes.
- Si el alta devuelve 401, comprobá que la credencial de catálogo coincida con la ingresada al iniciar el servidor.
- Si la sesión vence, el cliente se reincorpora automáticamente. Si indica sesión ocupada, esperá su vencimiento o pedí al operador que use `cerrar-sesion`.
- Si el servidor rechaza la identidad, el cliente ofrece crear otra mediante confirmación explícita NUEVO. La identidad nueva no recupera partidas anteriores. Si no puede leer o guardar los archivos locales, ofrece reintentar o salir sin reemplazarlos.

## Bases existentes tras la separación del servidor

El spec 006 conserva el esquema SQLite: usá el archivo habitual y las credenciales de las plazas existentes. El servidor reconstruye el dominio desde las filas guardadas y conserva recibos, manos y solicitudes. Al reiniciar, las sesiones temporales dejan de estar vigentes y ambos clientes deben reincorporarse. La compatibilidad automatizada se comprobó con una base creada por la versión anterior; no hace falta una migración para este cambio. [Referencia y límites de validación](plan-de-pruebas.md#separación-de-responsabilidades-spec-006).

## Bases existentes con inicio automático

El spec 007 añade Invitado y AsignacionInvitado mediante creación idempotente; conserva todas las tablas anteriores. Hacé una copia de seguridad de SQLite con el servidor detenido antes de actualizar. Las partidas antiguas conservan sus filas y credenciales en la base, pero el cliente actual no incluye el flujo para recuperarlas. No se reclaman por nombre. Para volver al binario anterior, detené el nuevo y conservá la base y las tablas añadidas; los accesos anteriores continúan disponibles. Los invitados y sus asignaciones solo se usan con la versión nueva.

### Actualizar una instancia que ya está abierta

Después de actualizar el código, cerrá los clientes e iniciá nuevamente el servidor para cargar la ruta de disponibilidad de perfiles. Se conservan la base y el acceso local anterior; no borres las credenciales para crear un segundo jugador. Crear perfil añade un acceso distinto al archivo existente.
