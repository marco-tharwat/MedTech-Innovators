using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.IRepositories
{
    public interface IRepository<T>
    {
        public void Create(T instance);
        public void Update(T instance);
        public void Delete(T instance);

        public List<T> GetAll();
        public T GetById(int id);

        public T GetByName(string name);
       
    }
}
