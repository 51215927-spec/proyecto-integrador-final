# ConsolaServidor.cs

Fuente: [src/servidor/Api/ConsolaServidor.cs](../../../../../src/servidor/Api/ConsolaServidor.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión: [catálogo común](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Comandos administrativos del operador.

## Estado, miembros y ejecución

comandos conserva grupos y descripciones. Ejecutar recibe CoordinadorPartidas, AdministradorSesiones y CatalogoPersonajes, muestra ayuda y lee hasta EOF/cancelación. Comando pregunta UUID/plaza/ID con EntradaConsola y despacha crear/iniciar/cancelar al coordinador, cerrar-sesion/reemplazar-credencial a sesiones y desactivar al catálogo. Ayuda agrupa las siete entradas sin escribir estado.

## Errores, límites y efectos

EOF durante preguntas cancela la acción incompleta. ErrorApi muestra mensaje y continúa; SqliteException/DbUpdateException muestran almacenamiento no disponible. Las credenciales creadas o reemplazadas se entregan intencionalmente al operador. El formato de comandos y salidas se conserva.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
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

    internal static void Ejecutar(CoordinadorPartidas juego, AdministradorSesiones sesiones, CatalogoPersonajes catalogo, TextReader entrada, TextWriter salida,
        CancellationToken detener)
    {
        Ayuda(salida);
        while (!detener.IsCancellationRequested && entrada.ReadLine() is string linea)
        {
            if (detener.IsCancellationRequested)
                break;

            try
            {
                Comando(juego, sesiones, catalogo, linea.Trim(), entrada, salida);
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

    private static void Comando(CoordinadorPartidas juego, AdministradorSesiones sesiones, CatalogoPersonajes catalogo, string linea, TextReader entrada, TextWriter salida)
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
                sesiones.CerrarSesion(id, rol!);
                salida.WriteLine("Sesión cerrada.");
                break;
            case "reemplazar-credencial":
                salida.WriteLine($"{rol} {sesiones.ReemplazarCredencial(id, rol!)}");
                break;
            case "desactivar" when int.TryParse(argumento, out int personaje):
                catalogo.Desactivar(personaje);
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
```

## Relaciones y verificación

Véanse [arquitectura](../../../../arquitectura.md), [modelos](../../../../modelos.md) y [auditoría del spec 006](../../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
