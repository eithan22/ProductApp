using Web.Services.Interfaces.IEndPoints;

namespace Web.Services.EndPoints
{
    public class ClienteEndpoint : IClienteEndpoint
    {
        public string GetAll => "Cliente/GetClientes";

        public string GetById => "Cliente/GetByIdClientes/";

        public string Create => "Cliente/CreateClientes";
        public string Update => "Cliente/UpdateCliente/";

        public string Delete => "Cliente/DisableCliente/";

        public string Enable => "Cliente/EnableCliente/";

        public string GetBuscar => "Cliente/GetBuscar";
    }
}
