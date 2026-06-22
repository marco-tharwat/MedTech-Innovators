using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories.Implementations
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(MedContext context) : base(context) { }

        
        public async Task<Patient?> GetPatientWithMedicalRecordsAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.MedicalRecords)
                .FirstOrDefaultAsync(p => p.Id == patientId);
        }

        
        public async Task<Patient?> GetPatientWithAppointmentsAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.Id == patientId);
        }

        // search by name 
        public async Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string name)
        {
            return await _context.Patients
                .Where(p => p.Name.Contains(name))
                .ToListAsync();
        }

        // search by id
        public async Task<Patient?> GetPatientByUserIdAsync(string userId)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}