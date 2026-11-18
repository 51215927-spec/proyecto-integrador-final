using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

internal static class ConsolaServidor
{
    private static readonly Dictionary<string, (string Grupo, string Uso, string Descripcion)> comandos = new()
    {
        ["crear"] = ("Partidas", "crear", "Crea una partida y muestra su ID y las credenciales de J1 y J2."),
        ["iniciar"] = ("Partidas", "iniciar <idPartida>", "Inicia la partida cuando están presentes los dos jugadores y hay al menos seis personajes activos."),
        ["cancelar"] = ("Partidas", "cancelar <idPartida>", "Finaliza la partida por cancelación."),
        ["cerrar-sesion"] = ("Jugadores", "cerrar-sesion <idPartida> <J1|J2>", "Cierra la sesión del jugador. Puede volver a conectarse con su credencial."),
        ["reemplazar-credencial"] = ("Jugadores", "reemplazar-credencial <idPartida> <J1|J2>", "Cierra la sesión, invalida la credencial anterior y muestra la nueva."),
        ["desactivar"] = ("Catálogo", "desactivar <idPersonaje>", "Excluye al personaje de las partidas que se inicien en adelante."),
        ["ayuda"] = ("Ayuda", "ayuda", "Vuelve a mostrar esta guía.")
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
                Comando(juego, linea.Trim(), salida);
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

    private static void Comando(RepositorioJuego juego, string linea, TextWriter salida)
    {
        string[] partes = linea.Split(' ', 2, StringSplitOptions.TrimEntries);
        string argumento = partes.Length == 2 ? partes[1] : "";
        string[] valores = argumento.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Guid id = valores.Length > 0 && Guid.TryParse(valores[0], out var uuid) ? uuid : Guid.Empty;
        string? rol = valores.Length == 2 ? valores[1].ToUpperInvariant() : null;

        string comando = partes[0].ToLowerInvariant();
        switch (comando)
        {
            case "":
                break;
            case "ayuda" when argumento.Length == 0:
                Ayuda(salida);
                break;
            case "crear" when argumento.Length == 0:
                var partida = juego.CrearPartida();
                salida.WriteLine($"Partida {partida.Id}");
                salida.WriteLine($"J1 {partida.CredencialJ1}");
                salida.WriteLine($"J2 {partida.CredencialJ2}");
                break;
            case "iniciar" when id != Guid.Empty && valores.Length == 1:
                juego.Iniciar(id);
                salida.WriteLine("Partida iniciada.");
                break;
            case "cancelar" when id != Guid.Empty && valores.Length == 1:
                juego.Cancelar(id);
                salida.WriteLine("Partida cancelada.");
                break;
            case "cerrar-sesion" when id != Guid.Empty && rol is "J1" or "J2":
                juego.CerrarSesion(id, rol);
                salida.WriteLine("Sesión cerrada.");
                break;
            case "reemplazar-credencial" when id != Guid.Empty && rol is "J1" or "J2":
                salida.WriteLine($"{rol} {juego.ReemplazarCredencial(id, rol)}");
                break;
            case "desactivar" when int.TryParse(argumento, out int personaje):
                juego.Desactivar(personaje);
                salida.WriteLine("Personaje desactivado.");
                break;
            default:
                if (comandos.TryGetValue(comando, out var conocido))
                    salida.WriteLine($"Argumentos inválidos. Uso: {conocido.Uso}. Escribí ayuda para ver los argumentos.");
                else
                    salida.WriteLine($"Comando desconocido: {partes[0]}. Escribí ayuda para ver los comandos.");
                break;
        }
    }

    private static void Ayuda(TextWriter salida)
    {
        salida.WriteLine("=== Consola del servidor de Guerra ===");
        salida.WriteLine("Para empezar, escribí crear y compartí con cada jugador el ID de la partida y su credencial.");
        salida.WriteLine("Cuando ambos estén conectados, usá iniciar con ese ID.");

        foreach (var grupo in comandos.Values.GroupBy(comando => comando.Grupo))
        {
            salida.WriteLine();
            salida.WriteLine(grupo.Key);
            foreach (var comando in grupo)
            {
                salida.WriteLine($"  {comando.Uso}");
                salida.WriteLine($"    {comando.Descripcion}");
            }
        }

        salida.WriteLine();
        salida.WriteLine("Argumentos");
        salida.WriteLine("  <idPartida>    ID que muestra crear, por ejemplo 12345678-1234-4234-8234-123456789abc.");
        salida.WriteLine("  <J1|J2>        Escribí J1 o J2 para elegir al jugador.");
        salida.WriteLine("  <idPersonaje>  ID numérico del catálogo. Ejemplo: desactivar 12.");
        salida.WriteLine("  Escribí los valores sin los signos < y >.");
        salida.WriteLine();
    }
}
