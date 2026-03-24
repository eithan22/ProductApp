using FluentValidation;
using ProductApp.Aplication.Dtos.ClienteDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.ClienteValidator
{
    public class UpdateClienteValidator : AbstractValidator<UpdateClienteDto>
    {
        public UpdateClienteValidator()
        {

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id es requerido.")
                .GreaterThan(0).WithMessage("El Id debe ser mayor que cero.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es requerido.")
                .EmailAddress().WithMessage("El correo no es válido.");

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido.")
                .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener 10 dígitos.");

            RuleFor(x => x.Cedula)
                .NotEmpty().WithMessage("La cédula es requerida.")
                .Matches(@"^\d{10}$").WithMessage("La cédula debe tener 10 dígitos.");

            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es requerida.")
                .MaximumLength(200).WithMessage("La dirección no puede exceder los 200 caracteres.");

        }
    }
}
