namespace Proyecto_Integrador_Final.Cliente;

internal sealed class DemostracionConsola(InterfazConsola interfaz, TextReader entrada, TextWriter salida, bool interactiva)
{
    public void Recorrer(string? cierre)
    {
        const string local = "Lucía";
        const string rival = "Mateo";
        var mesa = new MesaVisible(local, new(local, 14), new(rival, 14), 29, 1, 0, 0);
        var mano = new List<CartaVisible>
        {
            Carta(1, "Lysander", 750, 550, 450, 550, 350, 350),
            Carta(2, "Reched", 350, 750, 450, 450, 600, 400),
            Carta(3, "Vigía", 500, 350, 700, 500, 550, 400)
        };
        CartaVisible[] manoRival =
        [
            Carta(4, "Centinela", 750, 350, 550, 400, 450, 500),
            Carta(5, "Oráculo", 350, 500, 450, 750, 500, 450),
            Carta(6, "Lancero", 700, 650, 450, 250, 450, 500)
        ];

        interfaz.MostrarPreparacion(local, "Preparando la demostración local…");
        Continuar();
        interfaz.MostrarEsperaInicial(local, "Lucía presente · Mateo por incorporarse", "Falta Mateo");
        Continuar();
        interfaz.MostrarInicioRonda(mesa, local, mano);
        Continuar();

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
            string atributo = abre ? interfaz.PedirAtributo() : "Inteligencia";
            CartaVisible contraria = manoRival[mesa.Combate - 1];

            interfaz.MostrarEnvio(mesa, EstadoEnvio.Enviando);
            if (mesa.Combate == 1)
            {
                interfaz.MostrarEnvio(mesa, EstadoEnvio.Pendiente);
                Continuar();
            }
            interfaz.MostrarEnvio(mesa, EstadoEnvio.Recibido);

            int valorPropio = propia.Atributos[atributo];
            int valorRival = contraria.Atributos[atributo];
            ganaLucia = valorPropio > valorRival || (valorPropio == valorRival && !abre);
            mano.Remove(propia);
            mesa = ganaLucia
                ? mesa with { CombatesJ1 = mesa.CombatesJ1 + 1 }
                : mesa with { CombatesJ2 = mesa.CombatesJ2 + 1 };

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

    private void MostrarCierreSimulado(MesaVisible mesa, string cierre)
    {
        if (cierre == "pausa")
        {
            interfaz.MostrarSuspension(mesa, "Mateo perdió la sesión. Esperando que se reincorpore.", confirmada: true);
            Continuar();
            interfaz.MostrarSuspension(mesa, "Podés reintentar o salir.", confirmada: false);
            Continuar();
        }

        (string causa, string efecto, bool confirmada) = cierre switch
        {
            "abandono" => ("Mateo abandonó la partida.", "La partida terminó sin sumar una victoria.", true),
            "cancelacion" => ("El operador canceló la partida.", "La partida terminó sin sumar una victoria.", true),
            _ => ("Se cerró el cliente.", "La partida queda guardada.", false)
        };
        interfaz.MostrarInterrupcion(mesa.Local, causa, efecto, confirmada);
    }

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
}
