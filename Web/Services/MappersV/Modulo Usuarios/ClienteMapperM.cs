using ProductApp.Aplication.Dtos.ClienteDto;
using Web.Models.ClienteModels;

namespace Web.Services.Mappers
{
    public  class ClienteMapperM
    {
        public static CreateClienteDto MapAddClienteDto(CreateClienteModel model)
        {
            return new CreateClienteDto
            {
                Nombre = model.Nombre,
                Correo = model.Correo,
                Telefono = model.Telefono,
                Cedula = model.Cedula,
                Direccion = model.Direccion
            };

        }

        public static UpdateClienteDto MapUpdateClienteDto(UpdateClientemodel model)
        {
            return new UpdateClienteDto
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Correo = model.Correo,
                Telefono = model.Telefono,
                Cedula = model.Cedula,
                Direccion = model.Direccion
            };
        }

    }
}
