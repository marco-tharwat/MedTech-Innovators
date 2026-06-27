using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly MedContext _context;
        private const int SlotDurationMinutes = 30;

        public AppointmentService(MedContext context)
        {
            _context = context;
        }

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

        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == doctorId && w.DayOfWeek == date.DayOfWeek);

            if (workingHours is null)
                return Enumerable.Empty<DateTime>();

            // ولّد كل الـ slots
            var slots = new List<DateTime>();
            var current = date.Date.Add(workingHours.StartTime);
            var end     = date.Date.Add(workingHours.EndTime);

            while (current.AddMinutes(SlotDurationMinutes) <= end)
            {
                slots.Add(current);
                current = current.AddMinutes(SlotDurationMinutes);
            }

            // جيب المحجوز
            var booked = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == date.Date
                         && a.Status != Status.Cancelled)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            return slots.Where(s => s > DateTime.Now && !booked.Contains(s)).ToList();
        }

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

            var isTaken = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId
                            && a.AppointmentDate == slot
                            && a.Status != Status.Cancelled);

            if (isTaken)
                return ServiceResult.Failure("This slot is already booked. Please choose another.");

            await _context.Appointments.AddAsync(new Appointment
            {
                PatientId       = patientId,
                DoctorId        = doctorId,
                AppointmentDate = slot,
                Notes           = notes,
                Status          = Status.Pending
            });

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> CancelAsync(int appointmentId, string requestingUserId)
        {
            var appointment = await GetByIdAsync(appointmentId);
            if (appointment is null)
                return ServiceResult.Failure("Appointment not found.");

            bool isOwner = appointment.Patient.UserId == requestingUserId
                        || appointment.Doctor.UserId  == requestingUserId;
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

        public async Task<ServiceResult> RescheduleAsync(int appointmentId, DateTime newSlot, string requestingUserId)
        {
            var appointment = await GetByIdAsync(appointmentId);
            if (appointment is null)
                return ServiceResult.Failure("Appointment not found.");

            bool isOwner = appointment.Patient.UserId == requestingUserId
                        || appointment.Doctor.UserId  == requestingUserId;
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

            var isTaken = await _context.Appointments
                .AnyAsync(a => a.DoctorId == appointment.DoctorId
                            && a.AppointmentDate == newSlot
                            && a.Status != Status.Cancelled
                            && a.Id != appointmentId);

            if (isTaken)
                return ServiceResult.Failure("This slot is already booked. Please choose another.");

            appointment.AppointmentDate = newSlot;
            appointment.Status          = Status.Pending;
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }
    }
}
