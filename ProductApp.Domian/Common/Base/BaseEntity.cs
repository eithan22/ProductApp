namespace ProductApp.Domian.Common.Base
{
    public abstract class BaseEntity
    {
        public int Id { get; private set; }

        public bool EstaEliminado { get; protected set; } = false;

        public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;

        public DateTime ModificadoEn { get; private set; } = DateTime.UtcNow;

        public void Eliminar()
        {
            EstaEliminado = true;
            ActualizarFechaModificacion();
        }

        public void ActualizarFechaModificacion()
        {
            ModificadoEn = DateTime.UtcNow;
        }
    }
}
