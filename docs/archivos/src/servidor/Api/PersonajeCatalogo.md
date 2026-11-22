# PersonajeCatalogo.cs

Fuente: [src/servidor/Api/PersonajeCatalogo.cs](../../../../../src/servidor/Api/PersonajeCatalogo.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Declara el contrato de alta y la entidad persistida del catálogo. `AltaPersonaje.Validar` transforma un pedido en una entidad válida, combinando reglas del arte y del modelo [Personaje](../Personaje.md). El ID lo asigna [CatalogoPersonajes](CatalogoPersonajes.md).

## Dependencias

[Personaje](../Personaje.md) comprueba nombre y puntajes; [LotePersonajes.Invalido](LotePersonajes.md) crea errores 400; [BaseJuego](BaseJuego.md) mapea PersonajeCatalogo a SQLite. `string.Replace`, Split y LINQ Any calculan límites del arte. [Bases de datos](../../../../bases-de-datos.md) explica el catálogo y sus copias de partida.

## Estructura

`internal sealed record AltaPersonaje(string? Nombre, string? Descripcion, string? Ascii, int Fuerza, int Velocidad, int Resistencia, int Inteligencia, int Energia, int Habilidad)` genera propiedades públicas init, constructor e igualdad por valor. Declara `internal PersonajeCatalogo Validar()`.

`internal sealed class PersonajeCatalogo` contiene solo propiedades públicas get/set, sin constructor explícito, herencia, interfaces, atributos, partial ni lógica de acceso. `required` exige inicialización en C#, pero no es una validación HTTP de ejecución.

## Estado y datos

| Tipo | Propiedades y significado |
|---|---|
| AltaPersonaje | Nombre/Descripcion/Ascii anulables de entrada; seis puntajes int sin defecto explícito (cero si la deserialización no los aporta). |
| PersonajeCatalogo | `Id` int, clave manual; `required string Nombre`, `Descripcion`, `Ascii`; seis puntajes int con los mismos nombres; `bool Activo = true`. Todos mutables, persistidos por EF. |

No hay eventos, campos adicionales ni credenciales. La descripción vacía y el arte vacío se aceptan; nombre vacío no.

## Miembros y ejecución

`Validar()` rechaza primero descripción null, después arte null, después tabulaciones. Para contar líneas normaliza CRLF y CR a LF en una variable local y divide por LF. Si hay más de 25 líneas o cualquiera supera 120 caracteres UTF-16, falla. No almacena la versión normalizada: conserva el arte exacto recibido, incluidos saltos originales. Después construye temporalmente Personaje con los seis puntajes: rechaza nombre sin texto, atributo fuera de [250,750], suma distinta de 3000, falta de valor ≥650 o falta de valor ≤350, en ese orden. Captura `ArgumentException` de ese constructor y lo envuelve en error 400 con nombre de parámetro y mensaje. Si acepta, devuelve entidad nueva con todos los valores copiados, Id 0 aún sin asignar y Activo true; no persiste.

Fragmento real de la validación de arte y delegación numérica:

```csharp
        if (Descripcion is null) throw LotePersonajes.Invalido("descripcion: debe ser texto no nulo.");
        if (Ascii is null) throw LotePersonajes.Invalido("ascii: debe ser texto no nulo.");
        if (Ascii.Contains('\t')) throw LotePersonajes.Invalido("ascii: no admite tabulaciones.");
        var lineas = Ascii.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (lineas.Length > 25 || lineas.Any(linea => linea.Length > 120))
            throw LotePersonajes.Invalido("ascii: admite hasta 25 líneas de 120 caracteres.");

        try
        {
            _ = new Personaje(Nombre!, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);
        }
        catch (ArgumentException error)
        {
            throw LotePersonajes.Invalido($"{error.ParamName}: {error.Message}");
        }
```

No hay setters con lógica: el bloque final de inicialización simplemente copia los nueve campos. Las seis propiedades numéricas son Fuerza, Velocidad, Resistencia, Inteligencia, Energia y Habilidad; no convierten unidades ni redondean.

### Fragmento de entidad aceptada

Solo después de las validaciones se devuelve esta entidad. La inicialización no asigna ID ni Activo: quedan en cero y true por sus declaraciones hasta que el catálogo les asigne identidad:

```csharp
        return new()
        {
            Nombre = Nombre!,
            Descripcion = Descripcion,
            Ascii = Ascii,
            Fuerza = Fuerza,
            Velocidad = Velocidad,
            Resistencia = Resistencia,
            Inteligencia = Inteligencia,
            Energia = Energia,
            Habilidad = Habilidad
        };
```

## Recorridos y ejemplos

Normal: Ada, descripción vacía, arte `*` y `(250,350,450,550,650,750)` retorna entidad activa con Id pendiente. Alternativo: arte con tabulación falla antes de medir líneas; 26 líneas falla antes de construir Personaje; seis 500 suman 3000 pero fallan por falta de fortaleza. Si el nombre es null y el arte contiene tabulación, el error de arte aparece primero. Ejemplos revisados por lectura.

## Errores, límites y efectos observables

Todos los rechazos explícitos son `ErrorApi(400, entradaInvalida, ...)`. No comprueba duplicados de nombre, caracteres de control distintos de tab, unicidad o longitud del nombre. La entidad puede recibir valores inválidos si se crea directamente sin pasar por Validar; las propiedades no contienen las reglas. No altera ni normaliza los valores aceptados.

## Relaciones y verificación

[CargaJsonTests](../../../test/proyecto-integrador-final-tests/CargaJsonTests.md) define límites de texto, reglas de puntajes y diagnóstico de atributo; [ApiHttpTests](../../../test/proyecto-integrador-final-tests/ApiHttpTests.md) define alta autenticada y rechazo de puntajes como texto. [PersonajeTests](../../../test/proyecto-integrador-final-tests/PersonajeTests.md) cubre el dominio subyacente. Consultar [evidencia de ejecución](../../../../plan-de-pruebas.md); esta página no presenta tests definidos como ejecutados.
