namespace ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto
{
    public class ProveedorResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        // El dominio guarda un enum EstadoProveedor, pero hacia afuera se expone un bool:
        // es lo que pide el contrato de docs/06-openapi.yaml y lo que la Web necesita para
        // pintar un switch sin conocer el enum.
        public bool Activo { get; set; }
    }
}
