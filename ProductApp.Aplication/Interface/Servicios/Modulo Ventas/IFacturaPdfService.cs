using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface
{
    public interface IFacturaPdfService
    {
        Task<OperationResultD<string>> GenerarYAlmacenarAsync(int ordenId); // devuelve la URL en Blob Storage
        Task<OperationResultD<byte[]>> ObtenerAsync(int ordenId);
    }
}
