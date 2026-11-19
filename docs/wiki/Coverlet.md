# Coverlet

El proyecto declara `coverlet.collector` 6.0.0. El recolector mide qué código se ejecuta mientras corren las pruebas.

```sh
dotnet test proyecto-integrador-final.sln --collect:"XPlat Code Coverage"

```
El parámetro activa el recolector de VSTest. La salida indica la ruta del archivo `coverage.cobertura.xml` dentro de `TestResults`. La cobertura describe líneas y ramas recorridas. Las aserciones de xUnit determinan si el resultado observado coincide con el esperado. Los casos omitidos no aportan cobertura.
[Proyecto de pruebas](Archivos-de-compilacion.md) · [Integración de Coverlet con VSTest](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/VSTestIntegration.md)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
