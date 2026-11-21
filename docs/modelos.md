# Modelos

Las firmas muestran la visibilidad, los tipos y nombres de parámetros, los valores por defecto y el retorno. Los constructores se identifican por el nombre del tipo y devuelven la instancia creada. La visibilidad efectiva queda limitada por la del tipo contenedor.

## Aplicación

### [src/cliente/DemostracionConsola.cs](../src/cliente/DemostracionConsola.cs)

**`internal sealed class DemostracionConsola(InterfazConsola interfaz, TextReader entrada, TextWriter salida, bool interactiva)`**

- `public void Recorrer(string? cierre)` — Ejecuta una partida simulada con marcador inicial de 14 rondas por plaza. `cierre` selecciona una interrupción simulada o, si es nulo, el recorrido hasta la victoria. La entrada de consola puede terminar con `EndOfStreamException`.
- `private void MostrarCierreSimulado(MesaVisible mesa, string cierre)` — Presenta en `mesa` la pausa, el abandono, la cancelación o la salida local seleccionada por `cierre`.
- `private void Continuar()` — Espera una línea en modo interactivo. Lanza `EndOfStreamException` si se cierra la entrada durante la espera.
- `private static CartaVisible Carta(int id, string nombre, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)` — Construye una carta visible de demostración con el identificador, el nombre y el mapa de los seis puntajes recibidos.

### [src/cliente/InterfazConsola.cs](../src/cliente/InterfazConsola.cs)

**`internal sealed class InterfazConsola`**

- `public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)` — Usa los flujos y el ancho provistos o toma los valores de la terminal. Con salida redirigida y ancho sin especificar muestra los paneles en forma lineal.
- `public void MostrarPreparacion(string? local, string estado)` — Muestra el estado de preparación y la identidad local si está disponible.
- `public void MostrarEsperaInicial(string local, string participantes, string motivo)` — Muestra los participantes, el motivo de la espera y que el inicio depende del operador.
- `public void MostrarInicioRonda(MesaVisible mesa, string mano, IReadOnlyList<CartaVisible> cartas)` — Presenta en la mesa quién abre, la mano propia y el marcador inicial.
- `public void MostrarMano(IReadOnlyList<CartaVisible> mano)` — Dibuja la mano sin resaltar una carta. Admite una colección vacía.
- `private void MostrarMano(IReadOnlyList<CartaVisible> mano, int? foco, string titulo = "Tu mano")` — Presenta la cantidad de cartas y sus fichas bajo `titulo`. Resalta el índice `foco` cuando se proporciona.
- `public void MostrarEsperaRival(MesaVisible mesa, string rival, IReadOnlyList<CartaVisible> cartas)` — Presenta la mano propia y el marcador mientras el rival indicado realiza su elección.
- `public CartaVisible PedirCarta(IReadOnlyList<CartaVisible> mano)` — Copia las opciones de la mano y devuelve la carta que se confirma. Lanza `InvalidOperationException` si la mano está vacía y `EndOfStreamException` si se cierra la entrada antes de confirmar.
- `public CartaVisible PedirCarta(MesaVisible mesa, IReadOnlyList<CartaVisible> cartas, bool abre)` — Devuelve la carta confirmada y usa `abre` para distinguir apertura de respuesta al dibujar la mesa. Una lista vacía produce `InvalidOperationException`. El fin de entrada produce `EndOfStreamException`.
- `public string PedirAtributo(IReadOnlyList<string>? permitidos = null, MesaVisible? mesa = null)` — Devuelve el atributo confirmado de `permitidos`. Si la lista es nula, ofrece los seis atributos. Usa `mesa` cuando está disponible. Una lista vacía produce `InvalidOperationException`. El fin de entrada produce `EndOfStreamException`.
- `public void MostrarEnvio(MesaVisible mesa, EstadoEnvio estado)` — Selecciona el mensaje de la jugada según `estado`. Lanza `ArgumentOutOfRangeException` para un valor de enumeración desconocido.
- `public void MostrarCombate(MesaVisible mesa, CombateVisible combate)` — Dibuja el encabezado de la mesa y delega la presentación del resultado en la otra sobrecarga.
- `public void MostrarCombate(CombateVisible combate)` — Presenta el atributo disputado, las cartas jugadas, sus puntajes y el ganador del combate.
- `public void MostrarCierreRonda(MesaVisible mesa, string ganador, string? proximoMano)` — Muestra el ganador de la ronda y, si `proximoMano` está presente, quién abre la siguiente.
- `public void MostrarCierrePartida(string local, string ganador, JugadorVisible j1, JugadorVisible j2)` — Presenta el ganador y las rondas ganadas por ambos jugadores.
- `public void MostrarSuspension(MesaVisible mesa, string motivo, bool confirmada)` — Presenta el motivo, si la pausa fue confirmada y el último marcador de la mesa.
- `public void MostrarInterrupcion(string local, string causa, string efecto, bool confirmada)` — Muestra una salida local o un final confirmado según `confirmada`, junto con la causa y su efecto.
- `public void MostrarMarcador(JugadorVisible j1, JugadorVisible j2, int ronda)` — Muestra el número de ronda y las rondas ganadas por cada jugador.
- `public void MostrarEsperando(string mensaje)` — Muestra el mensaje de espera si difiere del último guardado y actualiza ese registro.
- `public void MostrarGanadorRonda(JugadorVisible ganador)` — Muestra el nombre del ganador de la ronda y su total de rondas ganadas.
- `public void MostrarGanadorPartida(JugadorVisible ganador)` — Muestra el nombre del ganador de la partida y su total de rondas ganadas.
- `public void MostrarError(string mensaje)` — Escribe el mensaje con estilo de error.
- `public void MostrarEstado(string titulo, string mensaje)` — Dibuja el encabezado con `titulo` y escribe el mensaje.
- `private void Encabezado(MesaVisible mesa, string? detalle = null)` — Extrae la identidad local y los números de ronda y combate de la mesa para dibujar el encabezado.
- `private void Encabezado(string? local, string situacion, string? detalle = null)` — Limpia la pantalla cuando la terminal está configurada para ello. Presenta identidad, situación y detalle opcional en una línea o en filas según el ancho.
- `private void Pie(string mensaje)` — Dibuja una separación y escribe el mensaje al pie.
- `private void Paneles(params Panel[] paneles)` — Distribuye los paneles en columnas cuando hay ancho suficiente y la salida admite esa disposición. En los demás casos dibuja cada panel por separado.
- `private static Panel PanelInfo(string titulo, string detalle)` — Convierte el texto del detalle en filas y devuelve un panel con encabezado.
- `private static Panel PanelInfo(string titulo, IRenderable detalle)` — Envuelve el contenido renderizable y devuelve un panel expandible con encabezado.
- `private static Panel Ficha(CartaVisible carta, bool enfocada)` — Construye un panel con el nombre y los atributos de la carta. Aplica el estilo de foco cuando `enfocada` es verdadero.
- `private static Panel FichaJugada(CartaJugada jugada, bool ganadora)` — Construye un panel con jugador, carta y puntaje. Resalta la jugada ganadora según `ganadora`.
- `private static string ResumenMano(IReadOnlyList<CartaVisible> cartas)` — Devuelve los nombres de las cartas en orden o el mensaje de mano vacía.
- `private static string Rondas(MesaVisible mesa)` — Devuelve el marcador de rondas ganadas de ambas plazas.
- `private static string Combates(MesaVisible mesa)` — Devuelve el marcador de combates ganados en la ronda actual.
- `private static string Marcador(MesaVisible mesa)` — Devuelve una línea para los combates y otra para las rondas.
- `private static CartaVisible[] CopiarOpciones(IReadOnlyList<CartaVisible> mano)` — Devuelve una copia superficial de la lista seleccionable. Lanza `InvalidOperationException` si está vacía.
- `private static Rows ListaOpciones(IEnumerable<string> opciones, int foco)` — Devuelve las filas de opciones resaltando el índice indicado por `foco`.
- `private static Text Opcion(string opcion, bool enfocada)` — Devuelve el texto de una opción, con indicador y estilo cuando tiene el foco.
- `private int Elegir(int cantidad, Action<int> dibujar)` — Invoca `dibujar` con el foco inicial y después de cada movimiento válido. Limita el índice a `[0, cantidad - 1]` y lo devuelve al confirmar. Propaga `EndOfStreamException` si se cierra la entrada.
- `private ConsoleKey LeerTecla()` — Devuelve una tecla física o interpreta una línea de entrada redirigida. Una línea vacía equivale a Enter y una entrada desconocida a `ConsoleKey.NoName`. Lanza `EndOfStreamException` al agotar las líneas.
- `private static string TextoVisible(string texto)` — Devuelve el texto con los caracteres de control, formato y separación de líneas o párrafos sustituidos por espacios.
- `private void Escribir(string texto, Style? estilo = null)` — Divide el texto en líneas y las envía a la terminal con el estilo indicado.
- `private void Renderizar(IRenderable contenido)` — Escribe el contenido renderizable en la terminal configurada.

### [src/cliente/Program.cs](../src/cliente/Program.cs)

**`internal static class Program`**

- `private static Task<int> Main()`. Solicita la configuración del cliente REST y ejecuta el coordinador con una sesión por proceso. Permite corregir identidad o conexión tras un rechazo. Ctrl+C y EOF cierran sin abandonar. Devuelve 0 al finalizar o salir localmente y 1 ante un error de E/S.

### Componentes del cliente REST

- `ConfiguracionCliente`: lectura cancelable, validación por campo, credencial oculta y normalización de URL con prefijo.
- `ContratosPartida`: mensajes JSON de sesión, presentación, solicitud, elección y recibo; no referencia entidades del servidor.
- `ClientePartidaHttp`: métodos HTTP, cabeceras de plaza/sesión, timeout y clasificación de errores sin mostrar cuerpos ni credenciales.
- `EstadoCliente`: valida presentaciones completas y mantiene la línea de base del historial y la cola de resultados nuevos.
- `SesionPartidaCliente`: coordina red, entrada, operación pendiente, reincorporación, lectura de resultados y salida. Acepta un TimeProvider para verificar intervalos.
- `EntradaCliente`: eventos de teclas o líneas, cancelación y descarte de entrada interactiva sobrante.
- `InterfazConsola.Red`: presentación de estados remotos y menú sin lecturas bloqueantes; reconstruye los marcadores de cada resultado hasta ese combate.

### [src/cliente/VistasConsola.cs](../src/cliente/VistasConsola.cs)

**`internal sealed record CartaVisible( int IdPersonaje, string Nombre, IReadOnlyDictionary<string, int> Atributos)`**

Datos de una ficha seleccionable. El ID distingue cartas con el mismo nombre.

**`internal sealed record JugadorVisible(string Nombre, int RondasGanadas)`**

Nombre y marcador de rondas de una plaza.

**`internal sealed record CartaJugada(string Jugador, string Carta, int Valor)`**

Jugador, nombre de la carta y valor del atributo disputado que se muestran al resolver el combate.

**`internal sealed record CombateVisible( string Atributo, CartaJugada J1, CartaJugada J2, string Ganador, int CombatesGanadosJ1, int CombatesGanadosJ2, string? ProximoEnElegir, bool? GanaJ1 = null)`**

Resultado para la pantalla del combate. `GanaJ1` identifica la plaza ganadora cuando se conoce. Si es nulo, la interfaz compara el nombre de J1 con `Ganador`. `ProximoEnElegir` se omite cuando no hay próximo elector.

**`internal sealed record MesaVisible( string Local, JugadorVisible J1, JugadorVisible J2, int Ronda, int Combate, int CombatesJ1, int CombatesJ2)`**

Contexto compartido por las pantallas. Identifica al usuario local, ambos marcadores, la ronda, el combate y las victorias de cada plaza en la ronda.

**`internal enum EstadoEnvio`**

Estados de presentación de una elección. Admite `Enviando`, `Recibido` y `Pendiente` de confirmación.

### [src/servidor/Api/BaseJuego.cs](../src/servidor/Api/BaseJuego.cs)

**`internal sealed class BaseJuego(DbContextOptions<BaseJuego> opciones)`**

Contexto EF Core que registra las ocho entidades de SQLite. Recibe las opciones del proveedor.

- `protected override void OnModelCreating(ModelBuilder modelo)` — Configura los nombres de tablas, claves, relaciones e índices de las entidades persistidas. El ID de catálogo se asigna fuera de EF Core. Los detalles del esquema están en [Bases de datos](bases-de-datos.md).

**`internal sealed class PartidaGuardada`**

Raíz del estado persistido. Conserva la fase, la revisión, el turno activo y el resultado final.

**`internal sealed class ParticipanteGuardado`**

Plaza persistida de la partida. Conserva nombre, hash de credencial y rondas ganadas. La vigencia de la sesión se mantiene en el repositorio en memoria.

### [src/servidor/Api/ConsolaServidor.cs](../src/servidor/Api/ConsolaServidor.cs)

**`internal static class ConsolaServidor`**

- `internal static void Ejecutar(RepositorioJuego juego, TextReader entrada, TextWriter salida, CancellationToken detener)` — Muestra la ayuda y procesa líneas de `entrada` hasta el fin de flujo o la cancelación. Informa los errores de negocio y los fallos de SQLite o EF Core en `salida`.
- `private static void Comando(RepositorioJuego juego, string linea, TextReader entrada, TextWriter salida)` — Interpreta la acción y solicita por separado sus identificadores. Valida UUID no vacío, plaza J1/J2 e ID positivo antes de invocar el repositorio. Fin de entrada interrumpe la acción sin aplicarla.
- `private static void Ayuda(TextWriter salida)` — Escribe los comandos disponibles, agrupados por función, con su efecto y las indicaciones para responder las preguntas.

### [src/servidor/Api/EstadoPartida.cs](../src/servidor/Api/EstadoPartida.cs)

**`internal sealed class PersonajePartida`**

Copia del personaje para una partida, con ubicación y orden de reparto propios.

- `internal static PersonajePartida Copiar(Guid partida, PersonajeCatalogo origen)` — Devuelve una instantánea de los datos del catálogo para la partida indicada. Conserva el ID del personaje y comienza con ubicación de mazo.
- `internal Personaje Dominio()` — Construye el `Personaje` que emplea el combate de dominio. Propaga su validación si los datos persistidos incumplen las reglas del personaje.
- `internal CartaVisible Vista()` — Proyecta el ID, los textos y el mapa de los seis atributos al formato visible de la API.

**`internal sealed record CartaVisible(int IdPersonaje, string Nombre, string Descripcion, string Ascii, Dictionary<string, int> Atributos)`**

Proyección de una carta para el contrato HTTP, con los datos descriptivos y los atributos publicados.

**`internal sealed class RondaGuardada`**

Fila de una ronda. Registra la plaza mano, las victorias por plaza y el ganador si se definió.

**`internal sealed class CombateGuardado`**

Fila de combate con el elector, ambas cartas, atributo y resultados a medida que se confirman.

**`internal sealed class SolicitudGuardada`**

Acción pendiente o consumida, asociada a una plaza y un combate. [Claves y ciclo de vida](bases-de-datos.md#solicitud)

**`internal sealed record Eleccion(Guid IdOperacion, Guid IdSolicitud, int? RevisionEsperada, int? IdPersonaje, string? Atributo)`**

Cuerpo de entrada de la API. La revisión y la carta admiten nulo en el tipo para detectar una omisión en la validación. El atributo es opcional cuando el turno solo pide carta.

**`internal sealed record Recibo(Guid IdOperacion, Guid IdSolicitud, int RevisionConfirmada)`**

Identificadores de operación y solicitud con la revisión que confirmó la elección.

**`internal sealed record Confirmacion(Recibo Recibo, bool Repetida)`**

Resultado interno del registro. `Repetida` distingue un recibo recuperado de un alta de operación.

**`internal sealed class OperacionGuardada`**

Elección confirmada persistida por partida, plaza e ID de operación. [Claves y transacción](bases-de-datos.md#operacion)

- `internal Eleccion Datos()` — Reconstruye la elección confirmada con la revisión esperada y el atributo normalizado para comparar un reintento.
- `internal Recibo Recibo()` — Proyecta los IDs de operación y solicitud y la revisión confirmada al recibo HTTP.

### [src/servidor/Api/PersonajeCatalogo.cs](../src/servidor/Api/PersonajeCatalogo.cs)

**`internal sealed record AltaPersonaje( string? Nombre, string? Descripcion, string? Ascii, int Fuerza, int Velocidad, int Resistencia, int Inteligencia, int Energia, int Habilidad)`**

Cuerpo del alta de catálogo. La validación convierte los campos de entrada en una entidad persistible.

- `internal PersonajeCatalogo Validar()` — Devuelve la entidad de catálogo sin asignar su ID. Responde con `400 entradaInvalida` si faltan descripción o ASCII, el dibujo contiene tabulaciones o supera 25 líneas de 120 caracteres, o el nombre o los atributos incumplen el contrato de `Personaje`.

**`internal sealed class PersonajeCatalogo`**

Entrada del catálogo. Conserva los datos descriptivos, puntajes e indicador de disponibilidad para nuevas partidas.

### [src/servidor/Api/PresentacionPartida.cs](../src/servidor/Api/PresentacionPartida.cs)

**`internal sealed record SolicitudVisible(Guid IdSolicitud, string Tipo, int[] IdsPersonajePermitidos, string[] AtributosPermitidos)`**

Acción expuesta a la plaza autorizada y opciones de carta y atributo que puede enviar.

**`internal sealed record ResultadoVisible(int NumeroRonda, int NumeroCombate, Dictionary<string, CartaVisible> Cartas, string Atributo, Dictionary<string, int> Puntajes, string GanadorCombate, string? GanadorRonda)`**

Combate resuelto con cartas y puntajes por plaza. Incluye ganador de ronda en el combate que la cierra.

**`internal sealed record PresentacionVisible(Guid IdPartida, int Revision, string Fase, bool Pausada, string RolPropio, Dictionary<string, string?> Nombres, int? NumeroRonda, int? NumeroCombate, Dictionary<string, int> Marcador, CartaVisible[] Mano, SolicitudVisible? Solicitud, ResultadoVisible[] Resultados, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MotivoFin, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Ganador)`**

Respuesta de consulta por plaza. Reúne la mano propia, fase, revisión, opción vigente, marcador e historial. Los campos finales nulos se omiten del JSON.

**`internal sealed partial class RepositorioJuego`**

- `internal PresentacionVisible Presentacion(Guid id, string? credencial, Guid idSesion)` — Valida la credencial y la sesión de la plaza y devuelve una vista con su mano, marcador e historial de combates resueltos. Incluye la solicitud solo para la plaza habilitada cuando la partida no está pausada. Propaga los errores de acceso de `Acceso`.

### [src/servidor/Api/RepositorioJuego.cs](../src/servidor/Api/RepositorioJuego.cs)

**`internal sealed record PartidaCreada(Guid Id, string CredencialJ1, string CredencialJ2)`**

Identidad de una partida nueva y credenciales originales de sus dos plazas para el operador.

**`internal sealed record Incorporacion(Guid IdSesion, string? Nombre)`**

Entrada de incorporación con UUID de sesión y nombre de jugador.

**`internal sealed record Sesion(string Rol, Guid IdSesion, DateTimeOffset VenceUtc)`**

Reserva en memoria de una plaza hasta `VenceUtc`. El UUID distingue sesiones de la misma plaza.

**`internal sealed partial class RepositorioJuego`**

Coordina la persistencia de partidas y catálogo y administra las sesiones en memoria.

- `internal RepositorioJuego(string cadena, TimeProvider reloj)` — Configura persistencia y vencimiento de sesiones con `cadena` y `reloj`. Abre y conserva la conexión para SQLite en memoria y crea el esquema si corresponde. Propaga los fallos de apertura o inicialización.
- `internal int DarDeAlta(AltaPersonaje datos)` — Valida `datos`, reserva el siguiente ID positivo y guarda el personaje. Devuelve el ID. Propaga `400 entradaInvalida` y devuelve `409 operacionIncompatible` si se agotaron los IDs.
- `internal PartidaCreada CrearPartida()` — Persiste la partida inicial y dos plazas con hashes de credenciales. Devuelve el UUID y las credenciales originales de J1 y J2 al operador.
- `internal void Desactivar(int id)` — Marca como inactivo el personaje identificado por `id` para futuros repartos. Devuelve `404 recursoNoEncontrado` si falta la fila.
- `internal void CerrarSesion(Guid id, string rol)` — Verifica que la plaza exista y elimina su sesión en memoria. Devuelve `404 recursoNoEncontrado` si la plaza no existe.
- `internal string ReemplazarCredencial(Guid id, string rol)` — Persiste el hash de una credencial nueva para la plaza y revoca su sesión vigente. Devuelve la credencial nueva. Devuelve `404 recursoNoEncontrado` si falta la plaza.
- `internal void Cancelar(Guid id)` — Incrementa la revisión y finaliza la partida con motivo `cancelacion`. Devuelve `404 recursoNoEncontrado` si falta la partida o `409 operacionIncompatible` si ya finalizó.
- `private static ParticipanteGuardado BuscarParticipante(BaseJuego db, Guid id, string rol)` — Devuelve la entidad de la plaza por clave `(id, rol)` o genera `404 recursoNoEncontrado`.
- `internal Sesion Incorporar(Guid id, string? credencial, Incorporacion datos)` — Identifica la plaza por credencial, registra el nombre y devuelve una sesión de 15 segundos. Devuelve 400 para nombre o UUID de sesión inválidos, 403 si el nombre registrado difiere y 409 si otra sesión ocupa la plaza. Propaga los errores de búsqueda y autenticación.
- `internal void Renovar(Guid id, string? credencial, Guid idSesion)` — Verifica partida, credencial y UUID de sesión y extiende el vencimiento en memoria. Conserva la revisión. Propaga los errores de acceso de `Acceso`.
- `private T Ejecutar<T>(bool escritura, Func<BaseJuego, T> operacion)` — Serializa el acceso, abre un contexto y ejecuta `operacion` en una transacción. Para escritura reserva el turno de SQLite antes de leer y llama a `SaveChanges` antes de confirmar. Devuelve el resultado de la función y propaga los fallos de la operación o del almacenamiento.
- `private BaseJuego Abrir()` — Devuelve un contexto EF Core configurado con la conexión en memoria o la cadena del repositorio.
- `private static PartidaGuardada Buscar(BaseJuego db, Guid id)` — Devuelve la partida identificada por `id` o genera `404 recursoNoEncontrado`.
- `private static ParticipanteGuardado Autenticar(BaseJuego db, Guid id, string? credencial)` — Compara el hash de la credencial con los de las plazas de la partida. Devuelve la plaza correspondiente o genera `401 credencialInvalida`.
- `private Sesion? Vigente(Guid id, string rol)` — Devuelve la sesión de la partida y plaza si todavía está vigente según el reloj. En los demás casos devuelve `null`.
- `private Sesion ExigirSesion(Guid id, string rol, Guid idSesion)` — Devuelve la sesión vigente si su UUID coincide con `idSesion`. Genera `409 sesionVencida` cuando falta, venció o su UUID difiere.
- `private static byte[] Hash(string credencial)` — Devuelve el resumen SHA-256 de la codificación UTF-8 de la credencial.
- `private static string NuevaCredencial()` — Genera 32 bytes aleatorios y devuelve su representación hexadecimal.
- `public void Dispose()` — Libera la conexión SQLite en memoria si el repositorio conserva una.

### [src/servidor/Api/ServidorHttp.cs](../src/servidor/Api/ServidorHttp.cs)

**`internal sealed class ErrorApi(int estado, string codigo, string mensaje)`**

Excepción con estado HTTP, código estable y mensaje para el contrato REST.

- `internal static ErrorApi Entrada()` — Construye el error HTTP `400 entradaInvalida`.
- `internal static ErrorApi NoEncontrado()` — Construye el error HTTP `404 recursoNoEncontrado`.
- `internal static ErrorApi Indisponible()` — Construye el error HTTP `503 servicioNoDisponible`.
- `internal Task Responder(HttpContext contexto)` — Escribe en la respuesta de `contexto` el estado y el JSON con código, mensaje e identificador de seguimiento. Devuelve la tarea de escritura.

**`internal static class ServidorHttp`**

- `internal static WebApplication Crear(string[] args, TimeProvider? reloj = null, ConfiguracionServidor? inicio = null)` — Configura y devuelve el host con sus rutas, repositorio y reloj. Si recibe `inicio`, sus valores prevalecen en dirección, SQLite y credencial de catálogo. Conserva los argumentos internos y el reloj inyectable para pruebas aisladas. El middleware traduce errores de entrada y almacenamiento al contrato HTTP.
- `private static string? Credencial(HttpContext contexto)` — Extrae la credencial de una cabecera `Authorization` con esquema Bearer, o devuelve `null`.
- `private static Guid IdSesion(HttpContext contexto)` — Interpreta `X-Sesion` como UUID o devuelve `Guid.Empty`.

### [src/servidor/Api/TransicionesPartida.cs](../src/servidor/Api/TransicionesPartida.cs)

**`internal sealed partial class RepositorioJuego`**

- `internal void Iniciar(Guid id)` — Copia el catálogo activo, reparte la primera ronda y aumenta la revisión. Requiere fase inicial, dos sesiones vigentes y seis personajes activos. Genera 404 para una partida inexistente y 409 si una precondición de estado falla.
- `internal Confirmacion Elegir(Guid id, string? credencial, Guid idSesion, Eleccion datos)` — Valida entrada y acceso. Devuelve el recibo previo si los datos normalizados coinciden con la operación registrada. Para una elección nueva consume la solicitud, aplica la jugada y guarda el recibo en la misma transacción. Los rechazos por cuerpo, estado o selección se definen en el [contrato HTTP](protocolo-de-partida.md#contrato-http).
- `internal Recibo ConsultarRecibo(Guid id, string? credencial, Guid idSesion, Guid idOperacion)` — Valida el acceso y devuelve el recibo por ID de operación de la plaza autenticada. Genera 404 si esa plaza no tiene el recibo solicitado.
- `internal void Abandonar(Guid id, string? credencial, Guid idSesion)` — Finaliza con motivo `abandono` e incrementa la revisión. Repetir el abandono desde la misma plaza conserva el estado. Genera 409 si la partida finalizó por otra causa y propaga los errores de acceso.
- `private (PartidaGuardada Partida, ParticipanteGuardado Jugador) Acceso( BaseJuego db, Guid id, string? credencial, Guid idSesion)` — Devuelve la partida y su participante tras buscar la partida, autenticar la credencial y exigir la sesión indicada. Genera 404, 401 o 409 según qué comprobación falle.
- `private bool AmbosPresentes(Guid id)` — Devuelve si J1 y J2 tienen sesiones vigentes para la partida.
- `private bool Pausada(PartidaGuardada partida)` — Devuelve verdadero si la fase espera una elección y falta al menos una sesión vigente.
- `private static string Otro(string rol)` — Devuelve la plaza opuesta a `rol`. El llamador usa exclusivamente `J1` o `J2`.
- `private static string? NormalizarAtributo(string? atributo)` — Conserva el nulo. Para texto, recorta espacios, normaliza Unicode y equipara mayúsculas y las variantes de Energía.
- `private static void Repartir(BaseJuego db, PartidaGuardada partida, PersonajePartida[] cartas, int numero, string mano)` — Mezcla las cartas, asigna orden y ubicación, registra la ronda con su plaza mano y abre el primer combate.
- `private static void AbrirCombate(BaseJuego db, PartidaGuardada partida, int numero, string elector)` — Agrega el combate numerado a la ronda actual y abre una solicitud de carta y atributo para el elector.
- `private static void AbrirSolicitud(BaseJuego db, PartidaGuardada partida, CombateGuardado combate, string rol, string tipo)` — Crea y vincula la solicitud al combate. Actualiza el ID activo y la fase de la partida según el tipo de elección.
- `private static void Resolver(BaseJuego db, PartidaGuardada partida, CombateGuardado guardado, PersonajePartida cartaRival)` — Compara las cartas con el atributo elegido mediante `Combate`. Persiste resultado, puntajes y ubicación de las cartas. Alterna elector, abre otra ronda o finaliza según el marcador.
- `private static void Finalizar(BaseJuego db, PartidaGuardada partida, string motivo, string? ganador = null)` — Consume la solicitud activa si existe, limpia su ID y guarda la fase final, el motivo y el ganador opcional. El llamador administra el incremento de revisión.

### [src/servidor/Combate.cs](../src/servidor/Combate.cs)

**`internal class Combate`**

- `public Combate(Jugador quienElige, Jugador rival)` — Registra el elector y el rival como instancias distintas. Lanza `ArgumentNullException` si falta un jugador y `ArgumentException` si ambos son la misma instancia.
- `public void SetAtributo(TipoAtributo tipo)` — Registra `tipo` una sola vez. Lanza `InvalidOperationException` si ya se eligió y `ArgumentOutOfRangeException` si el valor no pertenece a `TipoAtributo`.
- `public void SetCarta(Jugador jugador, Personaje personaje)` — Retira de la mano y registra la carta del participante tras validarla. Lanza `ArgumentNullException` por valores nulos, `ArgumentException` si el jugador es ajeno o la carta no está en su mano, e `InvalidOperationException` si ya registró una carta. Una validación fallida conserva la mano.
- `public bool EstaCompleto()` — Devuelve verdadero cuando el combate tiene atributo y ambas cartas.
- `public Jugador Resolver()` — Compara el atributo elegido, guarda y devuelve al ganador. El empate favorece al rival. Lanza `InvalidOperationException` si faltan elecciones o si el combate ya se resolvió.
- `public Jugador GetGanador()` — Devuelve el ganador guardado o lanza `InvalidOperationException` si el combate sigue sin resolver.
- `public TipoAtributo GetAtributo()` — Devuelve el atributo elegido o lanza `InvalidOperationException` si todavía falta.
- `public Personaje GetCarta(Jugador jugador)` — Devuelve la carta de `jugador` una vez completo el combate. Lanza `ArgumentNullException` si el jugador es nulo, `InvalidOperationException` si falta completar el combate y `ArgumentException` si el jugador no participa.
- `public Jugador[] GetParticipantes()` — Devuelve un arreglo nuevo con el elector primero y el rival segundo.
- `private void ValidarParticipante(Jugador jugador)` — Comprueba pertenencia por identidad de instancia. Lanza `ArgumentException` si el jugador es ajeno al combate.
- `private void ValidarCartaDisponible(Jugador jugador, Personaje personaje)` — Lanza `InvalidOperationException` si el participante ya registró una carta y `ArgumentException` si el personaje no está en su mano.
- `private void RegistrarCarta(Jugador jugador, Personaje personaje)` — Guarda el personaje en el campo del elector o del rival. Recibe un participante previamente validado.

### [src/servidor/Jugador.cs](../src/servidor/Jugador.cs)

**`internal class Jugador`**

- `public Jugador(string nombre)` — Inicializa una mano vacía y cero rondas ganadas. Lanza `ArgumentException` si el nombre es nulo, vacío o solo contiene espacios.
- `public void RecibirCartas(Personaje[] personajes)` — Agrega las cartas al final de la mano en el orden recibido. Lanza `ArgumentNullException` si el arreglo es nulo.
- `public void QuitarCarta(Personaje personaje)` — Elimina la primera coincidencia de la carta en la mano. Si está ausente, conserva la mano. Lanza `ArgumentNullException` si el personaje es nulo.
- `public Personaje[] EntregarCartas()` — Devuelve una copia de la mano y vacía la colección interna.
- `public Personaje[] GetCartas()` — Devuelve una copia superficial de la mano y conserva sus elementos.
- `public void SumarRondaGanada()` — Incrementa el contador de rondas ganadas.
- `public int GetRondasGanadas()` — Devuelve el contador de rondas ganadas.
- `public string GetNombre()` — Devuelve el nombre registrado en el constructor.
- `public bool TieneCarta(Personaje personaje)` — Devuelve si la mano contiene la instancia de `personaje`. Lanza `ArgumentNullException` para una referencia nula.

### [src/servidor/Mazo.cs](../src/servidor/Mazo.cs)

**`internal class Mazo`**

- `public Mazo(List<Personaje> personajes)` — Copia la lista de origen y conserva su orden. Lanza `ArgumentNullException` si la lista es nula.
- `public void Mezclar()` — Reordena el mazo en el lugar mediante Fisher–Yates con `Random.Shared`. Conserva sus cartas.
- `public Personaje[] Repartir(int cantidad)` — Devuelve y retira las primeras `cantidad` cartas. Un valor negativo produce `ArgumentOutOfRangeException` y uno superior al tamaño produce `ArgumentException`. En ambos casos el mazo conserva su contenido.
- `public void Devolver(Personaje[] personajes)` — Agrega las cartas al final del mazo, en el orden recibido. Lanza `ArgumentNullException` si el arreglo es nulo.
- `public int GetCantidad()` — Devuelve la cantidad actual de cartas.

### [src/servidor/Partida.cs](../src/servidor/Partida.cs)

**`internal class Partida`**

- `public Partida(Jugador j1, Jugador j2, Mazo mazo)` — Conserva los jugadores y el mazo e inicializa la partida sin ronda activa. Lanza `ArgumentNullException` si alguna referencia es nula.
- `public Jugador Jugar()` — Intenta iniciar el flujo en memoria y devolver al primer jugador que alcance 15 rondas. En el código actual, tras un reparto válido, `JugarRonda` lanza `InvalidOperationException` en el primer combate porque el flujo todavía carece de entrada de elecciones.
- `private void Iniciar()` — Exige seis cartas, crea la primera ronda con J1 como mano y reparte. Si el mazo es insuficiente, lanza `InvalidOperationException` antes de cambiar el estado.
- `private void RepartirCartas()` — Mezcla el mazo y entrega tres cartas a cada jugador. Requiere al menos seis cartas disponibles.
- `private void JugarRonda()` — Crea un combate para el próximo elector. El flujo actual lanza `InvalidOperationException` al comprobar que ese combate recién creado todavía carece de carta y atributo.
- `private void RecogerCartas()` — Reúne las cartas de los combates de la ronda y vacía ambas manos. Devuelve todas las cartas reunidas al mazo. Requiere una ronda actual y combates completos.
- `private bool EstaTerminada()` — Devuelve si al menos uno de los jugadores alcanzó 15 rondas ganadas.
- `private Jugador DeterminarGanadorPartida()` — Devuelve a J1 si alcanzó el umbral y a J2 en el otro caso. Se usa después de comprobar que la partida terminó.

### [src/servidor/Personaje.cs](../src/servidor/Personaje.cs)

**`internal enum TipoAtributo`**

Identifica fuerza, velocidad, resistencia, inteligencia, energía y habilidad.

**`internal class Personaje`**

- `public Personaje( string nombre, string ascii, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)` — Guarda el nombre, el ASCII y los seis puntajes. Valida los datos antes de completar la construcción y propaga `ArgumentException` si incumplen alguna regla.
- `private void Validar()` — Comprueba nombre con texto, ASCII no nulo, seis puntajes entre 250 y 750 inclusive, suma exacta de 3000, al menos uno ≥650 y otro ≤350. Lanza `ArgumentException` si falla una regla.
- `public string GetNombre()` — Devuelve el nombre validado.
- `public string GetAscii()` — Devuelve el arte ASCII original. Puede ser una cadena vacía.
- `public int GetAtributo(TipoAtributo tipo)` — Devuelve el puntaje correspondiente a `tipo`. Lanza `ArgumentOutOfRangeException` para valores ajenos a la enumeración.

### [src/servidor/Program.cs](../src/servidor/Program.cs)

**`internal static class Program`**

- `private static async Task<int> Main()` — Lee `ConfiguracionServidor` antes de construir el host, inicia servidor y consola administrativa. El fin de entrada administrativa detiene el host; Ctrl+C también permite apagarlo. Ante un fallo de inicio vuelve a pedir la configuración sin imprimir sus secretos. Devuelve 0 al cerrar normalmente y 1 si no puede leer la consola.

### [src/servidor/Ronda.cs](../src/servidor/Ronda.cs)

**`internal class Ronda`**

- `public Ronda(Jugador mano, Jugador rival)` — Registra quién abre como mano y crea un historial vacío. Lanza `ArgumentNullException` si falta un jugador y `ArgumentException` si ambos son la misma instancia.
- `public void AgregarCombate(Combate combate)` — Incorpora el combate al historial. Lanza `ArgumentNullException` si es nulo e `InvalidOperationException` si la ronda ya está definida o el historial llegó a tres. El llamador debe entregar un combate resuelto para que se pueda calcular el marcador.
- `public bool EstaDefinida()` — Devuelve si alguno de los jugadores ganó al menos dos combates. Propaga `InvalidOperationException` si el historial contiene un combate sin resolver.
- `public Jugador GetGanador()` — Devuelve al jugador que alcanzó dos victorias. Lanza `InvalidOperationException` si todavía falta definir la ronda o algún combate agregado sigue sin resolver.
- `public Jugador GetQuienEligeProximo()` — Devuelve mano con un número par de combates y rival con uno impar. Lanza `InvalidOperationException` si la ronda ya se definió o el marcador no se puede calcular.
- `public Combate[] GetCombates()` — Devuelve una copia superficial del historial en orden de incorporación.

### [src/subir-personajes/Program.cs](../src/subir-personajes/Program.cs)

**`internal static class Program`**

- `private static Task<int> Main()` — Delega en `ConsolaAlta.Iniciar` con la entrada y salida del proceso, sin argumentos de aplicación.

**`internal static class ConsolaAlta`**

- `internal static async Task<int> Iniciar(TextReader entrada, TextWriter salida)` — Solicita URL (Enter: `http://localhost:5000`) y credencial no vacía antes del personaje. Configura el cliente HTTP sin redirecciones y delega la carga. Fin de entrada incompleta devuelve 1.

- `internal static async Task<int> Ejecutar(HttpClient cliente, string credencial, TextReader entrada, TextWriter salida)` — Lee y permite corregir nombre, ASCII y atributos (rango, suma y especialización) antes de enviar el alta autenticada y confirma únicamente una respuesta `201` con ID positivo. Devuelve 0 al cerrar tras una carga confirmada y 1 ante rechazo, ID inválido, entrada incompleta o fallo de comunicación o deserialización.
- `private static bool EsSi(string respuesta)` — Devuelve si `respuesta` equivale a `sí` o `si`, sin distinguir mayúsculas.

**`private sealed record AltaConfirmada(int IdPersonaje)`**

## Entrada de consola compartida

### [src/comun/EntradaConsola.cs](../src/comun/EntradaConsola.cs)

**`internal sealed class EntradaConsola(TextReader entrada, TextWriter salida)`**

- `Texto(string pregunta)` devuelve una línea o lanza `EndOfStreamException`.
- `Validar(string pregunta, Func<string, bool> valida, string error, string? defecto = null)` repite un campo inválido y permite un valor por defecto.
- `Url(string defecto)` valida una URL HTTP/HTTPS de origen, sin credenciales, consulta ni fragmento.
- `Secreto(string pregunta, bool permiteVacio)` oculta la entrada en una terminal interactiva y lee líneas si está redirigida. No imprime su contenido. Permite borrar caracteres y finalizar con Enter; Ctrl+D o Ctrl+Z interrumpen la lectura secreta.

### [src/servidor/Api/ConfiguracionServidor.cs](../src/servidor/Api/ConfiguracionServidor.cs)

**`internal sealed record ConfiguracionServidor(string Direccion, string Archivo, string Credencial)`**

`Leer(TextReader entrada, TextWriter salida)` obtiene los tres datos antes del host. Usa `http://0.0.0.0:5000` y `guerra.db` por defecto, valida la ruta y avisa si una credencial vacía deshabilita el alta. Una configuración incompleta no construye el host.

## Pruebas automatizadas

Los casos se agrupan por archivo. Las verificaciones de ejecución están en [Plan de pruebas](plan-de-pruebas.md).

### [test/proyecto-integrador-final-tests/ApiHttpTests.cs](../test/proyecto-integrador-final-tests/ApiHttpTests.cs)

**`public class ApiHttpTests`**

- `public async Task PresentacionDePartidaDesconocidaDevuelveErrorDelProtocolo()` — Consulta un UUID inexistente y comprueba estado, código e ID de seguimiento.
- `public async Task IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion()` — Compara la incorporación original, su repetición y la colisión con otro UUID.
- `public async Task AltaExigeCredencialAdministrativaYValidaAtributos()` — Contrasta alta sin credencial, alta inválida y alta válida con ID.
- `public async Task CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez()` — Envía elecciones concurrentes, consulta el recibo propio y ajeno y luego verifica cuándo aparece el combate completo.
- `public async Task ReinicioRecuperaSolicitudYRecibosYExigeSesionesNuevas()` — Vuelve a abrir la misma base después de cerrar el host y comprueba que los datos persisten mientras las sesiones se pierden.
- `public async Task UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales()` — Induce un fallo de almacenamiento, comprueba que no haya recibo ni avance y vuelve a enviar.
- `public async Task PresenciaRenuevaElPlazoSinCambiarLaRevision()` — Controla el reloj para observar vigencia y revisión por separado.
- `public async Task ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza()` — Rota la credencial, vuelve a incorporar y consulta el estado cancelado.
- `public async Task PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra()` — Completa la partida por HTTP y verifica la fase y el marcador finales.
- `public async Task AltaRechazaPuntajesRepresentadosComoTexto()` — Envía puntajes como cadenas JSON y verifica el rechazo.
- `private static async Task Error(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)` — Comprueba el estado HTTP, el código de error y el identificador de seguimiento.
- `private static object DatosEleccion(JsonElement vista, string? atributo)` — Construye el cuerpo de una elección desde la solicitud, revisión y opciones de la vista.
- `private static async Task<HttpClient> Incorporar(string url, Guid partida, string credencial, string nombre)` — Abre una sesión y devuelve un cliente HTTP configurado con su credencial e identificador.
- `private static async Task<JsonElement> Vista(HttpClient cliente)` — Solicita la presentación y devuelve el JSON tras comprobar el estado de respuesta.

**`private sealed class RelojPruebas`**

- `public override DateTimeOffset GetUtcNow()` — Devuelve el instante controlado por la prueba.
- `internal void Avanzar(int segundos)` — Suma los segundos indicados al reloj de prueba.

### [test/proyecto-integrador-final-tests/CartasDePrueba.cs](../test/proyecto-integrador-final-tests/CartasDePrueba.cs)

**`internal static class CartasDePrueba`**

- `internal static Personaje Crear(int fuerza = 650)` — Fabrica un personaje válido con fuerza configurable y ajusta otro puntaje para conservar el total.

### [test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs](../test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs)

**`public sealed class ClienteConsolaContratoTests`**

- `public async Task Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince()` — Verifica que el ejecutable emita pantallas y marcadores en orden.
- `public async Task Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta()` — Cambia el foco con texto y observa qué se confirmó.
- `public async Task Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir()` — Arranca en modo pausa y comprueba que el marcador no cambie.
- `public async Task Demo_DistingueAbandonoCancelacionYSalidaLocal()` — Ejecuta los tres modos y compara sus mensajes.
- `public async Task Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada()` — Cierra la entrada y busca el cierre local.
- `private static void EnOrden(string salida, params string[] fragmentos)` — Verifica que cada fragmento aparezca en la salida después del anterior.
- `private static async Task<string> EjecutarDemo(string? modo, string entrada)` — Ejecuta la DLL con el modo indicado y entrada redirigida. Devuelve la salida capturada.

### [test/proyecto-integrador-final-tests/CombateTests.cs](../test/proyecto-integrador-final-tests/CombateTests.cs)

**`public class CombateTests`**

- `public void ValidaParticipantesYDevuelveUnaCopiaEnOrden()` — Prueba constructores inválidos y modifica un arreglo devuelto para comprobar que los participantes se conservan.
- `public void RechazaConsultasPrematurasYAtributosInvalidosORepetidos()` — Consulta antes de elegir y comprueba las excepciones y la inmutabilidad de la primera elección.
- `public void RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador()` — Mezcla cartas ajenas, jugadores inválidos y elecciones repetidas. Comprueba qué cartas se retiraron y cuáles quedan.
- `public void FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo(bool juegaElector)` — Alterna el participante que entrega la única carta y exige que resolver falle.
- `public void ResuelveSegunElAtributoElegidoYDaElEmpateAlRival(int fuerzaElector, int fuerzaRival, bool ganaElector)` — Parametriza mayor, menor e igualdad y compara la instancia ganadora.

### [test/proyecto-integrador-final-tests/ConsolaAltaTests.cs](../test/proyecto-integrador-final-tests/ConsolaAltaTests.cs)

**`public class ConsolaAltaTests`**

- `public async Task CargaDosPersonajesManualmenteYConfirmaSusIds()` — Alimenta dos formularios por texto, envía las dos altas a un servidor local con SQLite en memoria y comprueba los IDs impresos.
- `public async Task UnAltaRechazadaNoSeAnunciaComoConfirmada()` — Provoca una respuesta no autorizada y comprueba el mensaje y código de salida.

### [test/proyecto-integrador-final-tests/ConsolaServidorTests.cs](../test/proyecto-integrador-final-tests/ConsolaServidorTests.cs)

**`public sealed class ConsolaServidorTests`**

Las pruebas verifican ayuda sin argumentos, corrección del UUID, elección de plaza y fin de entrada durante acciones incompletas sobre un repositorio SQLite en memoria.

### [test/proyecto-integrador-final-tests/ContratoFactAttribute.cs](../test/proyecto-integrador-final-tests/ContratoFactAttribute.cs)

**`public sealed class ContratoFactAttribute`**

- `public ContratoFactAttribute(string variableDll)` El constructor de `ContratoFactAttribute` lee la variable de ruta de DLL y establece `Skip` si falta. Así los casos por proceso se descubren aunque sus binarios no estén configurados.

### [test/proyecto-integrador-final-tests/InterfazConsolaTests.cs](../test/proyecto-integrador-final-tests/InterfazConsolaTests.cs)

**`public class InterfazConsolaTests`**

- `private static CartaVisible Carta(int id, string nombre)` — Crea una vista de carta para alimentar la interfaz.
- `public void PedirCarta_ReintentaEntradasInvalidasYConservaLaMano()` — Alimenta líneas inválidas y válidas y comprueba la selección sin mutar la mano.
- `public void ManoVacia_SePuedeMostrarPeroNoSolicitar()` — Comprueba el mensaje de mano vacía y la excepción de selección.
- `public void PedirAtributo_OfreceLosSeisAtributos(int pasos, string esperado)` — Desplaza el foco la cantidad indicada y comprueba el atributo seleccionado.
- `public void PedirAtributo_RespetaLasOpcionesDeLaSolicitud()` — Comprueba que la selección use las opciones restringidas de la solicitud.
- `public void FinDeEntrada_TerminaLaSolicitud()` — Provoca fin de lectura y espera la excepción de entrada.
- `public void MostrarMano_AdaptaLasFichasAlAncho(int ancho, bool enParalelo)` — Comprueba si las fichas se disponen en paralelo para el ancho dado.
- `public void MostrarCombate_PresentaElResultadoConfirmado(int ancho)` — Verifica cartas, puntajes y ganador en la salida para el ancho dado.
- `public void PresentaMarcadorEstadosYCierresConLosNombresRecibidos()` — Comprueba los nombres y mensajes en marcador, estados y pantallas de cierre.

### [test/proyecto-integrador-final-tests/JugadorTests.cs](../test/proyecto-integrador-final-tests/JugadorTests.cs)

**`public class JugadorTests`**

- `public void IniciaConNombreManoVaciaYSinVictorias()` — Inspecciona los campos observables del jugador recién construido.
- `public void AcumulaCartasQuitaUnaYEntregaElResto()` — Combina varias recepciones, prueba la copia defensiva, retira una carta y comprueba el vaciado y el marcador.

### [test/proyecto-integrador-final-tests/MazoTests.cs](../test/proyecto-integrador-final-tests/MazoTests.cs)

**`public class MazoTests`**

- `public void CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal()` — Modifica la lista original, reparte por tramos y verifica orden y devolución.
- `public void MezclarConservaTodasLasCartas(int cantidad)` — Mezcla mazos de distintos tamaños y verifica que cada carta permanezca sin imponer un orden aleatorio concreto.

### [test/proyecto-integrador-final-tests/PantallasConsolaTests.cs](../test/proyecto-integrador-final-tests/PantallasConsolaTests.cs)

**`public class PantallasConsolaTests`**

- `public void PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles()` — Llama a tres pantallas y busca información específica de cada una.
- `public void Apertura_UsaMesaConFichasEncabezadoYPie(int ancho, bool paralelas)` — Ejecuta la apertura a diferentes anchos.
- `public void EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo()` — Usa la misma mesa para revisar espera y respuesta.
- `public void EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados()` — Comprueba los mensajes de envío, suspensión e interrupción.
- `public void ResultadoYRondas_UsanDueloVictoriaYPlanilla()` — Comprueba los paneles y textos del combate, ronda y partida.

### [test/proyecto-integrador-final-tests/PartidaTests.cs](../test/proyecto-integrador-final-tests/PartidaTests.cs)

**`public class PartidaTests`**

- `public void RechazaUnMazoInsuficienteAntesDeRepartir(int cantidad)` — Llama a `Jugar` con tamaños menores a seis, espera la excepción de inicio y comprueba que el mazo y las manos conservan su estado.

### [test/proyecto-integrador-final-tests/PersonajeTests.cs](../test/proyecto-integrador-final-tests/PersonajeTests.cs)

**`public class PersonajeTests`**

- `public void ConservaNombreAsciiYLosSeisAtributos()` — Construye un personaje y compara cada getter con su entrada.
- `public void AceptaUmbralesInclusivosYAsciiVacio()` — Usa valores límite y una cadena de dibujo vacía.
- `public void RechazaAtributosQueIncumplenUnaRegla(int fuerza, int velocidad, int resistencia, int inteligencia)` — Inyecta varios conjuntos con `InlineData` y espera una excepción de validación.
- `public void RechazaNombreSinTexto(string? nombre)` — Verifica que el constructor lance `ArgumentException` para nombres nulos o en blanco.
- `public void RechazaAsciiNulo()` — Verifica que el constructor rechace el ASCII nulo.

### [test/proyecto-integrador-final-tests/RondaTests.cs](../test/proyecto-integrador-final-tests/RondaTests.cs)

**`public class RondaTests`**

- `public void EmpiezaSinCombatesYEligeElJugadorMano()` — Confirma el estado inicial y la excepción al consultar ganador.
- `public void DosVictoriasSeguidasCierranLaRonda(bool ganaMano)` — Parametriza al vencedor y prueba cierre temprano.
- `public void ElTercerCombateDefineLaRondaCuandoVanUnoAUno(bool ganaMano)` — Genera un empate parcial y completa el tercer combate con cada posible ganador.
- `private static Combate ResolverCombate(Jugador elector, Jugador oponente, Jugador ganador)` — Prepara cartas y atributo para producir el ganador que necesita el escenario, luego devuelve el combate ya resuelto.

### [test/proyecto-integrador-final-tests/ServidorContratoTests.cs](../test/proyecto-integrador-final-tests/ServidorContratoTests.cs)

**`public sealed class ServidorContratoTests`**

- `public async Task Incorporacion_ValidaCredencialYSesionYNoCambiaRevisionConPresencia()` — Compara incorporación, renovación y estados de error vía HTTP.
- `public async Task Elecciones_ProtegenManosYDevuelvenElMismoReciboAlReintentar()` — Toma una vista por plaza y repite la elección para comprobar privacidad y el recibo original.
- `public async Task EleccionesConcurrentes_ConsumenUnaSolaSolicitud()` — Dispara dos operaciones sobre la misma solicitud y verifica que avance una vez.
- `public async Task SesionAusente_PausaYRecuperaLaMismaSolicitudSinCambiarRevision()` — Cierra una sesión y luego la reincorpora para observar la misma solicitud.
- `public async Task Partida_AlternaElElectorEnEmpatesYFinalizaAlLlegarAQuinceRondas()` — Conduce los turnos hasta el marcador final y compara la alternancia y los empates.
- `public async Task Reinicio_InvalidaSesionesYConservaEstadoYReciboConfirmados()` — Reinicia el proceso sobre el archivo SQLite anterior y verifica estado y autenticación.
- `public async Task Abandono_RepetirloConservaMarcadorYRevisionFinal()` — Envía dos veces el abandono y compara ambas presentaciones finales.
- `private static int Revision(JsonElement vista)` — Extrae la revisión de la presentación JSON.
- `private static int[] Mano(JsonElement vista)` — Devuelve los IDs de los personajes visibles en la mano.
- `private static Eleccion Elegir(JsonElement vista)` — Construye una elección desde la revisión y las opciones de la solicitud visible.
- `private static async Task<JsonElement> Leer(HttpResponseMessage respuesta, HttpStatusCode esperado)` — Verifica el estado esperado y devuelve el cuerpo JSON.
- `private static async Task Error(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)` — Verifica el estado y el código de error de la respuesta.

**`private sealed record Eleccion(Guid idOperacion, Guid idSolicitud, int revisionEsperada, int idPersonaje, string? atributo)`**

**`private sealed record Jugador(string Rol, string Credencial, Guid Sesion)`**

**`private sealed class Servidor`**

- `public Servidor()` — Prepara el proceso de servidor para usar una base SQLite aislada.
- `private Process Lanzar()` — Arranca el proceso de servidor con la configuración y base de la prueba.
- `public async Task Reiniciar()` — Detiene y vuelve a lanzar el servidor sobre la misma base.
- `public async Task Crear()` — Envía la orden de crear y lee el UUID y las credenciales de la partida.
- `public async Task Iniciar()` — Envía al operador la orden de iniciar la partida creada.
- `public async Task Renovar(Jugador jugador)` — Renueva la presencia por HTTP para el jugador indicado.
- `public Task CerrarSesion(Jugador jugador)` — Envía a la consola del servidor la orden de cerrar la sesión de la plaza.
- `public async Task<JsonElement> EsperarVista(Jugador jugador, Func<JsonElement, bool> coincide)` — Consulta la presentación hasta obtener una respuesta que cumpla `coincide`.
- `public async Task Incorporar(Jugador jugador, string nombre)` — Incorpora la plaza por HTTP con el nombre y el UUID de sesión del jugador.
- `public async Task<JsonElement> Vista(Jugador jugador)` — Consulta la presentación autenticada y devuelve su JSON.
- `public Task<HttpResponseMessage> Post(Jugador jugador, Eleccion eleccion)` — Envía la elección autenticada y devuelve la respuesta HTTP.
- `public async Task<HttpResponseMessage> Enviar(HttpMethod metodo, string ruta, Jugador jugador, object? cuerpo = null)` — Envía una solicitud con las cabeceras de la plaza y el cuerpo opcional.
- `private async Task<string> LeerDato(string prefijo)` — Lee la salida hasta encontrar el prefijo indicado. Lanza `InvalidOperationException` si se cierra el flujo antes de encontrarlo.
- `public async ValueTask DisposeAsync()` — Detiene el proceso, libera el cliente HTTP y elimina el directorio temporal.
- `private async Task Detener()` — Finaliza el proceso de servidor de la prueba y espera su salida.

### Pruebas de entrada interactiva

[ConfiguracionServidorTests](../test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs) cubre valores iniciales, configuración personalizada, corrección y fin de entrada. [EntradaProcesosContratoTests](../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs) arranca los tres ejecutables sin argumentos de aplicación y verifica EOF, cierre del host, corrección de una base inválida y alta real por HTTP. Las ampliaciones de `ConsolaAltaTests` verifican el orden de las preguntas y las validaciones. `ClienteConsolaContratoTests` conserva las variantes de demostración mediante el auxiliar interno; el ejecutable REST se verifica con los contratos de arranque y proceso del spec 001. [Evidencia por criterio](plan-de-pruebas.md#evidencia-del-spec-003)
