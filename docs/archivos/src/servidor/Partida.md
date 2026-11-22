# Partida.cs

Fuente: [src/servidor/Partida.cs](../../../../src/servidor/Partida.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](../../README.md#versión-documentada).

## Propósito y responsabilidad

Es el coordinador local del modelo de clases: prevé repartir, jugar rondas, recoger cartas y terminar en quince victorias. En la versión documentada **no puede completar una partida**: JugarRonda crea un combate pero no fija atributo ni cartas y lanza al comprobar completitud. El servidor de red usa [RepositorioJuego](Api/RepositorioJuego.md) y [TransicionesPartida](Api/TransicionesPartida.md), no este bucle. Este límite se describe sin cambiar código ni interpretar el comentario de actividad 6 como funcionalidad disponible.

## Dependencias

[Jugador](Jugador.md) mantiene manos y marcador, [Mazo](Mazo.md) mezcla y reparte, [Ronda](Ronda.md) alterna electores y cuenta victorias, [Combate](Combate.md) compara cartas. `List<Personaje>` reúne cartas al recoger. No depende de HttpClient, consola ni SQLite. Véanse [arquitectura](../../../arquitectura.md) y [reglas](../../../reglas-del-juego.md).

## Estructura

`internal class Partida`, sin herencia explícita, interfaces, atributos ni partial. Constructor y Jugar públicos; los seis ayudantes restantes son privados. No declara propiedades, eventos, sobrecargas, funciones locales ni operadores. `internal const int RondasParaGanar = 15` fija el umbral.

## Estado y datos

Campos privados readonly `Jugador j1`, `Jugador j2`, `Mazo mazo` se asignan al construir. `Ronda? rondaActual` empieza null; `int numeroRonda` empieza 0. Iniciar fija número 1 y ronda con j1 como mano. El diseño posterior alternaría mano por paridad, pero no se alcanza en el flujo actual con seis o más cartas y jugadores distintos. No resetea los marcadores previos de los jugadores ni sus manos al comenzar.

## Miembros y ejecución

### `public Partida(Jugador j1, Jugador j2, Mazo mazo)`

Rechaza null en j1, j2 y mazo, en ese orden, con `ArgumentNullException`; después guarda referencias y estado inicial. No rechaza j1 y j2 iguales aquí: con mazo suficiente fallará al construir Ronda en Iniciar. No copia los objetos; pueden ser compartidos externamente.

### `public Jugador Jugar()`

Llama a Iniciar antes de entrar a `while (true)`. El orden previsto por iteración es JugarRonda, RecogerCartas y EstaTerminada. Si terminó retorna DeterminarGanadorPartida; si no incrementa numeroRonda, elige j1 para número impar o j2 para par, obtiene el otro rival por ReferenceEquals, crea nueva Ronda y reparte otra vez. El bucle no tiene cancelación ni máximo de rondas; terminaría por marcador. En esta versión la primera llamada a JugarRonda lanza, de modo que el retorno exitoso no se produce por la API pública normal.

### `private void Iniciar()`

Si GetCantidad < 6, lanza `InvalidOperationException` con «No se puede jugar porque no hay suficientes cartas.» **antes** de cambiar número, ronda, mazo o manos. Con cantidad suficiente fija número 1, crea Ronda(j1,j2) y llama RepartirCartas. Si Ronda rechaza referencias iguales, número ya es 1 pero no hay reparto.

### `private void RepartirCartas()`

Mezcla todo el mazo, retira tres para j1 y después tres para j2; recién entonces agrega ambos arrays a las manos. No limpia cartas previas. La aleatoriedad pertenece al Mazo y no puede configurarse aquí. No hay transacción ni rollback si alguna operación excepcional falla a mitad.

### `private void JugarRonda()`

Mientras la ronda no esté definida obtiene elector, determina rival como el otro jugador y construye Combate. El comentario anuncia selección futura, pero no existe ninguna llamada a SetAtributo ni SetCarta. Por ello EstaCompleto es false y lanza «El combate no se completó.». Las líneas posteriores Resolver y AgregarCombate, así como obtener ganador y sumar una ronda al final del bucle, expresan el flujo previsto pero no se alcanzan con el estado que crea este método.

Fragmento real de JugarRonda que demuestra el límite:

```csharp
                Combate combate = new Combate(quienElige, rival);

                // La selección del atributo y de las cartas se completará
                // mediante la comunicación con los clientes en la actividad 6.

                if (!combate.EstaCompleto())
                    throw new InvalidOperationException(
                        "El combate no se completó."
                    );

                combate.Resolver();
                this.rondaActual.AgregarCombate(combate);
```

### `private void RecogerCartas()`

Prevé recorrer combates en orden y, para cada uno, sus dos participantes, agregando GetCarta de cada jugador a una lista. Luego EntregarCartas vacía la mano de j1 y agrega sus sobrantes; hace lo mismo con j2; finalmente devuelve todo al Mazo. No mezcla durante recogida. Los bucles terminan por tamaño de las colecciones. Requiere ronda no nula y combates completos; la supresión `!` no evita una excepción si se incumple esa precondición. En el flujo actual no llega a ejecutarse tras el error de JugarRonda.

### `private bool EstaTerminada()`

Retorna si j1 o j2 tiene al menos quince rondas ganadas. Cortocircuita el OR: con j1 ≥15 no consulta j2. No modifica estado.

### `private Jugador DeterminarGanadorPartida()`

Devuelve j1 si su contador ≥15; si no, devuelve j2 sin comprobar su contador. Su precondición prevista es haber obtenido true en EstaTerminada. Si ambos tuvieran quince, priorizaría j1. No se expone públicamente.

La guarda previa a repartir de Iniciar es:

```csharp
            if (this.mazo.GetCantidad() < 6)
                throw new InvalidOperationException(
                    "No se puede jugar porque no hay suficientes cartas."
                );
```

### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: referencias y estado inicial, sin validar igualdad de jugadores.

```csharp
        public Partida(Jugador j1, Jugador j2, Mazo mazo)
        {
            ArgumentNullException.ThrowIfNull(j1);
            ArgumentNullException.ThrowIfNull(j2);
            ArgumentNullException.ThrowIfNull(mazo);

            this.j1 = j1;
            this.j2 = j2;
            this.mazo = mazo;
            this.rondaActual = null;
            this.numeroRonda = 0;
        }
```

Jugar: secuencia prevista y punto de salida por marcador; el fallo de JugarRonda impide recorrerla por completo.

```csharp
        public Jugador Jugar()
        {
            this.Iniciar();

            while (true)
            {
                this.JugarRonda();
                this.RecogerCartas();

                if (this.EstaTerminada())
                    return this.DeterminarGanadorPartida();

                this.numeroRonda++;

                Jugador mano = this.numeroRonda % 2 == 1
                    ? this.j1
                    : this.j2;

                Jugador rival = ReferenceEquals(mano, this.j1)
                    ? this.j2
                    : this.j1;

                this.rondaActual = new Ronda(mano, rival);
                this.RepartirCartas();
            }
        }
```

Iniciar: insuficiencia se detecta antes de modificar estado.

```csharp
        private void Iniciar()
        {
            if (this.mazo.GetCantidad() < 6)
                throw new InvalidOperationException(
                    "No se puede jugar porque no hay suficientes cartas."
                );

            this.numeroRonda = 1;
            this.rondaActual = new Ronda(this.j1, this.j2);

            this.RepartirCartas();
        }
```

RepartirCartas: extrae ambos grupos antes de entregarlos.

```csharp
        private void RepartirCartas()
        {
            this.mazo.Mezclar();

            Personaje[] cartasJ1 = this.mazo.Repartir(3);
            Personaje[] cartasJ2 = this.mazo.Repartir(3);

            this.j1.RecibirCartas(cartasJ1);
            this.j2.RecibirCartas(cartasJ2);
        }
```

RecogerCartas: recorrido de jugadas y vaciado de sobrantes, no alcanzado en el flujo público actual.

```csharp
        private void RecogerCartas()
        {
            List<Personaje> cartas = new List<Personaje>();

            foreach (Combate combate in this.rondaActual!.GetCombates())
            {
                foreach (Jugador jugador in combate.GetParticipantes())
                    cartas.Add(combate.GetCarta(jugador));
            }

            cartas.AddRange(this.j1.EntregarCartas());
            cartas.AddRange(this.j2.EntregarCartas());

            this.mazo.Devolver(cartas.ToArray());
        }
```

EstaTerminada: umbral con OR de cortocircuito.

```csharp
        private bool EstaTerminada()
        {
            return this.j1.GetRondasGanadas() >= RondasParaGanar
                || this.j2.GetRondasGanadas() >= RondasParaGanar;
        }
```

DeterminarGanadorPartida: prioridad de j1 bajo su precondición.

```csharp
        private Jugador DeterminarGanadorPartida()
        {
            return this.j1.GetRondasGanadas() >= RondasParaGanar
                ? this.j1
                : this.j2;
        }
```

## Recorridos y ejemplos

Recorrido de entrada válida hasta el límite real: jugadores distintos con manos vacías y seis cartas válidas. Jugar establece ronda 1, mezcla, entrega tres a cada jugador y deja mazo vacío; luego lanza «El combate no se completó.». Las manos conservan sus tres cartas porque no se registró ninguna ni se alcanzó RecogerCartas; los marcadores no cambian. No se ofrece un ejemplo de victoria local, porque ese resultado no está implementado en este coordinador.

Error temprano: con cinco cartas, Jugar lanza por insuficiencia antes de repartir; mazo conserva cinco y manos vacías. Con cero cartas sucede lo mismo. Estos dos tamaños están definidos en PartidaTests; el recorrido de seis cartas se contrastó por lectura y no se confunde con ensayo de partida REST.

## Errores, límites y efectos observables

No es seguro reintentar Jugar suponiendo que hubo rollback: el error posterior al reparto deja cartas en manos y reduce el mazo. No valida manos iniciales vacías ni marcadores cero. No hay persistencia, sesiones, revisión idempotente, entrada de usuario ni cancelación; esas responsabilidades corresponden a la implementación de red separada.

## Relaciones y verificación

[PartidaTests](../../test/proyecto-integrador-final-tests/PartidaTests.md) prueba exclusivamente mazos de 0 y 5, texto exacto del error y conservación de mazo/manos. No demuestra victoria de este coordinador ni ejecución de sus ayudantes posteriores al fallo. Las pruebas de servidor de red verifican otro coordinador. Véase el [plan de pruebas](../../../plan-de-pruebas.md) para resultados de ejecución, con rama y condiciones.
