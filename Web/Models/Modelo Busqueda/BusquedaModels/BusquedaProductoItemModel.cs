namespace Web.Models.Modelo_Busqueda.BusquedaModels
{
    public class BusquedaProductoItemModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public decimal Precio { get; set; }

        // Null cuando el producto todavía no tiene registro de inventario.
        public int? StockActual { get; set; }
    }
}
