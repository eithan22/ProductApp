using Web.Services.Interfaces.IEndPoints.Modulo_Proveedores;

namespace Web.Services.EndPoints.Modulo_Proveedores
{
    public class ProveedorEndpoint : IProveedorEndpoint
    {
        public string GetAll => "Proveedor/GetProveedores";

        public string GetById => "Proveedor/GetByIdProveedor/";

        public string Create => "Proveedor/CreateProveedor";

        public string Update => "Proveedor/UpdateProveedor/";

        public string Disable => "Proveedor/DisableProveedor/";

        public string Enable => "Proveedor/EnableProveedor/";

        public string GetBuscar => "Proveedor/GetBuscar";
    }
}
