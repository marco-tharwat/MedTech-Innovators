using MediCare.Data.Models;
namespace MediCare.Services.DTO;
    public class AppointmentsDTO
    {
        public List<Appointment> Appointments { get; set; } = new();
        public int TotalCount { get; set; }
        public int NumOfPages { get; set; }
        public int CurrentPage { get; set; }
        public string? Status { get; set; }
        public string? Date { get; set; }
        public string? Order { get; set; }
    }