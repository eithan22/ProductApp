using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Interfaces
{
    public interface IOrdenRepository : IGeneryRepository<Orden>
    {
        Task<List<Orden>> ObtenerPorClienteAsync(int clienteId);
         Task<List<Orden>> ObtenerPorFechaAsync(DateTime fecha);

        

        

    }
}
