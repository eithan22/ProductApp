using FluentValidation;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator
{
    public class CreateProductoValidator : AbstractValidator<CreateProductoDto>
    {
        public CreateProductoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres.");

            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es requerida.")
                .MaximumLength(100).WithMessage("La descripción no puede exceder los 100 caracteres.");

            RuleFor(x => x.Costo)
                .GreaterThan(0).WithMessage("El costo debe ser mayor a 0.")
                .LessThanOrEqualTo(Producto.MontoMaximo)
                .WithMessage($"El costo no puede superar los {Producto.MontoMaximo}.");

            RuleFor(x => x.Precio)
                .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.")
                .LessThanOrEqualTo(Producto.MontoMaximo)
                .WithMessage($"El precio no puede superar los {Producto.MontoMaximo}.");

            RuleFor(x => x.CategoriaId)
                .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida.");

            // Solo si viene un valor: null es válido y significa "producto sin proveedor".
            RuleFor(x => x.ProveedorId)
                .GreaterThan(0).WithMessage("Debe seleccionar un proveedor válido.")
                .When(x => x.ProveedorId.HasValue);
        }
    }
}
