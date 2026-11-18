using DataBases;

namespace Proyecto_Integrador_Final.Api;

internal sealed record FilaCatalogo(
    int Id, string Nombre, string Descripcion, string Ascii, int Fuerza,
    int Velocidad, int Resistencia, int Inteligencia, int Energia, int Habilidad)
    : IEntidad<FilaCatalogo>
{
    // ConstructorModelo invoca Configurar sin argumentos. La ruta se usa solo
    // al construir el modelo y queda fijada en esa instancia.
    private static readonly object configuracion = new();
    private static string? rutaDeCarga;

    internal static PersonajeCatalogo[] Leer(string ruta)
    {
        Modelo modelo;
        lock (configuracion)
        {
            rutaDeCarga = ruta;
            try
            {
                modelo = new ConstructorModelo().Agregar<FilaCatalogo>().Construir();
            }
            finally
            {
                rutaDeCarga = null;
            }
        }

        var lote = new List<PersonajeCatalogo>();
        var ids = new HashSet<int>();
        int registro = 0;
        foreach (var fila in modelo.Cargar<FilaCatalogo>(exigirArchivo: true))
        {
            registro++;
            if (!ids.Add(fila.Id))
                throw new CsvException($"Registro {registro}: ID duplicado ({fila.Id}).");

            try
            {
                var datos = new AltaPersonaje(fila.Nombre, fila.Descripcion, fila.Ascii,
                    fila.Fuerza, fila.Velocidad, fila.Resistencia, fila.Inteligencia, fila.Energia, fila.Habilidad);
                var personaje = datos.Validar();
                personaje.Id = fila.Id;
                lote.Add(personaje);
            }
            catch (ErrorApi error)
            {
                throw new CsvException($"Registro {registro}: personaje inválido (ID {fila.Id}).", error);
            }
        }
        return lote.ToArray();
    }

    public static void Configurar(ConstructorEntidad<FilaCatalogo> entidad) => entidad
        .Archivo(rutaDeCarga ?? throw new InvalidOperationException("Falta la ruta del catálogo."))
        .Columna(p => p.Id, "id")
        .Columna(p => p.Nombre, "nombre")
        .Columna(p => p.Descripcion, "descripcion")
        .Columna(p => p.Ascii, "ascii")
        .Columna(p => p.Fuerza, "fuerza")
        .Columna(p => p.Velocidad, "velocidad")
        .Columna(p => p.Resistencia, "resistencia")
        .Columna(p => p.Inteligencia, "inteligencia")
        .Columna(p => p.Energia, "energia")
        .Columna(p => p.Habilidad, "habilidad");
}
