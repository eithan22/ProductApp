namespace ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto
{
    public class BusquedaProductoItemDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Dato secundario de la fila. El proyecto no maneja SKU, así que el meta del
        // resultado es la categoría.
        public string? Categoria { get; set; }

        public decimal Precio { get; set; }

        // Null cuando el producto todavía no tiene registro de inventario asociado.
        public int? StockActual { get; set; }
    }
}
