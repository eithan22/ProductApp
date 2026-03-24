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
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<Cliente>> BuscarAsync(string? nombre, string? telefono, string? correo)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(c => c.Nombre.Contains(nombre));
            }
            if (!string.IsNullOrWhiteSpace(telefono))
            {
                query = query.Where(c => c.Telefono.Contains(telefono));
            }
            if (!string.IsNullOrWhiteSpace(correo))
            {
                query = query.Where(c => c.Correo.Contains(correo));
            }
            return query.ToListAsync();

        }
    }
}
