using ProductApp.Domian.Entitis;

namespace ProductApp.Domian.Interfaces
{
    // Construye el binario de la factura. La librería de renderizado (QuestPDF) queda
    // encapsulada en Infraestructura: ni Application ni la Api la conocen.
    public interface IGeneradorFacturaPdf
    {
        // configuracion puede ser null, o tener LogoUrl/RucONit/Direccion vacíos: en ese caso
        // esos datos se omiten del PDF en vez de fallar (RF-3.2, SDD 2.2).
        Task<byte[]> GenerarAsync(
            Orden orden,
            IReadOnlyList<OrdenDetalle> detalles,
            ConfiguracionSistema? configuracion,
            decimal totalPagado,
            CancellationToken cancellationToken = default);
    }
}
