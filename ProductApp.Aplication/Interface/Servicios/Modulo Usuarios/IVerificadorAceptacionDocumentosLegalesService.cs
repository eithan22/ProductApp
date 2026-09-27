using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios
{
    public interface IVerificadorAceptacionDocumentosLegalesService
    {
        // Recibe el id que viaja en el token y responde si ese usuario ya aceptó la versión
        // vigente de los documentos legales.
        Task<OperationResult> VerificarAceptacionAsync(int usuarioId);
    }
}
