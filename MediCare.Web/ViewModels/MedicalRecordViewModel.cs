using System.ComponentModel.DataAnnotations;

namespace MediCare.Web.ViewModels
{
    public class MedicalRecordViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        public string? Symptoms { get; set; }

        public string? TreatmentPlan { get; set; }

        public string? Notes { get; set; }

        public IEnumerable<PatientDropdownItem> Patients { get; set; }
            = new List<PatientDropdownItem>();
    }
}