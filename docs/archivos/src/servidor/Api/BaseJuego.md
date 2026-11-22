# BaseJuego.cs

Fuente: [src/servidor/Api/BaseJuego.cs](../../../../../src/servidor/Api/BaseJuego.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Define el contexto de Entity Framework Core y las entidades principales de partida/plaza. Centraliza nombres de tablas, claves y relaciones para que [RepositorioJuego](RepositorioJuego.md) abra unidades de trabajo sobre SQLite. No autentica ni ejecuta turnos por sí mismo.

## Dependencias

`Microsoft.EntityFrameworkCore` aporta DbContext, DbSet, ModelBuilder y mapeo fluido. [PersonajeCatalogo](PersonajeCatalogo.md) es la entidad de catálogo; [EstadoPartida](EstadoPartida.md) declara cartas, rondas, combates, solicitudes y operaciones. [Persistencia](../../../../bases-de-datos.md) explica la separación entre catálogo y copias por partida.

## Estructura

`internal sealed class BaseJuego(DbContextOptions<BaseJuego> opciones) : DbContext(opciones)` usa constructor primario: pasa las opciones directamente a la base. Sobrescribe `protected override void OnModelCreating(ModelBuilder modelo)`; no declara partial. También declara `internal sealed class PartidaGuardada` y `internal sealed class ParticipanteGuardado`, sin bases ni interfaces explícitas. No hay eventos, operadores o funciones locales.

## Estado y datos

| Miembro de BaseJuego | Tipo, acceso y función |
|---|---|
| Catalogo | internal DbSet<PersonajeCatalogo>, getter `Set<T>()` |
| Partidas | internal DbSet<PartidaGuardada>, getter `Set<T>()` |
| Participantes | internal DbSet<ParticipanteGuardado>, getter `Set<T>()` |
| Cartas | internal DbSet<PersonajePartida>, getter `Set<T>()` |
| Rondas | internal DbSet<RondaGuardada>, getter `Set<T>()` |
| Combates | internal DbSet<CombateGuardado>, getter `Set<T>()` |
| Solicitudes | internal DbSet<SolicitudGuardada>, getter `Set<T>()` |
| Operaciones | internal DbSet<OperacionGuardada>, getter `Set<T>()` |

Estos accesores no ejecutan consultas ni guardan cambios; exponen conjuntos del contexto. El contexto se desecha por operación en el repositorio, que gobierna conexión y transacción.

`PartidaGuardada` tiene propiedades públicas get/set: `Guid Id` (clave por convención), `int Revision` (cero inicial); `int? NumeroRonda`, `NumeroCombate`; `Guid? IdSolicitud`; `string? MotivoFin`, `Ganador`, `AbandonoDe`, todos inicialmente null; `string Fase = "esperandoInicio"`. Los anulables distinguen preparación o ausencia de final de una partida activa; no hay guardas en sus setters.

`ParticipanteGuardado` tiene `Guid IdPartida`, `required string Rol`, `required byte[] Hash`, `string? Nombre` inicialmente null y `int RondasGanadas` inicialmente cero, todos públicos get/set. Hash almacena SHA-256, no la credencial. El array es mutable; la propiedad no lo copia. Rol junto a IdPartida forma la clave. `required` obliga inicialización de código, no garantiza valores J1/J2 en una asignación arbitraria.

## Miembros y ejecución

El constructor de BaseJuego no abre ni crea SQLite: lo hace el repositorio después. `OnModelCreating` se invoca por EF al construir el modelo. No retorna datos ni recorre partidas; declara este mapa:

| Entidad → tabla | Clave y restricciones declaradas aquí |
|---|---|
| PersonajeCatalogo → PersonajeCatalogo | Id no generado (`ValueGeneratedNever`) |
| PartidaGuardada → Partida | Id por convención |
| ParticipanteGuardado → Participante | (IdPartida,Rol); FK a Partida por IdPartida |
| PersonajePartida → PersonajePartida | (IdPartida,IdPersonaje); FK a Partida |
| RondaGuardada → Ronda | (IdPartida,Numero); FK a Partida |
| CombateGuardado → Combate | (IdPartida,NumeroRonda,Numero); FK a Ronda (IdPartida,Numero) |
| SolicitudGuardada → Solicitud | Id por convención; índice no único en IdPartida; FK a Combate (IdPartida,NumeroRonda,NumeroCombate) |
| OperacionGuardada → Operacion | (IdPartida,Rol,IdOperacion); FK a Participante (IdPartida,Rol) y a Solicitud por IdSolicitud |

Fragmento real del mapeo compuesto de combates y operaciones:

```csharp
        modelo.Entity<CombateGuardado>().ToTable("Combate")
            .HasKey(c => new { c.IdPartida, c.NumeroRonda, c.Numero });
        modelo.Entity<CombateGuardado>().HasOne<RondaGuardada>().WithMany()
            .HasForeignKey(c => new { c.IdPartida, Numero = c.NumeroRonda });
        modelo.Entity<SolicitudGuardada>().ToTable("Solicitud").HasIndex(s => s.IdPartida);
        modelo.Entity<SolicitudGuardada>().HasOne<CombateGuardado>().WithMany()
            .HasForeignKey(s => new { s.IdPartida, s.NumeroRonda, Numero = s.NumeroCombate });
        modelo.Entity<OperacionGuardada>().ToTable("Operacion")
            .HasKey(o => new { o.IdPartida, o.Rol, o.IdOperacion });
        modelo.Entity<OperacionGuardada>().HasOne<ParticipanteGuardado>().WithMany()
            .HasForeignKey(o => new { o.IdPartida, o.Rol });
        modelo.Entity<OperacionGuardada>().HasOne<SolicitudGuardada>().WithMany()
            .HasForeignKey(o => o.IdSolicitud);
```

La propiedad anónima `Numero = c.NumeroRonda` adapta el nombre del campo de combate al de la clave de Ronda; no cambia el número guardado. `HasOne<...>().WithMany()` declara relaciones sin propiedades de navegación. No declara FK de carta al catálogo: la copia debe sobrevivir al reemplazo del catálogo. Tampoco declara una FK para Partida.IdSolicitud; las transiciones mantienen ese puntero.

## Recorridos y ejemplos

Normal: crear partida P y participantes `(P,J1)` y `(P,J2)` permite dos plazas con mismo IdPartida porque la clave incluye Rol. Guardar un combate `(P,1,1)` requiere la ronda `(P,1)`. Alternativo: insertar otra plaza `(P,J1)` entra en conflicto de clave; una operación que referencia una solicitud inexistente viola la FK. Son consecuencias del modelo, no inserciones ejecutadas aquí.

## Errores, límites y efectos observables

Este archivo no convierte excepciones de base ni verifica umbrales académicos, roles o fases. Los setters admiten asignaciones que luego pueden fallar al guardar. EnsureCreated del repositorio crea esquema si falta; este archivo no contiene migraciones ni actualiza por sí solo un esquema antiguo. La configuración exacta de proveedor se recibe en opciones.

## Relaciones y verificación

[ApiHttpTests](../../../test/proyecto-integrador-final-tests/ApiHttpTests.md) define recuperación tras reinicio y rollback de elección; [CargaJsonTests](../../../test/proyecto-integrador-final-tests/CargaJsonTests.md) define persistencia exacta y reversión de reemplazo fallido. Cubren usos del esquema, no toda asignación arbitraria a las entidades. Véase [plan de pruebas](../../../../plan-de-pruebas.md) para ejecución y limitaciones.
