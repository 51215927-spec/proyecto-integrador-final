# InterfazConsola.Red.cs

Fuente: [src/cliente/InterfazConsola.Red.cs](../../../../src/cliente/InterfazConsola.Red.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Adapta una presentación REST y el estado local de interacción a paneles de consola. Prioriza menú, resultado en lectura, final y selección actual, sin pedir teclas ni realizar red. Los ganadores vienen del servidor, no de una comparación local de puntajes.

## Dependencias

Es la segunda porción de [InterfazConsola](InterfazConsola.md): reutiliza Encabezado, Paneles, PanelInfo, ListaOpciones, MostrarCombate, MostrarMano, Escribir y Pie. Consume [ContratosPartida](ContratosPartida.md); lo invoca [SesionPartidaCliente](SesionPartidaCliente.md). Véase [diseño de interfaz](../../../diseno-de-interfaz.md).

## Estructura y datos

`internal sealed partial class InterfazConsola`, sin campos, propiedades ni eventos nuevos. La herencia y atributos son los de la porción principal (ninguno explícito). Este archivo aporta un solo método internal con funciones locales. No conserva estado entre llamadas.

## Miembros y ejecución

`internal void MostrarRed(string local, PresentacionCliente? vista, bool actual, string aviso, ResultadoCliente? resultado, int menu, bool confirma, int foco, bool atributo, bool pendiente, bool abandono)` presenta y retorna void. local y aviso son textos, vista puede faltar; actual indica si se ha confirmado vigencia, no si hay una vista guardada. menu<0 significa cerrado; confirma selecciona No/Sí; foco selecciona carta o atributo; pendiente/abandono bloquean elección.

Función local `string Nombre(string rol)` usa Nombres de vista o el propio rol si no existe/está null. Resuelve roles, por lo que nombres iguales no identifican al ganador. El estado de encabezado se elige en orden: vista finalizada→Final confirmado; no actual→sin comunicación; vista pausada→pausa confirmada; resto→En línea. Añade aviso.

Orden de ramas y efectos:

1. menu≥0: dibuja menú o confirmación No/Sí con foco menu. Si abandono incierto imprime advertencia; pie de controles y return. Esta rama tiene precedencia incluso sobre un resultado o final.
2. vista null: pie Esc/Ctrl+C y return, sin acceder a datos de partida.
3. resultado no null: toma todos los resultados anteriores o iguales por coordenadas. Funciones locales `int Combates(string rol)` cuentan victorias de esa ronda hasta ese combate; `int Rondas(string rol)` cuenta ganadores de ronda hasta ese punto. Imprime coordenadas; MostrarCombate recibe cartas y puntajes revelados, ganador por rol, contador histórico y GanaJ1 explícito. Si GanadorRonda no null lo anuncia. Imprime marcador histórico y pie Enter/Esc, return. La revisión presente puede estar mucho más adelantada: no se mezcla su marcador.
4. Estado vigente: imprime nombres con J1/J2 y rol propio, y rondas del mapa Marcador. Si finalizada, victoria muestra Ganador; abandono muestra que no suma victoria; cualquier otro motivo se presenta como cancelación del operador (EstadoCliente ya restringe valores). Pie Enter para salir y return.
5. Si esperandoInicio, indica que depende del operador; si no, imprime coordenadas y cuenta combates ganados de la ronda actual.
6. Solo habilita selección si actual, no pausada, no pendiente, no abandono y solicitud no null. En atributo muestra opciones remotas de atributos con foco; en carta muestra texto de respuesta/apertura y cartas ordenadas según IDs permitidos, cada una resuelta con Single sobre la mano. Pie de controles/Esc.
7. Si no habilita selección, muestra mano completa propia. Si pendiente avisa recuperación; si no, actual y no pausada y no esperandoInicio, avisa que espera solicitud. Pie permite repintar estado o salir.

Fragmento de precedencia de menú y ausencia de vista; el encabezado lo precede:

```csharp
        if (menu >= 0)
        {
            Paneles(PanelInfo(confirma ? "¿Abandonar partida?" : "Menú",
                ListaOpciones(confirma ? ["No", "Sí"] : ["Continuar", "Salir sin abandonar", "Abandonar partida"], menu)));
            if (abandono) Escribir("Abandono sin confirmar. Salir localmente no confirma el abandono.");
            Pie(Controles); return;
        }
        if (vista is null) { Pie("Esc: menú · Ctrl+C: salir sin abandonar"); return; }
```

Fragmento de selección: el estado anterior ya fue dibujado; el `else` de espera lo sigue:

```csharp
        bool habilitada = actual && !vista.Pausada && !pendiente && !abandono && vista.Solicitud is not null;
        if (habilitada && vista.Solicitud is { } s)
        {
            if (atributo) Paneles(PanelInfo("Atributo del combate", ListaOpciones(s.AtributosPermitidos, foco)));
            else
            {
                Escribir(s.Tipo == "carta" ? "Te toca responder" : "Tu turno: carta y atributo");
                // El foco recorre exclusivamente las cartas incluidas en las opciones remotas.
                MostrarMano(s.IdsPersonajePermitidos.Select(id => vista.Mano.Single(c => c.IdPersonaje == id)).ToArray(), foco);
            }
            Pie(Controles + "\nEsc: menú");
```

Las opciones no se reconstruyen del catálogo local: la presentación remota manda el subconjunto y su orden.

## Recorridos y ejemplos

Normal: vista actual esperandoElector, solicitud cartaYAtributo IDs [7,9], foco 1, atributo=false: resuelve ambas cartas propias en ese orden y destaca ID9. Tras elegir carta, atributo=true presenta atributos permitidos en vez de la mano.

Alternativo: vista guardada pero actual=false nunca ofrece elección; presenta sin comunicación y última mano. Resultado (2,1) mientras vista ya está en ronda 3 muestra el marcador acumulado solo hasta (2,1). Ambos nombres Ana se distinguen para ganador usando rol J1/J2. Menú de abandono inicia en No por estado del coordinador.

## Errores, límites y efectos observables

Solo imprime. Confía en que EstadoCliente verificó claves, IDs y datos; Single lanza si faltan o se duplican cartas en mano. No vuelve a validar índice foco ni menu; el coordinador los limita. Los ayudantes principales neutralizan controles remotos y usan Text en vez de interpretar markup. No consulta HTTP ni cambia DTO, estado o marcador.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### MostrarRed: nombres y encabezado

Nombre resuelve por rol con fallback; la etiqueta prioriza cierre final, vigencia y pausa antes de dibujar.

```csharp
    internal void MostrarRed(string local, PresentacionCliente? vista, bool actual, string aviso,
        ResultadoCliente? resultado, int menu, bool confirma, int foco, bool atributo, bool pendiente, bool abandono)
    {
        // Se resuelven nombres por rol y se distingue pausa confirmada de pérdida de comunicación.
        string Nombre(string rol) => vista?.Nombres.GetValueOrDefault(rol) ?? rol;
        string estado = vista?.Fase == "finalizada" ? "Final confirmado" : !actual ? "Sin comunicación · Estado actual sin confirmar"
            : vista?.Pausada == true ? "Pausa confirmada" : "En línea";
        Encabezado(local, estado, aviso);
        // El menú conserva el estado en el encabezado y presenta No como primera opción de abandono.
```

### MostrarRed: historial en lectura

Estos contadores recortan historia por ronda/combate del resultado que se lee. GanaJ1 deriva del rol, por lo que nombres iguales no confunden al ganador.

```csharp
            var anteriores = vista.Resultados.Where(x => x.NumeroRonda < r.NumeroRonda ||
                x.NumeroRonda == r.NumeroRonda && x.NumeroCombate <= r.NumeroCombate).ToArray();
            int Combates(string rol) => anteriores.Count(x => x.NumeroRonda == r.NumeroRonda && x.GanadorCombate == rol);
            int Rondas(string rol) => anteriores.Count(x => x.GanadorRonda == rol);
            Escribir($"Ronda {r.NumeroRonda} · Combate {r.NumeroCombate}");
            // Se muestran los ganadores publicados por el servidor, sin volver a comparar atributos.
            MostrarCombate(new(r.Atributo,
                new(Nombre("J1"), r.Cartas["J1"].Nombre, r.Puntajes["J1"]),
                new(Nombre("J2"), r.Cartas["J2"].Nombre, r.Puntajes["J2"]),
                Nombre(r.GanadorCombate), Combates("J1"), Combates("J2"), null, r.GanadorCombate == "J1"));
            if (r.GanadorRonda is { } ganador) Escribir($"{Nombre(ganador)} gana la ronda");
            Escribir($"Rondas · {Nombre("J1")} {Rondas("J1")} / {Nombre("J2")} {Rondas("J2")}");
            Pie("Enter para continuar · Esc: menú"); return;
        }
        // Al agotar la lectura se muestra el estado vigente, que puede estar en una ronda posterior.
        Escribir($"{Nombre("J1")} (J1) / {Nombre("J2")} (J2) · Tu plaza: {vista.RolPropio}");
        Escribir($"Rondas · {Nombre("J1")} {vista.Marcador["J1"]} / {Nombre("J2")} {vista.Marcador["J2"]}");
        // Abandono y cancelación tienen mensajes propios y no se presentan como victorias de juego.
        if (vista.Fase == "finalizada")
        {
            Escribir(vista.MotivoFin switch
```

### MostrarRed: cierre y espera

La rama de cierre retorna antes de selección; abandono/cancelación no se muestran como victoria. En espera inicial se atribuye inicio al operador.

```csharp
            {
                "victoria" => $"{Nombre(vista.Ganador!)} gana la partida",
                "abandono" => "Partida finalizada por abandono, sin sumar una victoria de juego.",
                _ => "El operador canceló la partida, sin sumar una victoria de juego."
            });
            Pie("Enter para salir"); return;
        }
        // La espera inicial solo indica lo conocido: el inicio depende del operador.
        if (vista.Fase == "esperandoInicio") Escribir("Esperando inicio a cargo del operador.");
        else
        {
            Escribir($"Ronda {vista.NumeroRonda} · Combate {vista.NumeroCombate}");
            var combates = vista.Resultados.Where(r => r.NumeroRonda == vista.NumeroRonda).ToArray();
            Escribir($"Combates · {Nombre("J1")} {combates.Count(r => r.GanadorCombate == "J1")} / {Nombre("J2")} {combates.Count(r => r.GanadorCombate == "J2")}");
        }
        // Una vista anterior o una operación sin resolver nunca habilitan otra elección.
```

### MostrarRed: selección bloqueada

Cuando no hay elección habilitada conserva mano propia y explica operación pendiente o espera de solicitud, sin inventar opciones.

```csharp
        else
        {
            // Durante la espera se conserva la mano propia y se explica si falta confirmar una elección.
            MostrarMano(vista.Mano);
            if (pendiente) Escribir("Confirmación pendiente · Comprobando la elección antes de permitir otra.");
            else if (actual && !vista.Pausada && vista.Fase != "esperandoInicio") Escribir("Esperando solicitud del servidor.");
            Pie("Enter: volver a mostrar el estado · Esc: menú · Ctrl+C: salir sin abandonar");
        }
```

## Relaciones y verificación

[PresentacionRedTests](../../test/proyecto-integrador-final-tests/PresentacionRedTests.md) parametriza anchos y contrasta marcador histórico y neutralización de texto remoto; [SesionClienteTests](../../test/proyecto-integrador-final-tests/SesionClienteTests.md) verifica navegación, pendientes y resultados. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
