using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories.Implementations
{
    public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(MedContext context) : base(context) { }

       
        public async Task<IEnumerable<MedicalRecord>> GetRecordsByPatientAsync(int patientId)
        {
            return await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        
        public async Task<IEnumerable<MedicalRecord>> GetRecordsByDoctorAsync(int doctorId)
        {
            return await _context.MedicalRecords
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Patient)
                .ToListAsync();
        }

        
        public async Task<MedicalRecord?> GetRecordWithDocumentsAsync(int recordId)
        {
            return await _context.MedicalRecords
                .Include(r => r.MedicalDocuments)
                .FirstOrDefaultAsync(r => r.Id == recordId);
        }

        public async Task<MedicalRecord?> GetRecordWithMedicationsAsync(int recordId)
        {
            return await _context.MedicalRecords
                .Include(r => r.Medications)
                .FirstOrDefaultAsync(r => r.Id == recordId);
        }
    }
}