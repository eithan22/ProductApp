using ProductApp.Aplication.Dtos.Modulo_Configuracion;
using ProductApp.Aplication.Interface.IMappers.Modulo_Configuracion;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Mappers.Modulo_Configuracion
{
    public class ConfiguracionSistemaMapper : IMapperConfiguracionSistema
    {
        public ConfiguracionSistemaDto ToDto(ConfiguracionSistema entity)
        {
            return new ConfiguracionSistemaDto
            {
                CantidadMinimaInventarioDefecto = entity.CantidadMinimaInventarioDefecto,
                DuracionTokenMinutos = entity.DuracionTokenMinutos,
                NombreEmpresa = entity.NombreEmpresa,
                Moneda = entity.Moneda
            };
        }
    }
}
