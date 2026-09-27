using Azure.Storage.Blobs;
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
using ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto;
using ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Almacenamiento;
using ProductApp.Infraesctructura.Persistencia.Importacion;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Productos
{
    public static class ProductoDependenciesExtension
    {
        private const string ContenedorImagenesPorDefecto = "productos-imagenes";

        public static IServiceCollection AddModuloProductos(this IServiceCollection services, IConfiguration configuration)
        {
            // Almacenamiento de imágenes (Azure Blob Storage / Azurite en desarrollo)
            var storageConnectionString = configuration.GetConnectionString("AzureStorage")!;
            var contenedorImagenes = configuration["AzureStorage:ContenedorImagenes"] ?? ContenedorImagenesPorDefecto;

            // Reintentos acotados: sin esto, con Azurite/Storage caído el SDK reintenta hasta 6
            // veces con backoff exponencial (hasta ~40s) en CADA petición, porque el servicio es
            // Singleton y su guard de "contenedor verificado" nunca queda en true si el intento
            // falla. Con esto, un storage caído falla rápido (unos segundos) en vez de colgar la
            // petición del usuario.
            var opcionesBlob = new BlobClientOptions();
            opcionesBlob.Retry.MaxRetries = 1;
            opcionesBlob.Retry.NetworkTimeout = TimeSpan.FromSeconds(5);

            services.AddSingleton(_ => new BlobServiceClient(storageConnectionString, opcionesBlob));
            services.AddSingleton<IAlmacenamientoImagenes>(sp =>
                new AlmacenamientoImagenesBlob(sp.GetRequiredService<BlobServiceClient>(), contenedorImagenes));

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
            services.AddScoped<IImportacionProductosService, ImportacionProductosService>();

            // Importación masiva (RF-3.10): ClosedXML y CsvHelper quedan encerrados en
            // Infraestructura, igual que QuestPDF en la facturación. El servicio consume además
            // IProveedorRepository, que registra el módulo de Proveedores — no se re-registra acá.
            services.AddScoped<ILectorArchivoProductos, LectorArchivoProductos>();
            services.AddScoped<IGeneradorArchivoProductos, GeneradorArchivoProductosClosedXml>();

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
            services.AddScoped<IValidator<SubirImagenProductoDto>, SubirImagenProductoValidator>();
            services.AddScoped<IValidator<ImportarProductosDto>, ImportarProductosValidator>();

            // Validadores DTO — Inventario
            services.AddScoped<IValidator<MovimientoStockDto>, MovimientoStockValidator>();
            services.AddScoped<IValidator<AjustarStockDto>, AjustarStockValidator>();

            return services;
        }
    }
}
