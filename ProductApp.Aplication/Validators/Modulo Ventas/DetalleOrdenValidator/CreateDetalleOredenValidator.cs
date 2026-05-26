using FluentValidation;
using Microsoft.Identity.Client;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Ventas.DetalleOrdenValidator
{
    public class CreateDetalleOredenValidator : AbstractValidator<CreateDetalleOrdenDto>
    {
        public CreateDetalleOredenValidator()
        {
           RuleFor(x => x.OrdenId)
                .GreaterThan(0).WithMessage("El Id de la orden debe ser mayor que cero.");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("El Id del producto debe ser mayor que cero.");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
                



           
        }
    }
}
