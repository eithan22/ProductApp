using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;

namespace ProductApp.Aplication.Validators.Modulo_Proveedores.ProveedorValidator
{
    public class UpdateProveedorValidator : AbstractValidator<UpdateProveedorDto>
    {
        public UpdateProveedorValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id debe ser mayor que cero.");

            // Los largos y formatos son exactamente los mismos que en CreateProveedorValidator
            // y los mismos que declara ProveedorConfig: si crear algo es válido, editarlo con
            // ese mismo valor también lo es.
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
