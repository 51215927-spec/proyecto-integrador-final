using System.Security.Cryptography;
using System.Text;

namespace Proyecto_Integrador_Final.Api;

internal sealed class InvitadoGuardado
{
    public Guid Id { get; set; }
    public required byte[] Hash { get; set; }
    public required string Nombre { get; set; }
}

internal sealed class AsignacionInvitado
{
    public Guid IdInvitado { get; set; }
    public Guid IdPartida { get; set; }
    public required string Rol { get; set; }
}

internal sealed record AltaInvitado(string? Nombre);
internal sealed record AccesoInvitado(Guid Id, string Secreto)
{
    public override string ToString() => "Acceso de invitado";
}

internal sealed class Invitados(AlmacenPartidas almacen)
{
    internal AccesoInvitado Crear(string? nombre)
    {
        nombre = string.IsNullOrWhiteSpace(nombre) ? "Invitado" : nombre.Trim();
        if (nombre.Length > 80 || nombre.Any(char.IsControl)) throw ErrorApi.Entrada();
        var acceso = new AccesoInvitado(Guid.NewGuid(), Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
        almacen.Infraestructura.Ejecutar(true, db =>
        {
            db.Invitados.Add(new() { Id = acceso.Id, Hash = Hash(acceso.Secreto), Nombre = nombre });
            return true;
        });
        return acceso;
    }

    internal InvitadoGuardado Autenticar(Guid id, string? secreto) => almacen.Infraestructura.Ejecutar(false, db =>
    {
        var invitado = db.Invitados.Find(id);
        if (invitado is null || string.IsNullOrEmpty(secreto) ||
            !CryptographicOperations.FixedTimeEquals(invitado.Hash, Hash(secreto)))
            throw new ErrorApi(401, "invitadoInvalido", "El acceso de invitado no es válido.");
        return invitado;
    });

    private static byte[] Hash(string secreto) => SHA256.HashData(Encoding.UTF8.GetBytes(secreto));
}
