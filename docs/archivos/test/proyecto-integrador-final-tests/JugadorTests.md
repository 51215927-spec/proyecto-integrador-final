# JugadorTests.cs

Fuente: [test/proyecto-integrador-final-tests/JugadorTests.cs](../../../../test/proyecto-integrador-final-tests/JugadorTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`, ensamblado `Proyecto-Integrador-Final.Tests`, destino `net8.0`. Namespace: `Proyecto_Integrador_Final.Tests`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Comprueba el estado inicial y la propiedad de las colecciones de la mano del modelo local. Dos Facts síncronos sin red ni SQLite.

## Dependencias

xUnit (`Fact`, `Theory`, `InlineData`, `Assert`) descubre casos y transforma una aserción incumplida en fallo. El proyecto referencia cliente, servidor y carga; `InternalsVisibleTo` de los proyectos permite probar tipos internos. Los miembros auxiliares sin atributo de prueba no se descubren como casos independientes. `Assert.Same` compara identidad; `Assert.Equal` compara los valores o secuencias; `Assert.Throws`/`ThrowsAsync` exigen excepción en la acción indicada. Una lambda pasada a Assert no captura silenciosamente otros fallos de preparación.

Relaciones usadas por este archivo: [CartasDePrueba](CartasDePrueba.md), [Jugador](../../src/servidor/Jugador.md), [Personaje](../../src/servidor/Personaje.md).

Guías: [plan de pruebas y ejecución de esta referencia](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004), [instalación](../../../instalacion-y-uso.md), [protocolo HTTP](../../../protocolo-de-partida.md).

## Estructura

Declaración: `public class JugadorTests`.

Las declaraciones anteriores conservan visibilidad, sellado, constructor primario y herencia/interfaces cuando existen. No hay tipos parciales en este archivo. Los records generan igualdad por valor, propiedades posicionales y soporte de `with`; las clases ordinarias no adquieren esa semántica. No se declaran operadores ni eventos propios.

## Estado y datos

Sin campos de fixture; cada prueba crea objetos nuevos y xUnit no comparte jugadores.

## Miembros y ejecución

En los casos de prueba, el runner suministra los parámetros de cada `InlineData`; no son entradas de usuario. `void` no devuelve resultado y `Task` solo permite esperar la terminación del caso, no devuelve una jugada. Cualquier aserción fallida o excepción no esperada detiene ese caso. El orden de los bloques de código siguientes es el del archivo; cada bloque es código real del miembro identificado, no pseudocódigo.

### `JugadorTests.IniciaConNombreManoVaciaYSinVictorias`

Construye Jugador("Ana"), comprueba nombre literal, mano vacía y cero rondas; llama dos veces SumarRondaGanada y exige 2. No acredita límite de quince ni validación de nombre: solo la inicialización y el incremento.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void IniciaConNombreManoVaciaYSinVictorias()
    {
        var jugador = new Jugador("Ana");

        Assert.Equal("Ana", jugador.GetNombre());
        Assert.Empty(jugador.GetCartas());
        Assert.Equal(0, jugador.GetRondasGanadas());

        jugador.SumarRondaGanada();
        jugador.SumarRondaGanada();

        Assert.Equal(2, jugador.GetRondasGanadas());
    }
```

### `JugadorTests.AcumulaCartasQuitaUnaYEntregaElResto`

Construye tres referencias distintas con CartasDePrueba; recibe las dos primeras y altera el array origen para comprobar que la mano no cambia. Agrega la tercera y compara el orden [primera,segunda,tercera]. Modifica el array de GetCartas y exige que la primera referencia interna siga igual. Quita segunda, compara [primera,tercera], entrega ese orden y exige vaciado; una segunda entrega debe ser vacía. No prueba duplicados, null ni igualdad por contenido.

Fragmento del miembro completo; incluye preparación, acción y aserciones o las ramas del auxiliar cuando corresponda:

```csharp
[Fact]
    public void AcumulaCartasQuitaUnaYEntregaElResto()
    {
        var jugador = new Jugador("Ana");
        var primera = CartasDePrueba.Crear();
        var segunda = CartasDePrueba.Crear();
        var tercera = CartasDePrueba.Crear();

        Personaje[] recibidas = [primera, segunda];
        jugador.RecibirCartas(recibidas);
        recibidas[0] = tercera;
        jugador.RecibirCartas([tercera]);
        Assert.Equal(new[] { primera, segunda, tercera }, jugador.GetCartas());

        jugador.GetCartas()[0] = tercera;
        Assert.Same(primera, jugador.GetCartas()[0]);

        jugador.QuitarCarta(segunda);
        Assert.Equal(new[] { primera, tercera }, jugador.GetCartas());
        Assert.Equal(new[] { primera, tercera }, jugador.EntregarCartas());
        Assert.Empty(jugador.GetCartas());
        Assert.Empty(jugador.EntregarCartas());
    }
```

## Recorridos y ejemplos

Normal: Ana recibe A y B, luego C, retira B y entrega [A,C]; la mano termina vacía.

Alternativo o de error: Modificar el array recibido originalmente o el devuelto por GetCartas no reemplaza las posiciones de la mano; entregar otra vez retorna vacío. No hay un caso explícito de constructor inválido en este archivo.

Estos recorridos describen los datos y resultados previstos por las aserciones; no son una nueva sesión de juego ni comandos ejecutados para redactar esta página.

## Errores, límites y efectos observables

Cada fallo de aserción invalida el caso completo; no implica por sí solo un defecto en otra rama, otro worktree o el entorno confirmado por el usuario. Las pruebas observan únicamente las propiedades señaladas por sus aserciones: no acreditan Windows, dos equipos físicos, seguridad de transporte ni todos los errores de sistema. Los recursos temporales, cancelaciones, excepciones esperadas y límites de espera de este archivo están descritos por miembro. Donde solo se usan objetos y texto en memoria no corresponde una transacción ni limpieza de archivos; donde hay SQLite/host/proceso, su preparación y liberación están indicadas en los recorridos concretos.

## Relaciones y verificación

La lectura se contrastó por miembro, incluidos auxiliares privados y funciones locales, contra el commit común del catálogo. Todos los bloques `csharp` se cotejan como fragmentos literales del archivo fuente, respetando sus atributos y datos de entrada. La [evidencia de ejecución de la versión documentada](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) registra comando, directorio y resultado de la suite; distinguir esa ejecución de la cobertura definida aquí. No se atribuye a estos tests una ejecución aislada adicional ni cobertura de escenarios que no contienen.
