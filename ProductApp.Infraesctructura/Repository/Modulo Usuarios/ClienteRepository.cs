using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<Cliente>> BuscarClientesAsync(string? nombre, string? telefono, string? correo)
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

        public async Task<bool> ExistePorCorreoAsync(string correo)
        {
            return await _context.Clientes
                .AnyAsync(c => !c.EstaEliminado && c.Correo == correo);
        }

        public async Task<bool> ExistePorCedulaAsync(string cedula)
        {
            return await _context.Clientes
                .AnyAsync(c => !c.EstaEliminado && c.Cedula == cedula);
        }
    }
}
