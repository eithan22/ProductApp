using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Domian.Common.Enums.EnumsOrden;

namespace ProductApp.Aplication.Validators.Modulo_Ventas.OrdenValidator
{
    public class CambiarEstadoOrdenValidator : AbstractValidator<CambiarEstadoOrdenDto>
    {
        private static readonly string _valoresValidos =
            string.Join(", ", Enum.GetNames<EstadoOrden>());

        public CambiarEstadoOrdenValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id de la orden debe ser mayor que cero.");

            RuleFor(x => x.NuevoEstado)
                .NotEmpty().WithMessage("El nuevo estado es requerido.")
                .Must(x => Enum.TryParse<EstadoOrden>(x, true, out _))
                .WithMessage($"El estado debe ser uno de: {_valoresValidos}");
        }
    }
}
