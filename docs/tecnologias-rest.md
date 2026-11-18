# Tecnologías del protocolo REST

Selección tecnológica de TI para el [protocolo HTTP](protocolo-partida-web.md) de la actividad 6. La incorporación de paquetes al servidor corresponde a Taller.

## Tecnologías seleccionadas

| Tecnología | Responsabilidad prevista |
|---|---|
| C# y .NET | Lenguaje y plataforma para dominio, servidor y clientes de consola. |
| ASP.NET Core con Minimal APIs | Exponer las operaciones REST y delegar las reglas al dominio. |
| HttpClient | Realizar solicitudes HTTP desde los clientes de consola. |
| System.Text.Json | Serializar y deserializar los DTO de intercambio JSON. |
| SQLite | Persistir el catálogo de personajes y los datos de las partidas en una base local al servidor. |
| Entity Framework Core con proveedor SQLite | Acceso a datos, configuración del modelo persistente, migraciones y transacciones. |
| xUnit | Pruebas automatizadas de reglas e integración durante Taller, siguiendo TDD. |

## Persistencia

SQLite almacena en el servidor el catálogo permanente y el estado de las partidas. Los clientes obtienen sus vistas mediante la API. La importación del CSV, las copias por partida y los límites de transacción están en el [diseño de persistencia](protocolo-partida-web.md#persistencia-concurrencia-y-catálogo). El contrato de DataBases describe el [formato de origen](arquitectura.md#formato-de-archivos-de-personajes--actividad-5).

## Versiones de referencia

El proyecto usa `net8.0` como framework de destino. Las versiones acordadas para el servidor son las siguientes.

| Componente | Versión de referencia |
|---|---|
| Framework de destino | net8.0 |
| .NET Runtime y ASP.NET Core Runtime | 8.0.34 |
| Microsoft.EntityFrameworkCore | 8.0.34 |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.34 |
| Herramientas y paquete Design de EF Core, si se utilizan para migraciones | Misma línea y parche que EF Core; verificar al preparar Taller. |

Los SDK 8.0.428 y 8.0.134 incluyen los runtimes de referencia. La selección y la comprobación de las instalaciones de ambos equipos quedan para la [verificación en red](protocolo-partida-web.md#verificación-en-dos-equipos). La línea .NET 8 ya terminó su soporte.

## Instalación prevista en Taller

Estos son los comandos de instalación previstos. Se ejecutarán cuando Taller esté autorizado y exista el proyecto servidor; no deben aplicarse al proyecto de consola actual. Cuando llegue esa etapa, PowerShell pedirá la ruta real del archivo `.csproj` del servidor, para no depender de un nombre todavía sin definir:

```powershell
$proyectoServidor = Read-Host "Ruta del archivo .csproj del servidor"
if (-not (Test-Path -LiteralPath $proyectoServidor -PathType Leaf)) {
    throw "No existe el archivo de proyecto indicado."
}
dotnet add "$proyectoServidor" package Microsoft.EntityFrameworkCore --version 8.0.34
dotnet add "$proyectoServidor" package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.34
dotnet add "$proyectoServidor" package Microsoft.EntityFrameworkCore.Design --version 8.0.34
dotnet restore "$proyectoServidor"
dotnet list "$proyectoServidor" package
```

El proveedor SQLite incluye las dependencias necesarias para usar la base desde .NET. No hace falta instalar un servidor SQLite por separado. Los clientes de consola no necesitan estos paquetes de persistencia.

Para instalar la herramienta de migraciones de EF Core por primera vez:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.34
dotnet ef --version
```

Si `dotnet-ef` ya estaba instalada y necesitás seleccionar esta versión, usá esta alternativa al comando de instalación:

```powershell
dotnet tool update --global dotnet-ef --version 8.0.34
```

Si la versión global existente pertenece a otra línea y la herramienta rechaza el cambio, conservála y revisá una instalación local para este proyecto; no hace falta desinstalar herramientas usadas por otros proyectos. Crear tablas y migraciones requiere diseñar e implementar primero el modelo de datos; instalar los paquetes no crea la base del juego.

Referencias oficiales del [proveedor SQLite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/) y las [herramientas de EF Core](https://learn.microsoft.com/en-us/ef/core/cli/dotnet).

## Referencias

- [Minimal APIs de ASP.NET Core](https://dotnet.microsoft.com/en-us/learn/back-end-web-dev).
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview).
- [Proveedor SQLite de EF Core](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/).
- [Descarga oficial de .NET 8 y ASP.NET Core Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
- [EF Core 8.0.34](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/8.0.34).
- [Proveedor SQLite 8.0.34](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/8.0.34).
- [Política de soporte de .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
