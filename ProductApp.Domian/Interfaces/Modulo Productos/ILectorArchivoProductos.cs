using ProductApp.Domian.Common.Importacion;

namespace ProductApp.Domian.Interfaces
{
    // Lee el archivo de importación. Las librerías de parseo (ClosedXML para .xlsx,
    // CsvHelper para .csv) quedan encapsuladas en Infraestructura: ni Application ni la Api
    // las conocen. Mismo patrón que IGeneradorFacturaPdf con QuestPDF.
    public interface ILectorArchivoProductos
    {
        // Síncrono a propósito: el archivo se copia a memoria antes de parsear (lo exige
        // ClosedXML, que necesita un stream seekable), así que no hay I/O real que esperar.
        ResultadoLecturaArchivoProductos Leer(Stream contenido, string nombreArchivo);
    }
}
