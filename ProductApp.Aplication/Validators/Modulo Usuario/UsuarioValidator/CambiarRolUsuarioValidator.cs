using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Domian.Common.Enums.EnumsUsuario;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class CambiarRolUsuarioValidator : AbstractValidator<CambiarRolDto>
    {
        private static readonly string _valoresValidos =
            string.Join(", ", Enum.GetNames<RolUsuario>());

        public CambiarRolUsuarioValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id del usuario debe ser mayor que cero.");

            RuleFor(x => x.NuevoRol)
                .NotEmpty().WithMessage("El nuevo rol es requerido.")
                .Must(x => Enum.TryParse<RolUsuario>(x, true, out _))
                .WithMessage($"El rol debe ser uno de: {_valoresValidos}");
        }
    }
}
    