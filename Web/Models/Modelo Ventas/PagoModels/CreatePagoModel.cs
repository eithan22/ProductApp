namespace Web.Models.Modelo_Ventas.PagoModels
{
    public class CreatePagoModel
    {
        public int OrdenId { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
    }
}
