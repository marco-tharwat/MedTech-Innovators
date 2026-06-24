using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Services.Implementation
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork unitOfWork;

        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<bool> AddMedicationsToRecordAsync(int medicalRecordId, IEnumerable<Medication> medications)
        {
            if (medicalRecordId <= 0 || medications == null || !medications.Any()) return false;
            try
            {
                var medicationsRepo = unitOfWork.Repository<Medication>();

                foreach (var med in medications)
                {
                    med.MedicalRecordId = medicalRecordId;
                    await medicationsRepo.AddAsync(med);
                }

                var rowsChanged = await unitOfWork.SaveChangesAsync();
                return rowsChanged > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveMedicationAsync(int medicationId)
        {
            throw new NotImplementedException();
        }
    }
}
