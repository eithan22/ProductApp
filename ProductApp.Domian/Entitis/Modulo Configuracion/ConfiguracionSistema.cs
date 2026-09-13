using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class ConfiguracionSistema : BaseEntity
    {
        public int CantidadMinimaInventarioDefecto { get; private set; }
        public int DuracionTokenMinutos { get; private set; }
        public string NombreEmpresa { get; private set; } = string.Empty;
        public string Moneda { get; private set; } = string.Empty;
        public string? RucONit { get; private set; }
        public string? Direccion { get; private set; }
        public string? LogoUrl { get; private set; }

        public const int LargoMaximoRucONit = 30;
        public const int LargoMaximoDireccion = 200;
        public const int LargoMaximoLogoUrl = 500;

        protected ConfiguracionSistema() { }

        public ConfiguracionSistema(int cantidadMinimaInventarioDefecto, int duracionTokenMinutos, string nombreEmpresa, string moneda)
        {
            ValidarCantidadMinimaInventarioDefecto(cantidadMinimaInventarioDefecto);
            ValidarDuracionTokenMinutos(duracionTokenMinutos);
            ValidarNombreEmpresa(nombreEmpresa);
            ValidarMoneda(moneda);

            CantidadMinimaInventarioDefecto = cantidadMinimaInventarioDefecto;
            DuracionTokenMinutos = duracionTokenMinutos;
            NombreEmpresa = nombreEmpresa;
            Moneda = moneda;
        }

        private static void ValidarCantidadMinimaInventarioDefecto(int valor)
        {
            if (valor < 0)
                throw new ValidacionDominioException("CantidadMinimaInventarioDefecto", "La cantidad mínima de inventario por defecto no puede ser negativa.");
        }

        private static void ValidarDuracionTokenMinutos(int valor)
        {
            if (valor <= 0)
                throw new ValidacionDominioException("DuracionTokenMinutos", "La duración del token debe ser mayor a cero.");
        }

        private static void ValidarNombreEmpresa(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ValidacionDominioException("NombreEmpresa", "El nombre de la empresa no puede estar vacío.");
        }

        private static void ValidarMoneda(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ValidacionDominioException("Moneda", "La moneda no puede estar vacía.");
        }

        private static string? NormalizarRucONit(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var normalizado = valor.Trim();

            if (normalizado.Length > LargoMaximoRucONit)
                throw new ValidacionDominioException("RucONit", $"El RUC/NIT no puede exceder los {LargoMaximoRucONit} caracteres.");

            return normalizado;
        }

        private static string? NormalizarDireccion(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var normalizado = valor.Trim();

            if (normalizado.Length > LargoMaximoDireccion)
                throw new ValidacionDominioException("Direccion", $"La dirección no puede exceder los {LargoMaximoDireccion} caracteres.");

            return normalizado;
        }

        public void ActualizarParametros(
            int cantidadMinimaInventarioDefecto,
            int duracionTokenMinutos,
            string nombreEmpresa,
            string moneda,
            string? rucONit,
            string? direccion)
        {
            ValidarCantidadMinimaInventarioDefecto(cantidadMinimaInventarioDefecto);
            ValidarDuracionTokenMinutos(duracionTokenMinutos);
            ValidarNombreEmpresa(nombreEmpresa);
            ValidarMoneda(moneda);

            var rucNormalizado = NormalizarRucONit(rucONit);
            var direccionNormalizada = NormalizarDireccion(direccion);

            CantidadMinimaInventarioDefecto = cantidadMinimaInventarioDefecto;
            DuracionTokenMinutos = duracionTokenMinutos;
            NombreEmpresa = nombreEmpresa;
            Moneda = moneda;
            RucONit = rucNormalizado;
            Direccion = direccionNormalizada;

            ActualizarFechaModificacion();
        }

        public void AsignarLogo(string logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                throw new ValidacionDominioException("LogoUrl", "La referencia del logo no puede estar vacía.");

            if (logoUrl.Length > LargoMaximoLogoUrl)
                throw new ValidacionDominioException("LogoUrl", $"La referencia del logo no puede exceder los {LargoMaximoLogoUrl} caracteres.");

            LogoUrl = logoUrl;
            ActualizarFechaModificacion();
        }

        public void QuitarLogo()
        {
            LogoUrl = null;
            ActualizarFechaModificacion();
        }
    }
}
