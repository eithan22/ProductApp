using Web.Models.Modelo_Ventas.DetalleOrdenModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Ventas
{
    public interface IDetalleOrdenHttpServices
    {
        Task<DetalleOrdenModel> AgregarProductoAsync(CreateDetalleOrdenModel model);
        Task<List<DetalleOrdenModel>> GetDetallesPorOrdenAsync(int ordenId);
        Task<DetalleOrdenModel> ActualizarCantidadAsync(UpdateDetalleOrdenModel model);
        Task<bool> EliminarProductoAsync(int id);
    }
}
