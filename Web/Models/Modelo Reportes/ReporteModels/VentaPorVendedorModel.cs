namespace Web.Models.Modelo_Reportes.ReporteModels
{
    public class VentaPorVendedorModel
    {
        public int UsuarioId { get; set; }
        public string NombreVendedor { get; set; } = string.Empty;
        public int CantidadOrdenes { get; set; }
        public decimal Total { get; set; }
    }
}
