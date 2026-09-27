using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Proveedores;
using ProductApp.Domian.Common.Enums.EnumsProveedor;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Mappers.Modulo_Proveedores
{
    public class ProveedorMapper : IMapperProveedor
    {
        public ProveedorResponseDto MapToProveedorResponseDto(Proveedor proveedor)
        {
            var proveedorResponse = new ProveedorResponseDto
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = proveedor.Telefono,
                Correo = proveedor.Correo,
                Direccion = proveedor.Direccion,
                Activo = proveedor.Estado == EstadoProveedor.Activo
            };

            return proveedorResponse;
        }

        public Proveedor MapToCreateProveedor(CreateProveedorDto dto)
        {
            return new Proveedor(
                dto.Nombre,
                dto.Telefono,
                dto.Correo,
                dto.Direccion
                );
        }

        public void MapToUpdateProveedor(UpdateProveedorDto dto, Proveedor proveedor)
        {
            proveedor.CambiarYvalidarNombre(dto.Nombre);
            proveedor.CambiarYvalidarTelefono(dto.Telefono);
            proveedor.CambiarYvalidarCorreo(dto.Correo);
            proveedor.CambiarYvalidarDireccion(dto.Direccion);
        }
    }
}
