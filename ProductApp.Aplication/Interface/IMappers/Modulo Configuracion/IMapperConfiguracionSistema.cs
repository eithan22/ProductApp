using ProductApp.Aplication.Dtos.Modulo_Configuracion;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Configuracion
{
    public interface IMapperConfiguracionSistema
    {
        ConfiguracionSistemaDto ToDto(ConfiguracionSistema entity);
    }
}
