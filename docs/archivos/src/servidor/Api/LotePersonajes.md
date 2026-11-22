# LotePersonajes.cs

Fuente: [src/servidor/Api/LotePersonajes.cs](../../../../../src/servidor/Api/LotePersonajes.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Convierte el JSON de reemplazo de catálogo en un array tipado, comprobando forma, tipos, IDs prohibidos y nombres duplicados antes de persistir. [ServidorHttp](ServidorHttp.md) lo llama para PUT del catálogo; [RepositorioJuego](RepositorioJuego.md) vuelve a validar reglas de dominio y realiza el reemplazo atómico.

## Dependencias

`System.Text.Json.JsonElement` inspecciona el árbol sin deserialización permisiva. [AltaPersonaje](PersonajeCatalogo.md) recibe los nueve campos. [ErrorApi](ServidorHttp.md) transporta el rechazo HTTP 400. No requiere base ni configuración.

## Estructura

`internal static class LotePersonajes`, sin estado estático mutable. Declara `Leer`, `Invalido` y las funciones locales `Texto` y `Entero` dentro del recorrido. Las lambdas de `Any` detectan nombres de propiedad prohibidos. No tiene constructor de instancia, propiedades, eventos ni partial.

## Estado y datos

Cada llamada crea una lista de altas y un `HashSet<string>(StringComparer.OrdinalIgnoreCase)`. El hash usa nombre recortado con `Trim`, pero el nombre que se conserva en el alta no se recorta. El índice de error empieza en 1 y avanza solo tras agregar un personaje válido. Campos extra no prohibidos se ignoran; los nueve obligatorios se buscan con nombre exacto en minúsculas.

## Miembros y ejecución

`internal static AltaPersonaje[] Leer(JsonElement raiz)` rechaza raíz no-array o array vacío con 400. Recorre todos los elementos en orden; cada uno debe ser objeto. Si `nombre` ya es string, lo incluye en la ubicación del error. Antes de crear datos, examina todas las propiedades y rechaza `id` o `idPersonaje` sin distinguir mayúsculas. Llama a Texto para nombre, descripcion y ascii; a Entero para fuerza, velocidad, resistencia, inteligencia, energia y habilidad. Luego agrega el nombre normalizado al conjunto; si ya existía rechaza el lote completo. Si todo termina devuelve un array en orden de entrada, sin asignar IDs ni guardar nada.

`string Texto(string campo)` captura el objeto y la ubicación actuales: propiedad ausente o no-string produce error; string vacío sí pasa esta capa. Retorna el string no nulo. `int Entero(string campo)` exige propiedad numérica y `TryGetInt32` exitoso; no convierte texto ni fracciones fuera de la representación int32 aceptada. No aplica rangos académicos. Ambas funciones terminan en retorno o excepción, sin reintentos.

Fragmentos de `Leer`, incluyendo auxiliares y control de IDs:

```csharp
            string Texto(string campo)
            {
                if (!objeto.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.String)
                    throw Invalido($"{ubicacion}: {campo} debe ser texto y es obligatorio.");
                return valor.GetString()!;
            }
            int Entero(string campo)
            {
                if (!objeto.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.Number || !valor.TryGetInt32(out int numero))
                    throw Invalido($"{ubicacion}: {campo} debe ser un entero y es obligatorio.");
                return numero;
            }
            if (objeto.EnumerateObject().Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("idPersonaje", StringComparison.OrdinalIgnoreCase)))
                throw Invalido($"{ubicacion}: el ID lo asigna el servidor.");
```

`internal static ErrorApi Invalido(string mensaje)` devuelve una excepción nueva con estado 400 y código `entradaInvalida`; no la lanza por sí mismo. Los llamadores usan `throw`. La validación de atributos y ASCII está en AltaPersonaje, no en estas funciones locales.

### Fragmentos de raíz, ubicación y duplicados

El recorrido existe solo para un array no vacío; el diagnóstico de un elemento no-objeto antecede al acceso a propiedades:

```csharp
        if (raiz.ValueKind != JsonValueKind.Array || raiz.GetArrayLength() == 0)
            throw Invalido("La raíz debe ser una lista con al menos un personaje.");
        var resultado = new List<AltaPersonaje>();
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var objeto in raiz.EnumerateArray())
        {
            string ubicacion = $"Personaje {resultado.Count + 1}";
            if (objeto.ValueKind != JsonValueKind.Object) throw Invalido($"{ubicacion}: se esperaba un objeto.");
            if (objeto.TryGetProperty("nombre", out var nombre) && nombre.ValueKind == JsonValueKind.String)
                ubicacion += $" ({nombre.GetString()})";
```

Al construir AltaPersonaje, cada llamada local exige tipo exacto. El conjunto comprueba nombre recortado sin modificar el valor guardado:

```csharp
            var datos = new AltaPersonaje(Texto("nombre"), Texto("descripcion"), Texto("ascii"),
                Entero("fuerza"), Entero("velocidad"), Entero("resistencia"), Entero("inteligencia"), Entero("energia"), Entero("habilidad"));
            if (!nombres.Add(datos.Nombre!.Trim())) throw Invalido($"{ubicacion}: nombre duplicado.");
            resultado.Add(datos);
        }
        return resultado.ToArray();
```

## Recorridos y ejemplos

Normal: un objeto con nombre Ada, descripcion vacía, ascii `*` y seis enteros `(250,350,450,550,650,750)` produce un alta; RepositorioJuego decidirá el ID. Alternativo: nombres `Ada` y ` ada ` provocan `Personaje 2 ( ada ): nombre duplicado.`; una propiedad `ID` se rechaza aunque los demás datos sean correctos. Un array `[7]` falla como objeto antes de leer campos. Recorridos de lectura, no peticiones ejecutadas aquí.

## Errores, límites y efectos observables

No impone tamaño máximo de lote propio, ni verifica nombre en blanco, límites ASCII o suma de puntajes: la capa de dominio posterior lo hace. No contiene persistencia, concurrencia ni efectos externos; al fallar no devuelve un lote parcial.

## Relaciones y verificación

[CargaJsonTests](../../../test/proyecto-integrador-final-tests/CargaJsonTests.md) define rechazo de formato incompleto, duplicados normalizados, obligatoriedad y tipos estrictos. La atomicidad pertenece al repositorio y se prueba allí mediante reemplazo y fallo de inserción. Véase [plan de pruebas](../../../../plan-de-pruebas.md) para ejecución real y [carga](../../subir-personajes/Program.md) para la comprobación previa del cliente.
