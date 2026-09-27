namespace ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto
{
    // Mismo patrón que SubirImagenProductoDto: el controller abre el stream del IFormFile y
    // la capa de aplicación nunca conoce IFormFile (tipo de ASP.NET).
    public class ImportarProductosDto
    {
        public Stream Contenido { get; set; } = null!;
        public string NombreArchivo { get; set; } = null!;
        public long TamanoBytes { get; set; }
    }
}
