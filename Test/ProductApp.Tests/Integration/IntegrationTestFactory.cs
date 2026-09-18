using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProductApp.Aplication.BusinessValidator.Modulo_Productos;
using ProductApp.Aplication.BusinessValidator.Modulo_Proveedores;
using ProductApp.Aplication.BusinessValidator.Modulo_Usuarios;
using ProductApp.Aplication.BusinessValidator.Modulo_Ventas;
using ProductApp.Aplication.Mappers;
using ProductApp.Aplication.Mappers.Modulo_Notificaciones;
using ProductApp.Aplication.Mappers.Modulo_Producto;
using ProductApp.Aplication.Mappers.Modulo_Proveedores;
using ProductApp.Aplication.Mappers.Modulo_Reportes;
using ProductApp.Aplication.Mappers.Modulo_Ventas;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Validators.Modulo_Producto.InventarioValidator;
using ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator;
using ProductApp.Aplication.Validators.Modulo_Proveedores.ProveedorValidator;
using ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.DetalleOrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.OrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.PagoValidator;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Tests.Integration
{
    internal static class IntegrationTestFactory
    {
        public static AppDbContext CrearContexto()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        public static NotificacionService CrearNotificacionService(AppDbContext context)
            => new(
                new NotificacionRepository(context),
                new UsuarioRepository(context),
                new NotificacionMapper());

        public static OrdenServices CrearOrdenServices(AppDbContext context)
            => new(
                new OrdenRepository(context),
                new ClienteRepository(context),
                new OrdenMapper(),
                new DetalleOrdenRepository(context),
                new CreateOrdenValidator(),
                new CambiarEstadoOrdenValidator(),
                new ValidatorBusinessOrden(new ClienteRepository(context)),
                CrearNotificacionService(context),
                NullLogger<OrdenServices>.Instance);

        public static DetalleOrdenService CrearDetalleOrdenService(AppDbContext context)
            => new(
                new DetalleOrdenRepository(context),
                new OrdenRepository(context),
                new ProductoRepository(context),
                new OrdenDetalleMapper(),
                new CreateDetalleOrdenValidator(),
                new UpdateDetalleOrdenValidator(),
                new ValidatorBusinessDetalleOrden(
                    new OrdenRepository(context),
                    new DetalleOrdenRepository(context),
                    new ProductoRepository(context)));

        public static PagoService CrearPagoService(AppDbContext context)
            => new(
                new PagoRepository(context),
                new OrdenRepository(context),
                new DetalleOrdenRepository(context),
                new InventarioRepository(context),
                new PagoMapper(),
                new CreatePagoValidator(),
                new ValidatorBusinessPago(),
                CrearNotificacionService(context),
                new FacturaPdfServiceFake(),
                NullLogger<PagoService>.Instance);

        public static ReporteService CrearReporteService(AppDbContext context)
            => new(
                new ReporteRepository(context),
                new InventarioRepository(context),
                new ReporteMapper());

        public static InventarioService CrearInventarioService(AppDbContext context)
            => new(
                new InventarioRepository(context),
                new InventarioMapper(),
                new MovimientoStockValidator(),
                new AjustarStockValidator(),
                new ValidatorBusinessInventario(new ProductoRepository(context)),
                CrearNotificacionService(context),
                NullLogger<InventarioService>.Instance);

        public static ProveedorService CrearProveedorService(AppDbContext context)
            => new(
                new ProveedorRepository(context),
                new ProveedorMapper(),
                new CreateProveedorValidator(),
                new UpdateProveedorValidator(),
                new ValidatorBusinessProveedor(new ProveedorRepository(context)));

        // Orden del constructor verificado contra ProductoServices.cs: inventarioRepository va
        // 7º (después del business validator), aunque el campo se declare 2º.
        public static ProductoServices CrearProductoServices(AppDbContext context)
            => new(
                new ProductoRepository(context),
                new ProductoMapper(),
                new CreateProductoValidator(),
                new UpdateProductoValidator(),
                new SubirImagenProductoValidator(),
                new ValidatorBusinessProducto(new ProductoRepository(context)),
                new InventarioRepository(context),
                new ConfiguracionSistemaRepository(context),
                new AlmacenamientoImagenesFake(),
                NullLogger<ProductoServices>.Instance);

        // Orden del constructor verificado contra UsuarioService.cs: validatorBusinessUsuarios
        // va 5º, antes de los 4 validators de password/rol/perfil restantes.
        public static UsuarioService CrearUsuarioService(AppDbContext context)
            => new(
                new UsuarioRepository(context),
                new UsuarioMapper(),
                new CreateUsuarioValidator(),
                new UpdateUsuarioValidator(),
                new ValidatorBusinessUsuarios(new UsuarioRepository(context)),
                new ChangePasswordUsuarioValidator(),
                new ResetearPasswordUsuarioValidator(),
                new CambiarRolUsuarioValidator(),
                new ActualizarMiPerfilValidator(),
                NullLogger<UsuarioService>.Instance);

        public static ImportacionProductosService CrearImportacionProductosService(
            AppDbContext context,
            ILectorArchivoProductos lector,
            IGeneradorArchivoProductos generador)
            => new(
                lector,
                generador,
                CrearProductoServices(context),
                new ProductoRepository(context),
                new CategoriaRepository(context),
                new ProveedorRepository(context),
                new ImportarProductosValidator(),
                NullLogger<ImportacionProductosService>.Instance);

        public static async Task<(Categoria Categoria, Producto Producto, Inventario Inventario)> SembrarProductoConInventarioAsync(
            AppDbContext context, int cantidadActual, decimal precio = 10)
        {
            var categoria = new Categoria("Categoria Test", "Descripcion de la categoria");
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var producto = new Producto("Producto Test", "Descripcion", precio, precio / 2, categoria.Id);
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var inventario = new Inventario(cantidadActual, cantidadMinima: 1, producto.Id);
            context.Inventario.Add(inventario);
            await context.SaveChangesAsync();

            return (categoria, producto, inventario);
        }

        public static async Task<Cliente> SembrarClienteAsync(AppDbContext context)
        {
            var cliente = new Cliente("Cliente Test", "001-0000000-1", "Calle Falsa 123", "cliente@test.com", "809-000-0000");
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
            return cliente;
        }

        public static async Task<Usuario> SembrarUsuarioAsync(AppDbContext context)
        {
            var usuario = new Usuario("Usuario Test", "usuario@test.com", "usuario.test", RolUsuario.Vendedor);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        // El teléfono va sin guiones a propósito: CreateProveedorValidator exige ^\d{10}$,
        // a diferencia del de Cliente que acepta el formato con guiones.
        public static async Task<Proveedor> SembrarProveedorAsync(
            AppDbContext context,
            string nombre = "Proveedor Test",
            string correo = "proveedor@test.com")
        {
            var proveedor = new Proveedor(nombre, "8090000000", correo, "Av. Principal 1");
            context.Proveedores.Add(proveedor);
            await context.SaveChangesAsync();
            return proveedor;
        }
    }
}
