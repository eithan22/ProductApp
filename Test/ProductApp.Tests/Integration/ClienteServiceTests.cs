using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class ClienteServiceTests
    {
        // Los formatos vienen de CreateClienteValidator: teléfono de 10 dígitos exactos,
        // cédula de 11, nombre de máximo 20 caracteres y correo de máximo 30.
        private static CreateClienteDto CrearDto(
            string nombre = "Ana Pérez",
            string cedula = "40212345678",
            string correo = "ana@test.com",
            string telefono = "8095551234")
            => new()
            {
                Nombre = nombre,
                Cedula = cedula,
                Correo = correo,
                Telefono = telefono,
                Direccion = "Calle Duarte 10"
            };

        [Fact]
        public async Task CreateAsync_ConDatosValidos_GuardaElClienteActivo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Estado.Should().Be(nameof(EstadoCliente.Activo));
            resultado.Data.EsReservado.Should().BeFalse();
            (await context.Clientes.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConCedulaYaRegistrada_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearClienteServices(context);
            (await service.CreateAsync(CrearDto())).IsSuccess.Should().BeTrue();

            // Mismo documento, todo lo demás distinto: la única regla que puede saltar
            // es la de la cédula.
            var resultado = await service.CreateAsync(CrearDto(
                nombre: "Luis Gómez",
                cedula: "40212345678",
                correo: "luis@test.com",
                telefono: "8095559999"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("cédula ya está registrada");
            (await context.Clientes.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConCorreoYaRegistrado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearClienteServices(context);
            (await service.CreateAsync(CrearDto())).IsSuccess.Should().BeTrue();

            var resultado = await service.CreateAsync(CrearDto(
                nombre: "Luis Gómez",
                cedula: "40299999999",
                correo: "ana@test.com",
                telefono: "8095559999"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("correo ya está registrado");
            (await context.Clientes.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConTelefonoYaRegistrado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearClienteServices(context);
            (await service.CreateAsync(CrearDto())).IsSuccess.Should().BeTrue();

            var resultado = await service.CreateAsync(CrearDto(
                nombre: "Luis Gómez",
                cedula: "40299999999",
                correo: "luis@test.com",
                telefono: "8095551234"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("teléfono ya está registrado");
        }

        [Fact]
        public async Task ObtenerClienteReservadoAsync_ConElConsumidorFinalSembrado_LoDevuelve()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await IntegrationTestFactory.SembrarClienteAsync(context);
            var reservado = await IntegrationTestFactory.SembrarConsumidorFinalAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.ObtenerClienteReservadoAsync();

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Id.Should().Be(reservado.Id);
            resultado.Data.Cedula.Should().Be(Cliente.CedulaConsumidorFinal);
            resultado.Data.EsReservado.Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerClienteReservadoAsync_SinElConsumidorFinal_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await IntegrationTestFactory.SembrarClienteAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.ObtenerClienteReservadoAsync();

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("todavía no existe");
        }

        [Fact]
        public async Task UpdateAsync_SobreElConsumidorFinal_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var reservado = await IntegrationTestFactory.SembrarConsumidorFinalAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);

            // La cédula del DTO va de 10 dígitos porque es lo que exige
            // UpdateClienteValidator; con los 11 del reservado el DTO ni siquiera
            // llegaría a la regla de negocio que se quiere probar aquí.
            var resultado = await service.UpdateAsync(new UpdateClienteDto
            {
                Id = reservado.Id,
                Nombre = "Nombre Cambiado",
                Cedula = "4021234567",
                Correo = "otro@test.com",
                Telefono = "8095551234",
                Direccion = "Otra dirección"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("cliente reservado del sistema");
            (await context.Clientes.FindAsync(reservado.Id))!.Nombre.Should().Be(Cliente.NombreConsumidorFinal);
        }

        [Fact]
        public async Task DisableAsync_SobreElConsumidorFinal_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var reservado = await IntegrationTestFactory.SembrarConsumidorFinalAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.DisableAsync(reservado.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("cliente reservado del sistema");
            (await context.Clientes.FindAsync(reservado.Id))!.Estado.Should().Be(EstadoCliente.Activo);
        }

        [Fact]
        public async Task DeleteAsync_SobreElConsumidorFinal_DevuelveFailureYNoLoBorra()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var reservado = await IntegrationTestFactory.SembrarConsumidorFinalAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.DeleteAsync(reservado.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("cliente reservado del sistema");
            (await context.Clientes.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DisableAsync_SobreClienteYaInactivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);
            (await service.DisableAsync(cliente.Id)).IsSuccess.Should().BeTrue();

            var resultado = await service.DisableAsync(cliente.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("ya está inactivo");
        }

        [Fact]
        public async Task UpdateAsync_SobreClienteInactivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);
            (await service.DisableAsync(cliente.Id)).IsSuccess.Should().BeTrue();

            var resultado = await service.UpdateAsync(new UpdateClienteDto
            {
                Id = cliente.Id,
                Nombre = "Cliente Editado",
                Cedula = "4021234567",
                Correo = "editado@test.com",
                Telefono = "8095551234",
                Direccion = "Calle Nueva 5"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("cliente inactivo");
        }

        // BUG detectado: CreateClienteValidator exige cédula de 11 dígitos y
        // UpdateClienteValidator de 10. Un cliente creado por la vía normal nunca puede
        // editarse conservando su propia cédula. El test fija el comportamiento actual
        // para que se note el día que se corrija el validador.
        [Fact]
        public async Task UpdateAsync_ConservandoLaCedulaDeOnceDigitos_FallaEnElValidadorDeUpdate()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearClienteServices(context);
            var creado = await service.CreateAsync(CrearDto());
            creado.IsSuccess.Should().BeTrue(creado.Message);

            var resultado = await service.UpdateAsync(new UpdateClienteDto
            {
                Id = creado.Data!.Id,
                Nombre = creado.Data.Nombre,
                Cedula = creado.Data.Cedula,
                Correo = creado.Data.Email,
                Telefono = creado.Data.Telefono,
                Direccion = "Calle Duarte 12"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("cédula debe tener 10 dígitos");
        }

        [Fact]
        public async Task ObtenerTotalComprasAsync_SumaSoloLasOrdenesNoCanceladas()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 100, precio: 10);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            // Orden A: queda Pagada. Total = 3 * 10 = 30.
            var ordenA = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            ordenA.IsSuccess.Should().BeTrue(ordenA.Message);
            (await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = ordenA.Data!.Id,
                ProductId = producto.Id,
                Cantidad = 3
            })).IsSuccess.Should().BeTrue();
            (await pagoService.RegistrarPagoAsync(new CreatePagoDto
            {
                OrdenId = ordenA.Data.Id,
                Monto = 30,
                MetodoPago = "Efectivo"
            }, usuario.Id)).IsSuccess.Should().BeTrue();

            // Orden B: queda Cancelada. Total = 5 * 10 = 50 y no debe sumar.
            var ordenB = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            ordenB.IsSuccess.Should().BeTrue(ordenB.Message);
            (await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = ordenB.Data!.Id,
                ProductId = producto.Id,
                Cantidad = 5
            })).IsSuccess.Should().BeTrue();
            (await ordenServices.CancelarOrden(ordenB.Data.Id, usuario.Id, esAdministrador: false)).IsSuccess.Should().BeTrue();

            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.ObtenerTotalComprasAsync(cliente.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.CantidadOrdenes.Should().Be(1);
            resultado.Data.TotalComprado.Should().Be(30);
            resultado.Data.FechaUltimaCompra.Should().NotBeNull();
            resultado.Data.NombreCliente.Should().Be(cliente.Nombre);
        }

        [Fact]
        public async Task ObtenerTotalComprasAsync_NoMezclaLasOrdenesDeOtroCliente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 100, precio: 10);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var otroCliente = await IntegrationTestFactory.SembrarClienteAsync(
                context, nombre: "Otro Cliente", cedula: "001-0000000-2", correo: "otro@test.com", telefono: "809-000-0001");
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var ordenAjena = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = otroCliente.Id }, usuario.Id);
            ordenAjena.IsSuccess.Should().BeTrue(ordenAjena.Message);
            (await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = ordenAjena.Data!.Id,
                ProductId = producto.Id,
                Cantidad = 7
            })).IsSuccess.Should().BeTrue();

            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.ObtenerTotalComprasAsync(cliente.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.CantidadOrdenes.Should().Be(0);
            resultado.Data.TotalComprado.Should().Be(0);
            resultado.Data.FechaUltimaCompra.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerTotalComprasAsync_ConClienteInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.ObtenerTotalComprasAsync(999);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no fue encontrado");
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetAllAsync_ConPaginacionFueraDeRango_DevuelveFailure(int pageNumber, int pageSize)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearClienteServices(context);

            var resultado = await service.GetAllAsync(pageNumber, pageSize);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_PorDefecto_OmiteALosClientesInactivos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            await IntegrationTestFactory.SembrarConsumidorFinalAsync(context);
            var service = IntegrationTestFactory.CrearClienteServices(context);
            (await service.DisableAsync(cliente.Id)).IsSuccess.Should().BeTrue();

            var soloActivos = await service.GetAllAsync();
            var conInactivos = await service.GetAllAsync(incluirInactivos: true);

            soloActivos.Data!.TotalCount.Should().Be(1);
            conInactivos.Data!.TotalCount.Should().Be(2);
            // El reservado va siempre primero para que no se pierda al paginar.
            soloActivos.Data.Items[0].EsReservado.Should().BeTrue();
        }
    }
}
