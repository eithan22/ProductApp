namespace Web.Models.Modelo_Ventas.PagoModels
{
    public class OrdenCobrableModel
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
