namespace Web.Services.Interfaces.IEndPoints.Modulo_Productos
{
    public interface ICategoriaEndpoint
    {
        string GetAll { get; }
        string GetById { get; }

        string Create { get; }

        string Update { get; }

        string Disable { get; }



       
    }
}
