using System.ComponentModel.DataAnnotations;

namespace MediCare.Data.Models
{
    public class WorkingHours
    {
        [Key]
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }
}