using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.ProductoDto
{
    public  class ProductoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }

        public decimal Costo { get; set; }

        public EstadoProducto Estado { get; set; }

        public string? Categoria{ get; set; } // lo pongo que null porque en el reponse de create no se mostrara la categoria en si , pero si en el getallProductos 


    }
}
