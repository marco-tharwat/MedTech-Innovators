using MediCare.Data.Models;

namespace MediCare.Services.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<Doctor>> SearchAsync(string? searchTerm, int? specializationId, string? location);
        Task<Doctor?> GetDetailsAsync(int id);
        Task<Doctor?> GetByUserIdAsync(string userId);
        Task<bool> ExistAsync(string userId);
    }
}
