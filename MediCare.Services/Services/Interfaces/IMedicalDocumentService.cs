using MediCare.Data.Models;

namespace MediCare.Services.Services.Interfaces
{
    public interface IMedicalDocumentService
    {
        Task<bool> AddDocumentAsync(MedicalDocument doc);
        Task<bool> RemoveDocumentAsync(int documentId);
        Task<MedicalDocument?> GetDocumentByIdAsync(int documentId);
    }
}
