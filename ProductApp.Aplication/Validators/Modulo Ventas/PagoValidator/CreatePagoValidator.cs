using FluentValidation;
using ProductApp.Aplication.Dtos.PagoDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Ventas.PagoValidator
{
    public class CreatePagoValidator : AbstractValidator<CreatePagoDto>
    {
        public CreatePagoValidator()
        {
            RuleFor(x => x.OrdenId)
                .GreaterThan(0).WithMessage("El Id de la orden debe ser mayor que cero.");

            RuleFor(x => x.Monto)
                .GreaterThan(0).WithMessage("El monto del pago debe ser mayor que cero.");

            RuleFor(x => x.MetodoPago)
                .IsInEnum().WithMessage("El método de pago no es válido.");
        }
    }
}
