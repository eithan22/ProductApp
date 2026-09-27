namespace Web.Models.Modelo_Busqueda.BusquedaModels
{
    public class BusquedaOrdenItemModel
    {
        // Es a la vez el id y el número de orden que se muestra (#1046).
        public int Id { get; set; }

        public string NombreCliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int CantidadProductos { get; set; }

        // String, no enum: la vista pinta la pastilla con estado-@Estado.ToLower().
        public string Estado { get; set; } = string.Empty;

        public decimal Total { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
