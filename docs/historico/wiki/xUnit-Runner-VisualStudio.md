# xunit.runner.visualstudio

El paquete 2.5.3 adapta el proyecto xUnit a VSTest. Se usa mediante `dotnet test` en esta solución, también desde la terminal.

1. VSTest carga el adaptador declarado en el proyecto de pruebas.
2. El adaptador descubre los escenarios de xUnit en el ensamblado compilado y ejecuta los seleccionados por el comando.
3. Los informes muestran casos aprobados, fallidos u omitidos. En particular, el atributo de contrato puede marcar un caso como omitido cuando falta la configuración del ejecutable.
[Proyecto de pruebas](Archivos-de-compilacion.md#proyecto-integrador-final-testscsproj) · [Guía de xUnit v2](https://xunit.net/docs/getting-started/v2/getting-started)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
