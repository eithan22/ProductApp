using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel;

namespace Web.Services.Mappers.Modulo_Usuarios
{
    public class AuthMapperM
    {
        public static LoginDto MapLoginDto(LoginModel model)
        {
            return new LoginDto
            {
                Username = model.Username,
                Password = model.Password
            };
        }

    }
}
