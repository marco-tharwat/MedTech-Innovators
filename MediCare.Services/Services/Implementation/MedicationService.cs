using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Services.Implementation
{
    public class MedicationService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Medication> _medicationRepo;

        public MedicationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _medicationRepo = unitOfWork.Repository<Medication>();
        }
        public async Task<bool> AddMedicationsToRecordAsync(int medicalRecordId, IEnumerable<Medication> medications)
        {
            if (medicalRecordId <= 0 || medications == null || !medications.Any()) return false;

            var recordExists = await _unitOfWork.MedicalRecords.ExistsAsync(m => m.Id == medicalRecordId);
            if (!recordExists) return false;

            try
            {

                foreach (var med in medications)
                {
                    med.MedicalRecordId = medicalRecordId;
                }
                await _medicationRepo.AddRangeAsync(medications);
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
                var medication = await _medicationRepo.GetByIdAsync(medicationId);

                if (medication == null) return false;

                _medicationRepo.Remove(medication);
                var rowsChanged = await _unitOfWork.SaveChangesAsync();
                return rowsChanged > 0;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
