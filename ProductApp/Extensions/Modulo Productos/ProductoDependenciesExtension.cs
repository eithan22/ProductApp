using FluentValidation;
using ProductApp.Aplication.BusinessValidator.Modulo_Productos;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Mappers.Modulo_Producto;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Validators.Modulo_Producto.CategoriaValidator;
using ProductApp.Aplication.Validators.Modulo_Producto.InventarioValidator;
using ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Productos
{
    public static class ProductoDependenciesExtension
    {
        public static IServiceCollection AddModuloProductos(this IServiceCollection services)
        {
            // Repositorios
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IInventarioRepository, InventarioRepository>();

            // Mappers
            services.AddScoped<IMapperCategoria, CategoriaMapper>();
            services.AddScoped<IMapperProducto, ProductoMapper>();
            services.AddScoped<IMapperInventario, InventarioMapper>();

            // Servicios
            services.AddScoped<ICategoriaServices, CategoriaServices>();
            services.AddScoped<IProductoServices, ProductoServices>();
            services.AddScoped<IInventarioServices, InventarioService>();

            // Reglas de negocio
            services.AddScoped<IValidatorBusinessCategoria, ValidatorBusinessCategoria>();
            services.AddScoped<IValidatorBusinessProducto, ValidatorBusinessProducto>();
            services.AddScoped<IValidatorBusinessInventario, ValidatorBusinessInventario>();

            // Validadores DTO — Categorias
            services.AddScoped<IValidator<CreateCategoriaDto>, CreateCategoriaValidator>();
            services.AddScoped<IValidator<UpdateCategoriaDto>, UpdateCategoriaValidator>();

            // Validadores DTO — Productos
            services.AddScoped<IValidator<CreateProductoDto>, CreateProductoValidator>();
            services.AddScoped<IValidator<UpdateProductoDto>, UpdateProductoValidator>();

            // Validadores DTO — Inventario
            services.AddScoped<IValidator<MovimientoStockDto>, MovimientoStockValidator>();
            services.AddScoped<IValidator<AjustarStockDto>, AjustarStockValidator>();

            return services;
        }
    }
}
