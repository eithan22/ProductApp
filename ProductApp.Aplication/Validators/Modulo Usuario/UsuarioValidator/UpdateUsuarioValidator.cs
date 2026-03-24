using FluentValidation;
using ProductApp.Aplication.Dtos.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class UpdateUsuarioValidator : AbstractValidator<UpdateUsuarioDto>

    {
        public UpdateUsuarioValidator()
        {

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id es requerido.")
                .GreaterThan(0).WithMessage("El Id debe ser mayor que cero.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no es válido.");

            RuleFor(x => x.Contraseña)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
        }
    }
}
