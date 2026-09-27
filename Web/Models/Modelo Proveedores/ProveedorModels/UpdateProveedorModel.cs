namespace Web.Models.Modelo_Proveedores.ProveedorModels
{
    public class UpdateProveedorModel
    {
        // Viaja en la URL del PUT, no en el body: la API lo rellena desde la ruta.
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }
}
