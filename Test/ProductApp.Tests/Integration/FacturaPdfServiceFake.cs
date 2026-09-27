using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Tests.Integration
{
    // Los tests de integración corren contra un AppDbContext en memoria, sin Azurite ni QuestPDF
    // real: este fake solo permite que PagoService complete el flujo de pago sin depender de
    // infraestructura externa. No hay tests propios de facturación PDF todavía.
    internal class FacturaPdfServiceFake : IFacturaPdfService
    {
        public Task<OperationResultD<string>> GenerarYAlmacenarAsync(int ordenId)
            => Task.FromResult(OperationResultD<string>.Success($"https://fake-blob/facturas/orden-{ordenId}.pdf", "Factura generada exitosamente"));

        public Task<OperationResultD<byte[]>> ObtenerAsync(int ordenId)
            => Task.FromResult(OperationResultD<byte[]>.Failure("Fake: sin factura generada"));

        public Task<OperationResult> EliminarAsync(int ordenId, int usuarioSolicitanteId)
            => Task.FromResult(OperationResult.Success("Factura eliminada exitosamente"));
    }
}
