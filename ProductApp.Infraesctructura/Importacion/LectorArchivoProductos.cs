using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using ProductApp.Domian.Common.Importacion;
using ProductApp.Domian.Interfaces;
using System.Globalization;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Importacion
{
    public class LectorArchivoProductos : ILectorArchivoProductos
    {
        // Orden en el que se escriben en la plantilla. El lector NO depende de este orden:
        // ubica cada columna por su encabezado, así que el usuario puede reordenarlas.
        public static readonly string[] ColumnasRequeridas =
            { "Nombre", "Descripcion", "Precio", "Costo", "Categoria" };

        public const string ColumnaProveedor = "Proveedor";

        public ResultadoLecturaArchivoProductos Leer(Stream contenido, string nombreArchivo, int maximoFilas)
        {
            var extension = Path.GetExtension(nombreArchivo ?? string.Empty).ToLowerInvariant();

            // ClosedXML necesita un stream seekable y el de IFormFile no siempre lo es.
            // Copiarlo a memoria también permite reintentar el parseo sin re-leer la request.
            using var buffer = new MemoryStream();
            contenido.CopyTo(buffer);
            buffer.Position = 0;

            return extension switch
            {
                ".xlsx" => LeerExcel(buffer, maximoFilas),
                ".csv" => LeerCsv(buffer, maximoFilas),
                _ => ResultadoLecturaArchivoProductos.FormatoNoSoportado()
            };
        }

        // --- .xlsx (ClosedXML) ---

        private static ResultadoLecturaArchivoProductos LeerExcel(Stream contenido, int maximoFilas)
        {
            using var libro = new XLWorkbook(contenido);

            var hoja = libro.Worksheets.FirstOrDefault();
            if (hoja == null)
                return ResultadoLecturaArchivoProductos.Leido(Array.Empty<FilaArchivoProductos>());

            var filaEncabezados = hoja.FirstRowUsed();
            if (filaEncabezados == null)
                return ResultadoLecturaArchivoProductos.Leido(Array.Empty<FilaArchivoProductos>());

            var columnas = new Dictionary<string, int>();
            foreach (var celda in filaEncabezados.CellsUsed())
                columnas[Clave(celda.GetString())] = celda.Address.ColumnNumber;

            var faltantes = ColumnasFaltantes(columnas.Keys);
            if (faltantes.Count > 0)
                return ResultadoLecturaArchivoProductos.FaltanColumnas(faltantes);

            var filas = new List<FilaArchivoProductos>();

            foreach (var fila in hoja.RowsUsed().Where(r => r.RowNumber() > filaEncabezados.RowNumber()))
            {
                if (fila.IsEmpty())
                    continue;

                // Con una fila por encima del tope, Application ya tiene lo que necesita para
                // rechazar el archivo. Seguir parseando el resto para tirarlo es desperdicio.
                if (filas.Count > maximoFilas)
                    break;

                filas.Add(new FilaArchivoProductos
                {
                    NumeroFila = fila.RowNumber(),
                    Nombre = ValorCelda(fila, columnas, "Nombre"),
                    Descripcion = ValorCelda(fila, columnas, "Descripcion"),
                    Precio = ValorCelda(fila, columnas, "Precio"),
                    Costo = ValorCelda(fila, columnas, "Costo"),
                    Categoria = ValorCelda(fila, columnas, "Categoria"),
                    Proveedor = ValorCelda(fila, columnas, ColumnaProveedor)
                });
            }

            return ResultadoLecturaArchivoProductos.Leido(filas);
        }

        private static string? ValorCelda(IXLRow fila, IReadOnlyDictionary<string, int> columnas, string encabezado)
        {
            if (!columnas.TryGetValue(Clave(encabezado), out var numeroColumna))
                return null;

            var celda = fila.Cell(numeroColumna);
            if (celda.IsEmpty())
                return null;

            // Una celda numérica se convierte con cultura invariante: si se dejara el
            // ToString() por defecto, en un servidor con cultura es-DO "8450.50" saldría
            // "8450,50" y la capa de aplicación lo rechazaría como no numérico.
            if (celda.DataType == XLDataType.Number)
                return celda.GetDouble().ToString(CultureInfo.InvariantCulture);

            return celda.GetString().Trim();
        }

        // --- .csv (CsvHelper) ---

        private static ResultadoLecturaArchivoProductos LeerCsv(Stream contenido, int maximoFilas)
        {
            // detectEncodingFromByteOrderMarks: los CSV que exporta este mismo sistema
            // (GeneradorCsv) llevan BOM UTF-8; sin esto las tildes y la 'ñ' se corromperían.
            using var lector = new StreamReader(contenido, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var configuracion = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // El proyecto exporta con ';' (formato Excel en español), pero un archivo
                // traído de otra herramienta puede venir con ','. Se detecta y ';' es el
                // fallback si la detección no concluye.
                DetectDelimiter = true,
                Delimiter = ";",
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var csv = new CsvReader(lector, configuracion);

            if (!csv.Read())
                return ResultadoLecturaArchivoProductos.Leido(Array.Empty<FilaArchivoProductos>());

            csv.ReadHeader();

            var encabezados = csv.HeaderRecord ?? Array.Empty<string>();
            var columnas = new Dictionary<string, int>();
            for (var i = 0; i < encabezados.Length; i++)
                columnas[Clave(encabezados[i])] = i;

            var faltantes = ColumnasFaltantes(columnas.Keys);
            if (faltantes.Count > 0)
                return ResultadoLecturaArchivoProductos.FaltanColumnas(faltantes);

            var filas = new List<FilaArchivoProductos>();

            while (csv.Read())
            {
                var registro = csv.Parser.Record;
                if (registro == null || registro.All(string.IsNullOrWhiteSpace))
                    continue;

                // Mismo corte que en .xlsx. Acá pesa más: CsvHelper lee en streaming, así que
                // sin esto un .csv de 5 MB materializa ~100.000 filas para descartarlas enteras.
                if (filas.Count > maximoFilas)
                    break;

                filas.Add(new FilaArchivoProductos
                {
                    // Parser.Row cuenta el encabezado como fila 1, igual que Excel: así el
                    // número que se le reporta al usuario significa lo mismo en los dos formatos.
                    NumeroFila = csv.Parser.Row,
                    Nombre = ValorRegistro(registro, columnas, "Nombre"),
                    Descripcion = ValorRegistro(registro, columnas, "Descripcion"),
                    Precio = ValorRegistro(registro, columnas, "Precio"),
                    Costo = ValorRegistro(registro, columnas, "Costo"),
                    Categoria = ValorRegistro(registro, columnas, "Categoria"),
                    Proveedor = ValorRegistro(registro, columnas, ColumnaProveedor)
                });
            }

            return ResultadoLecturaArchivoProductos.Leido(filas);
        }

        private static string? ValorRegistro(string[] registro, IReadOnlyDictionary<string, int> columnas, string encabezado)
        {
            if (!columnas.TryGetValue(Clave(encabezado), out var indice) || indice >= registro.Length)
                return null;

            var valor = registro[indice]?.Trim();
            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        // --- Común ---

        private static List<string> ColumnasFaltantes(IEnumerable<string> encabezadosPresentes)
        {
            var presentes = new HashSet<string>(encabezadosPresentes);
            return ColumnasRequeridas.Where(c => !presentes.Contains(Clave(c))).ToList();
        }

        // Los encabezados se comparan sin mayúsculas, sin espacios y sin tildes: un archivo
        // con "CATEGORÍA" o "descripcion " sigue siendo válido. Las columnas que sobran
        // (ej. "Fila" y "Motivo" del reporte de errores) simplemente se ignoran, y por eso
        // ese reporte se puede corregir y volver a subir tal cual.
        private static string Clave(string? encabezado)
        {
            if (string.IsNullOrWhiteSpace(encabezado))
                return string.Empty;

            var descompuesto = encabezado.Trim().Normalize(NormalizationForm.FormD);
            var limpio = new StringBuilder(descompuesto.Length);

            foreach (var caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (!char.IsWhiteSpace(caracter))
                    limpio.Append(char.ToLowerInvariant(caracter));
            }

            return limpio.ToString();
        }
    }
}
