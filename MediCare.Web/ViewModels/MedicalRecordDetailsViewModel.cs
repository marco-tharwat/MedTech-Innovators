namespace MediCare.Web.ViewModels
{
    public class MedicalRecordDetailsViewModel
    {
        // Patient Information
        public string PatientName { get; set; } = string.Empty;
        public int PatientAge { get; set; }
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyContact { get; set; }
        public int MedicalRecordId { get; set; }
        public int PatientId { get; set; }

        // Doctor Information
        public string DoctorName { get; set; } = string.Empty;
        public string? Specialization { get; set; }

        // Record Information
        public DateTime CreatedAt { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Symptoms { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? Notes { get; set; }

        // Related Data
        public List<MedicationDetailsViewModel> Medications { get; set; } = new();
        public List<MedicalDocumentViewModel> Documents { get; set; } = new();
    }
}