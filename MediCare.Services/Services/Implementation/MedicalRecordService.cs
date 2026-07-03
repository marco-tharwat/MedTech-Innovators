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
            if (patientId <= 0) return null;

            var patientExists = await _unitOfWork.Patients.ExistsAsync(p => p.Id == patientId);
            if (!patientExists) return null;

            return await _unitOfWork.MedicalRecords.GetRecordsByPatientAsync(patientId);
        }

        public async Task<IEnumerable<MedicalRecord>> GetPatientMedicalRecordsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Enumerable.Empty<MedicalRecord>();
            var patient = await _unitOfWork.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null) return Enumerable.Empty<MedicalRecord>();

            var query = _unitOfWork.MedicalRecords.Query();

            var records = await query.
                Include(r => r.Doctor).ThenInclude(d => d.User).
                Include(r => r.Medications).
                Where(r => r.Patient.Id == patient.Id).
                ToListAsync();

            return records;
        }

        public async Task<MedicalRecord?> GetRecordDetailsAsync(int recordId)
        {
            if (recordId <= 0) return null;

            var recordExists = await _unitOfWork.MedicalRecords.ExistsAsync(r => r.Id == recordId);
            if (!recordExists) return null;

            var query = _unitOfWork.MedicalRecords.Query();

            var result = await query.Include(r => r.Medications).
                Include(r => r.MedicalDocuments).
                Include(r => r.Patient).ThenInclude(p => p.User).
                Include(r => r.Doctor).ThenInclude(d => d.User).
                Include(r => r.Doctor).ThenInclude(d => d.Specialization).
                FirstOrDefaultAsync(r => r.Id == recordId);

            return result;
        }
    }
}
