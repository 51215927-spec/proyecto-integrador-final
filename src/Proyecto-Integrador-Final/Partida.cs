using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Integrador_Final
{
    internal class Partida
    {
        private const int RONDAS_PARA_GANAR = 15;

        private readonly Jugador j1;
        private readonly Jugador j2;
        private readonly Mazo mazo;

        private Ronda? rondaActual;
        private int numeroRonda;

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

        private void Iniciar()
        {
            if (this.mazo.GetCantidadCartas() < 6)
                throw new InvalidOperationException(
                    "No se puede jugar porque no hay suficientes cartas."
                );

            this.numeroRonda = 1;
            this.rondaActual = new Ronda(this.j1, this.j2);

            this.RepartirCartas();
        }

        private void RepartirCartas()
        {
            this.mazo.Mezclar();

            Personaje[] cartasJ1 = this.mazo.Repartir(3);
            Personaje[] cartasJ2 = this.mazo.Repartir(3);

            this.j1.RecibirCartas(cartasJ1);
            this.j2.RecibirCartas(cartasJ2);
        }

        private void JugarRonda()
        {
            while (!this.rondaActual!.EstaDefinida())
            {
                Jugador quienElige = this.rondaActual.GetQuienEligeProximo();

                Jugador rival = ReferenceEquals(quienElige, this.j1)
                    ? this.j2
                    : this.j1;

                Combate combate = new Combate(quienElige, rival);

                // La selección del atributo y de las cartas se completará
                // mediante la comunicación con los clientes en la actividad 6.

                if (!combate.EstaCompleto())
                    throw new InvalidOperationException(
                        "El combate no se completó."
                    );

                combate.Resolver();
                this.rondaActual.AgregarCombate(combate);
            }

            Jugador ganador = this.rondaActual.GetGanador();
            ganador.SumarRondaGanada();
        }

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

            this.mazo.RecibirCartas(cartas.ToArray());
        }

        private bool EstaTerminada()
        {
            return this.j1.GetRondasGanadas() >= RONDAS_PARA_GANAR
                || this.j2.GetRondasGanadas() >= RONDAS_PARA_GANAR;
        }

        private Jugador DeterminarGanadorPartida()
        {
            return this.j1.GetRondasGanadas() >= RONDAS_PARA_GANAR
                ? this.j1
                : this.j2;
        }
    }
}
