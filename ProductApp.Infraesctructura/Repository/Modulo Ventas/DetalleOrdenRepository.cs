using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class DetalleOrdenRepository : GenericRepository<OrderDetalle>, IDetalleOrdenRepository
    {
        public DetalleOrdenRepository(AppDbContext context) : base(context)
        {
        }

        // Implementación del método para obtener el detalle de la orden con la información del producto relacionado

        public async Task<OrderDetalle?> ObtenerDetalleConProductoAsync(int id)
        {
            return await _context.DetalleOrden
                .Include(od => od.Producto) // Incluir la información del producto relacionado
                .FirstOrDefaultAsync(od => od.Id == id);


        }

        // Implementación del método para obtener todos los detalles de una orden específica con la información del producto relacionado

        public async Task<List<OrderDetalle>> ObtenerDetalleOrdenPorOrdenIdAsync(int Id)
        {
            return await _context.DetalleOrden
                .Include(od => od.Producto) // Incluir la información del producto relacionado
                .Where(od => od.OrdenId == Id)
                .ToListAsync();
        }


        // Implementación del método para obtener un detalle de orden específico por productoId y ordenId con la información del producto relacionado

        public Task<OrderDetalle?> ObtenerProductoEnOrdenAsync(int ordenId, int productoId)
        {
            return _context.DetalleOrden
                .Include(od => od.Producto) // Incluir la información del producto relacionado
                .FirstOrDefaultAsync(od => od.ProductId == productoId && od.OrdenId == ordenId);
        }
    }
}
