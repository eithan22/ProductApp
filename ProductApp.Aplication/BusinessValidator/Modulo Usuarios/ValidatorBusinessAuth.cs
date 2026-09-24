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
        // Único mensaje que ve el cliente cuando el login falla, sin importar la causa: usuario
        // inexistente, eliminado, inactivo/suspendido o contraseña incorrecta. Si cada caso
        // tuviera su propio texto, cualquiera podría probar nombres de usuario y deducir cuáles
        // existen en el sistema. El motivo real viaja en MotivoInterno y queda en el log.
        private const string CredencialesInvalidas = "Usuario o contraseña incorrectos";

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
                return OperationResult.Failure(CredencialesInvalidas, "El usuario no existe");

            if (usuario.EstaEliminado)
                return OperationResult.Failure(CredencialesInvalidas, "El usuario está eliminado");

            if (usuario.EstadoUsuario != EstadoUsuario.Activo)
                return OperationResult.Failure(CredencialesInvalidas, $"El usuario está en estado {usuario.EstadoUsuario}");

            bool valido = PasswordHelper.Verify(dto.Password, usuario.PasswordHash);

            if (!valido)
                return OperationResult.Failure(CredencialesInvalidas, "La contraseña es incorrecta");

            return OperationResult.Success();
        }
       
    }
}
