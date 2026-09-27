using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Enums.EnumsProveedor;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<Proveedor> Items, int TotalCount)> GetAllProveedoresAsync(bool incluirInactivos, int pageNumber, int pageSize)
        {
            var query = _context.Proveedores.Where(p => !p.EstaEliminado).AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Estado == EstadoProveedor.Activo);
            }

            var totalCount = await query.CountAsync();

            // Sin OrderBy, SQL Server no garantiza el orden entre páginas: un proveedor
            // podría repetirse o desaparecer al paginar.
            var items = await query
                .OrderBy(p => p.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public Task<List<Proveedor>> BuscarProveedoresAsync(string? nombre, bool incluirInactivos = false)
        {
            var query = _context.Proveedores.Where(p => !p.EstaEliminado).AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Estado == EstadoProveedor.Activo);
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.Contains(nombre));
            }

            return query
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        // Necesario para el borrado físico: con DeleteBehavior.Restrict la base rechazaría
        // la operación con un error de FK crudo, así que se consulta antes para poder
        // devolver un mensaje entendible desde la regla de negocio.
        public Task<int> ContarProductosAsociadosAsync(int proveedorId)
        {
            return _context.Productos
                .CountAsync(p => !p.EstaEliminado && p.ProveedorId == proveedorId);
        }
    }
}
