using FluentValidation;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsPago;

namespace ProductApp.Aplication.Validators.Modulo_Ventas.PagoValidator
{
    public class CreatePagoValidator : AbstractValidator<CreatePagoDto>
    {
        private static readonly string _valoresValidos =
            string.Join(", ", Enum.GetNames<MetodoPago>());

        public CreatePagoValidator()
        {
            RuleFor(x => x.OrdenId)
                .GreaterThan(0).WithMessage("El Id de la orden debe ser mayor que cero.");

            RuleFor(x => x.Monto)
                .GreaterThan(0).WithMessage("El monto del pago debe ser mayor que cero.");

            RuleFor(x => x.MetodoPago)
                .NotEmpty().WithMessage("El método de pago es requerido.")
                .Must(x => Enum.TryParse<MetodoPago>(x, true, out _))
                .WithMessage($"El método de pago debe ser uno de: {_valoresValidos}");
        }
    }
}
