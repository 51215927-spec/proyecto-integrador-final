using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Integrador_Final
{
    internal class Ronda
    {
        internal const int CombatesParaGanar = 2;

        private readonly Jugador mano;
        private readonly Jugador rival;
        private readonly List<Combate> combates;

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

        public bool EstaDefinida()
        {
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
        }

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

        public Jugador GetQuienEligeProximo()
        {
            if (this.EstaDefinida())
                throw new InvalidOperationException(
                    "La ronda ya está definida."
                );

            return this.combates.Count % 2 == 0
                ? this.mano
                : this.rival;
        }

        public Combate[] GetCombates() => this.combates.ToArray();
    }
}
