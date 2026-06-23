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

        public UnitOfWork(MedContext context)
        {
            _context = context;
            Doctors = new DoctorRepository(_context);
            Patients = new PatientRepository(_context);
            Appointments = new AppointmentRepository(_context);
            MedicalRecords = new MedicalRecordRepository(_context);
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