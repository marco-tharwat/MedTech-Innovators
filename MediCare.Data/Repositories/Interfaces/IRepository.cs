using System.Linq.Expressions;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Read 
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(<Func<T, bool> predicate);
        Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate);
        Task<bool> ExistsAsync(Func<T, bool> predicate);
        Task<int> CountAsync(Func<T, bool>? predicate = null);

        // Write 
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        // for query
        IQueryable<T> Query();
    }
}