using MediCare.Data.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace MediCare.Data.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }


        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;



        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;


        [Required]
        public Status Status { get; set; } = Status.Pending; // Pending, Confirmed, Completed, Cancelled

        public string? Notes { get; set; }
        public AppointmentType Type { get; set; } = AppointmentType.New;
    }
}