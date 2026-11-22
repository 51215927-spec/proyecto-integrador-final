# ConfiguracionServidor.cs

Fuente: [src/servidor/Api/ConfiguracionServidor.cs](../../../../../src/servidor/Api/ConfiguracionServidor.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Obtiene la dirección de escucha, ruta SQLite y secreto administrativo antes de crear el host. Separa la lectura interactiva del arranque del [servidor](../Program.md).

## Dependencias

[EntradaConsola](../../comun/EntradaConsola.md) reintenta campos, aplica valores por defecto y oculta secretos en terminal interactiva. `Path` y `Directory` inspeccionan rutas, sin crear el archivo. [Instalación](../../../../instalacion-y-uso.md) explica las tres preguntas.

## Estructura

`internal sealed record ConfiguracionServidor(string Direccion, string Archivo, string Credencial)` genera constructor posicional, propiedades públicas init, igualdad por valor y deconstrucción. No tiene partial, interfaces explícitas ni eventos. Sus dos métodos son estáticos; no guarda lector entre llamadas.

## Estado y datos

`Direccion`, `Archivo` y `Credencial` son strings no anulables. Se conservan en la instancia devuelta. Una credencial vacía es válida para deshabilitar la administración HTTP del catálogo; no significa acceso anónimo. La ruta puede ser relativa y se resolverá desde el directorio de ejecución.

## Miembros y ejecución

`internal static ConfiguracionServidor Leer(TextReader entrada, TextWriter salida)` crea el lector, pide URL con defecto `http://0.0.0.0:5000`, archivo con defecto `guerra.db`, y secreto con `permiteVacio: true`, en ese orden. Archivo inválido repite ese campo, no toda la configuración. Si el secreto resulta de longitud cero, informa que las altas quedan deshabilitadas. Devuelve un record con los tres resultados. EOF y errores de lectura se propagan al Program; no abre SQLite ni comprueba permiso efectivo de escritura.

`private static bool ArchivoValido(string valor)` obtiene `Path.GetFullPath`, luego exige que no haya caracteres de control, que la ruta no sea un directorio y que su directorio padre exista. Acepta archivo existente o aún no creado. Captura `ArgumentException`, `IOException`, `NotSupportedException` y `UnauthorizedAccessException`, devolviendo false; otros errores no se convierten en false.

Fragmento de `ArchivoValido` que sustenta aceptación y rechazo:

```csharp
            string ruta = Path.GetFullPath(valor);
            return !valor.Any(char.IsControl) && !Directory.Exists(ruta) &&
                Directory.Exists(Path.GetDirectoryName(ruta));
```

El `&&` corta al primer requisito falso. No hay prueba de escritura ni creación de directorios.

### Fragmentos de lectura y recuperación

Leer aplica valores predeterminados y permite secreto vacío únicamente para deshabilitar administración. No abre base durante estas preguntas:

```csharp
        var lector = new EntradaConsola(entrada, salida);
        string direccion = lector.Url("http://0.0.0.0:5000");
        string archivo = lector.Validar("Archivo SQLite (Enter: guerra.db)", ArchivoValido,
            "Ingresá una ruta de archivo válida en un directorio existente.", "guerra.db");
        string credencial = lector.Secreto("Credencial de catálogo (Enter: deshabilitar altas)", permiteVacio: true);
        if (credencial.Length == 0)
            salida.WriteLine("Credencial vacía: alta de personajes deshabilitada.");
        return new(direccion, archivo, credencial);
```

ArchivoValido convierte únicamente estas excepciones de ruta en false; el lector volverá a preguntar el campo:

```csharp
        catch (Exception error) when (error is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
        {
            return false;
        }
```

## Recorridos y ejemplos

Normal: dos Enter y secreto ficticio `catalogo-demo` producen URL por defecto y `guerra.db` si el directorio actual existe. Alternativo: indicar un directorio como archivo devuelve false y repite la pregunta; un secreto vacío termina correctamente e informa administración deshabilitada. EOF durante las preguntas impide devolver una configuración completa. Recorridos contrastados por lectura, sin abrir servicios reales.

## Errores, límites y efectos observables

Solo escribe preguntas/diagnósticos y consume entrada. Un directorio existente puede no ser escribible: el fallo de apertura se gestiona luego en Program. No registra el secreto ni lo incorpora al diagnóstico de ruta.

## Relaciones y verificación

[ConfiguracionServidorTests](../../../test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md) define valores predeterminados, configuración personalizada, corrección y EOF. [EntradaProcesosContratoTests](../../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) comprueba reintento ante base no utilizable. Una prueba definida no acredita su ejecución; consultar [evidencia](../../../../plan-de-pruebas.md).
