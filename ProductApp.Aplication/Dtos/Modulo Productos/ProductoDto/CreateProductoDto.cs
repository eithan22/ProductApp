using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.ProductoDto
{
    public class CreateProductoDto
    {
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }

        public EstadoProducto EstadoProducto { get; set; }

        public Categoria Categoria { get; set; }=null!;


        
    }
}
