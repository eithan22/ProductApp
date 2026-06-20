using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas
{
    public interface IValidatorBusinessPago
    {
        Task<OperationResult> ValidarRegistrarPagoAsync(CreatePagoDto dto, Orden orden, decimal saldoActual);
    }
}
