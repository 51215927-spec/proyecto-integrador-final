# Modelos

Las firmas muestran la visibilidad, los tipos y nombres de parámetros, los valores por defecto y el retorno. Los constructores se identifican por el nombre del tipo y devuelven la instancia creada. La visibilidad efectiva queda limitada por la del tipo contenedor.

## Firmas declaradas

### [src/cliente/DemostracionConsola.cs](../src/cliente/DemostracionConsola.cs)

**`internal sealed class DemostracionConsola(InterfazConsola interfaz, TextReader entrada, TextWriter salida, bool interactiva)`**

- `public void Recorrer(string? cierre)` El constructor primario de `DemostracionConsola` recibe la interfaz y las entradas y salidas. `Recorrer` crea una mesa ficticia con 14 rondas por jugador, muestra la preparación y pide elecciones. Calcula el puntaje de cada combate con datos locales y actualiza los records mediante `with`. Termina cuando uno gana dos combates y muestra una victoria de partida.
- `private void MostrarCierreSimulado(MesaVisible mesa, string cierre)` `MostrarCierreSimulado` presenta la pausa o simula abandono, cancelación o salida local. `Continuar` espera Enter en modo interactivo y señala el fin de entrada si falta una línea. `Carta` construye un record de presentación a partir de los seis valores.
- `private void Continuar()`. Errores explícitos en el cuerpo: `EndOfStreamException`. `MostrarCierreSimulado` presenta la pausa o simula abandono, cancelación o salida local. `Continuar` espera Enter en modo interactivo y señala el fin de entrada si falta una línea. `Carta` construye un record de presentación a partir de los seis valores.
- `private static CartaVisible Carta(int id, string nombre, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)` `MostrarCierreSimulado` presenta la pausa o simula abandono, cancelación o salida local. `Continuar` espera Enter en modo interactivo y señala el fin de entrada si falta una línea. `Carta` construye un record de presentación a partir de los seis valores.

### [src/cliente/InterfazConsola.cs](../src/cliente/InterfazConsola.cs)

**`internal sealed class InterfazConsola`**

- `public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)` `InterfazConsola(...)` configura la lectura por teclas o líneas, crea el `IAnsiConsole` con su salida y permite fijar el ancho para pruebas. Si la salida está redirigida y no hay ancho explícito, usa presentación lineal.
- `public void MostrarPreparacion(string? local, string estado)` `MostrarPreparacion` coloca identidad y estado en paneles. `MostrarEsperaInicial` presenta participantes, motivo y comienzo a cargo del operador. `MostrarInicioRonda` muestra quién abre, la mano resumida y el marcador.
- `public void MostrarEsperaInicial(string local, string participantes, string motivo)` `MostrarPreparacion` coloca identidad y estado en paneles. `MostrarEsperaInicial` presenta participantes, motivo y comienzo a cargo del operador. `MostrarInicioRonda` muestra quién abre, la mano resumida y el marcador.
- `public void MostrarInicioRonda(MesaVisible mesa, string mano, IReadOnlyList<CartaVisible> cartas)` `MostrarPreparacion` coloca identidad y estado en paneles. `MostrarEsperaInicial` presenta participantes, motivo y comienzo a cargo del operador. `MostrarInicioRonda` muestra quién abre, la mano resumida y el marcador.
- `public void MostrarMano(IReadOnlyList<CartaVisible> mano)` La sobrecarga pública de `MostrarMano` llama a la privada sin foco. La privada imprime la cantidad, trata la mano vacía y crea una ficha por carta. `MostrarEsperaRival` presenta la mano propia y el marcador mientras el rival elige.
- `private void MostrarMano(IReadOnlyList<CartaVisible> mano, int? foco, string titulo = "Tu mano")` La sobrecarga pública de `MostrarMano` llama a la privada sin foco. La privada imprime la cantidad, trata la mano vacía y crea una ficha por carta. `MostrarEsperaRival` presenta la mano propia y el marcador mientras el rival elige.
- `public void MostrarEsperaRival(MesaVisible mesa, string rival, IReadOnlyList<CartaVisible> cartas)` La sobrecarga pública de `MostrarMano` llama a la privada sin foco. La privada imprime la cantidad, trata la mano vacía y crea una ficha por carta. `MostrarEsperaRival` presenta la mano propia y el marcador mientras el rival elige.
- `public CartaVisible PedirCarta(IReadOnlyList<CartaVisible> mano)` Las dos sobrecargas de `PedirCarta` copian las opciones, llaman a `Elegir` y devuelven la carta del índice confirmado. La que recibe `MesaVisible` ajusta texto, marcador y ayuda según si abre el turno o responde.
- `public CartaVisible PedirCarta(MesaVisible mesa, IReadOnlyList<CartaVisible> cartas, bool abre)` Las dos sobrecargas de `PedirCarta` copian las opciones, llaman a `Elegir` y devuelven la carta del índice confirmado. La que recibe `MesaVisible` ajusta texto, marcador y ayuda según si abre el turno o responde.
- `public string PedirAtributo(IReadOnlyList<string>? permitidos = null, MesaVisible? mesa = null)`. Errores explícitos en el cuerpo: `InvalidOperationException`. `PedirAtributo` usa las opciones autorizadas cuando se proporcionan y, si faltan, usa los seis nombres de atributo. Dibuja la lista y devuelve la opción confirmada.
- `public void MostrarEnvio(MesaVisible mesa, EstadoEnvio estado)`. Errores explícitos en el cuerpo: `ArgumentOutOfRangeException`. `MostrarEnvio` usa `EstadoEnvio` para elegir el texto y presentarlo en el cuerpo o al pie de la pantalla. `MostrarCombate(MesaVisible, CombateVisible)` añade el encabezado de la mesa y llama a la sobrecarga que dibuja el atributo, las dos jugadas, el ganador y el marcador.
- `public void MostrarCombate(MesaVisible mesa, CombateVisible combate)` `MostrarEnvio` usa `EstadoEnvio` para elegir el texto y presentarlo en el cuerpo o al pie de la pantalla. `MostrarCombate(MesaVisible, CombateVisible)` añade el encabezado de la mesa y llama a la sobrecarga que dibuja el atributo, las dos jugadas, el ganador y el marcador.
- `public void MostrarCombate(CombateVisible combate)` `MostrarEnvio` usa `EstadoEnvio` para elegir el texto y presentarlo en el cuerpo o al pie de la pantalla. `MostrarCombate(MesaVisible, CombateVisible)` añade el encabezado de la mesa y llama a la sobrecarga que dibuja el atributo, las dos jugadas, el ganador y el marcador.
- `public void MostrarCierreRonda(MesaVisible mesa, string ganador, string? proximoMano)` `MostrarCierreRonda` presenta el ganador y el próximo jugador mano si existe. `MostrarCierrePartida` crea la planilla de ambos marcadores. `MostrarSuspension` y `MostrarInterrupcion` distinguen la información confirmada de la que sigue pendiente.
- `public void MostrarCierrePartida(string local, string ganador, JugadorVisible j1, JugadorVisible j2)` `MostrarCierreRonda` presenta el ganador y el próximo jugador mano si existe. `MostrarCierrePartida` crea la planilla de ambos marcadores. `MostrarSuspension` y `MostrarInterrupcion` distinguen la información confirmada de la que sigue pendiente.
- `public void MostrarSuspension(MesaVisible mesa, string motivo, bool confirmada)` `MostrarCierreRonda` presenta el ganador y el próximo jugador mano si existe. `MostrarCierrePartida` crea la planilla de ambos marcadores. `MostrarSuspension` y `MostrarInterrupcion` distinguen la información confirmada de la que sigue pendiente.
- `public void MostrarInterrupcion(string local, string causa, string efecto, bool confirmada)` `MostrarCierreRonda` presenta el ganador y el próximo jugador mano si existe. `MostrarCierrePartida` crea la planilla de ambos marcadores. `MostrarSuspension` y `MostrarInterrupcion` distinguen la información confirmada de la que sigue pendiente.
- `public void MostrarMarcador(JugadorVisible j1, JugadorVisible j2, int ronda)` `MostrarMarcador` imprime nombres, victorias y ronda. `MostrarEsperando` evita repetir el mismo mensaje consecutivo. `MostrarGanadorRonda`, `MostrarGanadorPartida` y `MostrarError` escriben mensajes especializados. `MostrarEstado` arma un encabezado y escribe el mensaje.
- `public void MostrarEsperando(string mensaje)` `MostrarMarcador` imprime nombres, victorias y ronda. `MostrarEsperando` evita repetir el mismo mensaje consecutivo. `MostrarGanadorRonda`, `MostrarGanadorPartida` y `MostrarError` escriben mensajes especializados. `MostrarEstado` arma un encabezado y escribe el mensaje.
- `public void MostrarGanadorRonda(JugadorVisible ganador)` `MostrarMarcador` imprime nombres, victorias y ronda. `MostrarEsperando` evita repetir el mismo mensaje consecutivo. `MostrarGanadorRonda`, `MostrarGanadorPartida` y `MostrarError` escriben mensajes especializados. `MostrarEstado` arma un encabezado y escribe el mensaje.
- `public void MostrarGanadorPartida(JugadorVisible ganador)` `MostrarMarcador` imprime nombres, victorias y ronda. `MostrarEsperando` evita repetir el mismo mensaje consecutivo. `MostrarGanadorRonda`, `MostrarGanadorPartida` y `MostrarError` escriben mensajes especializados. `MostrarEstado` arma un encabezado y escribe el mensaje.
- `public void MostrarError(string mensaje)` `MostrarMarcador` imprime nombres, victorias y ronda. `MostrarEsperando` evita repetir el mismo mensaje consecutivo. `MostrarGanadorRonda`, `MostrarGanadorPartida` y `MostrarError` escriben mensajes especializados. `MostrarEstado` arma un encabezado y escribe el mensaje.
- `public void MostrarEstado(string titulo, string mensaje)` `MostrarMarcador` imprime nombres, victorias y ronda. `MostrarEsperando` evita repetir el mismo mensaje consecutivo. `MostrarGanadorRonda`, `MostrarGanadorPartida` y `MostrarError` escriben mensajes especializados. `MostrarEstado` arma un encabezado y escribe el mensaje.
- `private void Encabezado(MesaVisible mesa, string? detalle = null)` Las dos sobrecargas de `Encabezado` convierten los datos de mesa en etiquetas y dibujan la barra de contexto. `Pie` imprime la acción o ayuda disponible. `Paneles` coloca los paneles juntos cuando el ancho alcanza y los organiza en filas cuando hace falta.
- `private void Encabezado(string? local, string situacion, string? detalle = null)` Las dos sobrecargas de `Encabezado` convierten los datos de mesa en etiquetas y dibujan la barra de contexto. `Pie` imprime la acción o ayuda disponible. `Paneles` coloca los paneles juntos cuando el ancho alcanza y los organiza en filas cuando hace falta.
- `private void Pie(string mensaje)` Las dos sobrecargas de `Encabezado` convierten los datos de mesa en etiquetas y dibujan la barra de contexto. `Pie` imprime la acción o ayuda disponible. `Paneles` coloca los paneles juntos cuando el ancho alcanza y los organiza en filas cuando hace falta.
- `private void Paneles(params Panel[] paneles)` Las dos sobrecargas de `Encabezado` convierten los datos de mesa en etiquetas y dibujan la barra de contexto. `Pie` imprime la acción o ayuda disponible. `Paneles` coloca los paneles juntos cuando el ancho alcanza y los organiza en filas cuando hace falta.
- `private static Panel PanelInfo(string titulo, string detalle)` `PanelInfo` convierte una cadena a texto visible o envuelve un objeto renderizable en un panel. `Ficha` compone nombre y atributos en una cuadrícula. `FichaJugada` dibuja nombre, puntaje y señal del ganador al presentar un resultado.
- `private static Panel PanelInfo(string titulo, IRenderable detalle)` `PanelInfo` convierte una cadena a texto visible o envuelve un objeto renderizable en un panel. `Ficha` compone nombre y atributos en una cuadrícula. `FichaJugada` dibuja nombre, puntaje y señal del ganador al presentar un resultado.
- `private static Panel Ficha(CartaVisible carta, bool enfocada)` `PanelInfo` convierte una cadena a texto visible o envuelve un objeto renderizable en un panel. `Ficha` compone nombre y atributos en una cuadrícula. `FichaJugada` dibuja nombre, puntaje y señal del ganador al presentar un resultado.
- `private static Panel FichaJugada(CartaJugada jugada, bool ganadora)` `PanelInfo` convierte una cadena a texto visible o envuelve un objeto renderizable en un panel. `Ficha` compone nombre y atributos en una cuadrícula. `FichaJugada` dibuja nombre, puntaje y señal del ganador al presentar un resultado.
- `private static string ResumenMano(IReadOnlyList<CartaVisible> cartas)` `ResumenMano` lista nombres o indica que está vacía. `Rondas` y `Combates` formatean los dos marcadores. `Marcador` combina ambas cadenas.
- `private static string Rondas(MesaVisible mesa)` `ResumenMano` lista nombres o indica que está vacía. `Rondas` y `Combates` formatean los dos marcadores. `Marcador` combina ambas cadenas.
- `private static string Combates(MesaVisible mesa)` `ResumenMano` lista nombres o indica que está vacía. `Rondas` y `Combates` formatean los dos marcadores. `Marcador` combina ambas cadenas.
- `private static string Marcador(MesaVisible mesa)` `ResumenMano` lista nombres o indica que está vacía. `Rondas` y `Combates` formatean los dos marcadores. `Marcador` combina ambas cadenas.
- `private static CartaVisible[] CopiarOpciones(IReadOnlyList<CartaVisible> mano)`. Errores explícitos en el cuerpo: `InvalidOperationException`. `CopiarOpciones` materializa la mano en un arreglo y rechaza una lista vacía. `ListaOpciones` construye filas y `Opcion` marca visualmente la enfocada, incluida una señal textual.
- `private static Rows ListaOpciones(IEnumerable<string> opciones, int foco)` `CopiarOpciones` materializa la mano en un arreglo y rechaza una lista vacía. `ListaOpciones` construye filas y `Opcion` marca visualmente la enfocada, incluida una señal textual.
- `private static Text Opcion(string opcion, bool enfocada)` `CopiarOpciones` materializa la mano en un arreglo y rechaza una lista vacía. `ListaOpciones` construye filas y `Opcion` marca visualmente la enfocada, incluida una señal textual.
- `private int Elegir(int cantidad, Action<int> dibujar)` `Elegir` empieza en índice cero, llama a la función de dibujo y lee teclas. Enter confirma. Las flechas y las teclas equivalentes desplazan el foco. `Math.Clamp` mantiene el índice dentro de la lista.
- `private ConsoleKey LeerTecla()`. Errores explícitos en el cuerpo: `EndOfStreamException`. `LeerTecla` usa `Console.ReadKey` en terminal interactiva. En entrada redirigida traduce líneas como `s` a teclas y una línea vacía a Enter. Si se acaba la entrada, lanza `EndOfStreamException`.
- `private static string TextoVisible(string texto)` `TextoVisible` cambia caracteres de control y formato por espacios. `Escribir` divide el mensaje en líneas y construye filas de texto. `Renderizar` escribe los componentes en Spectre.Console e invalida el recuerdo del último mensaje de espera.
- `private void Escribir(string texto, Style? estilo = null)` `TextoVisible` cambia caracteres de control y formato por espacios. `Escribir` divide el mensaje en líneas y construye filas de texto. `Renderizar` escribe los componentes en Spectre.Console e invalida el recuerdo del último mensaje de espera.
- `private void Renderizar(IRenderable contenido)` `TextoVisible` cambia caracteres de control y formato por espacios. `Escribir` divide el mensaje en líneas y construye filas de texto. `Renderizar` escribe los componentes en Spectre.Console e invalida el recuerdo del último mensaje de espera.

### [src/cliente/Program.cs](../src/cliente/Program.cs)

**`internal static class Program`**

- `private static int Main(string[] args)` `Main` crea la aplicación con `ServidorHttp.Crear`, obtiene el repositorio del contenedor de dependencias, inicia el servidor, arranca la consola del operador en otra tarea y espera el apagado.

### [src/cliente/VistasConsola.cs](../src/cliente/VistasConsola.cs)

**`internal sealed record CartaVisible( int IdPersonaje, string Nombre, IReadOnlyDictionary<string, int> Atributos)`**


**`internal sealed record JugadorVisible(string Nombre, int RondasGanadas)`**


**`internal sealed record CartaJugada(string Jugador, string Carta, int Valor)`**


**`internal sealed record CombateVisible( string Atributo, CartaJugada J1, CartaJugada J2, string Ganador, int CombatesGanadosJ1, int CombatesGanadosJ2, string? ProximoEnElegir, bool? GanaJ1 = null)`**


**`internal sealed record MesaVisible( string Local, JugadorVisible J1, JugadorVisible J2, int Ronda, int Combate, int CombatesJ1, int CombatesJ2)`**


**`internal enum EstadoEnvio`**

### [src/servidor/Api/BaseJuego.cs](../src/servidor/Api/BaseJuego.cs)

**`internal sealed class BaseJuego(DbContextOptions<BaseJuego> opciones)`**

- `protected override void OnModelCreating(ModelBuilder modelo)` `OnModelCreating` especifica nombres de tablas, claves principales simples o compuestas, relaciones y el índice de las solicitudes por partida. El ID del catálogo lo asigna el repositorio.

**`internal sealed class PartidaGuardada`**


**`internal sealed class ParticipanteGuardado`**


### [src/servidor/Api/ConsolaServidor.cs](../src/servidor/Api/ConsolaServidor.cs)

**`internal static class ConsolaServidor`**

- `internal static void Ejecutar(RepositorioJuego juego, TextReader entrada, TextWriter salida, CancellationToken detener)` `Ejecutar` escribe la ayuda y lee entradas hasta cancelación o fin de flujo. Muestra errores de negocio y de persistencia.
- `private static void Comando(RepositorioJuego juego, string linea, TextWriter salida)` `Comando` separa el verbo de sus argumentos, interpreta UUID, plaza o ID entero y llama a la operación correspondiente. `crear` imprime UUID y credenciales. `iniciar`, `cancelar`, `cerrar-sesion`, `reemplazar-credencial` y `desactivar` operan sobre la partida, sesión o catálogo.
- `private static void Ayuda(TextWriter salida)` `Ayuda` escribe la lista de órdenes y su sintaxis.

### [src/servidor/Api/EstadoPartida.cs](../src/servidor/Api/EstadoPartida.cs)

**`internal sealed class PersonajePartida`**

- `internal static PersonajePartida Copiar(Guid partida, PersonajeCatalogo origen)`
- `internal Personaje Dominio()` `PersonajePartida.Copiar` toma los datos del catálogo para una partida. `Dominio` construye el `Personaje` con el que se evalúa un combate. `Vista` produce la carta que se puede enviar, con ID, texto y diccionario de atributos.
- `internal CartaVisible Vista()` `PersonajePartida.Copiar` toma los datos del catálogo para una partida. `Dominio` construye el `Personaje` con el que se evalúa un combate. `Vista` produce la carta que se puede enviar, con ID, texto y diccionario de atributos.

**`internal sealed record CartaVisible(int IdPersonaje, string Nombre, string Descripcion, string Ascii, Dictionary<string, int> Atributos)`**


**`internal sealed class RondaGuardada`**


**`internal sealed class CombateGuardado`**


**`internal sealed class SolicitudGuardada`**


**`internal sealed record Eleccion(Guid IdOperacion, Guid IdSolicitud, int? RevisionEsperada, int? IdPersonaje, string? Atributo)`**


**`internal sealed record Recibo(Guid IdOperacion, Guid IdSolicitud, int RevisionConfirmada)`**


**`internal sealed record Confirmacion(Recibo Recibo, bool Repetida)`**


**`internal sealed class OperacionGuardada`**

- `internal Eleccion Datos()`
- `internal Recibo Recibo()` `OperacionGuardada.Datos` reconstruye la elección normalizada para compararla con un reintento. `Recibo` reconstruye el UUID de solicitud y operación y la revisión que se confirmó.

### [src/servidor/Api/PersonajeCatalogo.cs](../src/servidor/Api/PersonajeCatalogo.cs)

**`internal sealed record AltaPersonaje( string? Nombre, string? Descripcion, string? Ascii, int Fuerza, int Velocidad, int Resistencia, int Inteligencia, int Energia, int Habilidad)`**

- `internal PersonajeCatalogo Validar()`. Errores explícitos en el cuerpo: `ErrorApi.Entrada()`.

**`internal sealed class PersonajeCatalogo`**


### [src/servidor/Api/PresentacionPartida.cs](../src/servidor/Api/PresentacionPartida.cs)

**`internal sealed record SolicitudVisible(Guid IdSolicitud, string Tipo, int[] IdsPersonajePermitidos, string[] AtributosPermitidos)`**


**`internal sealed record ResultadoVisible(int NumeroRonda, int NumeroCombate, Dictionary<string, CartaVisible> Cartas, string Atributo, Dictionary<string, int> Puntajes, string GanadorCombate, string? GanadorRonda)`**


**`internal sealed record PresentacionVisible(Guid IdPartida, int Revision, string Fase, bool Pausada, string RolPropio, Dictionary<string, string?> Nombres, int? NumeroRonda, int? NumeroCombate, Dictionary<string, int> Marcador, CartaVisible[] Mano, SolicitudVisible? Solicitud, ResultadoVisible[] Resultados, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MotivoFin, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Ganador)`**


**`internal sealed partial class RepositorioJuego`**

- `internal PresentacionVisible Presentacion(Guid id, string? credencial, Guid idSesion)` `Presentacion` abre una lectura autenticada, consulta participantes y cartas y filtra la mano por plaza. Incluye la solicitud solo para su titular cuando hay presencia. Construye el historial a partir de combates con ganador, agrega el ganador de ronda al último combate correspondiente y devuelve los datos de fase y marcador sin revelar manos rivales.

### [src/servidor/Api/RepositorioJuego.cs](../src/servidor/Api/RepositorioJuego.cs)

**`internal sealed record PartidaCreada(Guid Id, string CredencialJ1, string CredencialJ2)`**


**`internal sealed record Incorporacion(Guid IdSesion, string? Nombre)`**


**`internal sealed record Sesion(string Rol, Guid IdSesion, DateTimeOffset VenceUtc)`**


**`internal sealed partial class RepositorioJuego`**

- `internal RepositorioJuego(string cadena, TimeProvider reloj)` El constructor de `RepositorioJuego` conserva cadena y reloj, mantiene abierta la conexión si SQLite está en memoria y ejecuta `EnsureCreated`.
- `internal int DarDeAlta(AltaPersonaje datos)`. Errores explícitos en el cuerpo: `new ErrorApi(409, "operacionIncompatible", "Se agotaron los IDs de alta automática.")`. `DarDeAlta` valida la entrada, calcula el siguiente ID positivo dentro de una escritura, agrega la fila y devuelve el ID. `CrearPartida` guarda una partida nueva y dos plazas con hashes de credenciales, y devuelve las credenciales originales al operador.
- `internal PartidaCreada CrearPartida()` `DarDeAlta` valida la entrada, calcula el siguiente ID positivo dentro de una escritura, agrega la fila y devuelve el ID. `CrearPartida` guarda una partida nueva y dos plazas con hashes de credenciales, y devuelve las credenciales originales al operador.
- `internal void Desactivar(int id)`. Errores explícitos en el cuerpo: `ErrorApi.NoEncontrado()`. `Desactivar` pone `Activo` en falso. `CerrarSesion` verifica la plaza y quita su sesión en memoria. `ReemplazarCredencial` guarda el hash de una credencial nueva y revoca la sesión de esa plaza. `Cancelar` exige una partida sin finalizar, aumenta su revisión y delega en `Finalizar`.
- `internal void CerrarSesion(Guid id, string rol)` `Desactivar` pone `Activo` en falso. `CerrarSesion` verifica la plaza y quita su sesión en memoria. `ReemplazarCredencial` guarda el hash de una credencial nueva y revoca la sesión de esa plaza. `Cancelar` exige una partida sin finalizar, aumenta su revisión y delega en `Finalizar`.
- `internal string ReemplazarCredencial(Guid id, string rol)` `Desactivar` pone `Activo` en falso. `CerrarSesion` verifica la plaza y quita su sesión en memoria. `ReemplazarCredencial` guarda el hash de una credencial nueva y revoca la sesión de esa plaza. `Cancelar` exige una partida sin finalizar, aumenta su revisión y delega en `Finalizar`.
- `internal void Cancelar(Guid id)`. Errores explícitos en el cuerpo: `new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.")`. `Desactivar` pone `Activo` en falso. `CerrarSesion` verifica la plaza y quita su sesión en memoria. `ReemplazarCredencial` guarda el hash de una credencial nueva y revoca la sesión de esa plaza. `Cancelar` exige una partida sin finalizar, aumenta su revisión y delega en `Finalizar`.
- `private static ParticipanteGuardado BuscarParticipante(BaseJuego db, Guid id, string rol)`. Errores explícitos en el cuerpo: `ErrorApi.NoEncontrado()`. `Abrir` configura EF Core con la cadena de archivo o la conexión en memoria. `Buscar` y `BuscarParticipante` consultan por clave y responden con error 404 si falta la fila.
- `internal Sesion Incorporar(Guid id, string? credencial, Incorporacion datos)`. Errores explícitos en el cuerpo: `ErrorApi.Entrada()`, `new ErrorApi(409, "sesionOcupada", "La plaza tiene una sesión vigente.")`, `new ErrorApi(403, "sinPermiso", "El nombre registrado debe coincidir.")`. `Incorporar` valida nombre y UUID, identifica la plaza por credencial y rechaza otra sesión vigente o un nombre distinto al guardado. Confirma los datos persistentes y luego instala la sesión con 15 segundos de vigencia.
- `internal void Renovar(Guid id, string? credencial, Guid idSesion)` `Renovar` comprueba credencial y sesión vigentes, y actualiza su vencimiento en memoria. `Vigente` busca por partida y plaza y consulta el reloj. `ExigirSesion` exige además el UUID correcto.
- `private T Ejecutar<T>(bool escritura, Func<BaseJuego, T> operacion)`
- `private BaseJuego Abrir()` `Abrir` configura EF Core con la cadena de archivo o la conexión en memoria. `Buscar` y `BuscarParticipante` consultan por clave y responden con error 404 si falta la fila.
- `private static PartidaGuardada Buscar(BaseJuego db, Guid id)`. Errores explícitos en el cuerpo: `ErrorApi.NoEncontrado()`. `Abrir` configura EF Core con la cadena de archivo o la conexión en memoria. `Buscar` y `BuscarParticipante` consultan por clave y responden con error 404 si falta la fila.
- `private static ParticipanteGuardado Autenticar(BaseJuego db, Guid id, string? credencial)`. Errores explícitos en el cuerpo: `new ErrorApi(401, "credencialInvalida", "La credencial no es válida.")`. `Autenticar` calcula el hash de la credencial entregada y lo compara con los de la partida. `Hash` convierte a UTF-8 y aplica SHA-256. `NuevaCredencial` genera 32 bytes aleatorios y los representa en hexadecimal. `Dispose` libera la conexión en memoria si existe.
- `private Sesion? Vigente(Guid id, string rol)` `Renovar` comprueba credencial y sesión vigentes, y actualiza su vencimiento en memoria. `Vigente` busca por partida y plaza y consulta el reloj. `ExigirSesion` exige además el UUID correcto.
- `private Sesion ExigirSesion(Guid id, string rol, Guid idSesion)`. Errores explícitos en el cuerpo: `new ErrorApi(409, "sesionVencida", "Se requiere una sesión vigente.")`. `Renovar` comprueba credencial y sesión vigentes, y actualiza su vencimiento en memoria. `Vigente` busca por partida y plaza y consulta el reloj. `ExigirSesion` exige además el UUID correcto.
- `private static byte[] Hash(string credencial)` `Autenticar` calcula el hash de la credencial entregada y lo compara con los de la partida. `Hash` convierte a UTF-8 y aplica SHA-256. `NuevaCredencial` genera 32 bytes aleatorios y los representa en hexadecimal. `Dispose` libera la conexión en memoria si existe.
- `private static string NuevaCredencial()` `Autenticar` calcula el hash de la credencial entregada y lo compara con los de la partida. `Hash` convierte a UTF-8 y aplica SHA-256. `NuevaCredencial` genera 32 bytes aleatorios y los representa en hexadecimal. `Dispose` libera la conexión en memoria si existe.
- `public void Dispose()` `Autenticar` calcula el hash de la credencial entregada y lo compara con los de la partida. `Hash` convierte a UTF-8 y aplica SHA-256. `NuevaCredencial` genera 32 bytes aleatorios y los representa en hexadecimal. `Dispose` libera la conexión en memoria si existe.

### [src/servidor/Api/ServidorHttp.cs](../src/servidor/Api/ServidorHttp.cs)

**`internal sealed class ErrorApi(int estado, string codigo, string mensaje)`**

- `internal static ErrorApi Entrada()` El constructor primario de `ErrorApi` conserva el estado y código. `Entrada`, `NoEncontrado` e `Indisponible` construyen los errores 400, 404 y 503. `Responder` fija el estado y escribe código, mensaje e identificador de seguimiento en JSON.
- `internal static ErrorApi NoEncontrado()` El constructor primario de `ErrorApi` conserva el estado y código. `Entrada`, `NoEncontrado` e `Indisponible` construyen los errores 400, 404 y 503. `Responder` fija el estado y escribe código, mensaje e identificador de seguimiento en JSON.
- `internal static ErrorApi Indisponible()` El constructor primario de `ErrorApi` conserva el estado y código. `Entrada`, `NoEncontrado` e `Indisponible` construyen los errores 400, 404 y 503. `Responder` fija el estado y escribe código, mensaje e identificador de seguimiento en JSON.
- `internal Task Responder(HttpContext contexto)` El constructor primario de `ErrorApi` conserva el estado y código. `Entrada`, `NoEncontrado` e `Indisponible` construyen los errores 400, 404 y 503. `Responder` fija el estado y escribe código, mensaje e identificador de seguimiento en JSON.

**`internal static class ServidorHttp`**

- `internal static WebApplication Crear(string[] args, TimeProvider? reloj = null)`. Errores explícitos en el cuerpo: `new ErrorApi(401, "credencialInvalida", "La credencial no es válida.")`. `Crear` construye el host, configura la escucha por defecto, registra repositorio y reloj como instancias compartidas, y exige tipos numéricos JSON estrictos. El middleware envuelve los handlers y traduce excepciones de entrada y SQLite al contrato de error.
- `private static string? Credencial(HttpContext contexto)` `Credencial` lee la cabecera de autorización como esquema Bearer. `IdSesion` interpreta `X-Sesion` como UUID o devuelve `Guid.Empty`.
- `private static Guid IdSesion(HttpContext contexto)` `Credencial` lee la cabecera de autorización como esquema Bearer. `IdSesion` interpreta `X-Sesion` como UUID o devuelve `Guid.Empty`.

### [src/servidor/Api/TransicionesPartida.cs](../src/servidor/Api/TransicionesPartida.cs)

**`internal sealed partial class RepositorioJuego`**

- `internal void Iniciar(Guid id)`. Errores explícitos en el cuerpo: `new ErrorApi(409, "operacionIncompatible", "La partida ya se inició o finalizó.")`, `new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.")`, `new ErrorApi(409, "operacionIncompatible", "Se necesitan seis personajes activos.")`. `Iniciar` exige fase inicial, dos sesiones y seis personajes activos. Copia el catálogo a la partida, llama a `Repartir` y aumenta la revisión.
- `internal Confirmacion Elegir(Guid id, string? credencial, Guid idSesion, Eleccion datos)`. Errores explícitos en el cuerpo: `ErrorApi.Entrada()`, `new ErrorApi(409, "operacionIncompatible", "La operación ya tiene otros datos.")`, `new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.")`, `new ErrorApi(409, "revisionDesactualizada", "La revisión ya cambió.")`, `new ErrorApi(409, "solicitudNoVigente", "La solicitud no está vigente para esta plaza.")`, `new ErrorApi(422, "eleccionInvalida", "La carta o el atributo no está entre las opciones.")`. `Elegir` valida IDs, revisión y carta presentes, normaliza el atributo y abre una transacción de escritura. Primero autentica y busca el ID de operación por partida y plaza. Si ya existe, compara los datos y devuelve el recibo original o rechaza la reutilización incompatible.
- `internal Recibo ConsultarRecibo(Guid id, string? credencial, Guid idSesion, Guid idOperacion)`. Errores explícitos en el cuerpo: `ErrorApi.NoEncontrado()`. `ConsultarRecibo` autentica y recupera únicamente recibos de la propia plaza. `Abandonar` admite repetir el abandono del mismo jugador y, en una partida vigente, aumenta la revisión y finaliza sin adjudicar victoria.
- `internal void Abandonar(Guid id, string? credencial, Guid idSesion)`. Errores explícitos en el cuerpo: `new ErrorApi(409, "operacionIncompatible", "La partida ya finalizó.")`. `ConsultarRecibo` autentica y recupera únicamente recibos de la propia plaza. `Abandonar` admite repetir el abandono del mismo jugador y, en una partida vigente, aumenta la revisión y finaliza sin adjudicar victoria.
- `private (PartidaGuardada Partida, ParticipanteGuardado Jugador) Acceso( BaseJuego db, Guid id, string? credencial, Guid idSesion)`
- `private bool AmbosPresentes(Guid id)` `Acceso` combina búsqueda de partida, autenticación y validación de sesión. `AmbosPresentes` comprueba dos sesiones. `Pausada` requiere una fase de elección y al menos un ausente. `Otro` intercambia J1 y J2.
- `private bool Pausada(PartidaGuardada partida)` `Acceso` combina búsqueda de partida, autenticación y validación de sesión. `AmbosPresentes` comprueba dos sesiones. `Pausada` requiere una fase de elección y al menos un ausente. `Otro` intercambia J1 y J2.
- `private static string Otro(string rol)` `Acceso` combina búsqueda de partida, autenticación y validación de sesión. `AmbosPresentes` comprueba dos sesiones. `Pausada` requiere una fase de elección y al menos un ausente. `Otro` intercambia J1 y J2.
- `private static string? NormalizarAtributo(string? atributo)` `NormalizarAtributo` conserva el nulo, recorta espacios, normaliza Unicode y unifica mayúsculas y el caso especial de Energía.
- `private static void Repartir(BaseJuego db, PartidaGuardada partida, PersonajePartida[] cartas, int numero, string mano)` `Repartir` mezcla las fichas, asigna orden y ubicación, añade la ronda y llama a `AbrirCombate`. `AbrirCombate` agrega el combate con su elector y abre una solicitud de carta y atributo. `AbrirSolicitud` crea el UUID, registra la plaza y ajusta la fase según el tipo de acción.
- `private static void AbrirCombate(BaseJuego db, PartidaGuardada partida, int numero, string elector)` `Repartir` mezcla las fichas, asigna orden y ubicación, añade la ronda y llama a `AbrirCombate`. `AbrirCombate` agrega el combate con su elector y abre una solicitud de carta y atributo. `AbrirSolicitud` crea el UUID, registra la plaza y ajusta la fase según el tipo de acción.
- `private static void AbrirSolicitud(BaseJuego db, PartidaGuardada partida, CombateGuardado combate, string rol, string tipo)` `Repartir` mezcla las fichas, asigna orden y ubicación, añade la ronda y llama a `AbrirCombate`. `AbrirCombate` agrega el combate con su elector y abre una solicitud de carta y atributo. `AbrirSolicitud` crea el UUID, registra la plaza y ajusta la fase según el tipo de acción.
- `private static void Resolver(BaseJuego db, PartidaGuardada partida, CombateGuardado guardado, PersonajePartida cartaRival)` `Resolver` materializa ambos personajes, arma un `Combate` de dominio, compara y traslada resultado, puntajes y ubicación a las filas. Si la ronda sigue, alterna elector y abre otro combate. Si se cierra, suma la ronda y alterna la mano o finaliza al llegar a 15.
- `private static void Finalizar(BaseJuego db, PartidaGuardada partida, string motivo, string? ganador = null)` `Finalizar` consume la solicitud activa, limpia su referencia y conserva fase final, motivo y ganador cuando corresponde.

### [src/servidor/Combate.cs](../src/servidor/Combate.cs)

**`internal class Combate`**

- `public Combate(Jugador quienElige, Jugador rival)`. Errores explícitos en el cuerpo: `ArgumentException`. `Combate(...)` comprueba que ambos jugadores existan y que sean instancias distintas. Conserva el elector y el rival en campos separados.
- `public void SetAtributo(TipoAtributo tipo)`. Errores explícitos en el cuerpo: `InvalidOperationException`, `ArgumentOutOfRangeException`. `SetAtributo` exige un valor definido en `TipoAtributo` y permite registrarlo una vez.
- `public void SetCarta(Jugador jugador, Personaje personaje)` `SetCarta` valida jugador y carta, comprueba la pertenencia y disponibilidad con los métodos privados, retira la carta de la mano y la registra.
- `public bool EstaCompleto()` `EstaCompleto` comprueba si hay atributo y una carta por participante.
- `public Jugador Resolver()`. Errores explícitos en el cuerpo: `InvalidOperationException`. `Resolver` exige un combate completo y sin resultado previo. Obtiene el mismo atributo de las dos cartas. Si el puntaje del elector es estrictamente mayor, gana él. En los demás casos gana el rival.
- `public Jugador GetGanador()`. Errores explícitos en el cuerpo: `InvalidOperationException`. `GetGanador` y `GetAtributo` leen sus campos y arrojan una excepción si todavía faltan. `GetCarta` valida al participante y exige que su carta ya esté registrada.
- `public TipoAtributo GetAtributo()`. Errores explícitos en el cuerpo: `InvalidOperationException`. `GetGanador` y `GetAtributo` leen sus campos y arrojan una excepción si todavía faltan. `GetCarta` valida al participante y exige que su carta ya esté registrada.
- `public Personaje GetCarta(Jugador jugador)`. Errores explícitos en el cuerpo: `InvalidOperationException`, `ArgumentException`. `GetGanador` y `GetAtributo` leen sus campos y arrojan una excepción si todavía faltan. `GetCarta` valida al participante y exige que su carta ya esté registrada.
- `public Jugador[] GetParticipantes()` `GetParticipantes` crea un arreglo con elector primero y rival segundo.
- `private void ValidarParticipante(Jugador jugador)`. Errores explícitos en el cuerpo: `ArgumentException`. `ValidarParticipante` compara identidad de objeto con ambas plazas. `ValidarCartaDisponible` comprueba que el jugador no haya registrado otra carta y que la elegida esté en su mano. `RegistrarCarta` guarda en el campo del elector o del rival.
- `private void ValidarCartaDisponible(Jugador jugador, Personaje personaje)`. Errores explícitos en el cuerpo: `InvalidOperationException`, `ArgumentException`. `ValidarParticipante` compara identidad de objeto con ambas plazas. `ValidarCartaDisponible` comprueba que el jugador no haya registrado otra carta y que la elegida esté en su mano. `RegistrarCarta` guarda en el campo del elector o del rival.
- `private void RegistrarCarta(Jugador jugador, Personaje personaje)` `ValidarParticipante` compara identidad de objeto con ambas plazas. `ValidarCartaDisponible` comprueba que el jugador no haya registrado otra carta y que la elegida esté en su mano. `RegistrarCarta` guarda en el campo del elector o del rival.

### [src/servidor/Jugador.cs](../src/servidor/Jugador.cs)

**`internal class Jugador`**

- `public Jugador(string nombre)`. Errores explícitos en el cuerpo: `ArgumentException`. `Jugador(...)` valida el nombre y comienza con mano vacía y cero rondas ganadas.
- `public void RecibirCartas(Personaje[] personajes)` `RecibirCartas` comprueba el arreglo y agrega sus elementos a la mano. `QuitarCarta` quita una instancia de esa lista.
- `public void QuitarCarta(Personaje personaje)` `RecibirCartas` comprueba el arreglo y agrega sus elementos a la mano. `QuitarCarta` quita una instancia de esa lista.
- `public Personaje[] EntregarCartas()` `EntregarCartas` copia la mano, vacía la lista y devuelve el arreglo. `GetCartas` entrega una copia sin vaciarla.
- `public Personaje[] GetCartas()` `EntregarCartas` copia la mano, vacía la lista y devuelve el arreglo. `GetCartas` entrega una copia sin vaciarla.
- `public void SumarRondaGanada()` `SumarRondaGanada` incrementa el contador. `GetRondasGanadas` y `GetNombre` leen esos campos.
- `public int GetRondasGanadas()` `SumarRondaGanada` incrementa el contador. `GetRondasGanadas` y `GetNombre` leen esos campos.
- `public string GetNombre()` `SumarRondaGanada` incrementa el contador. `GetRondasGanadas` y `GetNombre` leen esos campos.
- `public bool TieneCarta(Personaje personaje)` `TieneCarta` consulta si la mano contiene la instancia de `Personaje`.

### [src/servidor/Mazo.cs](../src/servidor/Mazo.cs)

**`internal class Mazo`**

- `public Mazo(List<Personaje> personajes)` `Mazo(...)` copia la lista recibida para que las operaciones sobre el mazo tengan su propia colección.
- `public void Mezclar()` `Mezclar` recorre desde el final e intercambia cada posición con una elegida al azar entre el inicio y esa posición. Es una mezcla de Fisher–Yates.
- `public Personaje[] Repartir(int cantidad)` `Repartir` toma las primeras `cantidad` cartas y elimina ese tramo. Si se piden más de las disponibles, `RemoveRange` rechaza el rango.
- `public void Devolver(Personaje[] personajes)` `Devolver` agrega las cartas al final de la lista. `GetCantidad` devuelve su tamaño.
- `public int GetCantidad()` `Devolver` agrega las cartas al final de la lista. `GetCantidad` devuelve su tamaño.

### [src/servidor/Partida.cs](../src/servidor/Partida.cs)

**`internal class Partida`**

- `public Partida(Jugador j1, Jugador j2, Mazo mazo)` `Partida(...)` guarda las referencias validadas de ambos jugadores y el mazo. Define `RondasParaGanar` en 15.
- `public Jugador Jugar()` `Jugar` llama a `Iniciar`, luego repite ronda y recuperación de cartas, comprueba la victoria y prepara la siguiente ronda con el jugador mano alternado.
- `private void Iniciar()`. Errores explícitos en el cuerpo: `InvalidOperationException`. `Iniciar` exige seis cartas, crea la ronda 1 con J1 como mano y llama a `RepartirCartas`. Este último mezcla y entrega tres cartas a cada jugador.
- `private void RepartirCartas()` `Iniciar` exige seis cartas, crea la ronda 1 con J1 como mano y llama a `RepartirCartas`. Este último mezcla y entrega tres cartas a cada jugador.
- `private void JugarRonda()`. Errores explícitos en el cuerpo: `InvalidOperationException`. `JugarRonda` construye el próximo combate. La entrada de elecciones sigue pendiente en este bucle en memoria, por lo que el combate queda incompleto y se lanza una excepción. El servidor HTTP registra las elecciones en `TransicionesPartida.cs`.
- `private void RecogerCartas()` `RecogerCartas` reúne lo usado en los combates y lo que sigue en ambas manos, y devuelve todo al mazo.
- `private bool EstaTerminada()` `EstaTerminada` compara ambos marcadores con 15. `DeterminarGanadorPartida` devuelve el jugador que alcanzó el umbral.
- `private Jugador DeterminarGanadorPartida()` `EstaTerminada` compara ambos marcadores con 15. `DeterminarGanadorPartida` devuelve el jugador que alcanzó el umbral.

### [src/servidor/Personaje.cs](../src/servidor/Personaje.cs)

**`internal enum TipoAtributo`**

**`internal class Personaje`**

- `public Personaje( string nombre, string ascii, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)`. Errores explícitos en el cuerpo: `ArgumentException`. `Personaje(...)` guarda los datos y llama a `Validar` antes de entregar el objeto.
- `private void Validar()`. Errores explícitos en el cuerpo: `ArgumentException`. `Validar` rechaza un nombre sin texto o un dibujo nulo. En los seis puntajes comprueba el intervalo 250–750, la suma 3000 y la existencia de al menos un valor ≥650 y otro ≤350.
- `public string GetNombre()` `GetNombre` y `GetAscii` devuelven los campos respectivos. El dibujo puede ser una cadena vacía.
- `public string GetAscii()` `GetNombre` y `GetAscii` devuelven los campos respectivos. El dibujo puede ser una cadena vacía.
- `public int GetAtributo(TipoAtributo tipo)`. Errores explícitos en el cuerpo: `ArgumentOutOfRangeException`. `GetAtributo` traduce el valor de la enumeración al campo correspondiente mediante un `switch` y rechaza un valor fuera de la enumeración.

### [src/servidor/Program.cs](../src/servidor/Program.cs)

**`internal static class Program`**

- `private static async Task Main(string[] args)` `Main` crea la aplicación con `ServidorHttp.Crear`, obtiene el repositorio del contenedor de dependencias, inicia el servidor, arranca la consola del operador en otra tarea y espera el apagado.

### [src/servidor/Ronda.cs](../src/servidor/Ronda.cs)

**`internal class Ronda`**

- `public Ronda(Jugador mano, Jugador rival)`. Errores explícitos en el cuerpo: `ArgumentException`. `Ronda(...)` valida la pareja y conserva qué jugador abre como mano.
- `public void AgregarCombate(Combate combate)`. Errores explícitos en el cuerpo: `InvalidOperationException`. `AgregarCombate` rechaza valores nulos, una ronda ya definida o más de tres combates. Agrega el objeto al historial. Las consultas de marcador dependen de que cada combate agregado ya tenga ganador.
- `public bool EstaDefinida()` `EstaDefinida` cuenta los combates ganados por cada participante y compara los resultados con `CombatesParaGanar`, que vale dos.
- `public Jugador GetGanador()`. Errores explícitos en el cuerpo: `InvalidOperationException`. `GetGanador` devuelve el jugador que alcanzó el umbral o arroja una excepción si falta cerrar la ronda.
- `public Jugador GetQuienEligeProximo()`. Errores explícitos en el cuerpo: `InvalidOperationException`. `GetQuienEligeProximo` alterna mano y rival según la paridad de los combates guardados. `GetCombates` devuelve una copia del historial.
- `public Combate[] GetCombates()` `GetQuienEligeProximo` alterna mano y rival según la paridad de los combates guardados. `GetCombates` devuelve una copia del historial.

### [src/subir-personajes/Program.cs](../src/subir-personajes/Program.cs)

**`internal static class Program`**

- `private static async Task<int> Main(string[] args)` `Main` crea la aplicación con `ServidorHttp.Crear`, obtiene el repositorio del contenedor de dependencias, inicia el servidor, arranca la consola del operador en otra tarea y espera el apagado.

**`internal static class ConsolaAlta`**

- `internal static async Task<int> Ejecutar(HttpClient cliente, string credencial, TextReader entrada, TextWriter salida)`. Errores explícitos en el cuerpo: `EndOfStreamException`. `Ejecutar` pregunta si se desea otra alta y vuelve al principio solo si se confirma. Trata fin de entrada, errores de red, cancelación y JSON inválido. `EsSi` acepta `sí` o `si` sin distinguir mayúsculas.
- `private static bool EsSi(string respuesta)` `Ejecutar` pregunta si se desea otra alta y vuelve al principio solo si se confirma. Trata fin de entrada, errores de red, cancelación y JSON inválido. `EsSi` acepta `sí` o `si` sin distinguir mayúsculas.

**`private sealed record AltaConfirmada(int IdPersonaje)`**


### [test/proyecto-integrador-final-tests/ApiHttpTests.cs](../test/proyecto-integrador-final-tests/ApiHttpTests.cs)

**`public class ApiHttpTests`**

- `public async Task PresentacionDePartidaDesconocidaDevuelveErrorDelProtocolo()` `PresentacionDePartidaDesconocidaDevuelveErrorDelProtocolo` consulta un UUID inexistente y comprueba estado, código e ID de seguimiento.
- `public async Task IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion()` `IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion` compara la incorporación original, su repetición y la colisión con otro UUID.
- `public async Task AltaExigeCredencialAdministrativaYValidaAtributos()` `AltaExigeCredencialAdministrativaYValidaAtributos` contrasta alta sin credencial, alta inválida y alta válida con ID.
- `public async Task CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez()` `CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez` envía elecciones concurrentes, consulta el recibo propio y ajeno y luego verifica cuándo aparece el combate completo.
- `public async Task ReinicioRecuperaSolicitudYRecibosYExigeSesionesNuevas()` `ReinicioRecuperaSolicitudYRecibosYExigeSesionesNuevas` vuelve a abrir la misma base después de cerrar el host y comprueba que los datos persisten mientras las sesiones se pierden.
- `public async Task UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales()` `UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales` induce un fallo de almacenamiento, comprueba que no haya recibo ni avance y vuelve a enviar.
- `public async Task PresenciaRenuevaElPlazoSinCambiarLaRevision()` `PresenciaRenuevaElPlazoSinCambiarLaRevision` controla el reloj para observar vigencia y revisión por separado.
- `public async Task ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza()` `ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza` rota la credencial, vuelve a incorporar y consulta el estado cancelado.
- `public async Task PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra()` `PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra` recorre los turnos por HTTP hasta el marcador final. `AltaRechazaPuntajesRepresentadosComoTexto` manda tipos JSON incorrectos y comprueba el error.
- `public async Task AltaRechazaPuntajesRepresentadosComoTexto()` `PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra` recorre los turnos por HTTP hasta el marcador final. `AltaRechazaPuntajesRepresentadosComoTexto` manda tipos JSON incorrectos y comprueba el error.
- `private static async Task Error(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)` `Error` comprueba cuerpo y estado HTTP. `GetUtcNow` devuelve el reloj controlado y `Avanzar` le suma segundos. `DatosEleccion` arma el cuerpo a partir de revisión y solicitud. `Incorporar` prepara un cliente autorizado y `Vista` lee su presentación.
- `private static object DatosEleccion(JsonElement vista, string? atributo)` `Error` comprueba cuerpo y estado HTTP. `GetUtcNow` devuelve el reloj controlado y `Avanzar` le suma segundos. `DatosEleccion` arma el cuerpo a partir de revisión y solicitud. `Incorporar` prepara un cliente autorizado y `Vista` lee su presentación.
- `private static async Task<HttpClient> Incorporar(string url, Guid partida, string credencial, string nombre)` `Error` comprueba cuerpo y estado HTTP. `GetUtcNow` devuelve el reloj controlado y `Avanzar` le suma segundos. `DatosEleccion` arma el cuerpo a partir de revisión y solicitud. `Incorporar` prepara un cliente autorizado y `Vista` lee su presentación.
- `private static async Task<JsonElement> Vista(HttpClient cliente)` `Error` comprueba cuerpo y estado HTTP. `GetUtcNow` devuelve el reloj controlado y `Avanzar` le suma segundos. `DatosEleccion` arma el cuerpo a partir de revisión y solicitud. `Incorporar` prepara un cliente autorizado y `Vista` lee su presentación.

**`private sealed class RelojPruebas`**

- `public override DateTimeOffset GetUtcNow()` `Error` comprueba cuerpo y estado HTTP. `GetUtcNow` devuelve el reloj controlado y `Avanzar` le suma segundos. `DatosEleccion` arma el cuerpo a partir de revisión y solicitud. `Incorporar` prepara un cliente autorizado y `Vista` lee su presentación.
- `internal void Avanzar(int segundos)` `Error` comprueba cuerpo y estado HTTP. `GetUtcNow` devuelve el reloj controlado y `Avanzar` le suma segundos. `DatosEleccion` arma el cuerpo a partir de revisión y solicitud. `Incorporar` prepara un cliente autorizado y `Vista` lee su presentación.

### [test/proyecto-integrador-final-tests/CartasDePrueba.cs](../test/proyecto-integrador-final-tests/CartasDePrueba.cs)

**`internal static class CartasDePrueba`**

- `internal static Personaje Crear(int fuerza = 650)` `Crear` fabrica un personaje válido con fuerza configurable y ajusta otro puntaje para conservar el total.

### [test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs](../test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs)

**`public sealed class ClienteConsolaContratoTests`**

- `public async Task Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince()` `Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince` verifica que el ejecutable emita pantallas y marcadores en orden.
- `public async Task Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta()` `Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta` cambia el foco con texto y observa qué se confirmó.
- `public async Task Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir()` `Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir` arranca en modo pausa y comprueba que el marcador no cambie.
- `public async Task Demo_DistingueAbandonoCancelacionYSalidaLocal()` `Demo_DistingueAbandonoCancelacionYSalidaLocal` ejecuta los tres modos y compara sus mensajes.
- `public async Task Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada()` `Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada` cierra la entrada y busca el cierre local.
- `private static void EnOrden(string salida, params string[] fragmentos)` `EnOrden` busca fragmentos sucesivos desde la última coincidencia. `EjecutarDemo` lanza la DLL con entrada y salida redirigidas, escribe el guion de teclas y devuelve el texto del proceso.
- `private static async Task<string> EjecutarDemo(string? modo, string entrada)` `EnOrden` busca fragmentos sucesivos desde la última coincidencia. `EjecutarDemo` lanza la DLL con entrada y salida redirigidas, escribe el guion de teclas y devuelve el texto del proceso.

### [test/proyecto-integrador-final-tests/CombateTests.cs](../test/proyecto-integrador-final-tests/CombateTests.cs)

**`public class CombateTests`**

- `public void ValidaParticipantesYDevuelveUnaCopiaEnOrden()` `ValidaParticipantesYDevuelveUnaCopiaEnOrden` prueba constructores inválidos y modifica un arreglo devuelto para comprobar que los participantes se conservan.
- `public void RechazaConsultasPrematurasYAtributosInvalidosORepetidos()` `RechazaConsultasPrematurasYAtributosInvalidosORepetidos` consulta antes de elegir y comprueba las excepciones y la inmutabilidad de la primera elección.
- `public void RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador()` `RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador` mezcla cartas ajenas, jugadores inválidos y elecciones repetidas. Comprueba qué cartas se retiraron y cuáles quedan.
- `public void FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo(bool juegaElector)` `FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo` alterna el participante que entrega la única carta y exige que resolver falle.
- `public void ResuelveSegunElAtributoElegidoYDaElEmpateAlRival(int fuerzaElector, int fuerzaRival, bool ganaElector)` `ResuelveSegunElAtributoElegidoYDaElEmpateAlRival` parametriza mayor, menor e igualdad y compara la instancia ganadora.

### [test/proyecto-integrador-final-tests/ConsolaAltaTests.cs](../test/proyecto-integrador-final-tests/ConsolaAltaTests.cs)

**`public class ConsolaAltaTests`**

- `public async Task CargaDosPersonajesManualmenteYConfirmaSusIds()` `CargaDosPersonajesManualmenteYConfirmaSusIds` alimenta dos formularios por texto, envía las dos altas a un servidor local con SQLite en memoria y comprueba los IDs impresos.
- `public async Task UnAltaRechazadaNoSeAnunciaComoConfirmada()` `UnAltaRechazadaNoSeAnunciaComoConfirmada` provoca una respuesta no autorizada y comprueba el mensaje y código de salida.

### [test/proyecto-integrador-final-tests/ConsolaServidorTests.cs](../test/proyecto-integrador-final-tests/ConsolaServidorTests.cs)

**`public sealed class ConsolaServidorTests`**

- `public void Ayuda_ExplicaArgumentosYComoEmpezar()`
- `public void ComandoInvalido_MuestraComoCorregirlo(string comando, string mensaje)`
- `private static string Ejecutar(string comandos)`

### [test/proyecto-integrador-final-tests/ContratoFactAttribute.cs](../test/proyecto-integrador-final-tests/ContratoFactAttribute.cs)

**`public sealed class ContratoFactAttribute`**

- `public ContratoFactAttribute(string variableDll)` El constructor de `ContratoFactAttribute` lee la variable de ruta de DLL y establece `Skip` si falta. Así los casos por proceso se descubren aunque sus binarios no estén configurados.

### [test/proyecto-integrador-final-tests/InterfazConsolaTests.cs](../test/proyecto-integrador-final-tests/InterfazConsolaTests.cs)

**`public class InterfazConsolaTests`**

- `private static CartaVisible Carta(int id, string nombre)` `Carta` crea una vista de carta para alimentar la interfaz.
- `public void PedirCarta_ReintentaEntradasInvalidasYConservaLaMano()` `PedirCarta_ReintentaEntradasInvalidasYConservaLaMano` alimenta líneas inválidas y válidas y comprueba la selección sin mutar la mano.
- `public void ManoVacia_SePuedeMostrarPeroNoSolicitar()` `ManoVacia_SePuedeMostrarPeroNoSolicitar` comprueba el mensaje de mano vacía y la excepción de selección.
- `public void PedirAtributo_OfreceLosSeisAtributos(int pasos, string esperado)` `PedirAtributo_OfreceLosSeisAtributos` parametriza el número de desplazamientos. `PedirAtributo_RespetaLasOpcionesDeLaSolicitud` aporta una lista restringida y verifica la selección.
- `public void PedirAtributo_RespetaLasOpcionesDeLaSolicitud()` `PedirAtributo_OfreceLosSeisAtributos` parametriza el número de desplazamientos. `PedirAtributo_RespetaLasOpcionesDeLaSolicitud` aporta una lista restringida y verifica la selección.
- `public void FinDeEntrada_TerminaLaSolicitud()` `FinDeEntrada_TerminaLaSolicitud` provoca fin de lectura y espera la excepción de entrada.
- `public void MostrarMano_AdaptaLasFichasAlAncho(int ancho, bool enParalelo)` `MostrarMano_AdaptaLasFichasAlAncho` y `MostrarCombate_PresentaElResultadoConfirmado` usan anchos determinados y verifican las fichas y el contenido del resultado. `PresentaMarcadorEstadosYCierresConLosNombresRecibidos` comprueba los nombres y mensajes en la salida capturada.
- `public void MostrarCombate_PresentaElResultadoConfirmado(int ancho)` `MostrarMano_AdaptaLasFichasAlAncho` y `MostrarCombate_PresentaElResultadoConfirmado` usan anchos determinados y verifican las fichas y el contenido del resultado. `PresentaMarcadorEstadosYCierresConLosNombresRecibidos` comprueba los nombres y mensajes en la salida capturada.
- `public void PresentaMarcadorEstadosYCierresConLosNombresRecibidos()` `MostrarMano_AdaptaLasFichasAlAncho` y `MostrarCombate_PresentaElResultadoConfirmado` usan anchos determinados y verifican las fichas y el contenido del resultado. `PresentaMarcadorEstadosYCierresConLosNombresRecibidos` comprueba los nombres y mensajes en la salida capturada.

### [test/proyecto-integrador-final-tests/JugadorTests.cs](../test/proyecto-integrador-final-tests/JugadorTests.cs)

**`public class JugadorTests`**

- `public void IniciaConNombreManoVaciaYSinVictorias()` `IniciaConNombreManoVaciaYSinVictorias` inspecciona los campos observables del jugador recién construido.
- `public void AcumulaCartasQuitaUnaYEntregaElResto()` `AcumulaCartasQuitaUnaYEntregaElResto` combina varias recepciones, prueba la copia defensiva, retira una carta y comprueba el vaciado y el marcador.

### [test/proyecto-integrador-final-tests/MazoTests.cs](../test/proyecto-integrador-final-tests/MazoTests.cs)

**`public class MazoTests`**

- `public void CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal()` `CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal` modifica la lista original, reparte por tramos y verifica orden y devolución.
- `public void MezclarConservaTodasLasCartas(int cantidad)` `MezclarConservaTodasLasCartas` mezcla mazos de distintos tamaños y verifica que cada carta permanezca sin imponer un orden aleatorio concreto.

### [test/proyecto-integrador-final-tests/PantallasConsolaTests.cs](../test/proyecto-integrador-final-tests/PantallasConsolaTests.cs)

**`public class PantallasConsolaTests`**

- `public void PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles()` `PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles` llama a tres pantallas y busca información específica de cada una.
- `public void Apertura_UsaMesaConFichasEncabezadoYPie(int ancho, bool paralelas)` `Apertura_UsaMesaConFichasEncabezadoYPie` ejecuta la apertura a diferentes anchos.
- `public void EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo()` `EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo` usa la misma mesa para revisar espera y respuesta.
- `public void EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados()` `EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados` comprueba los mensajes de envío, suspensión e interrupción.
- `public void ResultadoYRondas_UsanDueloVictoriaYPlanilla()` `ResultadoYRondas_UsanDueloVictoriaYPlanilla` comprueba los paneles y textos del combate, ronda y partida.

### [test/proyecto-integrador-final-tests/PartidaTests.cs](../test/proyecto-integrador-final-tests/PartidaTests.cs)

**`public class PartidaTests`**

- `public void RechazaUnMazoInsuficienteAntesDeRepartir(int cantidad)` `RechazaUnMazoInsuficienteAntesDeRepartir` llama a `Jugar` con tamaños menores a seis, espera la excepción de inicio y comprueba que el mazo y las manos conservan su estado.

### [test/proyecto-integrador-final-tests/PersonajeTests.cs](../test/proyecto-integrador-final-tests/PersonajeTests.cs)

**`public class PersonajeTests`**

- `public void ConservaNombreAsciiYLosSeisAtributos()` `ConservaNombreAsciiYLosSeisAtributos` construye un personaje y compara cada getter con su entrada.
- `public void AceptaUmbralesInclusivosYAsciiVacio()` `AceptaUmbralesInclusivosYAsciiVacio` usa valores límite y una cadena de dibujo vacía.
- `public void RechazaAtributosQueIncumplenUnaRegla(int fuerza, int velocidad, int resistencia, int inteligencia)` `RechazaAtributosQueIncumplenUnaRegla` inyecta varios conjuntos con `InlineData` y espera una excepción de validación.
- `public void RechazaNombreSinTexto(string? nombre)` `RechazaNombreSinTexto` parametriza nombres nulos o en blanco. `RechazaAsciiNulo` prueba por separado el dibujo nulo.
- `public void RechazaAsciiNulo()` `RechazaNombreSinTexto` parametriza nombres nulos o en blanco. `RechazaAsciiNulo` prueba por separado el dibujo nulo.

### [test/proyecto-integrador-final-tests/RondaTests.cs](../test/proyecto-integrador-final-tests/RondaTests.cs)

**`public class RondaTests`**

- `public void EmpiezaSinCombatesYEligeElJugadorMano()` `EmpiezaSinCombatesYEligeElJugadorMano` confirma el estado inicial y la excepción al consultar ganador.
- `public void DosVictoriasSeguidasCierranLaRonda(bool ganaMano)` `DosVictoriasSeguidasCierranLaRonda` parametriza al vencedor y prueba cierre temprano.
- `public void ElTercerCombateDefineLaRondaCuandoVanUnoAUno(bool ganaMano)` `ElTercerCombateDefineLaRondaCuandoVanUnoAUno` genera un empate parcial y completa el tercer combate con cada posible ganador.
- `private static Combate ResolverCombate(Jugador elector, Jugador oponente, Jugador ganador)` `ResolverCombate` prepara cartas y atributo para producir el ganador que necesita el escenario, luego devuelve el combate ya resuelto.

### [test/proyecto-integrador-final-tests/ServidorContratoTests.cs](../test/proyecto-integrador-final-tests/ServidorContratoTests.cs)

**`public sealed class ServidorContratoTests`**

- `public async Task Incorporacion_ValidaCredencialYSesionYNoCambiaRevisionConPresencia()` `Incorporacion_ValidaCredencialYSesionYNoCambiaRevisionConPresencia` compara incorporación, renovación y estados de error vía HTTP.
- `public async Task Elecciones_ProtegenManosYDevuelvenElMismoReciboAlReintentar()` `Elecciones_ProtegenManosYDevuelvenElMismoReciboAlReintentar` toma una vista por plaza y repite la elección para comprobar privacidad y el recibo original.
- `public async Task EleccionesConcurrentes_ConsumenUnaSolaSolicitud()` `EleccionesConcurrentes_ConsumenUnaSolaSolicitud` dispara dos operaciones sobre la misma solicitud y verifica que avance una vez.
- `public async Task SesionAusente_PausaYRecuperaLaMismaSolicitudSinCambiarRevision()` `SesionAusente_PausaYRecuperaLaMismaSolicitudSinCambiarRevision` cierra una sesión y luego la reincorpora para observar la misma solicitud.
- `public async Task Partida_AlternaElElectorEnEmpatesYFinalizaAlLlegarAQuinceRondas()` `Partida_AlternaElElectorEnEmpatesYFinalizaAlLlegarAQuinceRondas` conduce los turnos hasta el marcador final y compara la alternancia y los empates.
- `public async Task Reinicio_InvalidaSesionesYConservaEstadoYReciboConfirmados()` `Reinicio_InvalidaSesionesYConservaEstadoYReciboConfirmados` reinicia el proceso sobre el archivo SQLite anterior y verifica estado y autenticación.
- `public async Task Abandono_RepetirloConservaMarcadorYRevisionFinal()` `Abandono_RepetirloConservaMarcadorYRevisionFinal` envía dos veces el abandono y compara ambas presentaciones finales.
- `private static int Revision(JsonElement vista)` `Revision` y `Mano` extraen datos de una vista JSON. `Elegir` construye una elección desde las opciones visibles. `Leer` verifica estado y deserializa. `Error` comprueba los campos de un error.
- `private static int[] Mano(JsonElement vista)` `Revision` y `Mano` extraen datos de una vista JSON. `Elegir` construye una elección desde las opciones visibles. `Leer` verifica estado y deserializa. `Error` comprueba los campos de un error.
- `private static Eleccion Elegir(JsonElement vista)` `Revision` y `Mano` extraen datos de una vista JSON. `Elegir` construye una elección desde las opciones visibles. `Leer` verifica estado y deserializa. `Error` comprueba los campos de un error.
- `private static async Task<JsonElement> Leer(HttpResponseMessage respuesta, HttpStatusCode esperado)` `Revision` y `Mano` extraen datos de una vista JSON. `Elegir` construye una elección desde las opciones visibles. `Leer` verifica estado y deserializa. `Error` comprueba los campos de un error.
- `private static async Task Error(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)` `Revision` y `Mano` extraen datos de una vista JSON. `Elegir` construye una elección desde las opciones visibles. `Leer` verifica estado y deserializa. `Error` comprueba los campos de un error.

**`private sealed record Eleccion(Guid idOperacion, Guid idSolicitud, int revisionEsperada, int idPersonaje, string? atributo)`**


**`private sealed record Jugador(string Rol, string Credencial, Guid Sesion)`**


**`private sealed class Servidor`**

- `public Servidor()` Los records privados `Eleccion` y `Jugador` conservan los parámetros que usa el banco de prueba HTTP. El constructor de `Servidor` configura la instancia de prueba. `Lanzar` inicia el proceso con su base aislada. `Reiniciar` detiene y vuelve a levantar el proceso sobre la misma base. `Crear` envía la orden de operador y lee partida y credenciales. `Iniciar` envía la orden de comenzar.
- `private Process Lanzar()` Los records privados `Eleccion` y `Jugador` conservan los parámetros que usa el banco de prueba HTTP. El constructor de `Servidor` configura la instancia de prueba. `Lanzar` inicia el proceso con su base aislada. `Reiniciar` detiene y vuelve a levantar el proceso sobre la misma base. `Crear` envía la orden de operador y lee partida y credenciales. `Iniciar` envía la orden de comenzar.
- `public async Task Reiniciar()` Los records privados `Eleccion` y `Jugador` conservan los parámetros que usa el banco de prueba HTTP. El constructor de `Servidor` configura la instancia de prueba. `Lanzar` inicia el proceso con su base aislada. `Reiniciar` detiene y vuelve a levantar el proceso sobre la misma base. `Crear` envía la orden de operador y lee partida y credenciales. `Iniciar` envía la orden de comenzar.
- `public async Task Crear()` Los records privados `Eleccion` y `Jugador` conservan los parámetros que usa el banco de prueba HTTP. El constructor de `Servidor` configura la instancia de prueba. `Lanzar` inicia el proceso con su base aislada. `Reiniciar` detiene y vuelve a levantar el proceso sobre la misma base. `Crear` envía la orden de operador y lee partida y credenciales. `Iniciar` envía la orden de comenzar.
- `public async Task Iniciar()` Los records privados `Eleccion` y `Jugador` conservan los parámetros que usa el banco de prueba HTTP. El constructor de `Servidor` configura la instancia de prueba. `Lanzar` inicia el proceso con su base aislada. `Reiniciar` detiene y vuelve a levantar el proceso sobre la misma base. `Crear` envía la orden de operador y lee partida y credenciales. `Iniciar` envía la orden de comenzar.
- `public async Task Renovar(Jugador jugador)` `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `public Task CerrarSesion(Jugador jugador)` `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `public async Task<JsonElement> EsperarVista(Jugador jugador, Func<JsonElement, bool> coincide)` `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `public async Task Incorporar(Jugador jugador, string nombre)` `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `public async Task<JsonElement> Vista(Jugador jugador)` `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `public Task<HttpResponseMessage> Post(Jugador jugador, Eleccion eleccion)` `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `public async Task<HttpResponseMessage> Enviar(HttpMethod metodo, string ruta, Jugador jugador, object? cuerpo = null)` `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `private async Task<string> LeerDato(string prefijo)`. Errores explícitos en el cuerpo: `InvalidOperationException`. `LeerDato` busca la línea cuyo prefijo espera al leer la consola. `Detener` finaliza el proceso y `DisposeAsync` libera el cliente y elimina el directorio temporal.
- `public async ValueTask DisposeAsync()` `LeerDato` busca la línea cuyo prefijo espera al leer la consola. `Detener` finaliza el proceso y `DisposeAsync` libera el cliente y elimina el directorio temporal.
- `private async Task Detener()` `LeerDato` busca la línea cuyo prefijo espera al leer la consola. `Detener` finaliza el proceso y `DisposeAsync` libera el cliente y elimina el directorio temporal.
