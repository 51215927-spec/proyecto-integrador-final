# VistaPartidaTests.cs

Fuente: [test/proyecto-integrador-final-tests/VistaPartidaTests.cs](../../../../test/proyecto-integrador-final-tests/VistaPartidaTests.cs). Proyecto: `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj` (`net8.0`), namespace `Proyecto_Integrador_Final.Tests`. Referencia: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Privacidad, pausa y lectura sin efectos.

## Preparación, miembros, acciones y aserciones

CrearEstado prepara dos plazas/cartas y solicitud del elector. PresenciaSoloCambiaPausaYSolicitudSinModificarElEstado compara la serialización completa antes/después de construir, conserva revisión y UUID al reanudar. CadaPlazaSoloVeSuManoYElTurnoCorrespondiente exige IDs separados, opciones propias y ausencia de solicitud rival. ModificarUnaVistaNoCambiaCartasNiOpcionesDelDominio altera los diccionarios y arrays de respuesta y verifica una vista posterior intacta. HistorialExponeSoloCombatesResueltosYGanadorEnElCierreDeRonda filtra un combate pendiente, comprueba cartas/puntajes y ganador únicamente en el cierre. Resuelto crea los resultados usados por ese último caso.

## Errores, recursos y límites

Pruebas en memoria de VistaPartida, sin autenticar ni renovar sesiones; esas responsabilidades se verifican por integración. Los estados preparados son fixtures de proyección, no partidas ejecutadas. xUnit descubre Fact/Theory y crea cada caso con sus datos; una aserción fallida invalida el caso. La suite final tiene 190 aprobadas, cero fallidas/omitidas; no acredita dos Windows físicos.

## Código contrastado

Archivo completo: incluye auxiliares, datos, parámetros y limpieza; no se omiten aserciones.

```csharp
using System.Text.Json;
using Proyecto_Integrador_Final.Api;

namespace Proyecto_Integrador_Final.Tests;

public sealed class VistaPartidaTests
{
    [Fact]
    public void PresenciaSoloCambiaPausaYSolicitudSinModificarElEstado()
    {
        var estado = CrearEstado();
        var vista = new VistaPartida();
        string antes = JsonSerializer.Serialize(estado);

        var activa = vista.Construir(estado, "J1", true);
        var pausada = vista.Construir(estado, "J1", false);
        var reanudada = vista.Construir(estado, "J1", true);

        Assert.False(activa.Pausada);
        Assert.True(pausada.Pausada);
        Assert.Null(pausada.Solicitud);
        Assert.Equal(activa.Revision, pausada.Revision);
        Assert.Equal(activa.Solicitud!.IdSolicitud, reanudada.Solicitud!.IdSolicitud);
        Assert.Equal(antes, JsonSerializer.Serialize(estado));
    }

    [Fact]
    public void CadaPlazaSoloVeSuManoYElTurnoCorrespondiente()
    {
        var estado = CrearEstado();
        var vista = new VistaPartida();

        var primero = vista.Construir(estado, "J1", true);
        var segundo = vista.Construir(estado, "J2", true);

        Assert.Equal(1, Assert.Single(primero.Mano).IdPersonaje);
        Assert.Equal(2, Assert.Single(segundo.Mano).IdPersonaje);
        Assert.Equal(new[] { 1 }, primero.Solicitud!.IdsPersonajePermitidos);
        Assert.Equal(6, primero.Solicitud.AtributosPermitidos.Length);
        Assert.Null(segundo.Solicitud);
        Assert.Empty(primero.Resultados);
        Assert.Empty(segundo.Resultados);
    }

    [Fact]
    public void ModificarUnaVistaNoCambiaCartasNiOpcionesDelDominio()
    {
        var estado = CrearEstado();
        var vista = new VistaPartida();
        var salida = vista.Construir(estado, "J1", true);
        salida.Mano[0].Atributos["Fuerza"] = -1;
        salida.Solicitud!.IdsPersonajePermitidos[0] = -1;
        salida.Nombres["J1"] = "otro";

        var siguiente = vista.Construir(estado, "J1", true);
        Assert.Equal(750, siguiente.Mano[0].Atributos["Fuerza"]);
        Assert.Equal(1, siguiente.Solicitud!.IdsPersonajePermitidos[0]);
        Assert.Equal("Ana", siguiente.Nombres["J1"]);
    }

    [Fact]
    public void HistorialExponeSoloCombatesResueltosYGanadorEnElCierreDeRonda()
    {
        var estado = CrearEstado();
        estado.Rondas.Add(new RondaPartida { Numero = 1, Mano = "J1", Ganador = "J2" });
        estado.Combates.AddRange(
        [
            Resuelto(1), Resuelto(2),
            new CombatePartida { NumeroRonda = 2, Numero = 1, Elector = "J2", IdCartaElector = 2, Atributo = "Fuerza" }
        ]);

        var resultados = new VistaPartida().Construir(estado, "J1", true).Resultados;

        Assert.Equal(2, resultados.Length);
        Assert.Null(resultados[0].GanadorRonda);
        Assert.Equal("J2", resultados[1].GanadorRonda);
        Assert.Equal(1, resultados[0].Cartas["J1"].IdPersonaje);
        Assert.Equal(2, resultados[0].Cartas["J2"].IdPersonaje);
        Assert.Equal(750, resultados[0].Puntajes["J1"]);

        static CombatePartida Resuelto(int numero) => new()
        {
            NumeroRonda = 1, Numero = numero, Elector = "J1", IdCartaElector = 1,
            IdCartaRival = 2, Atributo = "Fuerza", PuntajeElector = 750, PuntajeRival = 750, Ganador = "J2"
        };
    }

    private static EstadoPartida CrearEstado()
    {
        var idSolicitud = Guid.NewGuid();
        return new EstadoPartida
        {
            Id = Guid.NewGuid(), Revision = 3, Fase = "esperandoElector",
            NumeroRonda = 1, NumeroCombate = 1, IdSolicitud = idSolicitud,
            Participantes =
            [
                new ParticipantePartida { Rol = "J1", Nombre = "Ana" },
                new ParticipantePartida { Rol = "J2", Nombre = "Bruno" }
            ],
            Cartas =
            [
                new CartaPartida { IdPersonaje = 1, Nombre = "propia", Descripcion = "", Ascii = "*", Fuerza = 750, Ubicacion = "J1", Orden = 0 },
                new CartaPartida { IdPersonaje = 2, Nombre = "rival", Descripcion = "", Ascii = "*", Fuerza = 750, Ubicacion = "J2", Orden = 1 }
            ],
            Solicitudes =
            [
                new SolicitudPartida { Id = idSolicitud, NumeroRonda = 1, NumeroCombate = 1, Rol = "J1", Tipo = "cartaYAtributo" }
            ]
        };
    }
}
```

Véase [auditoría006](../../verificacion.md#auditoría-del-spec-006--2026-11-22).
