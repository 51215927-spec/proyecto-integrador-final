using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Proyecto_Integrador_Final.Cliente;

internal sealed record InvitadoLocal(Guid Id, string Secreto, string Nombre)
{
    public override string ToString() => "Invitado local";
}
internal sealed record PreferenciasConexion(string Direccion = "http://localhost:5000/");

// La carpeta es exclusiva del usuario; la sustitución es atómica y nunca ignora corrupción.
internal sealed class AccesoLocal(string directorio)
{
    internal static AccesoLocal DelUsuario()
    {
        var baseUsuario = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.DoNotVerify);
        if (string.IsNullOrEmpty(baseUsuario) || !Path.IsPathFullyQualified(baseUsuario))
            throw new IOException("No se puede localizar el almacenamiento del usuario.");
        return new(Path.Combine(baseUsuario, "GuerraCartas"));
    }
    private string Ruta(Uri servidor) => Path.Combine(directorio,
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(servidor.AbsoluteUri))) + ".json");

    private sealed record ArchivoPerfiles(int Version, InvitadoLocal[] Perfiles);

    // Compatibilidad para consumidores de un único acceso; el menú usa siempre la lista.
    internal InvitadoLocal? Leer(Uri servidor) => Perfiles(servidor).FirstOrDefault();
    internal InvitadoLocal[] Perfiles(Uri servidor)
    {
        using var archivo = LeerArchivo<JsonDocument>(Ruta(servidor));
        if (archivo is null) return [];
        try
        {
            var raiz = archivo.RootElement;
            InvitadoLocal[] perfiles;
            if (raiz.TryGetProperty("Version", out var version))
            {
                if (version.GetInt32() != 2) throw new IOException("Versión de acceso local incompatible.");
                perfiles = raiz.GetProperty("Perfiles").Deserialize<InvitadoLocal[]>() ?? throw new IOException("Acceso local inválido.");
            }
            else perfiles = [raiz.Deserialize<InvitadoLocal>() ?? throw new IOException("Acceso local inválido.")];
            if (perfiles.Any(p => p is null || p.Id == Guid.Empty || p.Secreto is not { Length: 64 } || string.IsNullOrWhiteSpace(p.Nombre)) ||
                perfiles.Select(p => p.Id).Distinct().Count() != perfiles.Length)
                throw new IOException("Acceso local inválido.");
            return perfiles;
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        { throw new IOException("Acceso local inválido."); }
    }

    internal void Agregar(Uri servidor, InvitadoLocal invitado) => Modificar(servidor, perfiles =>
    {
        if (perfiles.Any(p => p.Id == invitado.Id))
        {
            if (!perfiles.Contains(invitado)) throw new IOException("El perfil existente tiene otro acceso.");
            return perfiles;
        }
        return [.. perfiles, invitado];
    });
    internal void Reemplazar(Uri servidor, Guid anterior, InvitadoLocal invitado) => Modificar(servidor, perfiles =>
    {
        if (!perfiles.Any(p => p.Id == anterior) || perfiles.Any(p => p.Id != anterior && p.Id == invitado.Id))
            throw new IOException("El perfil elegido cambió. Volvé a seleccionar.");
        return perfiles.Select(p => p.Id == anterior ? invitado : p).ToArray();
    });

    private static T? LeerArchivo<T>(string ruta) where T : class
    {
        // Solo la ausencia real equivale a primera ejecución. Acceso denegado y JSON inválido son errores.
        try
        {
            if (new FileInfo(ruta).LinkTarget is not null) throw new IOException("El acceso local no puede ser un enlace.");
            using var archivo = File.OpenRead(ruta);
            return JsonSerializer.Deserialize<T>(archivo) ?? throw new IOException("Archivo local inválido.");
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
        catch (JsonException) { throw new IOException("Archivo local inválido."); }
        catch (UnauthorizedAccessException) { throw new IOException("No se puede leer el acceso local."); }
    }
    internal void Preparar()
    {
        try { PrepararCarpeta(); }
        catch (UnauthorizedAccessException) { throw new IOException("No se puede acceder al almacenamiento local."); }
    }
    private void PrepararCarpeta()
    {
        if (Directory.Exists(directorio) && new DirectoryInfo(directorio).LinkTarget is not null)
            throw new IOException("La carpeta de acceso no puede ser un enlace.");
        if (OperatingSystem.IsWindows())
        {
            var seguridad = new DirectorySecurity();
            seguridad.SetAccessRuleProtection(true, false);
            seguridad.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!,
                FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None, AccessControlType.Allow));
            var carpeta = Directory.CreateDirectory(directorio);
            carpeta.SetAccessControl(seguridad);
        }
        else
        {
            Directory.CreateDirectory(directorio, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            File.SetUnixFileMode(directorio, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }
    internal void Guardar(Uri servidor, InvitadoLocal invitado, bool reemplazar = false) => Modificar(servidor, perfiles =>
    {
        if (perfiles.Length == 0) return [invitado];
        if (perfiles.Contains(invitado)) return perfiles;
        if (reemplazar && perfiles.Length == 1) return [invitado];
        throw new IOException("Ya existe una identidad para este servidor.");
    });

    private void Modificar(Uri servidor, Func<InvitadoLocal[], InvitadoLocal[]> cambio)
    {
        using var bloqueo = new Mutex(false, "GuerraCartas-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(directorio)))));
        try { bloqueo.WaitOne(); } catch (AbandonedMutexException) { }
        try { Escribir(Ruta(servidor), new ArchivoPerfiles(2, cambio(Perfiles(servidor)))); }
        finally { bloqueo.ReleaseMutex(); }
    }
    internal void GuardarConexion(Uri servidor) => Escribir(Path.Combine(directorio, "conexion.json"), new PreferenciasConexion(servidor.AbsoluteUri));
    internal PreferenciasConexion Conexion()
    {
        var valor = LeerArchivo<PreferenciasConexion>(Path.Combine(directorio, "conexion.json")) ?? new();
        if (!ConfiguracionCliente.EsUrl(valor.Direccion)) throw new IOException("Dirección guardada inválida.");
        return valor;
    }
    private void Escribir<T>(string ruta, T valor)
    {
        Preparar();
        var temporal = Path.Combine(directorio, Guid.NewGuid() + ".tmp");
        try
        {
            var opciones = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write, Share = FileShare.None };
            if (!OperatingSystem.IsWindows()) opciones.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
            using (var archivo = new FileStream(temporal, opciones))
            { JsonSerializer.Serialize(archivo, valor); archivo.Flush(true); }
            File.Move(temporal, ruta, true);
        }
        catch (UnauthorizedAccessException) { throw new IOException("No se puede guardar el acceso local."); }
        finally { if (File.Exists(temporal)) File.Delete(temporal); }
    }
}
