namespace Web.Services.Interfaces.IEndPoints.Modulo_Usuarios
{
    public interface IUsuariosEndpoint
    {
        string GetAll { get; }
        string GetById { get; }
        string Create { get; }
        string Update { get; }
        //string Delete { get; }
        string Disable { get; }
        string Enable { get; }

        string cambiarPassword { get; }

        string CambiarRol { get; }

        string ResetPassword { get; }
    }
}
