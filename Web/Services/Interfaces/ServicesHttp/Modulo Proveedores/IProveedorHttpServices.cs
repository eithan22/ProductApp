using ProductApp.Aplication.Common;
using Web.Models.Modelo_Proveedores.ProveedorModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Proveedores
{
    public interface IProveedorHttpServices
    {
        Task<PagedResult<ProveedorModel>> GetProveedoresAsync(bool incluirInactivos = false, int pageNumber = 1, int pageSize = PaginacionDefaults.PageSizeDefault);

        // Lista plana para los desplegables (producto, filtro de inventario):
        // solo los activos, porque a un proveedor dado de baja no se le asignan productos nuevos.
        Task<List<ProveedorModel>> GetProveedoresActivosAsync();

        Task<ProveedorModel> GetProveedorByIdAsync(int id);

        Task<ProveedorModel> CreateProveedorAsync(CreateProveedorModel model);

        Task<ProveedorModel> UpdateProveedorAsync(UpdateProveedorModel model);

        Task<List<ProveedorModel>> BuscarProveedoresAsync(string? nombre, bool incluirInactivos = false);

        Task<bool> DisableProveedorAsync(int id);

        Task<bool> EnableProveedorAsync(int id);
    }
}
