using ProductApp.Aplication.Dtos.Modulo_Notificaciones;
using ProductApp.Aplication.Interface.IMappers.Modulo_Notificaciones;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Mappers.Modulo_Notificaciones
{
    public class NotificacionMapper : IMapperNotificacion
    {
        public NotificacionResponseDto MapToNotificacionResponseDto(Notificacion notificacion)
        {
            return new NotificacionResponseDto
            {
                Id = notificacion.Id,
                Tipo = notificacion.Tipo.ToString(),
                Mensaje = notificacion.Mensaje,
                Leida = notificacion.Leida,
                CreadoEn = notificacion.CreadoEn
            };
        }
    }
}
