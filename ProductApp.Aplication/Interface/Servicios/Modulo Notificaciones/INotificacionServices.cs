using ProductApp.Aplication.Dtos.Modulo_Notificaciones;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;

namespace ProductApp.Aplication.Interface
{
    public interface INotificacionServices
    {
        Task NotificarUsuarioAsync(int usuarioId, TipoNotificacion tipo, string mensaje);
        Task NotificarAdministradoresAsync(TipoNotificacion tipo, string mensaje, int? excluirUsuarioId = null);
        Task<OperationResultD<NotificacionResumenDto>> ObtenerResumenAsync(int usuarioId, int cantidad = 10);
        Task<OperationResultD<bool>> MarcarComoLeidaAsync(int id, int usuarioId);
        Task<OperationResultD<bool>> MarcarTodasComoLeidasAsync(int usuarioId);
    }
}
