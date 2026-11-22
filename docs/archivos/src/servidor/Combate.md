# Combate.cs

Fuente: [src/servidor/Combate.cs](../../../../src/servidor/Combate.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](../../README.md#versión-documentada).

## Propósito y responsabilidad

Consume una carta de cada participante, fija un atributo una sola vez y resuelve una comparación. Modela un combate local; no incorpora sesiones HTTP, persistencia ni revisiones de solicitudes. La ronda registra su ganador, pero este objeto no incrementa el marcador.

## Dependencias

[Jugador](Jugador.md) conserva la mano y permite retirar referencias; [Personaje y TipoAtributo](Personaje.md) proporcionan los puntajes. `Enum.IsDefined` rechaza valores de enum sin nombre; `ReferenceEquals` distingue participantes por identidad, no por nombre. Véanse [reglas](../../../reglas-del-juego.md) y la coordinación del [dominio Partida](Partida.md).

## Estructura

`internal class Combate`, sin base explícita, interfaces, atributos ni partial. No hay propiedades, eventos, operadores, funciones locales ni sobrecargas. Métodos públicos salvo los tres ayudantes de validación y registro.

## Estado y datos

| Campo privado | Ciclo de vida |
|---|---|
| `readonly Jugador jugadorQueElige`, `jugadorRival` | Se asignan al construir y no cambian. Deben ser referencias distintas no nulas. |
| `TipoAtributo? atributoElegido` | Empieza null, una elección válida lo fija para siempre. `?` permite distinguir Fuerza (0) de ausencia. |
| `Personaje? cartaQueElige`, `cartaRival` | Empiezan null; cada participante registra una carta que se retira de su mano. |
| `Jugador? ganador` | Null hasta Resolver; luego referencia del vencedor. |

No hay bloqueos ni reversión de cartas consumidas. Los signos `!` en lecturas posteriores silencian advertencias de nulabilidad después de comprobar completitud; no son validaciones nuevas.

## Miembros y ejecución

- `public Combate(Jugador quienElige, Jugador rival)`: comprueba null de ambos en orden mediante `ArgumentNullException`, luego igualdad por referencia (`ArgumentException`). Solo después guarda participantes. Dos jugadores con igual nombre pero distinta instancia se aceptan.
- `public void SetAtributo(TipoAtributo tipo)`: si ya existe atributo, lanza `InvalidOperationException` antes de inspeccionar tipo. Si es primera elección y `Enum.IsDefined(tipo)` es false, lanza `ArgumentOutOfRangeException`. En otro caso asigna. No exige que las cartas ya estén presentes.
- `public void SetCarta(Jugador jugador, Personaje personaje)`: rechaza primero jugador null y después personaje null; llama a ValidarParticipante y ValidarCartaDisponible. Solo si todas pasan, retira la carta mediante QuitarCarta y registra la referencia. No retorna resultado y no exige elegir atributo primero.
- `private void ValidarParticipante(Jugador jugador)`: si no es ninguna de las dos referencias, lanza `ArgumentException` con parámetro jugador; si coincide, no modifica nada.
- `private void ValidarCartaDisponible(Jugador jugador, Personaje personaje)`: selecciona el campo de carta según sea el elector. Si ya tiene carta, lanza `InvalidOperationException`. Si aún no tiene pero `TieneCarta` es false, lanza `ArgumentException` con parámetro personaje. La precedencia evita consumir una segunda carta incluso si está en su mano.
- `private void RegistrarCarta(Jugador jugador, Personaje personaje)`: si es elector asigna cartaQueElige y retorna inmediatamente; de lo contrario asigna cartaRival. Se llama después de validar que el participante pertenece al combate.
- `public bool EstaCompleto()`: true únicamente con atributo y ambas cartas; no exige que ya se resolviera. Sigue siendo true después de Resolver.
- `public Jugador Resolver()`: comprueba completitud y luego que ganador siga null; cada fallo lanza `InvalidOperationException`. Lee ambos valores del atributo; elector gana solo si su valor es estrictamente mayor. Menor o igual da ganador al rival. Guarda y retorna vencedor. No suma rondas ni devuelve cartas.
- `public Jugador GetGanador()`: retorna ganador; null produce `InvalidOperationException` sin cambiar estado.
- `public TipoAtributo GetAtributo()`: retorna atributo o lanza `InvalidOperationException` si aún no fue fijado, aunque ya existan cartas.
- `public Personaje GetCarta(Jugador jugador)`: null produce `ArgumentNullException`. Luego exige combate completo (`InvalidOperationException`), sin exigir resuelto. Devuelve la carta de la referencia coincidente; un ajeno produce `ArgumentException`. Por ese orden un ajeno en combate incompleto recibe primero el error de completitud.
- `public Jugador[] GetParticipantes()`: array nuevo con elector y rival, en ese orden. Reemplazar posiciones no cambia los campos; los jugadores siguen siendo objetos compartidos.

El orden de las validaciones de SetCarta evita consumo en los rechazos:

```csharp
            ArgumentNullException.ThrowIfNull(jugador);
            ArgumentNullException.ThrowIfNull(personaje);

            ValidarParticipante(jugador);
            ValidarCartaDisponible(jugador, personaje);

            jugador.QuitarCarta(personaje);
            RegistrarCarta(jugador, personaje);
```

En Resolver, tras sus guardas de completitud y resolución única, el código real compara:

```csharp
            int valorElector = cartaQueElige!.GetAtributo(atributoElegido!.Value);
            int valorRival = cartaRival!.GetAtributo(atributoElegido.Value);

            // Si los valores empatan, gana el rival de quien eligió el atributo.
            ganador = valorElector > valorRival
                ? jugadorQueElige
                : jugadorRival;

            return ganador;
```

### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: guardas de referencias y asignaciones.

```csharp
        public Combate(Jugador quienElige, Jugador rival)
        {
            ArgumentNullException.ThrowIfNull(quienElige);
            ArgumentNullException.ThrowIfNull(rival);

            if (ReferenceEquals(quienElige, rival))
                throw new ArgumentException(
                    "Los participantes deben ser distintos."
                );

            jugadorQueElige = quienElige;
            jugadorRival = rival;
        }
```

SetAtributo: prioridad de repetición frente a enum inválido.

```csharp
        public void SetAtributo(TipoAtributo tipo)
        {
            if (atributoElegido.HasValue)
                throw new InvalidOperationException(
                    "El atributo ya fue elegido."
                );

            if (!Enum.IsDefined(tipo))
                throw new ArgumentOutOfRangeException(nameof(tipo));

            atributoElegido = tipo;
        }
```

ValidarParticipante: identidad exacta de alguna plaza.

```csharp
        private void ValidarParticipante(Jugador jugador)
        {
            if (!ReferenceEquals(jugador, jugadorQueElige) &&
                !ReferenceEquals(jugador, jugadorRival))
            {
                throw new ArgumentException(
                    "El jugador no participa en este combate.",
                    nameof(jugador)
                );
            }
        }
```

ValidarCartaDisponible: segunda carta se rechaza antes de consultar disponibilidad.

```csharp
        private void ValidarCartaDisponible(Jugador jugador, Personaje personaje)
        {
            bool esElector = ReferenceEquals(jugador, jugadorQueElige);
            Personaje? cartaRegistrada = esElector ? cartaQueElige : cartaRival;

            if (cartaRegistrada != null)
                throw new InvalidOperationException(
                    "El jugador ya registró su carta."
                );

            if (!jugador.TieneCarta(personaje))
                throw new ArgumentException(
                    "La carta no se encuentra en la mano del jugador.",
                    nameof(personaje)
                );
        }
```

RegistrarCarta: asignación por participante con retorno temprano.

```csharp
        private void RegistrarCarta(Jugador jugador, Personaje personaje)
        {
            if (ReferenceEquals(jugador, jugadorQueElige))
            {
                cartaQueElige = personaje;
                return;
            }

            cartaRival = personaje;
        }
```

GetCarta: primero completitud y después identidad.

```csharp
        public Personaje GetCarta(Jugador jugador)
        {
            ArgumentNullException.ThrowIfNull(jugador);

            if (!EstaCompleto())
                throw new InvalidOperationException(
                    "El combate todavía no está completo."
                );

            if (ReferenceEquals(jugador, jugadorQueElige))
                return cartaQueElige!;

            if (ReferenceEquals(jugador, jugadorRival))
                return cartaRival!;

            throw new ArgumentException(
                "El jugador no participa en este combate.",
                nameof(jugador)
            );
        }
```

## Recorridos y ejemplos

Normal: Ana tiene A con fuerza 750 y Bruno B con fuerza 650. Construir Combate(Ana,Bruno), fijar Fuerza y registrar A y B deja ambas manos sin esas cartas; EstaCompleto devuelve true. Resolver retorna Ana, GetGanador retorna la misma referencia y los contadores de rondas siguen iguales.

Alternativo: con fuerza 650 en ambas cartas gana Bruno porque es rival del elector. Registrar una carta de un tercer Jugador llamado Ana falla por identidad sin retirarla. Registrar dos cartas del elector falla antes de retirar la segunda. Resolver sin carta rival falla sin ganador; repetir Resolver después de un éxito falla y conserva el ganador anterior. Estos recorridos se contrastaron por lectura; la ejecución de suite se registra aparte.

## Errores, límites y efectos observables

Las cartas se retiran al registrarlas, no al resolver; un combate abandonado no las restaura por sí mismo. No limita combates por ronda ni comprueba orden de turnos de una sesión. No sincroniza accesos concurrentes; no hay transacción que cubra retirar y registrar. Una carta repetida en una mano se retira solo una vez. Las restricciones de puntajes pertenecen a Personaje.

## Relaciones y verificación

[Ronda](Ronda.md) cuenta vencedores; [Partida](Partida.md) recoge las referencias de cartas por participante. [CombateTests](../../test/proyecto-integrador-final-tests/CombateTests.md) define guardas, consulta prematura, una carta por jugador, completitud, identidad, copia del array, comparación y empate; algunas aserciones solo exigen que exista una excepción, no su tipo ni mensaje. El [plan de pruebas](../../../plan-de-pruebas.md) distingue evidencia histórica de ejecución actual. Esta explicación no acredita ejecución concurrente ni de la API a partir de este modelo.
