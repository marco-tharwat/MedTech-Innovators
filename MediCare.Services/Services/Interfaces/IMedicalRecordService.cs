using MediCare.Data.Models;

namespace MediCare.Services.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<MedicalRecord?> GetRecordDetailsAsync(int recordId);
        Task<IEnumerable<MedicalRecord>> GetPatientHistoryAsync(int patientId);
        Task<bool> CreateMedicalRecordAsync(MedicalRecord record);
    }
}