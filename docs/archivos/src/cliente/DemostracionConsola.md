# DemostracionConsola.cs

Fuente: [src/cliente/DemostracionConsola.cs](../../../../src/cliente/DemostracionConsola.cs). Proyecto: `src/cliente/cliente.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Cliente`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Recorre pantallas de una partida simulada local con Lucía y Mateo, o un cierre simulado. Es un auxiliar interno utilizado por pruebas; el Program actual inicia REST y no ofrece este recorrido como modo del ejecutable. No prueba conectividad ni el estado persistido del servidor.

## Dependencias

[InterfazConsola](InterfazConsola.md) dibuja y recoge elecciones; [VistasConsola](VistasConsola.md) lleva cartas, mesa y resultados. TextReader/TextWriter coordinan las pausas en modo interactivo. Véase [diseño de interfaz](../../../diseno-de-interfaz.md).

## Estructura y datos

`internal sealed class DemostracionConsola(InterfazConsola interfaz, TextReader entrada, TextWriter salida, bool interactiva)`. El constructor primario captura esos colaboradores; no valida null ni dispone sus flujos. No hay interfaces, eventos, partial ni campos explícitos. Recorrer crea estado local por llamada: Lucía y Mateo con 14 rondas cada uno, ronda 29, combate 1, victorias de combate cero; tres cartas propias y tres rivales. Los puntajes son valores de presentación, sin pasar por Personaje.Validar.

## Miembros y ejecución

`public void Recorrer(string? cierre)`: crea mano Lucía (Lysander ID1, Reched ID2, Vigía ID3), mano rival (Centinela ID4, Oráculo ID5, Lancero ID6); llama a preparación, espera inicial e inicio de ronda, pausando tras cada pantalla. Si cierre no null, delega MostrarCierreSimulado y retorna sin jugar.

Con cierre null, repite combates: abre Lucía en combate impar. Si responde, muestra espera rival y pausa. Pide carta propia; al abrir pide atributo, al responder usa Inteligencia fija. La carta rival se toma por índice combate−1. Muestra envío en curso; en el primer combate además confirmación pendiente; después recepción confirmada, cada una seguida de Continuar. Compara los dos valores del atributo: gana Lucía si es mayor o, empatando, si ella no abrió (pierde el elector). Retira su carta y crea copia de mesa incrementando contador del ganador. Continúa mientras ninguno llegue a dos victorias; muestra duelo con próximo elector alternado cuando continúa. Pausa tras resultado y, si sigue, incrementa combate. Con dos victorias el ganador recibe 15 rondas en la mesa; muestra cierre de ronda sin próximo mano y cierre de partida. Al final interactivo hace un ReadLine adicional cuyo null no lanza explícitamente.

Fragmento de comparación y transición de mesa de Recorrer:

```csharp
            int valorPropio = propia.Atributos[atributo];
            int valorRival = contraria.Atributos[atributo];
            ganaLucia = valorPropio > valorRival || (valorPropio == valorRival && !abre);
            mano.Remove(propia);
            mesa = ganaLucia
                ? mesa with { CombatesJ1 = mesa.CombatesJ1 + 1 }
                : mesa with { CombatesJ2 = mesa.CombatesJ2 + 1 };

            bool rondaContinua = mesa.CombatesJ1 < 2 && mesa.CombatesJ2 < 2;
```

`private void MostrarCierreSimulado(MesaVisible mesa, string cierre)`: si cierre="pausa", primero muestra pausa confirmada por pérdida de sesión, pausa, luego pérdida no confirmada y pausa. Después también ejecuta el switch de cierre: abandono/cancelacion generan final confirmado sin victoria; cualquier otro valor (incluida pausa) muestra salida local guardada, no confirmada. No cambia la mesa ni llama al servidor.

```csharp
        (string causa, string efecto, bool confirmada) = cierre switch
        {
            "abandono" => ("Mateo abandonó la partida.", "La partida terminó sin sumar una victoria.", true),
            "cancelacion" => ("El operador canceló la partida.", "La partida terminó sin sumar una victoria.", true),
            _ => ("Se cerró el cliente.", "La partida queda guardada.", false)
        };
        interfaz.MostrarInterrupcion(mesa.Local, causa, efecto, confirmada);
```

`private void Continuar()`: no interactiva retorna de inmediato. Interactiva imprime `Pulsá Enter para avanzar.`, lee una línea y lanza EndOfStreamException si null. Cualquier línea no null avanza, sin comprobar que sea vacía.

`private static CartaVisible Carta(int id, string nombre, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)`: retorna CartaVisible con diccionario de las seis claves españolas; no normaliza, valida ni hace copias posteriores. Energía lleva tilde en la clave aunque parámetro se llame energia.

## Recorridos y ejemplos

Normal: elegir Lysander y Fuerza en combate 1 compara 750 con Centinela 750; gana Mateo porque Lucía eligió atributo. En combate 2 Lucía puede responder con Vigía: Inteligencia 500 frente a Oráculo 750 vuelve a dar victoria a Mateo; llega a dos combates y 15 rondas, cierra sin combate 3. Este ejemplo es deducción de datos y decisiones del auxiliar.

Alternativo: cierre="pausa" muestra primero ambas suspensiones y luego salida local; cierre="abandono" no juega ni suma victoria. EOF en una pausa interactiva lanza y no continúa el recorrido. En modo no interactivo se omiten pausas pero PedirCarta/PedirAtributo aún consumen las líneas del lector de la interfaz.

## Errores, límites y efectos observables

La simulación termina en dos o tres combates porque cada combate otorga una victoria. No permite partida completa desde ronda 1, reconexión real, persistencia ni cambios de nombre. Depende de opciones válidas de la interfaz; mano vacía o atributos inexistentes no tienen recuperación propia. No dispone recursos ajenos ni captura excepciones de E/S.

## Fragmentos complementarios por flujo

Los bloques siguientes corresponden a los miembros indicados y sustentan las condiciones y efectos explicados en «Miembros y ejecución». Los rangos parciales omiten únicamente el contexto descrito fuera del bloque.

### Recorrer: cierre temprano y elecciones

Antes del bucle se ofrecen pantallas iniciales; cierre no null interrumpe el juego. La carta rival se indexa por combate y al responder el atributo está fijado a Inteligencia.

```csharp
        if (cierre is not null)
        {
            MostrarCierreSimulado(mesa, cierre);
            return;
        }

        bool ganaLucia;
        do
        {
            bool abre = mesa.Combate % 2 == 1;
            if (!abre)
            {
                interfaz.MostrarEsperaRival(mesa, rival, mano);
                Continuar();
            }

            CartaVisible propia = interfaz.PedirCarta(mesa, mano, abre);
            string atributo = abre ? interfaz.PedirAtributo(mesa: mesa) : "Inteligencia";
            CartaVisible contraria = manoRival[mesa.Combate - 1];

            interfaz.MostrarEnvio(mesa, EstadoEnvio.Enviando);
            Continuar();
            if (mesa.Combate == 1)
            {
                interfaz.MostrarEnvio(mesa, EstadoEnvio.Pendiente);
                Continuar();
            }
            interfaz.MostrarEnvio(mesa, EstadoEnvio.Recibido);
            Continuar();
```

### Recorrer: fin de ronda y marcador

La salida del bucle ocurre con dos victorias; solo entonces cambia el marcador de rondas a 15. El ReadLine final interactivo no lanza explícitamente por null.

```csharp
            bool rondaContinua = mesa.CombatesJ1 < 2 && mesa.CombatesJ2 < 2;
            interfaz.MostrarCombate(mesa, new(atributo,
                new(local, propia.Nombre, valorPropio), new(rival, contraria.Nombre, valorRival),
                ganaLucia ? local : rival, mesa.CombatesJ1, mesa.CombatesJ2,
                rondaContinua ? (abre ? rival : local) : null, ganaLucia));
            Continuar();
            if (!rondaContinua)
                break;

            mesa = mesa with { Combate = mesa.Combate + 1 };
        } while (true);

        mesa = ganaLucia ? mesa with { J1 = mesa.J1 with { RondasGanadas = 15 } }
            : mesa with { J2 = mesa.J2 with { RondasGanadas = 15 } };
        string ganador = ganaLucia ? local : rival;
        interfaz.MostrarCierreRonda(mesa, ganador, proximoMano: null);
        Continuar();
        interfaz.MostrarCierrePartida(local, ganador, mesa.J1, mesa.J2);
        if (interactiva)
            entrada.ReadLine();
    }
```

### MostrarCierreSimulado: pausa y Continuar

La variante pausa muestra dos estados de suspensión antes del switch de interrupción ya documentado.

```csharp
    private void MostrarCierreSimulado(MesaVisible mesa, string cierre)
    {
        if (cierre == "pausa")
        {
            interfaz.MostrarSuspension(mesa, "Mateo perdió la sesión. Esperando que se reincorpore.", confirmada: true);
            Continuar();
            interfaz.MostrarSuspension(mesa, "Podés reintentar o salir.", confirmada: false);
            Continuar();
        }
```

### Continuar y Carta

La pausa no interactiva retorna sin leer. La fábrica crea mapa con las seis claves y no aplica validación de dominio.

```csharp
    private void Continuar()
    {
        if (!interactiva)
            return;

        salida.WriteLine("Pulsá Enter para avanzar.");
        if (entrada.ReadLine() is null)
            throw new EndOfStreamException();
    }

    private static CartaVisible Carta(int id, string nombre, int fuerza, int velocidad,
        int resistencia, int inteligencia, int energia, int habilidad) =>
        new(id, nombre, new Dictionary<string, int>
        {
            ["Fuerza"] = fuerza, ["Velocidad"] = velocidad, ["Resistencia"] = resistencia,
            ["Inteligencia"] = inteligencia, ["Energía"] = energia, ["Habilidad"] = habilidad
        });
```

## Relaciones y verificación

[ClienteConsolaContratoTests](../../test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.md) conserva pruebas directas de victoria y cierres simulados. No son pruebas del arranque REST ni de transporte real. Las comprobaciones citadas son escenarios definidos en el código de pruebas; la evidencia de ejecución de esta entrega se registra por separado en el [seguimiento de verificación](../../../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004). La lectura de esta página no acredita una ejecución.
