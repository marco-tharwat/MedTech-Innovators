using MediCare.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    public class MedicalRecordRepository : Repository<MedicalRecord>,IMedicalRecordRepository
    {
        public MedicalRecordRepository(MedContext context) : base(context) { }
        
    }
}
