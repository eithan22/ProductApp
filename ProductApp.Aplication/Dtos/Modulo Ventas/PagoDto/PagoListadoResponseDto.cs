using ProductApp.Aplication.Common;

namespace ProductApp.Aplication.Dtos.PagoDto
{
    public class PagoListadoResponseDto
    {
        public PagedResult<PagoListaResponseDto> Pagos { get; set; } = new();

        public decimal TotalRecibido { get; set; }

        public int CantidadPagos { get; set; }

        public int OrdenesSaldadas { get; set; }

        public List<PagoMetodoResumenDto> PorMetodo { get; set; } = new();
    }
}
