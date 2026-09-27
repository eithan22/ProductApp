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
        //
        // maximoFilas: el tope que aplica la capa de aplicación. Se pasa acá para que el
        // lector deje de parsear al superarlo en vez de materializar el archivo entero para
        // que después lo rechacen. Lee hasta maximoFilas + 1 a propósito: con una fila de más
        // ya se sabe que el archivo se pasó, y eso es todo lo que necesita el servicio.
        ResultadoLecturaArchivoProductos Leer(Stream contenido, string nombreArchivo, int maximoFilas);
    }
}
