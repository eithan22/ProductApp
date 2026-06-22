using FluentValidation;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Domian.Common.Enums.EnumsUsuario;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class UpdateUsuarioValidator : AbstractValidator<UpdateUsuarioDto>
    {
        private static readonly string _rolesValidos =
            string.Join(", ", Enum.GetNames<RolUsuario>());

        public UpdateUsuarioValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id debe ser mayor que cero.");

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

            RuleFor(x => x.FechaNacimiento)
                .Must(f => !f.HasValue || f.Value < DateTime.UtcNow)
                .WithMessage("La fecha de nacimiento debe ser una fecha pasada.");
        }
    }
}
