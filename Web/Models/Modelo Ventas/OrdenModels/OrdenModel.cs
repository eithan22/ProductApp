namespace Web.Models.Modelo_Ventas.OrdenModels
{
    public class OrdenModel
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = null!;
        public decimal Total { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime Fecha { get; set; }
    }
}
