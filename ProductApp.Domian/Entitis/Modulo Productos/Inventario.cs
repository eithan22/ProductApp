using ProductApp.Domian.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Entitis
{
    public class Inventario : BaseEntity
    {
        public int CantidadActual { get; set; }

       public int CantidadMinima { get; set; }

       
       public int ProductoId { get; set; }

       public Producto Producto { get; set; } = null!;



    }
}
