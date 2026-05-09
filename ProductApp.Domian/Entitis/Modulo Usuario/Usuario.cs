using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Entitis
{
    public class Usuario : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public int Edad { get; set; } = 0;
        public string Email { get; set; } = string.Empty;

        public RolUsuario RolUsuario { get; set; }

        public EstadoUsuario EstadoUsuario { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public List<Orden> Ordenes { get; set; } = new List<Orden>();





        public void desactivar(Usuario usuario)
        {
            if (EstadoUsuario == EstadoUsuario.Inactivo)
                throw new InvalidOperationException("El cliente ya está inactivo.");

            EstadoUsuario = EstadoUsuario.Inactivo;
        }
        public void activar(Usuario usuario)
        {
            if (EstadoUsuario == EstadoUsuario.Activo)
                throw new InvalidOperationException("El cliente ya está activo.");
            EstadoUsuario = EstadoUsuario.Activo;


        }







    }
}
