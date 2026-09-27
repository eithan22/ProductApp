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

        // Mismo criterio que ObtenerEstadoSesionAsync: solo la columna que necesita el gate de
        // aceptación, sin traer la entidad ni sus órdenes. Devuelve null tanto si el usuario
        // nunca aceptó como si la fila no existe o está eliminada; los tres casos significan
        // lo mismo para el filtro, y el segundo ya lo cortó antes VerificarSesionVigenteFilter.
        Task<string?> ObtenerVersionDocumentosLegalesAceptadaAsync(int id);
    }
}
