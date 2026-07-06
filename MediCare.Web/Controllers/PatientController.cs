using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.Web.Controllers
{
    // Note: this controller was previously restricted at the class level to
    // [Authorize(Roles = "Patient")]. Now that it also hosts Admin-only patient
    // management (Details/Create/Edit/Delete), that restriction has moved to the
    // individual self-service actions (Profile/EditProfile) below. It could not stay
    // at the class level alongside a method-level [Authorize(Roles = "Admin")], since
    // stacked [Authorize] attributes are combined with AND semantics (a caller would
    // need to be in both roles at once), not OR.
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IAdminServices _adminServices;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(
            IPatientService patientService,
            IAdminServices adminServices,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _patientService = patientService;
            _adminServices = adminServices;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: PatientController/Profile — the logged-in patient's own profile
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<ActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetProfileByUserIdAsync(userId!);
            if (patient is null) return NotFound();

            return View(patient);
        }

        // GET: PatientController/EditProfile
        // Named "EditProfile" rather than "Edit" to avoid colliding/being ambiguous
        // with the admin-facing Edit(int id) action below, which binds a different
        // model (UpdatePatientsRequest) and serves a different purpose (an admin
        // editing any patient, vs. a patient editing their own profile).
        [Authorize(Roles = "Patient")]
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
        [Authorize(Roles = "Patient")]
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
            return View();
        }

        // GET: PatientController/Details/5 — admin viewing any patient's record
        // Reuses AdminServices.FetchPatientData, the same lookup AdminController's
        // UpdatePatients action already uses.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var patient = await _adminServices.FetchPatientData(id);
            if (patient is null) return NotFound();

            return View(patient);
        }

        // GET: PatientController/Create
        // Mirrors AccountController.Register's Patient branch: an ApplicationUser +
        // Patient profile created together by an admin. Extra profile fields
        // (blood type, emergency contact, allergies) are filled in afterwards via Edit,
        // exactly like public registration leaves them for later.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: PatientController/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterRequest request)
        {
            request.Role = "Patient";

            if (!ModelState.IsValid) return View(request);

            var user = new ApplicationUser
            {
                FullName = request.Name,
                Gender = request.Gender,
                Email = request.Email,
                UserName = request.UserName,
                Created = DateTime.Today
            };

            var res = await _userManager.CreateAsync(user, request.Password);

            if (res.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Patient");

                var patient = new Patient
                {
                    User = user,
                    UserId = user.Id
                };
                await _unitOfWork.Patients.AddAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in res.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(request);
        }

        // GET: PatientController/Edit/5 — admin editing any patient
        // Reuses the same UpdatePatientsRequest DTO and data AdminController's
        // UpdatePatients action already builds; renders "EditPatient" (rather than
        // "Edit") to avoid clashing with the self-service Edit.cshtml above.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var data = await _adminServices.FetchPatientData(id);
            if (data is null) return NotFound();

            var model = new UpdatePatientsRequest
            {
                id = id,
                Name = data.User.FullName,
                BirthDate = data.BirthDate,
                BloodType = data.BloodType,
                EmergencyContact = data.EmergencyContact,
                Allergies = data.Allergies
            };

            return View("EditPatient", model);
        }

        // POST: PatientController/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, UpdatePatientsRequest request)
        {
            if (!ModelState.IsValid) return View("EditPatient", request);

            var flag = await _adminServices.UpdatePatients(request);
            if (!flag)
            {
                ModelState.AddModelError("", "Patient could not be updated.");
                return View("EditPatient", request);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: PatientController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            var patient = await _adminServices.FetchPatientData(id);
            if (patient is null) return NotFound();

            return View(patient);
        }

        // POST: PatientController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _adminServices.DeletePatient(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

