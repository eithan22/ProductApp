using Web.Models.Modelo_Reportes.ReporteModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Reportes
{
    public interface IReporteHttpServices
    {
        Task<List<VentaPorFechaModel>> GetVentasPorFechaAsync(DateTime? desde, DateTime? hasta);
        Task<List<VentaPorProductoModel>> GetVentasPorProductoAsync(DateTime? desde, DateTime? hasta);
        Task<List<VentaPorVendedorModel>> GetVentasPorVendedorAsync(DateTime? desde, DateTime? hasta, int? usuarioId);
        Task<List<InventarioActualModel>> GetInventarioActualAsync();
        Task<List<ProductoMasVendidoModel>> GetProductosMasVendidosAsync(DateTime? desde, DateTime? hasta, int top);
        Task<IngresosTotalesModel> GetIngresosTotalesAsync(DateTime? desde, DateTime? hasta);
    }
}
