namespace Web.Models.Modelo_Proveedores.ProveedorModels
{
    public class ProveedorModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        // La API expone el estado como bool, no como el enum del dominio:
        // la vista solo necesita saber si está activo o no.
        public bool Activo { get; set; }
    }
}
