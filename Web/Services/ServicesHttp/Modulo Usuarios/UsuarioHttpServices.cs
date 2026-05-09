using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Mappers;
using Web.Models.Modelo_Usuarios.UsuarioModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Usuarios;
using Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios;
using Web.Services.Mappers.Modulo_Usuarios;

namespace Web.Services.ServicesHttp.Modulo_Usuarios
{
    public class UsuarioHttpServices : IUsuarioHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IUsuariosEndpoint _usuarioEndpoint;

        public UsuarioHttpServices(IBaseHttpServices baseHttpServices, IUsuariosEndpoint usuarioEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _usuarioEndpoint = usuarioEndpoint;
        }
        public async Task<bool> CambiarPasswordAsync(ChangePasswordModel model)
        {

            var dto = UsuarioMapperM.MapChangePasswordDto(model);
            if (dto == null)
                throw new Exception("Error al mapear el modelo de cambio de contraseña.");

            var response = await _baseHttpServices.PostAsync<ChangePasswordDto,bool>(_usuarioEndpoint.cambiarPassword, dto);
            

            return response;


        }

        public async Task<bool> CambiarRolAsync(CambiarRolModel model)
        {
           var dto = UsuarioMapperM.MapCambiarRolDto(model);
            if (dto == null)
                throw new Exception("Error al mapear el modelo de cambio de rol.");

            var response =   await _baseHttpServices.PostAsync<CambiarRolDto,bool>(_usuarioEndpoint.CambiarRol, dto);
            return response;
        }



        public async Task<UsuarioModel> CreateUsuarioAsync(CreateUsuarioModel model)
        {
           var dto = UsuarioMapperM.MapAddUsuarioDto(model);
            if (dto == null)
                throw new Exception("Error al mapear el modelo de creación de usuario.");

            var response =  await _baseHttpServices.PostAsync<CreateUsuarioDto, UsuarioModel>(_usuarioEndpoint.Create, dto);
            return response;
        }


        

        public async Task<bool> DisableUsuarioAsync(int id)
        {
           var response = await _baseHttpServices.DeleteAsync($"{_usuarioEndpoint.Disable}/{id}");
            return response;
        }





        public async Task<UsuarioModel> GetUsuarioByIdAsync(int id)
        {
           var response = await _baseHttpServices.GetAsync<UsuarioModel>($"{_usuarioEndpoint.GetById}/{id}");
            return response;
        }

        public async Task<List<UsuarioModel>> GetUsuariosAsync()
        {
            var response =  await _baseHttpServices.GetAsync<List<UsuarioModel>>($"{_usuarioEndpoint.GetAll}");
            return response;
        }



        public async Task<bool> ResetPasswordAsync(ResetearPasswordModel model)
        {
            var dto = UsuarioMapperM.MapResetPasswordDto(model);
            if (dto == null)
                throw new Exception("Error al mapear el modelo de reseteo de contraseña.");

            var response = await _baseHttpServices.PostAsync<ResetearPasswordDto,bool>(_usuarioEndpoint.ResetPassword, dto);

            return response;
        }

        public async Task<UsuarioModel> UpdateUsuarioAsync(UpdateUsuarioModel model)
        {
            var dto = UsuarioMapperM.MapUpdateUsuarioDto(model);

            if (dto == null)
                throw new Exception("Error al mapear el modelo de actualización de usuario.");

            var response = await _baseHttpServices.PutAsync<UpdateUsuarioDto, UsuarioModel>($"{_usuarioEndpoint.Update}/{model.Id}", dto);

            return response;

        }
    }
}
