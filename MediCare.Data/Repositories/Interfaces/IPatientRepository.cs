using MediCare.Data.Models;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<Patient?> GetPatientWithMedicalRecordsAsync(int patientId);
        Task<Patient?> GetPatientWithAppointmentsAsync(int patientId);
        Task<Patient?> GetPatientByUserIdAsync(string userId);
    }
}