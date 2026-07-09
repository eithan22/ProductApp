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

        public void ActualizarParametros(int cantidadMinimaInventarioDefecto, int duracionTokenMinutos, string nombreEmpresa, string moneda)
        {
            ValidarCantidadMinimaInventarioDefecto(cantidadMinimaInventarioDefecto);
            ValidarDuracionTokenMinutos(duracionTokenMinutos);
            ValidarNombreEmpresa(nombreEmpresa);
            ValidarMoneda(moneda);

            CantidadMinimaInventarioDefecto = cantidadMinimaInventarioDefecto;
            DuracionTokenMinutos = duracionTokenMinutos;
            NombreEmpresa = nombreEmpresa;
            Moneda = moneda;

            ActualizarFechaModificacion();
        }
    }
}
