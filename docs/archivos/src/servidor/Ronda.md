# Ronda.cs

Fuente: [src/servidor/Ronda.cs](../../../../src/servidor/Ronda.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión: [catálogo común](../../README.md#versión-documentada).

## Propósito y responsabilidad

Reglas compartidas de ronda y alternancia.

## Estado, miembros y ejecución

CombatesParaGanar fija dos. GanadorPorVictorias devuelve 1 o 2 al alcanzar el umbral, o null; EsTurnoMano decide la alternancia por cantidad de combates. Ambas funciones se reutilizan desde Partida y desde la instancia Ronda. El constructor exige participantes distintos y no nulos. AgregarCombate rechaza null, ronda definida y más de tres combates. EstaDefinida cuenta victorias y usa GanadorPorVictorias; GetGanador exige definición y devuelve la referencia participante. GetQuienEligeProximo exige ronda abierta y usa EsTurnoMano; GetCombates devuelve un array de las referencias registradas.

## Errores, límites y efectos

No persiste ni decide fin de partida. La comparación de ganadores de los combates usa identidad de Jugador. Las funciones compartidas evitan una segunda implementación de las reglas en REST.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
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

        internal static int? GanadorPorVictorias(int victoriasMano, int victoriasRival) =>
            victoriasMano >= CombatesParaGanar ? 1 : victoriasRival >= CombatesParaGanar ? 2 : null;

        internal static bool EsTurnoMano(int combatesResueltos) => combatesResueltos % 2 == 0;

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

            return GanadorPorVictorias(victoriasMano, victoriasRival) is not null;
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

            return GanadorPorVictorias(victoriasMano, this.combates.Count - victoriasMano) == 1
                ? this.mano
                : this.rival;
        }

        public Jugador GetQuienEligeProximo()
        {
            if (this.EstaDefinida())
                throw new InvalidOperationException(
                    "La ronda ya está definida."
                );

            // La elección empieza con el jugador mano y alterna después de cada combate.
            return EsTurnoMano(this.combates.Count)
                ? this.mano
                : this.rival;
        }

        public Combate[] GetCombates() => this.combates.ToArray();
    }
}
```

## Relaciones y verificación

Véanse [arquitectura](../../../arquitectura.md), [modelos](../../../modelos.md) y [auditoría del spec 006](../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
