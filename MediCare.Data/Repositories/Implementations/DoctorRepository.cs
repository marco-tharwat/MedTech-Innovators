using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories.Implementations
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(MedContext context) : base(context) { }

        
        public async Task<Doctor?> GetDoctorWithSpecializationAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == doctorId);
        }

        // specific specialization
        public async Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(int specializationId)
        {
            return await _context.Doctors
                .Where(d => d.SpecializationId == specializationId)
                .Include(d => d.Specialization)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorWithWorkingHoursAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.WorkingHours)
                .FirstOrDefaultAsync(d => d.Id == doctorId);
        }

        // doctor with his appointments
        public async Task<Doctor?> GetDoctorWithAppointmentsAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.Id == doctorId);
        }
    }
}