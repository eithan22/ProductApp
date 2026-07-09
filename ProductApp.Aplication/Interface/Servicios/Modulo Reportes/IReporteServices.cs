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
    }
}
