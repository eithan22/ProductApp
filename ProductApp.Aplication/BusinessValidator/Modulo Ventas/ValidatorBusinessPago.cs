using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Ventas
{
    public class ValidatorBusinessPago : IValidatorBusinessPago
    {
        public Task<OperationResult> ValidarRegistrarPagoAsync(CreatePagoDto dto, Orden orden, decimal saldoActual)
        {
            if (orden.Estado != EstadoOrden.Pendiente)
                return Task.FromResult(OperationResult.Failure(
                    "Solo se pueden registrar pagos para órdenes en estado Pendiente."));

            if (orden.Total <= 0)
                return Task.FromResult(OperationResult.Failure(
                    "La orden no tiene total calculado. Agregue productos antes de registrar un pago."));

            if (dto.Monto > saldoActual)
                return Task.FromResult(OperationResult.Failure(
                    $"El monto ({dto.Monto}) supera el saldo pendiente ({saldoActual})."));

            return Task.FromResult(OperationResult.Success());
        }
    }
}
