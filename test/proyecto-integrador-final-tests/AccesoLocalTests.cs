using Proyecto_Integrador_Final.Cliente;

namespace Proyecto_Integrador_Final.Tests;

public class AccesoLocalTests
{
    [Fact]
    public void ConservaIdentidadPorServidorFueraDelRepositorioYConPermisosExclusivos()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "acceso-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta);
            var a = new Uri("http://localhost:5000/");
            var b = new Uri("http://localhost:5001/");
            var invitado = new InvitadoLocal(Guid.NewGuid(), new string('A', 64), "Invitado");
            local.Guardar(a, invitado);
            local.GuardarConexion(b);
            var reabierto = new AccesoLocal(carpeta);
            Assert.Equal(invitado, reabierto.Leer(a));
            Assert.Null(reabierto.Leer(b));
            Assert.Equal(b.AbsoluteUri, reabierto.Conexion().Direccion);
            Assert.DoesNotContain(invitado.Secreto, invitado.ToString());
            if (!OperatingSystem.IsWindows())
            {
                Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(carpeta));
                foreach (var archivo in Directory.GetFiles(carpeta))
                    Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(archivo));
            }
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    [Fact]
    public void UnArchivoCorruptoNoSeInterpretaComoIdentidadAusenteNiSeSobrescribe()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "acceso-" + Guid.NewGuid());
        try
        {
            var local = new AccesoLocal(carpeta);
            var uri = new Uri("http://localhost:5000/");
            local.Guardar(uri, new(Guid.NewGuid(), new string('A', 64), "Ada"));
            var archivo = Directory.GetFiles(carpeta).Single();
            File.WriteAllText(archivo, "json corrupto");
            Assert.Throws<IOException>(() => local.Leer(uri));
            Assert.Throws<IOException>(() => local.Guardar(uri, new(Guid.NewGuid(), new string('B', 64), "Ana")));
            Assert.Equal("json corrupto", File.ReadAllText(archivo));
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }
}
