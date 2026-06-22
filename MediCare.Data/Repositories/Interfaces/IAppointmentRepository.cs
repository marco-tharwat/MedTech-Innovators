using MediCare.Data.Models;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int patientId);
        Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime appointmentDate);
    }
}