using Web.Services.Interfaces.IEndPoints.Modulo_Ventas;

namespace Web.Services.EndPoints.Modulo_Ventas
{
    public class PagoEndpoint : IPagoEndpoint
    {
        public string RegistrarPago => "Pago/RegistrarPago";
        public string GetPagosByOrden => "Pago/GetPagosByOrden/";
        public string GetSaldoPendiente => "Pago/GetSaldoPendiente/";
    }
}
