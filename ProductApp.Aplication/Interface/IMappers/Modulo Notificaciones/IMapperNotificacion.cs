using ProductApp.Aplication.Dtos.Modulo_Notificaciones;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Notificaciones
{
    public interface IMapperNotificacion
    {
        NotificacionResponseDto MapToNotificacionResponseDto(Notificacion notificacion);
    }
}
