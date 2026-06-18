using System.ComponentModel.DataAnnotations;

namespace MediCare.Data.Models
{
    public class MedicalDocument
    {
        [Key]
        public int Id { get; set; }

        public int MedicalRecordId { get; set; }
        public MedicalRecord MedicalRecord { get; set; } = null!;

        [Required]
        public string FilePath { get; set; } = null!;

        [Required, MaxLength(50)]
        public string FileType { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}