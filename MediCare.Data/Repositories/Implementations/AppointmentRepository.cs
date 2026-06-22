using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories.Implementations
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(MedContext context) : base(context) { }

        // specific appointments for doctor
        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Patient)
                .ToListAsync();
        }

        // specific appointments for patient
        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .ToListAsync();
        }

        // appointments
        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.AppointmentDate.Date == date.Date)
                .ToListAsync();
        }

        // coming appointments
        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentDate >= DateTime.Now)
                .Include(a => a.Doctor)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        // valid time
        public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime appointmentDate)
        {
            return !await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId
                    && a.AppointmentDate == appointmentDate);
        }
    }
}