using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly AppDbContext _context;

        public ReporteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<(DateTime Fecha, int CantidadOrdenes, decimal Total)>> ObtenerVentasPorFechaAsync(DateTime desde, DateTime hasta)
        {
            var query = await _context.Ordenes
                .Where(o => !o.EstaEliminado && o.Estado != EstadoOrden.Cancelada && o.Fecha >= desde && o.Fecha <= hasta)
                .GroupBy(o => o.Fecha.Date)
                .Select(g => new { Fecha = g.Key, CantidadOrdenes = g.Count(), Total = g.Sum(o => o.Total) })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            return query.Select(x => (x.Fecha, x.CantidadOrdenes, x.Total)).ToList();
        }

        public async Task<List<(int ProductoId, string NombreProducto, int CantidadVendida, decimal Total)>> ObtenerVentasPorProductoAsync(DateTime desde, DateTime hasta)
        {
            var query = await _context.DetalleOrden
                .Where(d => !d.EstaEliminado
                    && !d.Orden.EstaEliminado
                    && d.Orden.Estado != EstadoOrden.Cancelada
                    && d.Orden.Fecha >= desde && d.Orden.Fecha <= hasta)
                .GroupBy(d => new { d.ProductId, d.Producto.Nombre })
                .Select(g => new { g.Key.ProductId, g.Key.Nombre, CantidadVendida = g.Sum(d => d.Cantidad), Total = g.Sum(d => d.Subtotal) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return query.Select(x => (x.ProductId, x.Nombre, x.CantidadVendida, x.Total)).ToList();
        }

        public async Task<List<(int UsuarioId, string NombreVendedor, int CantidadOrdenes, decimal Total)>> ObtenerVentasPorVendedorAsync(DateTime desde, DateTime hasta, int? usuarioId)
        {
            var baseQuery = _context.Ordenes
                .Where(o => !o.EstaEliminado && o.Estado != EstadoOrden.Cancelada && o.Fecha >= desde && o.Fecha <= hasta);

            if (usuarioId.HasValue)
                baseQuery = baseQuery.Where(o => o.UsuarioId == usuarioId.Value);

            var query = await baseQuery
                .GroupBy(o => new { o.UsuarioId, o.Usuario.Nombre })
                .Select(g => new { g.Key.UsuarioId, g.Key.Nombre, CantidadOrdenes = g.Count(), Total = g.Sum(o => o.Total) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return query.Select(x => (x.UsuarioId, x.Nombre, x.CantidadOrdenes, x.Total)).ToList();
        }

        public async Task<List<(int ProductoId, string NombreProducto, int CantidadVendida)>> ObtenerProductosMasVendidosAsync(DateTime desde, DateTime hasta, int top)
        {
            var query = await _context.DetalleOrden
                .Where(d => !d.EstaEliminado
                    && !d.Orden.EstaEliminado
                    && d.Orden.Estado != EstadoOrden.Cancelada
                    && d.Orden.Fecha >= desde && d.Orden.Fecha <= hasta)
                .GroupBy(d => new { d.ProductId, d.Producto.Nombre })
                .Select(g => new { g.Key.ProductId, g.Key.Nombre, CantidadVendida = g.Sum(d => d.Cantidad) })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(top)
                .ToListAsync();

            return query.Select(x => (x.ProductId, x.Nombre, x.CantidadVendida)).ToList();
        }

        public async Task<(decimal Total, int CantidadPagos)> ObtenerIngresosTotalesAsync(DateTime desde, DateTime hasta)
        {
            var query = _context.Pagos
                .Where(p => !p.EstaEliminado && p.Estado != EstadoPago.Fallido && p.FechaPago >= desde && p.FechaPago <= hasta);

            var total = await query.SumAsync(p => (decimal?)p.Monto) ?? 0;
            var cantidadPagos = await query.CountAsync();

            return (total, cantidadPagos);
        }
    }
}
