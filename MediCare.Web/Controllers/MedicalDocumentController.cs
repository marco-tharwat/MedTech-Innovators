using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.Web.Controllers
{
    public class MedicalDocumentController : Controller
    {
        private readonly HashSet<string> validExtensions = new HashSet<string>
            {
                ".pdf",
                ".png",
                ".jpeg",
                ".jpg"
            };
        private readonly IMedicalDocumentService _service;
        private readonly IWebHostEnvironment _webHost;
        private readonly IDoctorService _doctorService;

        public MedicalDocumentController(IMedicalDocumentService service, IUnitOfWork unitOfWork, IWebHostEnvironment webHost, IDoctorService doctorService)
        {
            _service = service;
            _webHost = webHost;
            _doctorService = doctorService;
        }
        [HttpGet]
        [Authorize(Roles = "Doctor, Admin")]
        public IActionResult AddDocument(int medicalRecordId)
        {
            return View(new MedicalDocumentUploadViewModel
            {
                MedicalRecordId = medicalRecordId
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<ActionResult> AddDocument(MedicalDocumentUploadViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "An unexpected error occurred while identifying the doctor...");
                return View(viewModel);
            }

            var exists = await _doctorService.ExistAsync(userId);
            if (!exists)
            {
                ModelState.AddModelError("", "An unexpected error occurred while identifying the doctor...");
                return View(viewModel);
            }
            #region File validation
            if (viewModel.Document == null || viewModel.Document.Length == 0)
            {
                ModelState.AddModelError("", "Please choose a file");
                return View(viewModel);
            }

            var extension = Path.GetExtension(viewModel.Document.FileName).ToLower();
            if (!validExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Unsupported file type");
                return View(viewModel);
            }
            if (viewModel.Document.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size can't exceed 10 MB");
                return View(viewModel);
            }
            #endregion
            var fileName = Guid.NewGuid() + extension;
            var uploadPath = Path.Combine(_webHost.WebRootPath, "Uploads", "MedicalDocuments");
            Directory.CreateDirectory(uploadPath);
            var fullPath = Path.Combine(uploadPath, fileName);
            using Stream stream = new FileStream(fullPath, FileMode.Create);
            await viewModel.Document.CopyToAsync(stream);

            var document = new MedicalDocument
            {
                MedicalRecordId = viewModel.MedicalRecordId,
                FileSize = viewModel.Document.Length,
                OriginalFileName = Path.GetFileName(viewModel.Document.FileName),
                FilePath = Path.Combine("Uploads", "MedicalDocuments", fileName),
                FileType = Path.GetExtension(viewModel.Document.FileName).Substring(1)
            };
            var success = await _service.AddDocumentAsync(document);
            if (!success)
            {
                System.IO.File.Delete(fullPath);
                ModelState.AddModelError("", "An unexpected error occured while saving the file");
                return View(viewModel);
            }
            return RedirectToAction("GetRecordDetails", "MedicalRecord", new { medicalRecordId = viewModel.MedicalRecordId });
        }
        [HttpGet]
        [Authorize(Roles = "Doctor, Admin, Patient")]
        public async Task<ActionResult> DownloadDocument(int documentId)
        {
            if (documentId <= 0) return BadRequest("Invalid data");

            var doc = await _service.GetDocumentByIdAsync(documentId);
            if (doc == null) return NotFound("There is no such file");

            var fullPath = Path.Combine(_webHost.WebRootPath, doc.FilePath);
            var exists = System.IO.File.Exists(fullPath);
            if (!exists) return NotFound("There is no such file");

            return PhysicalFile(
                fullPath,
                "application/octet-stream",
                doc.OriginalFileName);
        }
        [HttpPost]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<ActionResult> RemoveDocument(int documentId)
        {
            if (documentId <= 0) return BadRequest("Invalid data");

            var doc = await _service.GetDocumentByIdAsync(documentId);
            if (doc == null) return NotFound("No such file found");

            var fullPath = Path.Combine(_webHost.WebRootPath, doc.FilePath);
            var medicalRecordId = doc.MedicalRecordId;

            var success = await _service.RemoveDocumentAsync(documentId);

            if (success)
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                return RedirectToAction("GetRecordDetails", "MedicalRecord", new { medicalRecordId = medicalRecordId });
            }
            else
            {
                return BadRequest("an error occured, the file wasn't deleted");
            }
        }
    }
}
