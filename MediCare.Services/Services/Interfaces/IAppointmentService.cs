using MediCare.Data.Models;
using MediCare.Web.ViewModels;

namespace MediCare.Services.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
        Task<Appointment?> GetByIdAsync(int id);
        Task<ServiceResult> BookAsync(int patientId, int doctorId, DateTime appointmentDate, string? notes);
        Task<ServiceResult> ApproveAsync(int appointmentId, string doctorUserId);
        Task<ServiceResult> RejectAsync(int appointmentId, string doctorUserId);
        Task<ServiceResult> CancelAsync(int appointmentId, string requestingUserId);
        Task<ServiceResult> RescheduleAsync(int appointmentId, DateTime newDate, string requestingUserId);
    }
}