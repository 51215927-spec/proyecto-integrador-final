# ConfiguracionCliente.cs

Fuente: [src/cliente/ConfiguracionCliente.cs](../../../../src/cliente/ConfiguracionCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Recoge y valida los cuatro datos de conexión antes de crear el transporte REST. La corrección vuelve a pedir solo el campo inválido; no envía solicitudes HTTP ni crea partidas.

## Dependencias

`TextReader`/`TextWriter` permiten usar consola o lectores en memoria. `Console` permite captura sin eco para el secreto; `StringBuilder` acumula caracteres; `Uri` y `Guid` verifican formatos. [Program](Program.md) llama a la lectura y [ClientePartidaHttp](ClientePartidaHttp.md) consume el resultado. Véase [instalación y uso](../../../instalacion-y-uso.md).

## Estructura y datos

`internal sealed record ConfiguracionCliente(Uri Url, Guid Partida, string Credencial, string Nombre)`. El constructor posicional y las propiedades init generadas almacenan exactamente esos valores; construir directamente el record no ejecuta las validaciones de lectura. Record aporta igualdad por valor y copia `with`; no tiene interfaces explícitas ni partial. Sus métodos de lectura son internal static; `ToString` es public override. No hay eventos ni persistencia local.

## Miembros y ejecución

- `Leer(TextReader entrada, TextWriter salida)` retorna ConfiguracionCliente bloqueando en `LeerAsync(..., default).GetAwaiter().GetResult()`. Reutiliza las mismas validaciones; propaga EOF y fallos de E/S.
- `LeerAsync(TextReader entrada, TextWriter salida, CancellationToken ct)` retorna Task del record. Pide URL, UUID no vacío, credencial y nombre, en ese orden. Cada valor se recorta por fuera. El UUID usa `Guid.TryParse` y no puede ser Guid.Empty. Credencial no vacía ni con controles; permite espacios internos. Nombre no blanco, conserva mayúsculas y espacios internos. La URL final elimina barras finales y añade exactamente una para concatenar rutas relativas.
- Función local `async Task<string> Campo(string pregunta, Func<string,bool> valida, string error, bool secreto = false)`: repite pregunta hasta validar. Si el lector es exactamente Console.In y stdin no está redirigido, espera teclas cada 25 ms con cancelación. Enter termina, Ctrl+D/Ctrl+Z lanzan EndOfStreamException, Backspace elimina si hay caracteres y solo borra visualmente en campos visibles; caracteres no de control se agregan y solo se imprimen si no es secreto. Otros controles se ignoran. Si hay tubería u otro lector, usa ReadLineAsync(ct); null lanza EOF. Tras Trim, devuelve si el predicado acepta o imprime el error y repite. El callback del UUID convierte y compara; los otros callbacks aplican las restricciones anteriores.
- `EsUrl(string valor)` devuelve bool: absoluta, HTTP o HTTPS, host presente, sin usuario/contraseña, query ni fragmento. Permite prefijo de ruta (a diferencia del lector común del servidor). No prueba conectividad.
- `public override string ToString()` retorna únicamente `Partida {Partida}`; no incluye credencial, URL ni nombre.

Fragmento de la secuencia final de `LeerAsync`; los bucles de Campo ya se explicaron:

```csharp
        string url = await Campo("URL base HTTP/HTTPS del servidor", EsUrl,
            "Ingresá una URL absoluta HTTP/HTTPS sin credenciales, consulta ni fragmento.");
        string id = await Campo("UUID de partida", v => Guid.TryParse(v, out var g) && g != Guid.Empty,
            "Ingresá un UUID distinto de vacío.");
        // El nombre conserva mayúsculas y espacios internos: el servidor exige coincidencia al retomar.
        string credencial = await Campo("Credencial de plaza", v => !string.IsNullOrWhiteSpace(v) && !v.Any(char.IsControl),
            "Ingresá una credencial no vacía y sin controles.", true);
        string nombre = await Campo("Nombre registrado (debe coincidir exactamente al retomar)",
            v => !string.IsNullOrWhiteSpace(v), "Ingresá un nombre no vacío.");
        return new(new Uri(url.TrimEnd('/') + "/"), Guid.Parse(id), credencial, nombre);
```

El sufijo `/` conserva un prefijo como `/juego/` al combinarlo con rutas del cliente. La validación de URL es:

```csharp
    internal static bool EsUrl(string valor) => Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https" && uri.Host.Length > 0 && uri.UserInfo.Length == 0 &&
        uri.Query.Length == 0 && uri.Fragment.Length == 0;
```

## Recorridos y ejemplos

Normal: `https://ejemplo.test/juego`, UUID `11111111-1111-1111-1111-111111111111`, credencial ficticia `plaza-demo`, nombre ` Ana María ` producen URL `https://ejemplo.test/juego/`, UUID parseado, secreto intacto y nombre `Ana María`. No hay HTTP todavía.

Alternativo: `ftp://ejemplo.test` repite solo URL. Después de URL válida, UUID vacío repite solo UUID. EOF mientras se pide el nombre lanza sin devolver configuración parcial; Ctrl+C cancela mediante el token del arranque. Una credencial escrita en terminal no aparece en eco; un lector redirigido es consumido línea por línea.

## Errores, límites y efectos observables

Imprime preguntas y errores controlados; no diagnostica contenidos secretos. El constructor directo y las copias `with` pueden contener valores que la lectura rechazaría. No comprueba longitud de nombre ni privilegios en el servidor. No tiene transacciones ni sincronización; su asincronía sirve a lectura y cancelación.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### Campo: teclado oculto y cancelable

Este fragmento de la función local muestra los dos bucles, los 25 ms cancelables, EOF físico, Backspace y eco condicionado por secreto. Tras Trim el predicado acepta o repite el campo.

```csharp
        // Cada campo se corrige por separado para conservar los valores ya aceptados.
        async Task<string> Campo(string pregunta, Func<string, bool> valida, string error, bool secreto = false)
        {
            while (true)
            {
                salida.WriteLine(pregunta);
                string valor;
                if (ReferenceEquals(entrada, Console.In) && !Console.IsInputRedirected)
                {
                    // La lectura por teclas permite ocultar el secreto y cancelar sin bloquear ReadLine.
                    var texto = new StringBuilder();
                    while (true)
                    {
                        while (!Console.KeyAvailable) await Task.Delay(25, ct);
                        var tecla = Console.ReadKey(true);
                        if (tecla.Key == ConsoleKey.Enter) { salida.WriteLine(); break; }
                        if (tecla.KeyChar is '\u0004' or '\u001a') throw new EndOfStreamException();
                        // El borrado modifica el valor; solo se refleja en pantalla para campos visibles.
                        if (tecla.Key == ConsoleKey.Backspace)
                        {
                            if (texto.Length > 0) { texto.Length--; if (!secreto) salida.Write("\b \b"); }
                        }
                        else if (!char.IsControl(tecla.KeyChar)) { texto.Append(tecla.KeyChar); if (!secreto) salida.Write(tecla.KeyChar); }
                    }
                    valor = texto.ToString().Trim();
                }
                // En una tubería se consume una línea por campo y EOF cancela la configuración.
                else valor = (await entrada.ReadLineAsync(ct) ?? throw new EndOfStreamException()).Trim();
                if (valida(valor)) return valor;
                salida.WriteLine(error);
            }
```

## Relaciones y verificación

[ConfiguracionClienteTests](../../test/proyecto-integrador-final-tests/ConfiguracionClienteTests.md) parametriza formatos URL y verifica recorte, conservación de nombre y repetición por campo. [ArranqueRestTests](../../test/proyecto-integrador-final-tests/ArranqueRestTests.md) cubre el ejecutable. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
