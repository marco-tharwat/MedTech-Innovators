using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;

namespace MediCare.Data.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MedContext _context;

        public IDoctorRepository Doctors { get; }
        public IPatientRepository Patients { get; }
        public IAppointmentRepository Appointments { get; }
        public IMedicalRecordRepository MedicalRecords { get; }

        public UnitOfWork(MedContext context)
        {
            _context = context;
            Doctors = new DoctorRepository(_context);
            Patients = new PatientRepository(_context);
            Appointments = new AppointmentRepository(_context);
            MedicalRecords = new MedicalRecordRepository(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}