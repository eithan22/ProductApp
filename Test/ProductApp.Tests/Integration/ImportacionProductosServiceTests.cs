using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto;
using ProductApp.Domian.Common.Importacion;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class ImportacionProductosServiceTests
    {
        private static ImportarProductosDto Dto(string nombreArchivo = "productos.xlsx", long tamano = 1024)
            => new() { Contenido = new MemoryStream(new byte[] { 1 }), NombreArchivo = nombreArchivo, TamanoBytes = tamano };

        private static FilaArchivoProductos Fila(
            int numeroFila, string? nombre = "Taladro", string? descripcion = "Taladro de impacto",
            string? precio = "185.00", string? costo = "120.00", string? categoria = "Herramientas", string? proveedor = null)
            => new()
            {
                NumeroFila = numeroFila,
                Nombre = nombre,
                Descripcion = descripcion,
                Precio = precio,
                Costo = costo,
                Categoria = categoria,
                Proveedor = proveedor
            };

        private static async Task<Categoria> SembrarCategoriaAsync(Infraesctructura.Persistencia.Contex.AppDbContext context, string nombre = "Herramientas")
        {
            var categoria = new Categoria(nombre, $"Descripción de {nombre}");
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();
            return categoria;
        }

        [Fact]
        public async Task ImportarAsync_ConExtensionNoPermitida_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearImportacionProductosService(
                context, new LectorArchivoProductosFake(ResultadoLecturaArchivoProductos.Leido(Array.Empty<FilaArchivoProductos>())), new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto("productos.pdf"), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("extensión");
        }

        [Fact]
        public async Task ImportarAsync_ConArchivoDe6MB_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearImportacionProductosService(
                context, new LectorArchivoProductosFake(ResultadoLecturaArchivoProductos.Leido(Array.Empty<FilaArchivoProductos>())), new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(tamano: 6 * 1024 * 1024), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("5 MB");
        }

        [Fact]
        public async Task ImportarAsync_ConFormatoNoSoportado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var lector = new LectorArchivoProductosFake(ResultadoLecturaArchivoProductos.FormatoNoSoportado());
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(".xlsx y .csv");
        }

        [Fact]
        public async Task ImportarAsync_ConColumnasFaltantes_LasListaEnElMensaje()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var lector = new LectorArchivoProductosFake(ResultadoLecturaArchivoProductos.FaltanColumnas(new[] { "Precio", "Costo" }));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Precio");
            resultado.Message.Should().Contain("plantilla");
        }

        [Fact]
        public async Task ImportarAsync_ConArchivoSinFilas_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var lector = new LectorArchivoProductosFake(ResultadoLecturaArchivoProductos.Leido(Array.Empty<FilaArchivoProductos>()));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no tiene filas");
        }

        [Fact]
        public async Task ImportarAsync_Con501Filas_RechazaTodoElArchivo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var filas = Enumerable.Range(2, 501).Select(n => Fila(n)).ToArray();
            var lector = new LectorArchivoProductosFake(ResultadoLecturaArchivoProductos.Leido(filas));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("501");
            resultado.Message.Should().Contain("500");
            (await context.Productos.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ImportarAsync_ConNombreVacio_RechazaLaFila()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, nombre: "  "));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasConError.Should().Be(1);
            resultado.Data.Errores[0].Columna.Should().Be("Nombre");
            resultado.Data.Errores[0].Motivo.Should().Contain("obligatorio");
        }

        [Fact]
        public async Task ImportarAsync_ConCategoriaInexistente_RechazaLaFilaConMensajeAccionable()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Errores[0].Columna.Should().Be("Categoria");
            resultado.Data.Errores[0].Motivo.Should().Contain("Créala primero");
            resultado.Data.FilasCreadas.Should().Be(0);
        }

        [Fact]
        public async Task ImportarAsync_ConCategoriaSinTildes_UsaLaExistenteYAvisa()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context, "Electrónica");
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, categoria: "electronica"));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasCreadas.Should().Be(1);
            resultado.Data.Advertencias[0].Columna.Should().Be("Categoria");
            resultado.Data.Advertencias[0].Mensaje.Should().Contain("Electrónica");
        }

        [Fact]
        public async Task ImportarAsync_ConProveedorInexistente_CreaElProductoSinProveedorYAvisa()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, proveedor: "Fantasma S.A."));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasCreadas.Should().Be(1);
            resultado.Data.Advertencias[0].Columna.Should().Be("Proveedor");
            (await context.Productos.SingleAsync()).ProveedorId.Should().BeNull();
        }

        [Fact]
        public async Task ImportarAsync_ConProveedorExistente_LoAsocia()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context, "Ferretería Norte", "norte@test.com");
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, proveedor: "Ferretería Norte"));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Productos.SingleAsync()).ProveedorId.Should().Be(proveedor.Id);
        }

        // Documenta un comportamiento real (no un bug de esta importación): el mensaje dice
        // "no existe" cuando en realidad el proveedor existe pero está inactivo, porque la
        // resolución de proveedores se hace con incluirInactivos: false.
        [Fact]
        public async Task ImportarAsync_ConProveedorInactivo_NoLoAsociaYElMensajeDiceQueNoExiste()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context, "Ferretería Norte", "norte@test.com");
            proveedor.Desactivar();
            await context.SaveChangesAsync();
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, proveedor: "Ferretería Norte"));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Productos.SingleAsync()).ProveedorId.Should().BeNull();
            resultado.Data!.Advertencias[0].Mensaje.Should().Contain("No existe un proveedor");
        }

        [Fact]
        public async Task ImportarAsync_ConPrecioNoNumerico_RechazaLaFilaConEjemploDeFormato()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, precio: "RD$ 185,00"));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Errores[0].Columna.Should().Be("Precio");
            resultado.Data.Errores[0].Motivo.Should().Contain("185.00");
        }

        // BUG-A ya corregido en ImportacionProductosService.TryParseMonto (NumberStyles sin
        // AllowThousands): "185,00" (coma decimal dominicana) ya NO se interpreta como 18500.
        // Ahora la fila se rechaza explícitamente en vez de importar un monto 100 veces mayor.
        [Fact]
        public async Task ImportarAsync_ConPrecioEnFormatoDominicano_RechazaLaFilaEnVezDeMultiplicarPorCien()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, precio: "185,00"));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasCreadas.Should().Be(0);
            resultado.Data.FilasConError.Should().Be(1);
            resultado.Data.Errores[0].Columna.Should().Be("Precio");
            (await context.Productos.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ImportarAsync_ConNombreYCategoriaYaExistentes_LaOmiteComoDuplicada()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var (_, _, _) = (categoria, default(Producto), default(Inventario));
            var productoServices = IntegrationTestFactory.CrearProductoServices(context);
            await productoServices.CreateAsync(new ProductApp.Aplication.Dtos.ProductoDto.CreateProductoDto
            {
                Nombre = "Taladro",
                Descripcion = "Taladro de impacto",
                Precio = 185m,
                Costo = 120m,
                CategoriaId = categoria.Id
            });

            var lector = LectorArchivoProductosFake.ConFilas(Fila(2));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasOmitidas.Should().Be(1);
            resultado.Data.FilasCreadas.Should().Be(0);
            resultado.Data.Duplicadas[0].Nombre.Should().Be("Taladro");
        }

        [Fact]
        public async Task ImportarAsync_ConDosFilasIdenticasEnElMismoArchivo_CreaUnaYOmiteLaOtra()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2), Fila(3));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasCreadas.Should().Be(1);
            resultado.Data.FilasOmitidas.Should().Be(1);
        }

        [Fact]
        public async Task ImportarAsync_ConProductoInactivoDelMismoNombre_TambienLoCuentaComoDuplicado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var productoServices = IntegrationTestFactory.CrearProductoServices(context);
            var creado = await productoServices.CreateAsync(new ProductApp.Aplication.Dtos.ProductoDto.CreateProductoDto
            {
                Nombre = "Taladro",
                Descripcion = "Taladro de impacto",
                Precio = 185m,
                Costo = 120m,
                CategoriaId = categoria.Id
            });
            await productoServices.DisableAsync(creado.Data!.Id);

            var lector = LectorArchivoProductosFake.ConFilas(Fila(2));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasOmitidas.Should().Be(1);
        }

        [Fact]
        public async Task ImportarAsync_ConFilasBuenasYMalas_ImportaLasBuenasYReportaLasMalas()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(
                Fila(2, nombre: "Taladro"),
                Fila(3, nombre: "Martillo"),
                Fila(4, nombre: "Sierra", categoria: "Categoría Inexistente"));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.TotalFilas.Should().Be(3);
            resultado.Data.FilasCreadas.Should().Be(2);
            resultado.Data.FilasConError.Should().Be(1);
            (await context.Productos.CountAsync()).Should().Be(2);
        }

        [Fact]
        public async Task ImportarAsync_ConErrores_AdjuntaElArchivoDescargableEnBase64()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2));
            var generador = new GeneradorArchivoProductosFake();
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, generador);

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.Data!.ArchivoErroresBase64.Should().NotBeNullOrEmpty();
            resultado.Data.NombreArchivoErrores.Should().EndWith(".xlsx");
            generador.VecesQueGeneroReporteErrores.Should().Be(1);
        }

        [Fact]
        public async Task ImportarAsync_SinErrores_NoAdjuntaArchivo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2));
            var generador = new GeneradorArchivoProductosFake();
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, generador);

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.Data!.ArchivoErroresBase64.Should().BeNull();
            generador.VecesQueGeneroReporteErrores.Should().Be(0);
        }

        [Fact]
        public async Task ImportarAsync_CadaProductoCreadoNaceConSuInventario()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context);
            var lector = LectorArchivoProductosFake.ConFilas(Fila(2));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Inventario.CountAsync()).Should().Be(1);
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(0);
        }

        // Documenta un comportamiento real (BUG-E): el criterio de duplicado del importador es
        // Nombre+Categoría, pero ValidatorBusinessProducto bloquea por Nombre global. Una fila
        // con nombre repetido en OTRA categoría no cae como "omitida" sino como error general.
        [Fact]
        public async Task ImportarAsync_ConNombreRepetidoEnOtraCategoria_CaeComoErrorGeneralYNoComoDuplicado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoriaA = await SembrarCategoriaAsync(context, "Herramientas");
            var categoriaB = await SembrarCategoriaAsync(context, "Ferretería");
            var productoServices = IntegrationTestFactory.CrearProductoServices(context);
            await productoServices.CreateAsync(new ProductApp.Aplication.Dtos.ProductoDto.CreateProductoDto
            {
                Nombre = "Taladro",
                Descripcion = "Taladro de impacto",
                Precio = 185m,
                Costo = 120m,
                CategoriaId = categoriaA.Id
            });

            var lector = LectorArchivoProductosFake.ConFilas(Fila(2, categoria: "Ferretería"));
            var service = IntegrationTestFactory.CrearImportacionProductosService(context, lector, new GeneradorArchivoProductosFake());

            var resultado = await service.ImportarAsync(Dto(), usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.FilasOmitidas.Should().Be(0);
            resultado.Data.FilasConError.Should().Be(1);
            resultado.Data.Errores[0].Columna.Should().Be("General");
        }

        [Fact]
        public void ObtenerPlantilla_DevuelveElBinarioDelGenerador()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearImportacionProductosService(
                context, new LectorArchivoProductosFake(ResultadoLecturaArchivoProductos.Leido(Array.Empty<FilaArchivoProductos>())), new GeneradorArchivoProductosFake());

            var resultado = service.ObtenerPlantilla();

            resultado.Data.Should().Equal(1, 2, 3);
        }
    }
}
