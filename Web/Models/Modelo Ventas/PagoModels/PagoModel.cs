namespace Web.Models.Modelo_Ventas.PagoModels
{
    public class PagoModel
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
