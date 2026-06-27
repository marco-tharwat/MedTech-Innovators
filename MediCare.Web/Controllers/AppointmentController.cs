using MediCare.Data.Models;
using MediCare.Services.Interfaces;
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
        private readonly IAppointmentService _appointmentService;
        private readonly IWorkingHoursService _workingHoursService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(
            IAppointmentService appointmentService,
            IWorkingHoursService workingHoursService,
            UserManager<ApplicationUser> userManager)
        {
            _appointmentService = appointmentService;
            _workingHoursService = workingHoursService;
            _userManager = userManager;
        }

        // ── Book ──────────────────────────────────────────────────────────────

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Book(int doctorId, DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            var doctor = await _userManager.Users
                .Select(u => u.DoctorProfile)
                .FirstOrDefaultAsync(d => d != null && d.Id == doctorId);

            var slots = await _workingHoursService.GetAvailableSlotsAsync(doctorId, selectedDate);

            var vm = new BookAppointmentViewModel
            {
                DoctorId = doctorId,
                DoctorName = doctor?.User.FullName ?? "",
                SpecializationName = doctor?.Specialization.Name ?? "",
                SelectedDate = selectedDate,
                AvailableSlots = slots.ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> Book(BookAppointmentViewModel vm)
        {
            if (vm.SelectedSlot is null)
            {
                ModelState.AddModelError("", "Please select a time slot.");
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            var patient = user?.PatientProfile;
            if (patient is null) return Forbid();

            var result = await _appointmentService
                .BookAsync(patient.Id, vm.DoctorId, vm.SelectedSlot.Value, vm.Notes);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage!);
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View(vm);
            }

            TempData["Success"] = "Appointment booked successfully.";
            return RedirectToAction("PatientAppointments");
        }

        // ── Cancel ────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _appointmentService.CancelAsync(id, userId!);

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Appointment cancelled." : result.ErrorMessage;

            return RedirectToAction("PatientAppointments");
        }

        // ── Reschedule ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Reschedule(int id, DateTime? date)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment is null) return NotFound();

            var selectedDate = date ?? DateTime.Today;
            var slots = await _workingHoursService
                .GetAvailableSlotsAsync(appointment.DoctorId, selectedDate);

            var vm = new BookAppointmentViewModel
            {
                AppointmentId = id,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.User.FullName,
                SpecializationName = appointment.Doctor.Specialization.Name,
                SelectedDate = selectedDate,
                AvailableSlots = slots.ToList()
            };

            return View("Book", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Reschedule(int id, BookAppointmentViewModel vm)
        {
            if (vm.SelectedSlot is null)
            {
                ModelState.AddModelError("", "Please select a time slot.");
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View("Book", vm);
            }

            var userId = _userManager.GetUserId(User);
            var result = await _appointmentService
                .RescheduleAsync(id, vm.SelectedSlot.Value, userId!);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage!);
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View("Book", vm);
            }

            TempData["Success"] = "Appointment rescheduled successfully.";
            return RedirectToAction("PatientAppointments");
        }

        // ── Lists ─────────────────────────────────────────────────────────────

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> PatientAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = user?.PatientProfile;
            if (patient is null) return Forbid();

            var appointments = await _appointmentService.GetByPatientIdAsync(patient.Id);
            return View(appointments.ToList());
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorDailyList(DateTime? date)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = user?.DoctorProfile;
            if (doctor is null) return Forbid();

            var selectedDate = date ?? DateTime.Today;
            var appointments = await _appointmentService.GetByDoctorIdAsync(doctor.Id);

            ViewBag.SelectedDate = selectedDate;
            return View(appointments
                .Where(a => a.AppointmentDate.Date == selectedDate.Date)
                .ToList());
        }
    }
}
