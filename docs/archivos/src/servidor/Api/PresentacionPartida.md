# PresentacionPartida.cs

Fuente: [src/servidor/Api/PresentacionPartida.cs](../../../../../src/servidor/Api/PresentacionPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Construye el estado visible para una plaza autenticada, sin revelar la mano rival ni cartas de combates incompletos. Es la porción de lectura de `internal sealed partial RepositorioJuego`, junto a [repositorio](RepositorioJuego.md) y [transiciones](TransicionesPartida.md). La presencia puede cambiar Pausada sin modificar revisión persistida.

## Dependencias

[EstadoPartida](EstadoPartida.md) proporciona CartaVisible y entidades; [BaseJuego](BaseJuego.md) permite consultas EF. `System.Text.Json.Serialization.JsonIgnore` omite dos campos de cierre cuando son null. LINQ ordena y agrupa historial. [Protocolo](../../../../protocolo-de-partida.md) describe privacidad y orden de resultados.

## Estructura

Además del método de la clase parcial declara tres `internal sealed record` posicionales. Constructor, propiedades públicas init e igualdad se generan; no hay setters con lógica, eventos ni funciones locales. El record no hace copia profunda de arrays o diccionarios.

## Estado y datos

| Record | Propiedades completas |
|---|---|
| SolicitudVisible | Guid IdSolicitud; string Tipo; int[] IdsPersonajePermitidos; string[] AtributosPermitidos. Solo se entrega a quien debe elegir. |
| ResultadoVisible | int NumeroRonda/NumeroCombate; Dictionary<string,CartaVisible> Cartas; string Atributo; Dictionary<string,int> Puntajes; string GanadorCombate; string? GanadorRonda. Mapas indexados por J1/J2. |
| PresentacionVisible | Guid IdPartida; int Revision; string Fase; bool Pausada; string RolPropio; Dictionary<string,string?> Nombres; int? NumeroRonda/NumeroCombate; Dictionary<string,int> Marcador; CartaVisible[] Mano; SolicitudVisible? Solicitud; ResultadoVisible[] Resultados; string? MotivoFin/Ganador. |

MotivoFin y Ganador llevan `[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`: se omiten solo al ser null. Otros anulables no tienen ese atributo. No hay campos de estado nuevos en la porción parcial; las colecciones temporales duran una lectura.

## Miembros y ejecución

`internal PresentacionVisible Presentacion(Guid id,string? credencial,Guid idSesion)` llama Ejecutar(false,...), por tanto abre una transacción de lectura y lock sin SaveChanges. Dentro:

1. Acceso exige existencia, credencial y sesión propia vigente. Calcula Pausada según ambos presentes y fase.
2. Consulta participantes de la partida y cartas ordenadas por Orden. Filtra mano por Ubicacion == rol propio y proyecta Vista. No incluye la reserva ni cartas en jugada dentro de Mano.
3. Inicia visible=null. Solo si no pausada y hay IdSolicitud busca la fila; solo si Rol coincide crea SolicitudVisible con IDs de la mano propia. Para cartaYAtributo incluye las seis claves canónicas del mapa; para respuesta carta devuelve atributos vacíos. No entrega solicitud al observador de turno ajeno.
4. Consulta combates de la partida con Ganador no null, ordenados primero por ronda y luego número. Obtiene ganadores de rondas ya definidas y el último combate resuelto por ronda. Crea diccionario de cartas por ID para reconstruir resultados con ambas cartas y puntajes.
5. Por cada combate resuelto arma ResultadoVisible. GanadorRonda solo se incluye en el último combate resuelto de su ronda y si la ronda está definida; los anteriores llevan null. Devuelve todos los resultados históricos resueltos, no solo los nuevos para un cliente.
6. Retorna presentación con revisión/fase persistidas, pausa calculada, rol, nombres, números actuales, marcador de rondas, mano propia, solicitud visible, resultados y cierre. No renueva presencia ni incrementa revisión.

Fragmento real de la condición de solicitud:

```csharp
            SolicitudVisible? visible = null;
            if (!pausada && partida.IdSolicitud is Guid idSolicitud)
            {
                var solicitud = db.Solicitudes.Find(idSolicitud)!;
                if (solicitud.Rol == jugador.Rol)
                    visible = new(solicitud.Id, solicitud.Tipo, mano.Select(c => c.IdPersonaje).ToArray(),
                        solicitud.Tipo == "cartaYAtributo" ? atributos.Keys.ToArray() : []);
            }
```

Fragmento de selección y orden del historial; excluye elecciones aún pendientes:

```csharp
            var historial = db.Combates.Where(c => c.IdPartida == id && c.Ganador != null)
                .OrderBy(c => c.NumeroRonda).ThenBy(c => c.Numero).ToArray();
            var ganadores = db.Rondas.Where(r => r.IdPartida == id && r.Ganador != null)
                .ToDictionary(r => r.Numero, r => r.Ganador);
            var ultimos = historial.GroupBy(c => c.NumeroRonda).ToDictionary(g => g.Key, g => g.Last().Numero);
            var cartasPorId = cartas.ToDictionary(c => c.IdPersonaje);
```

Las lambdas de proyección usan `Otro(c.Elector)` para asociar segunda carta y puntaje al rival, no a un orden fijo de J1/J2. Los `!.Value` presuponen que un combate con ganador tiene cartas y puntajes completos. Las claves duplicadas o referencias faltantes revelarían una base inconsistente mediante excepción, no se silencian.

### Fragmento de proyección de resultados y vista final

Cada resultado asigna carta/puntaje por rol del elector y su opuesto. La condición del último combate liga GanadorRonda solo al cierre de esa ronda; luego se combinan nombres/marcador y mano propia sin secreto:

```csharp
            var resultados = historial.Select(c => new ResultadoVisible(
                c.NumeroRonda, c.Numero,
                new()
                {
                    [c.Elector] = cartasPorId[c.IdCartaElector!.Value].Vista(),
                    [Otro(c.Elector)] = cartasPorId[c.IdCartaRival!.Value].Vista()
                },
                c.Atributo!,
                new()
                {
                    [c.Elector] = c.PuntajeElector!.Value,
                    [Otro(c.Elector)] = c.PuntajeRival!.Value
                },
                c.Ganador!,
                ultimos[c.NumeroRonda] == c.Numero ? ganadores.GetValueOrDefault(c.NumeroRonda) : null))
                .ToArray();

            return new PresentacionVisible(id, partida.Revision, partida.Fase, pausada, jugador.Rol,
                participantes.ToDictionary(p => p.Rol, p => p.Nombre), partida.NumeroRonda,
                partida.NumeroCombate, participantes.ToDictionary(p => p.Rol, p => p.RondasGanadas),
                mano, visible, resultados, partida.MotivoFin, partida.Ganador);
```

## Recorridos y ejemplos

Normal: J1 espera elegir y ambos presentes; su Mano tiene tres cartas, Solicitud incluye sus tres IDs y seis atributos. J2 recibe su propia mano y Solicitud null. Tras resolver combate 1 ambos ven las dos cartas del combate en Resultados ordenados; las cartas restantes del rival siguen fuera de Mano.

Alternativo: vence presencia de J2 durante esperandoElector; J1 aún autenticado recibe Pausada true y Solicitud null con la misma Revision. Si consultara J2 con su sesión vencida, falla Acceso antes de proyectar. En esperandoInicio, números pueden ser null e historial vacío; no hay carta que revelar. Recorridos de lectura, no requests ejecutados aquí.

## Errores, límites y efectos observables

Errores de acceso son 404/401/409 desde helpers. No hay escritura ni secreto dentro del payload. El historial crece con toda la partida; este método no pagina ni acepta revisión desde la cual leer. `no-store` lo agrega el handler HTTP, no este método. Los diccionarios y arrays son nuevos para la respuesta, pero el código no aplica saneamiento terminal: lo hace el cliente al presentar.

## Relaciones y verificación

[ApiHttpTests](../../../test/proyecto-integrador-final-tests/ApiHttpTests.md) define elecciones privadas y presencia sin cambio de revisión; [ServidorContratoTests](../../../test/proyecto-integrador-final-tests/ServidorContratoTests.md) define visibilidad por plaza, pausa y misma solicitud tras recuperar. [EstadoCliente](../../cliente/EstadoCliente.md) decide qué resultados nuevos mostrar. Véase [plan de pruebas](../../../../plan-de-pruebas.md) para evidencia de ejecución.
