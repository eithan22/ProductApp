namespace ProductApp.Aplication.Dtos.ProductoDto
{
    public class UpdateProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public int CategoriaId { get; set; }

        // null significa "no tocar la imagen actual": el update por JSON no sube ni borra archivos,
        // eso se hace contra el endpoint dedicado de imagen.
        public string? ImagenUrl { get; set; }
    }
}
