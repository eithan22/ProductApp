using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.AuthValidator
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("El nombre solo puede contener letras y espacios.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no es válido.")
                .MaximumLength(100).WithMessage("El email no puede exceder los 100 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
                .MaximumLength(20).WithMessage("La contraseña no puede exceder los 20 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario es requerido.")
                .MaximumLength(20).WithMessage("El nombre de usuario no puede exceder los 20 caracteres.")
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage("El nombre de usuario solo puede contener letras y números.");

            RuleFor(x => x.FechaNacimiento)
                .Must(f => !f.HasValue || f.Value < DateTime.UtcNow)
                .WithMessage("La fecha de nacimiento debe ser una fecha pasada.");
        }
    }
}
