# SesionPartidaCliente.cs

Fuente: [src/cliente/SesionPartidaCliente.cs](../../../../src/cliente/SesionPartidaCliente.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Es el coordinador del cliente REST. Un único bucle aplica respuestas, mantiene la sesión, recupera elecciones inciertas y procesa teclado. Las tareas HTTP producen respuestas pero nunca dibujan ni cambian el modelo por su cuenta. Separa el resultado que se está leyendo del estado remoto más reciente.

## Dependencias

[ConfiguracionCliente](ConfiguracionCliente.md) fija partida/nombre; [ClientePartidaHttp](ClientePartidaHttp.md) envía; [EstadoCliente](EstadoCliente.md) valida y encola historia nueva; [EntradaCliente](EntradaCliente.md) lee; [InterfazConsola.Red](InterfazConsola.Red.md) dibuja. [ContratosPartida](ContratosPartida.md) contiene DTO. TimeProvider sustituible permite pruebas de intervalos; JsonSerializer obtiene huellas; Volatile lee versión de pantalla. Véanse [protocolo](../../../protocolo-de-partida.md) y [diseño de interfaz](../../../diseno-de-interfaz.md).

## Estructura

`internal sealed class SesionPartidaCliente(ConfiguracionCliente configuracion, ClientePartidaHttp http, InterfazConsola interfaz, TimeProvider? reloj = null)` captura colaboradores, sin guardas explícitas. No hereda explícitamente ni declara interfaces, atributos, partial, eventos u operadores. La entrada pública efectiva del tipo es `internal async Task<bool> Ejecutar`; los demás ayudantes son privados.

## Estado y datos

| Campos/propiedad privados | Inicialización, significado y cambios |
|---|---|
| readonly `TimeProvider tiempo` | reloj o TimeProvider.System; todas las fechas/reintentos y delay del bucle usan este reloj. |
| readonly `EstadoCliente estado` | nueva instancia fijada a configuracion.Partida; único dueño de su aplicación y lectura de cola. |
| bool `incorporado` | false; true solo tras confirmar sesión; vuelve false al vencer o reintentar explícitamente. |
| bool `terminal` | false; true tras vista finalizada válida, bloquea nuevos envíos, permite leer últimos resultados. |
| bool `protocolo`, `corregir` | false; protocolo detiene consultas/escrituras hasta decisión del usuario; corregir hace salir hacia la configuración de Program. |
| bool `abandono`, `salidaFinal` | false; abandono solicita PUT cuando no haya elección incierta; salidaFinal evita mostrar salida local tras Enter final. |
| string `aviso` | `Conectando…`; mensajes de red, recepción, error, cierre. |
| string? `huella`, `seleccion` | null; huella JSON evita repintado idéntico; seleccion JSON identifica revisión/solicitud/pausa que autorizan el foco actual. |
| int `foco`, `carta`, `menu=-1` | foco/carta cero, menú cerrado; carta guarda ID tras elegir, foco índice de opciones y menú índice de acción. |
| bool `atributo`, `confirmarAbandono` | false; paso de selección atributo y confirmación No/Sí. |
| `EleccionCliente? pendiente` | null; cuerpo inmutable con UUID único guardado antes de primer POST y conservado en pérdida incierta. |
| bool `reciboAusente` | false; true permite enviar cuerpo si vista vigente; false exige consultar recibo antes de reenviar. |
| `ResultadoCliente? leyendo` | null; resultado en pantalla hasta Enter, independiente de revisiones nuevas. |
| DateTimeOffset `proximaVista`, `proximaPresencia`, `proximaEscritura` | default; instantes a partir de los que cada canal puede comenzar otra solicitud. |
| int `fallosVista`, `fallosPresencia`, `fallosEscritura` | cero; escalonan reintentos independientes, se reinician tras éxito de su canal. |
| `Task<PresentacionCliente>? consulta`, `Task? presencia`, `Task<object?>? escritura` | null; como máximo una operación en vuelo por canal. |
| string `claseEscritura` | vacío; sesion/eleccion/recibo/abandono identifica la respuesta serializada. |
| int `revisionMinima` | cero; suelo confirmado por recibo/abandono para descartar vistas anteriores. |
| int `generacion`, `generacionVista`, `generacionPresencia` | cero; cada recuperación/reintento invalida respuestas antiguas de consultas/presencia. |
| bool `descartarEntrada` | false; solicita vaciar sobrante físico tras cambiar contexto; no consume líneas redirigidas. |
| bool `corregible`, `presenciaBloqueada` | false; permite volver a datos o detiene renovación ante error definitivo. |
| int `versionPantalla` | cero; cambia al cambiar solicitud/lectura/elección y etiqueta eventos contra confirmaciones viejas. |
| `DateTimeOffset Ahora` | getter lógico `tiempo.GetUtcNow()`; sin setter. |

No hay persistencia local. La identidad HTTP la posee el transporte; estado de esta instancia se descarta cuando Program permite corregir configuración.

## Miembros y ejecución

### Reloj y bucle principal

`private TimeSpan Espera(ref int fallos)` incrementa el contador por referencia y retorna 1 segundo para el primero, 2 para segundo y 5 para tercero y siguientes. No cambia otros canales.

`internal async Task<bool> Ejecutar(EntradaCliente entrada, CancellationToken ct)` crea una fuente enlazada a ct y una tarea opcional de evento `(ConsoleKey? Tecla,int Version)`. Dibuja una vez. Mientras no cancelación externa ni corregir:

1. Completa tareas terminadas en orden escritura, consulta, presencia; solo esos ayudantes aplican respuestas.
2. Si no terminal y ya incorporado, inicia consulta si no protocolo, no hay consulta en vuelo y llegó proximaVista; toma generación actual y deja próximo tick a +1 segundo. Independientemente, inicia presencia si no corregible ni bloqueada, no hay presencia y llegó su instante; próxima a +5 segundos. Una consulta lenta no bloquea presencia ni crea cola de ticks atrasados.
3. Si no protocolo, no escritura y llegó proximaEscritura: primero incorporación si falta sesión. Si hay elección pendiente y no reciboAusente, GET recibo. Si reciboAusente, requiere estado actual: pausa conserva operación sin enviar, solicitud no Vigente descarta pendiente y avisa, vigente envía mismo cuerpo. Solo si no hay elección pendiente considera abandono, y requiere estado actual.
4. Mantiene una sola LeerEvento en vuelo. EOF deja aviso y rompe. Procesa tecla si menú abierto, resultado en lectura, Escape, o versión del evento igual a pantalla. Si Procesar retorna true sale. Evento de otra pantalla se descarta para no confirmar opciones obsoletas.
5. Si descartarEntrada llama al vaciado físico y baja flag. Espera 25 ms mediante TimeProvider/token y repite.

Fragmento de serialización de escrituras: la incorporación precede al pendiente y el abandono lo sigue:

```csharp
                        if (!incorporado) IniciarEscritura("sesion", async () => await http.Incorporar(token));
                        else if (pendiente is not null)
                        {
                            // Tras una respuesta perdida se busca el recibo antes de considerar cualquier reenvío.
                            if (!reciboAusente) IniciarEscritura("recibo", async () => await http.Recibo(pendiente.IdOperacion, token));
                            else if (estado.Actual && estado.Vista is { } v)
                            {
                                // La pausa conserva la operación; una solicitud distinta impide reutilizarla.
                                if (v.Pausada) { /* La operación se conserva hasta reanudación. */ }
                                else if (!Vigente(pendiente, v)) { pendiente = null; reciboAusente = false; aviso = "La solicitud cambió; no se reenvió la elección."; Dibujar(); }
                                else IniciarEscritura("eleccion", async () => await http.Elegir(pendiente, token));
                            }
                        }
                        else if (abandono && estado.Actual)
                            IniciarEscritura("abandono", async () => { await http.Abandonar(token); return null; });
```

Las lambdas de IniciarEscritura adaptan Task del DTO a Task<object?>; abandono espera PUT y retorna null. No mutan interfaz. Si cancelación del token enlazado ocurre se captura. Finally cancela y espera consulta/presencia/escritura/tecla pendientes una a una, ignorando únicamente FalloCliente, OperationCanceledException e IOException de esa limpieza. Tras finalizar, salvo corrección o salidaFinal, muestra salida local: si abandono aún true advierte que no está confirmado; si no, partida guardada y plaza posiblemente ocupada quince segundos o hasta cierre del operador. Retorna corregir para el do/while de Program.

### Escrituras y recuperación idempotente

`private void IniciarEscritura(string clase, Func<Task<object?>> iniciar)` fija claseEscritura, pone aviso de envío si clase eleccion y guarda tarea devuelta por iniciar. Lo llama solo el bucle cuando escritura es null; no hay escritura paralela.

`private async Task CompletarEscritura()` toma tarea y pone escritura=null antes de await. Éxito pone contador fallosEscritura=0 y proximaEscritura=Ahora. Switch:

- sesion: convierte a SesionConfirmada y exige IdSesion==http.IdSesion y rol J1/J2. Si válido incorporado=true, aviso con plaza, consulta inmediata y presencia +5s. No valida VenceUtc aquí.
- eleccion o recibo: convierte ReciboCliente; exige pendiente no null, ambos UUID iguales y RevisionConfirmada>RevisionEsperada. Sube revisionMinima al máximo, borra pendiente y reciboAusente, marca estado no actual, consulta inmediata y aviso de recepción. Un recibo no revela el próximo turno ni habilita otra selección.
- abandono: revisionMinima sube a vista.Revision+1 si no terminal (o misma si ya terminal), abandono=false, estado no actual, aviso confirmado y consulta inmediata. Solo una vista posterior define motivo final.

Fragmento real de aceptación del recibo:

```csharp
                    var r = (ReciboCliente)respuesta!;
                    if (pendiente is null || r.IdOperacion != pendiente.IdOperacion || r.IdSolicitud != pendiente.IdSolicitud || r.RevisionConfirmada <= pendiente.RevisionEsperada)
                        throw new FalloCliente("protocoloIncompatible");
                    // Se rechazan vistas anteriores al recibo para no reactivar una solicitud ya consumida.
                    revisionMinima = Math.Max(revisionMinima, r.RevisionConfirmada);
                    pendiente = null;
                    reciboAusente = false;
                    estado.PerderComunicacion(); // Un recibo no es una presentación del turno siguiente.
                    proximaVista = Ahora;
                    aviso = "Recepción confirmada. Esperando presentación del servidor.";
```

Catch FalloCliente se divide: recibo con recursoNoEncontrado→reciboAusente=true, estado no actual y nueva vista inmediata; eleccion rechazada con entradaInvalida/eleccionInvalida/revisionDesactualizada/solicitudNoVigente/operacionIncompatible→borra operación, no reutiliza UUID, informa rechazo, invalida estado y refresca; resto→si falló eleccion marca reciboAusente=false (podría haberse aplicado), llama Tratar y programa proximaEscritura con backoff. Siempre Dibujar al terminar; no inventa aceptación.

### Consultas, presencia y clasificación de fallos

`private async Task CompletarConsulta()` toma/borra tarea, espera vista; ignora respuesta si generación ya cambió o terminal. Revisión inferior a revisionMinima o a última Vista se descarta y programa otra consulta inmediata. Aplica EstadoCliente (puede rechazar semántica). Reinicia fallosVista; si ClaveSeleccion cambió, actualiza clave, foco=0, atributo=false, descartarEntrada=true y versionPantalla++. Si no había leyendo y hay resultados nuevos, extrae uno y cambia versión/descarta sobrante. Vista finalizada pone terminal=true, abandono=false y aviso final; si no final ni pendiente ni abandono, aviso actualizado. Dibuja. FalloCliente de consulta vieja/final se ignora; resto Tratar, proximaVista con backoff y dibuja. El resultado leído no se reemplaza por una vista más nueva.

`private async Task CompletarPresencia()` toma/borra tarea; éxito reinicia contador. Ante FalloCliente ignora respuesta vieja o terminal; resto Tratar. Si no transitorio y código no sesionVencida, bloquea futuras presencias; programa reintento de presencia y dibuja. El éxito de presencia no aplica una presentación ni actualiza Actual.

`private void Tratar(FalloCliente error)` marca estado no actual, seleccion=null y atributo=false; transitorio muestra reconexión con último estado. Luego distingue códigos:

- sesionVencida: incorporado=false, generacion++, reciboAusente=false; conserva pendiente e identidad, avisa reincorporación.
- entradaInvalida/credencialInvalida/sinPermiso/recursoNoEncontrado: protocolo=true, corregible=true; Enter vuelve a configuración, Escape permite menú; advierte que una elección previa puede haberse aplicado y exige datos originales para recuperarla.
- protocoloIncompatible: protocolo=true, Enter reintenta explícitamente; no aplica vista parcial.
- sesionOcupada: aviso de esperar liberación o quince segundos, sin marcar corregible.
- otros códigos conservan su aviso; el planificador define el siguiente intento. No convierte pérdida de comunicación en pausa confirmada.

### Contexto de entrada y vigencia

`private async Task<(ConsoleKey? Tecla,int Version)> LeerEvento(EntradaCliente entrada, CancellationToken ct)` espera entrada y lee versionPantalla con Volatile.Read al llegar la tecla, no al comenzar la espera. Esto permite usar el contexto que ya estaba visible cuando se tecleó.

`private static bool Vigente(EleccionCliente e, PresentacionCliente v)` exige revisión idéntica, solicitud presente con mismo UUID, carta en IDs y atributo compatible: tipo carta exige e.Atributo null; otro tipo exige pertenencia al array permitido. No prueba pausa aquí: el llamador la comprueba antes de reenviar.

```csharp
    private static bool Vigente(EleccionCliente e, PresentacionCliente v) =>
        v.Revision == e.RevisionEsperada && v.Solicitud is { } s && s.IdSolicitud == e.IdSolicitud &&
        s.IdsPersonajePermitidos.Contains(e.IdPersonaje) && (s.Tipo == "carta" ? e.Atributo is null : s.AtributosPermitidos.Contains(e.Atributo));
```

`private static string ClaveSeleccion(PresentacionCliente v)` serializa objeto anónimo con Revision, Solicitud y Pausada. Cambios de nombres o presencia que no cambian esos datos no reinician foco. Es una huella de contenido, no igualdad de referencia de records.

### Procesar teclado

`private bool Procesar(ConsoleKey tecla)` retorna true solo para salir localmente o terminar lectura del final; false sigue bucle. Orden de ramas:

1. Escape alterna menu entre 0 y −1, borra confirmación, dibuja forzado y retorna. Puede abrir menú aun en red perdida o envío pendiente.
2. Calcula paso −1 con Up/Left/W/A/H/K, +1 con Down/Right/S/D/J/L; resto0.
3. Menú abierto: paso clamp a [0,1] en confirmación o [0,2] normal. Enter confirmando: Sí y no terminal marca abandono=true y aviso; No no escribe; ambos cierran confirmación/menú. Menú normal: índice1 retorna true (salir sin abandonar); índice2 y no terminal abre confirmación con menu0 (No); resto cierra menú. Dibuja y retorna false. El flag abandono no ejecuta PUT en esta función.
4. Enter con protocolo: si corregible pone corregir=true y retorna false, saldrá bucle. Si no, quita protocolo/bloqueo de presencia, incorporado=false, generacion++, reciboAusente=false, escritura inmediata, dibuja y sigue; obliga comprobar sesión/operación.
5. Si leyendo: Enter pasa al siguiente resultado o null, descarta sobrante y cambia versión, dibuja; cualquier tecla restante no elige nada y retorna false.
6. Si terminal: salidaFinal=true solo para Enter y retorna ese bool. No crea más tráfico.
7. Si estado no actual, pendiente, abandono o escritura en vuelo: Enter solo repinta, resto ignora. Igual si vista pausada o no tiene solicitud.
8. Si clave de selección desactualizada: restablece clave/foco/paso atributo, dibuja y retorna sin confirmar con esa tecla.
9. Paso cambia foco con Clamp según longitud de atributos o IDs. Enter en carta guarda ID; si cartaYAtributo pasa a atributos y foco0 sin crear operación todavía. Si ya atributo o solo carta, crea EleccionCliente con nuevo UUID, UUID solicitud, revisión, carta y atributo o null; si no Vigente no hace nada. Si válido guarda pendiente ANTES del envío, reciboAusente=true, resetea selección, cambia versión/descarta sobrante y dibuja.

Fragmento de creación única de operación, después de completar la selección:

```csharp
            var e = new EleccionCliente(Guid.NewGuid(), s.IdSolicitud, v.Revision, carta,
                atributo ? s.AtributosPermitidos[foco] : null);
            if (!Vigente(e, v)) return false;
            pendiente = e; reciboAusente = true; atributo = false; foco = 0;
            aviso = "Envío en curso"; descartarEntrada = true; versionPantalla++;
            Dibujar(true);
        }
        return false;
```

### Dibujo

`private void Dibujar(bool forzar = false)` serializa vista, Actual, aviso, menu, confirmarAbandono, leyendo, foco, atributo, pendiente y terminal. Si no forzar y huella igual retorna; si cambia o se fuerza guarda huella y llama MostrarRed con los flags correspondientes. Evita repintados periódicos idénticos y no reinicia foco. El flag abandono se pasa al renderizador; no figura directamente en la huella, aunque sus cambios normales también alteran aviso/menu/pendiente.

## Recorridos y ejemplos

Normal: incorporación confirma sesión S/J1. Consulta revisión4 con solicitud Q permite carta7 y Fuerza; Enter carta, Enter atributo crean operación O con revisión4. POST devuelve recibo O/Q/revisión5: borra pendiente, bloquea elección y consulta. Vista5 con resultado nuevo se valida y se encola. Enter leyendo solo cierra ese resultado; otro Enter posterior puede empezar una elección nueva si el servidor la ofrece.

Alternativo: POST se aplicó pero respuesta se pierde. pendiente conserva O y cuerpo; tras backoff consulta recibo. Si existe, acepta revisión confirmada sin reenviar; si 404, refresca vista y solo reenvía exactamente el cuerpo original cuando revisión/Q/carta/atributo coinciden. Si está pausada conserva pendiente; si solicitud cambió lo descarta y no envía sobre otra. Un abandono solicitado durante esta recuperación espera detrás de ella. Respuesta tardía de presencia de una generación anterior no invalida la sesión reincorporada.

## Errores, límites y efectos observables

Puede cambiar sesión y partida remotamente por HTTP; perder respuesta no prueba ausencia de efectos persistidos. La serialización evita dos escrituras simultáneas, pero consulta y presencia son independientes. No hay escritura automática de abandono por EOF/Ctrl+C; puede quedar plaza ocupada. No reconcilia operaciones de otro proceso después de cerrar porque no guarda UUID/cuerpo en disco. Una vista final detiene tráfico nuevo, no borra tareas ya enviadas; se esperan al salir. Los fallos distintos a los previstos no se capturan como si fueran errores de red.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### Espera: incremento y escalones

El incremento por referencia ocurre antes del switch; el tercer fallo y todos los posteriores usan cinco segundos.

```csharp
    private DateTimeOffset Ahora => tiempo.GetUtcNow();
    private TimeSpan Espera(ref int fallos) => TimeSpan.FromSeconds(++fallos switch { 1 => 1, 2 => 2, _ => 5 });
```

### Ejecutar: planificación independiente

Este inicio del método procesa respuestas antes de programar consultas/presencia, fija sus generaciones y limita cada canal a una tarea. El resto de serialización de escrituras ya se mostró arriba.

```csharp
    internal async Task<bool> Ejecutar(EntradaCliente entrada, CancellationToken ct)
    {
        using var cierre = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = cierre.Token;
        // La versión de pantalla permite descartar confirmaciones capturadas para opciones anteriores.
        Task<(ConsoleKey? Tecla, int Version)>? tecla = null;
        try
        {
            Dibujar();
            while (!ct.IsCancellationRequested && !corregir)
            {
                // Se incorporan las respuestas antes de procesar entrada o programar nuevos envíos.
                if (escritura?.IsCompleted == true) await CompletarEscritura();
                if (consulta?.IsCompleted == true) await CompletarConsulta();
                if (presencia?.IsCompleted == true) await CompletarPresencia();
                if (!terminal)
                {
                    if (incorporado)
                    {
                        // Consulta y presencia avanzan por separado, incluso mientras se lee o se navega.
                        if (!protocolo && consulta is null && Ahora >= proximaVista)
                        {
                            generacionVista = generacion;
                            consulta = http.Presentacion(token);
                            proximaVista = Ahora.AddSeconds(1);
                        }
                        // Una consulta lenta no retiene la renovación de presencia ni acumula ticks atrasados.
                        if (!corregible && !presenciaBloqueada && presencia is null && Ahora >= proximaPresencia)
                        {
                            generacionPresencia = generacion;
                            presencia = http.Renovar(token);
                            proximaPresencia = Ahora.AddSeconds(5);
                        }
                    }
```

### Ejecutar: eventos y limpieza

Tras programar la red, solo un evento queda en espera. La versión limita confirmaciones antiguas, mientras menú/resultados/Escape siguen accesibles. El finally cancela y espera todas las tareas antes del retorno y del mensaje de salida.

```csharp
                // Menú y resultados admiten interacción mientras la red actualiza el modelo de fondo.
                tecla ??= LeerEvento(entrada, token);
                if (tecla.IsCompleted)
                {
                    var evento = await tecla;
                    tecla = null;
                    if (evento.Tecla is null) { aviso = "Fin de la entrada."; break; }
                    if ((menu >= 0 || leyendo is not null || evento.Tecla == ConsoleKey.Escape || evento.Version == versionPantalla) && Procesar(evento.Tecla.Value)) break;
                }
                // Al cambiar de pantalla solo se descarta el sobrante interactivo, nunca las líneas futuras.
                if (descartarEntrada) { entrada.DescartarSobrante(); descartarEntrada = false; }
                await Task.Delay(TimeSpan.FromMilliseconds(25), tiempo, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally
        {
            // La salida cancela y espera las tareas antes de desechar sus recursos.
            cierre.Cancel();
            var tareas = new Task?[] { consulta, presencia, escritura, tecla }.Where(t => t is not null).Cast<Task>();
            foreach (var tarea in tareas)
                try { await tarea; } catch (Exception ex) when (ex is FalloCliente or OperationCanceledException or IOException) { }
        }
        // Salir localmente no confirma un abandono pendiente ni libera inmediatamente la plaza.
        if (!corregir && !salidaFinal)
            interfaz.MostrarInterrupcion(configuracion.Nombre, "Salida local", abandono
                ? "Abandono sin confirmar. La plaza puede seguir ocupada hasta quince segundos."
                : "La partida queda guardada. La plaza puede seguir ocupada hasta quince segundos o hasta que el operador cierre la sesión.", false);
        return corregir;
    }
```

### IniciarEscritura: dueño único del canal

Registra clase y tarea; el planificador garantiza que no exista otra escritura en vuelo.

```csharp
    private void IniciarEscritura(string clase, Func<Task<object?>> iniciar)
    {
        claseEscritura = clase;
        if (clase == "eleccion") aviso = "Envío en curso";
        escritura = iniciar();
    }
```

### CompletarEscritura: errores y reintentos

Este catch sigue al switch de éxito descrito arriba. El 404 de recibo exige refrescar vista, el rechazo definitivo descarta operación y los fallos inciertos fuerzan recuperar recibo antes de repetir.

```csharp
        catch (FalloCliente error)
        {
            // Un 404 de recibo significa ausencia de confirmación; exige refrescar la vista antes de reenviar.
            if (claseEscritura == "recibo" && error.Codigo == "recursoNoEncontrado")
            {
                reciboAusente = true;
                estado.PerderComunicacion();
                proximaVista = Ahora;
            }
            // Los rechazos definitivos permiten corregir con otra operación, sin reutilizar el UUID rechazado.
            else if (claseEscritura == "eleccion" && error.Codigo is "entradaInvalida" or "eleccionInvalida" or "revisionDesactualizada" or "solicitudNoVigente" or "operacionIncompatible")
            {
                pendiente = null;
                reciboAusente = false;
                aviso = $"Elección rechazada: {error.Codigo}. Actualizando opciones.";
                estado.PerderComunicacion();
                proximaVista = Ahora;
            }
            else
            {
                // Un fallo incierto obliga a volver al recibo; no se asume que el POST quedó sin aplicar.
                if (claseEscritura == "eleccion") reciboAusente = false;
                Tratar(error);
                proximaEscritura = Ahora + Espera(ref fallosEscritura);
            }
        }
        Dibujar();
    }
```

### CompletarConsulta: generaciones y cola

El método entero muestra la precedencia de descartar respuestas viejas, validar, cambiar contexto de selección y conservar la lectura actual. Los errores programan backoff sin aplicar vistas parciales.

```csharp
    private async Task CompletarConsulta()
    {
        var tarea = consulta!;
        consulta = null;
        try
        {
            var vista = await tarea;
            // Una recuperación invalida respuestas antiguas; una revisión menor tampoco puede retroceder el estado.
            if (generacionVista != generacion || terminal) return;
            if (vista.Revision < revisionMinima || vista.Revision < (estado.Vista?.Revision ?? 0))
            { proximaVista = Ahora; return; }
            estado.Aplicar(vista);
            fallosVista = 0;
            if (seleccion != ClaveSeleccion(vista))
            {
                seleccion = ClaveSeleccion(vista); foco = 0; atributo = false; descartarEntrada = true; versionPantalla++;
            }
            // Un resultado permanece visible hasta Enter, aunque sigan llegando vistas más nuevas.
            if (leyendo is null && estado.Resultados.Count > 0)
            { leyendo = estado.Resultados.Dequeue(); descartarEntrada = true; versionPantalla++; }
            // El cierre detiene nuevas solicitudes, pero respeta la lectura de los últimos resultados.
            if (vista.Fase == "finalizada")
            {
                terminal = true;
                abandono = false;
                aviso = "Final confirmado por el servidor";
            }
            else if (pendiente is null && !abandono) aviso = "Estado actualizado";
            Dibujar();
        }
        catch (FalloCliente error)
        {
            if (generacionVista != generacion || terminal) return;
            Tratar(error);
            proximaVista = Ahora + Espera(ref fallosVista);
            Dibujar();
        }
    }
```

### CompletarPresencia: aislamiento de respuestas antiguas

Un fallo antiguo no toca la sesión nueva; el éxito reinicia solo contador de presencia. El bloqueo definitivo evita renovación inútil hasta decisión explícita.

```csharp
    private async Task CompletarPresencia()
    {
        var tarea = presencia!;
        presencia = null;
        try { await tarea; fallosPresencia = 0; }
        catch (FalloCliente error)
        {
            if (generacionPresencia != generacion || terminal) return;
            Tratar(error);
            if (!error.Transitorio && error.Codigo != "sesionVencida") presenciaBloqueada = true;
            proximaPresencia = Ahora + Espera(ref fallosPresencia);
            Dibujar();
        }
    }
```

### Tratar: clasificación y reincorporación

Cada rama invalida vigencia de pantalla antes de decidir recuperación, corrección de identidad o reintento explícito. El cambio de generación conserva la elección pendiente.

```csharp
    private void Tratar(FalloCliente error)
    {
        estado.PerderComunicacion();
        seleccion = null;
        atributo = false;
        aviso = error.Transitorio ? "Sin comunicación · Último estado confirmado. Reconectando…" : error.Codigo;
        // La reincorporación conserva identidad y operación, pero invalida las respuestas que quedaron en vuelo.
        if (error.Codigo == "sesionVencida")
        {
            incorporado = false; generacion++; reciboAusente = false;
            aviso = "Sesión vencida. Reincorporando con la misma identidad…";
        }
        // Identidad o partida incorrectas requieren corregir datos: no se reintenta indefinidamente lo mismo.
        else if (error.Codigo is "entradaInvalida" or "credencialInvalida" or "sinPermiso" or "recursoNoEncontrado")
        {
            protocolo = true;
            aviso += " · Enter para corregir los datos o Esc para salir. Una elección anterior puede haberse aplicado; recuperar requiere sus datos originales.";
            corregible = true;
        }
        // Ante un protocolo desconocido el usuario decide reintentar o salir antes de continuar.
        else if (error.Codigo == "protocoloIncompatible")
        {
            protocolo = true;
            aviso = "Respuesta incompatible o inconsistente · Enter para reintentar o Esc para salir.";
        }
        else if (error.Codigo == "sesionOcupada")
            aviso = "Sesión ocupada. Esperando liberación o vencimiento (hasta quince segundos). Esc para salir.";
    }
```

### LeerEvento y ClaveSeleccion: contexto de confirmación

LeerEvento etiqueta al llegar; Vigente comprueba contexto completo; la clave JSON incluye pausa además de revisión y solicitud.

```csharp
    private async Task<(ConsoleKey? Tecla, int Version)> LeerEvento(EntradaCliente entrada, CancellationToken ct)
    {
        var tecla = await entrada.Leer(ct);
        return (tecla, Volatile.Read(ref versionPantalla));
    }
    // Antes del POST deben coincidir revisión, solicitud, carta y atributo de la operación original.
    private static bool Vigente(EleccionCliente e, PresentacionCliente v) =>
        v.Revision == e.RevisionEsperada && v.Solicitud is { } s && s.IdSolicitud == e.IdSolicitud &&
        s.IdsPersonajePermitidos.Contains(e.IdPersonaje) && (s.Tipo == "carta" ? e.Atributo is null : s.AtributosPermitidos.Contains(e.Atributo));
    private static string ClaveSeleccion(PresentacionCliente v) => JsonSerializer.Serialize(new { v.Revision, v.Solicitud, v.Pausada });
```

### Procesar: Escape, menú y reintento

Este prefijo del método permite salida y confirmación No/Sí aun durante errores. Abandono solo pone un flag; el bucle lo envía después del pendiente. El reintento de protocolo crea otra generación.

```csharp
    private bool Procesar(ConsoleKey tecla)
    {
        // Esc siempre puede abrir o cerrar el menú, aun durante reconexión o envío pendiente.
        if (tecla == ConsoleKey.Escape)
        {
            menu = menu < 0 ? 0 : -1;
            confirmarAbandono = false;
            Dibujar(true);
            return false;
        }
        // Flechas, WASD y HJKL comparten navegación; el foco se limita a los extremos de la lista.
        int paso = tecla switch
        {
            ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.H or ConsoleKey.K => -1,
            ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L => 1,
            _ => 0
        };
        if (menu >= 0)
        {
            if (paso != 0) menu = Math.Clamp(menu + paso, 0, confirmarAbandono ? 1 : 2);
            if (tecla == ConsoleKey.Enter)
            {
                // Elegir No no escribe en el servidor; elegir Sí solo deja el abandono listo para el coordinador.
                if (confirmarAbandono)
                {
                    if (menu == 1 && !terminal) { abandono = true; aviso = "Abandono pendiente de confirmación del servidor."; }
                    confirmarAbandono = false; menu = -1;
                }
                else if (menu == 1) return true;
                else if (menu == 2 && !terminal) { confirmarAbandono = true; menu = 0; }
                else menu = -1;
            }
            Dibujar(true);
            return false;
        }
        // Un reintento explícito crea otra generación y vuelve a comprobar la sesión y la operación incierta.
        if (tecla == ConsoleKey.Enter && protocolo)
        {
            if (corregible) { corregir = true; return false; }
            protocolo = false; presenciaBloqueada = false; incorporado = false; generacion++; reciboAusente = false; proximaEscritura = Ahora;
            Dibujar(true); return false;
        }
        // Enter retira únicamente el resultado leído y nunca confirma una jugada del turno siguiente.
```

### Procesar: lectura, terminal y selección

La rama de lectura retorna antes de las elecciones; un Enter no puede a la vez cerrar resultado y jugar. Las guardas bloquean vista vieja/pausada y operaciones pendientes antes de acceder a opciones. La creación final de operación ya se mostró arriba.

```csharp
        if (leyendo is not null)
        {
            if (tecla == ConsoleKey.Enter)
            {
                leyendo = estado.Resultados.Count > 0 ? estado.Resultados.Dequeue() : null;
                descartarEntrada = true; versionPantalla++;
                Dibujar(true);
            }
            return false;
        }
        // La pantalla final solo admite salir; los estados no actuales o pendientes bloquean cualquier jugada.
        if (terminal)
        {
            salidaFinal = tecla == ConsoleKey.Enter;
            return salidaFinal;
        }
        if (!estado.Actual || pendiente is not null || abandono || escritura is not null)
        { if (tecla == ConsoleKey.Enter) Dibujar(true); return false; }
        var v = estado.Vista!;
        if (v.Pausada || v.Solicitud is not { } s)
        { if (tecla == ConsoleKey.Enter) Dibujar(true); return false; }
        // Si cambió el contexto de selección, se restablece el foco y se exige una confirmación nueva.
        if (seleccion != ClaveSeleccion(v)) { seleccion = ClaveSeleccion(v); foco = 0; atributo = false; Dibujar(true); return false; }
        if (paso != 0)
        {
            foco = Math.Clamp(foco + paso, 0, (atributo ? s.AtributosPermitidos.Length : s.IdsPersonajePermitidos.Length) - 1);
            Dibujar(true);
        }
        if (tecla == ConsoleKey.Enter)
        {
            // Abrir requiere carta y atributo; responder usa solo la carta publicada en la solicitud.
            if (!atributo)
            {
                carta = s.IdsPersonajePermitidos[foco];
                if (s.Tipo == "cartaYAtributo") { atributo = true; foco = 0; Dibujar(true); return false; }
            }
```

### Dibujar: huella del estado visible

Una huella igual evita repintado; la orden forzada de navegación permite dibujar incluso sin cambios de red. No modifica el estado remoto.

```csharp
    private void Dibujar(bool forzar = false)
    {
        var v = estado.Vista;
        string nueva = JsonSerializer.Serialize(new { v, estado.Actual, aviso, menu, confirmarAbandono, leyendo, foco, atributo, pendiente, terminal });
        if (!forzar && huella == nueva) return;
        huella = nueva;
        interfaz.MostrarRed(configuracion.Nombre, v, estado.Actual, aviso, leyendo, menu, confirmarAbandono,
            foco, atributo, pendiente is not null, abandono);
    }
```

### CompletarEscritura: incorporación y abandono

El éxito de incorporación confirma UUID/rol y fija próximos ticks. La rama abandono eleva el suelo de revisión y exige otra presentación antes de mostrar el motivo final; la rama de recibo, incluida aquí como contexto, ya se explicó arriba.

```csharp
    private async Task CompletarEscritura()
    {
        var tarea = escritura!;
        escritura = null;
        try
        {
            // La respuesta se aplica dentro del coordinador, que es el único dueño del estado.
            object? respuesta = await tarea;
            fallosEscritura = 0;
            proximaEscritura = Ahora;
            switch (claseEscritura)
            {
                case "sesion":
                    var s = (SesionConfirmada)respuesta!;
                    // Una incorporación solo es válida si confirma la sesión solicitada y un rol conocido.
                    if (s.IdSesion != http.IdSesion || s.Rol is not ("J1" or "J2")) throw new FalloCliente("protocoloIncompatible");
                    incorporado = true;
                    aviso = $"{configuracion.Nombre} · Plaza confirmada: {s.Rol}";
                    proximaVista = Ahora;
                    proximaPresencia = Ahora.AddSeconds(5);
                    break;
                case "eleccion":
                case "recibo":
                    // Un recibo ajeno o incoherente no permite retirar la operación pendiente.
                    var r = (ReciboCliente)respuesta!;
                    if (pendiente is null || r.IdOperacion != pendiente.IdOperacion || r.IdSolicitud != pendiente.IdSolicitud || r.RevisionConfirmada <= pendiente.RevisionEsperada)
                        throw new FalloCliente("protocoloIncompatible");
                    // Se rechazan vistas anteriores al recibo para no reactivar una solicitud ya consumida.
                    revisionMinima = Math.Max(revisionMinima, r.RevisionConfirmada);
                    pendiente = null;
                    reciboAusente = false;
                    estado.PerderComunicacion(); // Un recibo no es una presentación del turno siguiente.
                    proximaVista = Ahora;
                    aviso = "Recepción confirmada. Esperando presentación del servidor.";
                    break;
                // El éxito confirma el abandono; todavía se consulta el motivo final y se excluyen vistas viejas.
                case "abandono":
                    revisionMinima = Math.Max(revisionMinima, (estado.Vista?.Revision ?? 0) + (terminal ? 0 : 1));
                    abandono = false;
                    estado.PerderComunicacion();
                    aviso = "Abandono confirmado. Consultando motivo final.";
                    proximaVista = Ahora;
                    break;
            }
```

## Relaciones y verificación

[SesionClienteTests](../../test/proyecto-integrador-final-tests/SesionClienteTests.md) define consulta/presencia mientras menú/resultado, consulta lenta sin solapamiento, pérdida antes/después de aplicar POST, recibo/cuerpo original, Enter separado, confirmación No/Sí, EOF local, reincorporación estable, incompatibilidad explícita, pausa durante selección, rechazo/operación nueva, abandono incierto y serializado, foco estable, lectura final y respuesta tardía descartada. Usa transporte controlado/reloj sustituible; [IntegracionRespuestaPerdidaTests](../../test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.md) añade servidor/SQLite temporal. No acreditan por sí solos una partida entre dos equipos o todos los teclados. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
