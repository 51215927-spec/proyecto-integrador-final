using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Integrador_Final
{
    internal class Combate
    {
        private readonly Jugador jugadorQueElige;
        private readonly Jugador jugadorRival;

        private TipoAtributo? atributoElegido;
        private Personaje? cartaQueElige;
        private Personaje? cartaRival;

        private Jugador? ganador;

        public Combate(Jugador quienElige, Jugador rival)
        {
            jugadorQueElige = quienElige
                ?? throw new ArgumentNullException(nameof(quienElige));

            jugadorRival = rival
                ?? throw new ArgumentNullException(nameof(rival));

            if (ReferenceEquals(quienElige, rival))
                throw new ArgumentException(
                    "Los participantes de un combate deben ser distintos."
                );
        }

        public void SetAtributo(TipoAtributo tipo)
        {
            if (atributoElegido.HasValue)
                throw new InvalidOperationException(
                    "El atributo del combate ya fue elegido."
                );

            if (!Enum.IsDefined(tipo))
                throw new ArgumentOutOfRangeException(nameof(tipo));

            atributoElegido = tipo;
        }

        public void SetCarta(Jugador jugador, Personaje personaje)
        {
            ArgumentNullException.ThrowIfNull(jugador);
            ArgumentNullException.ThrowIfNull(personaje);

            if (!ReferenceEquals(jugador, jugadorQueElige) &&
                !ReferenceEquals(jugador, jugadorRival))
            {
                throw new ArgumentException(
                    "El jugador no participa en este combate.",
                    nameof(jugador)
                );
            }

            if (ReferenceEquals(jugador, jugadorQueElige))
            {
                if (cartaQueElige != null)
                    throw new InvalidOperationException(
                        "El jugador que elige ya registró su carta."
                    );
            }
            else
            {
                if (cartaRival != null)
                    throw new InvalidOperationException(
                        "El rival ya registró su carta."
                    );
            }

            if (!jugador.TieneCarta(personaje))
                throw new ArgumentException(
                    "La carta no se encuentra en la mano del jugador.",
                    nameof(personaje)
                );

            jugador.QuitarCarta(personaje);

            if (ReferenceEquals(jugador, jugadorQueElige))
                cartaQueElige = personaje;
            else
                cartaRival = personaje;
        }

        public bool EstaCompleto()
        {
            return atributoElegido.HasValue
                && cartaQueElige != null
                && cartaRival != null;
        }

        public Jugador Resolver()
        {
            if (!EstaCompleto())
                throw new InvalidOperationException(
                    "No se puede resolver un combate incompleto."
                );

            if (ganador != null)
                throw new InvalidOperationException(
                    "El combate ya fue resuelto."
                );

            int valorElector = cartaQueElige!.GetAtributo(atributoElegido!.Value);
            int valorRival = cartaRival!.GetAtributo(atributoElegido.Value);

            ganador = valorElector > valorRival
                ? jugadorQueElige
                : jugadorRival;

            return ganador;
        }

        public Jugador GetGanador()
        {
            return ganador
                ?? throw new InvalidOperationException(
                    "El combate todavía no fue resuelto."
                );
        }

        public TipoAtributo GetAtributo()
        {
            return atributoElegido
                ?? throw new InvalidOperationException(
                    "Todavía no se eligió un atributo."
                );
        }

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

        public Jugador[] GetParticipantes()
        {
            return new Jugador[]
            {
                jugadorQueElige,
                jugadorRival
            };
        }
    }
}
