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
    public class OrdenRepository : GenericRepository<Orden> , IOrdenRepository
    {
        public OrdenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Orden>> ObtenerPorClienteAsync(int clienteId)
        {
            return await _context.Ordenes
           .Include(o => o.Cliente)
           .Where(o => o.ClienteId == clienteId)
           .ToListAsync();
        }

        public async Task<List<Orden>> ObtenerPorFechaAsync(DateTime fecha)
        {
            return await _context.Ordenes
                .Where(o => o.Fecha.Date == fecha.Date)
                .ToListAsync();
        }
    }
}
