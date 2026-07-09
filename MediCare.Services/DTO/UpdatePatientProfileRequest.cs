using System.ComponentModel.DataAnnotations;
using MediCare.Services.Validation;

namespace MediCare.Services.DTO;
    public class UpdatePatientProfileRequest
    {
        [Required]
        public string Name { get; set; } = null!;

        [DataType(DataType.Date)]
        [MaxAge(100, ErrorMessage = "Birth date cannot be more than 100 years ago.")]
        public DateTime BirthDate { get; set; }

        public string? BloodType { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Allergies { get; set; }
    }
