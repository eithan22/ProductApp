using Web.Services.Interfaces.IEndPoints.Modulo_Configuracion;

namespace Web.Services.EndPoints.Modulo_Configuracion
{
    public class ConfiguracionEndpoint : IConfiguracionEndpoint
    {
        public string Url => "Configuracion";
        public string SubirLogo => "Configuracion/SubirLogo";
        public string Logo => "Configuracion/Logo";
    }
}
