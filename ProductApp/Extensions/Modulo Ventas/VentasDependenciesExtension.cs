using FluentValidation;
using ProductApp.Aplication.BusinessValidator.Modulo_Ventas;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Mappers.Modulo_Ventas;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Validators.Modulo_Ventas.DetalleOrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.OrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.PagoValidator;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Ventas
{
    public static class VentasDependenciesExtension
    {
        public static IServiceCollection AddModuloVentas(this IServiceCollection services)
        {
            // Repositorios
            services.AddScoped<IOrdenRepository, OrdenRepository>();
            services.AddScoped<IDetalleOrdenRepository, DetalleOrdenRepository>();
            services.AddScoped<IPagoRepository, PagoRepository>();

            // Mappers
            services.AddScoped<IMapperOrden, OrdenMapper>();
            services.AddScoped<IMapperDetalleOrden, OrdenDetalleMapper>();
            services.AddScoped<IMapperPago, PagoMapper>();

            // Servicios
            services.AddScoped<IOrdenServices, OrdenServices>();
            services.AddScoped<IDetalleOrdenServices, DetalleOrdenService>();
            services.AddScoped<IPagoServices, PagoService>();

            // Reglas de negocio
            services.AddScoped<IValidatorBusinessOrden, ValidatorBusinessOrden>();
            services.AddScoped<IValidatorBusinessPago, ValidatorBusinessPago>();

            // Validadores DTO — Orden
            services.AddScoped<IValidator<CreateOrdenDto>, CreateOrdenValidator>();

            // Validadores DTO — Detalle Orden
            services.AddScoped<IValidator<CreateDetalleOrdenDto>, CreateDetalleOrdenValidator>();
            services.AddScoped<IValidator<UpdateDetalleOrdenDto>, UpdateDetalleOrdenValidator>();

            // Validadores DTO — Pago
            services.AddScoped<IValidator<CreatePagoDto>, CreatePagoValidator>();

            return services;
        }
    }
}
