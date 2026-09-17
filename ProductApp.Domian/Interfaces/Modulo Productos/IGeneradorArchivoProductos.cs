using ProductApp.Domian.Common.Importacion;

namespace ProductApp.Domian.Interfaces
{
    // Construye los dos binarios .xlsx de la importación: la plantilla vacía y el reporte de
    // filas rechazadas. La librería de escritura queda encapsulada en Infraestructura.
    public interface IGeneradorArchivoProductos
    {
        byte[] GenerarPlantilla();

        byte[] GenerarReporteErrores(
            IReadOnlyList<FilaErroneaArchivoProductos> filasConError,
            IReadOnlyList<AdvertenciaArchivoProductos> advertencias);
    }
}
