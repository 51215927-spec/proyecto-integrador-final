# Código del dominio

Las clases de esta página trabajan con objetos en memoria. El servidor HTTP comparte las reglas de `Combate` y los límites de `Ronda` y `Partida`, y gestiona su flujo en la base de datos.

## `Personaje.cs`

[Ver `src/servidor/Personaje.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Personaje.cs)

`TipoAtributo` identifica los seis puntajes. El objeto conserva nombre, dibujo y puntajes. El ID y la descripción pertenecen al catálogo.

- `Personaje(...)` guarda los datos y llama a `Validar` antes de entregar el objeto.
- `Validar` rechaza un nombre sin texto o un dibujo nulo. En los seis puntajes comprueba el intervalo 250–750, la suma 3000 y la existencia de al menos un valor ≥650 y otro ≤350.
- `GetNombre` y `GetAscii` devuelven los campos respectivos. El dibujo puede ser una cadena vacía.
- `GetAtributo` traduce el valor de la enumeración al campo correspondiente mediante un `switch` y rechaza un valor fuera de la enumeración.

## `Mazo.cs`

[Ver `src/servidor/Mazo.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Mazo.cs)

- `Mazo(...)` copia la lista recibida para que las operaciones sobre el mazo tengan su propia colección.
- `Mezclar` recorre desde el final e intercambia cada posición con una elegida al azar entre el inicio y esa posición. Es una mezcla de Fisher–Yates.
- `Repartir` toma las primeras `cantidad` cartas y elimina ese tramo. Si se piden más de las disponibles, `RemoveRange` rechaza el rango.
- `Devolver` agrega las cartas al final de la lista. `GetCantidad` devuelve su tamaño.

## `Jugador.cs`

[Ver `src/servidor/Jugador.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Jugador.cs)

- `Jugador(...)` valida el nombre y comienza con mano vacía y cero rondas ganadas.
- `RecibirCartas` comprueba el arreglo y agrega sus elementos a la mano. `QuitarCarta` quita una instancia de esa lista.
- `EntregarCartas` copia la mano, vacía la lista y devuelve el arreglo. `GetCartas` entrega una copia sin vaciarla.
- `SumarRondaGanada` incrementa el contador. `GetRondasGanadas` y `GetNombre` leen esos campos.
- `TieneCarta` consulta si la mano contiene la instancia de `Personaje`.

## `Combate.cs`

[Ver `src/servidor/Combate.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Combate.cs)

- `Combate(...)` comprueba que ambos jugadores existan y que sean instancias distintas. Conserva el elector y el rival en campos separados.
- `SetAtributo` exige un valor definido en `TipoAtributo` y permite registrarlo una vez.
- `SetCarta` valida jugador y carta, comprueba la pertenencia y disponibilidad con los métodos privados, retira la carta de la mano y la registra.
- `EstaCompleto` comprueba si hay atributo y una carta por participante.
- `Resolver` exige un combate completo y sin resultado previo. Obtiene el mismo atributo de las dos cartas. Si el puntaje del elector es estrictamente mayor, gana él. En los demás casos gana el rival.
- `GetGanador` y `GetAtributo` leen sus campos y arrojan una excepción si todavía faltan. `GetCarta` valida al participante y exige que su carta ya esté registrada.
- `GetParticipantes` crea un arreglo con elector primero y rival segundo.
- `ValidarParticipante` compara identidad de objeto con ambas plazas. `ValidarCartaDisponible` comprueba que el jugador no haya registrado otra carta y que la elegida esté en su mano. `RegistrarCarta` guarda en el campo del elector o del rival.

## `Ronda.cs`

[Ver `src/servidor/Ronda.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Ronda.cs)

- `Ronda(...)` valida la pareja y conserva qué jugador abre como mano.
- `AgregarCombate` rechaza valores nulos, una ronda ya definida o más de tres combates. Agrega el objeto al historial. Las consultas de marcador dependen de que cada combate agregado ya tenga ganador.
- `EstaDefinida` cuenta los combates ganados por cada participante y compara los resultados con `CombatesParaGanar`, que vale dos.
- `GetGanador` devuelve el jugador que alcanzó el umbral o arroja una excepción si falta cerrar la ronda.
- `GetQuienEligeProximo` alterna mano y rival según la paridad de los combates guardados. `GetCombates` devuelve una copia del historial.

## `Partida.cs`

[Ver `src/servidor/Partida.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/Partida.cs)

- `Partida(...)` guarda las referencias validadas de ambos jugadores y el mazo. Define `RondasParaGanar` en 15.
- `Jugar` llama a `Iniciar`, luego repite ronda y recuperación de cartas, comprueba la victoria y prepara la siguiente ronda con el jugador mano alternado.
- `Iniciar` exige seis cartas, crea la ronda 1 con J1 como mano y llama a `RepartirCartas`. Este último mezcla y entrega tres cartas a cada jugador.
- `JugarRonda` construye el próximo combate. La entrada de elecciones sigue pendiente en este bucle en memoria, por lo que el combate queda incompleto y se lanza una excepción. El servidor HTTP registra las elecciones en `TransicionesPartida.cs`.
- `RecogerCartas` reúne lo usado en los combates y lo que sigue en ambas manos, y devuelve todo al mazo.
- `EstaTerminada` compara ambos marcadores con 15. `DeterminarGanadorPartida` devuelve el jugador que alcanzó el umbral.

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
