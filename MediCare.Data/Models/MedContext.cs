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

            // Seed the fixed catalogue of specializations with stable IDs. EF Core turns this
            // into InsertData statements in the next generated migration. IDs are fixed so
            // Doctor.SpecializationId foreign keys stay valid across environments.
            builder.Entity<Specialization>().HasData(
                new Specialization { Id = 1, Name = "Cardiology" },
                new Specialization { Id = 2, Name = "Dermatology" },
                new Specialization { Id = 3, Name = "Emergency Medicine" },
                new Specialization { Id = 4, Name = "Endocrinology" },
                new Specialization { Id = 5, Name = "Family Medicine" },
                new Specialization { Id = 6, Name = "Gastroenterology" },
                new Specialization { Id = 7, Name = "General Surgery" },
                new Specialization { Id = 8, Name = "Gynecology" },
                new Specialization { Id = 9, Name = "Internal Medicine" },
                new Specialization { Id = 10, Name = "Neurology" },
                new Specialization { Id = 11, Name = "Neurosurgery" },
                new Specialization { Id = 12, Name = "Obstetrics" },
                new Specialization { Id = 13, Name = "Oncology" },
                new Specialization { Id = 14, Name = "Ophthalmology" },
                new Specialization { Id = 15, Name = "Orthopedic Surgery" },
                new Specialization { Id = 16, Name = "Otolaryngology (ENT)" },
                new Specialization { Id = 17, Name = "Pediatrics" },
                new Specialization { Id = 18, Name = "Psychiatry" },
                new Specialization { Id = 19, Name = "Pulmonology" },
                new Specialization { Id = 20, Name = "Radiology" },
                new Specialization { Id = 21, Name = "Rheumatology" },
                new Specialization { Id = 22, Name = "Urology" },
                new Specialization { Id = 23, Name = "Nephrology" },
                new Specialization { Id = 24, Name = "Anesthesiology" },
                new Specialization { Id = 25, Name = "Pathology" }
            );
        }

    }
}
