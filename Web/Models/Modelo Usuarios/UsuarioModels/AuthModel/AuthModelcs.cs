using ProductApp.Aplication.Dtos.UsuarioDto;

namespace Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel
{
    public class AuthModelcs
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioModel Usuario { get; set; } = null!;
    }
}
