using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using Web.Models.Modelo_Usuarios.UsuarioModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios
{
    public interface IUsuarioHttpServices
    {
        Task<PagedResult<UsuarioModel>> GetUsuariosAsync(bool incluirInactivos = false, int pageNumber = 1, int pageSize = 10);
        Task<UsuarioModel> GetUsuarioByIdAsync(int id);
        Task<UsuarioModel> CreateUsuarioAsync(CreateUsuarioModel model);
        Task<UsuarioModel> UpdateUsuarioAsync(UpdateUsuarioModel model);

       // Task<bool> DeleteUsuarioAsync(int id);
        Task<bool> DisableUsuarioAsync(int id);
        Task<bool> EnableUsuarioAsync(int id);
        Task<bool> CambiarPasswordAsync(    ChangePasswordModel model);
        Task<bool> CambiarRolAsync(CambiarRolModel model);
        Task<bool> ResetPasswordAsync(ResetearPasswordModel model);
        Task<UsuarioModel> ObtenerMiPerfilAsync();
        Task<UsuarioModel> ActualizarMiPerfilAsync(UsuarioModel model);
    }
}
