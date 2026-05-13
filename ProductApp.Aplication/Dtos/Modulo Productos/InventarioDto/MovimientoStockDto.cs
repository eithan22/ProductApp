using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto
{
    public class MovimientoStockDto
    {
        public int ProductoId { get; set; }
        public int cantidad { get; set; } = 0;
    }
}
