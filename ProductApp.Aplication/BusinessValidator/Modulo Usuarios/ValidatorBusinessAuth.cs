using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Usuarios
{
    public class ValidatorBusinessAuth : IValidatorBusinessAuth
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public ValidatorBusinessAuth(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }



        public async Task<OperationResult> ValidarLoginAsync(LoginDto dto)
        {
            if(string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                return OperationResult.Failure("El nombre de usuario y la contraseña son obligatorios");

            var usuario = await _usuarioRepository
                .FirstOrDefaultAsync(x => x.Username == dto.Username);

            if (usuario == null)
                return OperationResult.Failure("Usuario no encontrado");

            if (usuario.EstaEliminado)
                return OperationResult.Failure("Usuario deshabilitado");

            if (usuario.EstadoUsuario != EstadoUsuario.Activo)
                return OperationResult.Failure("Usuario inactivo o suspendido");

            bool valido = PasswordHelper.Verify(dto.Password, usuario.PasswordHash);

            if (!valido)
                return OperationResult.Failure("Contraseña incorrecta");

            return OperationResult.Success();
        }
       
    }
}
