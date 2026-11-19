# Apéndice de bibliotecas y herramientas

Las versiones de la tabla corresponden al código documentado. Los paquetes directos aparecen en un `PackageReference`. Los componentes del framework llegan con .NET. El proveedor de EF Core incorpora dependencias como Microsoft.Data.Sqlite, que el servidor usa directamente en el código.

| Componente | Origen | Para qué sirve aquí |
| --- | --- | --- |
| [Spectre.Console](Spectre-Console.md) | paquete 0.57.2 | Dibujar y actualizar pantallas de terminal. |
| [ASP.NET Core y Kestrel](ASP-NET-Core-y-Kestrel.md) | SDK Web, framework compartido | Escuchar HTTP, enlazar datos y ejecutar endpoints. |
| [HttpClient y extensiones JSON para HTTP](HttpClient.md) | .NET | Enviar peticiones desde alta y pruebas. |
| [System.Text.Json](System-Text-Json.md) | .NET | Convertir entre mensajes JSON y datos C#. |
| [EF Core y su proveedor SQLite](Entity-Framework-Core.md) | paquete directo 8.0.34 | Mapear clases a tablas y guardar cambios. |
| [Microsoft.Data.Sqlite y SQLite](Microsoft-Data-Sqlite.md) | dependencia del proveedor | Acceder al motor local y controlar transacciones. |
| [xUnit](xUnit.md) | paquete 2.5.3 | Declarar y comprobar escenarios. |
| [xunit.runner.visualstudio](xUnit-Runner-VisualStudio.md) | paquete 2.5.3 | Conectar el descubrimiento de xUnit con VSTest. |
| [Microsoft.NET.Test.Sdk](Microsoft-NET-Test-Sdk.md) | paquete 17.8.0 | Preparar el proyecto para el host de pruebas. |
| [Coverlet](Coverlet.md) | paquete coverlet.collector 6.0.0 | Obtener un informe de cobertura durante la ejecución. |

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
