using ProductApp.Aplication.Dtos.ReporteDto;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Reportes
{
    public interface IMapperReporte
    {
        VentaPorFechaDto MapToVentaPorFechaDto((DateTime Fecha, int CantidadOrdenes, decimal Total) data);
        VentaPorProductoDto MapToVentaPorProductoDto((int ProductoId, string NombreProducto, int CantidadVendida, decimal Total) data);
        VentaPorVendedorDto MapToVentaPorVendedorDto((int UsuarioId, string NombreVendedor, int CantidadOrdenes, decimal Total) data);
        InventarioActualDto MapToInventarioActualDto(Inventario inventario);
        ProductoMasVendidoDto MapToProductoMasVendidoDto((int ProductoId, string NombreProducto, int CantidadVendida) data);
        IngresosTotalesDto MapToIngresosTotalesDto(DateTime desde, DateTime hasta, decimal total, int cantidadPagos, decimal ticketPromedio);
    }
}
