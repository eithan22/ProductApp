using ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto;
using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface
{
    public interface IImportacionProductosService
    {
        // Sin async: armar la plantilla es puro cómputo en memoria, no hay I/O que esperar.
        OperationResultD<byte[]> ObtenerPlantilla();

        Task<OperationResultD<ImportacionProductosResultadoDto>> ImportarAsync(
            ImportarProductosDto dto, int usuarioSolicitanteId);
    }
}
