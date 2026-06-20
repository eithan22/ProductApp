using ProductApp.Domian.Common.Enums.EnumsUsuario;

namespace ProductApp.Aplication.Dtos.UsuarioDto
{
    public class CreateUsuarioDto
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime? FechaNacimiento { get; set; }
        public string UserName { get; set; } = string.Empty;
        public RolUsuario RolUsuario { get; set; }
    }
}
