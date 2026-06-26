using MediCare.Data.Models;

namespace MediCare.Services.DTO
{
    public class ReportsResponse
    {
        public Dictionary<string, List<Appointment>> appointmentsPerDoctor { get; set; } = new();
        public Dictionary<string, int> specializationsCount { get; set; } = new();
    }
}
