using MediCare.Data.Models;

namespace MediCare.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<bool> AddMedicationsToRecordAsync(int medicalRecordId, IEnumerable<Medication> medications);
        Task<bool> RemoveMedicationAsync(int medicationId);
    }
}