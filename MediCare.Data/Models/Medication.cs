using System.ComponentModel.DataAnnotations;

namespace MediCare.Data.Models
{
    public class Medication
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string MedicationName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Dosage { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Frequency { get; set; } = null!;
        public string? Duration { get; set; }
        public string? Instructions { get; set; }


        public int MedicalRecordId { get; set; }
        public MedicalRecord MedicalRecord { get; set; } = null!;
    }
}