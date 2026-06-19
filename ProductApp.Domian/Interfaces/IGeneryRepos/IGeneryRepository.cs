using ProductApp.Domian.Common.Base;
using System.Linq.Expressions;

namespace ProductApp.Domian.Interfaces.IGeneryRepos
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity);
        Task<T?> GetByIdAsync(int id);
        Task UpdateAsync(T entity);
        Task DisableAsync(int id);
        Task DeleteAsync(int id);
        Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filtro);
    }
}
