using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

internal static class ConsolaServidor
{
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

        switch (partes[0].ToLowerInvariant())
        {
            case "":
                break;
            case "ayuda":
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
                salida.WriteLine("Comando o argumentos inválidos. Escribí ayuda.");
                break;
        }
    }

    private static void Ayuda(TextWriter salida) => salida.WriteLine(
        """
        crear
        iniciar <idPartida>
        cancelar <idPartida>
        cerrar-sesion <idPartida> <J1|J2>
        reemplazar-credencial <idPartida> <J1|J2>
        desactivar <idPersonaje>
        """);
}
