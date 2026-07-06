using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;

namespace MediCare.Services.Services.Implementation
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Patient>> GetPatientsWithUsersAsync()
        {
            return await _unitOfWork.Patients.GetAllWithUsersAsync();
        }

        public async Task<Patient?> GetProfileByUserIdAsync(string userId)
        {
            return await _unitOfWork.Patients.GetPatientByUserIdAsync(userId);
        }

        public async Task<ServiceResult> UpdateProfileAsync(string userId, UpdatePatientProfileRequest request)
        {
            var patient = await _unitOfWork.Patients.GetPatientByUserIdAsync(userId);
            if (patient is null)
                return ServiceResult.Failure("Patient profile not found.");

            if (patient.User != null)
                patient.User.FullName = request.Name;

            patient.BirthDate = request.BirthDate;
            patient.BloodType = request.BloodType;
            patient.EmergencyContact = request.EmergencyContact;
            patient.Allergies = request.Allergies;

            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }
    }
}
