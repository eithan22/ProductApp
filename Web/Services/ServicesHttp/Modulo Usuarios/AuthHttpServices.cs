using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Usuarios;
using Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios;
using Web.Services.Mappers.Modulo_Usuarios;

namespace Web.Services.ServicesHttp.Modulo_Usuarios
{
    public class AuthHttpServices : IAuthHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IAuthEndpoint _authEndpoint;

        public AuthHttpServices(IBaseHttpServices baseHttpServices, IAuthEndpoint authEndpoint )
        {
            _baseHttpServices = baseHttpServices;
            _authEndpoint = authEndpoint;
        }

        public async Task<AuthModelcs> Login(LoginModel model)
        {
            // Mapear el LoginModel a un LoginDto utilizando el AuthMappercs
            var dto = AuthMapperM.MapLoginDto(model);
            // Realizar la solicitud POST al endpoint de login utilizando el BaseHttpServices con el LoginDto y esperar la respuesta que es un AuthModelcs

            var response = await _baseHttpServices.PostAsync<LoginDto, AuthModelcs>(_authEndpoint.Login, dto);
            return response;
        }
           

        }

        
    }

