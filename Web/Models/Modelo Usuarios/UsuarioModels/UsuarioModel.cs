namespace Web.Models.Modelo_Usuarios.UsuarioModels
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public int Edad { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public string RolUsuario { get; set; } = null!;
        public string EstadoUsuario { get; set; } = null!;
    }
}
