using MediCare.Data.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    public class DoctorRepository : Repository<Doctor>,IDoctorRepository
    {
        private readonly MedContext context;

        public DoctorRepository(MedContext context):base(context)
        {
            this.context = context;
        }

        public async Task<Doctor?> GetProfileForBookingAsync(int id)
        {
            return await context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Include(d => d.WorkingHours).FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
