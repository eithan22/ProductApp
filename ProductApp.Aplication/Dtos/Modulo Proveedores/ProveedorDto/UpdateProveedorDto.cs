namespace ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto
{
    public class UpdateProveedorDto
    {
        // Lo rellena el controller desde la ruta, igual que en Cliente: el cliente HTTP
        // no tiene que mandarlo en el body.
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }
}
