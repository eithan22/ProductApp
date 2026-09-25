using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class FacturaPdfService : IFacturaPdfService
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IPagoRepository _pagoRepository;
        private readonly IConfiguracionSistemaRepository _configuracionSistemaRepository;
        private readonly IGeneradorFacturaPdf _generadorFacturaPdf;
        private readonly IAlmacenamientoFacturas _almacenamientoFacturas;
        private readonly ILogger<FacturaPdfService> _logger;

        public FacturaPdfService(
            IOrdenRepository ordenRepository,
            IDetalleOrdenRepository detalleOrdenRepository,
            IPagoRepository pagoRepository,
            IConfiguracionSistemaRepository configuracionSistemaRepository,
            IGeneradorFacturaPdf generadorFacturaPdf,
            IAlmacenamientoFacturas almacenamientoFacturas,
            ILogger<FacturaPdfService> logger)
        {
            _ordenRepository = ordenRepository;
            _detalleOrdenRepository = detalleOrdenRepository;
            _pagoRepository = pagoRepository;
            _configuracionSistemaRepository = configuracionSistemaRepository;
            _generadorFacturaPdf = generadorFacturaPdf;
            _almacenamientoFacturas = almacenamientoFacturas;
            _logger = logger;
        }

        // Convención del SDD: facturas/orden-{ordenId}.pdf ("facturas" es el contenedor).
        private static string NombreBlob(int ordenId) => $"orden-{ordenId}.pdf";

        public async Task<OperationResultD<string>> GenerarYAlmacenarAsync(int ordenId)
        {
            var orden = await _ordenRepository.GetByIdConClienteAsync(ordenId);
            if (orden == null)
                return OperationResultD<string>.Failure("Orden no encontrada");

            if (orden.Estado != EstadoOrden.Pagada)
                return OperationResultD<string>.Failure("Solo se emite factura de órdenes en estado Pagada");

            var detalles = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(ordenId);
            if (detalles.Count == 0)
                return OperationResultD<string>.Failure("La orden no tiene productos para facturar");

            var totalPagado = await _pagoRepository.ObtenerTotalPagadoPorOrdenAsync(ordenId);

            // La configuración de empresa es opcional aquí a propósito: si no existe, o si le
            // faltan logo/RUC/dirección, el PDF omite esos datos en vez de fallar. Nunca se
            // bloquea un cobro por configuración de empresa incompleta (SDD 2.2).
            var configuracion = await _configuracionSistemaRepository.ObtenerAsync();

            try
            {
                var pdf = await _generadorFacturaPdf.GenerarAsync(orden, detalles, configuracion, totalPagado);
                var url = await _almacenamientoFacturas.SubirAsync(pdf, NombreBlob(ordenId));

                _logger.LogInformation(
                    "Factura PDF emitida para la orden {OrdenId} y archivada en {UrlFactura}", ordenId, url);

                return OperationResultD<string>.Success(url, "Factura generada exitosamente");
            }
            catch (Exception ex)
            {
                // Se atrapa acá y se devuelve Failure (en vez de propagar) porque quien llama es
                // el registro de pago: el dinero ya entró y no puede revertirse por un fallo de
                // storage o de renderizado.
                _logger.LogError(ex,
                    "No se pudo generar o archivar la factura PDF de la orden {OrdenId}", ordenId);

                return OperationResultD<string>.Failure("No se pudo generar la factura PDF de la orden");
            }
        }

        public async Task<OperationResultD<byte[]>> ObtenerAsync(int ordenId)
        {
            var orden = await _ordenRepository.GetByIdAsync(ordenId);
            if (orden == null)
                return OperationResultD<byte[]>.Failure("Orden no encontrada");

            // Entregada también califica: es un estado posterior a Pagada (ver transiciones en
            // Orden), y esa orden sí tiene factura emitida.
            if (orden.Estado != EstadoOrden.Pagada && orden.Estado != EstadoOrden.Entregada)
                return OperationResultD<byte[]>.Failure("La factura solo está disponible para órdenes pagadas");

            try
            {
                var contenido = await _almacenamientoFacturas.DescargarAsync(NombreBlob(ordenId));
                if (contenido == null || contenido.Length == 0)
                    return OperationResultD<byte[]>.Failure("Esta orden no tiene una factura generada");

                return OperationResultD<byte[]>.Success(contenido, "Factura obtenida exitosamente");
            }
            catch (Exception ex)
            {
                // Mismo criterio que GenerarYAlmacenarAsync: un fallo de storage no debe salir
                // como 500 genérico, sino como un mensaje de negocio claro.
                _logger.LogError(ex, "No se pudo descargar la factura PDF de la orden {OrdenId}", ordenId);
                return OperationResultD<byte[]>.Failure("No se pudo obtener la factura de esta orden. Intente nuevamente en unos minutos.");
            }
        }
    }
}
