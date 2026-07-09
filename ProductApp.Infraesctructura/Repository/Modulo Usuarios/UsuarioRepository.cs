using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => !u.EstaEliminado && u.Email == email);
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => !u.EstaEliminado && u.Username == username);
        }

        public async Task<(List<Usuario> Items, int TotalCount)> GetAllUsuariosAsync(bool incluirInactivos, int pageNumber, int pageSize)
        {
            var query = _context.Usuarios.Where(u => !u.EstaEliminado).AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(u => u.EstadoUsuario == EstadoUsuario.Activo);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
