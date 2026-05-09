using FluentValidation;
using ProductApp.Aplication.Dtos.CategoriaDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Producto.CategoriaValidator
{
    public class UpdateCategoriaValidator : AbstractValidator<UpdateCategoriaDto>
    {
            public UpdateCategoriaValidator() 
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("El Id debe ser mayor que 0.");

              RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres.");


            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripcion es requerida.")
            .MaximumLength(200).WithMessage("La descripcion no puede exceder los 200 caracteres.");


        }


    }
}
