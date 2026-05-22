using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Interfaces
{
    public interface IDetalleOrdenRepository : IGeneryRepository<OrderDetalle>
    {
        Task<List<OrderDetalle>> ObtenerDetalleOrdenPorOrdenIdAsync(int Id);

        Task<OrderDetalle?> ObtenerDetalleConProductoAsync(int id);


    }
}
