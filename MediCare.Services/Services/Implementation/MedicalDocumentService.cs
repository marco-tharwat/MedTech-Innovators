using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Services.Interfaces;

namespace MediCare.Services.Services.Implementation
{
    public class MedicalDocumentService : IMedicalDocumentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IRepository<MedicalDocument> _repo;
        public MedicalDocumentService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            _repo = unitOfWork.Repository<MedicalDocument>();
        }
        public async Task<bool> AddDocumentAsync(MedicalDocument doc)
        {
            if (doc == null) return false;

            var exist = await unitOfWork.MedicalRecords.ExistsAsync(m => m.Id == doc.MedicalRecordId);
            if (!exist) return false;

            try
            {
                await _repo.AddAsync(doc);
                return await unitOfWork.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<MedicalDocument?> GetDocumentByIdAsync(int documentId)
        {
            if (documentId <= 0) return null;

            var doc = await _repo.GetByIdAsync(documentId);

            return doc;
        }

        public async Task<bool> RemoveDocumentAsync(int documentId)
        {
            if (documentId <= 0) return false;

            var doc = await GetDocumentByIdAsync(documentId);
            if (doc == null) return false;

            try
            {
                _repo.Remove(doc);
                return await unitOfWork.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {

                return false;
            }
        }

    }
}
