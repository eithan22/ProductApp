using ProductApp.Domian.Entitis;
using Web.Services.Interfaces.IEndPoints.Modulo_Usuarios;

namespace Web.Services.EndPoints.Modulo_Usuarios
{
    public class UsuarioEndpoint : IUsuariosEndpoint
    {
        public string GetAll => "Usuario/GetUsuarios";

        public string GetById => "Usuario/GetUsuarioById";

        public string Create => "Usuario/CreateUsuario";       
        public string Update => "Usuario/UpdateUsuario";

      // public string Delete => "Usuario/DeleteUsuario";

        public string Disable => "Usuario/DisableUsuario"; 
        public string cambiarPassword => "Usuario/CambiarPassword";

        public string CambiarRol => "Usuario/CambiarRol";

        public string ResetPassword => "Usuario/ResetPassword";
    }
}
