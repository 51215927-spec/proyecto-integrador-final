using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class InicioClienteTests
{
    [Fact]
    public async Task AbrirYSalirOfreceMenuSinSolicitarIdentificadoresNiCrearInvitado()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "menu-" + Guid.NewGuid());
        var salida = new StringWriter();
        await new InicioCliente(new StringReader("salir\n"), salida, new AccesoLocal(carpeta)).Ejecutar(default);
        Assert.Contains("Jugar", salida.ToString());
        Assert.Contains("Conexión", salida.ToString());
        Assert.Contains("localhost:5000", salida.ToString());
        Assert.DoesNotContain("UUID", salida.ToString());
        Assert.DoesNotContain("Credencial", salida.ToString());
        Assert.False(Directory.Exists(carpeta));
    }
}
