using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class ResetearPasswordUsuarioValidator : AbstractValidator<ResetearPasswordDto>
    {
        public  ResetearPasswordUsuarioValidator()
        {
           
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id del usuario es requerido.")
                .GreaterThan(0).WithMessage("El Id del usuario debe ser mayor que cero.");

            RuleFor(x => x.NuevaContraseña)
                .NotEmpty().WithMessage("La nueva contraseña es requerida.")
                .MinimumLength(6).WithMessage("La nueva contraseña debe tener al menos 6 caracteres.")
                .MaximumLength(20).WithMessage("La contraseña no puede exceder los 20 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.");
               
        }
    }
}
