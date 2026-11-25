using System.Reflection;
using System.Text.Json;

namespace Proyecto_Integrador_Final.Api;

internal static class CatalogoPredeterminado
{
    internal static int CargarSiVacio(CatalogoPersonajes catalogo)
    {
        if (catalogo.CantidadCatalogo() != 0) return 0;
        using var recurso = Assembly.GetExecutingAssembly().GetManifestResourceStream(
            "Proyecto_Integrador_Final.personajes-predeterminados.json")
            ?? throw new InvalidOperationException("No se encontró el catálogo predeterminado.");
        using var json = JsonDocument.Parse(recurso);
        var personajes = LotePersonajes.Leer(json.RootElement);
        // La transacción del catálogo reemplaza el lote solo después de validar todos sus personajes.
        return catalogo.ReemplazarCatalogo(personajes);
    }
}
