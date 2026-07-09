using FluentValidation;
using ProductApp.Aplication.Dtos.UsuarioDto;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class ActualizarMiPerfilValidator : AbstractValidator<ActualizarMiPerfilDto>
    {
        public ActualizarMiPerfilValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no es válido.");

            RuleFor(x => x.FechaNacimiento)
                .Must(f => !f.HasValue || f.Value < DateTime.UtcNow)
                .WithMessage("La fecha de nacimiento debe ser una fecha pasada.");
        }
    }
}
