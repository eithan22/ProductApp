using FluentValidation;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator
{
    public class UpdateProductoValidator : AbstractValidator<UpdateProductoDto>
    {
        public UpdateProductoValidator() 
        {

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El id es requerido")
                .GreaterThan(0).WithMessage("EL id debe ser mayor a 0 ");

            RuleFor(x => x.Descripcion)
              .MaximumLength(50).WithMessage("La descripcion no puede exederce de 50 caracteres")
              .NotEmpty().WithMessage("La descripcion es requerida");

            RuleFor(x => x.Costo)
                .NotEmpty().WithMessage("El Costo es requerido")
                .GreaterThan(0).WithMessage("El Costo debe ser mayor a 0.");

            RuleFor(x => x.Precio)
                 .NotEmpty().WithMessage("El precio es requerido")
                .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");


        }
    }
}
