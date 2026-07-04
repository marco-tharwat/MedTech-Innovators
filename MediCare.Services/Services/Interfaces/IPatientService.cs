using MediCare.Data.Models;
using MediCare.Services.DTO;
using MediCare.Web.ViewModels;

namespace MediCare.Services.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient?> GetProfileByUserIdAsync(string userId);
        Task<ServiceResult> UpdateProfileAsync(string userId, UpdatePatientProfileRequest request);
    }
}
