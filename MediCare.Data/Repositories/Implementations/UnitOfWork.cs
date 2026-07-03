using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediCare.Data.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MedContext _context;
        private IDbContextTransaction? _transaction;

        public IDoctorRepository Doctors { get; }
        public IPatientRepository Patients { get; }
        public IAppointmentRepository Appointments { get; }
        public IMedicalRecordRepository MedicalRecords { get; }

        public UnitOfWork
            (MedContext context,
             IDoctorRepository doctorRepository,
             IPatientRepository patientRepository,
             IAppointmentRepository appointmentRepository,
             IMedicalRecordRepository medicalRecordRepository
            )
        {
            _context = context;
            Doctors = doctorRepository;
            Patients = patientRepository;
            Appointments = appointmentRepository;
            MedicalRecords = medicalRecordRepository;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            return new Repository<T>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}