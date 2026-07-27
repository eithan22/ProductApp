using Web.Models.Modelo_Ventas.OrdenModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Ventas
{
    public interface IOrdenHttpServices
    {
        Task<OrdenModel> CreateOrdenAsync(CreateOrdenModel model);
        Task<List<OrdenModel>> GetOrdenesAsync(string? estado = null);
        Task<OrdenModel> GetOrdenByIdAsync(int id);
        Task<List<OrdenModel>> GetOrdenesByClienteAsync(int clienteId, string? estado = null);
        Task<List<OrdenModel>> GetOrdenesByUsuarioAsync(int usuarioId);
        Task<List<OrdenModel>> GetOrdenesByFechaAsync(DateTime fecha, string? estado = null);
        Task<bool> CambiarEstadoAsync(CambiarEstadoOrdenModel model);
        Task<bool> CancelarOrdenAsync(int id);
        Task<bool> ConfirmarOrdenAsync(int id);
    }
}
