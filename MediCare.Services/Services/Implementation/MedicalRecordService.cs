using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Services.Implementation
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalRecordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> CreateMedicalRecordAsync(MedicalRecord record)
        {
            if (record == null || record.PatientId <= 0 || record.DoctorId <= 0) return false;

            var patientExists = await _unitOfWork.Patients.ExistsAsync(p => p.Id == record.PatientId);
            var doctorExists = await _unitOfWork.Doctors.ExistsAsync(d => d.Id == record.DoctorId);

            if (!patientExists || !doctorExists) return false;

            try
            {
                await _unitOfWork.MedicalRecords.AddAsync(record);
                var rowsChanged = await _unitOfWork.SaveChangesAsync();

                return rowsChanged > 0;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<IEnumerable<MedicalRecord>> GetPatientHistoryAsync(int patientId)
        {
            return await _unitOfWork.MedicalRecords.GetRecordsByPatientAsync(patientId);
        }

        public async Task<MedicalRecord?> GetRecordDetailsAsync(int recordId)
        {
            var query = _unitOfWork.MedicalRecords.Query();

            var result = await query.Include(r => r.Medications).
                Include(r => r.MedicalDocuments).
                Include(r => r.Patient).
                Include(r => r.Doctor).
                FirstOrDefaultAsync(r => r.Id == recordId);

            return result;
        }
    }
}
