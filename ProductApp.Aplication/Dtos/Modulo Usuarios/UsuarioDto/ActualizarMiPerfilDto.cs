namespace ProductApp.Aplication.Dtos.UsuarioDto
{
    public class ActualizarMiPerfilDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
    }
}
