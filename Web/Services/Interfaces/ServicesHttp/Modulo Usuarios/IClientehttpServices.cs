using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.ClienteDto;
using Web.Models.ClienteModels;

namespace Web.Services.Interfaces.ServicesHttp
{

      public interface IClienteHttpServices
        {
            Task<PagedResult<ClienteModel>> GetClientesAsync(bool incluirInactivos = false, int pageNumber = 1, int pageSize = 10);

            Task<ClienteModel> GetClienteByIdAsync(int id);

            Task<ClienteModel> CreateClienteAsync(CreateClienteModel model);

            Task<ClienteModel> UpdateClienteAsync(UpdateClientemodel model);

            Task<List<ClienteModel>> GetBuscarClienteAsync(string? nombre, string? telefono, string? correo);



            Task<bool> DeleteClienteAsync(int id);

            Task<bool> EnableClienteAsync(int id);
        }



    }

