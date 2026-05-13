using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Interfaces
{
    public interface IInventarioRepository : IGeneryRepository<Inventario>
    {
        Task<Inventario?> GetByProductoIdAsync(int productoId);

        Task<List<Inventario>> GetStockBajoAsync();

        Task<List<Inventario>> GetAllInventariosAsync();
    }
}
