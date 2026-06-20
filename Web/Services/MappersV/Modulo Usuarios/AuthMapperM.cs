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

        public static RegisteDto MapRegisterDto(RegisterModel model)
        {
            return new RegisteDto
            {
                Nombre = model.Nombre,
                FechaNacimiento = model.FechaNacimiento,
                Email = model.Email,
                UserName = model.UserName,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };
        }
    }
}
