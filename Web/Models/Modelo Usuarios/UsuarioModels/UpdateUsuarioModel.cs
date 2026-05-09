using ProductApp.Domian.Common.Enums.EnumsUsuario;

namespace Web.Models.Modelo_Usuarios.UsuarioModels
{
    public class UpdateUsuarioModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;


        public int Edad { get; set; }

        public string UserName { get; set; } = string.Empty;
        

        public RolUsuario RolUsuario { get; set; }
    }
}
