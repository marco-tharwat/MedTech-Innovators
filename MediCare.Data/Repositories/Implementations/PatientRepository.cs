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

        public async Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Enumerable.Empty<Patient>();

            fullName = fullName.Trim().ToLower();

            return await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User != null &&
                            p.User.FullName != null &&
                            p.User.FullName.ToLower().Contains(fullName))
                .AsNoTracking()
                .ToListAsync();
        }

        // search by Id 
        public async Task<IEnumerable<Patient>> SearchPatientsByIdAsync(int id)
        {
            return await _context.Patients
                .Where(p => p.Id == id)
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