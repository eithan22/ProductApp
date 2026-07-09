namespace ProductApp.Aplication.Dtos.ReporteDto
{
    public class VentaPorVendedorDto
    {
        public int UsuarioId { get; set; }
        public string NombreVendedor { get; set; } = string.Empty;
        public int CantidadOrdenes { get; set; }
        public decimal Total { get; set; }
    }
}
