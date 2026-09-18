using ProductApp.Aplication.Common;
using Web.Models.Modelo_Ventas.PagoModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Ventas
{
    public interface IPagoHttpServices
    {
        Task<PagoModel> RegistrarPagoAsync(CreatePagoModel model);
        Task<List<PagoModel>> GetPagosPorOrdenAsync(int ordenId);
        Task<decimal> GetSaldoPendienteAsync(int ordenId);
        Task<PagoListadoModel> GetPagosAsync(
            int? ordenId = null,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? metodoPago = null,
            int pageNumber = 1,
            int pageSize = PaginacionDefaults.PageSizeDefault);
    }
}
