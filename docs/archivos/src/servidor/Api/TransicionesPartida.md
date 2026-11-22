# TransicionesPartida.cs

Fuente: [src/servidor/Api/TransicionesPartida.cs](../../../../../src/servidor/Api/TransicionesPartida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión común: [catálogo](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Implementa la máquina de estados persistida de partidas en red: inicio, elecciones, combate, rondas, victoria y abandono. Es una porción de `internal sealed partial RepositorioJuego`; [RepositorioJuego.cs](RepositorioJuego.md) aporta conexión, transacciones, autenticación y sesiones; [PresentacionPartida](PresentacionPartida.md) publica vistas. No usa el bucle incompleto de Partida.Jugar: reconstruye cada Combate para aplicar sus reglas.

## Dependencias

[BaseJuego](BaseJuego.md) persiste claves/relaciones; [EstadoPartida](EstadoPartida.md) declara las entidades y contratos. [Combate](../Combate.md), [Jugador](../Jugador.md), [Personaje](../Personaje.md), [Ronda](../Ronda.md) y [Partida](../Partida.md) aportan comparación y constantes de 2 combates/15 rondas. Random.Shared.Shuffle mezcla copias. [Protocolo](../../../../protocolo-de-partida.md) define solicitudes, revisión e idempotencia.

## Estructura y estado

No declara otro tipo ni constructores, eventos o interfaces. Campo privado estático readonly `Dictionary<string,TipoAtributo> atributos`, con OrdinalIgnoreCase, contiene Fuerza, Velocidad, Resistencia, Inteligencia, Energía y Habilidad mapeados al enum. Readonly no vuelve inmutable el diccionario, aunque esta implementación no lo modifica tras inicializar. Los demás estados viven en entidades y en las sesiones de la otra porción; no hay campos de ronda en memoria en este archivo.

## Miembros y ejecución

### `internal void Iniciar(Guid id)`

Ejecuta transacción de escritura. Busca partida o 404; exige fase esperandoInicio (si no, 409 operacionIncompatible), luego ambos presentes (409 partidaPausada). Copia solo catálogo activo con PersonajePartida.Copiar; menos de seis falla 409 operacionIncompatible antes de agregar copias. Agrega las cartas, reparte ronda 1 mano J1, incrementa revisión una vez y confirma. Las copias aíslan partidas iniciadas de cambios futuros del catálogo.

### `internal Confirmacion Elegir(Guid id,string? credencial,Guid idSesion,Eleccion datos)`

Antes de escribir rechaza IdOperacion/IdSolicitud Empty, RevisionEsperada null o negativa e IdPersonaje null con 400 entradaInvalida. Normaliza atributo. Dentro de transacción llama Acceso; consulta operación previa por `(id,rol,idOperacion)` **antes** de evaluar pausa/revisión/turno. Si existe y Datos difiere de la elección normalizada, falla 409 operacionIncompatible; si coincide retorna recibo previo con Repetida true, sin consumir nada ni incrementar revisión.

Para primera operación verifica en este orden: no Pausada (409 partidaPausada); revisión igual (409 revisionDesactualizada); IdSolicitud igual al puntero actual, fila existente, no consumida y Rol propio (409 solicitudNoVigente). Busca carta por partida e ID; exige ubicación propia. Si tipo cartaYAtributo exige atributo no nulo y existente en el mapa; al responder carta exige atributo null. Cualquiera de estos errores produce 422 eleccionInvalida, sin cambiar estado.

Luego marca solicitud Consumida e incrementa revisión. Elector: guarda IdCartaElector y Atributo, abre solicitud carta al otro. Respondedor: llama Resolver. En ambos crea OperacionGuardada con entrada normalizada y revisión posterior, añade al contexto y retorna Confirmacion con Repetida false. SaveChanges y commit se realizan junto al avance por Ejecutar; un fallo revierte recibo y jugada. Fragmento real de la prioridad de idempotencia:

```csharp
            var anterior = db.Operaciones.Find(id, jugador.Rol, datos.IdOperacion);
            if (anterior is not null)
            {
                if (anterior.Datos() != datos)
                    throw new ErrorApi(409, "operacionIncompatible", "La operación ya tiene otros datos.");
                return new Confirmacion(anterior.Recibo(), true);
            }
```

La comparación de records incluye operación, solicitud, revisión esperada, ID de carta y atributo. Un reintento idéntico sigue siendo recuperable después de avanzar turno o finalizar, siempre que credencial/sesión sean válidas.

### Consulta y abandono

`internal Recibo ConsultarRecibo(Guid id,string? credencial,Guid idSesion,Guid idOperacion)` ejecuta lectura, exige Acceso y busca solo operación de la plaza propia. Devuelve Recibo o 404; no renueva presencia ni cambia revisión.

`internal void Abandonar(Guid id,string? credencial,Guid idSesion)` ejecuta escritura y exige Acceso. Si AbandonoDe ya es ese rol retorna sin cambios, incluso si la partida finalizó por ese abandono. Si finalizó por otra causa/rol, 409 operacionIncompatible. En otro caso guarda AbandonoDe, incrementa revisión y Finalizar con motivo abandono, sin ganador. No transforma una salida local en victoria del rival.

### Acceso, presencia y normalización

- `private (PartidaGuardada Partida,ParticipanteGuardado Jugador) Acceso(BaseJuego db,Guid id,string? credencial,Guid idSesion)`: Busca, Autenticar, ExigirSesion, en ese orden; retorna la tupla. No renueva sesión.
- `private bool AmbosPresentes(Guid id)`: exige Vigente de J1 y J2; el && evita consultar J2 si falta J1.
- `private bool Pausada(PartidaGuardada partida)`: true solo en esperandoElector/esperandoRival cuando no están ambos; preparación/finalizada nunca aparecen pausadas por esta regla. No guarda flag ni incrementa revisión.
- `private static string Otro(string rol)`: J1 → J2; cualquier otro string → J1. Solo se llama con roles internos válidos.
- `private static string? NormalizarAtributo(string? atributo)`: null sigue null; si no, Trim y Normalize (forma Unicode predeterminada). Energia sin tilde, en cualquier caso, pasa a Energía. Busca nombre canónico del diccionario ignorando caso; desconocido conserva texto normalizado para posterior rechazo. No convierte cadena vacía a null.

### Reparto y apertura

`private static void Repartir(BaseJuego db,PartidaGuardada partida,PersonajePartida[] cartas,int numero,string mano)` mezcla todo el array. For desde 0 hasta Length−1 asigna Orden=i y Ubicacion J1 para i<3, J2 para 3≤i<6 y mazo para el resto. Agrega ronda con Mano, asigna NumeroRonda y abre combate 1 con ese elector. No consulta catálogo ni añade rondas ganadas. Precondición interna: al menos seis cartas; Iniciar la garantiza y las rondas siguientes reutilizan el conjunto completo.

`private static void AbrirCombate(BaseJuego db,PartidaGuardada partida,int numero,string elector)` crea fila usando NumeroRonda no nulo, agrega, asigna NumeroCombate y abre solicitud cartaYAtributo del elector. `private static void AbrirSolicitud(BaseJuego db,PartidaGuardada partida,CombateGuardado combate,string rol,string tipo)` crea Guid nuevo, copia coordenadas, rol y tipo, deja Consumida false; agrega solicitud, actualiza IdSolicitud y pone esperandoRival si tipo carta, esperandoElector en cualquier otro caso.

### Resolución y finalización

`private static void Resolver(BaseJuego db,PartidaGuardada partida,CombateGuardado guardado,PersonajePartida cartaRival)` busca carta del elector, crea dos Jugador con nombres J1/J2, convierte cartas al dominio y las entrega a cada jugador temporal. Mapea atributo, crea Combate, fija atributo y ambas cartas y resuelve. Usa nombre del ganador como rol; empate gana rival del elector según Combate. Guarda carta rival, ambos puntajes y ganador, y ubica ambas cartas en jugada.

Incrementa victorias de combate de J1 o J2 en RondaGuardada. Si quedan por debajo de 2, abre combate siguiente con elector alternado y retorna. Si llega a 2, guarda ganador de ronda y suma una ronda al participante; si alcanza 15, Finalizar(victoria,ganador) y retorna **sin repartir otra ronda**. En otro caso carga todas las cartas de la partida y Repartir con número siguiente y mano opuesta. No incrementa revisión adicional: Elegir ya lo hizo una vez por elección.

Fragmento real que sustenta cierre de combate/ronda/partida:

```csharp
        var ronda = db.Rondas.Find(partida.Id, guardado.NumeroRonda)!;
        var victorias = ganador == "J1" ? ++ronda.VictoriasJ1 : ++ronda.VictoriasJ2;
        if (victorias < Ronda.CombatesParaGanar)
        {
            AbrirCombate(db, partida, guardado.Numero + 1, Otro(guardado.Elector));
            return;
        }

        ronda.Ganador = ganador;
        var jugador = db.Participantes.Find(partida.Id, ganador)!;
        if (++jugador.RondasGanadas >= Partida.RondasParaGanar)
        {
            Finalizar(db, partida, "victoria", ganador);
            return;
        }

        var cartas = db.Cartas.Where(p => p.IdPartida == partida.Id).ToArray();
        Repartir(db, partida, cartas, ronda.Numero + 1, Otro(ronda.Mano));
```

`private static void Finalizar(BaseJuego db,PartidaGuardada partida,string motivo,string? ganador = null)` consume solicitud actual si existe, deja IdSolicitud null, fase finalizada y motivo/ganador. No borra historial, no incrementa revisión ni entrega cartas; cada llamador gestiona su incremento. Usa null-forgiving en búsquedas internas porque presupone integridad de transiciones.

### Fragmentos de las decisiones y efectos restantes

En `Iniciar`, estos guardas preceden al reparto y evitan crear copias si falta presencia o catálogo. Tras agregar/repartir, la revisión sube una vez:

```csharp
        var partida = Buscar(db, id);
        if (partida.Fase != "esperandoInicio")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya se inició o finalizó.");
        if (!AmbosPresentes(id))
            throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");

        // Cada partida conserva su propia copia de los personajes activos al iniciar.
        var cartas = db.Catalogo.Where(p => p.Activo).ToArray()
            .Select(p => PersonajePartida.Copiar(id, p)).ToArray();
        if (cartas.Length < 6)
            throw new ErrorApi(409, "operacionIncompatible", "Se necesitan seis personajes activos.");
        db.Cartas.AddRange(cartas);
        Repartir(db, partida, cartas, 1, "J1");
        partida.Revision++;
```

En `Elegir`, las validaciones de primera operación preceden a toda mutación. `esElector` selecciona la regla de atributo: obligatorio al abrir y prohibido al responder:

```csharp
            if (Pausada(partida))
                throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");
            if (partida.Revision != datos.RevisionEsperada)
                throw new ErrorApi(409, "revisionDesactualizada", "La revisión ya cambió.");
            var solicitud = partida.IdSolicitud == datos.IdSolicitud
                ? db.Solicitudes.Find(datos.IdSolicitud) : null;
            if (solicitud is null || solicitud.Consumida || solicitud.Rol != jugador.Rol)
                throw new ErrorApi(409, "solicitudNoVigente", "La solicitud no está vigente para esta plaza.");

            var carta = db.Cartas.Find(id, datos.IdPersonaje.Value);
            bool esElector = solicitud.Tipo == "cartaYAtributo";
            if (carta is null || carta.Ubicacion != jugador.Rol ||
                (esElector ? datos.Atributo is null || !atributos.ContainsKey(datos.Atributo) : datos.Atributo is not null))
                throw new ErrorApi(422, "eleccionInvalida", "La carta o el atributo no está entre las opciones.");
```

La solicitud se consume y la revisión sube antes de bifurcar apertura/respuesta. La fila de operación se añade en la misma transacción; por eso respuesta perdida no exige volver a consumir la solicitud:

```csharp
            var combate = db.Combates.Find(id, solicitud.NumeroRonda, solicitud.NumeroCombate)!;
            solicitud.Consumida = true;
            partida.Revision++;
            if (esElector)
            {
                combate.IdCartaElector = carta.IdPersonaje;
                combate.Atributo = datos.Atributo;
                AbrirSolicitud(db, partida, combate, Otro(jugador.Rol), "carta");
            }
            else
            {
                Resolver(db, partida, combate, carta);
            }

            // El recibo se guarda en la misma transacción que la jugada que confirma.
            var operacion = new OperacionGuardada
            {
                IdPartida = id, Rol = jugador.Rol, IdOperacion = datos.IdOperacion,
                IdSolicitud = datos.IdSolicitud, RevisionEsperada = datos.RevisionEsperada.Value,
                IdPersonaje = carta.IdPersonaje, Atributo = datos.Atributo,
                RevisionConfirmada = partida.Revision
            };
            db.Operaciones.Add(operacion);
            return new Confirmacion(operacion.Recibo(), false);
```

En `Abandonar`, la repetición del mismo rol se acepta antes del control de fase finalizada; el incremento corresponde solo a la primera aceptación:

```csharp
        if (partida.AbandonoDe == jugador.Rol)
            return true;
        if (partida.Fase == "finalizada")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.");
        partida.AbandonoDe = jugador.Rol;
        partida.Revision++;
        Finalizar(db, partida, "abandono");
```

La normalización del atributo no transforma desconocidos en una opción válida. Devuelve el nombre canónico o el texto para que la validación posterior lo rechace:

```csharp
        if (atributo is null)
            return null;
        var texto = atributo.Trim().Normalize();
        if (texto.Equals("Energia", StringComparison.OrdinalIgnoreCase))
            return "Energía";
        return atributos.Keys.FirstOrDefault(a => a.Equals(texto, StringComparison.OrdinalIgnoreCase)) ?? texto;
```

El reparto reasigna **todas** las ubicaciones y el orden. Esto reincorpora cartas jugadas y no jugadas al empezar otra ronda:

```csharp
        Random.Shared.Shuffle(cartas);
        for (int i = 0; i < cartas.Length; i++)
        {
            cartas[i].Orden = i;
            cartas[i].Ubicacion = i < 3 ? "J1" : i < 6 ? "J2" : "mazo";
        }
        db.Rondas.Add(new RondaGuardada { IdPartida = partida.Id, Numero = numero, Mano = mano });
        partida.NumeroRonda = numero;
        AbrirCombate(db, partida, 1, mano);
```

`AbrirSolicitud` crea una oportunidad nueva (UUID distinto) y cambia fase según tipo; no reutiliza una solicitud consumida:

```csharp
        var solicitud = new SolicitudGuardada
        {
            Id = Guid.NewGuid(), IdPartida = partida.Id, NumeroRonda = combate.NumeroRonda,
            NumeroCombate = combate.Numero, Rol = rol, Tipo = tipo
        };
        db.Solicitudes.Add(solicitud);
        partida.IdSolicitud = solicitud.Id;
        partida.Fase = tipo == "carta" ? "esperandoRival" : "esperandoElector";
```

En Resolver, cartas temporales se convierten a dominio y el resultado se materializa antes de sumar la ronda. Los dos IDs y puntajes permiten publicar resultados completos, sin revelar elección antes de resolver:

```csharp
        var cartaElector = db.Cartas.Find(partida.Id, guardado.IdCartaElector!.Value)!;
        var elector = new Jugador(guardado.Elector);
        var rival = new Jugador(Otro(guardado.Elector));
        var personajeElector = cartaElector.Dominio();
        var personajeRival = cartaRival.Dominio();
        elector.RecibirCartas([personajeElector]);
        rival.RecibirCartas([personajeRival]);

        var atributo = atributos[guardado.Atributo!];
        var combate = new Combate(elector, rival);
        combate.SetAtributo(atributo);
        combate.SetCarta(elector, personajeElector);
        combate.SetCarta(rival, personajeRival);
        var ganador = combate.Resolver().GetNombre();

        guardado.IdCartaRival = cartaRival.IdPersonaje;
        guardado.PuntajeElector = personajeElector.GetAtributo(atributo);
        guardado.PuntajeRival = personajeRival.GetAtributo(atributo);
        guardado.Ganador = ganador;
        cartaElector.Ubicacion = cartaRival.Ubicacion = "jugada";
```

Finalizar retira la solicitud vigente y no toca revisión; así cancelación, abandono y victoria mantienen el incremento decidido por su llamador:

```csharp
        if (partida.IdSolicitud is Guid id)
            db.Solicitudes.Find(id)!.Consumida = true;
        partida.IdSolicitud = null;
        partida.Fase = "finalizada";
        partida.MotivoFin = motivo;
        partida.Ganador = ganador;
```

## Recorridos y ejemplos

Normal: tras iniciar seis cartas, revisión pasa 0→1 y J1 recibe solicitud S1. J1 elige carta propia A/Fuerza con revisión 1: se consume S1, revisión 2, solicitud S2 carta para J2. J2 responde B sin atributo, revisión 2: revisión 3 y combate resuelto; si A y B tienen fuerza 650 ambos, gana J2. Si J2 llevaba una victoria de combate suma la segunda y gana ronda. Si llevaba 14 rondas alcanza 15 y finaliza con victoria, sin otra solicitud.

Alternativo: reenviar exactamente operación de J1 recupera su recibo con revisión confirmada 2 aunque la partida esté en 3; cambiar solo la carta usando mismo IdOperacion falla operacionIncompatible. Primera operación con revisión 1 cuando actual es 3 falla revisionDesactualizada; mismo reintento auténtico no. Responder con atributo Fuerza falla 422. Recorridos de lectura, no peticiones ejecutadas en esta página.

## Errores, límites y efectos observables

Todas las mutaciones se agrupan con recibo en transacción. Se rechaza turno equivocado sin consumir solicitud. Presencia vencida impide Acceso de esa sesión; ausencia del otro jugador pausa nuevas elecciones. La carta del elector conserva ubicación propia hasta resolver, pero su solicitud ya fue consumida y no permite otra jugada. Los resultados aún incompletos no se publican por PresentacionPartida. No hay bot ni recuperación de esquema corrupto; los `!` no son comprobaciones en ejecución.

## Relaciones y verificación

[ApiHttpTests](../../../test/proyecto-integrador-final-tests/ApiHttpTests.md) define privacidad/idempotencia, rollback, reinicio, quince rondas y plazo de presencia. [ServidorContratoTests](../../../test/proyecto-integrador-final-tests/ServidorContratoTests.md) define elecciones concurrentes, pausa y recuperación de solicitud, desempates y abandono repetido. [IntegracionRespuestaPerdidaTests](../../../test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.md) define recuperación del recibo tras pérdida de respuesta. Consultar [plan](../../../../plan-de-pruebas.md) para ejecución real; esta lectura no acredita tests ejecutados.
