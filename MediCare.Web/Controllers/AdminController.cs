using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Factorys;
using MediCare.Services.Services;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly IUnitOfWork _unitOfWork;
        readonly IAdminServices _adminServices;
        public AdminController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IAdminServices adminServices)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _adminServices = adminServices;
        }
        [HttpGet]
        [HttpGet] public IActionResult Dashboard() => View("Dashboard");
        [HttpGet]
        public async Task<IActionResult> ManageDoctors()
        {
            var data = await _unitOfWork.Doctors.GetAllAsync();
            return View("ManageDoctors", data);
        }
        [HttpGet]
        public async Task<IActionResult> ManagePatients()
        {
            var data = await _unitOfWork.Patients.GetAllAsync();
            return View("ManagePatients", data);
        }
        [HttpGet]
        public async Task<IActionResult> AllAppointments()
        {
            var data = await _unitOfWork.Appointments.GetAllAsync();
            return View("AllAppointments", data);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAppointmentStatus(string status, int id)
        {
            var flag = await _adminServices.UpdateAppointmentStatus(status, id);
            if (flag)
                return RedirectToAction("Dashboard", true);
            return RedirectToAction("Dashboard", false);
        }

        [HttpGet]
        public async Task<ActionResult> Specializations_()
        {
            var data = await _adminServices.GetSpecializationSetupAsync();
            return View(data);
        }

        [HttpGet]
        public async Task<ActionResult> Specialization(int id)
        {
            var res = await _adminServices.GetSpecialization(id);
            if (res is null) return RedirectToAction("Specializations_");
            return View(res);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDoctor(int id, int specID)
        {
            var flag = await _adminServices.DeleteDoctor(id);
            if (flag)
                return RedirectToAction("Specialization", new { specID });
            return RedirectToAction("Specialization", new { specID });
        }

        [HttpGet]
        public async Task<ActionResult> UpdateDoctor(int id, int specID)
        {
            var model = await _adminServices.mapFromDoctorToUpdateDoctorRequest(id);
            if (model is null) return RedirectToAction("Specialization", new { specID });
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateDoctor(UpdateDoctorRequest doctorRequest, int specID, int id)
        {
            if (!ModelState.IsValid) return View(doctorRequest);
            await _adminServices.UpdateValuesOfDoctor(doctorRequest, id);
            return RedirectToAction("Specialization", new { specID });
        }

        [HttpGet]
        public async Task<ActionResult> Reports()
        {
            var data = await _adminServices.GetReportsAsync();
            return View(data);
        }
    }
}
