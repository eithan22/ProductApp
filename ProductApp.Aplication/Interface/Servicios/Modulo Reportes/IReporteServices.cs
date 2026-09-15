using ProductApp.Aplication.Dtos.ReporteDto;
using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface
{
    public interface IReporteServices
    {
        Task<OperationResultD<List<VentaPorFechaDto>>> ObtenerVentasPorFechaAsync(DateTime? desde, DateTime? hasta);
        Task<OperationResultD<List<VentaPorProductoDto>>> ObtenerVentasPorProductoAsync(DateTime? desde, DateTime? hasta);
        Task<OperationResultD<List<VentaPorVendedorDto>>> ObtenerVentasPorVendedorAsync(DateTime? desde, DateTime? hasta, int? usuarioId, int usuarioAutenticadoId, bool esAdministrador);
        Task<OperationResultD<List<InventarioActualDto>>> ObtenerInventarioActualAsync();
        Task<OperationResultD<List<ProductoMasVendidoDto>>> ObtenerProductosMasVendidosAsync(DateTime? desde, DateTime? hasta, int top);
        Task<OperationResultD<IngresosTotalesDto>> ObtenerIngresosTotalesAsync(DateTime? desde, DateTime? hasta);

        // Exportación a CSV (RF-3.5). Cada método reutiliza su Obtener* de arriba, así
        // el filtro de fechas y las validaciones son exactamente los mismos que en pantalla.
        Task<OperationResultD<byte[]>> ExportarVentasPorFechaCsvAsync(DateTime? desde, DateTime? hasta);
        Task<OperationResultD<byte[]>> ExportarVentasPorProductoCsvAsync(DateTime? desde, DateTime? hasta);
        Task<OperationResultD<byte[]>> ExportarVentasPorVendedorCsvAsync(DateTime? desde, DateTime? hasta, int? usuarioId, int usuarioAutenticadoId, bool esAdministrador);
        Task<OperationResultD<byte[]>> ExportarInventarioActualCsvAsync();
        Task<OperationResultD<byte[]>> ExportarProductosMasVendidosCsvAsync(DateTime? desde, DateTime? hasta, int top);
        Task<OperationResultD<byte[]>> ExportarIngresosTotalesCsvAsync(DateTime? desde, DateTime? hasta);
    }
}
