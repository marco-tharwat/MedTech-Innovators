using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MediCare.Data.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; } = null!;
        public Gender Gender { get; set; }
        public Doctor? DoctorProfile { get; set; }
        public Patient? PatientProfile { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
