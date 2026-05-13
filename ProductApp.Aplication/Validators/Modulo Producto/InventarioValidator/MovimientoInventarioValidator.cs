using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Producto.InventarioValidator
{
    public class MovimientoStockValidator : AbstractValidator<MovimientoStockDto>
    {
        public MovimientoStockValidator()
        {
           RuleFor(x => x.ProductoId)
                .NotEmpty().WithMessage("El Id del producto es obligatorio.")
                .GreaterThan(0).WithMessage("El Id del producto debe ser mayor que cero.");

            RuleFor(x => x.cantidad)
                .NotEmpty().WithMessage("La cantidad es obligatoria.")
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
                

        }
    }
}
