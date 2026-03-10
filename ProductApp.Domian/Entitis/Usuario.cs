using ProductApp.Domian.Common.Base;
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

        public string Password { get; set;} = string.Empty;

        public List<Orden> Ordenes { get; set; } = new List<Orden>();
       
    }
}
