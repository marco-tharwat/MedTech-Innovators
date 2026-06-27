using System.ComponentModel.DataAnnotations;

namespace MediCare.Web.ViewModels
{
    // 1. The Main Wrapper (Passed directly to your Controller Action)
    public record BulkPrescriptionViewModel(
        [Required] int MedicalRecordId,
        List<MedicationViewModel> Medications
    );

    // 2. The Individual Row Structure (Used inside the List above)
    public record MedicationViewModel(
        [Required, MaxLength(150)] string MedicationName,
        [Required, MaxLength(100)] string Dosage,
        [Required, MaxLength(100)] string Frequency,
        string? Duration,
        string? Instructions
    );
}