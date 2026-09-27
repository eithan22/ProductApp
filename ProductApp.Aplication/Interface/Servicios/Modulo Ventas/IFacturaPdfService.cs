using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface
{
    public interface IFacturaPdfService
    {
        Task<OperationResultD<string>> GenerarYAlmacenarAsync(int ordenId); // devuelve la URL en Blob Storage
        Task<OperationResultD<byte[]>> ObtenerAsync(int ordenId);

        // Borra la factura archivada de una orden (solo Administrador desde la Api). Cumple la
        // promesa de la Política de Privacidad de poder eliminar la factura de un cliente que lo
        // solicite. Recibe usuarioSolicitanteId solo para el log de auditoría, igual que el resto
        // de las operaciones administrativas sensibles del proyecto.
        Task<OperationResult> EliminarAsync(int ordenId, int usuarioSolicitanteId);
    }
}
