using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        readonly IAdminServices _adminServices;
        public AdminController(IAdminServices adminServices)
        {
            _adminServices = adminServices;
        }

        [HttpGet]
        public async Task<ActionResult> Dashboard()
        {
            SummaryOfDataForAdmin model = await _adminServices.SetSummaryOfDataForAdmin();
            return View("Dashboard",model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageDoctors()
        {
            var data= await _adminServices.FetchDoctorsData();
            return View("ManageDoctors", data);
        }

        [HttpGet]
        public async Task<ActionResult> ManagePatients()
        {
            var data = await _adminServices.FetchPatientsData();
            return View("ManagePatients", data);
        }

        [HttpGet]
        public async Task<ActionResult> UpdatePatients(int id)
        {
            var data=await _adminServices.FetchPatientData(id);
            var model = new UpdatePatientsRequest
            {
                id = id,
                Name=data.User.FullName,
                BirthDate=data.BirthDate,
                BloodType=data.BloodType,
                EmergencyContact=data.EmergencyContact,
                Allergies=data.Allergies,
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePatients(UpdatePatientsRequest request)
        {
            var flag=await _adminServices.UpdatePatients(request);
            if (flag) return RedirectToAction("Dashboard");
            else return RedirectToAction("UpdatePatients");
        }

        [HttpGet]
        public async Task<IActionResult> AllAppointments(string status,string order,string date,int PageNum)
        {
            var model = await _adminServices.SetTheAppointments(status,order,date,PageNum);
            return View("AllAppointments", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        [HttpGet]
        public async Task<ActionResult> AllRegistered()
        {
            var dat = await _adminServices.SetRegisteredAccountsData();
            return View(dat);
        }
    }
}
