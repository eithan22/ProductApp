namespace Web.Models.Modelo_Productos.CategoriaModels
{
    public class CategoriaModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;

        public string Descripcion { get; set; } = null!;

        public String Estado { get; set; } = null!; // para mostrar si esta activo o desactivado sin el enum
    }
}
