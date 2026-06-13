using FluentValidation;
using ProductApp.Aplication.Dtos.OrdenDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Ventas.OrdenValidator
{
    public class CreateOrdenValidator : AbstractValidator<CreateOrdenDto>
    {
        public CreateOrdenValidator() 
        { 

            RuleFor(x => x.ClienteId)
                 .GreaterThan(0).WithMessage("El Id de la cliente debe ser mayor que cero.");

            
















        }
    }
}
