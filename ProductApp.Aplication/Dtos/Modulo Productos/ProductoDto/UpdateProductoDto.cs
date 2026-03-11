using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.ProductoDto
{
    public class UpdateProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; } 
        public decimal costo { get; set; } 

       public EstadoProducto Estado { get; set; } 
        public Categoria categoria { get; set; } = null!;
    }
}
