using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Services.Implementation
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork unitOfWork;

        public MedicalRecordService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<bool> CreateMedicalRecordAsync(MedicalRecord record)
        {
            if (record == null || record.PatientId <= 0 || record.DoctorId <= 0) return false;
            try
            {
                await unitOfWork.MedicalRecords.AddAsync(record);
                var rowsChanged = await unitOfWork.SaveChangesAsync();

                return rowsChanged > 0;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<IEnumerable<MedicalRecord>> GetPatientHistoryAsync(int patientId)
        {
            return await unitOfWork.MedicalRecords.GetRecordsByPatientAsync(patientId);
        }

        public async Task<MedicalRecord?> GetRecordDetailsAsync(int recordId)
        {
            var query = unitOfWork.MedicalRecords.Query();

            var result = query.Include(r => r.Medications).
                Include(r => r.MedicalDocuments).
                Include(r => r.Patient).
                Include(r => r.Doctor).
                FirstOrDefault(r => r.Id == recordId);

            return result;
        }
    }
}
