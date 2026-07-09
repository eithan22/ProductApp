using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Configuracion;

namespace ProductApp.Aplication.Validators.Modulo_Configuracion
{
    public class ActualizarConfiguracionSistemaValidator : AbstractValidator<ActualizarConfiguracionSistemaDto>
    {
        public ActualizarConfiguracionSistemaValidator()
        {
            RuleFor(x => x.CantidadMinimaInventarioDefecto)
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad mínima de inventario por defecto no puede ser negativa.");

            RuleFor(x => x.DuracionTokenMinutos)
                .GreaterThan(0).WithMessage("La duración del token debe ser mayor a cero.");

            RuleFor(x => x.NombreEmpresa)
                .NotEmpty().WithMessage("El nombre de la empresa es requerido.")
                .MaximumLength(100).WithMessage("El nombre de la empresa no puede exceder los 100 caracteres.");

            RuleFor(x => x.Moneda)
                .NotEmpty().WithMessage("La moneda es requerida.")
                .MaximumLength(10).WithMessage("La moneda no puede exceder los 10 caracteres.");
        }
    }
}
