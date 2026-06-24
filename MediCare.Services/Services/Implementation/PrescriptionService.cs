using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Services.Implementation
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddMedicationsToRecordAsync(int medicalRecordId, IEnumerable<Medication> medications)
        {
            if (medicalRecordId <= 0 || medications == null || !medications.Any()) return false;
            try
            {
                var medicationsRepo = GetRepo();

                foreach (var med in medications)
                {
                    med.MedicalRecordId = medicalRecordId;
                    await medicationsRepo.AddAsync(med);
                }

                var rowsChanged = await _unitOfWork.SaveChangesAsync();
                return rowsChanged > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveMedicationAsync(int medicationId)
        {
            if (medicationId <= 0) return false;
            try
            {
                var medicationsRepo = GetRepo();
                var medication = await medicationsRepo.GetByIdAsync(medicationId);

                if (medication == null) return false;

                medicationsRepo.Remove(medication);
                var rowsChanged = await _unitOfWork.SaveChangesAsync();
                return rowsChanged > 0;
            }
            catch (Exception)
            {

                return false;
            }
        }
        private IRepository<Medication> GetRepo()
        {
            return _unitOfWork.Repository<Medication>();
        }
    }
}
