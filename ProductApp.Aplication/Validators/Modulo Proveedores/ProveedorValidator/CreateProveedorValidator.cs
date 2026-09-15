using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;

namespace ProductApp.Aplication.Validators.Modulo_Proveedores.ProveedorValidator
{
    public class CreateProveedorValidator : AbstractValidator<CreateProveedorDto>
    {
        public CreateProveedorValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(60).WithMessage("El nombre no puede exceder los 60 caracteres.");

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido.")
                .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener 10 dígitos.");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es requerido.")
                .EmailAddress().WithMessage("El correo no es válido.")
                .MaximumLength(60).WithMessage("El correo no puede exceder los 60 caracteres.");

            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es requerida.")
                .MaximumLength(150).WithMessage("La dirección no puede exceder los 150 caracteres.");
        }
    }
}
