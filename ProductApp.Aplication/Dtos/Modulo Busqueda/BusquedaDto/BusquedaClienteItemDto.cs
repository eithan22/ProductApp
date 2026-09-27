namespace ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto
{
    public class BusquedaClienteItemDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}
