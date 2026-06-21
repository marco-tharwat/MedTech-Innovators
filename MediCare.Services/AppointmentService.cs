using MediCare.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services
{
    public class AppointmentService
    {
        private readonly MedContext _context;
        private readonly WorkingHoursService _workingHoursService;

        public AppointmentService(MedContext context, WorkingHoursService workingHoursService)
        {
            _context = context;
            _workingHoursService = workingHoursService;
        }

        // ---- Read ----

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Appointment>> GetByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByDoctorAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        // ---- Book an appointment ----
        public async Task<(bool Success, string? Error, Appointment? Appointment)> BookAsync(
            int patientId, int doctorId, DateTime appointmentDate, string? notes)
        {
            // Validation 1: date must not be in the past
            if (appointmentDate <= DateTime.Now)
                return (false, "Appointment date cannot be in the past.", null);

            // Validation 2: slot must actually be available (inside working hours and not already booked)
            var availableSlots = await _workingHoursService.GetAvailableSlotsAsync(doctorId, appointmentDate.Date);
            bool isValidSlot = availableSlots.Contains(appointmentDate);

            if (!isValidSlot)
                return (false, "This slot is not available, please pick another time.", null);

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                Status = Status.Pending,
                Notes = notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return (true, null, appointment);
        }

        // ---- Cancel an appointment ----
        public async Task<(bool Success, string? Error)> CancelAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return (false, "Appointment not found.");

            if (appointment.Status == Status.Cancelled)
                return (false, "This appointment is already cancelled.");

            if (appointment.Status == Status.Completed)
                return (false, "Cannot cancel an appointment that is already completed.");

            appointment.Status = Status.Cancelled;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        // ---- Reschedule an appointment ----
        public async Task<(bool Success, string? Error)> RescheduleAsync(int appointmentId, DateTime newAppointmentDate)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return (false, "Appointment not found.");

            if (appointment.Status == Status.Cancelled || appointment.Status == Status.Completed)
                return (false, "Cannot reschedule a cancelled or completed appointment.");

            if (newAppointmentDate <= DateTime.Now)
                return (false, "Appointment date cannot be in the past.");

            var availableSlots = await _workingHoursService.GetAvailableSlotsAsync(appointment.DoctorId, newAppointmentDate.Date);
            bool isValidSlot = availableSlots.Contains(newAppointmentDate);

            if (!isValidSlot)
                return (false, "The new slot is not available, please pick another time.");

            appointment.AppointmentDate = newAppointmentDate;
            appointment.Status = Status.Pending; // needs re-confirmation after reschedule
            await _context.SaveChangesAsync();

            return (true, null);
        }

        // ---- Update status (e.g. doctor confirms / completes an appointment) ----
        public async Task<(bool Success, string? Error)> UpdateStatusAsync(int appointmentId, Status newStatus)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return (false, "Appointment not found.");

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        // ---- Delete ----
        public async Task<(bool Success, string? Error)> DeleteAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return (false, "Appointment not found.");

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}