using ClosedXML.Excel;
using ProductApp.Domian.Common.Importacion;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Infraesctructura.Persistencia.Importacion
{
    public class GeneradorArchivoProductosClosedXml : IGeneradorArchivoProductos
    {
        private const string HojaProductos = "Productos";
        private const string HojaAdvertencias = "Advertencias";

        // Mismo orden que documenta el SDD 3.3 y que muestra la pantalla de importación.
        private static readonly string[] EncabezadosPlantilla =
            { "Nombre", "Descripcion", "Precio", "Costo", "Categoria", "Proveedor" };

        public byte[] GenerarPlantilla()
        {
            using var libro = new XLWorkbook();
            var hoja = libro.AddWorksheet(HojaProductos);

            for (var i = 0; i < EncabezadosPlantilla.Length; i++)
                hoja.Cell(1, i + 1).Value = EncabezadosPlantilla[i];

            AplicarEstiloEncabezados(hoja, EncabezadosPlantilla.Length);

            return AGuardarEnMemoria(libro);
        }

        public byte[] GenerarReporteErrores(
            IReadOnlyList<FilaErroneaArchivoProductos> filasConError,
            IReadOnlyList<AdvertenciaArchivoProductos> advertencias)
        {
            using var libro = new XLWorkbook();
            var hoja = libro.AddWorksheet(HojaProductos);

            // "Fila" primero y "Columna"/"Motivo" al final, con los datos originales en medio:
            // el usuario corrige el valor donde ya está y vuelve a subir ESTE mismo archivo.
            // El lector ubica columnas por nombre e ignora las que sobran, así que funciona.
            var encabezados = new[] { "Fila" }
                .Concat(EncabezadosPlantilla)
                .Concat(new[] { "Columna", "Motivo del error" })
                .ToArray();

            for (var i = 0; i < encabezados.Length; i++)
                hoja.Cell(1, i + 1).Value = encabezados[i];

            var numeroFila = 2;
            foreach (var error in filasConError)
            {
                hoja.Cell(numeroFila, 1).Value = error.Fila.NumeroFila;
                hoja.Cell(numeroFila, 2).Value = error.Fila.Nombre ?? string.Empty;
                hoja.Cell(numeroFila, 3).Value = error.Fila.Descripcion ?? string.Empty;
                hoja.Cell(numeroFila, 4).Value = error.Fila.Precio ?? string.Empty;
                hoja.Cell(numeroFila, 5).Value = error.Fila.Costo ?? string.Empty;
                hoja.Cell(numeroFila, 6).Value = error.Fila.Categoria ?? string.Empty;
                hoja.Cell(numeroFila, 7).Value = error.Fila.Proveedor ?? string.Empty;
                hoja.Cell(numeroFila, 8).Value = error.Columna;
                hoja.Cell(numeroFila, 9).Value = error.Motivo;
                numeroFila++;
            }

            AplicarEstiloEncabezados(hoja, encabezados.Length);

            // Las advertencias van en una hoja aparte y solo si existen: son filas que SÍ se
            // crearon, mezclarlas con los errores haría creer que también fallaron.
            if (advertencias.Count > 0)
            {
                var hojaAvisos = libro.AddWorksheet(HojaAdvertencias);
                hojaAvisos.Cell(1, 1).Value = "Fila";
                hojaAvisos.Cell(1, 2).Value = "Columna";
                hojaAvisos.Cell(1, 3).Value = "Advertencia";

                var filaAviso = 2;
                foreach (var advertencia in advertencias)
                {
                    hojaAvisos.Cell(filaAviso, 1).Value = advertencia.NumeroFila;
                    hojaAvisos.Cell(filaAviso, 2).Value = advertencia.Columna;
                    hojaAvisos.Cell(filaAviso, 3).Value = advertencia.Mensaje;
                    filaAviso++;
                }

                AplicarEstiloEncabezados(hojaAvisos, 3);
            }

            return AGuardarEnMemoria(libro);
        }

        private static void AplicarEstiloEncabezados(IXLWorksheet hoja, int cantidadColumnas)
        {
            hoja.Range(1, 1, 1, cantidadColumnas).Style.Font.Bold = true;
            hoja.SheetView.FreezeRows(1);
            hoja.Columns().AdjustToContents();
        }

        private static byte[] AGuardarEnMemoria(XLWorkbook libro)
        {
            using var memoria = new MemoryStream();
            libro.SaveAs(memoria);
            return memoria.ToArray();
        }
    }
}
