using MediCare.Data.IRepositories;
using MediCare.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    internal class DoctorRepository : IRepository<Doctor>
    {
        private readonly MedContext context;

        public DoctorRepository(MedContext context)
        {
            this.context = context;
        }
        public void Create(Doctor instance) => context.Doctors.Add(instance);


        public void Delete(Doctor instance) => context.Doctors.Remove(instance);

        public List<Doctor> GetAll() => context.Doctors.ToList();

        public Doctor GetById(int id) => context.Doctors.Find(id);

        public Doctor GetByName(string name)
            => context.Doctors.Include(d => d.User)
            .FirstOrDefault(d => d.User.UserName == name);

        public void Update(Doctor instance) => context.Doctors.Update(instance);
    }
}
