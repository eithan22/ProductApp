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
using ProductApp.Aplication.Validators.Modulo_Producto.CategoriaValidator;
using ProductApp.Aplication.Validators.Modulo_Producto.InventarioValidator;
using ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator;
using ProductApp.Aplication.Validators.Modulo_Proveedores.ProveedorValidator;
using ProductApp.Aplication.Validators.Modulo_Usuario.ClienteValidator;
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
                new PagoRepository(context),
                new CreateOrdenValidator(),
                new CambiarEstadoOrdenValidator(),
                new ValidatorBusinessOrden(new ClienteRepository(context)),
                CrearNotificacionService(context),
                NullLogger<OrdenServices>.Instance);

        // gestorTransacciones y ordenRepository son opcionales (mismo patrón que
        // almacenamientoImagenes en CrearProductoServices): los tests de transacción pasan un
        // espía o un repositorio que falla a propósito, el resto sigue llamando con (context).
        public static DetalleOrdenService CrearDetalleOrdenService(
            AppDbContext context,
            GestorTransaccionesFake? gestorTransacciones = null,
            IOrdenRepository? ordenRepository = null)
            => new(
                new DetalleOrdenRepository(context),
                ordenRepository ?? new OrdenRepository(context),
                new ProductoRepository(context),
                new OrdenDetalleMapper(),
                new CreateDetalleOrdenValidator(),
                new UpdateDetalleOrdenValidator(),
                new ValidatorBusinessDetalleOrden(
                    new OrdenRepository(context),
                    new DetalleOrdenRepository(context),
                    new ProductoRepository(context),
                    new PagoRepository(context)),
                gestorTransacciones ?? new GestorTransaccionesFake(),
                NullLogger<DetalleOrdenService>.Instance);

        public static PagoService CrearPagoService(AppDbContext context)
            => new(
                new PagoRepository(context),
                new OrdenRepository(context),
                new DetalleOrdenRepository(context),
                new InventarioRepository(context),
                new GestorTransaccionesFake(),
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

        public static CategoriaServices CrearCategoriaServices(AppDbContext context)
            => new(
                new CategoriaRepository(context),
                new CategoriaMapper(),
                new CreateCategoriaValidator(),
                new UpdateCategoriaValidator(),
                new ValidatorBusinessCategoria(new CategoriaRepository(context)));

        // Orden del constructor verificado contra ClienteServices.cs: el validator de
        // Update va 3º y el de Create 4º, al revés que en el resto de los servicios.
        public static ClienteServices CrearClienteServices(AppDbContext context)
            => new(
                new ClienteRepository(context),
                new ClienteMappers(),
                new UpdateClienteValidator(),
                new CreateClienteValidator(),
                new ValidatorBusinessClientes(new ClienteRepository(context)));

        public static ProveedorService CrearProveedorService(AppDbContext context)
            => new(
                new ProveedorRepository(context),
                new ProveedorMapper(),
                new CreateProveedorValidator(),
                new UpdateProveedorValidator(),
                new ValidatorBusinessProveedor(new ProveedorRepository(context)));

        // Orden del constructor verificado contra ProductoServices.cs: inventarioRepository va
        // 7º (después del business validator), aunque el campo se declare 2º, y
        // gestorTransacciones va 10º, entre almacenamientoImagenes y el logger.
        public static ProductoServices CrearProductoServices(
            AppDbContext context,
            AlmacenamientoImagenesFake? almacenamientoImagenes = null,
            GestorTransaccionesFake? gestorTransacciones = null,
            IInventarioRepository? inventarioRepository = null)
            => new(
                new ProductoRepository(context),
                new ProductoMapper(),
                new CreateProductoValidator(),
                new UpdateProductoValidator(),
                new SubirImagenProductoValidator(),
                new ValidatorBusinessProducto(
                    new ProductoRepository(context),
                    new CategoriaRepository(context),
                    new ProveedorRepository(context)),
                inventarioRepository ?? new InventarioRepository(context),
                new ConfiguracionSistemaRepository(context),
                almacenamientoImagenes ?? new AlmacenamientoImagenesFake(),
                gestorTransacciones ?? new GestorTransaccionesFake(),
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

        public static async Task<Cliente> SembrarClienteAsync(
            AppDbContext context,
            string nombre = "Cliente Test",
            string cedula = "001-0000000-1",
            string correo = "cliente@test.com",
            string telefono = "809-000-0000")
        {
            var cliente = new Cliente(nombre, cedula, "Calle Falsa 123", correo, telefono);
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
            return cliente;
        }

        // Replica exactamente lo que siembra DbInitializer en producción: el reservado se
        // reconoce por la cédula, así que los datos tienen que salir de las constantes
        // de la entidad y no de literales escritos a mano en el test.
        public static async Task<Cliente> SembrarConsumidorFinalAsync(AppDbContext context)
        {
            var cliente = new Cliente(
                Cliente.NombreConsumidorFinal,
                Cliente.CedulaConsumidorFinal,
                Cliente.DireccionConsumidorFinal,
                Cliente.CorreoConsumidorFinal,
                Cliente.TelefonoConsumidorFinal);

            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
            return cliente;
        }

        public static async Task<Usuario> SembrarUsuarioAsync(
            AppDbContext context,
            string nombre = "Usuario Test",
            string correo = "usuario@test.com",
            string nombreUsuario = "usuario.test")
        {
            var usuario = new Usuario(nombre, correo, nombreUsuario, RolUsuario.Vendedor);
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
