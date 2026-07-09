using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Aplication.Result.OperationResult;
using Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios
{
    public interface IAuthHttpServices
    {
        Task<AuthModelcs>Login(LoginModel model);
    }
}
