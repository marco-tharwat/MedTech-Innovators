using MediCare.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    //Set<T>() method requires this constraint so it knows T will always be
    //a database entity (a reference type/class)
    //and not a value type like an integer or a boolean.
    public class Repository<T> : IRepository<T> where T:class
    {
        private readonly MedContext context;

        public Repository(MedContext context)
        {
            this.context = context;
        }
        public void Create(T instance) => context.Set<T>().Add(instance);
        public void Delete(T instance) => context.Set<T>().Remove(instance);
        public void Update(T instance) => context.Set<T>().Update(instance);

        public async Task<List<T>> GetAllAsync() => await context.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);

    }
}
