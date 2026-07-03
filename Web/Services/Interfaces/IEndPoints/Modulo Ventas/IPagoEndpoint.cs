namespace Web.Services.Interfaces.IEndPoints.Modulo_Ventas
{
    public interface IPagoEndpoint
    {
        string RegistrarPago { get; }
        string GetPagosByOrden { get; }
        string GetSaldoPendiente { get; }
    }
}
