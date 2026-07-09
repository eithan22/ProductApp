using ProductApp.Aplication.Dtos.Modulo_Configuracion;
using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface.Servicios.Modulo_Configuracion
{
    public interface IConfiguracionSistemaService
    {
        Task<OperationResultD<ConfiguracionSistemaDto>> ObtenerAsync();
        Task<OperationResultD<ConfiguracionSistemaDto>> ActualizarAsync(ActualizarConfiguracionSistemaDto dto);
    }
}
