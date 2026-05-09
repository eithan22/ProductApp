using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel;

namespace Web.Services.Mappers.Modulo_Usuarios
{
    public class AuthMapperM
    {

        // Mapper para LoginModel a LoginDto
        public static LoginDto MapLoginDto(LoginModel model)
        {
            return new LoginDto
            {
                Username = model.Username,
                Password = model.Password
            };
        }
        // Mapper para RegisterModel a RegisterDto

        public static RegisteDto MapRegisterDto(RegisterModel model)
            {
                return new RegisteDto
                {
                    Nombre = model.Nombre,
                    Edad = model.Edad,
                    Email = model.Email,
                    UserName = model.UserName,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword
                };
        }
    }
}
