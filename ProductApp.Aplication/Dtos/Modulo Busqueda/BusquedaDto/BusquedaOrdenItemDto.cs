namespace ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto
{
    public class BusquedaOrdenItemDto
    {
        // Es a la vez el id y el número de orden: el dominio no guarda un número aparte.
        public int Id { get; set; }

        public string NombreCliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int CantidadProductos { get; set; }

        // Se expone como string, igual que en OrdenResponseDto, para que la Web pinte la
        // pastilla de color sin conocer el enum del dominio.
        public string Estado { get; set; } = string.Empty;

        public decimal Total { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
