using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface
{
    public interface IProveedorServices : IBaseServices<ProveedorResponseDto, CreateProveedorDto, UpdateProveedorDto>
    {
        Task<OperationResultD<PagedResult<ProveedorResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = PaginacionDefaults.PageSizeDefault);

        Task<OperationResultD<bool>> EnableProveedor(int id);

        Task<OperationResultD<List<ProveedorResponseDto>>> BuscarAsync(string? nombre, bool incluirInactivos = false);
    }
}
