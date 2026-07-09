using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Producto.InventarioValidator
{
    public class AjustarStockValidator : AbstractValidator<AjustarStockDto>
    {
        public AjustarStockValidator()
        {
            RuleFor(x => x.ProductoId)
                 .NotEmpty().WithMessage("El Id del producto es obligatorio.")
                 .GreaterThan(0).WithMessage("El Id del producto debe ser mayor que cero.");

            RuleFor(x => x.NuevoStock)
                 .NotEmpty().WithMessage("El nuevo stock es obligatorio.")
                 .GreaterThanOrEqualTo(0).WithMessage("El nuevo stock debe ser mayor o igual a cero.");

            RuleFor(x => x.NuevoStockMinimo)
                 .GreaterThanOrEqualTo(0).WithMessage("El nuevo stock mínimo debe ser mayor o igual a cero.")
                 .When(x => x.NuevoStockMinimo.HasValue);
        }
    }
}
