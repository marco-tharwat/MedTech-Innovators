using MediCare.Data.Models;
using MediCare.Web.ViewModels;

namespace MediCare.Services.Services.Interfaces
{
    public interface IWorkingHoursService
    {
        Task<IEnumerable<WorkingHours>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int doctorId, DateTime date);
        Task<ServiceResult> AddAsync(WorkingHours workingHours);
        Task<ServiceResult> UpdateAsync(WorkingHours workingHours);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
