# EntradaConsola.cs

Fuente: [src/comun/EntradaConsola.cs](../../../../src/comun/EntradaConsola.cs). Proyecto: fuente enlazado por cliente, servidor y subir-personajes. Namespace: `Proyecto_Integrador_Final.Consola`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Lector compartido por las consolas administrativas del servidor y la carga. Centraliza preguntas, reintento por campo, URL sin prefijo y secreto oculto. Está enlazado en los tres csproj; el cliente REST usa su propio lector de configuración con políticas distintas.

## Dependencias

`TextReader`, `TextWriter`, `Console`, `StringBuilder`, `Uri`, LINQ `Any` y excepciones de System. Lo usan configuración del servidor y herramienta subir-personajes; [ConfiguracionCliente](../cliente/ConfiguracionCliente.md) admite prefijos y no llama a este lector para su configuración. Véase [instalación y uso](../../../instalacion-y-uso.md).

## Estructura y datos

`internal sealed class EntradaConsola(TextReader entrada, TextWriter salida)`, constructor primario captura ambos flujos sin validarlos ni asumir propiedad de ellos. Sin herencia explícita, interfaces, atributos, partial, campos declarados adicionales, propiedades, eventos ni liberación de recursos. Todo es síncrono; no acepta token.

## Miembros y ejecución

- `internal string Texto(string pregunta)`: imprime pregunta por WriteLine y lee una línea; conserva espacios, retorna string, null produce EndOfStreamException.
- `internal string Validar(string pregunta, Func<string,bool> valida, string error, string? defecto = null)`: bucle sin límite de intentos. Texto seguido de Trim; si queda vacío y defecto no null, sustituye por defecto (sin recortarlo de nuevo). Retorna al aceptar predicado; si rechaza imprime error y repite el campo. Excepciones del predicado no se capturan.
- `internal string Url(string defecto)`: usa Validar y pregunta con valor por defecto; Enter lo aplica. `private static bool EsUrl(string valor)` exige Uri absoluta HTTP/HTTPS, host no vacío, sin UserInfo/query/fragmento y AbsolutePath exactamente `/`. No acepta prefijos como `/juego/` y no intenta conexión.
- `internal string Secreto(string pregunta, bool permiteVacio)`: repite pregunta; si entrada es Console.In y stdin no redirigido usa LeerSecreto, si no ReadLine o EOF. Acepta blanco solo si permiteVacio; lo normaliza a `""`. Un secreto no blanco exige cero caracteres de control y cero whitespace, sin Trim. Rechazo imprime un mensaje fijo y repite.
- `private static string LeerSecreto()`: StringBuilder, ReadKey(intercept:true) sin eco. Enter imprime salto y retorna; Ctrl+D/Ctrl+Z lanzan EOF; Backspace borra el último carácter si existe; otros no controles se agregan. No muestra asteriscos. El ciclo termina solo por Enter o EOF; no es cancelable por token.

Fragmento de `Validar`, con sustitución antes del predicado:

```csharp
            string valor = Texto(pregunta).Trim();
            if (valor.Length == 0 && defecto is not null)
                valor = defecto;
            if (valida(valor))
                return valor;
            salida.WriteLine(error);
```

Fragmento de la aceptación de secretos, sin exponer su contenido:

```csharp
            if ((permiteVacio && string.IsNullOrWhiteSpace(valor)) ||
                (!string.IsNullOrWhiteSpace(valor) && !valor.Any(char.IsControl) && !valor.Any(char.IsWhiteSpace)))
                return string.IsNullOrWhiteSpace(valor) ? "" : valor;
            salida.WriteLine("Ingresá una credencial no vacía, sin espacios ni caracteres de control.");
```

## Recorridos y ejemplos

Normal: Url con defecto `http://localhost:5000`, línea vacía, retorna ese destino. Secreto con `plaza-demo` lo retorna sin mostrarlo en terminal. Texto devuelve literalmente una línea como `  crear  `; quien llame decide si recorta.

Alternativo: URL `http://localhost:5000/api` repite por ruta distinta de `/`. Secreto `dos palabras` se rechaza; blanco con permiteVacio=true retorna vacío y puede deshabilitar altas en el servidor. EOF interrumpe sin devolver un dato incompleto.

## Errores, límites y efectos observables

Imprime preguntas y errores, no realiza HTTP ni persiste. Los flujos pueden lanzar IOException. No limita longitud de secretos ni reintentos. El modo oculto solo se activa con el objeto Console.In exacto; con un lector envuelto no usa captura de teclas. No dispone los flujos ajenos.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### Texto, Url y EsUrl

Texto conserva espacios; Validar los recorta y aplica defecto; EsUrl restringe camino raíz. Estos contratos no son idénticos a los del cliente REST.

```csharp
    internal string Texto(string pregunta)
    {
        salida.WriteLine(pregunta);
        return entrada.ReadLine() ?? throw new EndOfStreamException();
    }

    internal string Validar(string pregunta, Func<string, bool> valida, string error, string? defecto = null)
    {
        while (true)
        {
            string valor = Texto(pregunta).Trim();
            if (valor.Length == 0 && defecto is not null)
                valor = defecto;
            if (valida(valor))
                return valor;
            salida.WriteLine(error);
        }
    }

    internal string Url(string defecto) => Validar($"Dirección HTTP (Enter: {defecto})",
        EsUrl, "Ingresá una dirección HTTP o HTTPS válida, sin credenciales, consulta ni fragmento.", defecto);

    private static bool EsUrl(string valor) =>
        Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https" && !string.IsNullOrEmpty(uri.Host) &&
        uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0 &&
        uri.AbsolutePath == "/";

    internal string Secreto(string pregunta, bool permiteVacio)
```

### LeerSecreto: terminal sin eco

ReadKey intercepta, Enter devuelve, controles EOF lanzan y Backspace modifica solo memoria. No imprime el secreto ni asteriscos.

```csharp
    private static string LeerSecreto()
    {
        var texto = new StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return texto.ToString();
            }
            if (tecla.KeyChar is '\u0004' or '\u001a')
                throw new EndOfStreamException();
            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (texto.Length > 0) texto.Length--;
            }
            else if (!char.IsControl(tecla.KeyChar))
                texto.Append(tecla.KeyChar);
        }
    }
```

## Relaciones y verificación

[ConfiguracionServidorTests](../../test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md), [ConsolaAltaTests](../../test/proyecto-integrador-final-tests/ConsolaAltaTests.md) y [EntradaProcesosContratoTests](../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) ejercitan corrección, EOF y configuración. Los lectores en memoria no prueban por sí solos ocultación física de terminal. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
