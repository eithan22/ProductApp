using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Interfaces.IGeneryRepos
{
    public interface IGeneryRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity);
        Task<T?> GetByIdAsync(int id);
        Task UpdateAsync(T entity);
        Task DisebleAsync(int id);
        Task DeleteAsync(int id);

    }
}
