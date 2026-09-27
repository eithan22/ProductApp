using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Interfaces
{
    public interface IPagoRepository : IGenericRepository<Pago>
    {
        Task<List<Pago>> ObtenerPagosPorOrdenAsync(int ordenId);
        Task<decimal> ObtenerTotalPagadoPorOrdenAsync(int ordenId);

        Task<(List<Pago> Items, int TotalCount, decimal TotalMonto, int OrdenesSaldadas,
              Dictionary<MetodoPago, decimal> MontoPorMetodo, HashSet<int> IdsPrimerPago)>
            ObtenerPagosPaginadosAsync(
                int? ordenId, DateTime? desde, DateTime? hasta,
                MetodoPago? metodoPago, int pageNumber, int pageSize);
    }
}
