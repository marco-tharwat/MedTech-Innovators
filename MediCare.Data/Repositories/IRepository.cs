using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    public interface IRepository<T>
    {
        // only work in memory
        public void Create(T instance);
        public void Update(T instance);
        public void Delete(T instance);


        // go to DB
        public Task<List<T>> GetAllAsync();
        public Task<T?> GetByIdAsync(int id);
       
    }
}
