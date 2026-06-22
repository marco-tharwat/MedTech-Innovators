using MediCare.Data.Models;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IMedicalRecordRepository : IRepository<MedicalRecord>
    {
        Task<IEnumerable<MedicalRecord>> GetRecordsByPatientAsync(int patientId);
        Task<IEnumerable<MedicalRecord>> GetRecordsByDoctorAsync(int doctorId);
        Task<MedicalRecord?> GetRecordWithDocumentsAsync(int recordId);
        Task<MedicalRecord?> GetRecordWithMedicationsAsync(int recordId);
    }
}