using System.Text.Json;

namespace Proyecto_Integrador_Final.Api;

internal static class LotePersonajes
{
    internal static AltaPersonaje[] Leer(JsonElement raiz)
    {
        if (raiz.ValueKind != JsonValueKind.Array || raiz.GetArrayLength() == 0)
            throw Invalido("La raíz debe ser una lista con al menos un personaje.");
        var resultado = new List<AltaPersonaje>();
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var objeto in raiz.EnumerateArray())
        {
            string ubicacion = $"Personaje {resultado.Count + 1}";
            if (objeto.ValueKind != JsonValueKind.Object) throw Invalido($"{ubicacion}: se esperaba un objeto.");
            if (objeto.TryGetProperty("nombre", out var nombre) && nombre.ValueKind == JsonValueKind.String)
                ubicacion += $" ({nombre.GetString()})";
            string Texto(string campo)
            {
                if (!objeto.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.String)
                    throw Invalido($"{ubicacion}: {campo} debe ser texto y es obligatorio.");
                return valor.GetString()!;
            }
            int Entero(string campo)
            {
                if (!objeto.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.Number || !valor.TryGetInt32(out int numero))
                    throw Invalido($"{ubicacion}: {campo} debe ser un entero y es obligatorio.");
                return numero;
            }
            if (objeto.EnumerateObject().Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("idPersonaje", StringComparison.OrdinalIgnoreCase)))
                throw Invalido($"{ubicacion}: el ID lo asigna el servidor.");
            var datos = new AltaPersonaje(Texto("nombre"), Texto("descripcion"), Texto("ascii"),
                Entero("fuerza"), Entero("velocidad"), Entero("resistencia"), Entero("inteligencia"), Entero("energia"), Entero("habilidad"));
            if (!nombres.Add(datos.Nombre!.Trim())) throw Invalido($"{ubicacion}: nombre duplicado.");
            resultado.Add(datos);
        }
        return resultado.ToArray();
    }

    internal static ErrorApi Invalido(string mensaje) => new(400, "entradaInvalida", mensaje);
}
