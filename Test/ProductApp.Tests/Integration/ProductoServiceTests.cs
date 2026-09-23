using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Common.Exceptions.ExceptionsProducto;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Complementa a ProductoServiceCreateTests (que cubre solo CreateAsync + inventario inicial)
    // con el resto de ProductoServices: update, ciclo de vida, listados, búsqueda e imagen.
    public class ProductoServiceTests
    {
        private static async Task<Categoria> SembrarCategoriaAsync(
            AppDbContext context,
            string nombre = "Ferretería",
            string descripcion = "Herramientas y tornillería")
        {
            var categoria = new Categoria(nombre, descripcion);
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();
            return categoria;
        }

        private static async Task<Producto> SembrarProductoAsync(
            AppDbContext context,
            int categoriaId,
            string nombre = "Martillo",
            decimal precio = 100m,
            decimal costo = 50m)
        {
            var producto = new Producto(nombre, "Descripción original", precio, costo, categoriaId);
            context.Productos.Add(producto);
            await context.SaveChangesAsync();
            return producto;
        }

        private static UpdateProductoDto UpdateDto(
            int id,
            int categoriaId,
            string nombre = "Martillo",
            string descripcion = "Descripción editada",
            decimal precio = 150m,
            decimal costo = 70m)
            => new()
            {
                Id = id,
                Nombre = nombre,
                Descripcion = descripcion,
                Precio = precio,
                Costo = costo,
                CategoriaId = categoriaId
            };

        private static CreateProductoDto CreateDto(int categoriaId, string nombre = "Martillo")
            => new() { Nombre = nombre, Descripcion = "Descripción", Precio = 100m, Costo = 50m, CategoriaId = categoriaId };

        private static SubirImagenProductoDto ImagenDto(
            int productoId,
            string nombreArchivo = "foto.png",
            string contentType = "image/png",
            long tamanoBytes = 1024)
            => new()
            {
                ProductoId = productoId,
                Contenido = new MemoryStream(new byte[] { 1, 2, 3 }),
                NombreArchivo = nombreArchivo,
                ContentType = contentType,
                TamanoBytes = tamanoBytes
            };

        // ---------------------------------------------------------------
        // UpdateAsync / ValidarUpdateProductoAsync
        // ---------------------------------------------------------------

        // El caso que la regla `p.Id != dto.Id` existe para evitar: editar el precio de un
        // producto sin tocarle el nombre no puede leerse como "ya existe otro con ese nombre".
        [Fact]
        public async Task UpdateAsync_ConservandoSuPropioNombre_NoSeDetectaComoDuplicado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.UpdateAsync(UpdateDto(producto.Id, categoria.Id, nombre: "Martillo"));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            // Sin limpiar el tracker, FindAsync devolvería la misma instancia ya mutada en
            // memoria y el test pasaría aunque el servicio nunca hubiese guardado.
            context.ChangeTracker.Clear();
            var enBase = await context.Productos.FindAsync(producto.Id);
            enBase!.Nombre.Should().Be("Martillo");
            enBase.Precio.Should().Be(150m);
            enBase.Costo.Should().Be(70m);
            enBase.Descripcion.Should().Be("Descripción editada");
        }

        [Fact]
        public async Task UpdateAsync_ConElNombreDeOtroProducto_DevuelveFailureYNoPersiste()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            await SembrarProductoAsync(context, categoria.Id, "Martillo");
            var destornillador = await SembrarProductoAsync(context, categoria.Id, "Destornillador");
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.UpdateAsync(UpdateDto(destornillador.Id, categoria.Id, nombre: "Martillo"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Ya existe otro producto con ese nombre");
            (await context.Productos.FindAsync(destornillador.Id))!.Nombre.Should().Be("Destornillador");
        }

        [Fact]
        public async Task UpdateAsync_ConIdInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.UpdateAsync(UpdateDto(id: 9999, categoriaId: categoria.Id));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no encontrado");
        }

        [Fact]
        public async Task UpdateAsync_ConPrecioCero_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.UpdateAsync(UpdateDto(producto.Id, categoria.Id, precio: 0));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("precio debe ser mayor a 0");
            (await context.Productos.FindAsync(producto.Id))!.Precio.Should().Be(100m);
        }

        // Un producto soft-deleted ya no es alcanzable por GetByIdAsync, así que el update
        // devuelve "no encontrado" antes de llegar al validador de negocio.
        [Fact]
        public async Task UpdateAsync_SobreProductoEliminado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            producto.Eliminar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.UpdateAsync(UpdateDto(producto.Id, categoria.Id));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no encontrado");
        }

        // Editar un producto inactivo está permitido y no lo reactiva: el estado es
        // independiente del resto de los campos.
        [Fact]
        public async Task UpdateAsync_SobreProductoInactivo_LoActualizaSinReactivarlo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);
            (await service.DisableAsync(producto.Id)).IsSuccess.Should().BeTrue();

            var resultado = await service.UpdateAsync(UpdateDto(producto.Id, categoria.Id, precio: 200m));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Estado.Should().Be("Inactivo");
            context.ChangeTracker.Clear();
            (await context.Productos.FindAsync(producto.Id))!.Precio.Should().Be(200m);
        }

        // ProveedorId null en el dto no significa "no tocar": es la forma explícita de
        // dejar el producto sin proveedor. ImagenUrl null sí conserva la imagen actual.
        [Fact]
        public async Task UpdateAsync_ConProveedorIdNull_QuitaElProveedorPeroConservaLaImagen()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            producto.AsignarProveedor(proveedor.Id);
            producto.AsignarImagen("https://fake-blob/imagenes/vieja.png");
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var dto = UpdateDto(producto.Id, categoria.Id);
            dto.ProveedorId = null;
            dto.ImagenUrl = null;

            var resultado = await service.UpdateAsync(dto);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            context.ChangeTracker.Clear();
            var enBase = await context.Productos.FindAsync(producto.Id);
            enBase!.ProveedorId.Should().BeNull();
            enBase.ImagenUrl.Should().Be("https://fake-blob/imagenes/vieja.png");
        }

        // El caso opuesto al anterior: si el dto sí trae ImagenUrl, el update la aplica.
        [Fact]
        public async Task UpdateAsync_ConImagenUrlEnElDto_ReemplazaLaImagenActual()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            producto.AsignarImagen("https://fake-blob/imagenes/vieja.png");
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var dto = UpdateDto(producto.Id, categoria.Id);
            dto.ImagenUrl = "https://fake-blob/imagenes/nueva.png";

            var resultado = await service.UpdateAsync(dto);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.ImagenUrl.Should().Be("https://fake-blob/imagenes/nueva.png");
            context.ChangeTracker.Clear();
            (await context.Productos.FindAsync(producto.Id))!.ImagenUrl
                .Should().Be("https://fake-blob/imagenes/nueva.png");
        }

        // UpdateProductoValidator ahora sí valida ProveedorId cuando viene con valor, así que
        // un id negativo se corta en la validación del dto y nunca llega a la entidad: sale
        // como Failure con mensaje del servicio y no como excepción de dominio.
        [Fact]
        public async Task UpdateAsync_ConProveedorIdNegativo_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var dto = UpdateDto(producto.Id, categoria.Id);
            dto.ProveedorId = -3;

            var resultado = await service.UpdateAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("proveedor válido");
            // El dto se rechaza antes de que el mapper toque la entidad: nada queda a medias.
            context.ChangeTracker.Clear();
            var enBase = await context.Productos.FindAsync(producto.Id);
            enBase!.Precio.Should().Be(100m);
            enBase.Descripcion.Should().Be("Descripción original");
        }

        [Fact]
        public async Task UpdateAsync_ConProveedorInexistente_DevuelveFailureYNoModificaElProducto()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var dto = UpdateDto(producto.Id, categoria.Id);
            dto.ProveedorId = 9999;

            var resultado = await service.UpdateAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("proveedor seleccionado no existe");
            context.ChangeTracker.Clear();
            (await context.Productos.FindAsync(producto.Id))!.Precio.Should().Be(100m);
        }

        [Fact]
        public async Task UpdateAsync_ConProveedorInactivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            proveedor.Desactivar();
            await context.SaveChangesAsync();
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var dto = UpdateDto(producto.Id, categoria.Id);
            dto.ProveedorId = proveedor.Id;

            var resultado = await service.UpdateAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no está activo");
        }

        [Fact]
        public async Task UpdateAsync_ConCategoriaInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.UpdateAsync(UpdateDto(producto.Id, categoriaId: 9999));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("categoría seleccionada no existe");
        }

        // Tras guardar, UpdateAsync recarga el producto con Categoría, Inventario y Proveedor,
        // así que la respuesta del PUT trae los mismos campos que la del listado.
        [Fact]
        public async Task UpdateAsync_DevuelveLaRespuestaConCategoriaYStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (categoria, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 7);
            // Sin limpiar el tracker, EF rellena las navegaciones desde memoria y el test
            // pasaría por un motivo que no ocurre contra SQL Server.
            context.ChangeTracker.Clear();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.UpdateAsync(UpdateDto(producto.Id, categoria.Id, nombre: producto.Nombre));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Categoria.Should().Be("Categoria Test");
            resultado.Data.StockActual.Should().Be(7);
            resultado.Data.StockMinimo.Should().Be(1);
        }

        // ---------------------------------------------------------------
        // CreateAsync: unicidad de nombre frente al ciclo de vida
        // ---------------------------------------------------------------

        // Desactivar NO libera el nombre: DisableAsync solo cambia Estado, no marca
        // EstaEliminado, y ExisteAsync filtra por EstaEliminado. Es lo contrario de
        // Categoría, donde desactivar sí libera el nombre.
        [Fact]
        public async Task CreateAsync_ConElNombreDeUnProductoInactivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id, "Martillo");
            var service = IntegrationTestFactory.CrearProductoServices(context);
            (await service.DisableAsync(producto.Id)).IsSuccess.Should().BeTrue();

            var resultado = await service.CreateAsync(CreateDto(categoria.Id, "Martillo"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Ya existe un producto con ese nombre");
            (await context.Productos.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConElNombreDeUnProductoEliminado_LoPermite()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id, "Martillo");
            producto.Eliminar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.CreateAsync(CreateDto(categoria.Id, "Martillo"));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Productos.CountAsync()).Should().Be(2);
        }

        // ---------------------------------------------------------------
        // DisableAsync / EnableProducto
        // ---------------------------------------------------------------

        [Fact]
        public async Task DisableAsync_SacaAlProductoDelListadoPorDefectoPeroNoLoBorra()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.DisableAsync(producto.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await service.GetAllAsync()).Data!.TotalCount.Should().Be(0);
            (await service.GetAllAsync(incluirInactivos: true)).Data!.TotalCount.Should().Be(1);
            var enBase = await context.Productos.FindAsync(producto.Id);
            enBase!.EstaEliminado.Should().BeFalse();
        }

        // Acá sí entra el validador de negocio (a diferencia de Categoría, donde el producto
        // inactivo desaparecía del GetByIdAsync y salía "no encontrada"): el producto
        // inactivo sigue siendo alcanzable, así que el mensaje es el correcto del dominio.
        [Fact]
        public async Task DisableAsync_SobreProductoYaInactivo_DevuelveFailureDeReglaDeNegocio()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);
            await service.DisableAsync(producto.Id);

            var resultado = await service.DisableAsync(producto.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("ya está inactivo");
        }

        [Fact]
        public async Task EnableProducto_SobreProductoInactivo_LoReactiva()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);
            await service.DisableAsync(producto.Id);

            var resultado = await service.EnableProducto(producto.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await service.GetAllAsync()).Data!.TotalCount.Should().Be(1);
        }

        // EnableProducto no pasa por ningún validador de negocio antes de ActivarProducto():
        // el caso "ya está activo" sale como excepción de dominio y no como Failure. Mismo
        // comportamiento que EnableCategoria y EnableProveedor.
        [Fact]
        public async Task EnableProducto_SobreProductoYaActivo_LanzaEstadoInvalidoException()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var accion = async () => await service.EnableProducto(producto.Id);

            await accion.Should().ThrowAsync<EstadoInvalidoException>();
        }

        // ---------------------------------------------------------------
        // GetById / GetAll
        // ---------------------------------------------------------------

        // GetProductoConCategoriaByIdAsync incluye Categoria, Inventario y Proveedor, así que
        // el detalle de un producto trae el stock igual que el listado.
        [Fact]
        public async Task GetByIdAsync_DevuelveCategoriaYStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 7);
            context.ChangeTracker.Clear();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.GetByIdAsync(producto.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Categoria.Should().Be("Categoria Test");
            resultado.Data.StockActual.Should().Be(7);
            resultado.Data.StockMinimo.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ConIdInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.GetByIdAsync(9999);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no encontrado");
        }

        [Fact]
        public async Task GetAllAsync_DevuelveCategoriaYStockEnElListado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 7);
            context.ChangeTracker.Clear();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.GetAllAsync();

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var item = resultado.Data!.Items.Should().ContainSingle().Subject;
            item.Categoria.Should().Be("Categoria Test");
            item.StockActual.Should().Be(7);
            item.StockMinimo.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_SegundaPagina_DevuelveElRestoYCalculaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            for (var i = 1; i <= 5; i++)
                await SembrarProductoAsync(context, categoria.Id, $"Producto {i}");
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.GetAllAsync(pageNumber: 2, pageSize: 2);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.TotalCount.Should().Be(5);
            resultado.Data.TotalPages.Should().Be(3);
            resultado.Data.Items.Select(p => p.Nombre).Should().Equal("Producto 3", "Producto 4");
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetAllAsync_ConPaginacionFueraDeRango_DevuelveFailure(int pageNumber, int pageSize)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.GetAllAsync(pageNumber, pageSize);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task OperacionesPorId_ConIdInvalido_DevuelvenFailureSinTocarLaBase(int id)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            (await service.GetByIdAsync(id)).IsSuccess.Should().BeFalse();
            (await service.DisableAsync(id)).IsSuccess.Should().BeFalse();
            (await service.EnableProducto(id)).IsSuccess.Should().BeFalse();
            (await service.DeleteAsync(id)).IsSuccess.Should().BeFalse();
        }

        [Theory]
        [InlineData(9999)]
        public async Task OperacionesPorId_ConIdInexistente_DevuelvenFailure(int id)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            (await service.DisableAsync(id)).IsSuccess.Should().BeFalse();
            (await service.EnableProducto(id)).IsSuccess.Should().BeFalse();
            (await service.DeleteAsync(id)).IsSuccess.Should().BeFalse();
        }

        // ---------------------------------------------------------------
        // BuscarProductosPorNombreOCategoria
        // ---------------------------------------------------------------

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public async Task BuscarProductos_SinCriterios_DevuelveFailure(string? nombre, string? categoria)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.BuscarProductosPorNombreOCategoria(nombre, categoria);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("al menos un criterio");
        }

        // "No hay resultados" es un estado vacío exitoso, no un error: la UI muestra la
        // lista vacía con su mensaje en vez de un cartel rojo.
        [Fact]
        public async Task BuscarProductos_SinCoincidencias_DevuelveListaVaciaConSuccess()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.BuscarProductosPorNombreOCategoria("Inexistente", null);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().BeEmpty();
            resultado.Message.Should().Contain("No se encontraron productos");
        }

        [Fact]
        public async Task BuscarProductos_PorNombreParcial_DevuelveElProductoConSuStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 7);
            context.ChangeTracker.Clear();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.BuscarProductosPorNombreOCategoria("Producto", null);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var item = resultado.Data.Should().ContainSingle().Subject;
            item.Nombre.Should().Be("Producto Test");
            item.StockActual.Should().Be(7);
        }

        [Fact]
        public async Task BuscarProductos_PorCategoria_DevuelveSoloLosDeEsaCategoria()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var ferreteria = await SembrarCategoriaAsync(context, "Ferretería", "Herramientas");
            var pinturas = await SembrarCategoriaAsync(context, "Pinturas", "Pintura y solventes");
            await SembrarProductoAsync(context, ferreteria.Id, "Martillo");
            await SembrarProductoAsync(context, pinturas.Id, "Brocha");
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.BuscarProductosPorNombreOCategoria(null, "Pinturas");

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().ContainSingle(p => p.Nombre == "Brocha");
        }

        [Fact]
        public async Task BuscarProductos_ProductoInactivo_SoloApareceConIncluirInactivos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id, "Martillo");
            var service = IntegrationTestFactory.CrearProductoServices(context);
            await service.DisableAsync(producto.Id);

            (await service.BuscarProductosPorNombreOCategoria("Martillo", null)).Data.Should().BeEmpty();
            (await service.BuscarProductosPorNombreOCategoria("Martillo", null, incluirInactivos: true))
                .Data.Should().ContainSingle();
        }

        // ---------------------------------------------------------------
        // DeleteAsync
        // ---------------------------------------------------------------

        // DeleteAsync es borrado físico (no soft delete) y hoy no tiene ninguna regla de
        // negocio que lo frene, ni siquiera si el producto tiene inventario o ventas.
        [Fact]
        public async Task DeleteAsync_BorraElProductoFisicamente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.DeleteAsync(producto.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Productos.CountAsync()).Should().Be(0);
        }

        // ---------------------------------------------------------------
        // SubirImagenAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task SubirImagenAsync_GuardaLaUrlDevueltaPorElAlmacenamiento()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.SubirImagenAsync(ImagenDto(producto.Id));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.ImagenUrl.Should().Be("https://fake-blob/imagenes/foto.png");
            context.ChangeTracker.Clear();
            (await context.Productos.FindAsync(producto.Id))!.ImagenUrl
                .Should().Be("https://fake-blob/imagenes/foto.png");
        }

        [Fact]
        public async Task SubirImagenAsync_ReemplazandoUnaImagenPrevia_DejaLaNueva()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);
            await service.SubirImagenAsync(ImagenDto(producto.Id, "vieja.png"));

            var resultado = await service.SubirImagenAsync(ImagenDto(producto.Id, "nueva.webp", "image/webp"));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            context.ChangeTracker.Clear();
            (await context.Productos.FindAsync(producto.Id))!.ImagenUrl
                .Should().Be("https://fake-blob/imagenes/nueva.webp");
        }

        [Theory]
        [InlineData("documento.pdf", "application/pdf", 1024)]
        [InlineData("foto.png", "image/png", 0)]
        [InlineData("foto.png", "image/png", 5 * 1024 * 1024 + 1)]
        public async Task SubirImagenAsync_ConArchivoNoPermitido_DevuelveFailureYNoTocaElProducto(
            string nombreArchivo, string contentType, long tamanoBytes)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var producto = await SembrarProductoAsync(context, categoria.Id);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.SubirImagenAsync(ImagenDto(producto.Id, nombreArchivo, contentType, tamanoBytes));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Error de validación");
            (await context.Productos.FindAsync(producto.Id))!.ImagenUrl.Should().BeNull();
        }

        [Fact]
        public async Task SubirImagenAsync_ConProductoInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.SubirImagenAsync(ImagenDto(productoId: 9999));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Producto no encontrado");
        }
    }
}
