using MediCare.Services.DTO;
using MediCare.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.Web.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<ActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetProfileByUserIdAsync(userId!);
            if (patient is null) return NotFound();

            return View(patient);
        }

        [HttpGet]
        public async Task<ActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetProfileByUserIdAsync(userId!);
            if (patient is null) return NotFound();

            var model = new UpdatePatientProfileRequest
            {
                Name = patient.User?.FullName ?? string.Empty,
                BirthDate = patient.BirthDate,
                BloodType = patient.BloodType,
                EmergencyContact = patient.EmergencyContact,
                Allergies = patient.Allergies
            };

            return View("Edit", model);
        }

        // POST: PatientController/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProfile(UpdatePatientProfileRequest request)
        {
            if (!ModelState.IsValid) return View("Edit", request);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _patientService.UpdateProfileAsync(userId!, request);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage!);
                return View("Edit", request);
            }

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        // GET: PatientController
        public ActionResult Index()
        {
            return RedirectToAction("Profile"); // Redirect to Patient Profile since Index page is not required
            // return View();
        }

        // GET: PatientController/Details/5
        public ActionResult Details(int id)
        {
            return View(); // view not implemented yet
        }

        // GET: PatientController/Create
        public ActionResult Create()
        {
            return View(); // view not implemented yet
        }

        // POST: PatientController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PatientController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PatientController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PatientController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PatientController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
