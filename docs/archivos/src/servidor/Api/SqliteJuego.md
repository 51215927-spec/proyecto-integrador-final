# SqliteJuego.cs

Fuente: [src/servidor/Api/SqliteJuego.cs](../../../../../src/servidor/Api/SqliteJuego.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión: [catálogo común](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Conexiones, transacciones y exclusión compartida.

## Estado, miembros y ejecución

El constructor conserva la cadena, abre una conexión permanente para :memory: y llama EnsureCreated. Ejecutar toma Sincronizacion, abre contexto y conexión, inicia una transacción inmediata para escritura o diferida para lectura, ejecuta el callback, hace un único SaveChanges si corresponde y confirma. Abrir configura EF con cadena o conexión de memoria. Dispose cierra la conexión permanente.

## Errores, límites y efectos

La escritura toma el bloqueo SQLite antes de leer el estado modificable. La salida excepcional dispone la transacción sin commit. No tiene reglas de partidas, catálogo o sesiones; no requiere migración ni descarta bases previas.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

// Infraestructura compartida: ninguna regla de partidas, catálogo o sesiones.
internal sealed class SqliteJuego : IDisposable
{
    private readonly string cadena;
    private readonly SqliteConnection? memoria;
    internal object Sincronizacion { get; } = new();

    internal SqliteJuego(string cadena)
    {
        this.cadena = cadena;
        if (new SqliteConnectionStringBuilder(cadena).DataSource == ":memory:")
        {
            memoria = new SqliteConnection(cadena);
            memoria.Open();
        }
        using var db = Abrir();
        db.Database.EnsureCreated();
    }

    internal T Ejecutar<T>(bool escritura, Func<BaseJuego, T> operacion)
    {
        lock (Sincronizacion)
        {
            using var db = Abrir();
            db.Database.OpenConnection();
            using var transaccion = ((SqliteConnection)db.Database.GetDbConnection())
                .BeginTransaction(deferred: !escritura);
            db.Database.UseTransaction(transaccion);
            var resultado = operacion(db);
            if (escritura) db.SaveChanges();
            transaccion.Commit();
            return resultado;
        }
    }

    private BaseJuego Abrir()
    {
        var opciones = new DbContextOptionsBuilder<BaseJuego>();
        if (memoria is null) opciones.UseSqlite(cadena);
        else opciones.UseSqlite(memoria);
        return new BaseJuego(opciones.Options);
    }

    public void Dispose() => memoria?.Dispose();
}
```

## Relaciones y verificación

Véanse [arquitectura](../../../../arquitectura.md), [modelos](../../../../modelos.md) y [auditoría del spec 006](../../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
