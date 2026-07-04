using MediCare.Data.Models;
namespace MediCare.Services.DTO;
    public class DoctorSearchDTO
    {
        public string? SearchTerm { get; set; }
        public int? SpecializationId { get; set; }
        public string? Location { get; set; }

        public List<Doctor> Doctors { get; set; } = new();
    }
