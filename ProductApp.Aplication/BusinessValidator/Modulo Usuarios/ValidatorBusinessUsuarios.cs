using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Legal;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Usuarios
{
    public class ValidatorBusinessUsuarios : IValidatorBusinessUsuario
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

        public async Task<OperationResult> ValidarUpdateUsuarioAsync(UpdateUsuarioDto dto, Usuario usuario)
        {
            var existe = await _usuarioRepository.ExisteAsync(
                x => (x.Email == dto.Email || x.Username == dto.UserName) && x.Id != dto.Id);
            if (existe)
                return OperationResult.Failure("El email o nombre de usuario ya está en uso por otro usuario.");

            // Misma regla que ValidarCambiarRol: UsuarioMapper.MapUpdate también cambia el rol
            // (CambiarRol), así que esta puerta necesita la misma protección contra dejar el
            // sistema sin ningún administrador activo.
            if (!Enum.TryParse<RolUsuario>(dto.RolUsuario, true, out var nuevoRol))
                return OperationResult.Failure("El rol indicado no es válido.");

            if (usuario.RolUsuario == RolUsuario.Administrador && nuevoRol != RolUsuario.Administrador)
            {
                var administradoresActivos = await _usuarioRepository.ObtenerIdsAdministradoresActivosAsync();

                if (administradoresActivos.Count == 1 && administradoresActivos.Contains(usuario.Id))
                    return OperationResult.Failure(
                        "No se puede quitar el rol de Administrador: es el último administrador activo del sistema.");
            }

            return OperationResult.Success();
        }

        // Misma regla de email único que la actualización administrativa, con dos diferencias
        // deliberadas: el id no viene del DTO sino del token (en "mi perfil" el usuario solo
        // puede editarse a sí mismo, aceptar un id del cuerpo abriría la puerta a editar el
        // perfil de otro), y no se valida el Username porque el perfil propio no lo puede
        // cambiar: ActualizarMiPerfilDto no lo expone.
        public async Task<OperationResult> ValidarActualizarMiPerfilAsync(ActualizarMiPerfilDto dto, int usuarioId)
        {
            var existe = await _usuarioRepository.ExisteAsync(
                x => x.Email == dto.Email && x.Id != usuarioId);
            if (existe)
                return OperationResult.Failure("El email ya está en uso por otro usuario.");

            return OperationResult.Success();
        }

        // Delete lógico (desactivación): es la baja real que usa la aplicación.
        public async Task<OperationResult> ValidarDeleteUsuarioAsync(Usuario usuario)
        {
            if (usuario == null)
                return OperationResult.Failure("El usuario no existe.");

            if (await EsUltimoAdministradorActivoAsync(usuario))
                return OperationResult.Failure("No se puede desactivar al último administrador activo del sistema.");

            return OperationResult.Success();
        }

        // Baja definitiva (soft delete): la fila se conserva pero el usuario deja de existir para
        // la aplicación, así que la protección del último administrador aplica igual que en la
        // desactivación, solo cambia el verbo del mensaje.
        public async Task<OperationResult> ValidarBorradoFisicoUsuarioAsync(Usuario usuario)
        {
            if (usuario == null)
                return OperationResult.Failure("El usuario no existe.");

            if (await EsUltimoAdministradorActivoAsync(usuario))
                return OperationResult.Failure("No se puede eliminar al último administrador activo del sistema.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarCambiarPasswordUsuario(ChangePasswordDto dto, Usuario usuario)
        {
            bool valido = PasswordHelper.Verify(dto.PasswordActual, usuario.PasswordHash);
            if (!valido)
                return OperationResult.Failure("Contraseña actual incorrecta.");

            return OperationResult.Success("Contraseña válida para cambio.");
        }

        // La existencia del usuario ya la confirmó el servicio antes de llamar acá; no hay otra
        // regla de negocio que validar para resetear una contraseña. Task.FromResult en vez de
        // async sin await, mismo criterio que ValidarAceptacionDocumentosLegales.
        public Task<OperationResult> ValidarResetearPassword(ResetearPasswordDto dto, Usuario usuario)
        {
            return Task.FromResult(OperationResult.Success("Usuario encontrado para resetear contraseña."));
        }

        public async Task<OperationResult> ValidarCambiarRol(CambiarRolDto dto, Usuario usuario)
        {
            if (!Enum.TryParse<RolUsuario>(dto.NuevoRol, true, out var nuevoRol))
                return OperationResult.Failure("El rol indicado no es válido.");

            // El sistema no puede quedarse sin ningún Administrador: si el usuario es el
            // único administrador activo, no se le puede quitar ese rol. Un administrador
            // inactivo no cuenta, por eso la comprobación es contra la lista de activos.
            if (usuario.RolUsuario == RolUsuario.Administrador && nuevoRol != RolUsuario.Administrador)
            {
                var administradoresActivos = await _usuarioRepository.ObtenerIdsAdministradoresActivosAsync();

                if (administradoresActivos.Count == 1 && administradoresActivos.Contains(usuario.Id))
                    return OperationResult.Failure(
                        "No se puede quitar el rol de Administrador: es el último administrador activo del sistema.");
            }

            return OperationResult.Success("Usuario encontrado para cambiar rol.");
        }

        // La única regla de la aceptación: lo que el usuario dice haber aceptado tiene que ser
        // la versión que hoy rige. Si no coincide, leyó un texto que ya no es el vigente (la
        // pantalla quedó abierta durante un despliegue) y esa aceptación no probaría nada.
        // Task.FromResult en vez de async sin await: no hay nada que esperar acá.
        public Task<OperationResult> ValidarAceptacionDocumentosLegales(AceptarDocumentosLegalesDto dto)
        {
            if (!string.Equals(dto.Version, DocumentosLegales.VersionVigente, StringComparison.Ordinal))
                return Task.FromResult(OperationResult.Failure(
                    "Los documentos legales cambiaron mientras los leías. Recargá la página y volvé a revisarlos."));

            return Task.FromResult(OperationResult.Success());
        }

        // El sistema siempre debe conservar al menos un administrador activo: si se queda sin
        // ninguno, nadie puede volver a gestionar usuarios, roles ni configuración.
        // Si el usuario no es administrador activo no aparece en la lista, así que no bloquea.
        private async Task<bool> EsUltimoAdministradorActivoAsync(Usuario usuario)
        {
            var administradoresActivos = await _usuarioRepository.ObtenerIdsAdministradoresActivosAsync();

            return administradoresActivos.Count <= 1 && administradoresActivos.Contains(usuario.Id);
        }
    }
}
