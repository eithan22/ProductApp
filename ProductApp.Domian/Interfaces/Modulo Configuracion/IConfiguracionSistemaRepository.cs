using ProductApp.Domian.Entitis;

namespace ProductApp.Domian.Interfaces
{
    public interface IConfiguracionSistemaRepository
    {
        Task<ConfiguracionSistema?> ObtenerAsync();
        Task ActualizarAsync(ConfiguracionSistema configuracion);
    }
}
