using ProductApp.Aplication.Dtos.ClienteDto;
using Web.Models.ClienteModels;

namespace Web.Services.Interfaces.ServicesHttp
{
  
      public interface IClienteHttpServices
        {
            Task<List<ClienteModel>> GetClientesAsync();

            Task<ClienteModel> GetClienteByIdAsync(int id);

            Task<ClienteModel> CreateClienteAsync(CreateClienteModel model);

            Task<ClienteModel> UpdateClienteAsync(UpdateClientemodel model);

            Task<List<ClienteModel>> GetBuscarClienteAsync(string? nombre, string? telefono, string? correo);


                    
            Task<bool> DeleteClienteAsync(int id);
        }



    }

