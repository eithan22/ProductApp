namespace ProductApp.Aplication.Dtos.UsuarioDto
{
    public class UpdateUsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime? FechaNacimiento { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;
    }
}
