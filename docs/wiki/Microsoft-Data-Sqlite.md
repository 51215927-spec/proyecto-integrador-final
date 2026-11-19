# Microsoft.Data.Sqlite y SQLite

SQLite es el motor de base de datos empotrado del servidor. Microsoft.Data.Sqlite da a .NET conexiones, comandos y transacciones para ese motor.

1. Con una cadena que apunta a un archivo, el contenido queda en disco. Con `Data Source=:memory:` queda en memoria y el repositorio conserva abierta la conexión.
2. `Ejecutar<T>` abre la conexión y llama a `BeginTransaction(deferred: !escritura)`. Para una escritura solicita el turno antes de validar el estado. EF Core se une con `UseTransaction`.
3. Una confirmación guarda estado, revisión y recibo juntos. Las operaciones de sesión viven en memoria y el bloqueo del repositorio coordina su acceso dentro del proceso.
4. El middleware reconoce excepciones de SQLite y las presenta como indisponibilidad del almacenamiento cuando corresponde. Los tests de integración verifican que un fallo de escritura deje la elección sin confirmar.
[Código del repositorio](Codigo-del-servidor.md#repositoriojuegocs)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
