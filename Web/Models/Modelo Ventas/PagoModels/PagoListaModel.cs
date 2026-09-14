using ProductApp.Aplication.Common;

namespace Web.Models.Modelo_Ventas.PagoModels
{
    public class PagoListaModel
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public decimal TotalOrden { get; set; }
        public string EstadoOrden { get; set; } = string.Empty;
        public bool EsPrimerPago { get; set; }
    }

    public class PagoMetodoResumenModel
    {
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
    }

    public class PagoListadoModel
    {
        public PagedResult<PagoListaModel> Pagos { get; set; } = new();
        public decimal TotalRecibido { get; set; }
        public int CantidadPagos { get; set; }
        public int OrdenesSaldadas { get; set; }
        public List<PagoMetodoResumenModel> PorMetodo { get; set; } = new();
    }
}
