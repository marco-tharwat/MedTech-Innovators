using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; }

        public Gender Gender { get; set; }

        public Doctor? DoctorProfile { get; set; }
        public Patient? PatientProfile { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
