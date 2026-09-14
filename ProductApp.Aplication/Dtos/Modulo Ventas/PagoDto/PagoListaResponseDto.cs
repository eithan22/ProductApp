using System;

namespace ProductApp.Aplication.Dtos.PagoDto
{
    public class PagoListaResponseDto
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
}
