using ProductApp.Aplication.Dtos.UsuarioDto;

namespace Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel
{
    public class AuthModelcs
    {
        public string Token { get; set; } = string.Empty;
        public bool DebeCambiarPassword { get; set; }
        public bool DebeAceptarDocumentosLegales { get; set; }
        public UsuarioModel Usuario { get; set; } = null!;
    }
}
