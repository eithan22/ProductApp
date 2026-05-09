namespace Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel
{
    public class RegisterModel
    {
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
