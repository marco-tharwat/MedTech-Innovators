namespace MediCare.Services.DTO;
    public class UpdateDoctorRequest
    {
        public string id { get; set; } = null!;
        public decimal ConsultationFee { get; set; }
        public string Bio { get; set; } = null!;
        public string ImageURL { get; set; } = null!;
        public string Location { get; set; } = null!;
        public bool IsApproved { get; set; }
        public int SpecializationId { get; set; }
    }