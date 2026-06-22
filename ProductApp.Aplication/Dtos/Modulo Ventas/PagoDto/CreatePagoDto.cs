namespace ProductApp.Aplication.Dtos.PagoDto
{
    public class CreatePagoDto
    {
        public int OrdenId { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
    }
}
