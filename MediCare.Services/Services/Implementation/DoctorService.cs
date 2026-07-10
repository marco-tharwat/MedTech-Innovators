using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Services.Implementation
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // list/search doctors, only publicly showing doctors approved by admin
        public async Task<IEnumerable<Doctor>> SearchAsync(string? searchTerm, int? specializationId, string? location)
        {
            // Doctor has to be Approved in order to show on the system

            // var query = _unitOfWork.Doctors.Query()
            //     .Include(d => d.User)
            //     .Include(d => d.Specialization)
            //     .Where(d => d.IsApproved);

            // I temporarily disabled that feature
            IQueryable<Doctor> query = _unitOfWork.Doctors.Query()
                .Include(d => d.User)
                .Include(d => d.Specialization);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(d => d.User.FullName.ToLower().Contains(term)
                                       || d.Specialization.Name.ToLower().Contains(term));
            }

            if (specializationId.HasValue && specializationId.Value > 0)
            {
                query = query.Where(d => d.SpecializationId == specializationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var loc = location.Trim().ToLower();
                query = query.Where(d => d.Location != null && d.Location.ToLower().Contains(loc));
            }

            return await query.ToListAsync();
        }

        public async Task<Doctor?> GetDetailsAsync(int id)
        {
            // return await _unitOfWork.Doctors.Query()
            //     .Include(d => d.User)
            //     .Include(d => d.Specialization)
            //     .Include(d => d.WorkingHours)
            //     .FirstOrDefaultAsync(d => d.Id == id && d.IsApproved);
            //          there is a bug here                   ^^^^^^^^^^  while there is no doctors approved (and i can not approve a doctor yet => bio & image bug)
            //          the return value is null, so the NotFound view is luanched insted of Details
            //          i believe we should show details of doctor whatever its approval status is

            return await _unitOfWork.Doctors.Query()
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Include(d => d.WorkingHours)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
