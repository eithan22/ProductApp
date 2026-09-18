using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Domian.Common.Exceptions;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class ProveedorServiceTests
    {
        private static CreateProveedorDto CrearDto(
            string nombre = "Distribuidora Del Este",
            string correo = "ventas@deleste.com")
            => new() { Nombre = nombre, Telefono = "8095551234", Correo = correo, Direccion = "Av. España 45" };

        [Fact]
        public async Task CreateAsync_ConDatosValidos_GuardaElProveedorActivo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Activo.Should().BeTrue();
            (await context.Proveedores.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConTelefonoDeNueveDigitos_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProveedorService(context);
            var dto = CrearDto();
            dto.Telefono = "809555123";

            var resultado = await service.CreateAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
            (await context.Proveedores.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CreateAsync_ConCorreoMalFormado_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProveedorService(context);
            var dto = CrearDto(correo: "no-es-un-correo");

            var resultado = await service.CreateAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_ConNombreYaRegistrado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await IntegrationTestFactory.SembrarProveedorAsync(context, nombre: "Distribuidora Del Este", correo: "otro@test.com");
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("nombre ya está registrado");
        }

        // Riesgo de negocio real: desactivar un proveedor NO libera su nombre ni su correo,
        // porque ExisteAsync filtra por EstaEliminado y la baja del proveedor es por Estado.
        [Fact]
        public async Task CreateAsync_ConElNombreDeUnProveedorInactivo_SigueDevolviendoFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(
                context, nombre: "Distribuidora Del Este", correo: "otro@test.com");
            proveedor.Desactivar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("nombre ya está registrado");
        }

        [Fact]
        public async Task UpdateAsync_SobreProveedorInactivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            proveedor.Desactivar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.UpdateAsync(new UpdateProveedorDto
            {
                Id = proveedor.Id,
                Nombre = "Nombre Nuevo",
                Telefono = "8095551234",
                Correo = "nuevo@test.com",
                Direccion = "Calle 2"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("inactivo");
        }

        [Fact]
        public async Task UpdateAsync_GuardandoLosMismosDatos_NoSeDetectaComoDuplicado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.UpdateAsync(new UpdateProveedorDto
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = "8090000000",
                Correo = proveedor.Correo,
                Direccion = "Av. Principal 1 (corregida)"
            });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Direccion.Should().Be("Av. Principal 1 (corregida)");
        }

        [Fact]
        public async Task UpdateAsync_ConIdInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.UpdateAsync(new UpdateProveedorDto
            {
                Id = 999,
                Nombre = "Cualquiera",
                Telefono = "8095551234",
                Correo = "x@test.com",
                Direccion = "Calle 1"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no fue encontrado");
        }

        [Fact]
        public async Task DisableAsync_SacaAlProveedorDelListadoPorDefectoPeroNoDeLaBase()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.DisableAsync(proveedor.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await service.GetAllAsync()).Data!.TotalCount.Should().Be(0);
            (await service.GetAllAsync(incluirInactivos: true)).Data!.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task DisableAsync_SobreProveedorYaInactivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var service = IntegrationTestFactory.CrearProveedorService(context);
            await service.DisableAsync(proveedor.Id);

            var resultado = await service.DisableAsync(proveedor.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("ya está inactivo");
        }

        // EnableProveedor no consulta ningún business validator antes de Activar(): a
        // diferencia de DisableAsync, el caso "ya está activo" sale como excepción de
        // dominio y no como Failure. El test deja el comportamiento actual documentado.
        [Fact]
        public async Task EnableProveedor_SobreProveedorYaActivo_LanzaEstadoInvalidoException()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var accion = async () => await service.EnableProveedor(proveedor.Id);

            await accion.Should().ThrowAsync<EstadoInvalidoException>();
        }

        [Fact]
        public async Task EnableProveedor_SobreProveedorInactivo_LoReactiva()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var service = IntegrationTestFactory.CrearProveedorService(context);
            await service.DisableAsync(proveedor.Id);

            var resultado = await service.EnableProveedor(proveedor.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await service.GetAllAsync()).Data!.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_ConProductosAsociados_NoBorraYExplicaPorQue()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            producto.AsignarProveedor(proveedor.Id);
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.DeleteAsync(proveedor.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("1 producto");
            (await context.Proveedores.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_SinProductosAsociados_LoBorraFisicamente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.DeleteAsync(proveedor.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Proveedores.CountAsync()).Should().Be(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task BuscarAsync_SinCriterio_DevuelveFailure(string? nombre)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.BuscarAsync(nombre);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("al menos un criterio");
        }

        [Fact]
        public async Task BuscarAsync_PorDefecto_OmiteLosProveedoresInactivos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await IntegrationTestFactory.SembrarProveedorAsync(context, "Ferretería Norte", "norte@test.com");
            var sur = await IntegrationTestFactory.SembrarProveedorAsync(context, "Ferretería Sur", "sur@test.com");
            sur.Desactivar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var soloActivos = await service.BuscarAsync("Ferretería");
            var conInactivos = await service.BuscarAsync("Ferretería", incluirInactivos: true);

            soloActivos.Data!.Should().HaveCount(1);
            conInactivos.Data!.Should().HaveCount(2);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetAllAsync_ConPaginacionFueraDeRango_DevuelveFailure(int pageNumber, int pageSize)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.GetAllAsync(pageNumber, pageSize);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetByIdAsync_ConIdInvalido_DevuelveFailure(int id)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearProveedorService(context);

            var resultado = await service.GetByIdAsync(id);

            resultado.IsSuccess.Should().BeFalse();
        }
    }
}
