using Web.Models.Modelo_Reportes.ReporteModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Reportes;
using Web.Services.Interfaces.ServicesHttp.Modulo_Reportes;

namespace Web.Services.ServicesHttp.Modulo_Reportes
{
    public class ReporteHttpServices : IReporteHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IReporteEndpoint _reporteEndpoint;

        public ReporteHttpServices(IBaseHttpServices baseHttpServices, IReporteEndpoint reporteEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _reporteEndpoint = reporteEndpoint;
        }

        public async Task<List<VentaPorFechaModel>> GetVentasPorFechaAsync(DateTime? desde, DateTime? hasta)
        {
            return await _baseHttpServices.GetAsync<List<VentaPorFechaModel>>($"{_reporteEndpoint.VentasPorFecha}{BuildQuery(desde, hasta)}");
        }

        public async Task<List<VentaPorProductoModel>> GetVentasPorProductoAsync(DateTime? desde, DateTime? hasta)
        {
            return await _baseHttpServices.GetAsync<List<VentaPorProductoModel>>($"{_reporteEndpoint.VentasPorProducto}{BuildQuery(desde, hasta)}");
        }

        public async Task<List<VentaPorVendedorModel>> GetVentasPorVendedorAsync(DateTime? desde, DateTime? hasta, int? usuarioId)
        {
            var query = BuildQuery(desde, hasta, ("usuarioId", usuarioId?.ToString()));
            return await _baseHttpServices.GetAsync<List<VentaPorVendedorModel>>($"{_reporteEndpoint.VentasPorVendedor}{query}");
        }

        public async Task<List<InventarioActualModel>> GetInventarioActualAsync()
        {
            return await _baseHttpServices.GetAsync<List<InventarioActualModel>>(_reporteEndpoint.InventarioActual);
        }

        public async Task<List<ProductoMasVendidoModel>> GetProductosMasVendidosAsync(DateTime? desde, DateTime? hasta, int top)
        {
            var query = BuildQuery(desde, hasta, ("top", top.ToString()));
            return await _baseHttpServices.GetAsync<List<ProductoMasVendidoModel>>($"{_reporteEndpoint.ProductosMasVendidos}{query}");
        }

        public async Task<IngresosTotalesModel> GetIngresosTotalesAsync(DateTime? desde, DateTime? hasta)
        {
            return await _baseHttpServices.GetAsync<IngresosTotalesModel>($"{_reporteEndpoint.IngresosTotales}{BuildQuery(desde, hasta)}");
        }

        private static string BuildQuery(DateTime? desde, DateTime? hasta, params (string Key, string? Value)[] extra)
        {
            var parts = new List<string>();

            if (desde.HasValue)
                parts.Add($"desde={desde.Value:yyyy-MM-dd}");

            if (hasta.HasValue)
                parts.Add($"hasta={hasta.Value:yyyy-MM-dd}");

            foreach (var (key, value) in extra)
                if (!string.IsNullOrEmpty(value))
                    parts.Add($"{key}={value}");

            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }
    }
}
