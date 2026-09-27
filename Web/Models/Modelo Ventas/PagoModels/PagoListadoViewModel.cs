namespace Web.Models.Modelo_Ventas.PagoModels
{
    public class PagoListadoViewModel
    {
        public List<PagoListaModel> Pagos { get; set; } = new();

        public decimal TotalRecibido { get; set; }
        public int CantidadPagos { get; set; }
        public int OrdenesSaldadas { get; set; }
        public List<PagoMetodoResumenModel> PorMetodo { get; set; } = new();

        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        // Filtros activos, para repintarlos en la barra de herramientas.
        public int? OrdenId { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? MetodoPago { get; set; }

        public bool HayFiltros =>
            OrdenId.HasValue || Desde.HasValue || Hasta.HasValue || !string.IsNullOrWhiteSpace(MetodoPago);
    }
}
