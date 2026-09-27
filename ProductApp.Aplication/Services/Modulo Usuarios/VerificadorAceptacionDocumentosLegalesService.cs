using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Legal;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services.Modulo_Usuarios
{
    // Mismo papel que VerificadorSesionService para VerificarSesionVigenteFilter: el filtro no
    // contiene reglas, todas viven acá. La comprobación va contra la base y no contra un claim
    // del token porque este estado cambia DENTRO de la sesión — ver el comentario del filtro.
    public class VerificadorAceptacionDocumentosLegalesService : IVerificadorAceptacionDocumentosLegalesService
    {
        // Mensaje único y estable: la capa Web lo usa como señal para llevar al usuario a la
        // pantalla de aceptación (igual que ya hace con "Debe cambiar su contraseña").
        private const string MensajeAceptacionPendiente =
            "Debe aceptar los Términos de Servicio y la Política de Privacidad antes de continuar";

        private readonly IUsuarioRepository _usuarioRepository;

        public VerificadorAceptacionDocumentosLegalesService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<OperationResult> VerificarAceptacionAsync(int usuarioId)
        {
            if (usuarioId <= 0)
                return OperationResult.Failure(MensajeAceptacionPendiente);

            var versionAceptada = await _usuarioRepository.ObtenerVersionDocumentosLegalesAceptadaAsync(usuarioId);

            if (!string.Equals(versionAceptada, DocumentosLegales.VersionVigente, StringComparison.Ordinal))
                return OperationResult.Failure(MensajeAceptacionPendiente);

            return OperationResult.Success();
        }
    }
}
