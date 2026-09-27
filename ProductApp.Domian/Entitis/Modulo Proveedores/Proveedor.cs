using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsProveedor;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class Proveedor : BaseEntity
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Telefono { get; private set; } = string.Empty;
        public string Correo { get; private set; } = string.Empty;
        public string Direccion { get; private set; } = string.Empty;
        public EstadoProveedor Estado { get; private set; }

        // Lado inverso de la relación 1:N con Producto (RF-3.7.2). Es de solo lectura
        // hacia afuera: un producto se asocia a un proveedor desde Producto, nunca
        // agregando elementos a esta lista.
        public IReadOnlyList<Producto> Productos { get; private set; } = new List<Producto>();

        protected Proveedor() { }

        public Proveedor(string nombre, string telefono, string correo, string direccion)
        {
            CambiarYvalidarNombre(nombre);
            CambiarYvalidarTelefono(telefono);
            CambiarYvalidarCorreo(correo);
            CambiarYvalidarDireccion(direccion);
            Estado = EstadoProveedor.Activo;
        }

        // --- Validaciones privadas ---

        private static void ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidacionDominioException("Nombre", "El nombre del proveedor no puede estar vacío.");
        }

        private static void ValidarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                throw new ValidacionDominioException("Telefono", "El teléfono del proveedor no puede estar vacío.");
        }

        private static void ValidarCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new ValidacionDominioException("Correo", "El correo del proveedor no puede estar vacío.");
        }

        private static void ValidarDireccion(string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion))
                throw new ValidacionDominioException("Direccion", "La dirección del proveedor no puede estar vacía.");
        }

        // --- Métodos de actualización ---

        public void CambiarYvalidarNombre(string nombre)
        {
            ValidarNombre(nombre);
            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarTelefono(string telefono)
        {
            ValidarTelefono(telefono);
            Telefono = telefono;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarCorreo(string correo)
        {
            ValidarCorreo(correo);
            Correo = correo;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarDireccion(string direccion)
        {
            ValidarDireccion(direccion);
            Direccion = direccion;
            ActualizarFechaModificacion();
        }

        // --- Ciclo de vida ---

        public void Desactivar()
        {
            if (Estado == EstadoProveedor.Inactivo)
                throw new EstadoInvalidoException("Proveedor", Estado.ToString(), "Desactivar");

            Estado = EstadoProveedor.Inactivo;
            ActualizarFechaModificacion();
        }

        public void Activar()
        {
            if (Estado == EstadoProveedor.Activo)
                throw new EstadoInvalidoException("Proveedor", Estado.ToString(), "Activar");

            Estado = EstadoProveedor.Activo;
            ActualizarFechaModificacion();
        }
    }
}
