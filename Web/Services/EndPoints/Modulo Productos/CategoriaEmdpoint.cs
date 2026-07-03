using Web.Services.Interfaces.IEndPoints.Modulo_Productos;

namespace Web.Services.EndPoints.Modulo_Productos
{
    public class CategoriaEmdpoint : ICategoriaEndpoint
    {
        public string GetAll => "Categoria/GetAllCategorias";

        public string GetById => "Categoria/GetCategoriaById/";

        public string Create => "Categoria/CreateCategoria";
        public string Update => "Categoria/UpdateCategoria/";

        public string Disable => "Categoria/DisableCategoria/";
    }
}
