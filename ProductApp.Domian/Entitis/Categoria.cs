using ProductApp.Domian.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Entitis
{
    public class Categoria : BaseEntity
    {
        public string Nombre { get; set; }  = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public List<Producto> Productos { get; set; } = new List<Producto>();



    }
}
