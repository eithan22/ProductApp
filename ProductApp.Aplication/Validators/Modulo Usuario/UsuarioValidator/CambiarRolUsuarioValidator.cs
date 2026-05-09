using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class CambiarRolUsuarioValidator : AbstractValidator<CambiarRolDto>
    {
        public CambiarRolUsuarioValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id del usuario es requerido.")
                .GreaterThan(0).WithMessage("El Id del usuario debe ser mayor que cero.");
            RuleFor(x => x.NuevoRol)
                .NotEmpty().WithMessage("El nuevo rol es requerido.")
                .IsInEnum().WithMessage("El nuevo rol no es válido.");
        }
    }
}
    