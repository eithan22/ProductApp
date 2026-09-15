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

        // Devuelven también el nombre: lo decide la API y viaja en Content-Disposition.
        Task<(byte[] Contenido, string? NombreArchivo)> ExportarVentasPorFechaCsvAsync(DateTime? desde, DateTime? hasta);
        Task<(byte[] Contenido, string? NombreArchivo)> ExportarVentasPorProductoCsvAsync(DateTime? desde, DateTime? hasta);
        Task<(byte[] Contenido, string? NombreArchivo)> ExportarVentasPorVendedorCsvAsync(DateTime? desde, DateTime? hasta, int? usuarioId);
        Task<(byte[] Contenido, string? NombreArchivo)> ExportarInventarioActualCsvAsync();
        Task<(byte[] Contenido, string? NombreArchivo)> ExportarProductosMasVendidosCsvAsync(DateTime? desde, DateTime? hasta, int top);
        Task<(byte[] Contenido, string? NombreArchivo)> ExportarIngresosTotalesCsvAsync(DateTime? desde, DateTime? hasta);
    }
}
