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
    }
}
