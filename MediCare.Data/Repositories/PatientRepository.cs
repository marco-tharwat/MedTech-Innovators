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
    internal class PatientRepository : IRepository<Patient>
    {
        private readonly MedContext context;

        public PatientRepository(MedContext context)
        {
            this.context = context;
        }
        public void Create(Patient instance) => context.Patients.Add(instance);


        public void Delete(Patient instance) => context.Patients.Remove(instance);

        public List<Patient> GetAll() => context.Patients.ToList();

        public Patient GetById(int id) => context.Patients.Find(id);

        public Patient GetByName(string name)
            => context.Patients.Include(d => d.User)
            .FirstOrDefault(d => d.User.UserName == name);

        public void Update(Patient instance) => context.Patients.Update(instance);
    }
}
