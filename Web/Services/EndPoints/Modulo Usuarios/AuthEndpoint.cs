using Web.Services.Interfaces.IEndPoints.Modulo_Usuarios;

namespace Web.Services.EndPoints.Modulo_Usuarios
{
    public class AuthEndpoint : IAuthEndpoint
    {
        public string Login => "Auth/login";

        public string Register => "Auth/register";
    }
}
