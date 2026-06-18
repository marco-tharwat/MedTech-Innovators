using System.ComponentModel.DataAnnotations;

namespace MediCare.Data.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Diagnosis { get; set; } = null!;

        public string? Symptoms { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? Notes { get; set; }

        public virtual ICollection<MedicalDocument> MedicalDocuments { get; set; } = new List<MedicalDocument>();
        public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
    }
}