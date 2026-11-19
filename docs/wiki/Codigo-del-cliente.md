# Código del cliente de consola

El cliente tiene una demostración local. Los modelos de esta carpeta describen pantallas y la ruta de juego en red sigue pendiente.

## `Program.cs`

[Ver `src/cliente/Program.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/cliente/Program.cs)

- `Main` crea `InterfazConsola` y selecciona el modo por argumentos. Sin argumentos muestra preparación. `--demo` crea la demostración, `--help` imprime el uso y una opción inválida devuelve 2. Ante fin de entrada muestra el cierre local y ante un error de E/S devuelve 1.

## `VistasConsola.cs`

[Ver `src/cliente/VistasConsola.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/cliente/VistasConsola.cs)

El archivo declara tipos de datos sin métodos escritos a mano. `CartaVisible` da ID, nombre y puntajes. `JugadorVisible` contiene el marcador de un jugador. `CartaJugada` reúne jugador, carta y puntaje comparado. `CombateVisible` lleva el resultado y la siguiente acción visible. `MesaVisible` reúne el estado usado en encabezados y paneles. El enum `EstadoEnvio` distingue envío, confirmación pendiente y recepción.

## `DemostracionConsola.cs`

[Ver `src/cliente/DemostracionConsola.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/cliente/DemostracionConsola.cs)

- El constructor primario de `DemostracionConsola` recibe la interfaz y las entradas y salidas. `Recorrer` crea una mesa ficticia con 14 rondas por jugador, muestra la preparación y pide elecciones. Calcula el puntaje de cada combate con datos locales y actualiza los records mediante `with`. Termina cuando uno gana dos combates y muestra una victoria de partida.
- `MostrarCierreSimulado` presenta la pausa o simula abandono, cancelación o salida local. `Continuar` espera Enter en modo interactivo y señala el fin de entrada si falta una línea. `Carta` construye un record de presentación a partir de los seis valores.

## `InterfazConsola.cs`

[Ver `src/cliente/InterfazConsola.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/cliente/InterfazConsola.cs)

### Entrada y selección

- `InterfazConsola(...)` configura la lectura por teclas o líneas, crea el `IAnsiConsole` con su salida y permite fijar el ancho para pruebas. Si la salida está redirigida y no hay ancho explícito, usa presentación lineal.
- Las dos sobrecargas de `PedirCarta` copian las opciones, llaman a `Elegir` y devuelven la carta del índice confirmado. La que recibe `MesaVisible` ajusta texto, marcador y ayuda según si abre el turno o responde.
- `PedirAtributo` usa las opciones autorizadas cuando se proporcionan y, si faltan, usa los seis nombres de atributo. Dibuja la lista y devuelve la opción confirmada.
- `CopiarOpciones` materializa la mano en un arreglo y rechaza una lista vacía. `ListaOpciones` construye filas y `Opcion` marca visualmente la enfocada, incluida una señal textual.
- `Elegir` empieza en índice cero, llama a la función de dibujo y lee teclas. Enter confirma. Las flechas y las teclas equivalentes desplazan el foco. `Math.Clamp` mantiene el índice dentro de la lista.
- `LeerTecla` usa `Console.ReadKey` en terminal interactiva. En entrada redirigida traduce líneas como `s` a teclas y una línea vacía a Enter. Si se acaba la entrada, lanza `EndOfStreamException`.
### Pantallas

- `MostrarPreparacion` coloca identidad y estado en paneles. `MostrarEsperaInicial` presenta participantes, motivo y comienzo a cargo del operador. `MostrarInicioRonda` muestra quién abre, la mano resumida y el marcador.
- La sobrecarga pública de `MostrarMano` llama a la privada sin foco. La privada imprime la cantidad, trata la mano vacía y crea una ficha por carta. `MostrarEsperaRival` presenta la mano propia y el marcador mientras el rival elige.
- `MostrarEnvio` usa `EstadoEnvio` para elegir el texto y presentarlo en el cuerpo o al pie de la pantalla. `MostrarCombate(MesaVisible, CombateVisible)` añade el encabezado de la mesa y llama a la sobrecarga que dibuja el atributo, las dos jugadas, el ganador y el marcador.
- `MostrarCierreRonda` presenta el ganador y el próximo jugador mano si existe. `MostrarCierrePartida` crea la planilla de ambos marcadores. `MostrarSuspension` y `MostrarInterrupcion` distinguen la información confirmada de la que sigue pendiente.
- `MostrarMarcador` imprime nombres, victorias y ronda. `MostrarEsperando` evita repetir el mismo mensaje consecutivo. `MostrarGanadorRonda`, `MostrarGanadorPartida` y `MostrarError` escriben mensajes especializados. `MostrarEstado` arma un encabezado y escribe el mensaje.
### Composición y salida

- Las dos sobrecargas de `Encabezado` convierten los datos de mesa en etiquetas y dibujan la barra de contexto. `Pie` imprime la acción o ayuda disponible. `Paneles` coloca los paneles juntos cuando el ancho alcanza y los organiza en filas cuando hace falta.
- `PanelInfo` convierte una cadena a texto visible o envuelve un objeto renderizable en un panel. `Ficha` compone nombre y atributos en una cuadrícula. `FichaJugada` dibuja nombre, puntaje y señal del ganador al presentar un resultado.
- `ResumenMano` lista nombres o indica que está vacía. `Rondas` y `Combates` formatean los dos marcadores. `Marcador` combina ambas cadenas.
- `TextoVisible` cambia caracteres de control y formato por espacios. `Escribir` divide el mensaje en líneas y construye filas de texto. `Renderizar` escribe los componentes en Spectre.Console e invalida el recuerdo del último mensaje de espera.

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
