using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class Categoria : BaseEntity
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;

        public IReadOnlyList<Producto> Productos { get; private set; } = new List<Producto>();

        protected Categoria() { }

        public Categoria(string nombre, string descripcion)
        {
            CambiarYvalidarNombre(nombre);
            CambiarYvalidarDescripcion(descripcion);
        }

        // --- Validaciones privadas ---

        private static void ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidacionDominioException("Nombre", "El nombre de la categoría no puede estar vacío.");
        }

        private static void ValidarDescripcion(string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ValidacionDominioException("Descripcion", "La descripción de la categoría no puede estar vacía.");
        }

        // --- Métodos de actualización ---

        public void CambiarYvalidarNombre(string nombre)
        {
            ValidarNombre(nombre);
            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarDescripcion(string descripcion)
        {
            ValidarDescripcion(descripcion);
            Descripcion = descripcion;
            ActualizarFechaModificacion();
        }

        // --- Ciclo de vida ---

        public void Desactivar()
        {
            if (EstaEliminado)
                throw new EstadoInvalidoException("Categoria", "Inactivo", "Desactivar");

            EstaEliminado = true;
            ActualizarFechaModificacion();
        }

        public void Activar()
        {
            if (!EstaEliminado)
                throw new EstadoInvalidoException("Categoria", "Activo", "Activar");

            EstaEliminado = false;
            ActualizarFechaModificacion();
        }
    }
}
