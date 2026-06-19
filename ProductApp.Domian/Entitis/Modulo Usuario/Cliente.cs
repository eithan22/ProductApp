using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class Cliente : BaseEntity
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Cedula { get; private set; } = string.Empty;
        public string Direccion { get; private set; } = string.Empty;
        public string Correo { get; private set; } = string.Empty;
        public string Telefono { get; private set; } = string.Empty;
        public EstadoCliente Estado { get; private set; }

        public IReadOnlyList<Orden> Ordenes { get; private set; } = new List<Orden>();

        protected Cliente() { }

        public Cliente(string nombre, string cedula, string direccion, string correo, string telefono)
        {
            CambiarYvalidarNombre(nombre);
            CambiarYvalidarCedula(cedula);
            CambiarYvalidarDireccion(direccion);
            CambiarYvalidarCorreo(correo);
            CambiarYvalidarTelefono(telefono);
            Estado = EstadoCliente.Activo;
        }

        // --- Validaciones privadas ---

        private static void ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidacionDominioException("Nombre", "El nombre no puede estar vacío.");
        }

        private static void ValidarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                throw new ValidacionDominioException("Cedula", "La cédula no puede estar vacía.");
        }

        private static void ValidarDireccion(string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion))
                throw new ValidacionDominioException("Direccion", "La dirección no puede estar vacía.");
        }

        private static void ValidarCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new ValidacionDominioException("Correo", "El correo no puede estar vacío.");
        }

        private static void ValidarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                throw new ValidacionDominioException("Telefono", "El teléfono no puede estar vacío.");
        }

        // --- Métodos de actualización ---

        public void CambiarYvalidarNombre(string nombre)
        {
            ValidarNombre(nombre);
            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarCedula(string cedula)
        {
            ValidarCedula(cedula);
            Cedula = cedula;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarDireccion(string direccion)
        {
            ValidarDireccion(direccion);
            Direccion = direccion;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarCorreo(string correo)
        {
            ValidarCorreo(correo);
            Correo = correo;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarTelefono(string telefono)
        {
            ValidarTelefono(telefono);
            Telefono = telefono;
            ActualizarFechaModificacion();
        }

        // --- Ciclo de vida ---

        public void Desactivar()
        {
            if (Estado == EstadoCliente.Inactivo)
                throw new EstadoInvalidoException("Cliente", Estado.ToString(), "Desactivar");

            Estado = EstadoCliente.Inactivo;
            ActualizarFechaModificacion();
        }

        public void Activar()
        {
            if (Estado == EstadoCliente.Activo)
                throw new EstadoInvalidoException("Cliente", Estado.ToString(), "Activar");

            Estado = EstadoCliente.Activo;
            ActualizarFechaModificacion();
        }
    }
}
