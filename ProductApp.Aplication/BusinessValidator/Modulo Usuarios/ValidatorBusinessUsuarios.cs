using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Usuarios
{
    public class ValidatorBusinessUsuarios : IValitadorBusinessUsuario
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public ValidatorBusinessUsuarios(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<OperationResult> ValidarCreateUsuarioAsync(CreateUsuarioDto dto)
        {
            var existe = await _usuarioRepository.ExisteAsync(x => x.Email == dto.Email || x.Username == dto.UserName);
            if (existe)
                return OperationResult.Failure("El email o nombre de usuario ya está en uso.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarUpdateUsuarioAsync(UpdateUsuarioDto dto)
        {
            var existe = await _usuarioRepository.ExisteAsync(
                x => (x.Email == dto.Email || x.Username == dto.UserName) && x.Id != dto.Id);
            if (existe)
                return OperationResult.Failure("El email o nombre de usuario ya está en uso por otro usuario.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarDeleteUsuarioAsync(Usuario usuario)
        {
            if (usuario == null)
                return OperationResult.Failure("El usuario no existe.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarCambiarPasswordUsuario(ChangePasswordDto dto, Usuario usuario)
        {
            bool valido = PasswordHelper.Verify(dto.PasswordActual, usuario.PasswordHash);
            if (!valido)
                return OperationResult.Failure("Contraseña actual incorrecta.");

            return OperationResult.Success("Contraseña válida para cambio.");
        }

        public async Task<OperationResult> ValidarResetearPassword(ResetearPasswordDto dto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
                return OperationResult.Failure("Usuario no encontrado.");

            return OperationResult.Success("Usuario encontrado para resetear contraseña.");
        }

        public async Task<OperationResult> ValidarCambiarRol(CambiarRolDto dto)
        {
            return OperationResult.Success("Usuario encontrado para cambiar rol.");
        }
    }
}
