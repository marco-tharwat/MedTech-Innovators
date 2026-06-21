namespace MediCare.Web.ViewModels
{
    // Used for both booking a brand new appointment and rescheduling an existing one.
    // AppointmentId is null when booking a new appointment, and set when rescheduling.
    public class BookAppointmentViewModel
    {
        public int? AppointmentId { get; set; }

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string SpecializationName { get; set; } = null!;

        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public DateTime? SelectedSlot { get; set; }
        public string? Notes { get; set; }

        public List<DateTime> AvailableSlots { get; set; } = new();
    }
}
