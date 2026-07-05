namespace MediCare.Web.ViewModels
{
    public record PrescriptionHistoryItemViewModel(
     int MedicalRecordId,
     DateTime Date,
     string DoctorName,
     string Diagnosis,
     int MedicationCount
 );
}
