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

            // Sin OrderBy, SQL Server no garantiza el orden entre páginas: un usuario
            // podría repetirse o desaparecer al paginar.
            var items = await query
                .OrderBy(u => u.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<int>> ObtenerIdsAdministradoresActivosAsync()
        {
            return await _context.Usuarios
                .Where(u => !u.EstaEliminado && u.EstadoUsuario == EstadoUsuario.Activo && u.RolUsuario == RolUsuario.Administrador)
                .Select(u => u.Id)
                .ToListAsync();
        }

        public async Task<(bool Existe, RolUsuario Rol, EstadoUsuario Estado)> ObtenerEstadoSesionAsync(int id)
        {
            var datos = await _context.Usuarios
                .AsNoTracking()
                .Where(u => !u.EstaEliminado && u.Id == id)
                .Select(u => new { u.RolUsuario, u.EstadoUsuario })
                .FirstOrDefaultAsync();

            // Los valores de relleno son los más restrictivos a propósito (Vendedor + Inactivo):
            // si alguien ignora Existe, el peor caso es negar acceso, nunca concederlo.
            if (datos == null)
                return (false, RolUsuario.Vendedor, EstadoUsuario.Inactivo);

            return (true, datos.RolUsuario, datos.EstadoUsuario);
        }

        public async Task<string?> ObtenerVersionDocumentosLegalesAceptadaAsync(int id)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => !u.EstaEliminado && u.Id == id)
                .Select(u => u.VersionDocumentosLegalesAceptada)
                .FirstOrDefaultAsync();
        }
    }
}
