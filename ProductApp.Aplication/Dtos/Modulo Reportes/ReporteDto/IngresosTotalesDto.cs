namespace ProductApp.Aplication.Dtos.ReporteDto
{
    public class IngresosTotalesDto
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal Total { get; set; }
        public int CantidadPagos { get; set; }
        public decimal TicketPromedio { get; set; }
    }
}
