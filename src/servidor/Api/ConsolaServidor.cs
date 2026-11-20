using Proyecto_Integrador_Final.Consola;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

internal static class ConsolaServidor
{
    private static readonly Dictionary<string, (string Grupo, string Descripcion)> comandos = new()
    {
        ["crear"] = ("Partidas", "Crea una partida y muestra su ID y las credenciales de J1 y J2."),
        ["iniciar"] = ("Partidas", "Inicia la partida cuando están presentes los dos jugadores y hay al menos seis personajes activos."),
        ["cancelar"] = ("Partidas", "Finaliza la partida por cancelación."),
        ["cerrar-sesion"] = ("Jugadores", "Cierra la sesión del jugador. Puede volver a conectarse con su credencial."),
        ["reemplazar-credencial"] = ("Jugadores", "Cierra la sesión, invalida la credencial anterior y muestra la nueva."),
        ["desactivar"] = ("Catálogo", "Excluye al personaje de las partidas que se inicien en adelante."),
        ["ayuda"] = ("Ayuda", "Vuelve a mostrar esta guía.")
    };

    internal static void Ejecutar(RepositorioJuego juego, TextReader entrada, TextWriter salida,
        CancellationToken detener)
    {
        Ayuda(salida);
        while (!detener.IsCancellationRequested && entrada.ReadLine() is string linea)
        {
            if (detener.IsCancellationRequested)
                break;

            try
            {
                Comando(juego, linea.Trim(), entrada, salida);
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (ErrorApi error)
            {
                salida.WriteLine(error.Message);
            }
            catch (Exception error) when (error is SqliteException or DbUpdateException)
            {
                salida.WriteLine("El almacenamiento no está disponible.");
            }
        }
    }

    private static void Comando(RepositorioJuego juego, string linea, TextReader entrada, TextWriter salida)
    {
        string comando = linea.ToLowerInvariant();
        var lector = new EntradaConsola(entrada, salida);
        Guid id = Guid.Empty;
        string? rol = null;
        if (comando is "iniciar" or "cancelar" or "cerrar-sesion" or "reemplazar-credencial")
            id = Guid.Parse(lector.Validar("UUID de la partida", v => Guid.TryParse(v, out var g) && g != Guid.Empty,
                "Ingresá un UUID válido y distinto de cero."));
        if (comando is "cerrar-sesion" or "reemplazar-credencial")
            rol = lector.Validar("Plaza: J1 o J2", v => v.ToUpperInvariant() is "J1" or "J2",
                "Elegí J1 o J2.").ToUpperInvariant();
        string argumento = comando == "desactivar"
            ? lector.Validar("ID numérico del personaje", v => int.TryParse(v, out int n) && n > 0,
                "Ingresá un ID numérico positivo.") : "";
        switch (comando)
        {
            case "":
                break;
            case "ayuda":
                Ayuda(salida);
                break;
            case "crear":
                var partida = juego.CrearPartida();
                salida.WriteLine($"Partida {partida.Id}");
                salida.WriteLine($"J1 {partida.CredencialJ1}");
                salida.WriteLine($"J2 {partida.CredencialJ2}");
                break;
            case "iniciar":
                juego.Iniciar(id);
                salida.WriteLine("Partida iniciada.");
                break;
            case "cancelar":
                juego.Cancelar(id);
                salida.WriteLine("Partida cancelada.");
                break;
            case "cerrar-sesion":
                juego.CerrarSesion(id, rol!);
                salida.WriteLine("Sesión cerrada.");
                break;
            case "reemplazar-credencial":
                salida.WriteLine($"{rol} {juego.ReemplazarCredencial(id, rol!)}");
                break;
            case "desactivar" when int.TryParse(argumento, out int personaje):
                juego.Desactivar(personaje);
                salida.WriteLine("Personaje desactivado.");
                break;
            default:
                salida.WriteLine("Comando desconocido. Escribí ayuda para ver los comandos.");
                break;
        }
    }

    private static void Ayuda(TextWriter salida)
    {
        salida.WriteLine("=== Consola del servidor de Guerra ===");
        salida.WriteLine("Para empezar, escribí crear y compartí con cada jugador el ID de la partida y su credencial.");
        salida.WriteLine("Cuando ambos estén conectados, escribí iniciar y luego ingresá ese ID.");

        foreach (var grupo in comandos.GroupBy(comando => comando.Value.Grupo))
        {
            salida.WriteLine();
            salida.WriteLine(grupo.Key);
            foreach (var comando in grupo)
            {
                salida.WriteLine($"  {comando.Key}");
                salida.WriteLine($"    {comando.Value.Descripcion}");
            }
        }

        salida.WriteLine();
        salida.WriteLine("Elegí una acción y luego respondé sus preguntas. Fin de entrada cierra la consola.");
        salida.WriteLine();
    }
}
