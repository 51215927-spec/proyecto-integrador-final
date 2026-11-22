# Mazo.cs

Fuente: [src/servidor/Mazo.cs](../../../../src/servidor/Mazo.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Contiene la reserva de cartas del modelo local: copia la lista inicial, mezcla, retira un prefijo y agrega devoluciones al final. `Partida` lo utiliza antes de repartir tres cartas a cada jugador y al recoger la ronda. No carga el catálogo ni persiste cambios.

## Dependencias

Los elementos son [Personaje](Personaje.md). `List<T>` mantiene el orden; `Take`/`ToArray` de LINQ construyen el array repartido; `Random.Shared` proporciona el generador compartido. Véanse las [reglas](../../../reglas-del-juego.md).

## Estructura

`internal class Mazo`, sin herencia explícita, interfaces, atributos ni partial. No tiene eventos, propiedades, operadores ni funciones locales. Todos los métodos declarados son públicos.

## Estado y datos

`private List<Personaje> personajes` se asigna con `new List<Personaje>(personajes)` en el constructor. Copia el contenedor, no las instancias; limpiar la lista de origen no vacía el mazo. El campo no es readonly, aunque los métodos actuales no lo reasignan. El orden cambia por mezcla, retiro y devolución. No hay bloqueo concurrente.

## Miembros y ejecución

- `public Mazo(List<Personaje> personajes)`: copia elementos en orden; una lista null falla mediante el constructor de `List<T>`. Admite lista vacía, duplicados y referencias nulas sin inspeccionarlos.
- `public void Mezclar()`: parte de Count. Mientras n > 1 decrementa n, sortea k entre 0 y n inclusive e intercambia ambas posiciones. Termina tras Count−1 intercambios para Count ≥ 2; tamaños 0 y 1 no sortean. Mantiene tamaño y multiplicidad. No acepta semilla ni garantiza un orden distinto al original.
- `public Personaje[] Repartir(int cantidad)`: primero calcula `Take(cantidad).ToArray()`, después ejecuta `RemoveRange(0, cantidad)` y solo entonces retorna el array. Para 0 ≤ cantidad ≤ Count devuelve y elimina ese prefijo. Para cantidad negativa `Take` no aporta elementos pero `RemoveRange` lanza `ArgumentOutOfRangeException`; si supera Count, Take arma un array con lo disponible pero RemoveRange lanza `ArgumentException`, sin devolverlo al llamador ni retirar elementos. No aplica límite de tres por sí mismo.
- `public void Devolver(Personaje[] personajes)`: `AddRange` agrega en el orden recibido al final. Null lanza `ArgumentNullException` desde la biblioteca; un array vacío no cambia nada. No comprueba que las cartas procedan de este mazo.
- `public int GetCantidad()`: devuelve Count actual sin mutar.

Fragmento completo del bucle de `Mezclar`:

```csharp
            int n = this.personajes.Count;
            while (n > 1)
            {
                n--;
                // Genera un índice aleatorio entre 0 y n (inclusive)
                int k = Random.Shared.Next(n + 1);

                // Intercambia los elementos usando tuplas de C#
                (this.personajes[k], this.personajes[n]) = (this.personajes[n], this.personajes[k]);
            }
```

El límite exclusivo de `Next(n + 1)` permite k = n; intercambiar una posición consigo misma es válido. Es una mezcla de Fisher–Yates. `Repartir` conserva este orden y no vuelve a mezclar:

```csharp
        public Personaje[] Repartir(int cantidad)
        {
            Personaje[] repartidos = this.personajes.Take(cantidad).ToArray();
            this.personajes.RemoveRange(0, cantidad);

            return repartidos;
        }
```

### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: copia del contenedor inicial. Se omite la firma y las llaves del constructor en este fragmento.

```csharp
            this.personajes = new List<Personaje>(personajes);
```

Devolver: anexado al final sin deduplicación.

```csharp
        public void Devolver(Personaje[] personajes)
        {
            this.personajes.AddRange(personajes);
        }
```

## Recorridos y ejemplos

Normal sin mezcla: reserva `[A,B,C,D]`, repartir 2 retorna `[A,B]` y deja `[C,D]`; devolver `[A,B]` deja `[C,D,A,B]`; Count es 4. Mezclar conserva las cuatro referencias pero su posición posterior no se predice.

Error: con dos cartas, repartir 3 lanza antes del retorno y deja Count 2. No hay reparto parcial entregado al llamador. Repartir 0 retorna vacío y conserva todo. Ejemplos contrastados por lectura, no ejecutados como comandos en esta página.

## Errores, límites y efectos observables

Solo altera la reserva en memoria. No asegura seis cartas al inicio de partida: esa guarda corresponde a Partida. No registra las cartas prestadas, y una devolución repetida duplica referencias. La aleatoriedad impide usar un orden específico como aserción de mezcla; debe comprobarse conservación de elementos.

## Relaciones y verificación

[Jugador](Jugador.md) recibe las cartas retiradas. `MazoTests.CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal` comprueba independencia de la lista, dos repartos de tres y devolución al final. `MezclarConservaTodasLasCartas` parametriza 0, 1 y 6 y exige cada referencia exactamente una vez. [Pruebas fuente](../../test/proyecto-integrador-final-tests/MazoTests.md). No ejercitan cantidades inválidas, null ni duplicados; las limitaciones anteriores provienen de lectura y semántica de List/LINQ. Véase el [plan de pruebas](../../../plan-de-pruebas.md); definir pruebas no acredita ejecución en este worktree.
