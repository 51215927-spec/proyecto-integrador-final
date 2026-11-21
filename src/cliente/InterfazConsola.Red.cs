namespace Proyecto_Integrador_Final.Cliente;

internal sealed partial class InterfazConsola
{
    internal void MostrarRed(string local, PresentacionCliente? vista, bool actual, string aviso,
        ResultadoCliente? resultado, int menu, bool confirma, int foco, bool atributo, bool pendiente, bool abandono)
    {
        // Los nombres ausentes usan el rol, sin confundir comunicación perdida con pausa confirmada.
        string Nombre(string rol) => vista?.Nombres.GetValueOrDefault(rol) ?? rol;
        string estado = vista?.Fase == "finalizada" ? "Final confirmado" : !actual ? "Sin comunicación · Estado actual sin confirmar"
            : vista?.Pausada == true ? "Pausa confirmada" : "En línea";
        Encabezado(local, estado, aviso);
        // El menú tiene prioridad visual sobre el turno y los resultados pendientes de lectura.
        if (menu >= 0)
        {
            Paneles(PanelInfo(confirma ? "¿Abandonar partida?" : "Menú",
                ListaOpciones(confirma ? ["No", "Sí"] : ["Continuar", "Salir sin abandonar", "Abandonar partida"], menu)));
            if (abandono) Escribir("Abandono sin confirmar. Salir localmente no confirma el abandono.");
            Pie(Controles); return;
        }
        if (vista is null) { Pie("Esc: menú · Ctrl+C: salir sin abandonar"); return; }
        if (resultado is { } r)
        {
            // El marcador histórico se calcula hasta el combate que el usuario está leyendo.
            var anteriores = vista.Resultados.Where(x => x.NumeroRonda < r.NumeroRonda ||
                x.NumeroRonda == r.NumeroRonda && x.NumeroCombate <= r.NumeroCombate).ToArray();
            int Combates(string rol) => anteriores.Count(x => x.NumeroRonda == r.NumeroRonda && x.GanadorCombate == rol);
            int Rondas(string rol) => anteriores.Count(x => x.GanadorRonda == rol);
            Escribir($"Ronda {r.NumeroRonda} · Combate {r.NumeroCombate}");
            // Las dos cartas se muestran juntas únicamente en el resultado confirmado.
            MostrarCombate(new(r.Atributo,
                new(Nombre("J1"), r.Cartas["J1"].Nombre, r.Puntajes["J1"]),
                new(Nombre("J2"), r.Cartas["J2"].Nombre, r.Puntajes["J2"]),
                Nombre(r.GanadorCombate), Combates("J1"), Combates("J2"), null, r.GanadorCombate == "J1"));
            if (r.GanadorRonda is { } ganador) Escribir($"{Nombre(ganador)} gana la ronda");
            Escribir($"Rondas · {Nombre("J1")} {Rondas("J1")} / {Nombre("J2")} {Rondas("J2")}");
            Pie("Enter para continuar · Esc: menú"); return;
        }
        Escribir($"{Nombre("J1")} (J1) / {Nombre("J2")} (J2) · Tu plaza: {vista.RolPropio}");
        Escribir($"Rondas · {Nombre("J1")} {vista.Marcador["J1"]} / {Nombre("J2")} {vista.Marcador["J2"]}");
        // El motivo final distingue victoria, abandono y cancelación del operador.
        if (vista.Fase == "finalizada")
        {
            Escribir(vista.MotivoFin switch
            {
                "victoria" => $"{Nombre(vista.Ganador!)} gana la partida",
                "abandono" => "Partida finalizada por abandono, sin sumar una victoria de juego.",
                _ => "El operador canceló la partida, sin sumar una victoria de juego."
            });
            Pie("Enter para salir"); return;
        }
        // La espera inicial no necesita número de ronda ni de combate.
        if (vista.Fase == "esperandoInicio") Escribir("Esperando inicio a cargo del operador.");
        else
        {
            Escribir($"Ronda {vista.NumeroRonda} · Combate {vista.NumeroCombate}");
            var combates = vista.Resultados.Where(r => r.NumeroRonda == vista.NumeroRonda).ToArray();
            Escribir($"Combates · {Nombre("J1")} {combates.Count(r => r.GanadorCombate == "J1")} / {Nombre("J2")} {combates.Count(r => r.GanadorCombate == "J2")}");
        }
        // Solo una vista actual, sin pausa ni envío pendiente, permite elegir.
        bool habilitada = actual && !vista.Pausada && !pendiente && !abandono && vista.Solicitud is not null;
        if (habilitada && vista.Solicitud is { } s)
        {
            if (atributo) Paneles(PanelInfo("Atributo del combate", ListaOpciones(s.AtributosPermitidos, foco)));
            else
            {
                Escribir(s.Tipo == "carta" ? "Te toca responder" : "Tu turno: carta y atributo");
                MostrarMano(s.IdsPersonajePermitidos.Select(id => vista.Mano.Single(c => c.IdPersonaje == id)).ToArray(), foco);
            }
            Pie(Controles + "\nEsc: menú");
        }
        else
        {
            // Durante la espera se mantiene visible la mano, sin habilitar otra elección.
            MostrarMano(vista.Mano);
            if (pendiente) Escribir("Confirmación pendiente · Comprobando la elección antes de permitir otra.");
            else if (actual && !vista.Pausada && vista.Fase != "esperandoInicio") Escribir("Esperando solicitud del servidor.");
            Pie("Enter: volver a mostrar el estado · Esc: menú · Ctrl+C: salir sin abandonar");
        }
    }
}
