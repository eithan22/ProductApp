using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using Web.Models.Modelo_Proveedores.ProveedorModels;

namespace Web.Services.Mappers.Modulo_Proveedores
{
    public class ProveedorMapperM
    {
        public static CreateProveedorDto MapAddProveedorDto(CreateProveedorModel model)
        {
            return new CreateProveedorDto
            {
                Nombre = model.Nombre,
                Telefono = model.Telefono,
                Correo = model.Correo,
                Direccion = model.Direccion
            };
        }

        public static UpdateProveedorDto MapUpdateProveedorDto(UpdateProveedorModel model)
        {
            return new UpdateProveedorDto
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Telefono = model.Telefono,
                Correo = model.Correo,
                Direccion = model.Direccion
            };
        }
    }
}
