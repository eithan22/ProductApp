using Web.Models.Modelo_Configuracion;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Configuracion
{
    public interface IConfiguracionHttpServices
    {
        Task<ConfiguracionModel> ObtenerAsync();
        Task<ConfiguracionModel> ActualizarAsync(ConfiguracionModel model);
        Task<ConfiguracionModel> SubirLogoAsync(Stream contenido, string nombreArchivo, string contentType);
        Task<bool> QuitarLogoAsync();
    }
}
