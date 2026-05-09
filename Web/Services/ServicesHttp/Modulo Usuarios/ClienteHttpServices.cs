using ProductApp.Aplication.Dtos.ClienteDto;
using System.Buffers.Text;
using System.Net;
using Web.Models.ClienteModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints;
using Web.Services.Interfaces.ServicesHttp;
using Web.Services.Mappers;


namespace Web.Services.ServicesHttp
{
    public class ClienteHttpServices : IClienteHttpServices

    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IClienteEndpoint _clienteEndpointcs;
        public ClienteHttpServices(IBaseHttpServices baseHttpServices, IClienteEndpoint clienteEndpointcs)
        {
            _baseHttpServices = baseHttpServices;
            _clienteEndpointcs = clienteEndpointcs;
        }

        public async Task<ClienteModel> CreateClienteAsync(CreateClienteModel model)
        {
            // Mapeamos el modelo de creación al DTO correspondiente
            var dto = ClienteMapperM.MapAddClienteDto(model);

            var response = await _baseHttpServices.PostAsync<CreateClienteDto, ClienteModel>(_clienteEndpointcs.Create, dto);
           
            return response;

        }

        public async Task<bool> DeleteClienteAsync(int id)
        {
          var result =  await _baseHttpServices.DeleteAsync($"{_clienteEndpointcs.Delete}{id}");
            return result;
        }




       
        public async Task<List<ClienteModel>> GetBuscarClienteAsync(string? nombre, string? telefono, string? correo)
        {
            var response = await _baseHttpServices.GetAsync<List<ClienteModel>>($"{_clienteEndpointcs.GetBuscar}?nombre={nombre}&telefono={telefono}&correo={correo}");
            return response;
        }

        public async Task<ClienteModel> GetClienteByIdAsync(int id)
        {
                var response =  await _baseHttpServices.GetAsync<ClienteModel>($"{_clienteEndpointcs.GetById}{id}");
                return response;

        }

        public async Task<List<ClienteModel>> GetClientesAsync()
        {
           var response = await _baseHttpServices.GetAsync<List<ClienteModel>>(_clienteEndpointcs.GetAll);
            return response;
        }

        public async  Task<ClienteModel> UpdateClienteAsync(UpdateClientemodel model)
        {
            var dto = ClienteMapperM.MapUpdateClienteDto(model);

            // Agregamos async/await, concatenamos el ID en la URL y enviamos el DTO correcto
            var response = await _baseHttpServices.PutAsync<UpdateClienteDto, ClienteModel>($"{_clienteEndpointcs.Update}{model.Id}", dto);
            return response;

        }
    }
}
