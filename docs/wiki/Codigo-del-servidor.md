# Código del servidor HTTP

El servidor agrupa sus responsabilidades en varios archivos. `RepositorioJuego` es una clase parcial distribuida entre almacenamiento, transiciones y presentación.

## `Program.cs`

[Ver `src/servidor/Program.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Program.cs)

- `Main` crea la aplicación con `ServidorHttp.Crear`, obtiene el repositorio del contenedor de dependencias, inicia el servidor, arranca la consola del operador en otra tarea y espera el apagado.

## `ServidorHttp.cs`

[Ver `src/servidor/Api/ServidorHttp.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/ServidorHttp.cs)

- El constructor primario de `ErrorApi` conserva el estado y código. `Entrada`, `NoEncontrado` e `Indisponible` construyen los errores 400, 404 y 503. `Responder` fija el estado y escribe código, mensaje e identificador de seguimiento en JSON.
- `Crear` construye el host, configura la escucha por defecto, registra repositorio y reloj como instancias compartidas, y exige tipos numéricos JSON estrictos. El middleware envuelve los handlers y traduce excepciones de entrada y SQLite al contrato de error.
- Dentro de `Crear`, el handler de alta valida la credencial de catálogo y llama a `DarDeAlta`. El grupo de partidas convierte los parámetros de la ruta y llama a `Incorporar`, `Renovar`, `Presentacion`, `Elegir`, `ConsultarRecibo` o `Abandonar`. El POST de elección usa 201 para una recepción nueva y 200 para una repetición confirmada.
- `Credencial` lee la cabecera de autorización como esquema Bearer. `IdSesion` interpreta `X-Sesion` como UUID o devuelve `Guid.Empty`.

## `ConsolaServidor.cs`

[Ver `src/servidor/Api/ConsolaServidor.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/ConsolaServidor.cs)

- `Ejecutar` escribe la ayuda y lee entradas hasta cancelación o fin de flujo. Muestra errores de negocio y de persistencia.
- `Comando` separa el verbo de sus argumentos, interpreta UUID, plaza o ID entero y llama a la operación correspondiente. `crear` imprime UUID y credenciales. `iniciar`, `cancelar`, `cerrar-sesion`, `reemplazar-credencial` y `desactivar` operan sobre la partida, sesión o catálogo.
- `Ayuda` escribe la lista de órdenes y su sintaxis.

## `PersonajeCatalogo.cs`

[Ver `src/servidor/Api/PersonajeCatalogo.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/PersonajeCatalogo.cs)

- `AltaPersonaje.Validar` exige descripción y dibujo presentes. Normaliza saltos de línea para contarlas, limita el dibujo a 25 líneas de 120 caracteres y rechaza tabulaciones. Delega los puntajes y el nombre al constructor de `Personaje`. Si todo es válido, crea el registro de catálogo.
- `PersonajeCatalogo` define nombre, descripción, dibujo, seis puntajes, ID y disponibilidad. Las propiedades se materializan como columnas.

## `BaseJuego.cs`

[Ver `src/servidor/Api/BaseJuego.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/BaseJuego.cs)

- El constructor primario de `BaseJuego` pasa las opciones a `DbContext`. Cada propiedad `DbSet` expone las entidades de una tabla, desde el catálogo hasta los recibos.
- `OnModelCreating` especifica nombres de tablas, claves principales simples o compuestas, relaciones y el índice de las solicitudes por partida. El ID del catálogo lo asigna el repositorio.
- `PartidaGuardada` contiene fase, revisión, números, solicitud y resultado. `ParticipanteGuardado` conserva plaza, nombre, credencial resumida mediante hash y marcador.

## `EstadoPartida.cs`

[Ver `src/servidor/Api/EstadoPartida.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/EstadoPartida.cs)

- `PersonajePartida.Copiar` toma los datos del catálogo para una partida. `Dominio` construye el `Personaje` con el que se evalúa un combate. `Vista` produce la carta que se puede enviar, con ID, texto y diccionario de atributos.
- `RondaGuardada` guarda mano inicial y victorias. `CombateGuardado` registra elector, ambas cartas, atributo y ganador. `SolicitudGuardada` asocia el turno a un combate, una plaza y un tipo de elección.
- `CartaVisible` es la representación de carta que se serializa. `Eleccion` lleva la entrada del jugador. `Recibo` identifica el resultado confirmado. `Confirmacion` indica si se devolvió una confirmación previa.
- `OperacionGuardada.Datos` reconstruye la elección normalizada para compararla con un reintento. `Recibo` reconstruye el UUID de solicitud y operación y la revisión que se confirmó.

## `RepositorioJuego.cs`

[Ver `src/servidor/Api/RepositorioJuego.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/RepositorioJuego.cs)

- `PartidaCreada` devuelve las credenciales al operador, `Incorporacion` recibe el nombre y UUID elegido y `Sesion` representa la plaza y su vencimiento.
- El constructor de `RepositorioJuego` conserva cadena y reloj, mantiene abierta la conexión si SQLite está en memoria y ejecuta `EnsureCreated`.
- `DarDeAlta` valida la entrada, calcula el siguiente ID positivo dentro de una escritura, agrega la fila y devuelve el ID. `CrearPartida` guarda una partida nueva y dos plazas con hashes de credenciales, y devuelve las credenciales originales al operador.
- `Desactivar` pone `Activo` en falso. `CerrarSesion` verifica la plaza y quita su sesión en memoria. `ReemplazarCredencial` guarda el hash de una credencial nueva y revoca la sesión de esa plaza. `Cancelar` exige una partida sin finalizar, aumenta su revisión y delega en `Finalizar`.
- `Incorporar` valida nombre y UUID, identifica la plaza por credencial y rechaza otra sesión vigente o un nombre distinto al guardado. Confirma los datos persistentes y luego instala la sesión con 15 segundos de vigencia.
- `Renovar` comprueba credencial y sesión vigentes, y actualiza su vencimiento en memoria. `Vigente` busca por partida y plaza y consulta el reloj. `ExigirSesion` exige además el UUID correcto.
- `Ejecutar<T>` toma el bloqueo, abre el contexto e inicia una transacción. Una escritura reserva el turno de SQLite antes de leer. Ejecuta la función recibida, guarda cambios en las escrituras y confirma. El contexto y la transacción se liberan al salir.
- `Abrir` configura EF Core con la cadena de archivo o la conexión en memoria. `Buscar` y `BuscarParticipante` consultan por clave y responden con error 404 si falta la fila.
- `Autenticar` calcula el hash de la credencial entregada y lo compara con los de la partida. `Hash` convierte a UTF-8 y aplica SHA-256. `NuevaCredencial` genera 32 bytes aleatorios y los representa en hexadecimal. `Dispose` libera la conexión en memoria si existe.

## `TransicionesPartida.cs`

[Ver `src/servidor/Api/TransicionesPartida.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/TransicionesPartida.cs)

- `Iniciar` exige fase inicial, dos sesiones y seis personajes activos. Copia el catálogo a la partida, llama a `Repartir` y aumenta la revisión.
- `Elegir` valida IDs, revisión y carta presentes, normaliza el atributo y abre una transacción de escritura. Primero autentica y busca el ID de operación por partida y plaza. Si ya existe, compara los datos y devuelve el recibo original o rechaza la reutilización incompatible.
- Para una elección nueva, `Elegir` exige una partida sin pausa, revisión idéntica, solicitud vigente del jugador y carta de su mano. El elector debe traer atributo y el rival debe omitirlo. Consume la solicitud y aumenta la revisión. La primera elección registra carta y atributo y abre la solicitud del rival. La segunda llama a `Resolver`. Guarda el recibo en la misma transacción.
- `ConsultarRecibo` autentica y recupera únicamente recibos de la propia plaza. `Abandonar` admite repetir el abandono del mismo jugador y, en una partida vigente, aumenta la revisión y finaliza sin adjudicar victoria.
- `Acceso` combina búsqueda de partida, autenticación y validación de sesión. `AmbosPresentes` comprueba dos sesiones. `Pausada` requiere una fase de elección y al menos un ausente. `Otro` intercambia J1 y J2.
- `NormalizarAtributo` conserva el nulo, recorta espacios, normaliza Unicode y unifica mayúsculas y el caso especial de Energía.
- `Repartir` mezcla las fichas, asigna orden y ubicación, añade la ronda y llama a `AbrirCombate`. `AbrirCombate` agrega el combate con su elector y abre una solicitud de carta y atributo. `AbrirSolicitud` crea el UUID, registra la plaza y ajusta la fase según el tipo de acción.
- `Resolver` materializa ambos personajes, arma un `Combate` de dominio, compara y traslada resultado, puntajes y ubicación a las filas. Si la ronda sigue, alterna elector y abre otro combate. Si se cierra, suma la ronda y alterna la mano o finaliza al llegar a 15.
- `Finalizar` consume la solicitud activa, limpia su referencia y conserva fase final, motivo y ganador cuando corresponde.

## `PresentacionPartida.cs`

[Ver `src/servidor/Api/PresentacionPartida.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Api/PresentacionPartida.cs)

- `SolicitudVisible` enumera las opciones permitidas. `ResultadoVisible` contiene cartas y puntajes de un combate cerrado. `PresentacionVisible` es la respuesta completa de una plaza.
- `Presentacion` abre una lectura autenticada, consulta participantes y cartas y filtra la mano por plaza. Incluye la solicitud solo para su titular cuando hay presencia. Construye el historial a partir de combates con ganador, agrega el ganador de ronda al último combate correspondiente y devuelve los datos de fase y marcador sin revelar manos rivales.

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
