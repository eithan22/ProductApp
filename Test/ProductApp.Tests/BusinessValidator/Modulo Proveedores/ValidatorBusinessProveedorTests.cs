using FluentAssertions;
using Moq;
using ProductApp.Aplication.BusinessValidator.Modulo_Proveedores;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Proveedores
{
    public class ValidatorBusinessProveedorTests
    {
        private static Proveedor CrearProveedor()
            => new Proveedor("Proveedor Test", "8090000000", "proveedor@test.com", "Av. Principal 1");

        private static CreateProveedorDto CrearDto()
            => new() { Nombre = "Proveedor Test", Telefono = "8090000000", Correo = "proveedor@test.com", Direccion = "Av. Principal 1" };

        private static UpdateProveedorDto CrearUpdateDto(string nombre = "Proveedor Test", string correo = "proveedor@test.com")
            => new() { Id = 1, Nombre = nombre, Telefono = "8090000000", Correo = correo, Direccion = "Av. Principal 1" };

        // El validator consulta primero por nombre y después por correo. SetupSequence
        // se apoya en ese orden para poder distinguir las dos llamadas a ExisteAsync.
        private static Mock<IProveedorRepository> RepoConExistencias(params bool[] respuestas)
        {
            var mock = new Mock<IProveedorRepository>();
            var secuencia = mock.SetupSequence(r => r.ExisteAsync(It.IsAny<Expression<Func<Proveedor, bool>>>()));

            foreach (var respuesta in respuestas)
                secuencia = secuencia.ReturnsAsync(respuesta);

            return mock;
        }

        [Fact]
        public async Task ValidarCreateProveedorAsync_ConNombreYaRegistrado_DevuelveFailure()
        {
            var validator = new ValidatorBusinessProveedor(RepoConExistencias(true).Object);

            var resultado = await validator.ValidarCreateProveedorAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("nombre ya está registrado");
        }

        [Fact]
        public async Task ValidarCreateProveedorAsync_ConCorreoYaRegistrado_DevuelveFailure()
        {
            var validator = new ValidatorBusinessProveedor(RepoConExistencias(false, true).Object);

            var resultado = await validator.ValidarCreateProveedorAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("correo ya está registrado");
        }

        // Solo nombre y correo son únicos: el teléfono puede repetirse a propósito.
        // Por eso solo hay dos consultas de existencia, no tres.
        [Fact]
        public async Task ValidarCreateProveedorAsync_SinNombreNiCorreoRepetidos_DevuelveSuccessYSoloConsultaDosVeces()
        {
            var repoMock = RepoConExistencias(false, false);
            var validator = new ValidatorBusinessProveedor(repoMock.Object);

            var resultado = await validator.ValidarCreateProveedorAsync(CrearDto());

            resultado.IsSuccess.Should().BeTrue();
            repoMock.Verify(r => r.ExisteAsync(It.IsAny<Expression<Func<Proveedor, bool>>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ValidarUpdateProveedorAsync_SobreProveedorInactivo_DevuelveFailure()
        {
            var proveedor = CrearProveedor();
            proveedor.Desactivar();
            var validator = new ValidatorBusinessProveedor(new Mock<IProveedorRepository>().Object);

            var resultado = await validator.ValidarUpdateProveedorAsync(CrearUpdateDto(), proveedor);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("inactivo");
        }

        // Guardar sin cambiar nombre ni correo no debe chocar contra el propio registro:
        // el validator ni siquiera consulta duplicados en ese caso.
        [Fact]
        public async Task ValidarUpdateProveedorAsync_SinCambiarNombreNiCorreo_NoConsultaDuplicados()
        {
            var repoMock = new Mock<IProveedorRepository>();
            repoMock.Setup(r => r.ExisteAsync(It.IsAny<Expression<Func<Proveedor, bool>>>())).ReturnsAsync(true);
            var validator = new ValidatorBusinessProveedor(repoMock.Object);

            var resultado = await validator.ValidarUpdateProveedorAsync(CrearUpdateDto(), CrearProveedor());

            resultado.IsSuccess.Should().BeTrue();
            repoMock.Verify(r => r.ExisteAsync(It.IsAny<Expression<Func<Proveedor, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ValidarUpdateProveedorAsync_CambiandoANombreDeOtroProveedor_DevuelveFailure()
        {
            var validator = new ValidatorBusinessProveedor(RepoConExistencias(true).Object);

            var resultado = await validator.ValidarUpdateProveedorAsync(CrearUpdateDto(nombre: "Otro Proveedor"), CrearProveedor());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("nombre ya está registrado");
        }

        [Fact]
        public async Task ValidarUpdateProveedorAsync_CambiandoACorreoDeOtroProveedor_DevuelveFailure()
        {
            var validator = new ValidatorBusinessProveedor(RepoConExistencias(true).Object);

            var resultado = await validator.ValidarUpdateProveedorAsync(CrearUpdateDto(correo: "otro@test.com"), CrearProveedor());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("correo ya está registrado");
        }

        [Fact]
        public async Task ValidarDeleteProveedorAsync_SobreProveedorYaInactivo_DevuelveFailure()
        {
            var proveedor = CrearProveedor();
            proveedor.Desactivar();
            var validator = new ValidatorBusinessProveedor(new Mock<IProveedorRepository>().Object);

            var resultado = await validator.ValidarDeleteProveedorAsync(proveedor);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("ya está inactivo");
        }

        [Fact]
        public async Task ValidarDeleteProveedorAsync_SobreProveedorActivo_DevuelveSuccess()
        {
            var validator = new ValidatorBusinessProveedor(new Mock<IProveedorRepository>().Object);

            var resultado = await validator.ValidarDeleteProveedorAsync(CrearProveedor());

            resultado.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ValidarBorradoFisicoProveedorAsync_ConProductosAsociados_DevuelveFailureConLaCantidad()
        {
            var repoMock = new Mock<IProveedorRepository>();
            repoMock.Setup(r => r.ContarProductosAsociadosAsync(It.IsAny<int>())).ReturnsAsync(3);
            var validator = new ValidatorBusinessProveedor(repoMock.Object);

            var resultado = await validator.ValidarBorradoFisicoProveedorAsync(CrearProveedor());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("3");
            resultado.Message.Should().Contain("Desactívalo");
        }

        [Fact]
        public async Task ValidarBorradoFisicoProveedorAsync_SinProductosAsociados_DevuelveSuccess()
        {
            var repoMock = new Mock<IProveedorRepository>();
            repoMock.Setup(r => r.ContarProductosAsociadosAsync(It.IsAny<int>())).ReturnsAsync(0);
            var validator = new ValidatorBusinessProveedor(repoMock.Object);

            var resultado = await validator.ValidarBorradoFisicoProveedorAsync(CrearProveedor());

            resultado.IsSuccess.Should().BeTrue();
        }
    }
}
