using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace MediCare.Data.Models
{
    public class MedContext:IdentityDbContext<ApplicationUser>
    {
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;

        
        public DbSet<Specialization> Specializations { get; set; } = null!;
        public DbSet<WorkingHours> WorkingHours { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;

        
        public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;
        public DbSet<MedicalDocument> MedicalDocuments { get; set; } = null!;
        public DbSet<Medication> Medications { get; set; } = null!;

        
        public DbSet<Notification> Notifications { get; set; } = null!;
        public MedContext(DbContextOptions<MedContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // if patient or doctor is deleted => appointment isn't deleted
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);


            // if patient or doctor is deleted => MedicalRecords isn't deleted
            builder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MedicalRecord>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.MedicalRecords)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
