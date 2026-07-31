using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProductApp.Aplication.BusinessValidator.Modulo_Ventas;
using ProductApp.Aplication.Mappers.Modulo_Reportes;
using ProductApp.Aplication.Mappers.Modulo_Ventas;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Validators.Modulo_Ventas.DetalleOrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.OrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.PagoValidator;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
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

        public static OrdenServices CrearOrdenServices(AppDbContext context)
            => new(
                new OrdenRepository(context),
                new ClienteRepository(context),
                new OrdenMapper(),
                new DetalleOrdenRepository(context),
                new CreateOrdenValidator(),
                new CambiarEstadoOrdenValidator(),
                new ValidatorBusinessOrden(new ClienteRepository(context)),
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
                NullLogger<PagoService>.Instance);

        public static ReporteService CrearReporteService(AppDbContext context)
            => new(
                new ReporteRepository(context),
                new InventarioRepository(context),
                new ReporteMapper());

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
    }
}
