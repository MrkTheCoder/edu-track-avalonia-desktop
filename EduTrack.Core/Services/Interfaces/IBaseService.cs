using System.Linq.Expressions;

namespace EduTrack.Core.Services.Interfaces
{
    public interface IBaseService<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        Task AddAsync(T entity);

        Task RemoveByIdAsync(int id);

        Task UpdateAsync(T entity);
    }
}
