namespace Web.Models.Modelo_Ventas.DetalleOrdenModels
{
    public class DetalleOrdenModel
    {
        public int ID { get; set; }
        public string Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public int OrdenId { get; set; }
    }
}
