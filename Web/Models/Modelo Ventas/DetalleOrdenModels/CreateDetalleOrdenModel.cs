namespace Web.Models.Modelo_Ventas.DetalleOrdenModels
{
    public class CreateDetalleOrdenModel
    {
        public int ProductId { get; set; }
        public int Cantidad { get; set; }
        public int OrdenId { get; set; }
    }
}
