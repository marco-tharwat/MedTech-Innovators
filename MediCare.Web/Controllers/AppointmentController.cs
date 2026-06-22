using MediCare.Data.Models;
using MediCare.Services;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Web.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly AppointmentService _appointmentService;
        private readonly WorkingHoursService _workingHoursService;
        private readonly MedContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(
            AppointmentService appointmentService,
            WorkingHoursService workingHoursService,
            MedContext context,
            UserManager<ApplicationUser> userManager)
        {
            _appointmentService = appointmentService;
            _workingHoursService = workingHoursService;
            _context = context;
            _userManager = userManager;
        }

        // ===================== BOOK =====================

        // GET: Appointment/Book?doctorId=5&date=2026-06-25
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Book(int doctorId, DateTime? date)
        {
            var model = await BuildBookViewModelAsync(doctorId, date?.Date ?? DateTime.Today);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // POST: Appointment/Book
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            if (model.SelectedSlot == null)
                ModelState.AddModelError(string.Empty, "Please choose an available time slot.");

            if (!ModelState.IsValid)
            {
                await RepopulateSlotsAsync(model);
                return View(model);
            }

            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var result = await _appointmentService.BookAsync(
                patient.Id, model.DoctorId, model.SelectedSlot!.Value, model.Notes);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                await RepopulateSlotsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Appointment booked successfully.";
            return RedirectToAction(nameof(PatientAppointments));
        }

        // ===================== CANCEL =====================

        // POST: Appointment/Cancel/5
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null || appointment.PatientId != patient.Id)
                return NotFound();

            var result = await _appointmentService.CancelAsync(id);

            TempData[result.Success ? "Success" : "Error"] =
                result.Success ? "Appointment cancelled." : result.Error;

            return RedirectToAction(nameof(PatientAppointments));
        }

        // ===================== RESCHEDULE =====================

        // GET: Appointment/Reschedule/5  (reuses the Book view with the same ViewModel)
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Reschedule(int id, DateTime? date)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null || appointment.PatientId != patient.Id)
                return NotFound();

            var model = await BuildBookViewModelAsync(appointment.DoctorId, date?.Date ?? DateTime.Today);
            if (model == null)
                return NotFound();

            model.AppointmentId = appointment.Id;

            return View("Book", model);
        }

        // POST: Appointment/Reschedule
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(BookAppointmentViewModel model)
        {
            if (model.AppointmentId == null)
                return BadRequest();

            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var appointment = await _appointmentService.GetByIdAsync(model.AppointmentId.Value);
            if (appointment == null || appointment.PatientId != patient.Id)
                return NotFound();

            if (model.SelectedSlot == null)
                ModelState.AddModelError(string.Empty, "Please choose an available time slot.");

            if (!ModelState.IsValid)
            {
                await RepopulateSlotsAsync(model);
                return View("Book", model);
            }

            var result = await _appointmentService.RescheduleAsync(model.AppointmentId.Value, model.SelectedSlot!.Value);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                await RepopulateSlotsAsync(model);
                return View("Book", model);
            }

            TempData["Success"] = "Appointment rescheduled successfully.";
            return RedirectToAction(nameof(PatientAppointments));
        }

        // ===================== LISTS =====================

        // GET: Appointment/PatientAppointments
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> PatientAppointments()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var appointments = await _appointmentService.GetByPatientAsync(patient.Id);
            return View(appointments);
        }

        // GET: Appointment/DoctorDailyList?date=2026-06-25
        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public async Task<IActionResult> DoctorDailyList(DateTime? date)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
                return Forbid();

            var selectedDate = date?.Date ?? DateTime.Today;

            var appointments = (await _appointmentService.GetByDoctorAsync(doctor.Id))
                .Where(a => a.AppointmentDate.Date == selectedDate)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            ViewBag.SelectedDate = selectedDate;

            return View(appointments);
        }

        // ===================== HELPERS =====================

        private async Task<Patient?> GetCurrentPatientAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        private async Task<Doctor?> GetCurrentDoctorAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        private async Task<BookAppointmentViewModel?> BuildBookViewModelAsync(int doctorId, DateTime selectedDate)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return null;

            return new BookAppointmentViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                SpecializationName = doctor.Specialization.Name,
                SelectedDate = selectedDate,
                AvailableSlots = await _workingHoursService.GetAvailableSlotsAsync(doctorId, selectedDate)
            };
        }

        // Re-fetches doctor name/specialization + fresh available slots after a failed
        // ModelState/validation check, so the form can be redisplayed with correct data.
        private async Task RepopulateSlotsAsync(BookAppointmentViewModel model)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId);

            model.DoctorName = doctor?.User.FullName ?? string.Empty;
            model.SpecializationName = doctor?.Specialization.Name ?? string.Empty;
            model.AvailableSlots = await _workingHoursService.GetAvailableSlotsAsync(model.DoctorId, model.SelectedDate);
        }
    }
}
