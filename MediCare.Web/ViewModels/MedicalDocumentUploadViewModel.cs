namespace MediCare.Web.ViewModels
{
    public class MedicalDocumentUploadViewModel
    {
        public int MedicalRecordId { get; set; }
        public IFormFile Document { get; set; } = null!;
    }
}
