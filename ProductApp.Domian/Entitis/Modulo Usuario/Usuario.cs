using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Common.Legal;

namespace ProductApp.Domian.Entitis
{
    public class Usuario : BaseEntity
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public RolUsuario RolUsuario { get; private set; }
        public EstadoUsuario EstadoUsuario { get; private set; }
        public DateTime? FechaNacimiento { get; private set; }
        public bool DebeCambiarPassword { get; private set; } = false;

        // Constancia de la aceptación de los documentos legales. Nullable a propósito: null
        // significa "nunca aceptó", que es el estado de todo usuario creado antes de que este
        // registro existiera y de todo usuario nuevo.
        public string? VersionDocumentosLegalesAceptada { get; private set; }
        public DateTime? FechaAceptacionDocumentosLegales { get; private set; }

        public const int LargoMaximoVersionDocumentosLegales = 20;

        public int? Edad => FechaNacimiento.HasValue
            ? CalcularEdad(FechaNacimiento.Value)
            : null;

        // Se compara contra la versión que hay en el código, no contra una guardada en base:
        // ver el comentario de DocumentosLegales. Ordinal porque es un número de versión, no
        // texto de interfaz. Como Edad, es calculada: UsuarioConfig la ignora.
        public bool DebeAceptarDocumentosLegales =>
            !string.Equals(VersionDocumentosLegalesAceptada, DocumentosLegales.VersionVigente, StringComparison.Ordinal);

        public IReadOnlyList<Orden> Ordenes { get; private set; } = new List<Orden>();

        protected Usuario() { }

        public Usuario(string nombre, string email, string username, RolUsuario rolUsuario)
        {
            ValidarNombre(nombre);
            ValidarEmail(email);
            ValidarUsername(username);

            Nombre = nombre;
            Email = email;
            Username = username;
            RolUsuario = rolUsuario;
            EstadoUsuario = EstadoUsuario.Activo;
        }

        // --- Validaciones privadas ---

        private static void ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidacionDominioException("Nombre", "El nombre no puede estar vacío.");
        }

        private static void ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidacionDominioException("Email", "El email no puede estar vacío.");
        }

        private static void ValidarUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ValidacionDominioException("Username", "El username no puede estar vacío.");
        }

        private static int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.UtcNow;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        // --- Métodos de actualización ---

        public void CambiarNombre(string nombre)
        {
            ValidarNombre(nombre);
            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        public void CambiarEmail(string email)
        {
            ValidarEmail(email);
            Email = email;
            ActualizarFechaModificacion();
        }

        public void EstablecerFechaNacimiento(DateTime? fechaNacimiento)
        {
            FechaNacimiento = fechaNacimiento;
            ActualizarFechaModificacion();
        }

        public void EstablecerPasswordHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ValidacionDominioException("PasswordHash", "El hash de contraseña no puede estar vacío.");

            PasswordHash = hash;
        }

        public void MarcarPasswordComoTemporal()
        {
            DebeCambiarPassword = true;
            ActualizarFechaModificacion();
        }

        public void ConfirmarCambioPassword()
        {
            DebeCambiarPassword = false;
            ActualizarFechaModificacion();
        }

        // Deja constancia de qué versión aceptó el usuario y cuándo. Si ya había aceptado esa
        // misma versión no toca nada: la fecha de la PRIMERA aceptación es la que sirve como
        // prueba, y sobrescribirla con la de un segundo clic la destruiría.
        public void RegistrarAceptacionDocumentosLegales(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ValidacionDominioException("VersionDocumentosLegalesAceptada",
                    "La versión de los documentos aceptados no puede estar vacía.");

            if (version.Length > LargoMaximoVersionDocumentosLegales)
                throw new ValidacionDominioException("VersionDocumentosLegalesAceptada",
                    $"La versión de los documentos aceptados no puede exceder los {LargoMaximoVersionDocumentosLegales} caracteres.");

            if (string.Equals(VersionDocumentosLegalesAceptada, version, StringComparison.Ordinal))
                return;

            VersionDocumentosLegalesAceptada = version;
            FechaAceptacionDocumentosLegales = DateTime.UtcNow;
            ActualizarFechaModificacion();
        }

        public void CambiarRol(RolUsuario nuevoRol)
        {
            RolUsuario = nuevoRol;
            ActualizarFechaModificacion();
        }

        // --- Ciclo de vida ---

        public void Desactivar()
        {
            if (EstadoUsuario == EstadoUsuario.Inactivo)
                throw new EstadoInvalidoException("Usuario", EstadoUsuario.ToString(), "Desactivar");

            EstadoUsuario = EstadoUsuario.Inactivo;
            ActualizarFechaModificacion();
        }

        public void Activar()
        {
            if (EstadoUsuario == EstadoUsuario.Activo)
                throw new EstadoInvalidoException("Usuario", EstadoUsuario.ToString(), "Activar");

            EstadoUsuario = EstadoUsuario.Activo;
            ActualizarFechaModificacion();
        }
    }
}
