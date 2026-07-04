using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.DTO;
    public class UpdatePatientProfileRequest
    {
        [Required]
        public string Name { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public string? BloodType { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Allergies { get; set; }
    }
