using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCare.Web.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorController(IDoctorService doctorService, IUnitOfWork unitOfWork)
        {
            _doctorService = doctorService;
            _unitOfWork = unitOfWork;
        }

        // GET: DoctorController
        [HttpGet]
        public async Task<ActionResult> Index(DoctorSearchDTO filter)
        {
            filter ??= new DoctorSearchDTO();

            var results = await _doctorService.SearchAsync(filter.SearchTerm, filter.SpecializationId, filter.Location);
            filter.Doctors = results.ToList();

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
        public ActionResult Create()
        {
            return View();
        }

        // POST: DoctorController/Create
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

        // GET: DoctorController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DoctorController/Edit/5
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

        // GET: DoctorController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DoctorController/Delete/5
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
