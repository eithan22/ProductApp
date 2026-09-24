using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services.Modulo_Usuarios
{
    public class VerificadorSesionService : IVerificadorSesionService
    {
        // Mensaje único a propósito: el cliente no debe poder distinguir si la cuenta fue
        // desactivada, eliminada, si le bajaron el rol o si el token viene roto. Todas las
        // causas significan lo mismo para él: el token que trae ya no representa al usuario
        // que hay en la base.
        private const string MensajeSesionInvalida = "Su sesión ya no es válida. Inicie sesión nuevamente.";

        private readonly IUsuarioRepository _usuarioRepository;

        public VerificadorSesionService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<OperationResult> VerificarSesionAsync(int usuarioId, string? rolEnElToken)
        {
            if (usuarioId <= 0)
                return OperationResult.Failure(MensajeSesionInvalida);

            // ignoreCase igual que en UsuarioService.CambiarRol: el claim lo escribe AuthServices
            // con RolUsuario.ToString(), pero no se asume la caja exacta.
            if (!Enum.TryParse<RolUsuario>(rolEnElToken, ignoreCase: true, out var rolDelToken))
                return OperationResult.Failure(MensajeSesionInvalida);

            var (existe, rolActual, estadoActual) = await _usuarioRepository.ObtenerEstadoSesionAsync(usuarioId);

            if (!existe)
                return OperationResult.Failure(MensajeSesionInvalida);

            if (estadoActual != EstadoUsuario.Activo)
                return OperationResult.Failure(MensajeSesionInvalida);

            if (rolActual != rolDelToken)
                return OperationResult.Failure(MensajeSesionInvalida);

            return OperationResult.Success();
        }
    }
}
