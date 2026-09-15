using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Proveedores
{
    public interface IMapperProveedor
    {
        ProveedorResponseDto MapToProveedorResponseDto(Proveedor proveedor);
        Proveedor MapToCreateProveedor(CreateProveedorDto dto);
        void MapToUpdateProveedor(UpdateProveedorDto dto, Proveedor proveedor);
    }
}
