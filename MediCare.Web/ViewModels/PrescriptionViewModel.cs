namespace MediCare.Web.ViewModels
{
    public class PrescriptionViewModel
    {
        public string PatientName { get; set; }

        public int Age { get; set; }

        public string DoctorName { get; set; }

        public string Specialization { get; set; }

        public DateTime Date { get; set; }

        public string Diagnosis { get; set; }

        public List<MedicationViewModel> Medications { get; set; }
    }
}
