using FluentValidation;
using ProductApp.Aplication.Dtos.CategoriaDto;

namespace ProductApp.Aplication.Validators.Modulo_Producto.CategoriaValidator
{
    public class CreateCategoriaValidator : AbstractValidator<CreateCategoriaDto>
    {
        public CreateCategoriaValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres.");

            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es requerida.")
                .MaximumLength(100).WithMessage("La descripción no puede exceder los 100 caracteres.");
        }
    }
}
