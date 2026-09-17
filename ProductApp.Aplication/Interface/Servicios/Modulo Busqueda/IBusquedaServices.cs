using ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto;
using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface
{
    // Servicio de solo lectura, sin IBaseServices ni validador de negocio propio: no crea,
    // no modifica ni borra nada, solo agrega consultas de los módulos existentes.
    // Mismo criterio que IReporteServices.
    public interface IBusquedaServices
    {
        Task<OperationResultD<BusquedaGlobalResponseDto>> BuscarGlobalAsync(string? texto);
    }
}
