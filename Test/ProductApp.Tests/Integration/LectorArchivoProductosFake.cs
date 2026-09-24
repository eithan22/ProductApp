using ProductApp.Domian.Common.Importacion;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Tests.Integration
{
    // Sustituye a LectorArchivoProductos (ClosedXML/CsvHelper): los tests no arman archivos
    // .xlsx reales, inyectan directamente el resultado de la lectura. Lo que se prueba acá
    // es la orquestación del servicio, no el parseo del archivo.
    internal class LectorArchivoProductosFake : ILectorArchivoProductos
    {
        private readonly ResultadoLecturaArchivoProductos _resultado;

        public LectorArchivoProductosFake(ResultadoLecturaArchivoProductos resultado) => _resultado = resultado;

        public static LectorArchivoProductosFake ConFilas(params FilaArchivoProductos[] filas)
            => new(ResultadoLecturaArchivoProductos.Leido(filas));

        // maximoFilas se ignora: el fake inyecta el resultado ya armado, el corte real se
        // prueba en el lector de verdad, no acá.
        public ResultadoLecturaArchivoProductos Leer(Stream contenido, string nombreArchivo, int maximoFilas) => _resultado;
    }
}
