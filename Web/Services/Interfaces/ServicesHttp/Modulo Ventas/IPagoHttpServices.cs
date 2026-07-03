using Web.Models.Modelo_Ventas.PagoModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Ventas
{
    public interface IPagoHttpServices
    {
        Task<PagoModel> RegistrarPagoAsync(CreatePagoModel model);
        Task<List<PagoModel>> GetPagosPorOrdenAsync(int ordenId);
        Task<decimal> GetSaldoPendienteAsync(int ordenId);
    }
}
