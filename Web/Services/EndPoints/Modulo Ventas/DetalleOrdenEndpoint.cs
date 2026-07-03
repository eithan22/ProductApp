using Web.Services.Interfaces.IEndPoints.Modulo_Ventas;

namespace Web.Services.EndPoints.Modulo_Ventas
{
    public class DetalleOrdenEndpoint : IDetalleOrdenEndpoint
    {
        public string Create => "DetalleOrden/CreateDetalleOrden";
        public string GetByOrden => "DetalleOrden/GetDetalleOrden/";
        public string Update => "DetalleOrden/UpdateDetalleOrden/";
        public string Delete => "DetalleOrden/DeleteDetalleOrden/";
    }
}
