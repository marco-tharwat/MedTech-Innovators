using MediCare.Data.Models;
using MediCare.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.Web.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IMedicalRecordService _medicalRecordService;

        public PrescriptionController(IPrescriptionService prescriptionService, IMedicalRecordService medicalRecordService)
        {
            _prescriptionService = prescriptionService;
            _medicalRecordService = medicalRecordService;
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
        [HttpGet]
        [Authorize(Roles = "Doctor,Patient,Admin")]
        public async Task<ActionResult> PrintPrescription(int medicalRecordId)
        {
            if (medicalRecordId <= 0) return BadRequest("Invalid data");

            var record = await _medicalRecordService.GetRecordDetailsAsync(medicalRecordId);
            if (record == null) return NotFound("Record not found");

            var recordView = new PrescriptionViewModel
            {
                DoctorName = record.Doctor.User.FullName,
                Date = record.CreatedAt,
                PatientName = record.Patient.User.FullName,
                Age = record.Patient.Age,
                Specialization = record.Doctor.Specialization.Name,
                Diagnosis = record.Diagnosis,
                Medications = record.Medications.Select(m => new MedicationViewModel(
                    m.MedicationName,
                    m.Dosage,
                    m.Frequency,
                    m.Duration,
                    m.Instructions
                )).ToList()
            };

            return View(recordView);
        }
        [HttpGet]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult> GetPrescriptionHistory()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("No User logged in");
            var patientRecord = await _medicalRecordService.GetPatientMedicalRecordsAsync(id);

            var viewModel = new PrescriptionHistoryViewModel(
                patientRecord.Select(r => new PrescriptionHistoryItemViewModel(
                    r.Id,
                    r.CreatedAt,
                    r.Doctor.User.FullName,
                    r.Diagnosis,
                    r.Medications.Count
                )).ToList()
            );
            return View(viewModel);
        }
    }
}
