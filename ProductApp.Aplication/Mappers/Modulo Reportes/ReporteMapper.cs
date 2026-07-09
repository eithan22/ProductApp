using ProductApp.Aplication.Dtos.ReporteDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Reportes;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Mappers.Modulo_Reportes
{
    public class ReporteMapper : IMapperReporte
    {
        public VentaPorFechaDto MapToVentaPorFechaDto((DateTime Fecha, int CantidadOrdenes, decimal Total) data)
        {
            return new VentaPorFechaDto
            {
                Fecha = data.Fecha,
                CantidadOrdenes = data.CantidadOrdenes,
                Total = data.Total
            };
        }

        public VentaPorProductoDto MapToVentaPorProductoDto((int ProductoId, string NombreProducto, int CantidadVendida, decimal Total) data)
        {
            return new VentaPorProductoDto
            {
                ProductoId = data.ProductoId,
                NombreProducto = data.NombreProducto,
                CantidadVendida = data.CantidadVendida,
                Total = data.Total
            };
        }

        public VentaPorVendedorDto MapToVentaPorVendedorDto((int UsuarioId, string NombreVendedor, int CantidadOrdenes, decimal Total) data)
        {
            return new VentaPorVendedorDto
            {
                UsuarioId = data.UsuarioId,
                NombreVendedor = data.NombreVendedor,
                CantidadOrdenes = data.CantidadOrdenes,
                Total = data.Total
            };
        }

        public InventarioActualDto MapToInventarioActualDto(Inventario inventario)
        {
            return new InventarioActualDto
            {
                ProductoId = inventario.ProductoId,
                NombreProducto = inventario.Producto.Nombre,
                CantidadActual = inventario.CantidadActual,
                CantidadMinima = inventario.CantidadMinima,
                StockBajo = inventario.EsStockBajo()
            };
        }

        public ProductoMasVendidoDto MapToProductoMasVendidoDto((int ProductoId, string NombreProducto, int CantidadVendida) data)
        {
            return new ProductoMasVendidoDto
            {
                ProductoId = data.ProductoId,
                NombreProducto = data.NombreProducto,
                CantidadVendida = data.CantidadVendida
            };
        }

        public IngresosTotalesDto MapToIngresosTotalesDto(DateTime desde, DateTime hasta, decimal total, int cantidadPagos, decimal ticketPromedio)
        {
            return new IngresosTotalesDto
            {
                Desde = desde,
                Hasta = hasta,
                Total = total,
                CantidadPagos = cantidadPagos,
                TicketPromedio = ticketPromedio
            };
        }
    }
}
