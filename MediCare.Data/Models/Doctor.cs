using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MediCare.Data.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsultationFee { get; set; }

        public string? Bio { get; set; }
        public string? ImageURL { get; set; }
        public string? Location { get; set; }

        public bool IsApproved { get; set; }

        // Foreign Keys & Navigations
        public int SpecializationId { get; set; }
        public Specialization Specialization { get; set; } = null!;

        public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    }
}

