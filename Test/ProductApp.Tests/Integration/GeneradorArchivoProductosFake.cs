using ProductApp.Domian.Common.Importacion;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Tests.Integration
{
    internal class GeneradorArchivoProductosFake : IGeneradorArchivoProductos
    {
        public IReadOnlyList<FilaErroneaArchivoProductos>? UltimasFilasConError { get; private set; }
        public IReadOnlyList<AdvertenciaArchivoProductos>? UltimasAdvertencias { get; private set; }
        public int VecesQueGeneroReporteErrores { get; private set; }

        public byte[] GenerarPlantilla() => new byte[] { 1, 2, 3 };

        public byte[] GenerarReporteErrores(
            IReadOnlyList<FilaErroneaArchivoProductos> filasConError,
            IReadOnlyList<AdvertenciaArchivoProductos> advertencias)
        {
            UltimasFilasConError = filasConError;
            UltimasAdvertencias = advertencias;
            VecesQueGeneroReporteErrores++;
            return new byte[] { 9, 9, 9 };
        }
    }
}
