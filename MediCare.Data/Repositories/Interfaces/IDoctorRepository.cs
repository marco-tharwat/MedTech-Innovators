using MediCare.Data.Models;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<Doctor?> GetDoctorWithSpecializationAsync(int doctorId);
        Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(int specializationId);
        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(DateTime date);
        Task<Doctor?> GetDoctorWithWorkingHoursAsync(int doctorId);
        Task<Doctor?> GetDoctorWithAppointmentsAsync(int doctorId);

        Task<Doctor?> GetDoctorByUserIdAsync(string userId);
    }
}