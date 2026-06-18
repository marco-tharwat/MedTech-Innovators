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

    }
}
