using FluentValidation;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Domian.Common.Enums.EnumsUsuario;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class CreateUsuarioValidator : AbstractValidator<CreateUsuarioDto>
    {
        private static readonly string _rolesValidos =
            string.Join(", ", Enum.GetNames<RolUsuario>());

        public CreateUsuarioValidator()
        {
            RuleFor(x => x.RolUsuario)
                .NotEmpty().WithMessage("El rol del usuario es requerido.")
                .Must(x => Enum.TryParse<RolUsuario>(x, true, out _))
                .WithMessage($"El rol debe ser uno de: {_rolesValidos}");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no es válido.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario es requerido.")
                .MaximumLength(20).WithMessage("El nombre de usuario no puede exceder los 20 caracteres.")
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage("El nombre de usuario solo puede contener letras y números.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
                .MaximumLength(20).WithMessage("La contraseña no puede exceder los 20 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.");

            RuleFor(x => x.FechaNacimiento)
                .Must(f => !f.HasValue || f.Value < DateTime.UtcNow)
                .WithMessage("La fecha de nacimiento debe ser una fecha pasada.");
        }
    }
}
