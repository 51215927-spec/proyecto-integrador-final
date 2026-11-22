# Ronda.cs

Fuente: [src/servidor/Ronda.cs](../../../../src/servidor/Ronda.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](../../README.md#versión-documentada).

## Propósito y responsabilidad

Agrupa combates locales y declara ganador al alcanzar dos victorias. Alterna quién elige por cantidad de combates, empezando por mano. No consume cartas, no incrementa marcadores de jugadores ni ejecuta por sí misma un combate.

## Dependencias

[Jugador](Jugador.md) aporta identidad de mano y rival; [Combate](Combate.md) aporta ganadores ya resueltos. `List<Combate>` mantiene el orden y `ToArray` permite consultar una copia del contenedor. Véanse las [reglas](../../../reglas-del-juego.md).

## Estructura

`internal class Ronda` sin herencia explícita, interfaces, atributos ni partial. Todos los métodos son públicos; no hay propiedades, eventos, funciones locales ni operadores. `internal const int CombatesParaGanar = 2` establece el umbral en compilación.

## Estado y datos

Campos privados readonly `Jugador mano`, `Jugador rival` y `List<Combate> combates`. El constructor fija los participantes y crea lista vacía. La lista es mutable aunque no pueda reasignarse; sus elementos se comparten con el llamador. No mantiene contador cacheado: cada consulta calcula los triunfos recorriendo los combates.

## Miembros y ejecución

- `public Ronda(Jugador mano, Jugador rival)`: rechaza null de mano y rival en ese orden con `ArgumentNullException`. Si ambas referencias son iguales, `ArgumentException`. Guarda ambas y crea lista; no importa que sus nombres coincidan.
- `public void AgregarCombate(Combate combate)`: null falla primero. Después llama a EstaDefinida; si ya hay dos victorias de un lado, lanza `InvalidOperationException`. Luego rechaza Count ≥ 3 con otra `InvalidOperationException`; si pasa agrega al final. No inspecciona directamente que el combate nuevo esté resuelto ni que sus participantes sean los de esta ronda. Si los anteriores incluyen uno sin resolver, EstaDefinida ya puede lanzar antes del agregado.
- `public bool EstaDefinida()`: inicia contadores locales en cero. Recorre todos los elementos; si GetGanador es la referencia mano suma a mano; cualquier otra referencia suma a rival. Retorna true si algún contador ≥ 2. Un combate no resuelto propaga su excepción. Lista vacía retorna false. No retorna anticipadamente al llegar a dos: sigue recorriendo.
- `public Jugador GetGanador()`: exige EstaDefinida o lanza `InvalidOperationException`. Recuenta victorias de mano; con ≥2 retorna mano y de lo contrario rival. No incrementa rondasGanadas ni cambia la lista.
- `public Jugador GetQuienEligeProximo()`: si EstaDefinida, `InvalidOperationException`; si no, Count par retorna mano, impar rival. Alterna aunque un mismo participante haya ganado dos combates consecutivos; no usa el ganador anterior para elegir.
- `public Combate[] GetCombates()`: copia a array conservando orden; cambiar una posición no modifica la lista. Los objetos Combate no se clonan.

El cuerpo de EstaDefinida muestra tanto la ausencia de validación de identidad rival como la terminación por recorrido completo:

```csharp
            int victoriasMano = 0;
            int victoriasRival = 0;

            foreach (Combate combate in this.combates)
            {
                if (ReferenceEquals(combate.GetGanador(), this.mano))
                    victoriasMano++;
                else
                    victoriasRival++;
            }

            return victoriasMano >= CombatesParaGanar || victoriasRival >= CombatesParaGanar;
```

La selección siguiente, después de la guarda de ronda definida, usa la paridad:

```csharp
            // La elección empieza con el jugador mano y alterna después de cada combate.
            return this.combates.Count % 2 == 0
                ? this.mano
                : this.rival;
```

### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: referencias distintas y lista vacía.

```csharp
        public Ronda(Jugador mano, Jugador rival)
        {
            ArgumentNullException.ThrowIfNull(mano);
            ArgumentNullException.ThrowIfNull(rival);

            if (ReferenceEquals(mano, rival))
                throw new ArgumentException(
                    "Los jugadores deben ser distintos."
                );

            this.mano = mano;
            this.rival = rival;
            this.combates = new List<Combate>();
        }
```

AgregarCombate: orden de guardas y único efecto de agregado.

```csharp
        public void AgregarCombate(Combate combate)
        {
            ArgumentNullException.ThrowIfNull(combate);

            if (this.EstaDefinida())
                throw new InvalidOperationException(
                    "La ronda ya está definida."
                );

            if (this.combates.Count >= 3)
                throw new InvalidOperationException(
                    "La ronda no puede tener más de tres combates."
                );

            this.combates.Add(combate);
        }
```

GetGanador: verifica definición antes de contar triunfos de mano.

```csharp
        public Jugador GetGanador()
        {
            if (!this.EstaDefinida())
                throw new InvalidOperationException(
                    "La ronda todavía no tiene ganador."
                );

            int victoriasMano = 0;

            foreach (Combate combate in this.combates)
            {
                if (ReferenceEquals(combate.GetGanador(), this.mano))
                    victoriasMano++;
            }

            return victoriasMano >= CombatesParaGanar
                ? this.mano
                : this.rival;
        }
```

## Recorridos y ejemplos

Normal: ronda con mano Ana y rival Bruno. Sin combates abre Ana. Tras un triunfo de Ana abre Bruno. Si Ana gana el segundo, EstaDefinida es true y GetGanador retorna Ana; pedir siguiente elector o agregar un tercero falla porque la ronda terminó.

Alternativo: primero gana Ana y segundo Bruno; Count 2 es par, aún no hay vencedor y abre Ana. El tercer combate da la segunda victoria a alguno y define la ronda. Error adicional: agregar un primer combate incompleto sí puede realizarse porque la lista anterior está vacía; la próxima consulta de definición propaga «El combate todavía no fue resuelto.». Son consecuencias leídas del código; no se presenta ese último caso como prueba ejecutada.

## Errores, límites y efectos observables

El límite tres es defensivo: con dos participantes válidos y tres resultados resueltos, alguno ya tiene dos victorias. El tipo confía en el llamador para participantes y resolución; un ganador ajeno se contabiliza como victoria rival. No hay reversión, persistencia, aleatoriedad, asincronía ni sincronización; solo cambia la lista al agregar.

## Relaciones y verificación

[Partida](Partida.md) usa el ganador para sumar una ronda al Jugador. [RondaTests](../../test/proyecto-integrador-final-tests/RondaTests.md) prepara combates resueltos, verifica comienzo, alternancia, cierre 2–0 para ambos lados, resolución 2–1 y copia defensiva del array. No acredita validación de participantes de cada combate ni rechazo anticipado de combates sin resolver: esas validaciones no existen aquí. La ejecución global se registra en el [plan de pruebas](../../../plan-de-pruebas.md).
