using MediCare.Data.Models;
using MediCare.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace MediCare.Services.Implementations
{
    public class WorkingHoursService : IWorkingHoursService
    {
        private readonly MedContext _context;
        private const int SlotDurationMinutes = 30;

        public WorkingHoursService(MedContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WorkingHours>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.WorkingHours
                .Where(w => w.DoctorId == doctorId)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();
        }

        public async Task<WorkingHours?> GetByIdAsync(int id)
        {
            return await _context.WorkingHours.FindAsync(id);
        }

        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            // Get the doctor's working hours for the requested day
            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == doctorId && w.DayOfWeek == date.DayOfWeek);

            if (workingHours is null)
                return Enumerable.Empty<DateTime>();

            // Generate all 30-minute slots within the working window
            var slots = new List<DateTime>();
            var current = date.Date.Add(workingHours.StartTime);
            var end = date.Date.Add(workingHours.EndTime);

            while (current.AddMinutes(SlotDurationMinutes) <= end)
            {
                slots.Add(current);
                current = current.AddMinutes(SlotDurationMinutes);
            }

            // Get already-booked slots for that day
            var booked = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == date.Date
                         && a.Status != Status.Cancelled)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            // Return only future, unbooked slots
            return slots
                .Where(s => s > DateTime.Now && !booked.Contains(s))
                .ToList();
        }

        public async Task<ServiceResult> AddAsync(WorkingHours workingHours)
        {
            // A doctor cannot have two entries for the same day
            var exists = await _context.WorkingHours
                .AnyAsync(w => w.DoctorId == workingHours.DoctorId
                            && w.DayOfWeek == workingHours.DayOfWeek);

            if (exists)
                return ServiceResult.Failure("Working hours for this day already exist for this doctor.");

            if (workingHours.StartTime >= workingHours.EndTime)
                return ServiceResult.Failure("Start time must be before end time.");

            await _context.WorkingHours.AddAsync(workingHours);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> UpdateAsync(WorkingHours workingHours)
        {
            if (workingHours.StartTime >= workingHours.EndTime)
                return ServiceResult.Failure("Start time must be before end time.");

            _context.WorkingHours.Update(workingHours);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var workingHours = await _context.WorkingHours.FindAsync(id);
            if (workingHours is null)
                return ServiceResult.Failure("Working hours entry not found.");

            _context.WorkingHours.Remove(workingHours);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        Task<ServiceResult> IWorkingHoursService.AddAsync(WorkingHours workingHours)
        {
            throw new NotImplementedException();
        }

        Task<ServiceResult> IWorkingHoursService.UpdateAsync(WorkingHours workingHours)
        {
            throw new NotImplementedException();
        }

        Task<ServiceResult> IWorkingHoursService.DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
