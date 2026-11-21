using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public sealed class EstadoClienteTests
{
    [Fact]
    public void PrimeraVistaOmiteHistoriaYReconectarConservaResultadosNuevosEnOrden()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        var anterior = Resultado(1, 1);
        var nuevo = Resultado(1, 2);
        estado.Aplicar(Vista(partida) with { Resultados = [anterior] });
        Assert.Empty(estado.Resultados);
        estado.PerderComunicacion();
        estado.Aplicar(Vista(partida) with { Resultados = [nuevo, anterior] });
        estado.Aplicar(Vista(partida) with { Resultados = [anterior, nuevo] });
        Assert.Same(nuevo, Assert.Single(estado.Resultados));
        Assert.True(estado.Actual);
    }

    [Fact]
    public void PausaYNombresSeAplicanSinCambiarRevisionYErroresNoDestruyenUltimoEstado()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        var inicial = Vista(partida);
        estado.Aplicar(inicial);
        var pausa = inicial with { Pausada = true, Nombres = new() { ["J1"] = "Ana", ["J2"] = "Ada" } };
        estado.Aplicar(pausa);
        Assert.Same(pausa, estado.Vista);
        estado.PerderComunicacion();
        Assert.False(estado.Actual);
        Assert.Same(pausa, estado.Vista);
        Assert.Throws<FalloCliente>(() => estado.Aplicar(pausa with { Fase = "desconocida" }));
        Assert.Same(pausa, estado.Vista);
    }

    [Fact]
    public void ManoVaciaEsValidaEnEsperaPeroNoConOpcionesImposibles()
    {
        Guid partida = Guid.NewGuid();
        var estado = new EstadoCliente(partida);
        estado.Aplicar(Vista(partida));
        Assert.Throws<FalloCliente>(() => estado.Aplicar(Vista(partida) with
        {
            Solicitud = new(Guid.NewGuid(), "carta", [999], [])
        }));
    }

    internal static PresentacionCliente Vista(Guid id) => new(id, 1, "esperandoInicio", false, "J1",
        new() { ["J1"] = "Ana", ["J2"] = null }, null, null, new() { ["J1"] = 0, ["J2"] = 0 }, [], null, [], null, null);
    internal static ResultadoCliente Resultado(int ronda, int combate) => new(ronda, combate,
        new() { ["J1"] = new(1, "A", new Dictionary<string, int>()), ["J2"] = new(2, "B", new Dictionary<string, int>()) },
        "Fuerza", new() { ["J1"] = 10, ["J2"] = 20 }, "J2", null);
}
