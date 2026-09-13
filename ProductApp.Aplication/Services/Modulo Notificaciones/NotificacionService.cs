using ProductApp.Aplication.Dtos.Modulo_Notificaciones;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Notificaciones;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class NotificacionService : INotificacionServices
    {
        private readonly INotificacionRepository _notificacionRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapperNotificacion _mapperNotificacion;

        public NotificacionService(
            INotificacionRepository notificacionRepository,
            IUsuarioRepository usuarioRepository,
            IMapperNotificacion mapperNotificacion)
        {
            _notificacionRepository = notificacionRepository;
            _usuarioRepository = usuarioRepository;
            _mapperNotificacion = mapperNotificacion;
        }

        public async Task NotificarUsuarioAsync(int usuarioId, TipoNotificacion tipo, string mensaje)
        {
            var notificacion = new Notificacion(usuarioId, tipo, mensaje);
            await _notificacionRepository.CreateAsync(notificacion);
        }

        public async Task NotificarAdministradoresAsync(TipoNotificacion tipo, string mensaje, int? excluirUsuarioId = null)
        {
            var idsAdministradores = await _usuarioRepository.ObtenerIdsAdministradoresActivosAsync();

            foreach (var usuarioId in idsAdministradores)
            {
                // Evita notificar dos veces al mismo usuario cuando ya recibió la notificación
                // por otra vía (ej. el vendedor dueño de la orden que además es Administrador).
                if (excluirUsuarioId.HasValue && usuarioId == excluirUsuarioId.Value)
                    continue;

                await NotificarUsuarioAsync(usuarioId, tipo, mensaje);
            }
        }

        public async Task<OperationResultD<NotificacionResumenDto>> ObtenerResumenAsync(int usuarioId, int cantidad = 10)
        {
            var recientes = await _notificacionRepository.ObtenerRecientesPorUsuarioAsync(usuarioId, cantidad);
            var noLeidas = await _notificacionRepository.ContarNoLeidasPorUsuarioAsync(usuarioId);

            var resumen = new NotificacionResumenDto
            {
                Recientes = recientes.Select(_mapperNotificacion.MapToNotificacionResponseDto).ToList(),
                NoLeidas = noLeidas
            };

            return OperationResultD<NotificacionResumenDto>.Success(resumen, "Resumen de notificaciones obtenido exitosamente");
        }

        public async Task<OperationResultD<bool>> MarcarComoLeidaAsync(int id, int usuarioId)
        {
            var notificacion = await _notificacionRepository.GetByIdAsync(id);
            if (notificacion == null)
                return OperationResultD<bool>.Failure("Notificación no encontrada");

            if (notificacion.UsuarioId != usuarioId)
                return OperationResultD<bool>.Failure("No autorizado para modificar esta notificación");

            notificacion.MarcarComoLeida();
            await _notificacionRepository.UpdateAsync(notificacion);

            return OperationResultD<bool>.Success(true, "Notificación marcada como leída");
        }

        public async Task<OperationResultD<bool>> MarcarTodasComoLeidasAsync(int usuarioId)
        {
            await _notificacionRepository.MarcarTodasComoLeidasAsync(usuarioId);
            return OperationResultD<bool>.Success(true, "Notificaciones marcadas como leídas");
        }
    }
}
