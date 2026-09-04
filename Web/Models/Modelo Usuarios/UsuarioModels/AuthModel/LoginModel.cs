using System.ComponentModel.DataAnnotations;

namespace Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Ingresá tu usuario.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá tu contraseña.")]
        public string Password { get; set; } = string.Empty;
    }
}
