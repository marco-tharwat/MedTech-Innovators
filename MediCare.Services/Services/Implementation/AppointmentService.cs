using MediCare.Data.Models;
using MediCare.Data.Models.Enum;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using MediCare.Services.Factory;
using MediCare.Services.Services;

namespace MediCare.Services.Services.Implementation 
{
    public class AppointmentService : IAppointmentService
    {
        private readonly MedContext _context;
        private readonly AppointmentFactory _appointmentFactory;
        private const int SlotDurationMinutes = 30;
        public AppointmentService(MedContext context, AppointmentFactory appointmentFactory)
        {
            _context = context;
            _appointmentFactory = appointmentFactory;
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // ── Book ──────────────────────────────────────────────────────────────

        public async Task<ServiceResult> BookAsync(int patientId, int doctorId, DateTime slot, string? notes)
        {
            if (slot <= DateTime.Now)
                return ServiceResult.Failure("Appointment date must be in the future.");

            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == doctorId && w.DayOfWeek == slot.DayOfWeek);

            if (workingHours is null)
                return ServiceResult.Failure("The doctor does not work on this day.");

            if (slot.TimeOfDay < workingHours.StartTime || slot.TimeOfDay >= workingHours.EndTime)
                return ServiceResult.Failure("Selected time is outside the doctor's working hours.");

            // Conflict detection — prevent double-booking
            var isTaken = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId
                            && a.AppointmentDate == slot
                            && a.Status != Status.Cancelled);

            if (isTaken)
                return ServiceResult.Failure("This slot is already booked. Please choose another.");

            var appointment = await _appointmentFactory
     .CreateAsync(patientId, doctorId, slot, notes);

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        // ── Approve ───────────────────────────────────────────────────────────

        public async Task<ServiceResult> ApproveAsync(int appointmentId, string doctorUserId)
        {
            var appointment = await GetByIdAsync(appointmentId);
            if (appointment is null)
                return ServiceResult.Failure("Appointment not found.");

            // Only the doctor assigned to this appointment can approve
            if (appointment.Doctor.UserId != doctorUserId)
                return ServiceResult.Failure("You are not authorised to approve this appointment.");

            if (appointment.Status != Status.Pending)
                return ServiceResult.Failure("Only pending appointments can be approved.");

            appointment.Status = Status.Confirmed;
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        // ── Reject ────────────────────────────────────────────────────────────

        public async Task<ServiceResult> RejectAsync(int appointmentId, string doctorUserId)
        {
            var appointment = await GetByIdAsync(appointmentId);
            if (appointment is null)
                return ServiceResult.Failure("Appointment not found.");

            // Only the doctor assigned to this appointment can reject
            if (appointment.Doctor.UserId != doctorUserId)
                return ServiceResult.Failure("You are not authorised to reject this appointment.");

            if (appointment.Status == Status.Completed)
                return ServiceResult.Failure("A completed appointment cannot be rejected.");

            if (appointment.Status == Status.Cancelled)
                return ServiceResult.Failure("This appointment is already cancelled.");

            appointment.Status = Status.Cancelled;
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        // ── Cancel ────────────────────────────────────────────────────────────

        public async Task<ServiceResult> CancelAsync(int appointmentId, string requestingUserId)
        {
            var appointment = await GetByIdAsync(appointmentId);
            if (appointment is null)
                return ServiceResult.Failure("Appointment not found.");

            // Only the patient who owns it or the doctor can cancel
            bool isOwner = appointment.Patient.UserId == requestingUserId
                        || appointment.Doctor.UserId == requestingUserId;
            if (!isOwner)
                return ServiceResult.Failure("You are not authorised to cancel this appointment.");

            if (appointment.Status == Status.Completed)
                return ServiceResult.Failure("A completed appointment cannot be cancelled.");

            if (appointment.Status == Status.Cancelled)
                return ServiceResult.Failure("This appointment is already cancelled.");

            appointment.Status = Status.Cancelled;
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        // ── Reschedule ────────────────────────────────────────────────────────

        public async Task<ServiceResult> RescheduleAsync(int appointmentId, DateTime newSlot, string requestingUserId)
        {
            var appointment = await GetByIdAsync(appointmentId);
            if (appointment is null)
                return ServiceResult.Failure("Appointment not found.");

            bool isOwner = appointment.Patient.UserId == requestingUserId
                        || appointment.Doctor.UserId == requestingUserId;
            if (!isOwner)
                return ServiceResult.Failure("You are not authorised to reschedule this appointment.");

            if (appointment.Status == Status.Completed)
                return ServiceResult.Failure("A completed appointment cannot be rescheduled.");

            if (appointment.Status == Status.Cancelled)
                return ServiceResult.Failure("A cancelled appointment cannot be rescheduled.");

            if (newSlot <= DateTime.Now)
                return ServiceResult.Failure("New date must be in the future.");

            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == appointment.DoctorId && w.DayOfWeek == newSlot.DayOfWeek);

            if (workingHours is null)
                return ServiceResult.Failure("The doctor does not work on this day.");

            if (newSlot.TimeOfDay < workingHours.StartTime || newSlot.TimeOfDay >= workingHours.EndTime)
                return ServiceResult.Failure("Selected time is outside the doctor's working hours.");

            // Conflict detection — exclude current appointment from check
            var isTaken = await _context.Appointments
                .AnyAsync(a => a.DoctorId == appointment.DoctorId
                            && a.AppointmentDate == newSlot
                            && a.Status != Status.Cancelled
                            && a.Id != appointmentId);

            if (isTaken)
                return ServiceResult.Failure("This slot is already booked. Please choose another.");

            appointment.AppointmentDate = newSlot;
            appointment.Status = Status.Pending;
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }
    }
}