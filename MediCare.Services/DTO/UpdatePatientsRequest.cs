namespace MediCare.Services.DTO;

    public class UpdatePatientsRequest
    {
        public int id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string? BloodType { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Allergies { get; set; }
    }