namespace MediCare.Web.ViewModels
{
    public class MedicalDocumentViewModel
    {
        public int Id { get; set; }

        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}