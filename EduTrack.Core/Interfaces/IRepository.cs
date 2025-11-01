using System.Linq.Expressions;

namespace EduTrack.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        Task RemoveAsync(T entity);
        Task RemoveByIdAsync(int id);
        Task RemoveRangeAsync(IEnumerable<T> entities);
        Task RemoveAllAsync();

        Task UpdateAsync(T entity);
    }
}