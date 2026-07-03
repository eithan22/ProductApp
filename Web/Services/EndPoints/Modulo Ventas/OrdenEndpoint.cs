using Web.Services.Interfaces.IEndPoints.Modulo_Ventas;

namespace Web.Services.EndPoints.Modulo_Ventas
{
    public class OrdenEndpoint : IOrdenEndpoint
    {
        public string Create => "Orden/CreateOrden";
        public string GetAll => "Orden/GetAllOrdenes";
        public string GetById => "Orden/GetOrdenById/";
        public string GetByCliente => "Orden/GetOrdenByClientes/";
        public string GetByUsuario => "Orden/GetOrdenesByUsuario/";
        public string GetByFecha => "Orden/GetOrdenByFecha/";
        public string CambiarEstado => "Orden/CambiarEstadoOrden";
        public string Cancelar => "Orden/CancelarOrden/";
    }
}
