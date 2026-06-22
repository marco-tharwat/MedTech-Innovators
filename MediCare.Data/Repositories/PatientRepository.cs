using MediCare.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    public class PatientRepository : Repository<Patient>,IPatientRepository
    {
        public PatientRepository(MedContext context):base(context) { }
        

    }
}
