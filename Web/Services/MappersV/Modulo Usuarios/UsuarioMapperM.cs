using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using Web.Models.Modelo_Usuarios.UsuarioModels;

namespace Web.Services.Mappers.Modulo_Usuarios
{
    public class UsuarioMapperM
    {
        public static CreateUsuarioDto MapAddUsuarioDto(CreateUsuarioModel model)
        {
            return new CreateUsuarioDto
            {
                Nombre = model.Nombre,
                Email = model.Email,
                FechaNacimiento = model.FechaNacimiento,
                UserName = model.UserName,
                Password = model.Password,
                RolUsuario = model.RolUsuario.ToString(),
            };
        }

        public static UpdateUsuarioDto MapUpdateUsuarioDto(UpdateUsuarioModel model)
        {
            return new UpdateUsuarioDto
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Email = model.Email,
                FechaNacimiento = model.FechaNacimiento,
                UserName = model.UserName,
                RolUsuario = model.RolUsuario.ToString(),
            };
        }

        public static ChangePasswordDto MapChangePasswordDto(ChangePasswordModel model)
        {
            return new ChangePasswordDto
            {
                PasswordActual = model.PasswordActual,
                PasswordNueva = model.PasswordNueva
            };
        }

        public static CambiarRolDto MapCambiarRolDto(CambiarRolModel model)
        {
            return new CambiarRolDto
            {
                Id = model.Id,
                NuevoRol = model.NuevoRol.ToString()
            };
        }

        public static ResetearPasswordDto MapResetPasswordDto(ResetearPasswordModel model)
        {
            return new ResetearPasswordDto
            {
                Id = model.Id,
                NuevaContraseña = model.NuevaContraseña
            };
        }

        public static ActualizarMiPerfilDto MapMiPerfilDto(UsuarioModel model)
        {
            return new ActualizarMiPerfilDto
            {
                Nombre = model.Nombre,
                Email = model.Email,
                FechaNacimiento = model.FechaNacimiento
            };
        }
    }
}
