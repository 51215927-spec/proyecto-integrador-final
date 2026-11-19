# Entity Framework Core y su proveedor SQLite

EF Core es un mapeador entre objetos C# y una base relacional. El proyecto declara `Microsoft.EntityFrameworkCore.Sqlite` 8.0.34.

1. `BaseJuego` deriva de `DbContext`. Sus `DbSet<T>` permiten consultar y agregar entidades. `OnModelCreating` mapea clases, tablas, claves y relaciones.
2. `RepositorioJuego.Abrir` selecciona SQLite con `UseSqlite` y crea un contexto. El proveedor traduce las consultas LINQ aplicadas a los conjuntos de datos en consultas al motor.
3. EF Core rastrea las entidades cargadas en un contexto. Tras cambiar una propiedad, `SaveChanges` envía la escritura a la base. El repositorio confirma la transacción después.
4. `EnsureCreated` prepara el esquema en una base nueva. La aplicación actual arranca con esa inicialización directa.
[Código del modelo](Codigo-del-servidor.md#basejuegocs) · [Proveedor SQLite de EF Core](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
