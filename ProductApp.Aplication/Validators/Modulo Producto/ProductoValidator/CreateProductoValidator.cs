using FluentValidation;
using Microsoft.Identity.Client;
using ProductApp.Aplication.Dtos.ProductoDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator
{
    public class CreateProductoValidator : AbstractValidator<CreateProductoDto>
    {
        public CreateProductoValidator() 
        {
            RuleFor(x => x.Nombre)
                .MaximumLength(30).WithMessage("El nombre no puede exederce de 30 caracteres ")
                .NotEmpty().WithMessage("El Nombre es requerido");

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
