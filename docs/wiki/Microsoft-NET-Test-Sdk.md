# Microsoft.NET.Test.Sdk

El paquete 17.8.0 integra el proyecto de pruebas con las herramientas de ejecución de .NET. El proyecto también declara `IsTestProject`.

1. `dotnet test` restaura y compila la solución si se requiere, y entrega el ensamblado de pruebas al host.
2. El adaptador de xUnit aporta el conocimiento sobre atributos, descubrimiento y ejecución de los casos.
3. El host devuelve los resultados al comando. `--no-build` reutiliza binarios compilados para ejecutar la misma suite.
[Proyecto de pruebas](Archivos-de-compilacion.md)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
