using System;

namespace ProductApp.Aplication.Dtos.PagoDto
{
    public class PagoFiltroDto
    {
        public int? OrdenId { get; set; }

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }

        public string? MetodoPago { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
