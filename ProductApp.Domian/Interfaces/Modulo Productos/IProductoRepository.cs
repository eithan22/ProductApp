using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Interfaces
{
    public interface IProductoRepository : IGeneryRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetProductosConCategoriaAll();

        Task<List<Producto>> BuscarProductosAsync(string? nombre, string? categoria);

        Task<Producto?> ObtenerProductoConInventarioAsync(int id);


    }
}
