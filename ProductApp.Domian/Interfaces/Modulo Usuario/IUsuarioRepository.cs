using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByUsernameAsync(string username);
        Task<(List<Usuario> Items, int TotalCount)> GetAllUsuariosAsync(bool incluirInactivos, int pageNumber, int pageSize);
        Task<List<int>> ObtenerIdsAdministradoresActivosAsync();

        // Solo lo que necesita la verificación de sesión en cada petición: rol y estado, sin
        // traer la entidad completa ni sus órdenes. Existe = false si no hay fila o está eliminada.
        Task<(bool Existe, RolUsuario Rol, EstadoUsuario Estado)> ObtenerEstadoSesionAsync(int id);
    }
}
