# ConsolaServidor.cs

Fuente: [src/servidor/Api/ConsolaServidor.cs](../../../../../src/servidor/Api/ConsolaServidor.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Expone operaciones administrativas por entrada/salida del operador local, con preguntas separadas para UUID, plaza o personaje. [Program](../Program.md) ejecuta su bucle junto al host. No ofrece un protocolo de consola para los jugadores: ellos usan HTTP.

## Dependencias

[RepositorioJuego](RepositorioJuego.md) crea/cancela partidas, revoca sesiones y desactiva cartas; [transiciones](TransicionesPartida.md) inicia. [EntradaConsola](../../comun/EntradaConsola.md) valida y repite preguntas. SqliteException y DbUpdateException producen diagnóstico de almacenamiento. [Protocolo](../../../../protocolo-de-partida.md) describe operación del servidor.

## Estructura y estado

`internal static class ConsolaServidor`, sin instancia, partial, eventos o interfaces. Campo privado estático readonly `Dictionary<string,(string Grupo,string Descripcion)> comandos`: crear/iniciar/cancelar en Partidas; cerrar-sesion/reemplazar-credencial en Jugadores; desactivar en Catálogo; ayuda en Ayuda. El diccionario conserva textos para mostrar ayuda, no delegados que ejecuten comandos; el switch de Comando hace el despacho. No se modifica después de inicializar.

## Miembros y ejecución

`internal static void Ejecutar(RepositorioJuego juego,TextReader entrada,TextWriter salida,CancellationToken detener)` imprime Ayuda y repite mientras no haya cancelación y ReadLine devuelva línea. Vuelve a comprobar cancelación después de leer para no aplicar una línea recibida al detener. Pasa `linea.Trim()` a Comando. EOF durante preguntas produce EndOfStreamException capturada y retorno sin ejecutar la acción incompleta; ErrorApi imprime Message y continúa con próximo comando; SqliteException o DbUpdateException imprimen «El almacenamiento no está disponible.» y continúan. Otras excepciones se propagan. Una cancelación no interrumpe una ReadLine ya bloqueada: se comprueba al recuperar control.

`private static void Comando(RepositorioJuego juego,string linea,TextReader entrada,TextWriter salida)` pasa comando a minúsculas invariantes, crea lector, inicia id=Guid.Empty y rol=null. Para iniciar/cancelar/cerrar-sesion/reemplazar-credencial pregunta UUID, exige TryParse y no Empty y repite si falla; después Parse es seguro para la cadena aceptada. Para cerrar/reemplazar pregunta rol con aceptación J1/J2 ignorando caso y almacena mayúsculas. Para desactivar pide entero positivo. No mezcla los argumentos con la línea del comando: escribir `iniciar UUID` se trata como comando desconocido.

Fragmento real de las precondiciones de preguntas:

```csharp
        if (comando is "iniciar" or "cancelar" or "cerrar-sesion" or "reemplazar-credencial")
            id = Guid.Parse(lector.Validar("UUID de la partida", v => Guid.TryParse(v, out var g) && g != Guid.Empty,
                "Ingresá un UUID válido y distinto de cero."));
        if (comando is "cerrar-sesion" or "reemplazar-credencial")
            rol = lector.Validar("Plaza: J1 o J2", v => v.ToUpperInvariant() is "J1" or "J2",
                "Elegí J1 o J2.").ToUpperInvariant();
        string argumento = comando == "desactivar"
            ? lector.Validar("ID numérico del personaje", v => int.TryParse(v, out int n) && n > 0,
                "Ingresá un ID numérico positivo.") : "";
```

Despacho y efectos del switch:

| Comando | Acción y salida |
|---|---|
| cadena vacía | No hace nada; vuelve al bucle. |
| ayuda | Reimprime Ayuda. |
| crear | CrearPartida; imprime `Partida {UUID}`, `J1 {credencial}` y `J2 {credencial}`. Las credenciales se revelan aquí intencionalmente al operador. |
| iniciar | Iniciar(id); si acepta imprime «Partida iniciada.». |
| cancelar | Cancelar(id); imprime «Partida cancelada.». |
| cerrar-sesion | CerrarSesion(id,rol); imprime «Sesión cerrada.». |
| reemplazar-credencial | ReemplazarCredencial(id,rol); imprime rol y nuevo secreto. |
| desactivar | Si argumento parsea int, Desactivar; imprime «Personaje desactivado.». La pregunta ya validó positivo. |
| cualquier otro | Imprime comando desconocido y sugiere ayuda; no llama al repositorio. |

`private static void Ayuda(TextWriter salida)` imprime título e instrucciones crear/iniciar, agrupa comandos por Grupo con GroupBy, recorre cada grupo y luego cada entrada mostrando nombre y descripción. Termina con instrucciones de responder preguntas y EOF. Las iteraciones son finitas sobre siete entradas; no interactúan con la base ni consumen entrada.

Fragmento real del bucle de ayuda:

```csharp
        foreach (var grupo in comandos.GroupBy(comando => comando.Value.Grupo))
        {
            salida.WriteLine();
            salida.WriteLine(grupo.Key);
            foreach (var comando in grupo)
            {
                salida.WriteLine($"  {comando.Key}");
                salida.WriteLine($"    {comando.Value.Descripcion}");
            }
        }
```

### Fragmentos de despacho y control del bucle

Ejecutar vuelve a comprobar cancelación después de leer; EOF en una pregunta retorna, mientras ErrorApi permite continuar. El diagnóstico de base no imprime detalles internos:

```csharp
        while (!detener.IsCancellationRequested && entrada.ReadLine() is string linea)
        {
            if (detener.IsCancellationRequested)
                break;

            try
            {
                Comando(juego, linea.Trim(), entrada, salida);
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (ErrorApi error)
            {
                salida.WriteLine(error.Message);
            }
            catch (Exception error) when (error is SqliteException or DbUpdateException)
            {
                salida.WriteLine("El almacenamiento no está disponible.");
            }
        }
```

Estos casos del switch vinculan comandos aceptados con efectos reales: crear revela credenciales al operador; iniciar/cancelar solo imprimen éxito después del retorno del repositorio; cerrar/reemplazar afectan sesión/credencial; desactivar actúa sobre catálogo:

```csharp
            case "crear":
                var partida = juego.CrearPartida();
                salida.WriteLine($"Partida {partida.Id}");
                salida.WriteLine($"J1 {partida.CredencialJ1}");
                salida.WriteLine($"J2 {partida.CredencialJ2}");
                break;
            case "iniciar":
                juego.Iniciar(id);
                salida.WriteLine("Partida iniciada.");
                break;
            case "cancelar":
                juego.Cancelar(id);
                salida.WriteLine("Partida cancelada.");
                break;
            case "cerrar-sesion":
                juego.CerrarSesion(id, rol!);
                salida.WriteLine("Sesión cerrada.");
                break;
            case "reemplazar-credencial":
                salida.WriteLine($"{rol} {juego.ReemplazarCredencial(id, rol!)}");
                break;
            case "desactivar" when int.TryParse(argumento, out int personaje):
                juego.Desactivar(personaje);
                salida.WriteLine("Personaje desactivado.");
                break;
            default:
                salida.WriteLine("Comando desconocido. Escribí ayuda para ver los comandos.");
                break;
```

## Recorridos y ejemplos

Normal: escribir `crear` produce UUID P y dos credenciales; tras incorporar ambas plazas y cargar seis cartas, escribir `iniciar`, luego P, inicia y confirma. Entrada ` CERRAR-SESION ` se recorta y baja a minúsculas; pregunta UUID y plaza, acepta `j1` y la convierte a J1 antes de cerrar.

Alternativo: `cancelar`, UUID inválido, UUID válido reintenta solo el dato y finalmente cancela. EOF mientras espera UUID cierra sin Cancelar. `iniciar P` en una sola línea imprime desconocido. ErrorApi de partida inexistente imprime su mensaje y permite otro comando. Recorridos de lectura, no administración de servicios reales.

## Errores, límites y efectos observables

Hay escritura persistida cuando el repositorio la ejecuta y cambios de sesión en memoria para cerrar/reemplazar. No imprime stack ni errores SQLite internos. Solo captura los fallos enumerados. Una cancelación durante lectura no puede cerrar el flujo por sí misma; Program coordina parada del host.

## Relaciones y verificación

[ConsolaServidorTests](../../../test/proyecto-integrador-final-tests/ConsolaServidorTests.md) define ayuda, corrección de UUID/plaza y EOF antes de confirmar. [EntradaProcesosContratoTests](../../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) ejercita entrada del proceso real. La suite de referencia se registra en [verificación spec 004](../../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004); no equivale a ensayo de administración de un servidor real de producción.
