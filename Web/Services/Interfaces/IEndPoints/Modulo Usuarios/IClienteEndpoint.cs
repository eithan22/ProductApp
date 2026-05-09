namespace Web.Services.Interfaces.IEndPoints
{
    public interface IClienteEndpoint
    {
        string GetAll { get; }
        string GetById { get; }
        string Create { get; }
        string Update { get; }
        string Delete { get; }

        string GetBuscar { get; }

        
    }
}
