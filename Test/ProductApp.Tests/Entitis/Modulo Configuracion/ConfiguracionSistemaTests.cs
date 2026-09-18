using FluentAssertions;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class ConfiguracionSistemaTests
    {
        private static ConfiguracionSistema Crear()
            => new ConfiguracionSistema(5, 60, "ProductApp SRL", "DOP");

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_ConDuracionTokenNoPositiva_LanzaValidacionDominioException(int minutos)
        {
            var accion = () => new ConfiguracionSistema(5, minutos, "Empresa", "DOP");

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void Constructor_ConCantidadMinimaNegativa_LanzaValidacionDominioException()
        {
            var accion = () => new ConfiguracionSistema(-1, 60, "Empresa", "DOP");

            accion.Should().Throw<ValidacionDominioException>();
        }

        // Cero sí es válido: significa "no avisar por stock bajo por defecto".
        [Fact]
        public void Constructor_ConCantidadMinimaCero_EsValido()
        {
            var configuracion = new ConfiguracionSistema(0, 60, "Empresa", "DOP");

            configuracion.CantidadMinimaInventarioDefecto.Should().Be(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConNombreEmpresaVacio_LanzaValidacionDominioException(string nombre)
        {
            var accion = () => new ConfiguracionSistema(5, 60, nombre, "DOP");

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConMonedaVacia_LanzaValidacionDominioException(string moneda)
        {
            var accion = () => new ConfiguracionSistema(5, 60, "Empresa", moneda);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void ActualizarParametros_ConRucVacio_LoGuardaComoNull()
        {
            var configuracion = Crear();

            configuracion.ActualizarParametros(5, 60, "Empresa", "DOP", "   ", null);

            configuracion.RucONit.Should().BeNull();
        }

        [Fact]
        public void ActualizarParametros_ConRucConEspacios_LoGuardaRecortado()
        {
            var configuracion = Crear();

            configuracion.ActualizarParametros(5, 60, "Empresa", "DOP", "  131-12345-6  ", "  Calle 1  ");

            configuracion.RucONit.Should().Be("131-12345-6");
            configuracion.Direccion.Should().Be("Calle 1");
        }

        [Fact]
        public void ActualizarParametros_ConRucDemasiadoLargo_LanzaValidacionDominioException()
        {
            var configuracion = Crear();
            var rucLargo = new string('9', ConfiguracionSistema.LargoMaximoRucONit + 1);

            var accion = () => configuracion.ActualizarParametros(5, 60, "Empresa", "DOP", rucLargo, null);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void ActualizarParametros_ConDireccionDemasiadoLarga_LanzaValidacionDominioException()
        {
            var configuracion = Crear();
            var direccionLarga = new string('a', ConfiguracionSistema.LargoMaximoDireccion + 1);

            var accion = () => configuracion.ActualizarParametros(5, 60, "Empresa", "DOP", null, direccionLarga);

            accion.Should().Throw<ValidacionDominioException>();
        }

        // Todas las validaciones corren antes de cualquier asignación: una actualización
        // inválida no puede dejar la configuración a medio aplicar.
        [Fact]
        public void ActualizarParametros_ConDatoInvalido_NoMutaNingunValorPrevio()
        {
            var configuracion = Crear();

            var accion = () => configuracion.ActualizarParametros(99, 0, "Empresa Nueva", "USD", "131", "Calle 9");

            accion.Should().Throw<ValidacionDominioException>();
            configuracion.CantidadMinimaInventarioDefecto.Should().Be(5);
            configuracion.DuracionTokenMinutos.Should().Be(60);
            configuracion.NombreEmpresa.Should().Be("ProductApp SRL");
            configuracion.Moneda.Should().Be("DOP");
        }

        [Fact]
        public void ActualizarParametros_ConRucDemasiadoLargo_TampocoAplicaLosCamposValidos()
        {
            var configuracion = Crear();
            var rucLargo = new string('9', ConfiguracionSistema.LargoMaximoRucONit + 1);

            var accion = () => configuracion.ActualizarParametros(20, 120, "Empresa Nueva", "USD", rucLargo, null);

            accion.Should().Throw<ValidacionDominioException>();
            configuracion.NombreEmpresa.Should().Be("ProductApp SRL");
            configuracion.DuracionTokenMinutos.Should().Be(60);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AsignarLogo_ConReferenciaVacia_LanzaValidacionDominioException(string logoUrl)
        {
            var configuracion = Crear();

            var accion = () => configuracion.AsignarLogo(logoUrl);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void AsignarLogo_ConReferenciaDemasiadoLarga_LanzaValidacionDominioException()
        {
            var configuracion = Crear();
            var urlLarga = new string('u', ConfiguracionSistema.LargoMaximoLogoUrl + 1);

            var accion = () => configuracion.AsignarLogo(urlLarga);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void QuitarLogo_DejaLaReferenciaEnNull()
        {
            var configuracion = Crear();
            configuracion.AsignarLogo("https://blob/logos/empresa.png");

            configuracion.QuitarLogo();

            configuracion.LogoUrl.Should().BeNull();
        }

        // ActualizarParametros no toca el logo: se administra por su propio par de métodos.
        [Fact]
        public void ActualizarParametros_NoBorraElLogoYaAsignado()
        {
            var configuracion = Crear();
            configuracion.AsignarLogo("https://blob/logos/empresa.png");

            configuracion.ActualizarParametros(10, 90, "Otra Empresa", "USD", null, null);

            configuracion.LogoUrl.Should().Be("https://blob/logos/empresa.png");
        }
    }
}
