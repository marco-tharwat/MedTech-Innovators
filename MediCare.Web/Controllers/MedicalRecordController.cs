using MediCare.Data.Models;
using MediCare.Services.Interfaces;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.Web.Controllers
{
    [Authorize(Roles = "Doctor, Admin")]
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public MedicalRecordController(IMedicalRecordService medicalRecordService, IPatientService patientService, IDoctorService doctorService)
        {
            _medicalRecordService = medicalRecordService;
            _patientService = patientService;
            _doctorService = doctorService;
        }
        [HttpGet]
        public async Task<ActionResult> GetPatientHistory(int patientId)
        {
            if (patientId <= 0) return BadRequest("The patient isn't in the database");

            var medicalHistory = await _medicalRecordService.GetPatientHistoryAsync(patientId);

            return View("PatientHistory", medicalHistory);
        }
        [HttpGet]
        public async Task<ActionResult> GetRecordDetails(int medicalRecordId)
        {
            if (medicalRecordId <= 0) return BadRequest("The medical record isn't valid");

            var medicalRecord = await _medicalRecordService.GetRecordDetailsAsync(medicalRecordId);

            if (medicalRecord == null) return NotFound("The medical record does not exist");
            #region mapping
            var vm = new MedicalRecordDetailsViewModel
            {
                PatientName = medicalRecord.Patient.User.FullName,
                PatientAge = medicalRecord.Patient.Age,
                BloodType = medicalRecord.Patient.BloodType,
                Allergies = medicalRecord.Patient.Allergies,
                EmergencyContact = medicalRecord.Patient.EmergencyContact,
                MedicalRecordId = medicalRecord.Id,
                DoctorName = medicalRecord.Doctor.User.FullName,
                Specialization = medicalRecord.Doctor.Specialization?.Name,

                CreatedAt = medicalRecord.CreatedAt,
                Diagnosis = medicalRecord.Diagnosis,
                Symptoms = medicalRecord.Symptoms,
                TreatmentPlan = medicalRecord.TreatmentPlan,
                Notes = medicalRecord.Notes,

                Medications = medicalRecord.Medications
                    .Select(m => new MedicationDetailsViewModel
                    {
                        Id = m.Id,
                        MedicationName = m.MedicationName,
                        Dosage = m.Dosage,
                        Frequency = m.Frequency,
                        Duration = m.Duration,
                        Instructions = m.Instructions
                    })
                    .ToList(),

                Documents = medicalRecord.MedicalDocuments
                    .Select(d => new MedicalDocumentViewModel
                    {
                        Id = d.Id,
                        FilePath = d.FilePath,
                        FileType = d.FileType,
                        OriginalFileName = d.OriginalFileName,
                        UploadedAt = d.UploadedAt
                    })
                    .ToList()
            };
            #endregion

            return View("RecordDetails", vm);
        }
        [HttpGet]
        public async Task<IActionResult> CreateMedicalRecord()
        {
            var patients = await _patientService.GetPatientsWithUsersAsync();

            var vm = new MedicalRecordViewModel
            {
                Patients = patients.Select(p => new PatientDropdownItem
                {
                    Id = p.Id,
                    DisplayName = $"{p.User.FullName} ({p.Age} years)"
                })
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateMedicalRecord(MedicalRecordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "An unexpected error occurred while identifying the doctor...");
                    return View(viewModel);
                }

                var doctor = await _doctorService.GetByUserIdAsync(userId);
                if (doctor == null)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while identifying the doctor...");
                    var patientsWithUsers = await _patientService.GetPatientsWithUsersAsync();

                    viewModel.Patients = patientsWithUsers.Select(p => new PatientDropdownItem
                    {
                        Id = p.Id,
                        DisplayName = $"{p.User.FullName} ({p.Age} years)"
                    });

                    return View(viewModel);
                }

                var medicalRecord = new MedicalRecord
                {
                    PatientId = viewModel.PatientId,
                    DoctorId = doctor.Id,
                    Diagnosis = viewModel.Diagnosis,
                    Symptoms = viewModel.Symptoms,
                    TreatmentPlan = viewModel.TreatmentPlan,
                    Notes = viewModel.Notes
                };

                var success = await _medicalRecordService.CreateMedicalRecordAsync(medicalRecord);
                if (success) return RedirectToAction("GetRecordDetails", new { medicalRecordId = medicalRecord.Id });
            }
            ModelState.AddModelError("", "An unexpected error occurred while saving...");
            var patients = await _patientService.GetPatientsWithUsersAsync();

            viewModel.Patients = patients.Select(p => new PatientDropdownItem
            {
                Id = p.Id,
                DisplayName = $"{p.User.FullName} ({p.Age} years)"
            });

            return View(viewModel);
        }
    }
}
