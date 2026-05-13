using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto
{
    public class AjustarStockDto
    {
        public int ProductoId { get; set; }

        public int NuevoStock { get; set; }
    }
}
