using System.Linq.Expressions;
using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly MedContext _context;

        public Repository(MedContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_context.Set<T>().Where(predicate).AsEnumerable());
        }

        public async Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_context.Set<T>().FirstOrDefault(predicate));
        }

        public async Task<bool> ExistsAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_context.Set<T>().Any(predicate));
        }

        public async Task<int> CountAsync(Func<T, bool>? predicate = null)
        {
            if (predicate == null)
                return await Task.FromResult(_context.Set<T>().Count());

            return await Task.FromResult(_context.Set<T>().Count(predicate));
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public IQueryable<T> Query()
        {
            return _context.Set<T>().AsQueryable();
        }
    }
}