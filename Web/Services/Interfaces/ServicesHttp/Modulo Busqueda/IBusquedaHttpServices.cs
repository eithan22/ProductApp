using Web.Models.Modelo_Busqueda.BusquedaModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Busqueda
{
    public interface IBusquedaHttpServices
    {
        Task<BusquedaGlobalModel> BuscarGlobalAsync(string texto);
    }
}
