namespace Proyecto_Integrador_Final.Api;

internal sealed record AltaPersonaje(
    string? Nombre, string? Descripcion, string? Ascii,
    int Fuerza, int Velocidad, int Resistencia, int Inteligencia, int Energia, int Habilidad)
{
    internal PersonajeCatalogo Validar()
    {
        if (Descripcion is null) throw LotePersonajes.Invalido("descripcion: debe ser texto no nulo.");
        if (Ascii is null) throw LotePersonajes.Invalido("ascii: debe ser texto no nulo.");
        if (Ascii.Contains('\t')) throw LotePersonajes.Invalido("ascii: no admite tabulaciones.");
        var lineas = Ascii.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (lineas.Length > 25 || lineas.Any(linea => linea.Length > 120))
            throw LotePersonajes.Invalido("ascii: admite hasta 25 líneas de 120 caracteres.");

        try
        {
            _ = new Personaje(Nombre!, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);
        }
        catch (ArgumentException error)
        {
            throw LotePersonajes.Invalido($"{error.ParamName}: {error.Message}");
        }

        return new()
        {
            Nombre = Nombre!,
            Descripcion = Descripcion,
            Ascii = Ascii,
            Fuerza = Fuerza,
            Velocidad = Velocidad,
            Resistencia = Resistencia,
            Inteligencia = Inteligencia,
            Energia = Energia,
            Habilidad = Habilidad
        };
    }
}

internal sealed class PersonajeCatalogo
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public required string Ascii { get; set; }
    public int Fuerza { get; set; }
    public int Velocidad { get; set; }
    public int Resistencia { get; set; }
    public int Inteligencia { get; set; }
    public int Energia { get; set; }
    public int Habilidad { get; set; }
    public bool Activo { get; set; } = true;
}
