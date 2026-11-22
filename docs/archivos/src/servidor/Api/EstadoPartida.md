# EstadoPartida.cs

Fuente: [src/servidor/Api/EstadoPartida.cs](../../../../../src/servidor/Api/EstadoPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Agrupa los datos persistidos de cartas de una partida, rondas, combates, solicitudes y recibos idempotentes. Proporciona conversiones entre copia persistida, dominio local y JSON visible. [TransicionesPartida](TransicionesPartida.md) modifica estas entidades; [PresentacionPartida](PresentacionPartida.md) compone la vista sin filtrar manos ajenas por accidente.

## Dependencias

[PersonajeCatalogo](PersonajeCatalogo.md) es origen de copias; [Personaje](../Personaje.md) valida/reconstruye reglas; [BaseJuego](BaseJuego.md) declara claves compuestas y FKs. `Dictionary<string,int>` transporta puntajes. [Protocolo](../../../../protocolo-de-partida.md) explica solicitudes y recibos.

## Estructura

Clases `internal sealed`: PersonajePartida, RondaGuardada, CombateGuardado, SolicitudGuardada y OperacionGuardada. Records posicionales `internal sealed`: CartaVisible, Eleccion, Recibo y Confirmacion. No tienen partial, interfaces explícitas, atributos, eventos, operadores ni funciones locales. Los records generan constructor, propiedades públicas init e igualdad por valor; arrays/diccionarios no se clonan mediante esa generación.

## Estado y datos

Todas las propiedades de las clases son públicas get/set y por tanto mutables. Sin inicializador, enteros son cero, bool es false, Guid es Empty y anulables null; `required` exige inicializar al crear desde C#.

| Tipo | Propiedades completas y significado |
|---|---|
| PersonajePartida | Guid IdPartida, int IdPersonaje (clave compuesta); required string Nombre/Descripcion/Ascii; int Fuerza/Velocidad/Resistencia/Inteligencia/Energia/Habilidad; string Ubicacion = "mazo"; int Orden. Ubicación cambia a J1/J2/jugada en el ciclo de ronda. Orden fija presentación y reparto. |
| RondaGuardada | Guid IdPartida; int Numero; required string Mano; int VictoriasJ1/VictoriasJ2; string? Ganador. Mano inicia elecciones y alterna entre rondas; contadores son combates ganados, no rondas de partida. |
| CombateGuardado | Guid IdPartida; int NumeroRonda/Numero; required string Elector; int? IdCartaElector/IdCartaRival; string? Atributo; int? PuntajeElector/PuntajeRival; string? Ganador. Los null distinguen elecciones/resultados aún incompletos. |
| SolicitudGuardada | Guid Id/IdPartida; int NumeroRonda/NumeroCombate; required string Rol/Tipo; bool Consumida. Una solicitud identifica la oportunidad concreta para una plaza. |
| OperacionGuardada | Guid IdPartida/IdOperacion/IdSolicitud; required string Rol; int RevisionEsperada/IdPersonaje/RevisionConfirmada; string? Atributo. Conserva entrada y revisión posterior para comparar reintentos. |
| CartaVisible | int IdPersonaje; string Nombre/Descripcion/Ascii; Dictionary<string,int> Atributos. Payload de carta, sin ubicación del rival ni credenciales. |
| Eleccion | Guid IdOperacion/IdSolicitud; int? RevisionEsperada/IdPersonaje; string? Atributo. Anulables permiten distinguir ausencia de campos numéricos. |
| Recibo | Guid IdOperacion/IdSolicitud; int RevisionConfirmada. Confirma la transacción, no el turno actual. |
| Confirmacion | Recibo Recibo; bool Repetida. El bool selecciona HTTP 200 para repetición y 201 para primera confirmación. |

## Miembros y ejecución

`internal static PersonajePartida Copiar(Guid partida, PersonajeCatalogo origen)` crea entidad nueva con IdPartida recibido y copia Id, nombre, descripción, arte y los seis puntajes del catálogo, sin normalización ni vínculo vivo con ese objeto. No copia Activo: el llamador ya filtró activos. Orden queda cero y Ubicacion mazo hasta repartir; null en origen produciría NullReferenceException porque no hay guarda.

`internal Personaje Dominio()` construye un nuevo Personaje con nombre, arte y seis puntajes; puede lanzar ArgumentException si la fila es inválida. No copia descripción o ID al dominio porque ese tipo no los tiene. No persiste cambios.

`internal CartaVisible Vista()` crea record y diccionario nuevos con los seis valores. Las claves visibles llevan inicial mayúscula y `Energía` con tilde, mientras el campo persistido se llama Energia. No oculta atributos ni modifica la entidad.

Fragmentos reales de las conversiones, sin omitir ninguna asignación:

```csharp
    internal static PersonajePartida Copiar(Guid partida, PersonajeCatalogo origen) => new()
    {
        IdPartida = partida, IdPersonaje = origen.Id, Nombre = origen.Nombre,
        Descripcion = origen.Descripcion, Ascii = origen.Ascii, Fuerza = origen.Fuerza,
        Velocidad = origen.Velocidad, Resistencia = origen.Resistencia,
        Inteligencia = origen.Inteligencia, Energia = origen.Energia, Habilidad = origen.Habilidad
    };

    internal Personaje Dominio() =>
        new(Nombre, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);

    internal CartaVisible Vista() => new(IdPersonaje, Nombre, Descripcion, Ascii, new()
    {
        ["Fuerza"] = Fuerza, ["Velocidad"] = Velocidad, ["Resistencia"] = Resistencia,
        ["Inteligencia"] = Inteligencia, ["Energía"] = Energia, ["Habilidad"] = Habilidad
    });
```

`internal Eleccion Datos()` de OperacionGuardada devuelve exactamente los cinco campos de entrada (no RevisionConfirmada); habilita comparación por igualdad del record. `internal Recibo Recibo()` devuelve IdOperacion, IdSolicitud y RevisionConfirmada. Son asignaciones sin ramas, consultas o efectos externos:

```csharp
    internal Eleccion Datos() => new(IdOperacion, IdSolicitud, RevisionEsperada, IdPersonaje, Atributo);
    internal Recibo Recibo() => new(IdOperacion, IdSolicitud, RevisionConfirmada);
```

Las demás clases solo contienen datos; sus constructores implícitos y accesores no añaden validación ni recorridos propios.

## Recorridos y ejemplos

Normal: copiar catálogo ID 8 con fuerza 650 a partida P produce clave `(P,8)`, ubicación mazo y fuerza 650. Vista devuelve clave `Fuerza:650`; cambiar después el catálogo no modifica la copia. Una operación con revisión esperada 4 y confirmada 5 devuelve Datos con 4 y Recibo con 5.

Alternativo: Dominio de una fila con seis 500 falla por falta de fortaleza aunque la suma sea correcta; los setters de la entidad no lo habían impedido. No existe rama de error en Vista ni Datos/Recibo cuando sus datos están inicializados: solo proyectan valores. Recorridos deducidos por lectura.

## Errores, límites y efectos observables

No guarda ni hace transacciones. El repositorio asigna IDs, cambia ubicaciones y marca Consumida. Required no equivale a validación de JSON ni SQL de reglas académicas. Las propiedades permiten estados intermedios deliberados (combate sin ambas cartas); las transiciones fijan cuándo pueden publicarse.

## Relaciones y verificación

[ApiHttpTests](../../../test/proyecto-integrador-final-tests/ApiHttpTests.md) y [ServidorContratoTests](../../../test/proyecto-integrador-final-tests/ServidorContratoTests.md) definen privacidad, recuperación y recibo estable; [CargaJsonTests](../../../test/proyecto-integrador-final-tests/CargaJsonTests.md) define datos exactos y reversión del catálogo; no ensaya explícitamente una partida activa durante su reemplazo. La independencia de copias se comprueba aquí por las asignaciones de Copiar y la ausencia de FK al catálogo. La lectura de estas pruebas no acredita ejecución; consultar [plan](../../../../plan-de-pruebas.md).
