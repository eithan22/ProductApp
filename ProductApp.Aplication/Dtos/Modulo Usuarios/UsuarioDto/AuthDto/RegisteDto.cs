namespace ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto
{
    public class RegisteDto
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
