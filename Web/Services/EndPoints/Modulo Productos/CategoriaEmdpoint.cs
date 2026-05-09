using Web.Services.Interfaces.IEndPoints.Modulo_Productos;

namespace Web.Services.EndPoints.Modulo_Productos
{
    public class CategoriaEmdpoint : ICategoriaEndpoint
    {
        public string GetAll => "categorias";

        public string GetById => "categorias/{id}";

        public string Create => "categorias";   
        public string Update => "categorias/{id}";

        public string Disable => "categorias/{id}";
    }
}
