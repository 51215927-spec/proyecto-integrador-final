# Jugador.cs

Fuente: [src/servidor/Jugador.cs](../../../../src/servidor/Jugador.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Representa el nombre, la mano y el contador de rondas de un participante del modelo local. Lo usan `Combate`, `Ronda` y `Partida`; no representa la sesión HTTP ni una fila de SQLite. Sus operaciones son síncronas y solo cambian memoria.

## Dependencias

[Personaje](Personaje.md) es el elemento de la mano. `List<T>` permite agregar y quitar; LINQ `ToArray` produce copias de la colección. `ArgumentException` y `ArgumentNullException` rechazan entradas inválidas. Véanse las [reglas](../../../reglas-del-juego.md) para distinguir rondas de combates.

## Estructura

`internal class Jugador`, sin base explícita, interfaces, atributos ni porciones parciales. Sus métodos son públicos dentro de la visibilidad efectiva del tipo. No declara funciones locales, operadores ni eventos.

## Estado y datos

| Campo privado | Inicialización y cambios |
|---|---|
| `readonly string nombre` | Constructor; se conserva literalmente, sin recortar espacios. No se reasigna. |
| `readonly List<Personaje> mano` | Lista vacía; readonly impide reemplazarla, pero no agregar o borrar elementos. |
| `int rondasGanadas` | Cero; se incrementa sin límite de quince dentro de este tipo. |

Los elementos son referencias compartidas a personajes; las copias de array no clonan esos personajes. No hay sincronización para acceso concurrente.

## Miembros y ejecución

- `public Jugador(string nombre)`: primero rechaza null, vacío o solo espacios con `ArgumentException` y parámetro `nombre`. Si acepta, asigna nombre, crea mano vacía y deja contador cero. No exige nombre único.
- `public void RecibirCartas(Personaje[] personajes)`: null produce `ArgumentNullException` antes de mutar. `AddRange` agrega todas las referencias al final, conservando orden. No limita tamaño, comprueba elementos nulos ni evita repetidos. Array vacío no cambia la mano.
- `public void QuitarCarta(Personaje personaje)`: rechaza null y llama a `Remove`. Quita la primera coincidencia; como Personaje no redefine igualdad, coincide por referencia. Si no existe, no hace nada y no informa un bool al llamador.
- `public Personaje[] EntregarCartas()`: copia toda la mano al array de retorno y vacía la lista. Una segunda llamada devuelve array vacío. No afecta el marcador.
- `public Personaje[] GetCartas()`: devuelve copia de la colección actual sin vaciarla; modificar una posición del array no modifica la lista.
- `public void SumarRondaGanada()`: incrementa el entero en uno; no valida fin de partida ni asigna ganador.
- `public int GetRondasGanadas()` y `public string GetNombre()`: devuelven los campos correspondientes sin efectos secundarios.
- `public bool TieneCarta(Personaje personaje)`: rechaza null; devuelve `mano.Contains(personaje)`, por referencia; no consume la carta.

La secuencia decisiva de `EntregarCartas` es real:

```csharp
        public Personaje[] EntregarCartas()
        {
            Personaje[] cartas = this.mano.ToArray();
            this.mano.Clear();

            return cartas;
        }
```

La copia sucede antes del vaciado, por eso el llamador puede devolver las cartas al mazo sin perderlas. La lectura sin consumo usa:

```csharp
        public Personaje[] GetCartas() => this.mano.ToArray();

        public void SumarRondaGanada() => this.rondasGanadas++;

        public int GetRondasGanadas() => this.rondasGanadas;

        public string GetNombre() => this.nombre;
```

### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: rechazo de nombre y estado inicial.

```csharp
        public Jugador(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre del jugador no puede estar vacío.",
                    nameof(nombre)
                );

            this.nombre = nombre;
            this.mano = new List<Personaje>();
            this.rondasGanadas = 0;
        }
```

RecibirCartas: se valida el contenedor y se agrega en orden.

```csharp
        public void RecibirCartas(Personaje[] personajes)
        {
            ArgumentNullException.ThrowIfNull(personajes);
            this.mano.AddRange(personajes);
        }
```

QuitarCarta: null se rechaza y Remove solo retira una coincidencia.

```csharp
        public void QuitarCarta(Personaje personaje)
        {
            ArgumentNullException.ThrowIfNull(personaje);
            this.mano.Remove(personaje);
        }
```

TieneCarta: null no equivale a carta ausente.

```csharp
        public bool TieneCarta(Personaje personaje)
        {
            ArgumentNullException.ThrowIfNull(personaje);
            return this.mano.Contains(personaje);
        }
```

## Recorridos y ejemplos

Normal: `new Jugador("Ana")` tiene cero cartas y rondas. Recibir `[A,B]` y después `[C]` deja `[A,B,C]`; quitar B deja `[A,C]`; entregar devuelve `[A,C]` y deja mano vacía. Dos llamadas a sumar dejan marcador 2.

Alternativo: quitar D cuando solo están A y C deja ambas referencias intactas. `RecibirCartas(null)` lanza antes de agregar; `new Jugador("   ")` no produce instancia válida. Estos recorridos se deducen por lectura; no son evidencia de ejecución propia de esta página.

## Errores, límites y efectos observables

Solo hay efectos en memoria. El tipo permite manos mayores a tres y contadores mayores a quince; son responsabilidades del coordinador. No devuelve cartas automáticamente al quitar: Combate guarda la referencia para recogerla al cerrar la ronda. Un array de entrada con elementos nulos no queda validado por la guarda del array.

## Relaciones y verificación

`JugadorTests.IniciaConNombreManoVaciaYSinVictorias` comprueba nombre, mano inicial y dos incrementos. `AcumulaCartasQuitaUnaYEntregaElResto` verifica acumulación, copias defensivas, retirada y entrega repetida. [Fuente de pruebas](../../test/proyecto-integrador-final-tests/JugadorTests.md). No cubren explícitamente todas las guardas nulas ni concurrencia. [Plan de pruebas](../../../plan-de-pruebas.md): consultar allí evidencia histórica; esta página no implica que se haya ejecutado la suite de esta versión.
