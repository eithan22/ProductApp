using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IOrdenRepository : IGenericRepository<Orden>
    {
        Task<List<Orden>> GetAllConDetallesAsync(EstadoOrden? estado = null);
        Task<List<Orden>> ObtenerPorClienteAsync(int clienteId, EstadoOrden? estado = null);
        Task<List<Orden>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<List<Orden>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, EstadoOrden? estado = null);
        Task<Orden?> GetByIdConClienteAsync(int id);

        // Búsqueda global (RF-3.8.1). Devuelve la orden junto a los dos datos derivados que
        // el dropdown de resultados necesita, para no tener que ir al repositorio de pagos
        // ni al de detalles una vez por cada fila encontrada.
        Task<List<(Orden Orden, int CantidadProductos, decimal TotalPagado)>> BuscarOrdenesAsync(string texto);
    }
}
