using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCare.Web.Controllers
{
    // Note: this controller was previously restricted to [Authorize(Roles = "Doctor")],
    // which would block the doctor-listing/search feature it now hosts (meant to be
    // browsed by patients, per the "Find Doctor" link in _AdminLayout's Patient section,
    // and reasonably by anonymous visitors too). The restriction has been removed here;
    // it was never protecting any implemented logic below since Index/Details were stubs.
    // Create/Edit/Delete are doctor-management actions (same responsibility as
    // AdminController's UpdateDoctor/DeleteDoctor) and are protected individually below.
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminServices _adminServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(
            IDoctorService doctorService,
            IUnitOfWork unitOfWork,
            IAdminServices adminServices,
            UserManager<ApplicationUser> userManager)
        {
            _doctorService = doctorService;
            _unitOfWork = unitOfWork;
            _adminServices = adminServices;
            _userManager = userManager;
        }

        // GET: DoctorController/Index — the logged-in doctor's home dashboard.
        // Pure landing page (quick-action hub) linking only to actions the doctor
        // actually has: DoctorDailyList, ManageWorkingHours, CreateMedicalRecord,
        // Notifications and the public doctor Filter/search. No new business logic.
        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Filter(DoctorSearchDTO filter)
        {
            filter ??= new DoctorSearchDTO();

            var results = await _doctorService.SearchAsync(filter.SearchTerm, filter.SpecializationId, filter.Location);

            // Excpects a list of doctors that matches with SearchTerm, Specialization and Location inside results

            filter.Doctors = results.ToList(); // This list is saved her in "filter" and will be passed to Views/Doctor/Index 

            var specializations = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specializations, "Id", "Name", filter.SpecializationId);

            return View(filter);
        }

        // GET: DoctorController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var doctor = await _doctorService.GetDetailsAsync(id);
            if (doctor is null) return NotFound();

            return View(doctor);
        }

        // GET: DoctorController/Create
        // Mirrors AccountController.Register's Doctor branch: a Doctor account is an
        // ApplicationUser + a Doctor profile created together. Extra profile fields
        // (fee, bio, location) are populated afterwards via Edit, exactly like the
        // public registration flow leaves them for the admin to fill in via UpdateDoctor.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specs, "Id", "Name");
            return View();
        }

        // POST: DoctorController/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterRequest request)
        {
            request.Role = "Doctor";

            if (!ModelState.IsValid)
            {
                var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
                ViewBag.Specializations = new SelectList(specs, "Id", "Name", request.SpecializationId);
                return View(request);
            }

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
                await _userManager.AddToRoleAsync(user, "Doctor");

                var doctor = new Doctor
                {
                    User = user,
                    UserId = user.Id,
                    IsApproved = false,
                    SpecializationId = request.SpecializationId ?? 0
                };
                await _unitOfWork.Doctors.AddAsync(doctor);
                await _unitOfWork.SaveChangesAsync();

                return RedirectToAction("ManageDoctors", "Admin");
            }

            foreach (var error in res.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            var specializations = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specializations, "Id", "Name", request.SpecializationId);
            return View(request);
        }

        // GET: DoctorController/Edit/5
        // Reuses the existing AdminServices mapping/update logic (same one AdminController's
        // UpdateDoctor action uses), just exposed here without the specialization-browsing
        // redirect context.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var model = await _adminServices.mapFromDoctorToUpdateDoctorRequest(id);
            if (model is null) return NotFound();

            var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specs, "Id", "Name", model.SpecializationId);
            ViewBag.DoctorId = id; // UpdateDoctorRequest.id is actually the UserId string, not the Doctor's int Id

            return View(model);
        }

        // POST: DoctorController/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, UpdateDoctorRequest doctorRequest)
        {
            if (!ModelState.IsValid)
            {
                var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
                ViewBag.Specializations = new SelectList(specs, "Id", "Name", doctorRequest.SpecializationId);
                ViewBag.DoctorId = id;
                return View(doctorRequest);
            }

            await _adminServices.UpdateValuesOfDoctor(doctorRequest, id);

            // return RedirectToAction(nameof(Details), new { id });
            return RedirectToAction("ManageDoctors", "Admin");
        }

        // GET: DoctorController/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var doctor = await _adminServices.FetchDoctorData(id);
            if (doctor is null) return NotFound();

            return View(doctor);
        }

        // POST: DoctorController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _adminServices.DeleteDoctor(id);
            return RedirectToAction("ManageDoctors", "Admin");
        }
    }
}

