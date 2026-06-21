using MediCare.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services
{
    public class WorkingHoursService
    {
        private readonly MedContext _context;

        // Fixed slot duration for every doctor
        private const int SlotDurationMinutes = 30;

        public WorkingHoursService(MedContext context)
        {
            _context = context;
        }

        // ---- Doctor's weekly schedule (CRUD) ----

        public async Task<List<WorkingHours>> GetByDoctorAsync(int doctorId)
        {
            return await _context.WorkingHours
                .Where(w => w.DoctorId == doctorId)
                .OrderBy(w => w.DayOfWeek)
                .ThenBy(w => w.StartTime)
                .ToListAsync();
        }

        public async Task<(bool Success, string? Error, WorkingHours? Result)> AddAsync(WorkingHours workingHours)
        {
            if (workingHours.StartTime >= workingHours.EndTime)
                return (false, "Start time must be before end time.", null);

            _context.WorkingHours.Add(workingHours);
            await _context.SaveChangesAsync();

            return (true, null, workingHours);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(WorkingHours workingHours)
        {
            var existing = await _context.WorkingHours.FindAsync(workingHours.Id);
            if (existing == null)
                return (false, "Working hours entry not found.");

            if (workingHours.StartTime >= workingHours.EndTime)
                return (false, "Start time must be before end time.");

            existing.DayOfWeek = workingHours.DayOfWeek;
            existing.StartTime = workingHours.StartTime;
            existing.EndTime = workingHours.EndTime;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int workingHoursId)
        {
            var existing = await _context.WorkingHours.FindAsync(workingHoursId);
            if (existing == null)
                return (false, "Working hours entry not found.");

            _context.WorkingHours.Remove(existing);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ---- Slot generation ----
        // Returns every available slot for a given doctor on a given date,
        // excluding slots that are already booked or already in the past.
        public async Task<List<DateTime>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            var workingHoursForDay = await _context.WorkingHours
                .Where(w => w.DoctorId == doctorId && w.DayOfWeek == dayOfWeek)
                .ToListAsync();

            if (!workingHoursForDay.Any())
                return new List<DateTime>(); // doctor doesn't work this day

            var bookedTimes = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                    && a.AppointmentDate.Date == date.Date
                    && a.Status != Status.Cancelled)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            var slots = new List<DateTime>();

            foreach (var wh in workingHoursForDay)
            {
                var slotStart = date.Date + wh.StartTime;
                var rangeEnd = date.Date + wh.EndTime;

                while (slotStart.AddMinutes(SlotDurationMinutes) <= rangeEnd)
                {
                    bool isBooked = bookedTimes.Contains(slotStart);
                    bool isPast = slotStart <= DateTime.Now;

                    if (!isBooked && !isPast)
                        slots.Add(slotStart);

                    slotStart = slotStart.AddMinutes(SlotDurationMinutes);
                }
            }

            return slots.OrderBy(s => s).ToList();
        }
    }
}
