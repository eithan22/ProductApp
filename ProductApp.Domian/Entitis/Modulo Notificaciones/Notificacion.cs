using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class Notificacion : BaseEntity
    {
        public int UsuarioId { get; private set; }
        public TipoNotificacion Tipo { get; private set; }
        public string Mensaje { get; private set; } = string.Empty;
        public bool Leida { get; private set; }

        public Usuario Usuario { get; private set; } = null!;

        protected Notificacion() { }

        public Notificacion(int usuarioId, TipoNotificacion tipo, string mensaje)
        {
            if (usuarioId <= 0)
                throw new ValidacionDominioException("UsuarioId", "El id del usuario no es válido.");

            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ValidacionDominioException("Mensaje", "El mensaje de la notificación no puede estar vacío.");

            UsuarioId = usuarioId;
            Tipo = tipo;
            Mensaje = mensaje;
            Leida = false;
        }

        public void MarcarComoLeida()
        {
            if (Leida) return;
            Leida = true;
            ActualizarFechaModificacion();
        }
    }
}
