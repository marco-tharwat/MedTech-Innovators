using MediCare.Data.Models;
using MediCare.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }
        [HttpGet]
        public IActionResult AddMedication(int medicalRecordId)
        {
            if (medicalRecordId <= 0) return BadRequest("Invalid medical recoed id");

            var vm = new BulkPrescriptionViewModel(
                MedicalRecordId: medicalRecordId,
                Medications: new List<MedicationViewModel> { new("", "", "", "", "") });
            return View("AddMedication", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddMedication(BulkPrescriptionViewModel viewModel)
        {
            if (!ModelState.IsValid) return View("AddMedication", viewModel);
            if (viewModel.Medications == null || !viewModel.Medications.Any())
            {
                ModelState.AddModelError("", "Add at least one medication before saving");
                return View("AddMedication", viewModel);
            }

            var medicationList = viewModel.Medications.Select(item => new Medication
            {
                MedicalRecordId = viewModel.MedicalRecordId,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                Duration = item.Duration,
                MedicationName = item.MedicationName,
                Instructions = item.Instructions
            }).ToList();

            var success = await _prescriptionService.AddMedicationsToRecordAsync(viewModel.MedicalRecordId, medicationList);

            if (!success)
            {
                ModelState.AddModelError("", "An error happened while adding the medications to the record");
                return View("AddMedication", viewModel);
            }

            return RedirectToAction("GetRecordDetails", "MedicalRecord", new { medicalRecordId = viewModel.MedicalRecordId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveMedication(int medicalRecordId, int medicationId)
        {
            if (medicalRecordId <= 0 || medicationId <= 0) return BadRequest("Invalid Medical Record or Medication Id");

            var success = await _prescriptionService.RemoveMedicationAsync(medicationId);

            if (!success)
            {
                TempData["ErrorMessage"] = "Could not delete medication from the record.";
            }

            return RedirectToAction("GetRecordDetails", "MedicalRecord", new { medicalRecordId = medicalRecordId });
        }
    }
}
