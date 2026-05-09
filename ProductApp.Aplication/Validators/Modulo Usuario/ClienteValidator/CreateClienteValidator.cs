using FluentValidation;
using ProductApp.Aplication.Dtos.ClienteDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.ClienteValidator
{
    public class CreateClienteValidator : AbstractValidator<CreateClienteDto>
    {
        public CreateClienteValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(20).WithMessage("El nombre no puede exceder los 20 caracteres.");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es requerido.")
                .EmailAddress().WithMessage("El correo no es válido.")
                .MaximumLength(30).WithMessage("El correo no puede excederse de 30 caracteres");
            

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido.")
                .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener 10 dígitos.");   //829-656-2345

            RuleFor(x => x.Cedula)
                .NotEmpty().WithMessage("La cédula es requerida.")
                .Matches(@"^\d{10}$").WithMessage("La cédula debe tener 10 dígitos.");  //402-1363129-0

            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es requerida.")
                .MaximumLength(100).WithMessage("La dirección no puede exceder los 100 caracteres.");

        }
    }
}
