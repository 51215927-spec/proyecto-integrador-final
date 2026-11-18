namespace Proyecto_Integrador_Final.Api;

internal sealed record AltaPersonaje(
    string? Nombre, string? Descripcion, string? Ascii,
    int Fuerza, int Velocidad, int Resistencia, int Inteligencia, int Energia, int Habilidad)
{
    internal PersonajeCatalogo Validar()
    {
        if (Descripcion is null || Ascii is null || Ascii.Contains('\t'))
            throw ErrorApi.Entrada();

        var lineas = Ascii.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (lineas.Length > 25 || lineas.Any(linea => linea.Length > 120))
            throw ErrorApi.Entrada();

        try
        {
            _ = new Personaje(Nombre!, Ascii, Fuerza, Velocidad, Resistencia, Inteligencia, Energia, Habilidad);
        }
        catch (ArgumentException)
        {
            throw ErrorApi.Entrada();
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
