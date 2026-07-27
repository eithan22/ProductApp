using System;

namespace ProductApp.Aplication.Dtos.ClienteDto
{
    public class ClienteTotalComprasDto
    {
        public int ClienteId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public int CantidadOrdenes { get; set; }
        public decimal TotalComprado { get; set; }
        public DateTime? FechaUltimaCompra { get; set; }
    }
}
