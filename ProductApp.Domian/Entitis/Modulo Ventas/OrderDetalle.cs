using ProductApp.Domian.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Entitis
{
    public class OrderDetalle : BaseEntity // clase intermedia n*n
    {
        public int ProductId { get; set; }
        public Producto Producto { get; set; } = null!;

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public int OrdenId { get; set; }
        public Orden Orden { get; set; } = null!;


    }
}
