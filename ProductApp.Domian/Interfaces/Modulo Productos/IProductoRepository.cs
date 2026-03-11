using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Interfaces
{
    public interface IProductoRepository
    {
        Task<IEnumerable<Producto>> GetAllAsync();

        Task<Producto> CreateAsync(Producto producto);

        Task<Producto?> GetByIdAsync(int id);

        Task UpdateAsync(Producto producto);

        Task DisebleAsync(int id);

        Task DeleteAsync(int id);

    }
}
