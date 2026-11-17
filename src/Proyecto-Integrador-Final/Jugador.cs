using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Integrador_Final
{
    internal class Jugador
    {
        private readonly string nombre;
        private readonly List<Personaje> mano;
        private int rondasGanadas;

        public Jugador(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre del jugador no puede estar vacío.",
                    nameof(nombre)
                );

            this.nombre = nombre;
            this.mano = new List<Personaje>();
            this.rondasGanadas = 0;
        }

        public void RecibirCartas(Personaje[] personajes)
        {
            ArgumentNullException.ThrowIfNull(personajes);
            this.mano.AddRange(personajes);
        }

        public void QuitarCarta(Personaje personaje)
        {
            ArgumentNullException.ThrowIfNull(personaje);
            this.mano.Remove(personaje);
        }

        public Personaje[] EntregarCartas()
        {
            Personaje[] cartas = this.mano.ToArray();
            this.mano.Clear();

            return cartas;
        }

        public Personaje[] GetCartas() => this.mano.ToArray();

        public void SumarRondaGanada() => this.rondasGanadas++;

        public int GetRondasGanadas() => this.rondasGanadas;

        public string GetNombre() => this.nombre;

        public bool TieneCarta(Personaje personaje)
        {
            ArgumentNullException.ThrowIfNull(personaje);
            return this.mano.Contains(personaje);
        }
    }
}
