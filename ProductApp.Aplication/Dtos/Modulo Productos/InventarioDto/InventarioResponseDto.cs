using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto
{
    public class InventarioResponseDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Producto { get; set; } = string.Empty;

        // Del producto, no del inventario: permite agrupar la vista de stock bajo por
        // proveedor sin tener que pedir cada producto por separado (RF-3.7.3).
        public int? ProveedorId { get; set; }
        public string? Proveedor { get; set; }

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
