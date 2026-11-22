# RepositorioJuego.cs

Fuente: [src/servidor/Api/RepositorioJuego.cs](../../../../../src/servidor/Api/RepositorioJuego.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Proporciona acceso serializado al catálogo y partidas, transacciones SQLite y autenticación por plaza. Conserva presencia en memoria durante 15 segundos. Es la parte central de un tipo parcial: [TransicionesPartida](TransicionesPartida.md) ejecuta jugadas y [PresentacionPartida](PresentacionPartida.md) proyecta el estado.

## Dependencias

[BaseJuego](BaseJuego.md) y EF Core consultan/persisten entidades; Microsoft.Data.Sqlite abre transacciones. SHA256, UTF8 y `CryptographicOperations.FixedTimeEquals` comparan hashes; RandomNumberGenerator genera credenciales. TimeProvider permite tiempo sustituible. [AltaPersonaje](PersonajeCatalogo.md) valida altas. [Protocolo](../../../../protocolo-de-partida.md) explica presencia, errores y operación administrativa; [base de datos](../../../../bases-de-datos.md) explica durabilidad.

## Estructura

`internal sealed partial class RepositorioJuego : IDisposable`, con constructor interno y Dispose público. Esta porción declara catálogo, credenciales, sesiones, apertura y ejecución; los métodos privados son compartidos por las otras porciones. Los records `internal sealed record PartidaCreada(Guid Id,string CredencialJ1,string CredencialJ2)`, `internal sealed record Incorporacion(Guid IdSesion,string? Nombre)` y `internal sealed record Sesion(string Rol,Guid IdSesion,DateTimeOffset VenceUtc)` generan constructor, propiedades públicas init e igualdad por valor. No tienen lógica adicional ni guardas propias.

## Estado y datos

Campos privados readonly: `string cadena` (conexión); `SqliteConnection? memoria` (solo se abre para DataSource exactamente `:memory:`); `TimeProvider reloj`; `object sincronizacion = new()`; `Dictionary<(Guid,string),Sesion> sesiones = []`. Readonly impide reasignar el contenedor, no mutar el diccionario. Las claves son partida/rol y no se persisten; reiniciar pierde sesiones, mientras nombres, hashes, turnos y recibos quedan en SQLite. Las credenciales en claro se devuelven al crear/reemplazar, pero solo su hash se guarda.

## Miembros y ejecución

### Apertura, transacción y recursos

`internal RepositorioJuego(string cadena, TimeProvider reloj)` conserva argumentos, interpreta la cadena y, si DataSource es `:memory:`, abre una conexión ancla para que la base sobreviva a los contextos. Luego crea un contexto temporal y llama EnsureCreated; no migra esquema existente. Errores de cadena, permisos o SQLite se propagan.

`private BaseJuego Abrir()` construye opciones EF: UseSqlite(cadena) para archivo o UseSqlite(memoria) para conexión ancla; devuelve contexto nuevo, aún sin ejecutar consultas. `public void Dispose()` desecha la conexión ancla si existe; para archivos no tiene conexión permanente que cerrar.

`private T Ejecutar<T>(bool escritura, Func<BaseJuego,T> operacion)` adquiere lock del repositorio, abre contexto y conexión, inicia transacción con `deferred: !escritura`, asocia EF, ejecuta el delegado, hace SaveChanges solo en escritura, confirma y devuelve resultado. Escritura reserva bloqueo SQLite antes de leer lo que modificará. Excepción antes de Commit sale mediante using y revierte la transacción; no retorna un resultado parcial. Lock de C# es reentrante, por eso puede llamarse desde otros bloques sobre el mismo objeto. Fragmento completo:

```csharp
    private T Ejecutar<T>(bool escritura, Func<BaseJuego, T> operacion)
    {
        lock (sincronizacion)
        {
            using var db = Abrir();
            db.Database.OpenConnection();
            // La escritura adquiere el bloqueo de SQLite antes de leer el estado que va a modificar.
            using var transaccion = ((SqliteConnection)db.Database.GetDbConnection())
                .BeginTransaction(deferred: !escritura);
            db.Database.UseTransaction(transaccion);
            var resultado = operacion(db);
            if (escritura)
                db.SaveChanges();
            transaccion.Commit();
            return resultado;
        }
    }

```

No hay await dentro del lock. El delegado no debe escapar con un contexto vivo; el contexto queda desechado al retornar.

### Catálogo

`internal int DarDeAlta(AltaPersonaje datos)` valida antes de la transacción. Dentro calcula el máximo ID positivo (cero si no existe); si es int.MaxValue falla 409 operacionIncompatible. Asigna max+1, agrega entidad y retorna ID después del commit. No comprueba duplicado de nombre en alta individual.

`internal int CantidadCatalogo()` ejecuta lectura Count de todos, incluidos inactivos.

`internal int ReemplazarCatalogo(AltaPersonaje[] datos)` rechaza array vacío; valida cada alta en orden con Select, asigna IDs índice+1, rechaza nombres repetidos tras Trim con comparación OrdinalIgnoreCase. ErrorApi durante esa preparación se envuelve con índice desde 1 y nombre. ToArray fuerza validación completa antes de borrar. Luego, en una única transacción de escritura, ExecuteDelete borra catálogo, AddRange inserta todos y retorna cantidad después de SaveChanges/Commit. Si falla inserción, rollback recupera catálogo anterior; las copias PersonajePartida no se borran. Fragmento decisivo:

```csharp
        return Ejecutar(true, db =>
        {
            db.Catalogo.ExecuteDelete();
            db.Catalogo.AddRange(personajes);
            return personajes.Length;
        });
```

`internal void Desactivar(int id)` busca entidad por ID o falla 404; pone Activo false y confirma. Repetirlo conserva false; no altera copias ya repartidas.

### Administración de partidas y credenciales

`internal PartidaCreada CrearPartida()` genera Guid y dos secretos antes de escribir. Agrega PartidaGuardada (fase esperandoInicio, revisión cero) y dos participantes J1/J2 con hashes; retorna ID y ambos secretos tras commit. No inicia ni copia cartas.

`internal void CerrarSesion(Guid id,string rol)` dentro de lock exige participante existente mediante lectura, luego elimina presencia en memoria. No cambia revisión ni hash; rol ausente produce 404 antes de borrar.

`internal string ReemplazarCredencial(Guid id,string rol)` genera secreto nuevo, guarda su hash sobre participante existente en transacción y luego retira la sesión; retorna nuevo secreto. Si falla guardado no elimina sesión ni devuelve secreto válido.

`internal void Cancelar(Guid id)` busca partida o 404; si ya finalizada falla 409 operacionIncompatible. Incrementa revisión y llama Finalizar con motivo cancelacion sin ganador. No borra plazas ni historial.

### Incorporación y presencia

`internal Sesion Incorporar(Guid id,string? credencial,Incorporacion datos)` rechaza IdSesion Empty o nombre null/blanco con 400 antes del lock. Dentro exige partida, autentica plaza, consulta sesión vigente; una vigente con UUID distinto falla 409 sesionOcupada. Si ya hay nombre registrado y no coincide exactamente (incluidos espacios/caso), falla 403 sinPermiso. Si no existe, asigna nombre. Crea sesión que vence en reloj UTC+15s, confirma transacción y recién entonces actualiza diccionario. Misma sesión vigente puede renovarse mediante incorporación; una vencida permite nuevo UUID con mismo nombre. No incrementa revisión.

`internal void Renovar(Guid id,string? credencial,Guid idSesion)` dentro de lock exige partida, credencial y sesión vigente exacta. Después sustituye record por `with { VenceUtc = ahora+15s }`; no persiste sesión ni cambia revisión. Datos inválidos no prolongan presencia.

### Ayudantes privados

- `static ParticipanteGuardado BuscarParticipante(BaseJuego db,Guid id,string rol)`: Find compuesto o ErrorApi.NoEncontrado (404).
- `static PartidaGuardada Buscar(BaseJuego db,Guid id)`: Find de partida o 404.
- `static ParticipanteGuardado Autenticar(BaseJuego db,Guid id,string? credencial)`: si no es null/blanco, calcula hash y recorre participantes de esa partida, devolviendo primero con FixedTimeEquals. Si ninguna coincidencia, 401 credencialInvalida. No recorta secreto ni revela qué plaza falló.
- `Sesion? Vigente(Guid id,string rol)`: devuelve sesión del diccionario solo si VenceUtc > reloj UTC actual; igualdad exacta ya está vencida. No elimina entradas vencidas.
- `Sesion ExigirSesion(Guid id,string rol,Guid idSesion)`: exige Vigente y UUID igual o 409 sesionVencida.
- `static byte[] Hash(string credencial)`: SHA256 de bytes UTF8 exactos.
- `static string NuevaCredencial()`: 32 bytes criptográficos a hexadecimal (64 caracteres); no usa Random.Shared.

Fragmento real de sesión vigente y errores:

```csharp
    private Sesion? Vigente(Guid id, string rol) =>
        sesiones.TryGetValue((id, rol), out var sesion) && sesion.VenceUtc > reloj.GetUtcNow()
            ? sesion : null;

    private Sesion ExigirSesion(Guid id, string rol, Guid idSesion) =>
        Vigente(id, rol) is { } sesion && sesion.IdSesion == idSesion
            ? sesion : throw new ErrorApi(409, "sesionVencida", "Se requiere una sesión vigente.");
```

### Fragmentos de autenticación y mutaciones

El constructor conserva una conexión solo para `:memory:`; mantenerla abierta evita que cada contexto vea una base nueva. EnsureCreated usa ese mismo origen:

```csharp
        if (new SqliteConnectionStringBuilder(cadena).DataSource == ":memory:")
        {
            memoria = new SqliteConnection(cadena);
            memoria.Open();
        }

        using var db = Abrir();
        db.Database.EnsureCreated();
```

DarDeAlta calcula un ID positivo nuevo bajo transacción; agotar int32 produce conflicto en vez de desbordar:

```csharp
            int mayor = db.Catalogo.Where(p => p.Id > 0).Select(p => (int?)p.Id).Max() ?? 0;
            if (mayor == int.MaxValue)
                throw new ErrorApi(409, "operacionIncompatible", "Se agotaron los IDs de alta automática.");
            personaje.Id = mayor + 1;
            db.Catalogo.Add(personaje);
            return personaje.Id;
```

Antes de ReemplazarCatalogo, el Select se materializa con ToArray: valida y asigna IDs antes del bloque que borra. Cada error señala personaje y nombre:

```csharp
        if (datos.Length == 0) throw LotePersonajes.Invalido("La lista no puede estar vacía.");
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var personajes = datos.Select((dato, indice) =>
        {
            try
            {
                var personaje = dato.Validar();
                if (!nombres.Add(personaje.Nombre.Trim())) throw LotePersonajes.Invalido("nombre duplicado.");
                personaje.Id = indice + 1;
                return personaje;
            }
            catch (ErrorApi error)
            {
                throw LotePersonajes.Invalido($"Personaje {indice + 1} ({dato.Nombre}): {error.Message}");
            }
        }).ToArray();
```

CrearPartida agrega partida y ambas plazas en la misma operación; las credenciales en claro `j1`/`j2` solo forman el valor de retorno y sus hashes se persisten:

```csharp
        var id = Guid.NewGuid();
        var j1 = NuevaCredencial();
        var j2 = NuevaCredencial();
        return Ejecutar(true, db =>
        {
            db.Partidas.Add(new PartidaGuardada { Id = id });
            db.Participantes.AddRange(
                new ParticipanteGuardado { IdPartida = id, Rol = "J1", Hash = Hash(j1) },
                new ParticipanteGuardado { IdPartida = id, Rol = "J2", Hash = Hash(j2) });
            return new PartidaCreada(id, j1, j2);
```

ReemplazarCredencial solo borra sesión después de confirmar el hash nuevo. Si falla el guardado, no ejecuta Remove:

```csharp
            var nueva = NuevaCredencial();
            Ejecutar(true, db => BuscarParticipante(db, id, rol).Hash = Hash(nueva));
            sesiones.Remove((id, rol));
            return nueva;
```

Cancelar rechaza una partida ya cerrada antes del incremento y usa el helper compartido de finalización:

```csharp
        if (partida.Fase == "finalizada")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.");
        partida.Revision++;
        Finalizar(db, partida, "cancelacion");
```

Incorporar distingue plaza ocupada de nombre distinto y guarda el nombre una sola vez. El diccionario de sesiones se actualiza tras la transacción:

```csharp
                var actual = Vigente(id, participante.Rol);
                if (actual is not null && actual.IdSesion != datos.IdSesion)
                    throw new ErrorApi(409, "sesionOcupada", "La plaza tiene una sesión vigente.");
                if (participante.Nombre is not null && participante.Nombre != datos.Nombre)
                    throw new ErrorApi(403, "sinPermiso", "El nombre registrado debe coincidir.");

                participante.Nombre ??= datos.Nombre;
                return new Sesion(participante.Rol, datos.IdSesion, reloj.GetUtcNow().AddSeconds(15));
            });
            sesiones[(id, sesion.Rol)] = sesion;
            return sesion;
```

La renovación exige acceso vigente y sustituye únicamente vencimiento de presencia; no cambia estado de partida:

```csharp
                Buscar(db, id);
                var jugador = Autenticar(db, id, credencial);
                return ExigirSesion(id, jugador.Rol, idSesion);
            });
            sesiones[(id, sesion.Rol)] = sesion with { VenceUtc = reloj.GetUtcNow().AddSeconds(15) };
```

Autenticar recorre solo plazas de la partida pedida y compara hashes con tiempo fijo; un secreto null o blanco cae directamente en 401:

```csharp
        if (!string.IsNullOrWhiteSpace(credencial))
        {
            var hash = Hash(credencial);
            foreach (var jugador in db.Participantes.Where(p => p.IdPartida == id))
            {
                if (CryptographicOperations.FixedTimeEquals(jugador.Hash, hash))
                    return jugador;
            }
        }

        throw new ErrorApi(401, "credencialInvalida", "La credencial no es válida.");
```

## Recorridos y ejemplos

Normal: CrearPartida produce P y secretos ficticios representados aquí como C1/C2; Incorporar(P,C1,{S1,"Ana"}) guarda Ana y presencia hasta t+15; Renovar a t+10 mueve vencimiento a t+25 sin cambiar revisión. Reiniciar conserva Ana/hash pero exige nueva incorporación porque sesiones está vacío.

Alternativo: a t+14, UUID S2 sobre la misma plaza falla sesionOcupada; a t+15 exacto la sesión anterior ya venció y S2 puede incorporarse si nombre coincide. ReemplazarCatalogo con dos nombres `Ada`/` ada ` falla antes del borrado. Un fallo de inserción tras ExecuteDelete revierte todo. Recorridos por lectura, no operaciones contra servicios reales.

## Errores, límites y efectos observables

Catalogo, nombres y hashes son persistentes; presencia solo en memoria. No hay cancelación asíncrona ni reintento automático de SQLite. Null en datos o array no está defendido por estos contratos internos. El lock serializa esta instancia, no otros procesos; transacciones SQLite protegen almacenamiento. EnsureCreated no sustituye migraciones. Dispose no limpia sesiones individualmente ni elimina la base.

## Relaciones y verificación

[ApiHttpTests](../../../test/proyecto-integrador-final-tests/ApiHttpTests.md) define incorporación, reloj, credencial reemplazada, rollback y reinicio. [CargaJsonTests](../../../test/proyecto-integrador-final-tests/CargaJsonTests.md) define reemplazo repetido sin acumulación, validación previa, datos exactos y rollback por fallo de inserción. [ServidorContratoTests](../../../test/proyecto-integrador-final-tests/ServidorContratoTests.md) ejercita procesos reales. Consultar [plan de pruebas](../../../../plan-de-pruebas.md) para ejecución real; la descripción de los casos no prueba que se hayan ejecutado aquí.
