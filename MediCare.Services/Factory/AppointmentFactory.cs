using MediCare.Data.Models;
using MediCare.Data.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Factory
{
    public class AppointmentFactory
    {
        private readonly MedContext _context;

        public AppointmentFactory(MedContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateAsync(int patientId, int doctorId, DateTime slot, string? notes)
        {
   
            bool hasPreviousVisit = await _context.Appointments
                .AnyAsync(a => a.PatientId == patientId
                            && a.DoctorId == doctorId
                            && a.Status == Status.Completed);
            var type = hasPreviousVisit ? AppointmentType.FollowUp : AppointmentType.New;
            return new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = slot,
                Notes = notes,
                Status = Status.Pending,
                Type = type 
            };
        }
    }
}