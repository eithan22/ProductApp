using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Mappers
{
    public class UsuarioMapper : IMapperUsuario
    {
        public Usuario MapToEntity(CreateUsuarioDto dto)
        {
            return new Usuario(dto.Nombre, dto.Email, dto.UserName, dto.RolUsuario);
        }

        public UsuarioResponseDto ToDto(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                UserName = usuario.Username,
                Edad = usuario.Edad,
                FechaNacimiento = usuario.FechaNacimiento,
                RolUsuario = usuario.RolUsuario.ToString(),
                EstadoUsuario = usuario.EstadoUsuario.ToString()
            };
        }

        public void mapUpdate(UpdateUsuarioDto dto, Usuario usuario)
        {
            usuario.CambiarNombre(dto.Nombre);
            usuario.CambiarEmail(dto.Email);
            usuario.CambiarRol(dto.RolUsuario);
            usuario.EstablecerFechaNacimiento(dto.FechaNacimiento);
        }

        public Usuario MapFromRegisterDto(RegisterDto dto)
        {
            return new Usuario(dto.Nombre, dto.Email, dto.UserName, RolUsuario.Vendedor);
        }
    }
}
