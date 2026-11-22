# InterfazConsola.cs

Fuente: [src/cliente/InterfazConsola.cs](../../../../src/cliente/InterfazConsola.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Dibuja paneles del juego y ofrece selección síncrona para el auxiliar de demostración. El cliente REST usa principalmente la otra porción partial, [InterfazConsola.Red](InterfazConsola.Red.md), con su coordinador asíncrono. Este archivo no envía HTTP ni calcula victorias de juego.

## Dependencias

Spectre.Console 0.57.2 aporta IAnsiConsole, Text, Panel, Rows, Columns, Grid, Table y Rule. `CultureInfo.InvariantCulture` fija números; UnicodeCategory permite neutralizar texto remoto. [VistasConsola](VistasConsola.md) lleva datos; [DemostracionConsola](DemostracionConsola.md) pide selecciones; [SesionPartidaCliente](SesionPartidaCliente.md) usa presentación de red. Véase [diseño de interfaz](../../../diseno-de-interfaz.md).

## Estructura

`internal sealed partial class InterfazConsola`; la porción de red declara MostrarRed y no duplica estos campos. No tiene interfaces, atributos, eventos ni operadores propios. Métodos públicos tienen visibilidad efectiva limitada por el tipo internal; los ayudantes son privados y algunos static.

## Estado y datos

| Declaración | Valor y uso |
|---|---|
| private static readonly `string[] Atributos` | Fuerza, Velocidad, Resistencia, Inteligencia, Energía, Habilidad; opciones por defecto. readonly no congela elementos, pero el código no los muta. |
| private static readonly `Color Verde` | RGB 189,232,140. |
| private static readonly `Style Titulo` | Verde y negrita. |
| private static readonly `Style Barra` | Verde sobre RGB 32,44,36 y negrita. |
| private static readonly `Style Borde` | RGB 95,120,105. |
| private const `string Controles` | Flechas/WASD/HJKL, salto de línea y Enter para confirmar. |
| private readonly `TextReader entrada` | Entrada inyectada o Console.In; no se dispone aquí. |
| private readonly bool `leerTeclas`, `limpiarPantalla`, `salidaLineal` | Fijados en constructor según redirección, lector y ancho. |
| private readonly `IAnsiConsole terminal` | Consola de Spectre con salida/configuración propia. |
| private `string? ultimaEspera` | null inicial; evita repetir un mismo mensaje de espera, se invalida con cualquier Renderizar. |

## Miembros y ejecución

### Construcción y pantallas de preparación/mano

`public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)` considera interactiva solo si salida no inyectada y stdout no redirigido. En ese caso limpia pantalla; lee teclas solo además si no hay lector inyectado y stdin no redirigido. Salida lineal se activa si no interactiva y ancho no proporcionado. Crea terminal con ANSI Detect en interactiva o No en el resto. Si ancho presente fija Profile.Width y fuerza capacidad Unicode. No valida ancho manualmente ni posee/disciplina los flujos del llamador.

- `public void MostrarPreparacion(string? local, string estado)`: encabezado Preparación, panel identidad (pendiente si null) y estado sanitizado.
- `public void MostrarEsperaInicial(string local, string participantes, string motivo)`: encabezado con motivo, participantes e inicio a cargo del operador; pie esperar rival/inicio.
- `public void MostrarInicioRonda(MesaVisible mesa, string mano, IReadOnlyList<CartaVisible> cartas)`: anuncia mano, resume cartas y rondas. Si mesa.Local==mano pie pide carta/atributo; si no, espera al nombre mano. Comparación de strings, no roles, en este auxiliar local.
- `public void MostrarMano(IReadOnlyList<CartaVisible> mano)` delega a privada con foco null.
- `private void MostrarMano(IReadOnlyList<CartaVisible> mano, int? foco, string titulo = "Tu mano")`: título con cantidad y singular/plural; mano vacía imprime aviso y retorna. Si hay cartas crea Ficha por elemento, enfocada cuando índice==foco, y las pasa a Paneles. No cambia la mano.
- `public void MostrarEsperaRival(MesaVisible mesa, string rival, IReadOnlyList<CartaVisible> cartas)`: estado rival eligiendo, resumen de mano y marcador, pie espera de solicitud.

### Selección síncrona

`public CartaVisible PedirCarta(IReadOnlyList<CartaVisible> mano)` copia opciones con CopiarOpciones y llama Elegir. Su lambda Action<int> redibuja encabezado, mano con foco y controles; retorna la carta del índice confirmado.

`public CartaVisible PedirCarta(MesaVisible mesa, IReadOnlyList<CartaVisible> cartas, bool abre)` hace la misma copia. Lambda distingue apertura (rondas y pie indicando atributo siguiente) de respuesta (combates/rondas y Cartas disponibles); retorna referencia confirmada. No retira cartas.

`public string PedirAtributo(IReadOnlyList<string>? permitidos = null, MesaVisible? mesa = null)` copia permitidos o usa seis Atributos. Cero opciones lanza InvalidOperationException antes de dibujar. Lambda usa encabezado simple o de mesa según null, panel de lista y pie de controles; retorna el string del índice confirmado. No valida sus nombres contra la lista canónica si el llamador pasa otros.

`private int Elegir(int cantidad, Action<int> dibujar)` inicializa foco 0 y dibuja. Repite LeerTecla; Enter retorna foco. Up/Left/W/A/K/H restan uno, Down/Right/S/D/J/L suman uno, otras teclas paso 0. Clamp evita salir de [0,cantidad−1], sin circular; solo redibuja si cambió foco. Las precondiciones de cantidad positiva las garantizan CopiarOpciones/PedirAtributo. No hay cancelación por token ni Esc especial en esta selección síncrona.

```csharp
            int paso = tecla switch
            {
                ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.W or ConsoleKey.A or ConsoleKey.K or ConsoleKey.H => -1,
                ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.S or ConsoleKey.D or ConsoleKey.J or ConsoleKey.L => 1,
                _ => 0
            };

            int nuevo = Math.Clamp(foco + paso, 0, cantidad - 1);
            if (nuevo != foco)
            {
                foco = nuevo;
                dibujar(foco);
            }
```

`private ConsoleKey LeerTecla()` usa ReadKey(intercept:true).Key si leerTeclas; de otro modo ReadLine, EOF lanza EndOfStreamException con mensaje. Trim y minúsculas: vacío Enter; w/a/s/d/h/j/k/l sus teclas; cualquier otro texto NoName. A diferencia de EntradaCliente, aquí `esc` escrito no se convierte en Escape, ni Ctrl+D/Ctrl+Z físicos tienen un tratamiento propio sobre KeyChar.

### Envíos, resultados, cierres y avisos públicos

| Firma | Decisiones, salida y efectos |
|---|---|
| `void MostrarEnvio(MesaVisible mesa, EstadoEnvio estado)` | Encabezado; switch Enviando→en curso, Recibido→confirmada, Pendiente→comprobando recibo. Estado enum desconocido lanza ArgumentOutOfRangeException. Siempre título elección en curso; Pendiente usa Pie, otros Escribir. |
| `void MostrarCombate(MesaVisible mesa, CombateVisible combate)` | Encabezado con rondas y delega a la sobrecarga sin mesa. |
| `void MostrarCombate(CombateVisible combate)` | Escribe atributo, determina resaltado usando GanaJ1 explícito o comparación de nombre J1 con Ganador. Crea fichas jugadas. Si ancho<54 usa salto de línea entre elementos; si no separador punto medio. Si ProximoEnElegir no null añade anuncio; pie con victorias de combate. No compara puntajes para elegir ganador. |
| `void MostrarCierreRonda(MesaVisible mesa, string ganador, string? proximoMano)` | Encabezado ronda finalizada; ganador, combates, rondas/meta15. Si próximo no null anuncia ronda+1; si null omite ese pie. No incrementa marcador. |
| `void MostrarCierrePartida(string local, string ganador, JugadorVisible j1, JugadorVisible j2)` | Encabezado y ganador; tabla dos filas con nombres/rondas, borde cuadrado; pie Enter salir. No lee Enter. |
| `void MostrarSuspension(MesaVisible mesa, string motivo, bool confirmada)` | Distingue pausa confirmada de sin comunicación; muestra último marcador confirmado. |
| `void MostrarInterrupcion(string local, string causa, string efecto, bool confirmada)` | Encabezado de sesión; etiqueta Final confirmado o Salida local, causa y efecto en pie. |
| `void MostrarMarcador(JugadorVisible j1, JugadorVisible j2, int ronda)` | Encabezado sin identidad local con número de ronda y rondas de ambos; no cambia estado. |
| `void MostrarEsperando(string mensaje)` | Si ultimaEspera==mensaje retorna sin imprimir. En otro caso llama MostrarEstado y después registra el mensaje (Renderizar lo había invalidado). |
| `void MostrarGanadorRonda(JugadorVisible ganador)` | MostrarEstado con nombre y rondas. |
| `void MostrarGanadorPartida(JugadorVisible ganador)` | Igual, título de partida. |
| `void MostrarError(string mensaje)` | Texto prefijado ERROR, rojo y negrita; contenido sanitizado. |
| `void MostrarEstado(string titulo, string mensaje)` | Encabezado sin local con título y después mensaje sanitizado. |

Todas las firmas de esta tabla son public y retornan void. Los objetos de entrada son de solo lectura desde estas operaciones.

### Composición y formato privados

- `void Encabezado(MesaVisible mesa, string? detalle = null)`: delega con Local, Ronda y Combate al otro encabezado.
- `void Encabezado(string? local, string situacion, string? detalle = null)`: si limpiarPantalla, terminal.Clear(home:true). Identidad GUERRA o GUERRA/local. Sanitiza situación/detalle, compone con separador; si longitud >= ancho, coloca cada parte en línea separada. Escribe línea vacía, barra y Rule. No trunca datos.
- `void Pie(string mensaje)`: Rule y Escribir con estilo Barra.
- `void Paneles(params Panel[] paneles)`: si no salidaLineal, hay más de uno y ancho≥cantidad×27, reparte `espacio=ancho−cantidad+1`, anchura entera y resto a los primeros paneles; Renderizar Columns con padding derecho 1 y return. En otros casos recorre y renderiza uno por uno. No cambia orden, sí Width de cada panel recibido.

```csharp
        if (!salidaLineal && paneles.Length > 1 && ancho >= paneles.Length * 27)
        {
            int espacio = ancho - paneles.Length + 1;
            for (int indice = 0; indice < paneles.Length; indice++)
                paneles[indice].Width = espacio / paneles.Length + (indice < espacio % paneles.Length ? 1 : 0);

            Renderizar(new Columns(paneles) { Padding = new Padding(0, 0, 1, 0) });
            return;
        }

        foreach (Panel panel in paneles)
            Renderizar(panel);
```

- `static Panel PanelInfo(string titulo, string detalle)`: separa detalle en saltos y crea Rows de Text sanitizados; delega a sobrecarga IRenderable.
- `static Panel PanelInfo(string titulo, IRenderable detalle)`: Panel con título sanitizado y Markup.Escape dentro del markup fijo negrita verde; borde cuadrado, estilo Borde, Expand=true. Solo el título usa markup y está escapado.
- `static Panel Ficha(CartaVisible carta, bool enfocada)`: Grid de dos columnas (valor alineado derecha), recorre mapa en su orden de enumeración y agrega atributos y enteros invariantes; Rows con Opcion(nombre,enfocada) y grid. Borde Titulo si enfocada, si no Borde.
- `static Panel FichaJugada(CartaJugada jugada, bool ganadora)`: Rows de jugador/carta, valor y Ganador o vacío; borde ganador resaltado. No infiere ganador por número.
- `static string ResumenMano(IReadOnlyList<CartaVisible> cartas)`: aviso si vacía, si no concatena nombres sanitizados con punto medio.
- `static string Rondas(MesaVisible mesa)`: nombre J1/rondas J1, barra, nombre J2/rondas J2.
- `static string Combates(MesaVisible mesa)`: mismo formato con contadores parciales.
- `static string Marcador(MesaVisible mesa)`: Combates y Rondas en dos líneas.
- `static CartaVisible[] CopiarOpciones(IReadOnlyList<CartaVisible> mano)`: Count>0 produce ToArray; si cero lanza InvalidOperationException. Es copia de colección, no clon de cartas.
- `static Rows ListaOpciones(IEnumerable<string> opciones, int foco)`: enumera opciones y genera Opcion destacando índice igual a foco.
- `static Text Opcion(string opcion, bool enfocada)`: enfocada usa prefijo `> ` y estilo Barra; resto dos espacios sin estilo. Sanitiza texto, no interpreta markup.
- `static string TextoVisible(string texto)`: por cada carácter reemplaza por espacio controles, categorías Format, LineSeparator y ParagraphSeparator; resto conserva. No borra markup textual, que se dibuja como Text.

```csharp
    private static string TextoVisible(string texto) =>
        string.Concat(texto.Select(caracter =>
            char.IsControl(caracter) || char.GetUnicodeCategory(caracter) is UnicodeCategory.Format
                or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator
                ? ' ' : caracter));
```

- `void Escribir(string texto, Style? estilo = null)`: divide por `\n`, crea Text sanitizado para cada línea y Renderizar Rows. La división previa conserva saltos internos de textos compuestos; controles incrustados en nombres ya se neutralizan antes de concatenarlos.
- `void Renderizar(IRenderable contenido)`: terminal.Write y ultimaEspera=null. Todas las salidas por aquí invalidan deduplicación salvo que MostrarEsperando la establezca después.

## Recorridos y ejemplos

Normal: mano [A,B,C], foco inicial A. Teclas S,S,S,Enter seleccionan C: el tercer S queda limitado en índice2 y no redibuja; Enter retorna la misma instancia C sin quitarla. Un ancho 90 y tres paneles permite columnas (90≥81); ancho 60 obliga apilado para tres.

Alternativo: mano vacía en MostrarMano imprime aviso; la misma mano en PedirCarta lanza antes de leer. Un nombre remoto con escape/control se muestra con espacios en esas posiciones, sin ejecutar controles de terminal. EOF de lector síncrono lanza; no se transforma en confirmación. MostrarEnvio((EstadoEnvio)99) lanza. No hay un flujo alternativo de red aquí: eso se presenta en la porción Red.

## Errores, límites y efectos observables

Solo modifica pantalla, terminal y ultimaEspera, además del Width de paneles internos. E/S, datos null o claves ausentes pueden lanzar: no hay captura general ni validación semántica de las vistas. La selección síncrona bloquea esperando entrada y no coordina presencia; el ejecutable REST usa EntradaCliente y SesionPartidaCliente para evitar ese bloqueo. No conserva ni envía elecciones en este archivo.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### Constructor: terminal y redirección

El modo interactivo determina limpieza, ANSI y teclas; ancho explícito configura perfil y permite probar composición sin terminal física.

```csharp
    public InterfazConsola(TextReader? entrada = null, TextWriter? salida = null, int? ancho = null)
    {
        this.entrada = entrada ?? Console.In;
        bool interactiva = salida is null && !Console.IsOutputRedirected;
        limpiarPantalla = interactiva;
        leerTeclas = interactiva && entrada is null && !Console.IsInputRedirected;
        salidaLineal = !interactiva && ancho is null;
        terminal = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(salida ?? Console.Out),
            Ansi = interactiva ? AnsiSupport.Detect : AnsiSupport.No
        });

        if (ancho is { } fijo)
        {
            terminal.Profile.Width = fijo;
            terminal.Profile.Capabilities.Unicode = true;
        }
    }
```

### MostrarInicioRonda y MostrarMano: estado y vacío

La igualdad de nombre decide el pie local del auxiliar. El aviso de mano vacía no intenta crear columnas de cartas.

```csharp
    public void MostrarInicioRonda(MesaVisible mesa, string mano, IReadOnlyList<CartaVisible> cartas)
    {
        Encabezado(mesa, $"{TextoVisible(mano)} abre la ronda");
        Paneles(PanelInfo("Tu mano", ResumenMano(cartas)), PanelInfo("Rondas ganadas", Rondas(mesa)));
        Pie(mesa.Local == mano ? "Elegí una carta y después un atributo." : $"Esperá a {TextoVisible(mano)}.");
    }

    public void MostrarMano(IReadOnlyList<CartaVisible> mano) => MostrarMano(mano, foco: null);

    private void MostrarMano(IReadOnlyList<CartaVisible> mano, int? foco, string titulo = "Tu mano")
    {
        Escribir($"{titulo} · {mano.Count} {(mano.Count == 1 ? "carta" : "cartas")}", Titulo);
        if (mano.Count == 0)
        {
            Escribir("No hay cartas disponibles.");
            return;
        }

        Paneles(mano.Select((carta, i) => Ficha(carta, i == foco)).ToArray());
    }
```

### PedirCarta: lambda de selección contextual

La copia fija las opciones durante la selección síncrona. El callback redibuja apertura o respuesta, y el índice retornado selecciona la referencia original.

```csharp
    public CartaVisible PedirCarta(MesaVisible mesa, IReadOnlyList<CartaVisible> cartas, bool abre)
    {
        CartaVisible[] opciones = CopiarOpciones(cartas);
        int elegida = Elegir(opciones.Length, foco =>
        {
            string estado = abre
                ? $"Rondas ganadas · {Rondas(mesa)}"
                : $"Te toca responder · Marcador · Combates {Combates(mesa)} · Rondas {Rondas(mesa)}";
            Encabezado(mesa, estado);
            MostrarMano(opciones, foco, abre ? "Tu mano" : "Cartas disponibles");
            Pie(abre ? $"Tu turno\nDespués elegís el atributo.\n{Controles}" : Controles);
        });
        return opciones[elegida];
    }
```

### PedirAtributo: guarda y callback

La lista vacía falla antes de leer; mesa opcional elige encabezado, sin cambiar reglas de foco.

```csharp
    public string PedirAtributo(IReadOnlyList<string>? permitidos = null, MesaVisible? mesa = null)
    {
        string[] opciones = permitidos?.ToArray() ?? Atributos;
        if (opciones.Length == 0)
            throw new InvalidOperationException("Se solicitó un atributo sin opciones disponibles.");

        int elegida = Elegir(opciones.Length, foco =>
        {
            if (mesa is null)
                Encabezado((string?)null, "Elegí un atributo");
            else
                Encabezado(mesa, "Elegí un atributo");

            Paneles(PanelInfo("Atributo del combate", ListaOpciones(opciones, foco)));
            Pie(Controles);
        });
        return opciones[elegida];
    }
```

### MostrarEnvio: estados válidos y desconocidos

Cada enum tiene mensaje propio; desconocido lanza. Pendiente se enfatiza en el pie en vez de describirse como recepción.

```csharp
    public void MostrarEnvio(MesaVisible mesa, EstadoEnvio estado)
    {
        Encabezado(mesa);
        (string etiqueta, string mensaje) = estado switch
        {
            EstadoEnvio.Enviando => ("Envío en curso", "Enviando tu elección…"),
            EstadoEnvio.Recibido => ("Recepción confirmada", "Elección recibida. Esperando el resultado."),
            EstadoEnvio.Pendiente => ("Confirmación pendiente", "Comprobando el recibo. La elección sigue en trámite."),
            _ => throw new ArgumentOutOfRangeException(nameof(estado))
        };
        Escribir("Elección en curso", Titulo);
        if (estado == EstadoEnvio.Pendiente)
            Pie($"{etiqueta} · {TextoVisible(mensaje)}");
        else
            Escribir($"{etiqueta} · {TextoVisible(mensaje)}");
    }
```

### MostrarCombate: ganador y ancho

GanaJ1 explícito prevalece sobre nombres; el ancho cambia separadores y ProximoEnElegir null evita anunciar otro combate.

```csharp
    public void MostrarCombate(CombateVisible combate)
    {
        Escribir($"Atributo · {TextoVisible(combate.Atributo)}", Titulo);
        bool ganaJ1 = combate.GanaJ1 ?? combate.J1.Jugador == combate.Ganador;
        Paneles(FichaJugada(combate.J1, ganaJ1), FichaJugada(combate.J2, !ganaJ1));

        string separador = terminal.Profile.Width < 54 ? "\n" : " · ";
        string siguiente = combate.ProximoEnElegir is { } nombre
            ? $"\nPróximo combate{separador}{TextoVisible(nombre)} elige el atributo" : "";
        Pie($"Combates de la ronda{separador}{TextoVisible(combate.J1.Jugador)} {combate.CombatesGanadosJ1} / {TextoVisible(combate.J2.Jugador)} {combate.CombatesGanadosJ2}{siguiente}");
    }
```

### MostrarCierreRonda y MostrarCierrePartida

El cierre de ronda omite próximo mano si no existe. El cierre de partida crea tabla con valores recibidos y solo imprime indicación, sin leer Enter.

```csharp
    public void MostrarCierreRonda(MesaVisible mesa, string ganador, string? proximoMano)
    {
        Encabezado(mesa.Local, $"Ronda {mesa.Ronda} finalizada");
        Escribir($"{TextoVisible(ganador)} gana la ronda", Titulo);
        Escribir($"Combates · {Combates(mesa)}");
        Escribir($"Rondas ganadas · {Rondas(mesa)} · Meta 15");
        if (proximoMano is not null)
            Pie($"{TextoVisible(proximoMano)} abre la ronda {mesa.Ronda + 1}");
    }

    public void MostrarCierrePartida(string local, string ganador, JugadorVisible j1, JugadorVisible j2)
    {
        Encabezado(local, "Partida finalizada");
        Escribir($"{TextoVisible(ganador)} gana la partida", Titulo);
        var planilla = new Table { Border = TableBorder.Square, BorderStyle = Borde, Expand = true };
        planilla.AddColumn("Jugador").AddColumn("Resultado");
        planilla.AddRow(new Text(TextoVisible(j1.Nombre)), new Text($"{j1.RondasGanadas} rondas ganadas"));
        planilla.AddRow(new Text(TextoVisible(j2.Nombre)), new Text($"{j2.RondasGanadas} rondas ganadas"));
        Renderizar(planilla);
        Pie("Pulsá Enter para salir.");
    }
```

### MostrarSuspension y MostrarInterrupcion

Los booleanos confirmada cambian etiquetas, evitando presentar pérdida de red o cierre local como final remoto.

```csharp
    public void MostrarSuspension(MesaVisible mesa, string motivo, bool confirmada)
    {
        Encabezado(mesa);
        Escribir("Jugada temporalmente indisponible", Titulo);
        string estado = confirmada ? "Pausa confirmada" : "Sin comunicación · Estado actual sin confirmar";
        Escribir($"{TextoVisible(estado)} · {TextoVisible(motivo)}");
        Pie($"Último marcador confirmado · {Marcador(mesa)}");
    }

    public void MostrarInterrupcion(string local, string causa, string efecto, bool confirmada)
    {
        Encabezado(local, $"Sesión de {TextoVisible(local)}");
        Escribir("Cierre de sesión o partida", Titulo);
        Escribir($"{(confirmada ? "Final confirmado" : "Salida local")} · {TextoVisible(causa)}");
        Pie(TextoVisible(efecto));
    }
```

### MostrarEsperando: deduplicación

Solo un mensaje igual al último de espera se omite; MostrarEstado renderiza y después se vuelve a fijar ultimaEspera.

```csharp
    public void MostrarEsperando(string mensaje)
    {
        if (ultimaEspera == mensaje)
            return;

        MostrarEstado("En espera", mensaje);
        ultimaEspera = mensaje;
    }
```

### Encabezado: limpieza y disposición

El encabezado elige identidad, sanea texto y apila las partes si su longitud alcanza el ancho. No interpreta texto remoto como markup.

```csharp
    private void Encabezado(string? local, string situacion, string? detalle = null)
    {
        if (limpiarPantalla)
            terminal.Clear(home: true);

        string identidad = local is null ? "GUERRA" : $"GUERRA / {TextoVisible(local)}";
        string[] partes = detalle is null ? [identidad, TextoVisible(situacion)] : [identidad, TextoVisible(situacion), TextoVisible(detalle)];
        string linea = string.Join("  │  ", partes);
        terminal.WriteLine();
        Escribir(linea.Length >= terminal.Profile.Width ? string.Join('\n', partes) : linea, Barra);
        Renderizar(new Rule { Style = Borde });
    }
```

### PanelInfo y Ficha: texto y atributos

El título de panel escapa markup; los valores de cartas se convierten con cultura invariante y el foco solo cambia el estilo de borde.

```csharp
    private static Panel PanelInfo(string titulo, string detalle) =>
        PanelInfo(titulo, new Rows(detalle.Split('\n').Select(linea => new Text(TextoVisible(linea)))));

    private static Panel PanelInfo(string titulo, IRenderable detalle) => new(detalle)
    {
        Header = new PanelHeader($"[bold #bde88c]{Markup.Escape(TextoVisible(titulo))}[/]"),
        Border = BoxBorder.Square,
        BorderStyle = Borde,
        Expand = true
    };

    private static Panel Ficha(CartaVisible carta, bool enfocada)
    {
        var valores = new Grid { Expand = true };
        valores.AddColumn(new GridColumn());
        valores.AddColumn(new GridColumn { Alignment = Justify.Right });
        foreach ((string atributo, int valor) in carta.Atributos)
            valores.AddRow(new Text(TextoVisible(atributo)), new Text(valor.ToString(CultureInfo.InvariantCulture), Titulo));

        var ficha = new Rows(Opcion(carta.Nombre, enfocada), valores);
        return new Panel(ficha) { Border = BoxBorder.Square, BorderStyle = enfocada ? Titulo : Borde, Expand = true };
    }
```

### FichaJugada: ganador anunciado

Recibe el booleano ganador, dibuja el valor sin compararlo y añade etiqueta/borde ganador.

```csharp
    private static Panel FichaJugada(CartaJugada jugada, bool ganadora)
    {
        var resultado = new Rows(
            new Text(TextoVisible($"{jugada.Jugador} · {jugada.Carta}")),
            new Text(jugada.Valor.ToString(CultureInfo.InvariantCulture), Titulo),
            new Text(ganadora ? "Ganador" : "", Titulo));
        return new Panel(resultado)
        {
            Border = BoxBorder.Square,
            BorderStyle = ganadora ? Titulo : Borde,
            Expand = true
        };
    }
```

### ResumenMano y formatos de marcador

Estas expresiones no cambian marcador: resumen vacío, contadores, guarda de opciones, fila enfocada y lista se construyen a partir de datos recibidos.

```csharp
    private static string ResumenMano(IReadOnlyList<CartaVisible> cartas) => cartas.Count == 0
        ? "No hay cartas disponibles."
        : string.Join(" · ", cartas.Select(c => TextoVisible(c.Nombre)));

    private static string Rondas(MesaVisible mesa) =>
        $"{TextoVisible(mesa.J1.Nombre)} {mesa.J1.RondasGanadas} / {TextoVisible(mesa.J2.Nombre)} {mesa.J2.RondasGanadas}";

    private static string Combates(MesaVisible mesa) =>
        $"{TextoVisible(mesa.J1.Nombre)} {mesa.CombatesJ1} / {TextoVisible(mesa.J2.Nombre)} {mesa.CombatesJ2}";

    private static string Marcador(MesaVisible mesa) =>
        $"Combates · {Combates(mesa)}\nRondas · {Rondas(mesa)}";

    private static CartaVisible[] CopiarOpciones(IReadOnlyList<CartaVisible> mano) =>
        mano.Count > 0 ? mano.ToArray() : throw new InvalidOperationException("Se solicitó una carta con la mano vacía.");

    private static Rows ListaOpciones(IEnumerable<string> opciones, int foco) =>
        new(opciones.Select((opcion, i) => Opcion(opcion, i == foco)));

    private static Text Opcion(string opcion, bool enfocada) =>
        enfocada ? new Text($"> {TextoVisible(opcion)}", Barra) : new Text($"  {TextoVisible(opcion)}");
```

### LeerTecla: modo físico y línea

En modo de líneas solo las teclas listadas se traducen; EOF lanza y nunca se transforma en Enter.

```csharp
    private ConsoleKey LeerTecla()
    {
        if (leerTeclas)
            return Console.ReadKey(intercept: true).Key;

        // Con entrada por líneas, una línea vacía equivale a presionar Enter.
        string linea = entrada.ReadLine()
            ?? throw new EndOfStreamException("Se cerró la entrada de consola.");

        return linea.Trim().ToLowerInvariant() switch
        {
            "" => ConsoleKey.Enter,
            "w" => ConsoleKey.W,
            "a" => ConsoleKey.A,
            "s" => ConsoleKey.S,
            "d" => ConsoleKey.D,
            "h" => ConsoleKey.H,
            "j" => ConsoleKey.J,
            "k" => ConsoleKey.K,
            "l" => ConsoleKey.L,
            _ => ConsoleKey.NoName
        };
    }
```

### Escribir y Renderizar: salida y caché de espera

Separar líneas precede al saneado y toda renderización invalida ultimaEspera. La caché solo se vuelve a establecer desde MostrarEsperando.

```csharp
    private void Escribir(string texto, Style? estilo = null) =>
        Renderizar(new Rows(texto.Split('\n').Select(linea => new Text(TextoVisible(linea), estilo))));

    private void Renderizar(IRenderable contenido)
    {
        terminal.Write(contenido);
        ultimaEspera = null;
    }
```

## Relaciones y verificación

[InterfazConsolaTests](../../test/proyecto-integrador-final-tests/InterfazConsolaTests.md) cubre selección, foco, fin de entrada y formato; [PantallasConsolaTests](../../test/proyecto-integrador-final-tests/PantallasConsolaTests.md) cubre preparación, espera, anchos, envío y cierres; [PresentacionRedTests](../../test/proyecto-integrador-final-tests/PresentacionRedTests.md) agrega pruebas del adaptador. Esos escenarios no cubren todos los drivers/terminales ni garantizan renderizado idéntico entre plataformas. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
