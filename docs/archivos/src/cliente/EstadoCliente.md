# EstadoCliente.cs

Fuente: [src/cliente/EstadoCliente.cs](../../../../src/cliente/EstadoCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Guarda la última presentación íntegra validada y una cola de resultados nuevos para leer. Marca el estado como no actual ante pérdida de red, sin inventar una pausa ni borrar el último marcador.

## Dependencias

Los DTO vienen de [ContratosPartida](ContratosPartida.md) y las cartas de [VistasConsola](VistasConsola.md). LINQ ordena y contrasta opciones; [FalloCliente](ClientePartidaHttp.md) comunica incompatibilidad al [coordinador](SesionPartidaCliente.md). Véase [protocolo](../../../protocolo-de-partida.md).

## Estructura y datos

`internal sealed class EstadoCliente(Guid partida)` captura la identidad fija del constructor primario. `private readonly HashSet<(int Ronda,int Combate)> conocidos` inicia vacío, conserva claves incluso tras reconectar. `internal PresentacionCliente? Vista {get; private set;}` inicia null; `internal bool Actual {get; private set;}` inicia false; `internal Queue<ResultadoCliente> Resultados {get;}` crea cola vacía mutable, consumida por el coordinador. No hay herencia explícita, eventos, interfaces ni partial.

## Miembros y ejecución

`internal void PerderComunicacion()` solo asigna Actual=false; no toca Vista, cola ni conocidos.

`internal void Aplicar(PresentacionCliente vista)` valida primero. NullReferenceException, KeyNotFoundException y ArgumentException durante Validar se convierten en FalloCliente("protocoloIncompatible"); ese mismo FalloCliente ya emitido por Exigir se propaga. Solo tras validar recorre resultados por ronda y combate ascendentes. Añade cada clave al conjunto; en la primera vista no encola historia previa. En vistas posteriores encola solo claves inéditas. Finalmente reemplaza Vista y establece Actual=true, incluso si la revisión no cambió. La monotonía de revisiones se controla en el coordinador, no aquí.

```csharp
        bool inicial = Vista is null;
        foreach (var resultado in vista.Resultados.OrderBy(r => r.NumeroRonda).ThenBy(r => r.NumeroCombate))
            if (conocidos.Add((resultado.NumeroRonda, resultado.NumeroCombate)) && !inicial)
                Resultados.Enqueue(resultado);
        Vista = vista;
        Actual = true;
```

`private void Validar(PresentacionCliente vista)` usa funciones locales estáticas `bool Rol(string? v)` (solo J1 o J2) y `void Exigir(bool condicion)` (lanza incompatibilidad si false). Verifica en orden:

1. UUID coincide con partida, revisión ≥0, rol propio conocido; fase en esperandoInicio/esperandoElector/esperandoRival/finalizada.
2. Nombres y Marcador contienen J1 y J2; Mano y Resultados no son null.
3. Toda carta propia tiene ID positivo, nombre y atributos no null, y las seis claves Fuerza, Velocidad, Resistencia, Inteligencia, Energía, Habilidad. IDs de mano sin duplicados.
4. Si finalizada: solicitud null y motivo victoria/abandono/cancelacion; victoria exige ganador J1/J2. No exige ganador nulo para los otros motivos.
5. Si existe solicitud: no pausada, fase de elección; UUID no vacío, tipo carta o cartaYAtributo; al menos un ID permitido y todos presentes en mano. Tipo carta requiere cero atributos; cartaYAtributo requiere al menos uno y cada atributo existe en todas las cartas permitidas. No exige explícitamente unicidad de opciones ni que los atributos correspondan solo a las seis claves conocidas.
6. Por cada resultado: ronda y combate positivos, ganador de combate válido, ganador de ronda null o válido, mapas Cartas y Puntajes con J1/J2, atributo no null. No recalcula ganador ni valida aquí cada detalle de las cartas históricas.

Fragmento de la guarda de solicitudes, que bloquea opciones imposibles:

```csharp
        if (vista.Solicitud is { } s)
        {
            Exigir(!vista.Pausada && vista.Fase is "esperandoElector" or "esperandoRival");
            Exigir(s.IdSolicitud != Guid.Empty && s.Tipo is "carta" or "cartaYAtributo");
            // Una mano vacía es válida en espera, pero ninguna solicitud puede ofrecer cartas inexistentes.
            Exigir(s.IdsPersonajePermitidos.Length > 0 && s.IdsPersonajePermitidos.All(id => vista.Mano.Any(c => c.IdPersonaje == id)));
            Exigir(s.Tipo == "carta" ? s.AtributosPermitidos.Length == 0 : s.AtributosPermitidos.Length > 0 &&
                s.AtributosPermitidos.All(a => vista.Mano.Where(c => s.IdsPersonajePermitidos.Contains(c.IdPersonaje)).All(c => c.Atributos.ContainsKey(a))));
        }

```

## Recorridos y ejemplos

Normal: primera vista con resultado (1,1) lo marca conocido sin mostrarlo otra vez. Segunda vista con (1,1),(1,3),(1,2) encola solo (1,2),(1,3), en ese orden. PerderComunicacion mantiene la cola; nueva vista de igual revisión puede actualizar la pausa.

Error: solicitud ofrece ID 99 ausente de mano; Validar lanza y conserva Vista, Actual y cola anteriores. Mano vacía es válida si no hay solicitud; con opciones obligatorias es incompatible. No crea estados parciales.

## Errores, límites y efectos observables

No hace HTTP ni persiste. No limita tamaño de historia o cola, ni impide revisiones menores por sí mismo. No hay bloqueos: el bucle del coordinador es su dueño único. Una clave repetida con contenido distinto se considera ya conocida y no vuelve a encolarse.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### Validar: identidad, mano y cierre

Las funciones locales Rol y Exigir sustentan comprobaciones previas a mutar estado. Required de JSON no reemplaza estas condiciones de coherencia.

```csharp
    {
        // Las identidades son J1/J2: dos jugadores pueden tener el mismo nombre visible.
        static bool Rol(string? v) => v is "J1" or "J2";
        static void Exigir(bool condicion) { if (!condicion) throw new FalloCliente("protocoloIncompatible"); }
        Exigir(vista.IdPartida == partida && vista.Revision >= 0 && Rol(vista.RolPropio));
        Exigir(vista.Fase is "esperandoInicio" or "esperandoElector" or "esperandoRival" or "finalizada");
        // Los datos usados por el renderizado deben estar completos antes de habilitar la interacción.
        Exigir(vista.Nombres.ContainsKey("J1") && vista.Nombres.ContainsKey("J2") &&
            vista.Marcador.ContainsKey("J1") && vista.Marcador.ContainsKey("J2"));
        Exigir(vista.Mano is not null && vista.Resultados is not null);
        string[] atributos = ["Fuerza", "Velocidad", "Resistencia", "Inteligencia", "Energía", "Habilidad"];
        Exigir(vista.Mano!.All(c => c.IdPersonaje > 0 && c.Nombre is not null && c.Atributos is not null &&
            atributos.All(c.Atributos.ContainsKey)));
        Exigir(vista.Mano!.Select(c => c.IdPersonaje).Distinct().Count() == vista.Mano!.Length);
        // Un cierre válido ya no publica solicitudes y solo atribuye ganador a una victoria.
        if (vista.Fase == "finalizada")
            Exigir(vista.Solicitud is null && vista.MotivoFin is "victoria" or "abandono" or "cancelacion" &&
                (vista.MotivoFin != "victoria" || Rol(vista.Ganador)));
```

### Validar: resultados revelados

Solo se verifica la forma y roles de resultados; no se vuelve a resolver el duelo ni se recalculan puntajes.

```csharp
        foreach (var r in vista.Resultados!)
            Exigir(r.NumeroRonda > 0 && r.NumeroCombate > 0 && Rol(r.GanadorCombate) &&
                (r.GanadorRonda is null || Rol(r.GanadorRonda)) && r.Cartas.ContainsKey("J1") && r.Cartas.ContainsKey("J2") &&
                r.Puntajes.ContainsKey("J1") && r.Puntajes.ContainsKey("J2") && r.Atributo is not null);
```

### Aplicar: guarda antes de mutación

La validación antecede a cualquier inserción en conocidos o cola. Los errores de forma se traducen en incompatibilidad; primera vista establece historia sin reproducirla.

```csharp

    internal void Aplicar(PresentacionCliente vista)
    {
        // Se valida antes de modificar el estado para no aplicar respuestas parcialmente incompatibles.
        try { Validar(vista); }
        catch (Exception ex) when (ex is NullReferenceException or KeyNotFoundException or ArgumentException)
        { throw new FalloCliente("protocoloIncompatible"); }
        // La primera vista establece la línea de base; una reconexión conserva las claves conocidas.
        bool inicial = Vista is null;
        foreach (var resultado in vista.Resultados.OrderBy(r => r.NumeroRonda).ThenBy(r => r.NumeroCombate))
            if (conocidos.Add((resultado.NumeroRonda, resultado.NumeroCombate)) && !inicial)
                Resultados.Enqueue(resultado);
        Vista = vista;
        Actual = true;
    }
```

## Relaciones y verificación

[EstadoClienteTests](../../test/proyecto-integrador-final-tests/EstadoClienteTests.md): primera vista/historia, orden tras reconectar, pausa y nombres con igual revisión, rechazo sin destruir estado y mano vacía frente a opciones imposibles. No prueban por sí solos el transporte. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
