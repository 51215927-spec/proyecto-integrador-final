using System.Globalization;
using System.Text;

namespace Proyecto_Integrador_Final.Api;

internal static class CatalogoCsv
{
    private static readonly string[] Columnas =
    [
        "id", "nombre", "descripcion", "ascii", "fuerza", "velocidad",
        "resistencia", "inteligencia", "energia", "habilidad"
    ];

    internal static PersonajeCatalogo[] Leer(string ruta)
    {
        using var archivo = File.OpenText(ruta);
        int numeroLinea = 0;
        string? linea;
        do
        {
            linea = archivo.ReadLine();
            numeroLinea++;
        } while (linea is not null && string.IsNullOrWhiteSpace(linea));

        if (linea is null)
            return [];

        var encabezado = LeerCeldas(linea, numeroLinea)
            .Select(celda => celda?.Trim()).ToArray();
        var indices = Columnas.Select(columna => Array.IndexOf(encabezado, columna)).ToArray();
        if (encabezado.Length != Columnas.Length || indices.Contains(-1))
            throw new FormatException($"Línea {numeroLinea}: el encabezado debe contener una vez cada columna del catálogo.");

        var lote = new List<PersonajeCatalogo>();
        var ids = new HashSet<int>();
        while ((linea = archivo.ReadLine()) is not null)
        {
            numeroLinea++;
            if (string.IsNullOrWhiteSpace(linea))
                continue;

            var celdas = LeerCeldas(linea, numeroLinea);
            if (celdas.Count != Columnas.Length)
                throw new FormatException($"Línea {numeroLinea}: se esperaban {Columnas.Length} celdas y se encontraron {celdas.Count}.");

            string? Celda(int columna) => celdas[indices[columna]];
            int Entero(int columna)
            {
                if (int.TryParse(Celda(columna), NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                    return valor;
                throw new FormatException($"Línea {numeroLinea}: la columna {Columnas[columna]} requiere un entero.");
            }

            int id = Entero(0);
            if (!ids.Add(id))
                throw new FormatException($"Línea {numeroLinea}: ID duplicado ({id}).");

            var datos = new AltaPersonaje(Celda(1), Celda(2), Celda(3),
                Entero(4), Entero(5), Entero(6), Entero(7), Entero(8), Entero(9));
            try
            {
                var personaje = datos.Validar();
                personaje.Id = id;
                lote.Add(personaje);
            }
            catch (ErrorApi error)
            {
                throw new FormatException($"Línea {numeroLinea}: personaje inválido (ID {id}).", error);
            }
        }

        return lote.ToArray();
    }

    private static List<string?> LeerCeldas(string linea, int numeroLinea)
    {
        var celdas = new List<string?>();
        var texto = new StringBuilder();
        bool comillasAbiertas = false;
        bool tuvoComillas = false;

        void AgregarCelda()
        {
            var valor = texto.ToString();
            celdas.Add(!tuvoComillas && valor is ("" or "null") ? null : valor);
            texto.Clear();
            tuvoComillas = false;
        }

        for (int i = 0; i < linea.Length; i++)
        {
            char caracter = linea[i];
            switch (caracter)
            {
                case '\\':
                    if (++i == linea.Length)
                        throw new FormatException($"Línea {numeroLinea}: escape incompleto.");
                    texto.Append(linea[i] switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        char literal => literal
                    });
                    break;
                case '"':
                    comillasAbiertas = !comillasAbiertas;
                    tuvoComillas = true;
                    break;
                case ',' when !comillasAbiertas:
                    AgregarCelda();
                    break;
                default:
                    texto.Append(caracter);
                    break;
            }
        }

        if (comillasAbiertas)
            throw new FormatException($"Línea {numeroLinea}: hay comillas sin cerrar.");

        AgregarCelda();
        return celdas;
    }
}
