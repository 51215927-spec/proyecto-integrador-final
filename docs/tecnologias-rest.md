# Tecnologías del protocolo REST

Selección tecnológica de TI para el [protocolo HTTP](protocolo-partida-web.md) de la actividad 6. La rama `feature/api-rest-partida` incorpora el proveedor SQLite al proyecto servidor.

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

## Preparación del servidor

En la rama `feature/api-rest-partida`, el proyecto `src/servidor/servidor.csproj` declara `Microsoft.EntityFrameworkCore.Sqlite` 8.0.34. Restaurar la solución obtiene el proveedor y sus dependencias, incluido EF Core. La base se crea al iniciar el servidor. Consultá los comandos y la configuración de almacenamiento en la [guía de operación](protocolo-partida-web.md#operación-del-servidor).

## Referencias

- [Minimal APIs de ASP.NET Core](https://dotnet.microsoft.com/en-us/learn/back-end-web-dev).
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview).
- [Proveedor SQLite de EF Core](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/).
- [Descarga oficial de .NET 8 y ASP.NET Core Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
- [EF Core 8.0.34](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/8.0.34).
- [Proveedor SQLite 8.0.34](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/8.0.34).
- [Política de soporte de .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
