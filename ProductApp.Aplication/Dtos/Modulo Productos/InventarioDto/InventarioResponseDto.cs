using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto
{
    public class InventarioResponseDto
    {
        public int Id { get; set; }
        public string Producto { get; set; } = string.Empty;

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
