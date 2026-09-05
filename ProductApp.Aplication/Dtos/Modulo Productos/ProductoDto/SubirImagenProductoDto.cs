namespace ProductApp.Aplication.Dtos.ProductoDto
{
    public class SubirImagenProductoDto
    {
        public int ProductoId { get; set; }
        public Stream Contenido { get; set; } = null!;
        public string NombreArchivo { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long TamanoBytes { get; set; }
    }
}
