namespace Web.Models.ClienteModels
{
    public class UpdateClientemodel
    {
        public int Id { get; set; } // no se usa

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

    }
}
