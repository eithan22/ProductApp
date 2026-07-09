using System;
using System.Collections.Generic;

namespace ProductApp.Domian.Interfaces
{
    public interface IReporteRepository
    {
        Task<List<(DateTime Fecha, int CantidadOrdenes, decimal Total)>> ObtenerVentasPorFechaAsync(DateTime desde, DateTime hasta);

        Task<List<(int ProductoId, string NombreProducto, int CantidadVendida, decimal Total)>> ObtenerVentasPorProductoAsync(DateTime desde, DateTime hasta);

        Task<List<(int UsuarioId, string NombreVendedor, int CantidadOrdenes, decimal Total)>> ObtenerVentasPorVendedorAsync(DateTime desde, DateTime hasta, int? usuarioId);

        Task<List<(int ProductoId, string NombreProducto, int CantidadVendida)>> ObtenerProductosMasVendidosAsync(DateTime desde, DateTime hasta, int top);

        Task<(decimal Total, int CantidadPagos)> ObtenerIngresosTotalesAsync(DateTime desde, DateTime hasta);
    }
}
