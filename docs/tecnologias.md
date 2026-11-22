# Tecnologías

| Tecnología | Uso en Guerra |
|---|---|
| SQLite | Guarda el catálogo y las partidas en un archivo local del servidor. |
| Entity Framework Core | Traduce las entidades de `BaseJuego` a tablas y gestiona consultas y escrituras con el proveedor SQLite. |
| ASP.NET Core y Kestrel | Alojan las rutas REST y procesan solicitudes JSON recibidas por HTTP. |
| HttpClient | El cliente consulta presentaciones, renueva presencia y envía elecciones; la carga consulta cantidades y reemplaza el catálogo JSON por HTTP. |
| Spectre.Console | Dibuja cartas, mesas, menús y marcadores en el cliente de consola. |

Las dependencias y versiones se declaran en los archivos `.csproj`. [Proyectos de la solución](arquitectura.md#proyectos) · [Esquema SQLite](bases-de-datos.md)
