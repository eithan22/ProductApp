using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Ventas.DetalleOrdenValidator
{
    public class UpdateDetalleOrdenValidator : AbstractValidator<UpdateDetalleOrdenDto>
    {
        public UpdateDetalleOrdenValidator() 
        {

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
                


            


        }

    }
}
